using System.Collections.Generic;
using KnightRun.Core;
using KnightRun.Player;
using KnightRun.World;
using UnityEngine;

namespace KnightRun.Gameplay
{
    public class BossMineDerailAttack : MonoBehaviour
    {
        public const float MinimumWarningDuration = 1.5f;
        public const int ContactDamage = 350;
        public const int TargetedLaneCount = 3;
        const float ImpactOffsetBehindBoss = 3f;
        const float CrashTriggerDistance = 10f;
        const float ActiveDuration = 2.2f;
        const float HitDepth = 1.8f;

        Transform player;
        RunnerController runner;
        GameManager gameManager;
        readonly List<int> markedLanes = new List<int>();
        readonly List<Transform> warnings = new List<Transform>();
        readonly List<Transform> crashes = new List<Transform>();
        float warningTimer;
        bool hasTriggered;
        bool hasHit;
        float activeTimer;
        float impactZ;

        public static BossMineDerailAttack Spawn(RunnerController runner, Vector3 bossPosition)
        {
            var go = new GameObject("BossMineDerail");
            go.transform.position = bossPosition;

            var attack = go.AddComponent<BossMineDerailAttack>();
            attack.Build(runner, bossPosition);
            return attack;
        }

        void Build(RunnerController target, Vector3 bossPosition)
        {
            runner = target;
            player = target.transform;
            warningTimer = MinimumWarningDuration;
            impactZ = bossPosition.z - ImpactOffsetBehindBoss;

            float[] lanes = RunnerController.LanePositions;
            var available = new List<int>();
            for (int i = 0; i < lanes.Length; i++)
                available.Add(i);

            int count = Mathf.Min(TargetedLaneCount, available.Count);
            for (int i = 0; i < count; i++)
            {
                int pick = Random.Range(0, available.Count);
                int lane = available[pick];
                available.RemoveAt(pick);
                markedLanes.Add(lane);

                float x = lanes[lane];

                var mark = GameObject.CreatePrimitive(PrimitiveType.Cube);
                mark.name = $"LaneWarning_{lane}";
                mark.transform.SetParent(transform, false);
                mark.transform.position = new Vector3(x, 0.08f, impactZ);
                mark.transform.localScale = new Vector3(1.35f, 0.1f, 4f);
                mark.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.MineRail);
                ApplyTint(mark.GetComponent<Renderer>(), new Color(0.95f, 0.25f, 0.1f));
                Object.Destroy(mark.GetComponent<Collider>());
                warnings.Add(mark.transform);
            }
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
        }

        void Update()
        {
            if (gameManager == null || gameManager.State != GameState.Running)
                return;

            if (player == null)
                return;

            if (!hasTriggered)
            {
                PulseWarnings();
                warningTimer -= Time.deltaTime;
                float distanceAhead = impactZ - player.position.z;
                if (warningTimer <= 0f && distanceAhead <= CrashTriggerDistance)
                    TriggerCrash();
                return;
            }

            activeTimer -= Time.deltaTime;
            TryHitPlayer();

            if (activeTimer <= 0f || player.position.z > impactZ + HitDepth + 4f)
                Destroy(gameObject);
        }

        void PulseWarnings()
        {
            float pulse = 1f + Mathf.Sin(Time.time * 11f) * 0.12f;
            for (int i = 0; i < warnings.Count; i++)
            {
                if (warnings[i] == null)
                    continue;
                warnings[i].localScale = new Vector3(1.35f * pulse, 0.1f, 4f);
            }
        }

        void TriggerCrash()
        {
            hasTriggered = true;
            activeTimer = ActiveDuration;
            float[] lanes = RunnerController.LanePositions;

            for (int i = 0; i < warnings.Count; i++)
            {
                if (warnings[i] != null)
                    warnings[i].gameObject.SetActive(false);
            }

            for (int i = 0; i < markedLanes.Count; i++)
            {
                int lane = markedLanes[i];
                float x = lanes[lane];

                var cart = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cart.name = $"DerailCart_{lane}";
                cart.transform.SetParent(transform, false);
                cart.transform.position = new Vector3(x, 1.6f, impactZ);
                cart.transform.localScale = new Vector3(1.2f, 1.1f, 1.6f);
                cart.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.MineCart);
                Object.Destroy(cart.GetComponent<Collider>());
                crashes.Add(cart.transform);
            }
        }

        void TryHitPlayer()
        {
            if (hasHit || runner == null || !runner.IsGrounded)
                return;

            if (!markedLanes.Contains(runner.CurrentLane))
                return;

            if (Mathf.Abs(player.position.z - impactZ) > HitDepth)
                return;

            KnightHealth health = player.GetComponent<KnightHealth>();
            if (health == null)
                return;

            health.TakeDamage(ContactDamage);
            activeTimer = 0f;
            hasHit = true;
        }
    }
}
