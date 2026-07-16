using System;
using KnightRun.Core;
using UnityEngine;

namespace KnightRun.World
{
    [CreateAssetMenu(menuName = "KnightRun/Low Poly Biome Catalog")]
    public class LowPolyBiomeCatalog : ScriptableObject
    {
        public const string ResourcePath = "KnightRun/LowPolyBiomeCatalog";

        [Serializable]
        public class BiomeSet
        {
            [Tooltip("Tiles used as visible ground (platforms / terrain chunks).")]
            public GameObject[] groundPrefabs;

            [Tooltip("Large side pieces: mountains, cave mouths.")]
            public GameObject[] wallPrefabs;

            [Tooltip("Closer barrier rocks along the wall line.")]
            public GameObject[] rockPrefabs;

            [Tooltip("Scatter props on/near the track.")]
            public GameObject[] decorationPrefabs;

            [Tooltip("Path obstacles for this biome (cacti, trees, etc.).")]
            public GameObject[] obstaclePrefabs;

            [Tooltip("Hide the procedural ground cube and use groundPrefabs instead.")]
            public bool replaceGround = true;

            [Tooltip("Hide the procedural wall cubes and use wall/rock prefabs instead.")]
            public bool replaceWalls = true;

            [Range(2f, 12f)]
            public float wallTargetHeight = 5f;

            [Range(1f, 4f)]
            public float rockTargetHeight = 2.2f;

            [Range(0.5f, 3f)]
            public float groundTileDepth = 1.4f;
        }

        public BiomeSet cave;
        public BiomeSet volcano;
        public BiomeSet ice;
        public BiomeSet desert;
        public BiomeSet mine;

        static LowPolyBiomeCatalog instance;

        public static LowPolyBiomeCatalog Instance
        {
            get
            {
                if (instance == null)
                    instance = Resources.Load<LowPolyBiomeCatalog>(ResourcePath);

                return instance;
            }
        }

        public BiomeSet GetSet(RunPhase phase)
        {
            return phase switch
            {
                RunPhase.Cave => cave,
                RunPhase.Volcano => volcano,
                RunPhase.IceCavern => ice,
                RunPhase.Desert => desert,
                RunPhase.MineCart => mine,
                _ => null
            };
        }

        public static bool UsesPrefabEnvironment(RunPhase phase)
        {
            return phase is RunPhase.Cave
                or RunPhase.Volcano
                or RunPhase.IceCavern
                or RunPhase.Desert
                or RunPhase.MineCart;
        }

        public GameObject GetGround(BiomeSet set, int index) => GetAt(set?.groundPrefabs, index);
        public GameObject GetWall(BiomeSet set, int index) => GetAt(set?.wallPrefabs, index);
        public GameObject GetRock(BiomeSet set, int index) => GetAt(set?.rockPrefabs, index);
        public GameObject GetDecoration(BiomeSet set, int index) => GetAt(set?.decorationPrefabs, index);

        public GameObject GetRandomObstacle(BiomeSet set)
        {
            if (set?.obstaclePrefabs == null || set.obstaclePrefabs.Length == 0)
                return null;

            return GetAt(set.obstaclePrefabs, UnityEngine.Random.Range(0, set.obstaclePrefabs.Length));
        }

        static GameObject GetAt(GameObject[] prefabs, int index)
        {
            if (prefabs == null || prefabs.Length == 0)
                return null;

            GameObject prefab = prefabs[Mathf.Abs(index) % prefabs.Length];
            return prefab != null ? prefab : null;
        }
    }
}
