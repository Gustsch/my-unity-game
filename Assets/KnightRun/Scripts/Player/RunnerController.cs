using KnightRun;
using KnightRun.Meta;
using KnightRun.Core;
using KnightRun.Progression;
using UnityEngine;

namespace KnightRun.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class RunnerController : MonoBehaviour
    {
        public static float[] LanePositions => PhaseTrackLayout.GetLanePositions();
        public static readonly Vector3 StartPosition = new Vector3(0f, 0f, 2f);
        public static float TrackMinX => PhaseTrackLayout.GetPlayableMinX();
        public static float TrackMaxX => PhaseTrackLayout.GetPlayableMaxX();

        public int CurrentLane { get; private set; } = PhaseTrackLayout.GetCenterLaneIndex();
        public bool IsSliding { get; private set; }
        public bool IsGrounded { get; private set; } = true;
        public bool IsRooted => rootTimer > 0f;
        public bool IsChilled => chillTimer > 0f;
        public bool IsStumbling => stumbleTimer > 0f;
        public bool IsSlideInvulnerable =>
            IsSliding && GetSlidePhase() >= SlideImmunityStart && GetSlidePhase() <= SlideImmunityEnd;

        CharacterController controller;
        MineCartVisual mineCartVisual;
        KnightSlideVisual slideVisual;
        PlayerAnimationDriver animationDriver;
        PlayerCharacterVisual characterVisual;
        HeroUpgradeStats upgradeStats;
        GameManager gameManager;
        RunPhaseManager phaseManager;

        float verticalVelocity;
        float targetLaneX;
        float horizontalInput;
        float iceHorizontalVelocity;
        float slideTimer;
        float fallRestartCooldown;
        float rootTimer;
        float chillTimer;
        float chillMoveMultiplier = 1f;
        float stumbleTimer;
        float stumbleElapsed;
        float stumbleOffsetX;
        float previousX;

        const float LaneSwitchSpeed = 15f;
        const float BaseFreeMoveSpeed = 5f;
        const float JumpForce = 12f;
        const float Gravity = -32f;
        const float BaseSlideDuration = 0.55f;
        const float SlideImmunityStart = 0.25f;
        const float SlideImmunityEnd = 0.75f;
        const float NormalHeight = 2f;
        const float SlideHeight = 1f;
        const float FallRestartY = -4f;
        const float IceMaxHorizontalSpeed = 6.5f;
        const float IceAcceleration = 5.5f;
        const float IceStartAcceleration = 2.8f;
        const float IceDeceleration = 2.4f;
        const float IceStopThreshold = 0.12f;
        const float StumbleDuration = 1f;
        const float StumbleSpeedMultiplier = 0.65f;
        const float StumbleSwayAmplitude = 0.12f;
        const float StumbleSwayFrequency = 24f;
        const float StrafeFacingSpeedReference = 5f;

        float FreeMoveSpeed
        {
            get
            {
                float chill = IsChilled ? chillMoveMultiplier : 1f;
                return BaseFreeMoveSpeed * MetaBonuses.MoveSpeedMultiplier * chill;
            }
        }

        float ChillAccelerationMultiplier => IsChilled ? chillMoveMultiplier : 1f;

        float SlideDuration
        {
            get
            {
                return BaseSlideDuration;
            }
        }

        bool UsesLaneMovement =>
            phaseManager != null && phaseManager.GameplaySettings.useLaneMovement;


        bool UsesSlideMovement =>
            phaseManager != null && phaseManager.GameplaySettings.useSlideMovement;

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            mineCartVisual = GetComponent<MineCartVisual>();
            slideVisual = GetComponent<KnightSlideVisual>();
            animationDriver = GetComponent<PlayerAnimationDriver>();
            characterVisual = GetComponent<PlayerCharacterVisual>();
            upgradeStats = GetComponent<HeroUpgradeStats>();
            CurrentLane = PhaseTrackLayout.GetCenterLaneIndex();
            targetLaneX = LanePositions[CurrentLane];
            previousX = transform.position.x;
        }

        void Start()
        {
            gameManager = GameManager.Instance;
            phaseManager = RunPhaseManager.Instance;

            if (phaseManager != null)
            {
                phaseManager.OnGameplayPhaseChanged += HandleGameplayPhaseChanged;
                HandleGameplayPhaseChanged(phaseManager.GameplayPhase, phaseManager.GameplaySettings);
            }
        }

        void OnDestroy()
        {
            if (phaseManager != null)
                phaseManager.OnGameplayPhaseChanged -= HandleGameplayPhaseChanged;
        }

        void Update()
        {
            if (gameManager == null || gameManager.State != GameState.Running)
                return;

            if (rootTimer > 0f)
                rootTimer -= Time.deltaTime;

            if (chillTimer > 0f)
            {
                chillTimer -= Time.deltaTime;
                if (chillTimer <= 0f)
                    chillMoveMultiplier = 1f;
            }

            if (stumbleTimer > 0f)
            {
                stumbleTimer -= Time.deltaTime;
                stumbleElapsed += Time.deltaTime;
            }

            HandleInput();
            UpdateSlide();
            Move();
            UpdateStrafeFacing();
            UpdateAnimationDriver();
            CheckFall();
        }

        void UpdateStrafeFacing()
        {
            if (characterVisual == null)
                characterVisual = GetComponent<PlayerCharacterVisual>();
            if (characterVisual == null)
                return;

            bool allowTurn = !IsSliding && !IsRooted && !UsesLaneMovement;
            float strafeAmount = 0f;

            if (allowTurn)
            {
                float lateralSpeed = (transform.position.x - previousX) / Mathf.Max(Time.deltaTime, 0.0001f);
                // Prefer stick/keyboard intent when present; fall back to actual velocity (lanes/ice).
                if (Mathf.Abs(horizontalInput) > 0.01f)
                    strafeAmount = horizontalInput;
                else if (UsesSlideMovement && Mathf.Abs(iceHorizontalVelocity) > 0.15f)
                    strafeAmount = Mathf.Clamp(iceHorizontalVelocity / IceMaxHorizontalSpeed, -1f, 1f);
                else
                    strafeAmount = Mathf.Clamp(lateralSpeed / StrafeFacingSpeedReference, -1f, 1f);
            }

            characterVisual.SetStrafeFacing(strafeAmount, allowTurn);
            previousX = transform.position.x;
        }

        void UpdateAnimationDriver()
        {
            if (animationDriver == null)
                return;

            float forwardSpeed = gameManager != null ? gameManager.CurrentSpeed : 0f;
            if (IsStumbling)
                forwardSpeed *= StumbleSpeedMultiplier;

            animationDriver.SetSpeed(forwardSpeed);
            animationDriver.SetGrounded(IsGrounded);
            animationDriver.SetSliding(IsSliding);
            animationDriver.SetStumbling(IsStumbling);
            animationDriver.SetInMineCart(UsesLaneMovement);
            animationDriver.SetVerticalVelocity(verticalVelocity);
        }

        public void ApplyRoot(float duration)
        {
            if (duration <= 0f)
                return;

            rootTimer = Mathf.Max(rootTimer, duration);
            horizontalInput = 0f;
            iceHorizontalVelocity = 0f;

            if (IsSliding)
                EndSlide();
        }

        public void ClearRoot()
        {
            rootTimer = 0f;
        }

        public void ApplyChill(float duration, float moveMultiplier)
        {
            if (duration <= 0f)
                return;

            chillTimer = Mathf.Max(chillTimer, duration);
            chillMoveMultiplier = Mathf.Clamp(moveMultiplier, 0.05f, 1f);
        }

        public void ClearChill()
        {
            chillTimer = 0f;
            chillMoveMultiplier = 1f;
        }

        public void ApplyPhaseObstacleHit()
        {
            stumbleTimer = StumbleDuration;
            stumbleElapsed = 0f;
        }

        void ClearStumble()
        {
            stumbleTimer = 0f;
            stumbleElapsed = 0f;
            stumbleOffsetX = 0f;
        }

        public void ClearStatusEffects()
        {
            ClearRoot();
            ClearChill();
            ClearStumble();
        }

        void HandleGameplayPhaseChanged(RunPhase phase, RunPhaseSettings settings)
        {
            SetMineCartMode(settings.useLaneMovement);

            if (settings.useLaneMovement)
                SnapToNearestLane();

            if (!settings.useSlideMovement)
                iceHorizontalVelocity = 0f;
        }

        void CheckFall()
        {
            if (fallRestartCooldown > 0f)
            {
                fallRestartCooldown -= Time.deltaTime;
                return;
            }

            if (transform.position.y < FallRestartY)
            {
                fallRestartCooldown = 1f;
                GameBootstrap.RestartGame();
            }
        }

        void HandleInput()
        {
            if (IsRooted)
            {
                horizontalInput = 0f;
                return;
            }

            if (UsesLaneMovement)
            {
                horizontalInput = 0f;

                if (KnightInput.GetKeyDown(KeyCode.A) || KnightInput.GetKeyDown(KeyCode.LeftArrow))
                    ChangeLane(-1);
                if (KnightInput.GetKeyDown(KeyCode.D) || KnightInput.GetKeyDown(KeyCode.RightArrow))
                    ChangeLane(1);
            }
            else
            {
                horizontalInput = 0f;
                if (KnightInput.GetKey(KeyCode.A) || KnightInput.GetKey(KeyCode.LeftArrow))
                    horizontalInput -= 1f;
                if (KnightInput.GetKey(KeyCode.D) || KnightInput.GetKey(KeyCode.RightArrow))
                    horizontalInput += 1f;
            }

            if ((KnightInput.GetKeyDown(KeyCode.W) || KnightInput.GetKeyDown(KeyCode.Space) || KnightInput.GetKeyDown(KeyCode.UpArrow)) && IsGrounded && !IsSliding)
                Jump();

            if ((KnightInput.GetKeyDown(KeyCode.S) || KnightInput.GetKeyDown(KeyCode.DownArrow)) && IsGrounded && !IsSliding)
                StartSlide();
        }

        void ChangeLane(int direction)
        {
            float[] lanes = LanePositions;
            int next = Mathf.Clamp(CurrentLane + direction, 0, lanes.Length - 1);
            if (next == CurrentLane)
                return;

            CurrentLane = next;
            targetLaneX = lanes[CurrentLane];
        }

        public void SnapToNearestLane()
        {
            float[] lanes = LanePositions;
            CurrentLane = GetNearestLaneIndex(transform.position.x);
            targetLaneX = lanes[CurrentLane];
        }

        public static int GetNearestLaneIndex(float x)
        {
            float[] lanes = LanePositions;
            int nearest = 0;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < lanes.Length; i++)
            {
                float distance = Mathf.Abs(lanes[i] - x);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    nearest = i;
                }
            }

            return nearest;
        }

        void Jump()
        {
            verticalVelocity = JumpForce;
            IsGrounded = false;
        }

        void StartSlide()
        {
            IsSliding = true;
            slideTimer = SlideDuration;
            controller.height = SlideHeight;
            controller.center = new Vector3(0f, SlideHeight * 0.5f, 0f);
            UpdateSlideVisual();
        }

        void UpdateSlide()
        {
            if (!IsSliding)
                return;

            slideTimer -= Time.deltaTime;
            UpdateSlideVisual();

            if (slideTimer <= 0f)
                EndSlide();
        }

        float GetSlidePhase()
        {
            if (!IsSliding)
                return 0f;

            return 1f - (slideTimer / SlideDuration);
        }

        float GetSlideAnimationAmount()
        {
            float phase = GetSlidePhase();

            if (phase < 0.2f)
                return phase / 0.2f;
            if (phase > 0.8f)
                return (1f - phase) / 0.2f;

            return 1f;
        }

        void UpdateSlideVisual()
        {
            if (slideVisual != null)
                slideVisual.SetSlideAmount(GetSlideAnimationAmount());
        }

        void EndSlide()
        {
            IsSliding = false;
            controller.height = NormalHeight;
            controller.center = new Vector3(0f, NormalHeight * 0.5f, 0f);
            UpdateSlideVisual();
        }

        void Move()
        {
            float speed = gameManager.CurrentSpeed;
            float phaseMultiplier = phaseManager != null
                ? phaseManager.GameplaySettings.speedMultiplier
                : 1f;

            Vector3 position = transform.position;
            position.x -= stumbleOffsetX;
            bool grounded = controller.isGrounded;

            if (IsRooted)
            {
                horizontalInput = 0f;
                iceHorizontalVelocity = 0f;
            }
            else if (UsesLaneMovement)
                position.x = Mathf.MoveTowards(position.x, targetLaneX, LaneSwitchSpeed * Time.deltaTime);
            else if (UsesSlideMovement)
            {
                if (grounded)
                {
                    position.x = ApplyIceMovement(position.x);
                }
                else
                {
                    // Keep ice momentum while airborne; air steering stays soft.
                    float airControl = IceStartAcceleration * 0.55f * ChillAccelerationMultiplier;
                    float airTarget = horizontalInput
                        * IceMaxHorizontalSpeed
                        * MetaBonuses.MoveSpeedMultiplier
                        * (IsChilled ? chillMoveMultiplier : 1f);
                    iceHorizontalVelocity = Mathf.MoveTowards(
                        iceHorizontalVelocity,
                        airTarget,
                        airControl * Time.deltaTime);

                    float nextX = position.x + iceHorizontalVelocity * Time.deltaTime;
                    if (nextX <= TrackMinX || nextX >= TrackMaxX)
                    {
                        nextX = Mathf.Clamp(nextX, TrackMinX, TrackMaxX);
                        iceHorizontalVelocity = 0f;
                    }

                    position.x = nextX;
                }
            }
            else
            {
                iceHorizontalVelocity = 0f;
                position.x = Mathf.Clamp(position.x + horizontalInput * FreeMoveSpeed * Time.deltaTime, TrackMinX, TrackMaxX);
            }

            if (grounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
                IsGrounded = true;
            }
            else if (!grounded)
            {
                IsGrounded = false;
            }

            verticalVelocity += Gravity * Time.deltaTime;
            float nextStumbleOffset = GetStumbleOffset();
            float targetX = Mathf.Clamp(position.x + nextStumbleOffset, TrackMinX, TrackMaxX);
            float deltaX = targetX - transform.position.x;
            float deltaY = verticalVelocity * Time.deltaTime;
            float stumbleSpeed = IsStumbling ? StumbleSpeedMultiplier : 1f;
            float deltaZ = speed * phaseMultiplier * stumbleSpeed * Time.deltaTime;
            controller.Move(new Vector3(deltaX, deltaY, deltaZ));
            stumbleOffsetX = targetX - position.x;
        }

        float GetStumbleOffset()
        {
            if (!IsStumbling)
                return 0f;

            float fade = Mathf.Clamp01(stumbleTimer / StumbleDuration);
            return Mathf.Sin(stumbleElapsed * StumbleSwayFrequency) * StumbleSwayAmplitude * fade;
        }

        float ApplyIceMovement(float currentX)
        {
            float chilledMultiplier = IsChilled ? chillMoveMultiplier : 1f;
            float maxSpeed = IceMaxHorizontalSpeed * MetaBonuses.MoveSpeedMultiplier * chilledMultiplier;
            float targetVelocity = horizontalInput * maxSpeed;
            bool hasInput = Mathf.Abs(horizontalInput) > 0.01f;

            if (hasInput)
            {
                float speed = Mathf.Abs(iceHorizontalVelocity);
                bool startingFromRest = speed < IceStopThreshold;
                bool reversing =
                    speed > IceStopThreshold &&
                    Mathf.Sign(horizontalInput) != Mathf.Sign(iceHorizontalVelocity);

                // Ice: slow to get going / reverse; only a bit easier once already sliding that way.
                float acceleration = (startingFromRest || reversing
                    ? IceStartAcceleration
                    : IceAcceleration) * ChillAccelerationMultiplier;

                iceHorizontalVelocity = Mathf.MoveTowards(
                    iceHorizontalVelocity,
                    targetVelocity,
                    acceleration * Time.deltaTime);
            }
            else
            {
                // Keep coasting a good while after releasing left/right.
                iceHorizontalVelocity = Mathf.MoveTowards(
                    iceHorizontalVelocity,
                    0f,
                    IceDeceleration * ChillAccelerationMultiplier * Time.deltaTime);

                if (Mathf.Abs(iceHorizontalVelocity) < IceStopThreshold)
                    iceHorizontalVelocity = 0f;
            }

            float nextX = currentX + iceHorizontalVelocity * Time.deltaTime;

            if (nextX <= TrackMinX)
            {
                nextX = TrackMinX;
                iceHorizontalVelocity = 0f;
            }
            else if (nextX >= TrackMaxX)
            {
                nextX = TrackMaxX;
                iceHorizontalVelocity = 0f;
            }

            return nextX;
        }

        public void SetMineCartMode(bool enabled)
        {
            if (mineCartVisual != null)
                mineCartVisual.SetActive(enabled);

            animationDriver ??= GetComponent<PlayerAnimationDriver>();
            animationDriver?.SetInMineCart(enabled);
        }

        public void ResetToStart(Vector3 startPosition)
        {
            controller.enabled = false;
            transform.position = startPosition;
            CurrentLane = PhaseTrackLayout.GetCenterLaneIndex();
            targetLaneX = LanePositions[CurrentLane];
            horizontalInput = 0f;
            iceHorizontalVelocity = 0f;
            verticalVelocity = 0f;
            IsGrounded = true;
            rootTimer = 0f;
            chillTimer = 0f;
            chillMoveMultiplier = 1f;
            ClearStumble();
            fallRestartCooldown = 0.5f;
            EndSlide();
            previousX = startPosition.x;
            characterVisual ??= GetComponent<PlayerCharacterVisual>();
            characterVisual?.ResetStrafeFacing();
            GetComponent<KnightHealth>()?.ResetHealth();
            SetMineCartMode(false);
            controller.enabled = true;
        }
    }
}
