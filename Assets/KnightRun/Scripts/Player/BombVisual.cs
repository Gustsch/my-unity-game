using KnightRun.World;
using UnityEngine;

namespace KnightRun.Player
{
    public class BombVisual : MonoBehaviour
    {
        Transform bombRoot;

        public Vector3 ThrowPosition => bombRoot != null
            ? bombRoot.position
            : transform.position + Vector3.up * 1.1f + Vector3.forward * 0.3f;

        public void Build(Transform parent)
        {
            var rootGo = new GameObject("BombVisual");
            rootGo.transform.SetParent(parent, false);
            rootGo.transform.localPosition = new Vector3(-0.35f, 0.75f, 0.25f);
            bombRoot = rootGo.transform;

            var body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            body.name = "BombBody";
            body.transform.SetParent(bombRoot, false);
            body.transform.localScale = Vector3.one * 0.22f;
            body.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.VolcanoRock);
            Object.Destroy(body.GetComponent<Collider>());

            var fuse = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            fuse.name = "BombFuse";
            fuse.transform.SetParent(bombRoot, false);
            fuse.transform.localScale = new Vector3(0.08f, 0.12f, 0.08f);
            fuse.transform.localPosition = new Vector3(0f, 0.2f, 0f);
            fuse.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.Coin);
            Object.Destroy(fuse.GetComponent<Collider>());

            SetVisible(false);
        }

        public void SetVisible(bool visible)
        {
            if (bombRoot != null)
                bombRoot.gameObject.SetActive(visible);
        }
    }
}
