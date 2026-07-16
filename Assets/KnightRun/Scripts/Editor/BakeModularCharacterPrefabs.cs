#if UNITY_EDITOR
using KnightRun.Meta;
using KnightRun.Player;
using UnityEditor;
using UnityEngine;

namespace KnightRun.EditorTools
{
    public static class BakeModularCharacterPrefabs
    {
        const string OutputFolder = "Assets/KnightRun/Resources/KnightRun/Characters";
        const string BasePrefabPath =
            "Assets/KnightRun/Resources/KnightRun/Characters/ModularBase.prefab";

        [MenuItem("KnightRun/Bake Modular Character Prefabs")]
        public static void Bake()
        {
            GameObject basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BasePrefabPath);
            if (basePrefab == null)
            {
                Debug.LogError("ModularBase prefab not found at " + BasePrefabPath);
                return;
            }

            BakeOne(basePrefab, HeroCharacterId.Knight);
            BakeOne(basePrefab, HeroCharacterId.Archer);
            BakeOne(basePrefab, HeroCharacterId.Ninja);
            BakeOne(basePrefab, HeroCharacterId.Barbarian);
            BakeOne(basePrefab, HeroCharacterId.Hunter);
            BakeOne(basePrefab, HeroCharacterId.Alchemist);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Baked modular character prefabs into " + OutputFolder);
        }

        static void BakeOne(GameObject basePrefab, HeroCharacterId id)
        {
            GameObject instance = Object.Instantiate(basePrefab);
            instance.name = id.ToString();
            ModularCharacterAssembler.ApplyPreset(instance, CharacterVisualCatalog.GetPreset(id));

            string path = $"{OutputFolder}/{id}.prefab";
            PrefabUtility.SaveAsPrefabAsset(instance, path);
            Object.DestroyImmediate(instance);
        }
    }
}
#endif
