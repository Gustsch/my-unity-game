using KnightRun.World;
using UnityEngine;

namespace KnightRun.Player
{
    public class ShurikenVisual : MonoBehaviour
    {
        Transform shurikenRoot;
        Transform bladeA;
        Transform bladeB;

        public Vector3 ThrowPosition => shurikenRoot != null
            ? shurikenRoot.position
            : transform.position + Vector3.up * 1f;

        public void Build(Transform parent)
        {
            var rootGo = new GameObject("ShurikenVisual");
            rootGo.transform.SetParent(parent, false);
            rootGo.transform.localPosition = new Vector3(0.45f, 1.05f, 0.2f);
            shurikenRoot = rootGo.transform;

            bladeA = CreateBlade(shurikenRoot, new Vector3(0.22f, 0.03f, 0.05f), Quaternion.Euler(0f, 0f, 45f), "BladeA");
            bladeB = CreateBlade(shurikenRoot, new Vector3(0.22f, 0.03f, 0.05f), Quaternion.Euler(0f, 0f, -45f), "BladeB");
            CreateBlade(shurikenRoot, new Vector3(0.05f, 0.03f, 0.22f), Quaternion.identity, "BladeC");
            CreateBlade(shurikenRoot, new Vector3(0.05f, 0.03f, 0.22f), Quaternion.Euler(0f, 90f, 0f), "BladeD");
            CreateBlade(shurikenRoot, new Vector3(0.06f, 0.06f, 0.06f), Quaternion.identity, "Core");

            SetVisible(false);
        }

        public void SetVisible(bool visible)
        {
            if (shurikenRoot != null)
                shurikenRoot.gameObject.SetActive(visible);
        }

        public void SetAttackAreaMultiplier(float multiplier)
        {
            multiplier = Mathf.Max(1f, multiplier);

            if (bladeA != null)
                bladeA.localScale = new Vector3(0.22f, 0.03f, 0.05f) * multiplier;
            if (bladeB != null)
                bladeB.localScale = new Vector3(0.22f, 0.03f, 0.05f) * multiplier;
        }

        void Update()
        {
            if (shurikenRoot != null && shurikenRoot.gameObject.activeSelf)
                shurikenRoot.Rotate(Vector3.up, 360f * Time.deltaTime, Space.Self);
        }

        static Transform CreateBlade(Transform parent, Vector3 scale, Quaternion rotation, string name)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localScale = scale;
            go.transform.localRotation = rotation;
            go.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.KnightSword);
            Object.Destroy(go.GetComponent<Collider>());
            return go.transform;
        }
    }
}
