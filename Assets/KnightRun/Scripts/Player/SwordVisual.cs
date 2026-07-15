using KnightRun.World;
using UnityEngine;

namespace KnightRun.Player
{
    public class SwordVisual : MonoBehaviour
    {
        Transform swordPivot;
        Transform modelRoot;
        Vector3 modelBaseScale;

        public Transform Pivot => swordPivot;

        public void Build(Transform parent)
        {
            var pivotGo = new GameObject("SwordPivot");
            pivotGo.transform.SetParent(parent, false);
            pivotGo.transform.localPosition = new Vector3(0f, 0.9f, 0.15f);
            pivotGo.transform.localRotation = Quaternion.Euler(-20f, 70f, 0f);
            swordPivot = pivotGo.transform;

            modelRoot = WeaponAssetVisual.Create(
                "Sword",
                swordPivot,
                1.05f,
                Quaternion.Euler(90f, 0f, 0f),
                new Vector3(0f, 0f, 0.4f));
            if (modelRoot != null)
                modelBaseScale = modelRoot.localScale;

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

            if (modelRoot != null)
                modelRoot.localScale = modelBaseScale * multiplier;
        }
    }
}
