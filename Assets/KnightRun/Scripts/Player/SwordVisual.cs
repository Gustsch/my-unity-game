using KnightRun.World;
using UnityEngine;

namespace KnightRun.Player
{
    public class SwordVisual : MonoBehaviour
    {
        Transform swordPivot;
        Transform blade;
        Transform guard;
        Vector3 bladeBaseScale;
        Vector3 guardBaseScale;

        public Transform Pivot => swordPivot;

        public void Build(Transform parent)
        {
            var pivotGo = new GameObject("SwordPivot");
            pivotGo.transform.SetParent(parent, false);
            pivotGo.transform.localPosition = new Vector3(0f, 0.9f, 0.15f);
            pivotGo.transform.localRotation = Quaternion.Euler(-20f, 70f, 0f);
            swordPivot = pivotGo.transform;

            CreatePart(PrimitiveType.Cube, swordPivot, new Vector3(0.07f, 0.07f, 0.3f), new Vector3(0f, 0f, -0.12f), "Handle");
            guard = CreatePart(PrimitiveType.Cube, swordPivot, new Vector3(0.22f, 0.05f, 0.05f), new Vector3(0f, 0f, 0.06f), "Guard");
            blade = CreatePart(PrimitiveType.Cube, swordPivot, new Vector3(0.1f, 0.03f, 0.85f), new Vector3(0f, 0f, 0.52f), "Blade");

            guardBaseScale = guard.localScale;
            bladeBaseScale = blade.localScale;

            SetVisible(false);
        }

        public void SetVisible(bool visible)
        {
            if (swordPivot != null)
                swordPivot.gameObject.SetActive(visible);
        }

        public void SetAttackAreaMultiplier(float multiplier)
        {
            multiplier = Mathf.Max(1f, multiplier);

            if (blade != null)
                blade.localScale = bladeBaseScale * multiplier;

            if (guard != null)
            {
                float guardScale = 1f + (multiplier - 1f) * 0.6f;
                guard.localScale = guardBaseScale * guardScale;
            }
        }

        static Transform CreatePart(PrimitiveType type, Transform parent, Vector3 scale, Vector3 localPosition, string name)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localScale = scale;
            go.transform.localPosition = localPosition;
            go.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.KnightSword);
            Object.Destroy(go.GetComponent<Collider>());
            return go.transform;
        }
    }
}
