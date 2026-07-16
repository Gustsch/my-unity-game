using UnityEngine;

namespace KnightRun.Player
{
    /// <summary>
    /// Forwards gameplay motion state to the Humanoid PlayerRunner controller.
    /// Parameters: Speed, Grounded, Sliding, Stumbling, InMineCart, VerticalVelocity.
    /// </summary>
    public class PlayerAnimationDriver : MonoBehaviour
    {
        public const string ControllerResourcePath = "KnightRun/Animations/PlayerRunner";

        static readonly int SpeedHash = Animator.StringToHash("Speed");
        static readonly int GroundedHash = Animator.StringToHash("Grounded");
        static readonly int SlidingHash = Animator.StringToHash("Sliding");
        static readonly int StumblingHash = Animator.StringToHash("Stumbling");
        static readonly int InMineCartHash = Animator.StringToHash("InMineCart");
        static readonly int VerticalVelocityHash = Animator.StringToHash("VerticalVelocity");

        static RuntimeAnimatorController cachedController;

        Animator animator;
        bool hasSpeed;
        bool hasGrounded;
        bool hasSliding;
        bool hasStumbling;
        bool hasInMineCart;
        bool hasVerticalVelocity;

        public Animator BoundAnimator => animator;
        public bool HasBoundController => animator != null && animator.runtimeAnimatorController != null;

        public void Bind(Animator nextAnimator)
        {
            animator = nextAnimator;
            if (animator != null)
            {
                animator.applyRootMotion = false;
                EnsureController(animator);
            }

            CacheParameterPresence();

            SetGrounded(true);
            SetSliding(false);
            SetStumbling(false);
            SetInMineCart(false);
            SetSpeed(1f);
            SetVerticalVelocity(0f);
        }

        public void Unbind()
        {
            animator = null;
            hasSpeed = false;
            hasGrounded = false;
            hasSliding = false;
            hasStumbling = false;
            hasInMineCart = false;
            hasVerticalVelocity = false;
        }

        public void SetSpeed(float speed)
        {
            if (hasSpeed)
                animator.SetFloat(SpeedHash, speed);
        }

        public void SetGrounded(bool grounded)
        {
            if (hasGrounded)
                animator.SetBool(GroundedHash, grounded);
        }

        public void SetSliding(bool sliding)
        {
            if (hasSliding)
                animator.SetBool(SlidingHash, sliding);
        }

        public void SetStumbling(bool stumbling)
        {
            if (hasStumbling)
                animator.SetBool(StumblingHash, stumbling);
        }

        public void SetInMineCart(bool inMineCart)
        {
            if (hasInMineCart)
                animator.SetBool(InMineCartHash, inMineCart);
        }

        public void SetVerticalVelocity(float verticalVelocity)
        {
            if (hasVerticalVelocity)
                animator.SetFloat(VerticalVelocityHash, verticalVelocity);
        }

        public static RuntimeAnimatorController LoadController()
        {
            if (cachedController == null)
                cachedController = Resources.Load<RuntimeAnimatorController>(ControllerResourcePath);
            return cachedController;
        }

        static void EnsureController(Animator target)
        {
            RuntimeAnimatorController controller = LoadController();
            if (controller == null)
                return;

            // Always use the shared PlayerRunner controller so rebuilds apply to all characters.
            if (target.runtimeAnimatorController != controller)
                target.runtimeAnimatorController = controller;
        }

        void CacheParameterPresence()
        {
            hasSpeed = false;
            hasGrounded = false;
            hasSliding = false;
            hasStumbling = false;
            hasInMineCart = false;
            hasVerticalVelocity = false;

            if (animator == null || animator.runtimeAnimatorController == null)
                return;

            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.nameHash == SpeedHash)
                    hasSpeed = true;
                else if (parameter.nameHash == GroundedHash)
                    hasGrounded = true;
                else if (parameter.nameHash == SlidingHash)
                    hasSliding = true;
                else if (parameter.nameHash == StumblingHash)
                    hasStumbling = true;
                else if (parameter.nameHash == InMineCartHash)
                    hasInMineCart = true;
                else if (parameter.nameHash == VerticalVelocityHash)
                    hasVerticalVelocity = true;
            }
        }
    }
}
