using KnightRun.World;
using UnityEngine;

namespace KnightRun.Player
{
    public class ThrowingAxeVisual : MonoBehaviour
    {
        Transform axeRoot;

        public Vector3 ThrowPosition => axeRoot != null
            ? axeRoot.position
            : transform.position + Vector3.up;

        public void Build(Transform parent)
        {
            if (axeRoot != null)
            {
                Destroy(axeRoot.gameObject);
                axeRoot = null;
            }

            var rootGo = new GameObject("ThrowingAxeVisual");
            rootGo.transform.SetParent(parent, false);
            rootGo.transform.localPosition = parent != null && parent.name.Contains("Socket")
                ? Vector3.zero
                : new Vector3(0.55f, 1f, 0.1f);
            axeRoot = rootGo.transform;

            var head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.transform.SetParent(axeRoot, false);
            head.transform.localScale = new Vector3(0.22f, 0.05f, 0.14f);
            head.transform.localPosition = new Vector3(0f, 0f, 0.1f);
            head.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.KnightSword);
            Destroy(head.GetComponent<Collider>());

            var handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            handle.transform.SetParent(axeRoot, false);
            handle.transform.localScale = new Vector3(0.05f, 0.05f, 0.18f);
            handle.transform.localPosition = new Vector3(0f, 0f, -0.05f);
            handle.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.LogObstacle);
            Destroy(handle.GetComponent<Collider>());

            SetVisible(false);
        }

        public void SetVisible(bool visible)
        {
            if (axeRoot != null)
                axeRoot.gameObject.SetActive(visible);
        }
    }
}
