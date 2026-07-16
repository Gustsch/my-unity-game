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

        const string UrpVertexColorShaderName = "KnightRun/URP Vertex Color Unlit";

        static void EnsureUrpMaterials(GameObject visual)
        {
            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
            Shader urpVertexColor = Shader.Find(UrpVertexColorShaderName);
            if (urpLit == null)
                return;

            foreach (Renderer renderer in visual.GetComponentsInChildren<Renderer>(true))
            {
                Material[] materials = renderer.sharedMaterials;
                bool changed = false;

                for (int i = 0; i < materials.Length; i++)
                {
                    Material source = materials[i];
                    if (source == null)
                        continue;

                    if (IsVertexColorShader(source.shader))
                    {
                        if (urpVertexColor == null || source.shader == urpVertexColor)
                            continue;

                        if (!urpMaterials.TryGetValue(source, out Material vertexConverted))
                        {
                            vertexConverted = CreateVertexColorMaterial(source, urpVertexColor);
                            urpMaterials.Add(source, vertexConverted);
                        }

                        materials[i] = vertexConverted;
                        changed = true;
                        continue;
                    }

                    if (!NeedsBuiltInToUrpConversion(source, urpLit))
                        continue;

                    if (!urpMaterials.TryGetValue(source, out Material converted))
                    {
                        converted = new Material(urpLit)
                        {
                            name = source.name + "_URP_Runtime"
                        };

                        Color color = source.HasProperty("_BaseColor")
                            ? source.GetColor("_BaseColor")
                            : source.HasProperty("_Color")
                                ? source.GetColor("_Color")
                                : Color.white;
                        if (converted.HasProperty("_BaseColor"))
                            converted.SetColor("_BaseColor", color);
                        converted.color = color;

                        Texture mainTex = null;
                        if (source.HasProperty("_BaseMap"))
                            mainTex = source.GetTexture("_BaseMap");
                        if (mainTex == null)
                            mainTex = source.mainTexture;
                        if (mainTex != null)
                        {
                            if (converted.HasProperty("_BaseMap"))
                                converted.SetTexture("_BaseMap", mainTex);
                            converted.mainTexture = mainTex;
                        }

                        urpMaterials.Add(source, converted);
                    }

                    materials[i] = converted;
                    changed = true;
                }

                if (changed)
                    renderer.sharedMaterials = materials;
            }
        }

        static Material CreateVertexColorMaterial(Material source, Shader urpVertexColor)
        {
            Color tint = Color.white;
            if (source.HasProperty("_BaseColor"))
                tint = source.GetColor("_BaseColor");
            else if (source.HasProperty("_Color"))
                tint = source.GetColor("_Color");

            Material converted = new Material(urpVertexColor)
            {
                name = source.name + "_URP_VertexColor"
            };
            if (converted.HasProperty("_BaseColor"))
                converted.SetColor("_BaseColor", tint);
            if (converted.HasProperty("_Color"))
                converted.SetColor("_Color", tint);
            return converted;
        }

        static bool IsVertexColorShader(Shader shader)
        {
            if (shader == null)
                return false;

            string shaderName = shader.name;
            if (string.IsNullOrEmpty(shaderName))
                return false;

            return shaderName.IndexOf("Vertex_Color", System.StringComparison.OrdinalIgnoreCase) >= 0
                || shaderName.IndexOf("Vertex Color", System.StringComparison.OrdinalIgnoreCase) >= 0
                || shaderName.IndexOf("VertexColor", System.StringComparison.OrdinalIgnoreCase) >= 0
                || shaderName == UrpVertexColorShaderName;
        }

        static bool NeedsBuiltInToUrpConversion(Material source, Shader urpLit)
        {
            if (source.shader == null)
                return true;

            if (source.shader == urpLit)
                return false;

            string shaderName = source.shader.name;
            if (string.IsNullOrEmpty(shaderName))
                return true;

            if (shaderName.StartsWith("Universal Render Pipeline/", System.StringComparison.Ordinal))
                return false;
            if (shaderName.StartsWith("KnightRun/", System.StringComparison.Ordinal))
                return false;
            if (IsVertexColorShader(source.shader))
                return false;

            return shaderName == "Standard"
                || shaderName == "Standard (Specular setup)"
                || shaderName.StartsWith("Legacy Shaders/", System.StringComparison.Ordinal)
                || shaderName.StartsWith("Hidden/InternalErrorShader", System.StringComparison.Ordinal);
        }
    }
}
