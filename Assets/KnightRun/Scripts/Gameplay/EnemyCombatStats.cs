using UnityEngine;

namespace KnightRun.Gameplay
{
    public static class EnemyCombatStats
    {
        public const int BaseEnemyHealth = 1;
        public const int HealthGainPer100Meters = 1;
        public const int SwordDamage = 1;
        public const float HealthDistanceStep = 100f;

        public static int GetMaxHealthForDistance(float distance)
        {
            // int bonus = Mathf.FloorToInt(distance / HealthDistanceStep);
            int bonus = 0;
            return BaseEnemyHealth + bonus * HealthGainPer100Meters;
        }
    }
}
