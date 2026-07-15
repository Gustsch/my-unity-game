using System.Collections.Generic;
using UnityEngine;

namespace KnightRun.World
{
    [CreateAssetMenu(menuName = "KnightRun/Simple Nature Catalog")]
    public class SimpleNatureCatalog : ScriptableObject
    {
        public const string ResourcePath = "KnightRun/SimpleNatureCatalog";

        public GameObject[] wallPrefabs;
        public GameObject[] obstaclePrefabs;
        public GameObject[] bushBarrierPrefabs;
        public GameObject[] decorationPrefabs;

        static SimpleNatureCatalog instance;
        static readonly Dictionary<Material, Material> urpMaterials = new Dictionary<Material, Material>();

        public static SimpleNatureCatalog Instance
        {
            get
            {
                if (instance == null)
                    instance = Resources.Load<SimpleNatureCatalog>(ResourcePath);

                return instance;
            }
        }

        public GameObject GetRandomWall()
        {
            return GetRandom(wallPrefabs);
        }

        public GameObject GetWall(int index)
        {
            return GetAt(wallPrefabs, index);
        }

        public GameObject GetRandomObstacle()
        {
            return GetRandom(obstaclePrefabs);
        }

        public GameObject GetRandomRock()
        {
            if (obstaclePrefabs == null || obstaclePrefabs.Length == 0)
                return null;

            const int rockPrefabCount = 4;
            int count = Mathf.Min(rockPrefabCount, obstaclePrefabs.Length);
            return GetAt(obstaclePrefabs, Random.Range(0, count));
        }

        public GameObject GetRandomDecoration()
        {
            return GetRandom(decorationPrefabs);
        }

        public GameObject GetBushBarrier(int index)
        {
            return GetAt(bushBarrierPrefabs, index);
        }

        public GameObject GetDecoration(int index)
        {
            return GetAt(decorationPrefabs, index);
        }

        public static GameObject InstantiateVisual(GameObject prefab, Transform parent)
        {
            if (prefab == null)
                return null;

            GameObject visual = Object.Instantiate(prefab, parent);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one;
            RemoveColliders(visual);
            EnsureUrpMaterials(visual);
            return visual;
        }

        public static void RemoveColliders(GameObject visual)
        {
            if (visual == null)
                return;

            foreach (Collider collider in visual.GetComponentsInChildren<Collider>(true))
            {
                collider.enabled = false;
                Object.Destroy(collider);
            }
        }

        static GameObject GetRandom(GameObject[] prefabs)
        {
            GameObject prefab = GetAt(prefabs, Random.Range(0, Mathf.Max(1, prefabs != null ? prefabs.Length : 1)));
            return prefab;
        }

        static GameObject GetAt(GameObject[] prefabs, int index)
        {
            if (prefabs == null || prefabs.Length == 0)
                return null;

            GameObject prefab = prefabs[Mathf.Abs(index) % prefabs.Length];
            return prefab != null ? prefab : null;
        }

        static void EnsureUrpMaterials(GameObject visual)
        {
            Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
            if (urpShader == null)
                return;

            foreach (Renderer renderer in visual.GetComponentsInChildren<Renderer>(true))
            {
                Material[] materials = renderer.sharedMaterials;
                bool changed = false;

                for (int i = 0; i < materials.Length; i++)
                {
                    Material source = materials[i];
                    if (source == null || source.shader == urpShader)
                        continue;

                    if (!urpMaterials.TryGetValue(source, out Material converted))
                    {
                        converted = new Material(urpShader)
                        {
                            name = source.name + "_URP_Runtime",
                            color = source.HasProperty("_Color") ? source.color : Color.white,
                            mainTexture = source.mainTexture
                        };
                        urpMaterials.Add(source, converted);
                    }

                    materials[i] = converted;
                    changed = true;
                }

                if (changed)
                    renderer.sharedMaterials = materials;
            }
        }
    }
}
