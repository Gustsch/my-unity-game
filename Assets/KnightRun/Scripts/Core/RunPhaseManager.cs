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

        public RunPhase CurrentPhase { get; private set; } = RunPhase.Forest;
        public int CurrentPhaseIndex { get; private set; }
        public int CompletedCycles { get; private set; }
        public int DifficultyPhaseNumber =>
            CompletedCycles * RunPhaseDefaults.All.Length + CurrentPhaseIndex + 1;
        public RunPhaseSettings CurrentSettings { get; private set; } = RunPhaseDefaults.All[0];
        public float PhaseRunElapsed { get; private set; }
        public bool IsPhaseRunComplete => PhaseRunElapsed >= PhaseRunDuration;
        public bool IsFinalPhase => CurrentPhaseIndex >= RunPhaseDefaults.All.Length - 1;

        public event Action<RunPhase, RunPhaseSettings> OnPhaseChanged;

        GameManager gameManager;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            gameManager = GameManager.Instance;
            ApplyPhaseByIndex(0);
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
                ApplyPhaseByIndex(0);
                return;
            }

            ApplyPhaseByIndex(CurrentPhaseIndex + 1);
        }

        public void ResetPhases()
        {
            PhaseBossController.Instance?.ResetBossState();
            CompletedCycles = 0;
            ApplyPhaseByIndex(0);
        }

        void ApplyPhaseByIndex(int phaseIndex)
        {
            phaseIndex = Mathf.Clamp(phaseIndex, 0, RunPhaseDefaults.All.Length - 1);
            CurrentPhaseIndex = phaseIndex;
            PhaseRunElapsed = 0f;
            CurrentPhase = RunPhaseDefaults.All[phaseIndex].phase;
            CurrentSettings = GetScaledSettings(phaseIndex);
            OnPhaseChanged?.Invoke(CurrentPhase, CurrentSettings);
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
