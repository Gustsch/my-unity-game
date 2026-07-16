#if UNITY_EDITOR
using System.Collections.Generic;
using KnightRun.World;
using UnityEditor;
using UnityEngine;

namespace KnightRun.Editor
{
    [InitializeOnLoad]
    public static class BuildLowPolyBiomeCatalog
    {
        const string CatalogPath = "Assets/KnightRun/Resources/KnightRun/LowPolyBiomeCatalog.asset";
        const string PrefsKey = "KnightRun.LowPolyBiomeCatalog.BuildVersion";
        const int BuildVersion = 6;

        static readonly string EnvRoot = "Assets/3D Enivronment Assets/Prefabs";
        static readonly string DesertPackRoot = "Assets/Free Low Poly Desert Pack/Unity Prefabs";
        static readonly string NatureRoot = "Assets/SimpleNaturePack/Prefabs";

        static BuildLowPolyBiomeCatalog()
        {
            EditorApplication.delayCall += AutoBuildIfNeeded;
        }

        [MenuItem("Knight Run/Build Low Poly Biome Catalog")]
        public static void Build()
        {
            EnsureCatalogWired(force: true);
            EditorPrefs.SetInt(PrefsKey, BuildVersion);
            Debug.Log("[Knight Run] LowPolyBiomeCatalog built (v" + BuildVersion + ").");
        }

        static void AutoBuildIfNeeded()
        {
            LowPolyBiomeCatalog catalog = AssetDatabase.LoadAssetAtPath<LowPolyBiomeCatalog>(CatalogPath);
            bool needsBuild = EditorPrefs.GetInt(PrefsKey, 0) < BuildVersion
                || catalog == null
                || !HasAnyPrefabs(catalog);

            if (!needsBuild)
                return;

            EnsureCatalogWired(force: true);
            EditorPrefs.SetInt(PrefsKey, BuildVersion);
        }

        static void EnsureCatalogWired(bool force)
        {
            System.IO.Directory.CreateDirectory("Assets/KnightRun/Resources/KnightRun");

            LowPolyBiomeCatalog catalog = AssetDatabase.LoadAssetAtPath<LowPolyBiomeCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<LowPolyBiomeCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }
            else if (!force && HasAnyPrefabs(catalog))
            {
                return;
            }

            // Cave: brown stone only — never Magma (red). Keep textured ground cube.
            catalog.cave = new LowPolyBiomeCatalog.BiomeSet
            {
                groundPrefabs = new GameObject[0],
                wallPrefabs = LoadMany($"{EnvRoot}/DesertPrefabs", "DesertMountain_01", "DesertMountain_02", "DesertMountain_03"),
                rockPrefabs = Combine(
                    LoadMany($"{EnvRoot}/DesertPrefabs", "DesertRock_01", "DesertRock_02", "DesertRock_03"),
                    LoadMany(NatureRoot, "Rock_01", "Rock_02", "Rock_03", "Rock_04")),
                decorationPrefabs = new GameObject[0],
                replaceGround = false,
                replaceWalls = true,
                wallTargetHeight = 4.5f,
                rockTargetHeight = 2.6f,
                groundTileDepth = 1.35f
            };

            catalog.volcano = new LowPolyBiomeCatalog.BiomeSet
            {
                // Keep the procedural lava ground visible; Magma platforms leave gaps when used alone.
                groundPrefabs = new GameObject[0],
                wallPrefabs = LoadMany($"{EnvRoot}/MagmaPrefabs", "MagmaMountain_01", "MagmaMountain_02", "MagmaMountain_03"),
                rockPrefabs = LoadMany($"{EnvRoot}/MagmaPrefabs", "MagmaRock_01", "MagmaRock_02", "MagmaRock_03"),
                decorationPrefabs = LoadMany($"{EnvRoot}/MagmaPrefabs", "MagmaGrass", "MagmaTree"),
                replaceGround = false,
                replaceWalls = true,
                wallTargetHeight = 6f,
                rockTargetHeight = 2.3f,
                groundTileDepth = 1.35f
            };

