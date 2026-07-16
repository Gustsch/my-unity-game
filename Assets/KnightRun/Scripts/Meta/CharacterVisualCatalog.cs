using UnityEngine;

namespace KnightRun.Meta
{
    [System.Serializable]
    public struct ModularCharacterPreset
    {
        public int armorType;
        public int armorColor;
        public bool showHelmet;
        public int hairType;
        public int hairColor;
        public int faceHairType;
        public int faceHairColor;
        public int eyesType;
        public int eyesColor;
        public int eyebrowType;
        public int eyebrowColor;
        public int noseType;
        public int earsType;
        public float visualScale;
        public Vector3 visualOffset;
        public Vector3 visualEuler;

        public static ModularCharacterPreset CreateDefault(
            int armorType,
            int armorColor,
            bool showHelmet,
            int hairType = 1,
            int hairColor = 1,
            int faceHairType = 0,
            int faceHairColor = 1,
            int eyesType = 1,
            int eyesColor = 1,
            int eyebrowType = 1,
            int eyebrowColor = 1,
            int noseType = 1,
            int earsType = 1)
        {
            return new ModularCharacterPreset
            {
                armorType = armorType,
                armorColor = armorColor,
                showHelmet = showHelmet,
                hairType = hairType,
                hairColor = hairColor,
                faceHairType = faceHairType,
                faceHairColor = faceHairColor,
                eyesType = eyesType,
                eyesColor = eyesColor,
                eyebrowType = eyebrowType,
                eyebrowColor = eyebrowColor,
                noseType = noseType,
                earsType = earsType,
                visualScale = 1f,
                visualOffset = Vector3.zero,
                visualEuler = Vector3.zero
            };
        }
    }

    [System.Serializable]
    public struct CharacterVisualDefinition
    {
        public HeroCharacterId id;
        public bool useLegacyCapsuleVisual;
        public ModularCharacterPreset preset;
    }

    [CreateAssetMenu(menuName = "KnightRun/Character Visual Catalog")]
    public class CharacterVisualCatalog : ScriptableObject
    {
        public const string ResourcePath = "KnightRun/CharacterVisualCatalog";
        public const string ModularBaseResourcePath = "KnightRun/Characters/ModularBase";

        public GameObject modularBasePrefab;
        public CharacterVisualDefinition[] entries;

        static CharacterVisualCatalog instance;

        public static CharacterVisualCatalog Instance
        {
            get
            {
                if (instance == null)
                    instance = Resources.Load<CharacterVisualCatalog>(ResourcePath);
                return instance;
            }
        }

        public static bool UsesLegacyVisual(HeroCharacterId id)
        {
            CharacterVisualDefinition definition = GetDefinition(id);
            return definition.useLegacyCapsuleVisual;
        }

        public static CharacterVisualDefinition GetDefinition(HeroCharacterId id)
        {
            CharacterVisualCatalog catalog = Instance;
            if (catalog != null && catalog.entries != null)
            {
                for (int i = 0; i < catalog.entries.Length; i++)
                {
                    if (catalog.entries[i].id == id)
                        return catalog.entries[i];
                }
            }

            return CreateFallback(id);
        }

        public static ModularCharacterPreset GetPreset(HeroCharacterId id)
        {
            return GetDefinition(id).preset;
        }

        public static GameObject LoadModularBasePrefab()
        {
            CharacterVisualCatalog catalog = Instance;
            if (catalog != null && catalog.modularBasePrefab != null)
                return catalog.modularBasePrefab;

            return Resources.Load<GameObject>(ModularBaseResourcePath);
        }

        public static GameObject LoadCharacterPrefab(HeroCharacterId id)
        {
            if (!UsesLegacyVisual(id))
            {
                GameObject baked = Resources.Load<GameObject>($"KnightRun/Characters/{id}");
                if (baked != null)
                    return baked;
            }

            return LoadModularBasePrefab();
        }

        static CharacterVisualDefinition CreateFallback(HeroCharacterId id)
        {
            return id switch
            {
                HeroCharacterId.Archer => new CharacterVisualDefinition
                {
                    id = id,
                    useLegacyCapsuleVisual = false,
                    preset = ModularCharacterPreset.CreateDefault(
                        armorType: 2,
                        armorColor: 2,
                        showHelmet: false,
                        hairType: 2,
                        hairColor: 2,
                        faceHairType: 0,
                        eyesType: 2,
                        eyesColor: 3,
                        eyebrowType: 2,
                        eyebrowColor: 2,
                        noseType: 2)
                },
                HeroCharacterId.Ninja => new CharacterVisualDefinition
                {
                    id = id,
                    useLegacyCapsuleVisual = false,
                    preset = ModularCharacterPreset.CreateDefault(
                        armorType: 3,
                        armorColor: 3,
                        showHelmet: true,
                        hairType: 1,
                        hairColor: 5,
                        faceHairType: 0,
                        eyesType: 4,
                        eyesColor: 1,
                        eyebrowType: 4,
                        eyebrowColor: 5,
                        noseType: 1)
                },
                HeroCharacterId.Barbarian => new CharacterVisualDefinition
                {
                    id = id,
                    useLegacyCapsuleVisual = false,
                    preset = ModularCharacterPreset.CreateDefault(
                        armorType: 4,
                        armorColor: 1,
                        showHelmet: false,
                        hairType: 5,
                        hairColor: 1,
                        faceHairType: 5,
                        faceHairColor: 1,
                        eyesType: 3,
                        eyesColor: 2,
                        eyebrowType: 5,
                        eyebrowColor: 1,
                        noseType: 4)
                },
                HeroCharacterId.Hunter => new CharacterVisualDefinition
                {
                    id = id,
                    useLegacyCapsuleVisual = false,
                    preset = ModularCharacterPreset.CreateDefault(
                        armorType: 5,
                        armorColor: 2,
                        showHelmet: false,
                        hairType: 3,
                        hairColor: 2,
                        faceHairType: 2,
                        faceHairColor: 2,
                        eyesType: 1,
                        eyesColor: 4,
                        eyebrowType: 3,
                        eyebrowColor: 2,
                        noseType: 3)
                },
                HeroCharacterId.Alchemist => new CharacterVisualDefinition
                {
                    id = id,
                    useLegacyCapsuleVisual = false,
                    preset = ModularCharacterPreset.CreateDefault(
                        armorType: 6,
                        armorColor: 3,
                        showHelmet: false,
                        hairType: 4,
                        hairColor: 4,
                        faceHairType: 3,
                        faceHairColor: 3,
                        eyesType: 2,
                        eyesColor: 5,
                        eyebrowType: 3,
                        eyebrowColor: 4,
                        noseType: 3,
                        earsType: 1)
                },
                _ => new CharacterVisualDefinition
                {
                    id = HeroCharacterId.Knight,
                    useLegacyCapsuleVisual = false,
                    preset = ModularCharacterPreset.CreateDefault(
                        armorType: 1,
                        armorColor: 1,
                        showHelmet: true,
                        hairType: 1,
                        hairColor: 1,
                        faceHairType: 1,
                        faceHairColor: 1,
                        eyesType: 1,
                        eyesColor: 1,
                        eyebrowType: 1,
                        eyebrowColor: 1,
                        noseType: 1)
                }
            };
        }
    }
}
