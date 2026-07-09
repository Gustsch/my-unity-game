using KnightRun.World;
using UnityEngine;

namespace KnightRun.Player
{
    public class KnightSlideVisual : MonoBehaviour
    {
        Transform helmet;
        Transform swordPivot;
        Renderer bodyRenderer;
        GameObject slideBody;
        KnightSwordAttack swordAttack;

        Vector3 helmetStandPosition;
        Vector3 helmetStandScale;
        Quaternion helmetStandRotation;

        static readonly Vector3 HelmetSlidePosition = new Vector3(0f, 0.55f, 0.22f);
        static readonly Vector3 HelmetSlideScale = new Vector3(0.85f, 0.25f, 0.85f);
        static readonly Quaternion HelmetSlideRotation = Quaternion.Euler(18f, 0f, 0f);

        static readonly Vector3 SwordStandRotation = new Vector3(-20f, 70f, 0f);
        static readonly Vector3 SwordSlideRotation = new Vector3(78f, 70f, 0f);

        KnightSwordAttack SwordAttack
        {
            get
            {
                if (swordAttack == null)
                    swordAttack = GetComponent<KnightSwordAttack>();
                return swordAttack;
            }
        }

        public void Build(Transform helmetTransform, Transform swordPivotTransform, Renderer capsuleRenderer)
        {
            helmet = helmetTransform;
            swordPivot = swordPivotTransform;
            bodyRenderer = capsuleRenderer;

            helmetStandPosition = helmet.localPosition;
            helmetStandScale = helmet.localScale;
            helmetStandRotation = helmet.localRotation;

            slideBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slideBody.name = "SlideBody";
            slideBody.transform.SetParent(transform, false);
            slideBody.transform.localScale = new Vector3(0.85f, 0.45f, 1.1f);
            slideBody.transform.localPosition = new Vector3(0f, 0.28f, 0.05f);
            slideBody.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.KnightArmor);
            Object.Destroy(slideBody.GetComponent<Collider>());
            slideBody.SetActive(false);
        }

        public void SetSlideAmount(float amount)
        {
            amount = Mathf.Clamp01(amount);

            if (amount <= 0.001f)
            {
                if (bodyRenderer != null)
                    bodyRenderer.enabled = true;
                if (slideBody != null)
                    slideBody.SetActive(false);
                if (helmet != null)
                {
                    helmet.localPosition = helmetStandPosition;
                    helmet.localScale = helmetStandScale;
                    helmet.localRotation = helmetStandRotation;
                }
                if (swordPivot != null && (SwordAttack == null || !SwordAttack.IsSwinging))
                    swordPivot.localRotation = Quaternion.Euler(SwordStandRotation);
                return;
            }

            if (bodyRenderer != null)
                bodyRenderer.enabled = false;
            if (slideBody != null)
                slideBody.SetActive(true);

            if (helmet != null)
            {
                helmet.localPosition = Vector3.Lerp(helmetStandPosition, HelmetSlidePosition, amount);
                helmet.localScale = Vector3.Lerp(helmetStandScale, HelmetSlideScale, amount);
                helmet.localRotation = Quaternion.Slerp(helmetStandRotation, HelmetSlideRotation, amount);
            }

            if (swordPivot != null && (SwordAttack == null || !SwordAttack.IsSwinging))
                swordPivot.localRotation = Quaternion.Euler(Vector3.Lerp(SwordStandRotation, SwordSlideRotation, amount));
        }
    }
}
