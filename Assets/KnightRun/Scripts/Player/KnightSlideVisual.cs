using KnightRun.World;
using UnityEngine;

namespace KnightRun.Player
{
    public class KnightSlideVisual : MonoBehaviour
    {
        Transform helmet;
        Transform swordPivot;
        Transform modularVisualRoot;
        Renderer bodyRenderer;
        GameObject slideBody;
        KnightSwordAttack swordAttack;
        PlayerAnimationDriver animationDriver;
        bool usingLegacyVisual;

        Vector3 helmetStandPosition;
        Vector3 helmetStandScale;
        Quaternion helmetStandRotation;
        Vector3 modularStandPosition;
        Quaternion modularStandRotation;

        static readonly Vector3 HelmetSlidePosition = new Vector3(0f, 0.55f, 0.22f);
        static readonly Vector3 HelmetSlideScale = new Vector3(0.85f, 0.25f, 0.85f);
        static readonly Quaternion HelmetSlideRotation = Quaternion.Euler(18f, 0f, 0f);

        static readonly Vector3 SwordStandRotation = new Vector3(-20f, 70f, 0f);
        static readonly Vector3 SwordSlideRotation = new Vector3(78f, 70f, 0f);
        static readonly Vector3 ModularSlideEuler = new Vector3(70f, 0f, 0f);
        static readonly Vector3 ModularSlideOffset = new Vector3(0f, -0.35f, 0.25f);

        KnightSwordAttack SwordAttack
        {
            get
            {
                if (swordAttack == null)
                    swordAttack = GetComponent<KnightSwordAttack>();
                return swordAttack;
            }
        }

        public void BuildLegacy(Transform helmetTransform, Transform swordPivotTransform, Renderer capsuleRenderer)
        {
            usingLegacyVisual = true;
            modularVisualRoot = null;
            animationDriver = GetComponent<PlayerAnimationDriver>();
            helmet = helmetTransform;
            swordPivot = swordPivotTransform;
            bodyRenderer = capsuleRenderer;

            if (helmet != null)
            {
                helmetStandPosition = helmet.localPosition;
                helmetStandScale = helmet.localScale;
                helmetStandRotation = helmet.localRotation;
            }

            EnsureSlideBody();
        }

        public void BuildModular(Transform visualRoot, Transform swordPivotTransform)
        {
            usingLegacyVisual = false;
            modularVisualRoot = visualRoot;
            swordPivot = swordPivotTransform;
            bodyRenderer = null;
            helmet = null;
            animationDriver = GetComponent<PlayerAnimationDriver>();

            if (slideBody != null)
                slideBody.SetActive(false);

            if (modularVisualRoot != null)
            {
                modularStandPosition = modularVisualRoot.localPosition;
                modularStandRotation = modularVisualRoot.localRotation;
            }
        }

        /// <summary>Legacy API used by older call sites.</summary>
        public void Build(Transform helmetTransform, Transform swordPivotTransform, Renderer capsuleRenderer)
        {
            BuildLegacy(helmetTransform, swordPivotTransform, capsuleRenderer);
        }

        public void SetSwordPivot(Transform swordPivotTransform)
        {
            swordPivot = swordPivotTransform;
        }

        public void SetSlideAmount(float amount)
        {
            amount = Mathf.Clamp01(amount);
            animationDriver ??= GetComponent<PlayerAnimationDriver>();
            animationDriver?.SetSliding(amount > 0.001f);

            if (!usingLegacyVisual)
            {
                ApplyModularSlide(amount);
                return;
            }

            ApplyLegacySlide(amount);
        }

        void ApplyModularSlide(float amount)
        {
            if (modularVisualRoot == null)
                return;

            // When the Blink RollForward animation is driving the pose, keep VisualRoot upright
            // (strafe facing is also suppressed by RunnerController while sliding).
            animationDriver ??= GetComponent<PlayerAnimationDriver>();
            if (animationDriver != null && animationDriver.HasBoundController)
            {
                return;
            }

            if (amount <= 0.001f)
            {
                modularVisualRoot.localPosition = modularStandPosition;
                modularVisualRoot.localRotation = modularStandRotation;
                if (swordPivot != null && (SwordAttack == null || !SwordAttack.IsSwinging))
                    swordPivot.localRotation = Quaternion.Euler(SwordStandRotation);
                return;
            }

            modularVisualRoot.localPosition = Vector3.Lerp(
                modularStandPosition,
                modularStandPosition + ModularSlideOffset,
                amount);
            modularVisualRoot.localRotation = Quaternion.Slerp(
                modularStandRotation,
                Quaternion.Euler(ModularSlideEuler),
                amount);

            if (swordPivot != null && (SwordAttack == null || !SwordAttack.IsSwinging))
                swordPivot.localRotation = Quaternion.Euler(Vector3.Lerp(SwordStandRotation, SwordSlideRotation, amount));
        }

        void ApplyLegacySlide(float amount)
        {
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

        void EnsureSlideBody()
        {
            if (slideBody != null)
                return;

            slideBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slideBody.name = "SlideBody";
            slideBody.transform.SetParent(transform, false);
            slideBody.transform.localScale = new Vector3(0.85f, 0.45f, 1.1f);
            slideBody.transform.localPosition = new Vector3(0f, 0.28f, 0.05f);
            slideBody.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.KnightArmor);
            Destroy(slideBody.GetComponent<Collider>());
            slideBody.SetActive(false);
        }
    }
}
