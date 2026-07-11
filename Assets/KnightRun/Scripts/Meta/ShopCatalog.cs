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
            {
                ShopUpgradeId.MaxHealth => 100 + level * 127,
                ShopUpgradeId.MoveSpeed => 135 + level * 120,
                ShopUpgradeId.AttackSpeed => 138 + level * 122,
                ShopUpgradeId.Ressurection => 300 + level * 153,
                ShopUpgradeId.BaseDamage => 120 + level * 112,
                ShopUpgradeId.Healing => 130 + level * 126,
                ShopUpgradeId.MultiStrike => 255 + level * 130,
                _ => 50
            };
        }

        public static string GetName(ShopUpgradeId id)
        {
            return id switch
            {
                ShopUpgradeId.MaxHealth => "Vida Extra",
                ShopUpgradeId.MoveSpeed => "Passo Veloz",
                ShopUpgradeId.AttackSpeed => "Golpes Rapidos",
                ShopUpgradeId.Ressurection => "Revivida",
                ShopUpgradeId.BaseDamage => "Dano Base",
                ShopUpgradeId.Healing => "Cura",
                ShopUpgradeId.MultiStrike => "Golpes Multiplos",
                _ => id.ToString()
            };
        }

        public static string GetDescription(ShopUpgradeId id, int nextLevel)
        {
            return id switch
            {
                ShopUpgradeId.MaxHealth => $"+150 HP maximo (Nv {nextLevel})",
                ShopUpgradeId.MoveSpeed => $"+4% velocidade de movimento (Nv {nextLevel})",
                ShopUpgradeId.AttackSpeed => $"+4% velocidade de ataque (Nv {nextLevel})",
                ShopUpgradeId.Ressurection => $"+1 Ressurreicao por Level (Nv {nextLevel})",
                ShopUpgradeId.BaseDamage => $"+10% de Dano Base por Level (Nv {nextLevel})",
                ShopUpgradeId.Healing => $"+10 de Cura a cada 10 inimigos mortos (Nv {nextLevel})",
                ShopUpgradeId.MultiStrike => $"+1 ataque simultaneo (Nv {nextLevel})",
                _ => string.Empty
            };
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
