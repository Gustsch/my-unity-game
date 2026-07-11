using UnityEngine;

namespace KnightRun.Gameplay
{
    public static class PlayerShieldController
    {
        static int remainingAbsorb;
        static float expiresAt;

        public static bool IsActive => remainingAbsorb > 0 && Time.time < expiresAt;

        public static int RemainingAbsorb => IsActive ? remainingAbsorb : 0;

        public static float RemainingSeconds => IsActive ? expiresAt - Time.time : 0f;

        public static void Activate(int absorbAmount, float duration)
        {
            if (absorbAmount <= 0 || duration <= 0f)
                return;

            remainingAbsorb = absorbAmount;
            expiresAt = Time.time + duration;
        }

        public static int Absorb(int damage)
        {
            if (!IsActive || damage <= 0)
                return damage;

            int absorbed = Mathf.Min(remainingAbsorb, damage);
            remainingAbsorb -= absorbed;

            if (remainingAbsorb <= 0)
            {
                remainingAbsorb = 0;
                expiresAt = 0f;
            }

            return damage - absorbed;
        }

        public static void Reset()
        {
            remainingAbsorb = 0;
            expiresAt = 0f;
        }
    }
}
