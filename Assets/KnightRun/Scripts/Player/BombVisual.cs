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
            if (bombRoot != null)
            {
                Destroy(bombRoot.gameObject);
                bombRoot = null;
            }

            var rootGo = new GameObject("BombVisual");
            rootGo.transform.SetParent(parent, false);
            rootGo.transform.localPosition = parent != null && parent.name.Contains("Socket")
                ? Vector3.zero
                : new Vector3(-0.35f, 0.75f, 0.25f);
            bombRoot = rootGo.transform;

            WeaponAssetVisual.Create(
                "Bomb",
                bombRoot,
                0.45f,
                Quaternion.Euler(-90f, 0f, 0f),
                Vector3.zero);

            SetVisible(false);
        }

        public void SetVisible(bool visible)
        {
            if (bombRoot != null)
                bombRoot.gameObject.SetActive(visible);
        }
    }
}
