using System.Collections.Generic;
using UnityEngine;

namespace KnightRun.Progression
{
    public static class SkillPool
    {
        public const int MaxSkillLevel = 10;
        public const int MaxWeapons = 4;
        public const int MaxSkills = 4;
        public const int MaxLevelCoinReward = 10;
        public const int StartingSwordLevel = 1;
        public const int SwordDamagePerLevel = 10;
        public const int BowDamagePerLevel = 10;
        public const int BombDamagePerLevel = 10;

        public static int GetSwordDamage(int level)
        {
            return Mathf.Max(10, level * SwordDamagePerLevel);
        }

        public const float IronSkinReductionPerLevel = 0.1f;
        public const float QuickSlashSpeedPerLevel = 0.1f;
        public const float PiercingChancePerLevel = 0.2f;
        public const int PiercingGuaranteedEveryLevels = 5;
        public const float CriticalStrikeChancePerLevel = 0.05f;
        public const float CriticalStrikeDamageMultiplier = 2f;
        public const float ExperienceBoostPerLevel = 0.15f;
        public const int BaseXpPerLevel = 10;
        public const int VigorHealthPerLevel = 100;
        public const float BowAttackInterval = 1.2f;
        public const float BowMinAttackInterval = 0.45f;
        public const float ShurikenAttackInterval = 1.5f;
        public const float ShurikenMinAttackInterval = 0.1f;
        public const float ShurikenBaseDamage = 5f;
        public const float ShurikenDamagePerLevel = 5f;
        public const float MagicBookOrbitRadius = 1.7f;
        public const float MagicBookBaseSpinSpeed = 160f;
        public const float MagicBookBaseHitRadius = 0.5f;
        public const float MagicBookBaseScale = 1f;
        public const float MagicBookOrbitHeight = 1.1f;
        public const float MagicBookContactDamage = 5f;
        public const float MagicBookContactDamagePerLevel = 2f;
        public const float BombAttackInterval = 2f;
        public const float BombBaseDamage = 20f;
        public const float BombMinAttackInterval = 0.8f;
        public const float BombFixedThrowDistance = 25f;
        public const float BombBaseExplosionRadius = 3f;
        public const float BoomerangBaseDamage = 20f;
        public const float BoomerangDamagePerLevel = 5f;
        public const float BoomerangBaseSpeed = 12f;
        public const float ThrowingAxeAttackInterval = 2f;
        public const float ThrowingAxeMinAttackInterval = 1.6f;
        public const float ThrowingAxeBaseDamage = 30f;
        public const float ThrowingAxeDamagePerLevel = 10f;
        public const float ThrowingAxeSpeed = 24f;

        public static readonly SkillDefinition[] All =
        {
            new SkillDefinition { Id = HeroSkillId.Sword, DisplayName = "Espada", Category = UpgradeCategory.Weapon },
            new SkillDefinition { Id = HeroSkillId.Bow, DisplayName = "Arco e Flecha", Category = UpgradeCategory.Weapon },
            new SkillDefinition { Id = HeroSkillId.Shuriken, DisplayName = "Shurikens", Category = UpgradeCategory.Weapon },
            new SkillDefinition { Id = HeroSkillId.MagicBook, DisplayName = "Livro de Magia", Category = UpgradeCategory.Skill },
            new SkillDefinition { Id = HeroSkillId.Bomb, DisplayName = "Bombinha", Category = UpgradeCategory.Weapon },
            new SkillDefinition { Id = HeroSkillId.Boomerang, DisplayName = "Bumerangue", Category = UpgradeCategory.Weapon },
            new SkillDefinition { Id = HeroSkillId.ThrowingAxe, DisplayName = "Machado de Arremesso", Category = UpgradeCategory.Weapon },
            new SkillDefinition { Id = HeroSkillId.QuickSlash, DisplayName = "Golpe Rapido", Category = UpgradeCategory.Skill },
            new SkillDefinition { Id = HeroSkillId.Piercing, DisplayName = "Perfuracao", Category = UpgradeCategory.Skill },
            new SkillDefinition { Id = HeroSkillId.Vigor, DisplayName = "Vigor", Category = UpgradeCategory.Skill },
            new SkillDefinition { Id = HeroSkillId.CriticalStrike, DisplayName = "Golpe Critico", Category = UpgradeCategory.Skill },
            new SkillDefinition { Id = HeroSkillId.ExperienceBoost, DisplayName = "Sabedoria", Category = UpgradeCategory.Skill },
            new SkillDefinition { Id = HeroSkillId.IronSkin, DisplayName = "Pele de Ferro", Category = UpgradeCategory.Skill }
        };

        public static SkillDefinition Get(HeroSkillId id)
        {
            foreach (SkillDefinition skill in All)
            {
                if (skill.Id == id)
                    return skill;
            }

            return default;
        }

        public static string GetDescription(HeroSkillId id, int nextLevel)
        {
            return Localization.GetSkillDescription(id, nextLevel);
        }

        public static UpgradeOffer[] DrawOffers(HeroUpgradeStats stats, int count)
        {
            var available = new List<SkillDefinition>();

            foreach (SkillDefinition skill in All)
            {
                int level = stats.GetLevel(skill.Id);
                if (level >= MaxSkillLevel)
                    continue;

                if (skill.Id == HeroSkillId.Piercing && level == 0 && !stats.HasPierceWeapon())
                    continue;

                if (level > 0 || stats.CanUnlockNew(skill))
                    available.Add(skill);
            }

            if (available.Count == 0)
                return new[] { UpgradeOffer.Coin(MaxLevelCoinReward) };

            int availableCount = available.Count;
            int skillSlots = Mathf.Min(count, availableCount);
            int totalOffers = availableCount < count ? skillSlots + 1 : skillSlots;
            var result = new UpgradeOffer[totalOffers];

            for (int i = 0; i < skillSlots; i++)
            {
                int index = Random.Range(0, available.Count);
                result[i] = UpgradeOffer.FromSkill(available[index]);
                available.RemoveAt(index);
            }

            if (availableCount < count)
                result[skillSlots] = UpgradeOffer.Coin(MaxLevelCoinReward);

            return result;
        }
    }
}
