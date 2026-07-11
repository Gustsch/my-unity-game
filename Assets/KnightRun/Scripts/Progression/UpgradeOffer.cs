namespace KnightRun.Progression
{
    public struct UpgradeOffer
    {
        public bool IsCoinReward;
        public int CoinAmount;
        public SkillDefinition Skill;

        public static UpgradeOffer FromSkill(SkillDefinition skill)
        {
            return new UpgradeOffer { Skill = skill };
        }

        public static UpgradeOffer Coin(int amount)
        {
            return new UpgradeOffer { IsCoinReward = true, CoinAmount = amount };
        }
    }
}
