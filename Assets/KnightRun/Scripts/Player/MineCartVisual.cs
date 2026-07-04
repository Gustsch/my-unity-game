using KnightRun.World;
using UnityEngine;

namespace KnightRun.Player
{
    public class MineCartVisual : MonoBehaviour
    {
        GameObject cartRoot;

        public void Build(Transform parent)
        {
            cartRoot = new GameObject("MineCartVisual");
            cartRoot.transform.SetParent(parent, false);
            cartRoot.transform.localPosition = new Vector3(0f, -0.35f, 0f);

            CreatePrimitive(PrimitiveType.Cube, cartRoot.transform, new Vector3(1.6f, 0.5f, 1.8f), new Vector3(0f, 0.25f, 0f),
                KnightRunTexture.MineCart, "CartBody");

            CreateWheel(cartRoot.transform, new Vector3(-0.55f, 0f, 0.55f));
            CreateWheel(cartRoot.transform, new Vector3(0.55f, 0f, 0.55f));
            CreateWheel(cartRoot.transform, new Vector3(-0.55f, 0f, -0.55f));
            CreateWheel(cartRoot.transform, new Vector3(0.55f, 0f, -0.55f));

            cartRoot.SetActive(false);
        }

        void CreateWheel(Transform parent, Vector3 localPosition)
        {
            CreatePrimitive(PrimitiveType.Cylinder, parent, new Vector3(0.35f, 0.12f, 0.35f), localPosition,
                KnightRunTexture.MineRail, "Wheel");
        }

        static GameObject CreatePrimitive(PrimitiveType type, Transform parent, Vector3 scale, Vector3 localPosition, KnightRunTexture texture, string name)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localScale = scale;
            go.transform.localPosition = localPosition;
            if (type == PrimitiveType.Cylinder)
                go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            var renderer = go.GetComponent<Renderer>();
            renderer.sharedMaterial = KnightRunMaterials.Get(texture);
            Object.Destroy(go.GetComponent<Collider>());
            return go;
        }

        public void SetActive(bool active)
        {
            if (cartRoot != null)
                cartRoot.SetActive(active);
        }
    }
}
