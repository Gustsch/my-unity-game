using UnityEngine;

namespace KnightRun.Gameplay
{
    public sealed class DeathShrinkEffect : MonoBehaviour
    {
        const float MinimumDuration = 0.05f;

        Vector3 initialScale;
        float duration;
        float elapsed;

        public static void Play(GameObject target, float duration)
        {
            if (target == null)
                return;

            DeathShrinkEffect effect = target.GetComponent<DeathShrinkEffect>();
            if (effect == null)
                effect = target.AddComponent<DeathShrinkEffect>();

            effect.Begin(duration);
        }

        void Begin(float requestedDuration)
        {
            initialScale = transform.localScale;
            duration = Mathf.Max(MinimumDuration, requestedDuration);
            elapsed = 0f;

            foreach (Collider collider in GetComponentsInChildren<Collider>(true))
                collider.enabled = false;
        }

        void Update()
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float scale = 1f - Mathf.SmoothStep(0f, 1f, progress);
            transform.localScale = initialScale * scale;

            if (progress >= 1f)
                Destroy(gameObject);
        }
    }
}
