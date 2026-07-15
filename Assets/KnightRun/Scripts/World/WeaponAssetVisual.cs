using System.Collections.Generic;
using UnityEngine;

namespace KnightRun.World
{
    public static class WeaponAssetVisual
    {
        const string ResourceRoot = "KnightRun/Weapons/";

        static readonly Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
        static readonly Dictionary<Material, Material> urpMaterials = new Dictionary<Material, Material>();

        public static Transform Create(
            string assetName,
            Transform parent,
            float targetSize,
            Quaternion localRotation,
            Vector3 targetCenter)
        {
            GameObject prefab = Load(assetName);
            if (prefab == null)
                return null;

            GameObject visual = Object.Instantiate(prefab, parent, false);
            visual.name = assetName;
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = localRotation;
            visual.transform.localScale = Vector3.one;

            RemoveGameplayComponents(visual);
            EnsureUrpMaterials(visual);
            FitToSize(visual.transform, targetSize, targetCenter);
            return visual.transform;
        }

        static GameObject Load(string assetName)
        {
            if (prefabs.TryGetValue(assetName, out GameObject cached))
                return cached;

            GameObject prefab = Resources.Load<GameObject>(ResourceRoot + assetName);
            prefabs[assetName] = prefab;

            if (prefab == null)
                Debug.LogWarning($"Weapon visual not found at Resources/{ResourceRoot}{assetName}.");

            return prefab;
        }

        static void RemoveGameplayComponents(GameObject visual)
        {
            foreach (Collider collider in visual.GetComponentsInChildren<Collider>(true))
            {
                collider.enabled = false;
                Object.Destroy(collider);
            }

            foreach (Rigidbody body in visual.GetComponentsInChildren<Rigidbody>(true))
            {
                body.isKinematic = true;
                Object.Destroy(body);
            }

            foreach (Animator animator in visual.GetComponentsInChildren<Animator>(true))
            {
                animator.enabled = false;
                Object.Destroy(animator);
            }
        }

        static void FitToSize(Transform visual, float targetSize, Vector3 targetCenter)
        {
            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
                return;

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            float largestDimension = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            if (largestDimension > 0.0001f)
                visual.localScale *= Mathf.Max(0.01f, targetSize) / largestDimension;

            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            Vector3 localCenter = visual.parent.InverseTransformPoint(bounds.center);
            visual.localPosition += targetCenter - localCenter;
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
                        Texture texture = source.HasProperty("_MainTex")
                            ? source.GetTexture("_MainTex")
                            : source.mainTexture;
                        Color color = source.HasProperty("_Color") ? source.color : Color.white;

                        converted = new Material(urpShader)
                        {
                            name = source.name + "_URP_Runtime",
                            color = color,
                            mainTexture = texture
                        };
                        if (converted.HasProperty("_BaseMap"))
                            converted.SetTexture("_BaseMap", texture);
                        if (converted.HasProperty("_BaseColor"))
                            converted.SetColor("_BaseColor", color);

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
