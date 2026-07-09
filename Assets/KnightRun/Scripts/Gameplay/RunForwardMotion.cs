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
                phaseMultiplier = phaseManager.CurrentSettings.speedMultiplier;

            return gameManager.CurrentSpeed * phaseMultiplier;
        }

        public static Vector3 GetDelta()
        {
            return Vector3.forward * GetCurrentSpeed() * Time.deltaTime;
        }
    }
}