            catalog.ice = new LowPolyBiomeCatalog.BiomeSet
            {
                // Platforms leave gaps — keep solid ice-colored ground cube.
                groundPrefabs = new GameObject[0],
                wallPrefabs = LoadMany($"{EnvRoot}/IcePrefabs", "IceMountain_01", "IceMountain_02", "IceMountain_03"),
                rockPrefabs = LoadMany($"{EnvRoot}/IcePrefabs", "IceRock_01", "IceRock_02", "IceRock_03"),
                decorationPrefabs = LoadMany($"{EnvRoot}/IcePrefabs", "IceTree", "Snowman_01", "Snowman_02"),
                replaceGround = false,
                replaceWalls = true,
                wallTargetHeight = 5.5f,
                rockTargetHeight = 2.2f,
                groundTileDepth = 1.35f
            };

            catalog.desert = new LowPolyBiomeCatalog.BiomeSet
            {
                // Same as ice/volcano: procedural ground avoids untextured holes between tiles.
                groundPrefabs = new GameObject[0],
                wallPrefabs = Combine(
                    LoadMany($"{EnvRoot}/DesertPrefabs", "DesertMountain_01", "DesertMountain_02", "DesertMountain_03"),
                    LoadMany($"{DesertPackRoot}/Rocks", "Mixed_Rock_01", "Flat_Rock_01")),
                rockPrefabs = Combine(
                    LoadMany($"{EnvRoot}/DesertPrefabs", "DesertRock_01", "DesertRock_02", "DesertRock_03"),
                    LoadMany($"{DesertPackRoot}/Rocks", "Mixed_Rock_01", "Flat_Rock_01")),
                decorationPrefabs = new GameObject[0],
                obstaclePrefabs = Combine(
                    LoadMany($"{EnvRoot}/DesertPrefabs", "Cactus_01", "Cactus_02", "Cactus_03", "Cactus_04", "DesertTree", "DesertGrass_01", "DesertGrass_02"),
                    LoadMany($"{DesertPackRoot}/Plants", "Mixed_Cactus_01", "Mixed_Cactus_02", "Flat_Cactus_01", "Flat_Cactus_02", "Mixed_Plant_02", "Flat_Plant_02"),
                    LoadMany($"{DesertPackRoot}/Trees", "Mixed_Palm_tree_01", "Flat_Palm_tree_01", "Mixed_Tree_02", "Flat_Tree_02")),
                replaceGround = false,
                replaceWalls = true,
                wallTargetHeight = 7f,
                rockTargetHeight = 2.6f,
                groundTileDepth = 2.2f
            };

            // Mine: keep rails + cave wall cubes. Only scatter props — no sandbags-as-floor.
            catalog.mine = new LowPolyBiomeCatalog.BiomeSet
            {
                groundPrefabs = new GameObject[0],
                wallPrefabs = new GameObject[0],
                rockPrefabs = Combine(
                    LoadMany($"{EnvRoot}/DesertPrefabs", "DesertRock_01", "DesertRock_02"),
                    LoadMany(NatureRoot, "Rock_01", "Rock_02", "Rock_03")),
                decorationPrefabs = LoadMany(
                    $"{EnvRoot}/GeneralPrefabs",
                    "Toolbox", "CraftingTable", "Dynamite_01", "Dynamite_02", "TNT", "Chest", "Hammer", "SandBag"),
                replaceGround = false,
                replaceWalls = false,
                wallTargetHeight = 3.2f,
                rockTargetHeight = 1.8f,
                groundTileDepth = 1.2f
            };

            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        static bool HasAnyPrefabs(LowPolyBiomeCatalog catalog)
        {
            return (catalog.cave?.rockPrefabs?.Length ?? 0) > 0
                || (catalog.desert?.obstaclePrefabs?.Length ?? 0) > 0
                || (catalog.volcano?.wallPrefabs?.Length ?? 0) > 0;
        }

        static GameObject[] LoadMany(string folder, params string[] names)
        {
            var list = new List<GameObject>(names.Length);
            foreach (string name in names)
            {
                string path = $"{folder}/{name}.prefab";
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                    list.Add(prefab);
                else
                    Debug.LogWarning($"[Knight Run] Missing biome prefab: {path}");
            }

            return list.ToArray();
        }

        static GameObject[] Combine(params GameObject[][] groups)
        {
            var list = new List<GameObject>();
            foreach (GameObject[] group in groups)
            {
                if (group == null)
                    continue;

                foreach (GameObject prefab in group)
                {
                    if (prefab != null)
                        list.Add(prefab);
                }
            }

            return list.ToArray();
        }
    }
}
#endif
