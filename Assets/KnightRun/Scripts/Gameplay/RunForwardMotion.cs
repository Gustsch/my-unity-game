using KnightRun.Core;
using UnityEngine;

namespace KnightRun.Gameplay
{
    public static class RunForwardMotion
    {
        public static float GetCurrentSpeed()
        {
            GameManager gameManager = GameManager.Instance;
            if (gameManager == null || gameManager.State != GameState.Running)
                return 0f;

            float phaseMultiplier = 1f;
            RunPhaseManager phaseManager = RunPhaseManager.Instance;
            if (phaseManager != null)
                phaseMultiplier = phaseManager.GameplaySettings.speedMultiplier;

            return gameManager.CurrentSpeed * phaseMultiplier;
        }

        public static float GetPhaseSpeedMultiplier()
        {
            RunPhaseManager phaseManager = RunPhaseManager.Instance;
            if (phaseManager == null)
                return 1f;

            return Mathf.Max(1f, phaseManager.GameplaySettings.speedMultiplier);
        }

        public static float GetScaledProjectileRange(float baseRange)
        {
            return baseRange * GetPhaseSpeedMultiplier();
        }

        public static float GetEnemyHorizontalMoveScale()
        {
            float phaseMultiplier = 1f;
            RunPhaseManager phaseManager = RunPhaseManager.Instance;
            if (phaseManager != null)
                phaseMultiplier = phaseManager.GameplaySettings.speedMultiplier;

            return phaseMultiplier > 0f ? 1f / phaseMultiplier : 1f;
        }

        public static Vector3 GetDelta()
        {
            return Vector3.forward * GetCurrentSpeed() * Time.deltaTime;
        }
    }
}
