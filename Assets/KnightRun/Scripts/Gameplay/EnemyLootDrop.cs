using KnightRun.Core;
using KnightRun.Player;
using UnityEngine;

namespace KnightRun.Gameplay
{
    public static class EnemyLootDrop
    {
        public const float FoodDropChance = 0.005f;
        public const float FreezeDropChance = 0.004f;
        public const float CoinBagDropChance = 0.003f;
        public const float ShieldDropChance = 0.002f;
        public const float KillAllDropChance = 0.001f;
        public const float MaxHealthPercentForFood = 0.6f;

        public static bool TrySpawnSpecialDrop(Vector3 position)
        {
            float foodChance = CanDropFood() ? FoodDropChance : 0f;
            float roll = Random.value;
            float threshold = foodChance;

            if (roll < threshold)
            {
                FoodPickup.Spawn(position);
                return true;
            }

            threshold += FreezeDropChance;
            if (roll < threshold)
            {
                FreezePickup.Spawn(position);
                return true;
            }

            threshold += CoinBagDropChance;
            if (roll < threshold)
            {
                CoinBagPickup.SpawnRandom(position);
                return true;
            }

            threshold += ShieldDropChance;
            if (roll < threshold)
            {
                ShieldPickup.Spawn(position);
                return true;
            }

            threshold += KillAllDropChance;
            if (roll < threshold)
            {
                KillAllPickup.Spawn(position);
                return true;
            }

            return false;
        }

        public static void SpawnBossCoinBag(Vector3 position)
        {
            int phaseLevel = 1;
            RunPhaseManager phaseManager = RunPhaseManager.Instance;
            if (phaseManager != null)
                phaseLevel = phaseManager.CurrentPhaseIndex + 1;

            int coins = 50 + 10 * phaseLevel;
            CoinBagPickup.Spawn(position, coins);
        }

        static bool CanDropFood()
        {
            KnightHealth health = Object.FindFirstObjectByType<KnightHealth>();
            if (health == null || health.MaxHealth <= 0)
                return true;

            return (float)health.CurrentHealth / health.MaxHealth <= MaxHealthPercentForFood;
        }
    }
}
