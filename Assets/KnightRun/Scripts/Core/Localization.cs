using System;
using System.Collections.Generic;
using KnightRun.Meta;
using KnightRun.Progression;
using UnityEngine;
using UnityEngine.UI;

namespace KnightRun
{
    public enum GameLanguage
    {
        Portuguese = 0,
        English = 1,
        Spanish = 2,
        French = 3,
        Japanese = 4,
        German = 5
    }

    public static class Localization
    {
        const string LanguageKey = "KnightRun_Language";
        const int LanguageCount = 6;

        static Dictionary<string, string> currentTable;
        static Dictionary<string, string> fallbackTable;

        public static GameLanguage CurrentLanguage { get; private set; } =
            (GameLanguage)Mathf.Clamp(PlayerPrefs.GetInt(LanguageKey, 0), 0, LanguageCount - 1);

        public static event Action OnLanguageChanged;

        static Localization()
        {
            ReloadTables();
        }

        public static string T(string key)
        {
            EnsureTables();
            if (currentTable != null && currentTable.TryGetValue(key, out string value))
                return value;
            if (fallbackTable != null && fallbackTable.TryGetValue(key, out string fallback))
                return fallback;
            return key;
        }

        public static string Format(string key, params object[] args)
        {
            return string.Format(T(key), args);
        }

        public static void ToggleLanguage()
        {
            int next = ((int)CurrentLanguage + 1) % LanguageCount;
            SetLanguage((GameLanguage)next);
        }

        public static void SetLanguage(GameLanguage language)
        {
            if (CurrentLanguage == language && currentTable != null)
                return;

            CurrentLanguage = language;
            PlayerPrefs.SetInt(LanguageKey, (int)language);
            PlayerPrefs.Save();
            ReloadTables();
            OnLanguageChanged?.Invoke();
        }

        public static string GetLanguageButtonLabel()
        {
            return T("ui.language_button");
        }

        public static string GetPhaseName(Core.RunPhase phase)
        {
            return phase switch
            {
                Core.RunPhase.Forest => T("phase.forest"),
                Core.RunPhase.Cave => T("phase.cave"),
                Core.RunPhase.MineCart => T("phase.mine"),
                Core.RunPhase.Volcano => T("phase.volcano"),
                Core.RunPhase.IceCavern => T("phase.ice"),
                Core.RunPhase.Desert => T("phase.desert"),
                _ => phase.ToString()
            };
        }

        public static string GetCharacterName(HeroCharacterId id)
        {
            return id switch
            {
                HeroCharacterId.Knight => T("character.knight"),
                HeroCharacterId.Archer => T("character.archer"),
                HeroCharacterId.Ninja => T("character.ninja"),
                HeroCharacterId.Barbarian => T("character.barbarian"),
                HeroCharacterId.Alchemist => T("character.alchemist"),
                HeroCharacterId.Hunter => T("character.hunter"),
                _ => id.ToString()
            };
        }

        public static string GetSkillName(HeroSkillId id)
        {
            return id switch
            {
                HeroSkillId.Sword => T("skill.sword"),
                HeroSkillId.Bow => T("skill.bow"),
                HeroSkillId.Shuriken => T("skill.shuriken"),
                HeroSkillId.MagicBook => T("skill.magic_book"),
                HeroSkillId.Bomb => T("skill.bomb"),
                HeroSkillId.Boomerang => T("skill.boomerang"),
                HeroSkillId.ThrowingAxe => T("skill.throwing_axe"),
                HeroSkillId.QuickSlash => T("skill.quick_slash"),
                HeroSkillId.WideArc => T("skill.wide_arc"),
                HeroSkillId.Vigor => T("skill.vigor"),
                HeroSkillId.AgileSteps => T("skill.agile_steps"),
                HeroSkillId.ExtendedSlide => T("skill.extended_slide"),
                HeroSkillId.IronSkin => T("skill.iron_skin"),
                _ => id.ToString()
            };
        }

        public static string GetSkillDescription(HeroSkillId id, int nextLevel)
        {
            string skillKey = ToKey(id);
            string unlockKey = $"skill.{skillKey}.desc.unlock";
            string levelKey = $"skill.{skillKey}.desc.level";
            string key = nextLevel == 1 && HasKey(unlockKey) ? unlockKey : levelKey;

            return id switch
            {
                HeroSkillId.Shuriken when nextLevel == 1 => Format(key, SkillPool.ShurikenBaseDamage),
                HeroSkillId.Shuriken => Format(key, SkillPool.ShurikenDamagePerLevel),
                HeroSkillId.Bomb when nextLevel == 1 => Format(key, SkillPool.BombFixedThrowDistance),
                HeroSkillId.Boomerang when nextLevel == 1 => Format(key, SkillPool.BoomerangBaseDamage),
                HeroSkillId.Boomerang => Format(key, SkillPool.BoomerangDamagePerLevel),
                HeroSkillId.ThrowingAxe when nextLevel == 1 => Format(key, SkillPool.ThrowingAxeBaseDamage),
                HeroSkillId.ThrowingAxe => Format(key, SkillPool.ThrowingAxeDamagePerLevel),
                HeroSkillId.QuickSlash => Format(key, SkillPool.QuickSlashSpeedPerLevel * 100f),
                HeroSkillId.WideArc => Format(key, SkillPool.WideArcAreaPerLevel * 100f),
                HeroSkillId.Vigor => Format(key, SkillPool.VigorHealthPerLevel),
                HeroSkillId.AgileSteps => Format(key, SkillPool.AgileStepsSpeedPerLevel * 100f),
                HeroSkillId.ExtendedSlide => Format(key, SkillPool.ExtendedSlideDurationPerLevel * 100f),
                HeroSkillId.IronSkin => Format(key, SkillPool.IronSkinReductionPerLevel * 100f),
                _ => T(key)
            };
        }

