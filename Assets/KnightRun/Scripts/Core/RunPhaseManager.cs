using System;
using UnityEngine;

namespace KnightRun.Core
{
    public class RunPhaseManager : MonoBehaviour
    {
        public static RunPhaseManager Instance { get; private set; }

        public RunPhase CurrentPhase { get; private set; } = RunPhase.Forest;
        public RunPhaseSettings CurrentSettings { get; private set; } = RunPhaseDefaults.All[0];

        public event Action<RunPhase, RunPhaseSettings> OnPhaseChanged;

        GameManager gameManager;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            gameManager = GameManager.Instance;
            if (gameManager != null)
                gameManager.OnDistanceChanged += HandleDistanceChanged;

            ApplyPhase(CurrentPhase);
        }

        void OnDestroy()
        {
            if (gameManager != null)
                gameManager.OnDistanceChanged -= HandleDistanceChanged;
        }

        void HandleDistanceChanged(float distance)
        {
            RunPhase next = GetPhaseForDistance(distance);
            if (next == CurrentPhase)
                return;

            ApplyPhase(next);
        }

        RunPhase GetPhaseForDistance(float distance)
        {
            for (int i = RunPhaseDefaults.All.Length - 1; i >= 0; i--)
            {
                if (distance >= RunPhaseDefaults.All[i].distanceStart)
                    return RunPhaseDefaults.All[i].phase;
            }

            return RunPhase.Forest;
        }

        public void ResetPhases()
        {
            ApplyPhase(RunPhase.Forest);
        }

        void ApplyPhase(RunPhase phase)
        {
            CurrentPhase = phase;
            CurrentSettings = Array.Find(RunPhaseDefaults.All, s => s.phase == phase);
            OnPhaseChanged?.Invoke(CurrentPhase, CurrentSettings);
        }
    }
}
