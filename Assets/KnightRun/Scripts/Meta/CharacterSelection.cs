using System;
using UnityEngine;

namespace KnightRun.Meta
{
    public static class CharacterSelection
    {
        const string SelectedCharacterKey = "KnightRun_SelectedCharacter";

        static HeroCharacterId selectedCharacter = HeroCharacterId.Knight;

        public static HeroCharacterId SelectedCharacter => selectedCharacter;

        public static event Action OnCharacterChanged;

        public static void Load()
        {
            int raw = PlayerPrefs.GetInt(SelectedCharacterKey, (int)HeroCharacterId.Knight);
            if (!Enum.IsDefined(typeof(HeroCharacterId), raw))
                raw = (int)HeroCharacterId.Knight;

            selectedCharacter = (HeroCharacterId)raw;
            if (!CharacterOwnership.IsOwned(selectedCharacter))
                selectedCharacter = HeroCharacterId.Knight;
        }

        public static void Select(HeroCharacterId id)
        {
            if (!Enum.IsDefined(typeof(HeroCharacterId), id))
                id = HeroCharacterId.Knight;

            if (!CharacterOwnership.IsOwned(id))
                return;

            if (selectedCharacter == id)
                return;

            selectedCharacter = id;
            PlayerPrefs.SetInt(SelectedCharacterKey, (int)id);
            PlayerPrefs.Save();
            OnCharacterChanged?.Invoke();
        }

        public static void ResetToDefault()
        {
            selectedCharacter = HeroCharacterId.Knight;
            PlayerPrefs.DeleteKey(SelectedCharacterKey);
        }
    }
}
