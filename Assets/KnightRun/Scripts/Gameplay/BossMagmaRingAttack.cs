using KnightRun.Core;
using KnightRun.Player;
using KnightRun.World;
using UnityEngine;

namespace KnightRun.Gameplay
{
    public class BossMagmaRingAttack : MonoBehaviour
    {
        public const float WarningDuration = 1.2f;
        public const float ActiveDuration = 1.1f;
        public const float OuterRadius = 1.85f;
        public const int GroundedDamage = 450;

        Transform player;
        Transform warningRing;
        Transform magmaVisual;
        RunnerController runner;
        GameManager gameManager;
        float warningTimer;
        float activeTimer;
        bool hasTriggered;
        bool hasHit;

        public static BossMagmaRingAttack Spawn(Vector3 worldPosition)
        {
            var go = new GameObject("BossMagmaRing");
            go.transform.position = worldPosition;

            var attack = go.AddComponent<BossMagmaRingAttack>();
            attack.BuildVisual();
            return attack;
        }

        void BuildVisual()
        {
            var mark = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mark.name = "WarningRing";
            mark.transform.SetParent(transform, false);
            mark.transform.localScale = new Vector3(OuterRadius * 2f, 0.06f, OuterRadius * 2f);
            mark.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            mark.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.LavaPool);
            ApplyTint(mark.GetComponent<Renderer>(), new Color(1f, 0.45f, 0.1f));
            Object.Destroy(mark.GetComponent<Collider>());
            warningRing = mark.transform;

            var magma = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            magma.name = "MagmaBurst";
            magma.transform.SetParent(transform, false);
            magma.transform.localScale = new Vector3(OuterRadius * 1.9f, 0.01f, OuterRadius * 1.9f);
            magma.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            magma.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.LavaPool);
            ApplyTint(magma.GetComponent<Renderer>(), new Color(1f, 0.28f, 0.05f));
            Object.Destroy(magma.GetComponent<Collider>());
            magmaVisual = magma.transform;
            magmaVisual.gameObject.SetActive(false);

            warningTimer = WarningDuration;
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

            if (!hasTriggered)
            {
                AnimateWarning();
                warningTimer -= Time.deltaTime;
                if (warningTimer <= 0f)
                    TriggerBurst();
                return;
            }

            activeTimer -= Time.deltaTime;
            float rise = Mathf.Clamp01(1f - activeTimer / ActiveDuration);
            if (magmaVisual != null)
            {
                magmaVisual.localScale = new Vector3(OuterRadius * 1.9f, 0.05f + rise * 1.4f, OuterRadius * 1.9f);
                magmaVisual.localPosition = new Vector3(0f, magmaVisual.localScale.y * 0.5f, 0f);
            }

            if (!hasHit)
                TryHitPlayer();

            if (activeTimer <= 0f)
                Destroy(gameObject);
        }

        void AnimateWarning()
        {
            if (warningRing == null)
                return;

            float pulse = 1f + Mathf.Sin(Time.time * 10f) * 0.12f;
            warningRing.localScale = new Vector3(OuterRadius * 2f * pulse, 0.06f, OuterRadius * 2f * pulse);
        }

        void TriggerBurst()
        {
            hasTriggered = true;
            activeTimer = ActiveDuration;
            if (warningRing != null)
                warningRing.gameObject.SetActive(false);
            if (magmaVisual != null)
                magmaVisual.gameObject.SetActive(true);
        }

        void TryHitPlayer()
        {
            if (runner == null || !runner.IsGrounded)
                return;

            Vector3 delta = player.position - transform.position;
            delta.y = 0f;
            if (delta.magnitude > OuterRadius)
                return;

            KnightHealth health = player.GetComponent<KnightHealth>();
            if (health == null)
                return;

            health.TakeDamage(GroundedDamage);
            hasHit = true;
        }

        void ResolvePlayer()
        {
            runner = FindFirstObjectByType<RunnerController>();
            if (runner != null)
                player = runner.transform;
        }
    }
}
