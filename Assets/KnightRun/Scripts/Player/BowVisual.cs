using KnightRun.World;
using UnityEngine;

namespace KnightRun.Player
{
    public class BowVisual : MonoBehaviour
    {
        Transform bowRoot;
        Transform upperLimb;
        Transform lowerLimb;
        Transform grip;
        Transform arrowSpawn;

        Vector3 upperLimbBaseScale;
        Vector3 lowerLimbBaseScale;
        Vector3 gripBaseScale;

        public Vector3 ArrowSpawnPosition => arrowSpawn != null
            ? arrowSpawn.position
            : transform.position + Vector3.forward * 1.2f + Vector3.up * 1f;

        public void Build(Transform parent)
        {
            var rootGo = new GameObject("BowVisual");
            rootGo.transform.SetParent(parent, false);
            rootGo.transform.localPosition = new Vector3(-0.55f, 0.95f, 0.05f);
            rootGo.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
            bowRoot = rootGo.transform;

            upperLimb = CreatePart(bowRoot, new Vector3(0.06f, 0.35f, 0.06f), new Vector3(0f, 0.22f, 0f), "UpperLimb");
            lowerLimb = CreatePart(bowRoot, new Vector3(0.06f, 0.35f, 0.06f), new Vector3(0f, -0.22f, 0f), "LowerLimb");
            grip = CreatePart(bowRoot, new Vector3(0.05f, 0.05f, 0.28f), new Vector3(0f, 0f, 0f), "Grip");
            CreatePart(bowRoot, new Vector3(0.02f, 0.02f, 0.5f), new Vector3(0f, 0f, 0.02f), "String");

            upperLimbBaseScale = upperLimb.localScale;
            lowerLimbBaseScale = lowerLimb.localScale;
            gripBaseScale = grip.localScale;

            var spawnGo = new GameObject("ArrowSpawn");
            spawnGo.transform.SetParent(bowRoot, false);
            spawnGo.transform.localPosition = new Vector3(0f, 0f, 0.35f);
            arrowSpawn = spawnGo.transform;

            SetVisible(false);
        }

        public void SetVisible(bool visible)
        {
            if (bowRoot != null)
                bowRoot.gameObject.SetActive(visible);
        }

        public void SetAttackAreaMultiplier(float multiplier)
        {
            multiplier = Mathf.Max(1f, multiplier);

            if (upperLimb != null)
                upperLimb.localScale = upperLimbBaseScale * multiplier;
            if (lowerLimb != null)
                lowerLimb.localScale = lowerLimbBaseScale * multiplier;
            if (grip != null)
            {
                float gripScale = 1f + (multiplier - 1f) * 0.6f;
                grip.localScale = gripBaseScale * gripScale;
            }
        }

        static Transform CreatePart(Transform parent, Vector3 scale, Vector3 localPosition, string name)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localScale = scale;
            go.transform.localPosition = localPosition;
            go.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.LogObstacle);
            Object.Destroy(go.GetComponent<Collider>());
            return go.transform;
        }
    }
}
