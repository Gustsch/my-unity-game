using KnightRun.World;
using UnityEngine;

namespace KnightRun.Player
{
    public class BoomerangVisual : MonoBehaviour
    {
        Transform boomerangRoot;

        public Vector3 ThrowPosition => boomerangRoot != null
            ? boomerangRoot.position
            : transform.position + Vector3.up;

        public void Build(Transform parent)
        {
            var rootGo = new GameObject("BoomerangVisual");
            rootGo.transform.SetParent(parent, false);
            rootGo.transform.localPosition = new Vector3(-0.45f, 1.05f, 0.15f);
            boomerangRoot = rootGo.transform;

            CreateBlade(boomerangRoot, new Vector3(0.28f, 0.04f, 0.08f), Quaternion.Euler(0f, 0f, 18f));
            CreateBlade(boomerangRoot, new Vector3(0.12f, 0.04f, 0.2f), Quaternion.Euler(0f, 72f, 0f));

            SetVisible(false);
        }

        public void SetVisible(bool visible)
        {
            if (boomerangRoot != null)
                boomerangRoot.gameObject.SetActive(visible);
        }

        void Update()
        {
            if (boomerangRoot != null && boomerangRoot.gameObject.activeSelf)
                boomerangRoot.Rotate(Vector3.up, 300f * Time.deltaTime, Space.Self);
        }

        static void CreateBlade(Transform parent, Vector3 scale, Quaternion rotation)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.SetParent(parent, false);
            go.transform.localScale = scale;
            go.transform.localRotation = rotation;
            go.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.LogObstacle);
            Object.Destroy(go.GetComponent<Collider>());
        }
    }
}
