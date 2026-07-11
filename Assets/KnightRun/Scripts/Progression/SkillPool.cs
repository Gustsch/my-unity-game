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
        public const float QuickSlashSpeedPerLevel = 0.08f;
        public const float WideArcAreaPerLevel = 0.2f;
        public const int VigorHealthPerLevel = 200;
        public const float AgileStepsSpeedPerLevel = 0.08f;
        public const float ExtendedSlideDurationPerLevel = 0.1f;
        public const float BowAttackInterval = 1.2f;
        public const float BowMinAttackInterval = 0.45f;
        public const float ShurikenAttackInterval = 0.3f;
        public const float ShurikenMinAttackInterval = 0.1f;
        public const float ShurikenBaseDamage = 4f;
        public const float ShurikenDamagePerLevel = 4f;
        public const float MagicBookBaseAuraDamage = 4f;
        public const float MagicBookAuraDamagePerLevel = 4f;
        public const float MagicBookAuraTickInterval = 0.5f;
        public const float MagicBookAuraStunDuration = 0.1f;
        public const float MagicBookBaseAuraRadius = 2.9f;
        public const float BombAttackInterval = 2f;
        public const float BombBaseDamage = 20f;
        public const float BombMinAttackInterval = 0.8f;
        public const float BombFixedThrowDistance = 25f;
        public const float BombBaseExplosionRadius = 2f;
        public const float BoomerangBaseDamage = 20f;
        public const float BoomerangDamagePerLevel = 5f;
        public const float BoomerangBaseSpeed = 34f;
        public const float ThrowingAxeAttackInterval = 3f;
        public const float ThrowingAxeMinAttackInterval = 1.6f;
        public const float ThrowingAxeBaseDamage = 30f;
        public const float ThrowingAxeDamagePerLevel = 10f;
        public const float ThrowingAxeSpeed = 24f;
        public const float ThrowingAxeDirectionSpread = 35f;

        public static readonly SkillDefinition[] All =
        {
            new SkillDefinition { Id = HeroSkillId.Sword, DisplayName = "Espada", Category = UpgradeCategory.Weapon },
            new SkillDefinition { Id = HeroSkillId.Bow, DisplayName = "Arco e Flecha", Category = UpgradeCategory.Weapon },
            new SkillDefinition { Id = HeroSkillId.Shuriken, DisplayName = "Shurikens", Category = UpgradeCategory.Weapon },
            new SkillDefinition { Id = HeroSkillId.MagicBook, DisplayName = "Livro de Magia", Category = UpgradeCategory.Weapon },
            new SkillDefinition { Id = HeroSkillId.Bomb, DisplayName = "Bombinha", Category = UpgradeCategory.Weapon },
            new SkillDefinition { Id = HeroSkillId.Boomerang, DisplayName = "Bumerangue", Category = UpgradeCategory.Weapon },
            new SkillDefinition { Id = HeroSkillId.ThrowingAxe, DisplayName = "Machado de Arremesso", Category = UpgradeCategory.Weapon },
            new SkillDefinition { Id = HeroSkillId.QuickSlash, DisplayName = "Golpe Rapido", Category = UpgradeCategory.Skill },
            new SkillDefinition { Id = HeroSkillId.WideArc, DisplayName = "Arco Amplo", Category = UpgradeCategory.Skill },
            new SkillDefinition { Id = HeroSkillId.Vigor, DisplayName = "Vigor", Category = UpgradeCategory.Skill },
            new SkillDefinition { Id = HeroSkillId.AgileSteps, DisplayName = "Passos Ageis", Category = UpgradeCategory.Skill },
            new SkillDefinition { Id = HeroSkillId.ExtendedSlide, DisplayName = "Slide Prolongado", Category = UpgradeCategory.Skill },
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
            return id switch
            {
                HeroSkillId.Sword => nextLevel == 1
                    ? "Desbloqueia a espada corpo a corpo e o corte a distancia"
                    : "+10 dano da espada",
                HeroSkillId.Bow => nextLevel == 1
                    ? "Desbloqueia flechas que acertam 1 inimigo"
                    : "+10 dano da flecha",
                HeroSkillId.Shuriken => nextLevel == 1
                    ? $"Lanca shurikens com {ShurikenBaseDamage:0.#} de dano no inimigo mais proximo"
                    : $"+{ShurikenDamagePerLevel:0.#} dano da shuriken",
                HeroSkillId.MagicBook => nextLevel == 1
                    ? $"Aura com {MagicBookBaseAuraDamage:0.#} de dano a cada {MagicBookAuraTickInterval:0.#}s e stun leve"
                    : $"+{MagicBookAuraDamagePerLevel:0.#} dano da aura",
                HeroSkillId.Bomb => nextLevel == 1
                    ? $"Arremessa bomba a {BombFixedThrowDistance:0.#}m que explode em area"
                    : "+10 dano da bomba",
                HeroSkillId.Boomerang => nextLevel == 1
                    ? $"Bumerangue com {BoomerangBaseDamage:0.#} de dano no inimigo mais proximo"
                    : $"+{BoomerangDamagePerLevel:0.#} dano do bumerangue",
                HeroSkillId.ThrowingAxe => nextLevel == 1
                    ? $"Machado lento com {ThrowingAxeBaseDamage:0.#} de dano para frente"
                    : $"+{ThrowingAxeDamagePerLevel:0.#} dano do machado",
                HeroSkillId.QuickSlash => $"+{QuickSlashSpeedPerLevel * 100f:0.#}% velocidade de ataque",
                HeroSkillId.WideArc => $"+{WideArcAreaPerLevel * 100f:0.#}% area do ataque",
                HeroSkillId.Vigor => $"+{VigorHealthPerLevel} HP maximo",
                HeroSkillId.AgileSteps => $"+{AgileStepsSpeedPerLevel * 100f:0.#}% velocidade de movimento",
                HeroSkillId.ExtendedSlide => $"+{ExtendedSlideDurationPerLevel * 100f:0.#}% duracao do slide",
                HeroSkillId.IronSkin => $"-{IronSkinReductionPerLevel * 100f:0.#}% dano recebido",
                _ => string.Empty
            };
        }

        public static UpgradeOffer[] DrawOffers(HeroUpgradeStats stats, int count)
        {
            var available = new List<SkillDefinition>();

            foreach (SkillDefinition skill in All)
            {
                int level = stats.GetLevel(skill.Id);
                if (level >= MaxSkillLevel)
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
