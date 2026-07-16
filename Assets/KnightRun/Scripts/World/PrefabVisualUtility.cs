using UnityEngine;

namespace KnightRun.World
{
    public static class PrefabVisualUtility
    {
        public static GameObject InstantiateVisual(GameObject prefab, Transform parent)
        {
            return SimpleNatureCatalog.InstantiateVisual(prefab, parent);
        }

        public static bool TryGetBounds(GameObject visual, out Bounds bounds)
        {
            bounds = default;
            if (visual == null)
                return false;

            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
            bool found = false;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                    continue;

                if (!found)
                {
                    bounds = renderer.bounds;
                    found = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            return found;
        }

        public static void FitHeight(GameObject visual, float targetHeight, float maxWidth = 0f)
        {
            if (visual == null)
                return;

            if (!TryGetBounds(visual, out Bounds bounds) || bounds.size.y < 0.01f)
            {
                visual.transform.localScale = Vector3.one * Mathf.Max(0.01f, targetHeight);
                return;
            }

            float scale = targetHeight / bounds.size.y;
            visual.transform.localScale = Vector3.one * scale;

            // Huge meshes (caves/mountains) can still swallow a narrow track after height fit.
            if (maxWidth > 0.1f && TryGetBounds(visual, out Bounds scaled) && scaled.size.x > maxWidth)
            {
                float widthScale = maxWidth / scaled.size.x;
                visual.transform.localScale *= widthScale;
            }

            SnapToGround(visual);
        }

        public static void FitCoverXZ(GameObject visual, float targetWidth, float targetDepth, float targetHeight = 0.28f)
        {
            if (visual == null)
                return;

            if (!TryGetBounds(visual, out Bounds bounds))
            {
                visual.transform.localScale = new Vector3(
                    Mathf.Max(0.01f, targetWidth),
                    Mathf.Max(0.01f, targetHeight),
                    Mathf.Max(0.01f, targetDepth));
                return;
            }

            // Scale X/Z independently so flat platforms don't become tall walls.
            float sx = bounds.size.x > 0.01f ? targetWidth / bounds.size.x : 1f;
            float sz = bounds.size.z > 0.01f ? targetDepth / bounds.size.z : 1f;
            float sy = bounds.size.y > 0.01f
                ? Mathf.Clamp(targetHeight / bounds.size.y, 0.05f, 3f)
                : 1f;

            visual.transform.localScale = new Vector3(sx, sy, sz);
            SnapToGround(visual);
        }

        public static void SnapToGround(GameObject visual)
        {
            if (visual == null || !TryGetBounds(visual, out Bounds bounds))
                return;

            Vector3 pos = visual.transform.position;
            float bottom = bounds.min.y;
            pos.y += -bottom;
            visual.transform.position = pos;
        }
    }
}
