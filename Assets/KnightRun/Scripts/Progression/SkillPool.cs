using System.Collections.Generic;
using UnityEngine;

namespace KnightRun.Progression
{
    public static class SkillPool
    {
        public const int MaxSkillLevel = 10;
        public const int StartingSwordLevel = 1;

        public const float IronSkinReductionPerLevel = 0.1f;
        public const float QuickSlashSpeedPerLevel = 0.08f;
        public const float WideArcAreaPerLevel = 0.2f;
        public const int VigorHealthPerLevel = 20;
        public const float AgileStepsSpeedPerLevel = 0.08f;
        public const float ExtendedSlideDurationPerLevel = 0.1f;
        public const float BowAttackInterval = 1.2f;
        public const float BowMinAttackInterval = 0.45f;
        public const float ShurikenAttackInterval = 0.3f;
        public const float ShurikenMinAttackInterval = 0.1f;
        public const float ShurikenBaseDamage = 0.4f;
        public const float ShurikenDamagePerLevel = 0.4f;
        public const float MagicBookBaseAuraDps = 0.1f;
        public const float MagicBookAuraDpsPerLevel = 0.1f;
        public const float MagicBookBaseAuraRadius = 2.2f;
        public const float BombAttackInterval = 2f;
        public const float BombBaseDamage = 2f;
        public const float BombDamagePerLevel = 0.1f;
        public const float BombMinAttackInterval = 0.8f;
        public const float BombFixedThrowDistance = 15f;
        public const float BombBaseExplosionRadius = 2f;

        public static readonly SkillDefinition[] All =
        {
            new SkillDefinition { Id = HeroSkillId.Sword, DisplayName = "Espada", Category = UpgradeCategory.Weapon },
            new SkillDefinition { Id = HeroSkillId.Bow, DisplayName = "Arco e Flecha", Category = UpgradeCategory.Weapon },
            new SkillDefinition { Id = HeroSkillId.Shuriken, DisplayName = "Shurikens", Category = UpgradeCategory.Weapon },
            new SkillDefinition { Id = HeroSkillId.MagicBook, DisplayName = "Livro de Magia", Category = UpgradeCategory.Weapon },
            new SkillDefinition { Id = HeroSkillId.Bomb, DisplayName = "Bombinha", Category = UpgradeCategory.Weapon },
            new SkillDefinition { Id = HeroSkillId.QuickSlash, DisplayName = "Golpe Rapido", Category = UpgradeCategory.Skill },
            new SkillDefinition { Id = HeroSkillId.WideArc, DisplayName = "Arco Amplo", Category = UpgradeCategory.Skill },
            new SkillDefinition { Id = HeroSkillId.MultiStrike, DisplayName = "Ataque Multiplo", Category = UpgradeCategory.Skill },
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
                HeroSkillId.Sword => "+1 dano da espada",
                HeroSkillId.Bow => nextLevel == 1
                    ? "Desbloqueia flechas que acertam 1 inimigo"
                    : "+1 dano da flecha",
                HeroSkillId.Shuriken => nextLevel == 1
                    ? $"Lanca shurikens com {ShurikenBaseDamage:0.#} de dano no inimigo mais proximo"
                    : $"+{ShurikenDamagePerLevel:0.#} dano da shuriken",
                HeroSkillId.MagicBook => nextLevel == 1
                    ? $"Aura com {MagicBookBaseAuraDps:0.#} de dano/s em inimigos proximos"
                    : $"+{MagicBookAuraDpsPerLevel:0.#} dano/s da aura",
                HeroSkillId.Bomb => nextLevel == 1
                    ? $"Arremessa bomba a {BombFixedThrowDistance:0.#}m que explode em area"
                    : "+1 dano da bomba",
                HeroSkillId.QuickSlash => $"+{QuickSlashSpeedPerLevel * 100f:0.#}% velocidade de ataque",
                HeroSkillId.WideArc => $"+{WideArcAreaPerLevel * 100f:0.#}% area do ataque",
                HeroSkillId.MultiStrike => "+1 ataque por vez",
                HeroSkillId.Vigor => $"+{VigorHealthPerLevel} HP maximo",
                HeroSkillId.AgileSteps => $"+{AgileStepsSpeedPerLevel * 100f:0.#}% velocidade de movimento",
                HeroSkillId.ExtendedSlide => $"+{ExtendedSlideDurationPerLevel * 100f:0.#}% duracao do slide",
                HeroSkillId.IronSkin => $"-{IronSkinReductionPerLevel * 100f:0.#}% dano recebido",
                _ => string.Empty
            };
        }

        public static SkillDefinition[] DrawOffers(HeroUpgradeStats stats, int count)
        {
            var available = new List<SkillDefinition>();

            foreach (SkillDefinition skill in All)
            {
                if (stats.GetLevel(skill.Id) < MaxSkillLevel)
                    available.Add(skill);
            }

            int drawCount = Mathf.Min(count, available.Count);
            var result = new SkillDefinition[drawCount];

            for (int i = 0; i < drawCount; i++)
            {
                int index = Random.Range(0, available.Count);
                result[i] = available[index];
                available.RemoveAt(index);
            }

            return result;
        }
    }
}
