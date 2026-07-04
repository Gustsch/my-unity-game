using KnightRun;
using KnightRun.Core;
using UnityEngine;

namespace KnightRun.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class RunnerController : MonoBehaviour
    {
        public static readonly float[] LanePositions = { -2f, 0f, 2f };
        public static readonly Vector3 StartPosition = new Vector3(0f, 0f, 2f);
        public const float TrackMinX = -3.2f;
        public const float TrackMaxX = 3.2f;

        public int CurrentLane { get; private set; } = 1;
        public bool IsSliding { get; private set; }
        public bool IsGrounded { get; private set; } = true;
        public bool IsSlideInvulnerable =>
            IsSliding && GetSlidePhase() >= SlideImmunityStart && GetSlidePhase() <= SlideImmunityEnd;

        CharacterController controller;
        MineCartVisual mineCartVisual;
        KnightSlideVisual slideVisual;
        GameManager gameManager;
        RunPhaseManager phaseManager;

        float verticalVelocity;
        float targetLaneX;
        float horizontalInput;
        float slideTimer;
        float fallRestartCooldown;

        const float LaneSwitchSpeed = 80f;
        const float FreeMoveSpeed = 80f;
        const float JumpForce = 10f;
        const float Gravity = -32f;
        const float SlideDuration = 0.55f;
        const float SlideImmunityStart = 0.25f;
        const float SlideImmunityEnd = 0.75f;
        const float NormalHeight = 2f;
        const float SlideHeight = 1f;
        const float FallRestartY = -4f;

        bool UsesLaneMovement =>
            phaseManager != null && phaseManager.CurrentSettings.useLaneMovement;

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            mineCartVisual = GetComponent<MineCartVisual>();
            slideVisual = GetComponent<KnightSlideVisual>();
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

                if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                    ChangeLane(-1);
                if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                    ChangeLane(1);
            }
            else
            {
                horizontalInput = 0f;
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                    horizontalInput -= 1f;
                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                    horizontalInput += 1f;
            }

            if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow)) && IsGrounded && !IsSliding)
                Jump();

            if ((Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) && IsGrounded && !IsSliding)
                StartSlide();
        }

        void ChangeLane(int direction)
        {
            int next = Mathf.Clamp(CurrentLane + direction, 0, LanePositions.Length - 1);
            if (next == CurrentLane)
                return;

            CurrentLane = next;
            targetLaneX = LanePositions[CurrentLane];
        }

        public void SnapToNearestLane()
        {
            CurrentLane = GetNearestLaneIndex(transform.position.x);
            targetLaneX = LanePositions[CurrentLane];
        }

        public static int GetNearestLaneIndex(float x)
        {
            int nearest = 0;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < LanePositions.Length; i++)
            {
                float distance = Mathf.Abs(LanePositions[i] - x);
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

            if (UsesLaneMovement)
                position.x = Mathf.MoveTowards(position.x, targetLaneX, LaneSwitchSpeed * Time.deltaTime);
            else
                position.x = Mathf.Clamp(position.x + horizontalInput * FreeMoveSpeed * Time.deltaTime, TrackMinX, TrackMaxX);

            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
                IsGrounded = true;
            }
            else if (!controller.isGrounded)
            {
                IsGrounded = false;
            }

            verticalVelocity += Gravity * Time.deltaTime;

            Vector3 motion = new Vector3(position.x - transform.position.x, verticalVelocity, speed * phaseMultiplier) * Time.deltaTime;
            controller.Move(motion);
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
            CurrentLane = 1;
            targetLaneX = LanePositions[CurrentLane];
            horizontalInput = 0f;
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
