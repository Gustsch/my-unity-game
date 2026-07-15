using KnightRun.World;
using UnityEngine;

namespace KnightRun.Player
{
    public class BowVisual : MonoBehaviour
    {
        Transform bowRoot;
        Transform modelRoot;
        Transform arrowSpawn;

        Vector3 modelBaseScale;

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

            modelRoot = WeaponAssetVisual.Create(
                "Bow",
                bowRoot,
                0.85f,
                Quaternion.identity,
                Vector3.zero);
            if (modelRoot != null)
                modelBaseScale = modelRoot.localScale;

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

            if (modelRoot != null)
                modelRoot.localScale = modelBaseScale * multiplier;
        }
    }
}
