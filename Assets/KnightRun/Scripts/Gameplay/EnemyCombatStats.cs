using KnightRun.Core;
using UnityEngine;

namespace KnightRun.Gameplay
{
    public static class EnemyCombatStats
    {
        public const int BaseContactDamage = 10;
        public const float ContactDamageCooldown = 0.75f;
        public const float EliteSpawnChance = 0.01f;
        public const int EliteHealthMultiplier = 5;
        public const int EliteDamageMultiplier = 5;

        public static int RollHealthForPhase(RunPhaseSettings settings)
        {
            int min = Mathf.Max(1, settings.enemyHealthMin);
            int max = Mathf.Max(min, settings.enemyHealthMax);
            return Random.Range(min, max + 1);
        }

        public static int GetAverageEnemyHealth(RunPhaseSettings settings)
        {
            int min = Mathf.Max(1, settings.enemyHealthMin);
            int max = Mathf.Max(min, settings.enemyHealthMax);
            return (min + max + 1) / 2;
        }
    }
}
