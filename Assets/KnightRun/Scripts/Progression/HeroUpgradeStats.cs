using System;
using System.Collections.Generic;
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

        public int SwordDamage => Mathf.Max(1, GetLevel(HeroSkillId.Sword));
        public int BowDamage => Mathf.Max(1, GetLevel(HeroSkillId.Bow));
        public bool HasBow => GetLevel(HeroSkillId.Bow) > 0;
        public bool HasShuriken => GetLevel(HeroSkillId.Shuriken) > 0;
        public bool HasMagicBook => GetLevel(HeroSkillId.MagicBook) > 0;
        public bool HasBomb => GetLevel(HeroSkillId.Bomb) > 0;
        public int BombDamage => Mathf.Max(1, GetLevel(HeroSkillId.Bomb));
        public int AttackVolleyCount => 1 + GetLevel(HeroSkillId.MultiStrike);

        public float ShurikenDamage
        {
            get
            {
                int level = GetLevel(HeroSkillId.Shuriken);
                if (level <= 0)
                    return 0f;

                return SkillPool.ShurikenBaseDamage + (level - 1) * SkillPool.ShurikenDamagePerLevel;
            }
        }

        public float MagicBookAuraDps
        {
            get
            {
                int level = GetLevel(HeroSkillId.MagicBook);
                if (level <= 0)
                    return 0f;

                return SkillPool.MagicBookBaseAuraDps + (level - 1) * SkillPool.MagicBookAuraDpsPerLevel;
            }
        }
        public float AttackSpeedMultiplier => 1f + GetLevel(HeroSkillId.QuickSlash) * SkillPool.QuickSlashSpeedPerLevel;
        public float AttackAreaMultiplier => 1f + GetLevel(HeroSkillId.WideArc) * SkillPool.WideArcAreaPerLevel;
        public int BonusMaxHealth => GetLevel(HeroSkillId.Vigor) * SkillPool.VigorHealthPerLevel;
        public float MoveSpeedMultiplier => 1f + GetLevel(HeroSkillId.AgileSteps) * SkillPool.AgileStepsSpeedPerLevel;
        public float SlideDurationMultiplier => 1f + GetLevel(HeroSkillId.ExtendedSlide) * SkillPool.ExtendedSlideDurationPerLevel;
        public float DamageReductionPercent => GetLevel(HeroSkillId.IronSkin) * SkillPool.IronSkinReductionPerLevel;

        public bool CanLevelUp(HeroSkillId id)
        {
            return GetLevel(id) < SkillPool.MaxSkillLevel;
        }

        public void LevelUpSkill(HeroSkillId id)
        {
            if (!CanLevelUp(id))
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
