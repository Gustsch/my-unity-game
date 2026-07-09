using KnightRun.World;
using UnityEngine;

namespace KnightRun.Player
{
    public class MagicBookVisual : MonoBehaviour
    {
        Transform bookRoot;
        Transform auraRing;
        Vector3 auraBaseScale;

        public void Build(Transform parent)
        {
            var rootGo = new GameObject("MagicBookVisual");
            rootGo.transform.SetParent(parent, false);
            rootGo.transform.localPosition = new Vector3(0.55f, 1.15f, -0.15f);
            bookRoot = rootGo.transform;

            var cover = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cover.name = "BookCover";
            cover.transform.SetParent(bookRoot, false);
            cover.transform.localScale = new Vector3(0.28f, 0.34f, 0.08f);
            cover.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.KnightArmor);
            Object.Destroy(cover.GetComponent<Collider>());

            var pages = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pages.name = "BookPages";
            pages.transform.SetParent(bookRoot, false);
            pages.transform.localScale = new Vector3(0.24f, 0.3f, 0.05f);
            pages.transform.localPosition = new Vector3(0f, 0f, 0.03f);
            pages.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.Coin);
            Object.Destroy(pages.GetComponent<Collider>());

            var auraGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            auraGo.name = "AuraRing";
            auraGo.transform.SetParent(parent, false);
            auraGo.transform.localPosition = new Vector3(0f, 0.55f, 0.35f);
            auraGo.transform.localRotation = Quaternion.identity;
            auraGo.transform.localScale = new Vector3(2.4f, 0.03f, 2.4f);
            auraGo.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.Coin);
            Object.Destroy(auraGo.GetComponent<Collider>());
            auraRing = auraGo.transform;
            auraBaseScale = auraRing.localScale;

            SetVisible(false);
        }

        public void SetVisible(bool visible)
        {
            if (bookRoot != null)
                bookRoot.gameObject.SetActive(visible);
            if (auraRing != null)
                auraRing.gameObject.SetActive(visible);
        }

        public void SetAuraRadiusMultiplier(float multiplier)
        {
            multiplier = Mathf.Max(1f, multiplier);

            if (auraRing != null)
            {
                auraRing.localScale = new Vector3(
                    auraBaseScale.x * multiplier,
                    auraBaseScale.y,
                    auraBaseScale.z * multiplier);
            }
        }

        void Update()
        {
            if (bookRoot == null || !bookRoot.gameObject.activeSelf)
                return;

            float bob = Mathf.Sin(Time.time * 3f) * 0.05f;
            bookRoot.localPosition = new Vector3(0.55f, 1.15f + bob, -0.15f);
            bookRoot.Rotate(Vector3.up, 45f * Time.deltaTime, Space.Self);
        }
    }
}
