using UnityEngine;

namespace KnightRun.Gameplay
{
    public static class EnemyFreezeController
    {
        static float freezeUntil;

        public static bool IsActive => Time.time < freezeUntil;

        public static float RemainingSeconds => IsActive ? freezeUntil - Time.time : 0f;

        public static void FreezeAll(float duration)
        {
            if (duration <= 0f)
                return;

            freezeUntil = Mathf.Max(freezeUntil, Time.time + duration);
        }

        public static void Reset()
        {
            freezeUntil = 0f;
        }
    }
}
