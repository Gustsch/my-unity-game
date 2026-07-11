using System;
using System.Collections.Generic;
using KnightRun.Meta;
using UnityEngine;

namespace KnightRun.Progression
{
    public class HeroUpgradeStats : MonoBehaviour
    {
        readonly Dictionary<HeroSkillId, int> levels = new Dictionary<HeroSkillId, int>();

        public int HeroLevel { get; private set; }

        public event Action<HeroSkillId, int> OnSkillLeveled;
        public event Action OnBonusesChanged;

        public int GetLevel(HeroSkillId id)
        {
            return levels.TryGetValue(id, out int level) ? level : 0;
        }

        public int SwordDamage => ScaleDamage(
            SkillPool.GetSwordDamage(GetLevel(HeroSkillId.Sword)));
        public int BowDamage => ScaleDamage(Mathf.Max(10, GetLevel(HeroSkillId.Bow) * SkillPool.BowDamagePerLevel));
        public bool HasBow => GetLevel(HeroSkillId.Bow) > 0;
        public bool HasShuriken => GetLevel(HeroSkillId.Shuriken) > 0;
        public bool HasMagicBook => GetLevel(HeroSkillId.MagicBook) > 0;
        public bool HasBomb => GetLevel(HeroSkillId.Bomb) > 0;
        public bool HasBoomerang => GetLevel(HeroSkillId.Boomerang) > 0;
        public bool HasThrowingAxe => GetLevel(HeroSkillId.ThrowingAxe) > 0;
        public int BombDamage => ScaleDamage(Mathf.Max(10, GetLevel(HeroSkillId.Bomb) * SkillPool.BombDamagePerLevel));
        public int AttackVolleyCount => 1 + GetLevel(HeroSkillId.MultiStrike);

        public float ShurikenDamage
        {
            get
            {
                int level = GetLevel(HeroSkillId.Shuriken);
                if (level <= 0)
                    return 0f;

                float baseDamage = SkillPool.ShurikenBaseDamage + (level - 1) * SkillPool.ShurikenDamagePerLevel;
                return ScaleDamage(baseDamage);
            }
        }

        public float MagicBookAuraDps
        {
            get
            {
                int level = GetLevel(HeroSkillId.MagicBook);
                if (level <= 0)
                    return 0f;

                float baseDamage = SkillPool.MagicBookBaseAuraDps + (level - 1) * SkillPool.MagicBookAuraDpsPerLevel;
                return ScaleDamage(baseDamage);
            }
        }

        public float BoomerangDamage
        {
            get
            {
                int level = GetLevel(HeroSkillId.Boomerang);
                if (level <= 0)
                    return 0f;

                float baseDamage = SkillPool.BoomerangBaseDamage + (level - 1) * SkillPool.BoomerangDamagePerLevel;
                return ScaleDamage(baseDamage);
            }
        }

        public float ThrowingAxeDamage
        {
            get
            {
                int level = GetLevel(HeroSkillId.ThrowingAxe);
                if (level <= 0)
                    return 0f;

                float baseDamage = SkillPool.ThrowingAxeBaseDamage + (level - 1) * SkillPool.ThrowingAxeDamagePerLevel;
                return ScaleDamage(baseDamage);
            }
        }
        public float AttackSpeedMultiplier => 1f + GetLevel(HeroSkillId.QuickSlash) * SkillPool.QuickSlashSpeedPerLevel;
        public float AttackAreaMultiplier => 1f + GetLevel(HeroSkillId.WideArc) * SkillPool.WideArcAreaPerLevel;
        public int BonusMaxHealth => GetLevel(HeroSkillId.Vigor) * SkillPool.VigorHealthPerLevel;
        public float MoveSpeedMultiplier => 1f + GetLevel(HeroSkillId.AgileSteps) * SkillPool.AgileStepsSpeedPerLevel;
        public float SlideDurationMultiplier => 1f + GetLevel(HeroSkillId.ExtendedSlide) * SkillPool.ExtendedSlideDurationPerLevel;
        public float DamageReductionPercent => GetLevel(HeroSkillId.IronSkin) * SkillPool.IronSkinReductionPerLevel;

        static int ScaleDamage(int damage) =>
            Mathf.Max(1, Mathf.RoundToInt(damage * MetaBonuses.BaseDamageMultiplier));

        static float ScaleDamage(float damage) =>
            Mathf.Max(1f, damage * MetaBonuses.BaseDamageMultiplier);

        public int GetUnlockedWeaponCount()
        {
            int count = 0;
            foreach (SkillDefinition skill in SkillPool.All)
            {
                if (skill.Category != UpgradeCategory.Weapon)
                    continue;

                if (GetLevel(skill.Id) > 0)
                    count++;
            }

            return count;
        }

        public int GetUnlockedPassiveSkillCount()
        {
            int count = 0;
            foreach (SkillDefinition skill in SkillPool.All)
            {
                if (skill.Category != UpgradeCategory.Skill)
                    continue;

                if (GetLevel(skill.Id) > 0)
                    count++;
            }

            return count;
        }

        public bool CanUnlockNew(SkillDefinition skill)
        {
            if (GetLevel(skill.Id) > 0)
                return true;

            return skill.Category switch
            {
                UpgradeCategory.Weapon => GetUnlockedWeaponCount() < SkillPool.MaxWeapons,
                UpgradeCategory.Skill => GetUnlockedPassiveSkillCount() < SkillPool.MaxSkills,
                _ => false
            };
        }

        public bool CanLevelUp(HeroSkillId id)
        {
            return GetLevel(id) < SkillPool.MaxSkillLevel;
        }

        public void LevelUpSkill(HeroSkillId id)
        {
            if (!CanLevelUp(id))
                return;

            SkillDefinition skill = SkillPool.Get(id);
            if (GetLevel(id) == 0 && !CanUnlockNew(skill))
                return;

            int nextLevel = GetLevel(id) + 1;
            levels[id] = nextLevel;
            HeroLevel++;

            OnSkillLeveled?.Invoke(id, nextLevel);
            OnBonusesChanged?.Invoke();
        }

        public void ResetBonuses()
        {
            levels.Clear();
            levels[HeroSkillId.Sword] = SkillPool.StartingSwordLevel;
            HeroLevel = 0;
            OnBonusesChanged?.Invoke();
        }

        void Awake()
        {
            if (GetLevel(HeroSkillId.Sword) == 0)
                ResetBonuses();
        }
    }
}
