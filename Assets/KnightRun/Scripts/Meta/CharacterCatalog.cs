using KnightRun.Core;
using KnightRun.Progression;

namespace KnightRun.Meta
{
    public struct CharacterDefinition
    {
        public HeroCharacterId Id;
        public string DisplayName;
        public HeroSkillId StartingWeapon;
        public string WeaponName;
        public int UnlockAfterBossPhaseIndex;
        public int PurchaseCost;
    }

    public static class CharacterCatalog
    {
        public const int StartingWeaponLevel = 1;

        public static readonly CharacterDefinition[] All =
        {
            new CharacterDefinition
            {
                Id = HeroCharacterId.Knight,
                DisplayName = "Cavaleiro",
                StartingWeapon = HeroSkillId.Sword,
                WeaponName = "Espada",
                UnlockAfterBossPhaseIndex = -1,
                PurchaseCost = 0
            },
            new CharacterDefinition
            {
                Id = HeroCharacterId.Archer,
                DisplayName = "Arqueiro",
                StartingWeapon = HeroSkillId.Bow,
                WeaponName = "Arco e Flecha",
                UnlockAfterBossPhaseIndex = 0,
                PurchaseCost = 150
            },
            new CharacterDefinition
            {
                Id = HeroCharacterId.Ninja,
                DisplayName = "Ninja",
                StartingWeapon = HeroSkillId.Shuriken,
                WeaponName = "Shuriken",
                UnlockAfterBossPhaseIndex = 1,
                PurchaseCost = 250
            },
            new CharacterDefinition
            {
                Id = HeroCharacterId.Barbarian,
                DisplayName = "Barbaro",
                StartingWeapon = HeroSkillId.ThrowingAxe,
                WeaponName = "Machado de Arremesso",
                UnlockAfterBossPhaseIndex = 2,
                PurchaseCost = 400
            },
            new CharacterDefinition
            {
                Id = HeroCharacterId.Alchemist,
                DisplayName = "Alquimista",
                StartingWeapon = HeroSkillId.Bomb,
                WeaponName = "Bombinha",
                UnlockAfterBossPhaseIndex = 3,
                PurchaseCost = 550
            },
            new CharacterDefinition
            {
                Id = HeroCharacterId.Hunter,
                DisplayName = "Cacador",
                StartingWeapon = HeroSkillId.Boomerang,
                WeaponName = "Bumerangue",
                UnlockAfterBossPhaseIndex = 4,
                PurchaseCost = 700
            }
        };

        public static CharacterDefinition Get(HeroCharacterId id)
        {
            foreach (CharacterDefinition character in All)
            {
                if (character.Id == id)
                    return character;
            }

            return All[0];
        }

        public static HeroSkillId GetStartingWeapon(HeroCharacterId id)
        {
            return Get(id).StartingWeapon;
        }

        public static bool IsUnlockedForPurchase(HeroCharacterId id)
        {
            if (DebugTestMode.IsActive)
                return true;

            CharacterDefinition character = Get(id);
            if (character.UnlockAfterBossPhaseIndex < 0)
                return true;

            return CharacterUnlockProgress.HasDefeatedBossPhase(character.UnlockAfterBossPhaseIndex);
        }

        public static string GetUnlockRequirementText(HeroCharacterId id)
        {
            CharacterDefinition character = Get(id);
            if (character.UnlockAfterBossPhaseIndex < 0)
                return string.Empty;

            int phaseIndex = character.UnlockAfterBossPhaseIndex;
            if (phaseIndex < 0 || phaseIndex >= RunPhaseDefaults.All.Length)
                return Localization.T("ui.unlock_previous");

            string phaseName = Localization.GetPhaseName(RunPhaseDefaults.All[phaseIndex].phase);
            return Localization.Format("ui.unlock_phase", phaseName);
        }
    }
}
