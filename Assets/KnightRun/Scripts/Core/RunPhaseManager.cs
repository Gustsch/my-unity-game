using System;
using KnightRun.Gameplay;
using UnityEngine;

namespace KnightRun.Core
{
    public class RunPhaseManager : MonoBehaviour
    {
        public static RunPhaseManager Instance { get; private set; }

        public const float PhaseRunDuration = 30f;
        public const float CycleSpeedIncrease = 0.05f;

        /// <summary>World/spawn phase — advances when the boss dies so new segments can build ahead.</summary>
        public RunPhase CurrentPhase { get; private set; } = RunPhase.Forest;
        public int CurrentPhaseIndex { get; private set; }
        public int CompletedCycles { get; private set; }
        public int DifficultyPhaseNumber =>
            CompletedCycles * RunPhaseDefaults.All.Length + CurrentPhaseIndex + 1;
        public RunPhaseSettings CurrentSettings { get; private set; } = RunPhaseDefaults.All[0];

        /// <summary>Active movement/hazard phase — advances only when the player enters that biome.</summary>
        public RunPhase GameplayPhase { get; private set; } = RunPhase.Forest;
        public RunPhaseSettings GameplaySettings { get; private set; } = RunPhaseDefaults.All[0];

        public float PhaseRunElapsed { get; private set; }
        public bool IsPhaseRunComplete => PhaseRunElapsed >= PhaseRunDuration;
        public bool IsFinalPhase => CurrentPhaseIndex >= RunPhaseDefaults.All.Length - 1;

        public event Action<RunPhase, RunPhaseSettings> OnPhaseChanged;
        public event Action<RunPhase, RunPhaseSettings> OnGameplayPhaseChanged;

        GameManager gameManager;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            gameManager = GameManager.Instance;
            ApplyPhaseByIndex(0, syncGameplay: true);
        }

        void Update()
        {
            if (gameManager == null || gameManager.State != GameState.Running)
                return;

            PhaseBossController bossController = PhaseBossController.Instance;
            if (bossController == null || !bossController.IsBossFightActive)
                PhaseRunElapsed += Time.deltaTime;

            bossController?.TrySpawnBoss();
        }

        public void AdvanceToNextPhase()
        {
            if (IsFinalPhase)
            {
                CompletedCycles++;
                ApplyPhaseByIndex(0, syncGameplay: false);
                return;
            }

            ApplyPhaseByIndex(CurrentPhaseIndex + 1, syncGameplay: false);
        }

        public void ResetPhases()
        {
            PhaseBossController.Instance?.ResetBossState();
            CompletedCycles = 0;
            ApplyPhaseByIndex(0, syncGameplay: true);
        }

        /// <summary>
        /// Apply movement mechanics for the biome the player just stepped onto.
        /// </summary>
        public void EnterGameplayPhase(RunPhaseSettings settings)
        {
            if (GameplayPhase == settings.phase
                && GameplaySettings.useLaneMovement == settings.useLaneMovement
                && GameplaySettings.useSlideMovement == settings.useSlideMovement
                && Mathf.Approximately(GameplaySettings.speedMultiplier, settings.speedMultiplier)
                && Mathf.Approximately(GameplaySettings.trackHalfWidth, settings.trackHalfWidth))
            {
                return;
            }

            GameplayPhase = settings.phase;
            GameplaySettings = settings;
            OnGameplayPhaseChanged?.Invoke(GameplayPhase, GameplaySettings);
        }

        void ApplyPhaseByIndex(int phaseIndex, bool syncGameplay)
        {
            phaseIndex = Mathf.Clamp(phaseIndex, 0, RunPhaseDefaults.All.Length - 1);
            CurrentPhaseIndex = phaseIndex;
            PhaseRunElapsed = 0f;
            CurrentPhase = RunPhaseDefaults.All[phaseIndex].phase;
            CurrentSettings = GetScaledSettings(phaseIndex);
            OnPhaseChanged?.Invoke(CurrentPhase, CurrentSettings);

            if (syncGameplay)
                EnterGameplayPhase(CurrentSettings);
        }

        RunPhaseSettings GetScaledSettings(int phaseIndex)
        {
            RunPhaseSettings settings = RunPhaseDefaults.All[phaseIndex];
            if (CompletedCycles <= 0)
                return settings;

            RunPhaseSettings finalPhase = RunPhaseDefaults.All[RunPhaseDefaults.All.Length - 1];
            RunPhaseSettings firstPhase = RunPhaseDefaults.All[0];
            settings.enemyHealthMin = AddCycleHealth(
                settings.enemyHealthMin,
                finalPhase.enemyHealthMin);
            settings.enemyHealthMax = AddCycleHealth(
                settings.enemyHealthMax,
                finalPhase.enemyHealthMax);
            float speedRange = finalPhase.speedMultiplier - firstPhase.speedMultiplier;
            settings.speedMultiplier += CompletedCycles * (speedRange + CycleSpeedIncrease);
            return settings;
        }

        int AddCycleHealth(int baseHealth, int healthPerCycle)
        {
            long scaled = baseHealth + (long)healthPerCycle * CompletedCycles;
            return (int)Math.Min(int.MaxValue, scaled);
        }
    }
}
