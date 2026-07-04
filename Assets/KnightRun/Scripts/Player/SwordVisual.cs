using KnightRun.World;
using UnityEngine;

namespace KnightRun.Player
{
    public class SwordVisual : MonoBehaviour
    {
        Transform swordPivot;

        public Transform Pivot => swordPivot;

        public void Build(Transform parent)
        {
            var pivotGo = new GameObject("SwordPivot");
            pivotGo.transform.SetParent(parent, false);
            pivotGo.transform.localPosition = new Vector3(0f, 0.9f, 0.15f);
            pivotGo.transform.localRotation = Quaternion.Euler(-20f, 70f, 0f);
            swordPivot = pivotGo.transform;

            CreatePart(PrimitiveType.Cube, swordPivot, new Vector3(0.07f, 0.07f, 0.3f), new Vector3(0f, 0f, -0.12f), "Handle");
            CreatePart(PrimitiveType.Cube, swordPivot, new Vector3(0.22f, 0.05f, 0.05f), new Vector3(0f, 0f, 0.06f), "Guard");
            CreatePart(PrimitiveType.Cube, swordPivot, new Vector3(0.1f, 0.03f, 0.85f), new Vector3(0f, 0f, 0.52f), "Blade");
        }

        static void CreatePart(PrimitiveType type, Transform parent, Vector3 scale, Vector3 localPosition, string name)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localScale = scale;
            go.transform.localPosition = localPosition;
            go.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.KnightSword);
            Object.Destroy(go.GetComponent<Collider>());
        }
    }
}
