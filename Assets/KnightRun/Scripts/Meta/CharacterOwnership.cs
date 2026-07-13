using UnityEngine;

namespace KnightRun.Meta
{
    public static class CharacterOwnership
    {
        static readonly HeroCharacterId[] PurchasableCharacters =
        {
            HeroCharacterId.Archer,
            HeroCharacterId.Ninja,
            HeroCharacterId.Barbarian,
            HeroCharacterId.Alchemist,
            HeroCharacterId.Hunter,
        };

        public static bool IsOwned(HeroCharacterId id)
        {
            if (id == HeroCharacterId.Knight)
                return true;

            return PlayerPrefs.GetInt(GetOwnedKey(id), 0) > 0;
        }

        public static void SetOwned(HeroCharacterId id)
        {
            if (id == HeroCharacterId.Knight)
                return;

            PlayerPrefs.SetInt(GetOwnedKey(id), 1);
        }

        public static void Reset()
        {
            foreach (HeroCharacterId id in PurchasableCharacters)
                PlayerPrefs.DeleteKey(GetOwnedKey(id));
        }

        static string GetOwnedKey(HeroCharacterId id) => $"KnightRun_CharacterOwned_{id}";
    }
}
