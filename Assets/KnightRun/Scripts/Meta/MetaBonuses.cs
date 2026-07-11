namespace KnightRun.Meta
{
    public static class MetaBonuses
    {
        public static int BonusMaxHealth => ShopCatalog.GetLevel(ShopUpgradeId.MaxHealth) * 150;

        public static float MoveSpeedMultiplier =>
            1f + ShopCatalog.GetLevel(ShopUpgradeId.MoveSpeed) * 0.04f;

        public static float AttackSpeedMultiplier =>
            1f + ShopCatalog.GetLevel(ShopUpgradeId.AttackSpeed) * 0.04f;

        public static int RessurectionsPerRun => ShopCatalog.GetLevel(ShopUpgradeId.Ressurection);

        public static float BaseDamageMultiplier =>
            1f + ShopCatalog.GetLevel(ShopUpgradeId.BaseDamage) * 0.1f;

        public static int HealingPerTenKills => ShopCatalog.GetLevel(ShopUpgradeId.Healing) * 10;

        public static int AttackVolleyCount => 1 + ShopCatalog.GetLevel(ShopUpgradeId.MultiStrike);
    }
}
