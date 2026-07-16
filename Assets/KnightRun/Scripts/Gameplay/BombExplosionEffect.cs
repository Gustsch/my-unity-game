using UnityEngine;

namespace KnightRun.Gameplay
{
    /// <summary>
    /// Procedural blast that expands to the bomb's damage radius so the hitbox is readable.
    /// </summary>
    public sealed class BombExplosionEffect : MonoBehaviour
    {
        const float Duration = 0.45f;
        const float FlashPeak = 0.18f;

        float radius;
        float elapsed;
        Transform shockwave;
        Transform blastCore;
        Transform groundRing;
        Renderer shockwaveRenderer;
        Renderer coreRenderer;
        Renderer ringRenderer;
        Material shockwaveMaterial;
        Material coreMaterial;
        Material ringMaterial;

        public static void Play(Vector3 position, float radius)
        {
            var go = new GameObject("BombExplosion");
            go.transform.position = position;

            var effect = go.AddComponent<BombExplosionEffect>();
            effect.radius = Mathf.Max(0.5f, radius);
            effect.Build();
        }

        void Build()
        {
            // Vertical blast cylinder — matches OverlapBox horizontal footprint.
            var shockwaveGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shockwaveGo.name = "Shockwave";
            shockwaveGo.transform.SetParent(transform, false);
            Object.Destroy(shockwaveGo.GetComponent<Collider>());
            shockwave = shockwaveGo.transform;
            shockwaveRenderer = shockwaveGo.GetComponent<Renderer>();
            shockwaveMaterial = CreateBlastMaterial(new Color(1f, 0.55f, 0.12f, 0.55f));
            shockwaveRenderer.sharedMaterial = shockwaveMaterial;

            var coreGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            coreGo.name = "BlastCore";
            coreGo.transform.SetParent(transform, false);
            Object.Destroy(coreGo.GetComponent<Collider>());
            blastCore = coreGo.transform;
            coreRenderer = coreGo.GetComponent<Renderer>();
            coreMaterial = CreateBlastMaterial(new Color(1f, 0.92f, 0.45f, 0.95f));
            coreRenderer.sharedMaterial = coreMaterial;

            // Flat disc on the ground outlining the attack area.
            var ringGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ringGo.name = "GroundRing";
            ringGo.transform.SetParent(transform, false);
            Object.Destroy(ringGo.GetComponent<Collider>());
            groundRing = ringGo.transform;
            ringRenderer = ringGo.GetComponent<Renderer>();
            ringMaterial = CreateBlastMaterial(new Color(1f, 0.35f, 0.05f, 0.7f));
            ringRenderer.sharedMaterial = ringMaterial;

            ApplyFrame(0f);
        }

        static Material CreateBlastMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Unlit/Color")
                ?? Shader.Find("Universal Render Pipeline/Lit");

            var material = new Material(shader != null ? shader : Shader.Find("Sprites/Default"))
            {
                name = "BombExplosion_Runtime"
            };

            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color"))
                material.SetColor("_Color", color);

            // Best-effort transparent fade on URP Unlit.
            if (material.HasProperty("_Surface"))
                material.SetFloat("_Surface", 1f);
            if (material.HasProperty("_Blend"))
                material.SetFloat("_Blend", 0f);
            if (material.HasProperty("_SrcBlend"))
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            if (material.HasProperty("_DstBlend"))
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            if (material.HasProperty("_ZWrite"))
                material.SetFloat("_ZWrite", 0f);

            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = 3000;
            return material;
        }

        void Update()
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / Duration);
            ApplyFrame(progress);

            if (progress >= 1f)
                Destroy(gameObject);
        }

        void ApplyFrame(float progress)
        {
            // Fast expand to full radius, then soft settle.
            float expand = progress < 0.35f
                ? Mathf.SmoothStep(0f, 1f, progress / 0.35f)
                : 1f;
            float fade = progress < FlashPeak
                ? Mathf.Lerp(0.35f, 1f, progress / FlashPeak)
                : 1f - Mathf.SmoothStep(0f, 1f, (progress - FlashPeak) / (1f - FlashPeak));

            float currentRadius = Mathf.Lerp(0.15f, radius, expand);
            float height = Mathf.Lerp(0.4f, 2.4f, expand) * Mathf.Lerp(1f, 0.55f, progress);

            if (shockwave != null)
            {
                shockwave.localScale = new Vector3(currentRadius * 2f, height * 0.5f, currentRadius * 2f);
                shockwave.localPosition = new Vector3(0f, height * 0.5f, 0f);
                SetAlpha(shockwaveMaterial, fade * 0.45f);
            }

            if (blastCore != null)
            {
                float coreSize = Mathf.Lerp(0.2f, radius * 0.85f, expand) * Mathf.Lerp(1f, 0.2f, progress);
                blastCore.localScale = Vector3.one * coreSize;
                blastCore.localPosition = new Vector3(0f, Mathf.Lerp(0.35f, 1.1f, expand), 0f);
                SetAlpha(coreMaterial, fade);
            }

            if (groundRing != null)
            {
                groundRing.localScale = new Vector3(currentRadius * 2f, 0.04f, currentRadius * 2f);
                groundRing.localPosition = new Vector3(0f, 0.06f, 0f);
                SetAlpha(ringMaterial, fade * 0.75f);
            }
        }

        static void SetAlpha(Material material, float alpha)
        {
            if (material == null)
                return;

            Color color = material.HasProperty("_BaseColor")
                ? material.GetColor("_BaseColor")
                : material.color;
            color.a = Mathf.Clamp01(alpha);
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
            material.color = color;
        }

        void OnDestroy()
        {
            if (shockwaveMaterial != null)
                Destroy(shockwaveMaterial);
            if (coreMaterial != null)
                Destroy(coreMaterial);
            if (ringMaterial != null)
                Destroy(ringMaterial);
        }
    }
}
