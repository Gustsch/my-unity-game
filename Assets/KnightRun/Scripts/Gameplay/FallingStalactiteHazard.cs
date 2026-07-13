using KnightRun.Core;
using KnightRun.Player;
using KnightRun.World;
using UnityEngine;

namespace KnightRun.Gameplay
{
    public class FallingStalactiteHazard : MonoBehaviour
    {
        public const int ImpactDamage = 500;
        public const float DespawnBehindDistance = 14f;

        const float ContactCooldown = 0.5f;
        const float FallTriggerDistance = 12f;
        const float FallDuration = 0.28f;
        const float ShatterDuration = 0.45f;
        const float HangHeight = 3.5f;
        const float ImpactY = 0.35f;
        const float MarkRadius = 0.95f;
        const float BaseMarkHeight = 0.07f;

        enum Phase
        {
            Warning,
            Falling,
            Shatter
        }

        Transform player;
        Transform warningMark;
        Transform spikeVisual;
        BoxCollider hitbox;
        GameManager gameManager;
        Phase phase = Phase.Warning;
        float phaseTimer;
        float contactCooldownTimer;
        Vector3 hangLocalPosition;
        Vector3 impactLocalPosition;
        bool damagedThisFall;

        public static FallingStalactiteHazard Spawn(Transform parent, Vector3 worldPosition)
        {
            var go = new GameObject("FallingStalactite");
            go.transform.SetParent(parent, false);
            go.transform.position = worldPosition;

            var hazard = go.AddComponent<FallingStalactiteHazard>();
            hazard.BuildVisual();
            return hazard;
        }

        void BuildVisual()
        {
            var mark = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mark.name = "WarningMark";
            mark.transform.SetParent(transform, false);
            mark.transform.localScale = new Vector3(MarkRadius * 2f, BaseMarkHeight, MarkRadius * 2f);
            mark.transform.localPosition = new Vector3(0f, BaseMarkHeight, 0f);
            mark.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.Stalactite);
            ApplyTint(mark.GetComponent<Renderer>(), new Color(0.55f, 0.85f, 1f));
            Object.Destroy(mark.GetComponent<Collider>());
            warningMark = mark.transform;

            var spike = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spike.name = "Spike";
            spike.transform.SetParent(transform, false);
            spike.transform.localScale = new Vector3(0.55f, 0.95f, 0.55f);
            hangLocalPosition = new Vector3(0f, HangHeight, 0f);
            impactLocalPosition = new Vector3(0f, ImpactY, 0f);
            spike.transform.localPosition = hangLocalPosition;
            spike.transform.localRotation = Quaternion.Euler(180f, 0f, 0f);
            spike.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.Stalactite);
            ApplyTint(spike.GetComponent<Renderer>(), new Color(0.7f, 0.88f, 1f));
            Object.Destroy(spike.GetComponent<Collider>());
            spikeVisual = spike.transform;

            hitbox = gameObject.AddComponent<BoxCollider>();
            hitbox.isTrigger = true;
            hitbox.enabled = false;
            hitbox.size = new Vector3(MarkRadius * 1.6f, 1.6f, MarkRadius * 1.6f);
            hitbox.center = new Vector3(0f, 0.8f, 0f);

            phase = Phase.Warning;
        }

        static void ApplyTint(Renderer renderer, Color color)
        {
            if (renderer == null)
                return;

            Material material = renderer.material;
            material.color = color;
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
        }

        void Start()
        {
            gameManager = GameManager.Instance;
            ResolvePlayer();
        }

        void Update()
        {
            if (gameManager == null || gameManager.State != GameState.Running)
                return;

            if (player == null)
            {
                ResolvePlayer();
                return;
            }

            if (contactCooldownTimer > 0f)
                contactCooldownTimer -= Time.deltaTime;

            UpdatePhase();
            AnimateWarning();

            if (transform.position.z < player.position.z - DespawnBehindDistance)
                Destroy(gameObject);
        }

        void UpdatePhase()
        {
            switch (phase)
            {
                case Phase.Warning:
                    float aheadDistance = transform.position.z - player.position.z;
                    if (aheadDistance <= FallTriggerDistance && aheadDistance >= -1f)
                        BeginFall();
                    break;

                case Phase.Falling:
                    phaseTimer -= Time.deltaTime;
                    float t = 1f - Mathf.Clamp01(phaseTimer / FallDuration);
                    float eased = t * t;
                    if (spikeVisual != null)
                        spikeVisual.localPosition = Vector3.Lerp(hangLocalPosition, impactLocalPosition, eased);

                    if (phaseTimer <= 0f)
                        BeginShatter();
                    break;

                case Phase.Shatter:
                    phaseTimer -= Time.deltaTime;
                    if (spikeVisual != null)
                    {
                        float fade = Mathf.Clamp01(phaseTimer / ShatterDuration);
                        spikeVisual.localScale = new Vector3(0.55f, 0.95f, 0.55f) * Mathf.Lerp(0.3f, 1f, fade);
                    }

                    if (hitbox != null)
                        hitbox.enabled = phaseTimer > ShatterDuration * 0.35f;

                    if (phaseTimer <= 0f)
                        Destroy(gameObject);
                    break;
            }
        }

        void BeginFall()
        {
            phase = Phase.Falling;
            phaseTimer = FallDuration;
            damagedThisFall = false;
            if (hitbox != null)
                hitbox.enabled = true;
        }

        void BeginShatter()
        {
            phase = Phase.Shatter;
            phaseTimer = ShatterDuration;
            if (spikeVisual != null)
                spikeVisual.localPosition = impactLocalPosition;
        }

        void AnimateWarning()
        {
            if (warningMark == null)
                return;

            if (phase != Phase.Warning)
            {
                warningMark.gameObject.SetActive(phase == Phase.Falling);
                return;
            }

            float pulse = 1f + Mathf.Sin(Time.time * 9f) * 0.2f;
            warningMark.localScale = new Vector3(MarkRadius * 2f * pulse, BaseMarkHeight, MarkRadius * 2f * pulse);

            if (spikeVisual != null)
            {
                float shake = Mathf.Sin(Time.time * 22f) * 0.04f;
                spikeVisual.localPosition = hangLocalPosition + new Vector3(shake, 0f, -shake * 0.5f);
            }

            var renderer = warningMark.GetComponent<Renderer>();
            if (renderer == null)
                return;

            float glow = 0.55f + Mathf.Sin(Time.time * 11f) * 0.35f;
            Color color = new Color(0.45f + glow * 0.25f, 0.8f, 1f);
            ApplyTint(renderer, color);
        }

        void OnTriggerEnter(Collider other)
        {
            TryHitPlayer(other);
        }

        void OnTriggerStay(Collider other)
        {
            TryHitPlayer(other);
        }

        void TryHitPlayer(Collider other)
        {
            if (phase == Phase.Warning || !other.CompareTag("Player") || contactCooldownTimer > 0f)
                return;

            if (damagedThisFall && phase == Phase.Shatter)
                return;

            KnightHealth health = other.GetComponent<KnightHealth>();
            if (health == null)
                return;

            health.TakeDamage(ImpactDamage);
            contactCooldownTimer = ContactCooldown;
            damagedThisFall = true;
        }

        void ResolvePlayer()
        {
            var runner = FindFirstObjectByType<RunnerController>();
            if (runner != null)
                player = runner.transform;
        }
    }
}