        static bool HasKey(string key)
        {
            EnsureTables();
            return (currentTable != null && currentTable.ContainsKey(key))
                || (fallbackTable != null && fallbackTable.ContainsKey(key));
        }

        public static string GetShopName(ShopUpgradeId id)
        {
            return id switch
            {
                ShopUpgradeId.MaxHealth => T("shop.max_health"),
                ShopUpgradeId.MoveSpeed => T("shop.move_speed"),
                ShopUpgradeId.AttackSpeed => T("shop.attack_speed"),
                ShopUpgradeId.Ressurection => T("shop.resurrection"),
                ShopUpgradeId.BaseDamage => T("shop.base_damage"),
                ShopUpgradeId.Healing => T("shop.healing"),
                ShopUpgradeId.MultiStrike => T("shop.multi_strike"),
                _ => id.ToString()
            };
        }

        public static string GetShopDescription(ShopUpgradeId id, int nextLevel)
        {
            string level = T("ui.level_abbr");
            string key = id switch
            {
                ShopUpgradeId.MaxHealth => "shop.max_health.desc",
                ShopUpgradeId.MoveSpeed => "shop.move_speed.desc",
                ShopUpgradeId.AttackSpeed => "shop.attack_speed.desc",
                ShopUpgradeId.Ressurection => "shop.resurrection.desc",
                ShopUpgradeId.BaseDamage => "shop.base_damage.desc",
                ShopUpgradeId.Healing => "shop.healing.desc",
                ShopUpgradeId.MultiStrike => "shop.multi_strike.desc",
                _ => string.Empty
            };

            return string.IsNullOrEmpty(key) ? string.Empty : Format(key, level, nextLevel);
        }

        public static void TranslateStaticText(Transform root)
        {
            if (root == null)
                return;

            EnsureTables();
            Text[] labels = root.GetComponentsInChildren<Text>(true);
            foreach (Text label in labels)
            {
                string key = FindKeyForAnyLanguage(label.text);
                if (!string.IsNullOrEmpty(key))
                    label.text = T(key);
            }
        }

        static string FindKeyForAnyLanguage(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            // Normalize legacy English button labels used at Build time.
            if (value == "PLAY") value = "JOGAR";
            if (value == "HIGHSCORES") value = "RANKING";
            if (value == "OPTIONS") value = "OPÇÕES";
            if (value == "EXIT") value = "SAIR";
            if (value == "SHOP") value = "LOJA";
            if (value == "MENU") value = "MENU";

            foreach (GameLanguage language in Enum.GetValues(typeof(GameLanguage)))
            {
                Dictionary<string, string> table = LocalizationTables.Get(language);
                foreach (KeyValuePair<string, string> pair in table)
                {
                    if (pair.Value == value)
                        return pair.Key;
                }
            }

            return null;
        }

        static string ToKey(HeroSkillId id)
        {
            return id switch
            {
                HeroSkillId.Sword => "sword",
                HeroSkillId.Bow => "bow",
                HeroSkillId.Shuriken => "shuriken",
                HeroSkillId.MagicBook => "magic_book",
                HeroSkillId.Bomb => "bomb",
                HeroSkillId.Boomerang => "boomerang",
                HeroSkillId.ThrowingAxe => "throwing_axe",
                HeroSkillId.QuickSlash => "quick_slash",
                HeroSkillId.WideArc => "wide_arc",
                HeroSkillId.Vigor => "vigor",
                HeroSkillId.AgileSteps => "agile_steps",
                HeroSkillId.ExtendedSlide => "extended_slide",
                HeroSkillId.IronSkin => "iron_skin",
                _ => id.ToString().ToLowerInvariant()
            };
        }

        static void EnsureTables()
        {
            if (currentTable == null)
                ReloadTables();
        }

        static void ReloadTables()
        {
            currentTable = LocalizationTables.Get(CurrentLanguage);
            fallbackTable = LocalizationTables.Get(GameLanguage.English);
        }
    }
}
