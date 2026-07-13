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
        public bool IsSlideInvulnerable =>
            IsSliding && GetSlidePhase() >= SlideImmunityStart && GetSlidePhase() <= SlideImmunityEnd;

        CharacterController controller;
        MineCartVisual mineCartVisual;
        KnightSlideVisual slideVisual;
        HeroUpgradeStats upgradeStats;
        GameManager gameManager;
        RunPhaseManager phaseManager;

        float verticalVelocity;
        float targetLaneX;
        float horizontalInput;
        float iceHorizontalVelocity;
        float slideTimer;
        float fallRestartCooldown;

        const float LaneSwitchSpeed = 10000f;
        const float BaseFreeMoveSpeed = 300;
        const float JumpForce = 10f;
        const float Gravity = -32f;
        const float BaseSlideDuration = 0.55f;
        const float SlideImmunityStart = 0.25f;
        const float SlideImmunityEnd = 0.75f;
        const float NormalHeight = 2f;
        const float SlideHeight = 1f;
        const float FallRestartY = -4f;
        const float IceAcceleration = 55f;
        const float IceStartAcceleration = 28f;
        const float IceDeceleration = 90f;
        const float IceStopThreshold = 0.15f;

        float FreeMoveSpeed
        {
            get
            {
                float multiplier = upgradeStats != null ? upgradeStats.MoveSpeedMultiplier : 1f;
                return BaseFreeMoveSpeed * multiplier * MetaBonuses.MoveSpeedMultiplier;
            }
        }

        float SlideDuration
        {
            get
            {
                float multiplier = upgradeStats != null ? upgradeStats.SlideDurationMultiplier : 1f;
                return BaseSlideDuration * multiplier;
            }
        }

        bool UsesLaneMovement =>
            phaseManager != null && phaseManager.CurrentSettings.useLaneMovement;


        bool UsesSlideMovement =>
            phaseManager != null && phaseManager.CurrentSettings.useSlideMovement;

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            mineCartVisual = GetComponent<MineCartVisual>();
            slideVisual = GetComponent<KnightSlideVisual>();
            upgradeStats = GetComponent<HeroUpgradeStats>();
            CurrentLane = PhaseTrackLayout.GetCenterLaneIndex();
            targetLaneX = LanePositions[CurrentLane];
        }

        void Start()
        {
            gameManager = GameManager.Instance;
            phaseManager = RunPhaseManager.Instance;

            if (phaseManager != null)
            {
                phaseManager.OnPhaseChanged += HandlePhaseChanged;
                HandlePhaseChanged(phaseManager.CurrentPhase, phaseManager.CurrentSettings);
            }
        }

        void OnDestroy()
        {
            if (phaseManager != null)
                phaseManager.OnPhaseChanged -= HandlePhaseChanged;
        }

        void Update()
        {
            if (gameManager == null || gameManager.State != GameState.Running)
                return;

            HandleInput();
            UpdateSlide();
            Move();
            CheckFall();
        }

        void HandlePhaseChanged(RunPhase phase, RunPhaseSettings settings)
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
                ? phaseManager.CurrentSettings.speedMultiplier
                : 1f;

            Vector3 position = transform.position;
            bool grounded = controller.isGrounded;

            if (UsesLaneMovement)
                position.x = Mathf.MoveTowards(position.x, targetLaneX, LaneSwitchSpeed * Time.deltaTime);
            else if (UsesSlideMovement)
            {
                if (grounded)
                    position.x = ApplyIceMovement(position.x);
                else
                {
                    iceHorizontalVelocity = 0f;
                    position.x = Mathf.Clamp(
                        position.x + horizontalInput * FreeMoveSpeed * Time.deltaTime,
                        TrackMinX,
                        TrackMaxX);
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

            Vector3 motion = new Vector3(position.x - transform.position.x, verticalVelocity, speed * phaseMultiplier) * Time.deltaTime;
            controller.Move(motion);
        }

        float ApplyIceMovement(float currentX)
        {
            float targetVelocity = horizontalInput * FreeMoveSpeed;
            bool hasInput = Mathf.Abs(horizontalInput) > 0.01f;

            if (hasInput)
            {
                bool startingFromRest = Mathf.Abs(iceHorizontalVelocity) < IceStopThreshold;
                bool reversing =
                    Mathf.Abs(iceHorizontalVelocity) > IceStopThreshold &&
                    Mathf.Sign(horizontalInput) != Mathf.Sign(iceHorizontalVelocity);

                float acceleration = startingFromRest || reversing
                    ? IceStartAcceleration
                    : IceAcceleration;

                iceHorizontalVelocity = Mathf.MoveTowards(
                    iceHorizontalVelocity,
                    targetVelocity,
                    acceleration * Time.deltaTime);
            }
            else
            {
                iceHorizontalVelocity = Mathf.MoveTowards(
                    iceHorizontalVelocity,
                    0f,
                    IceDeceleration * Time.deltaTime);
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
            fallRestartCooldown = 0.5f;
            EndSlide();
            GetComponent<KnightHealth>()?.ResetHealth();
            SetMineCartMode(false);
            controller.enabled = true;
        }
    }
}
