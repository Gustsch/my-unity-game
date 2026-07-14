using UnityEngine;

namespace KnightRun.Meta
{
    public static class ShopCatalog
    {
        public const int MaxLevel = 5;

        public static int GetLevel(ShopUpgradeId id)
        {
            return PlayerPrefs.GetInt(GetLevelKey(id), 0);
        }

        public static bool CanUpgrade(ShopUpgradeId id)
        {
            return GetLevel(id) < MaxLevel;
        }

        public static int GetCost(ShopUpgradeId id)
        {
            int level = GetLevel(id);
            return id switch
            // {
            //     ShopUpgradeId.MaxHealth => 0,
            //     ShopUpgradeId.MoveSpeed => 0,
            //     ShopUpgradeId.AttackSpeed => 0,
            //     ShopUpgradeId.Ressurection => 0,
            //     ShopUpgradeId.BaseDamage => 0,
            //     ShopUpgradeId.Healing => 0,
            //     ShopUpgradeId.MultiStrike => 0,
            //     _ => 50
            // };
            {
                ShopUpgradeId.MaxHealth => 100 + level * 127,
                ShopUpgradeId.MoveSpeed => 135 + level * 120,
                ShopUpgradeId.AttackSpeed => 138 + level * 122,
                ShopUpgradeId.Ressurection => 312 + level * 553,
                ShopUpgradeId.BaseDamage => 120 + level * 112,
                ShopUpgradeId.Healing => 130 + level * 126,
                ShopUpgradeId.MultiStrike => 255 + level * 137,
                _ => 50
            };
        }

        public static string GetName(ShopUpgradeId id)
        {
            return Localization.GetShopName(id);
        }

        public static string GetDescription(ShopUpgradeId id, int nextLevel)
        {
            return Localization.GetShopDescription(id, nextLevel);
        }

        public static void IncrementLevel(ShopUpgradeId id)
        {
            int next = Mathf.Min(MaxLevel, GetLevel(id) + 1);
            PlayerPrefs.SetInt(GetLevelKey(id), next);
        }

        static string GetLevelKey(ShopUpgradeId id) => $"KnightRun_Shop_{id}";

        public static void ResetAll()
        {
            foreach (ShopUpgradeId id in System.Enum.GetValues(typeof(ShopUpgradeId)))
                PlayerPrefs.DeleteKey(GetLevelKey(id));
        }
    }
}
