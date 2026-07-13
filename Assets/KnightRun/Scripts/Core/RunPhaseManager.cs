using System;
using KnightRun.Gameplay;
using UnityEngine;

namespace KnightRun.Core
{
    public class RunPhaseManager : MonoBehaviour
    {
        public static RunPhaseManager Instance { get; private set; }

        public const float PhaseRunDuration = 30f;

        public RunPhase CurrentPhase { get; private set; } = RunPhase.Forest;
        public int CurrentPhaseIndex { get; private set; }
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
                return;

            ApplyPhaseByIndex(CurrentPhaseIndex + 1);
        }

        public void ResetPhases()
        {
            PhaseBossController.Instance?.ResetBossState();
            ApplyPhaseByIndex(0);
        }

        void ApplyPhaseByIndex(int phaseIndex)
        {
            phaseIndex = Mathf.Clamp(phaseIndex, 0, RunPhaseDefaults.All.Length - 1);
            CurrentPhaseIndex = phaseIndex;
            PhaseRunElapsed = 0f;
            CurrentPhase = RunPhaseDefaults.All[phaseIndex].phase;
            CurrentSettings = RunPhaseDefaults.All[phaseIndex];
            OnPhaseChanged?.Invoke(CurrentPhase, CurrentSettings);
        }
    }
}
