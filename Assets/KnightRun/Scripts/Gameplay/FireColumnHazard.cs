using KnightRun.Core;
using KnightRun.Player;
using KnightRun.World;
using UnityEngine;

namespace KnightRun.Gameplay
{
    public class FireColumnHazard : MonoBehaviour
    {
        public const int GroundedDamage = 500;
        public const int AirborneDamage = 300;
        public const float DespawnBehindDistance = 14f;

        const float ContactCooldown = 0.55f;
        const float RiseDuration = 0.35f;
        const float ActiveDuration = 1.8f;
        const float RiseTriggerDistance = 11f;
        const float ColumnRadius = 0.85f;
        const float ColumnHeight = 3.4f;
        const float BaseMarkHeight = 0.08f;

        enum Phase
        {
            Warning,
            Rising,
            Active
        }

        Transform player;
        Transform warningMark;
        Transform columnVisual;
        BoxCollider hitbox;
        GameManager gameManager;
        Phase phase = Phase.Warning;
        float phaseTimer;
        float contactCooldownTimer;

        public static FireColumnHazard Spawn(Transform parent, Vector3 worldPosition)
        {
            var go = new GameObject("FireColumn");
            go.transform.SetParent(parent, false);
            go.transform.position = worldPosition;

            var hazard = go.AddComponent<FireColumnHazard>();
            hazard.BuildVisual();
            return hazard;
        }

        void BuildVisual()
        {
            var mark = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mark.name = "WarningMark";
            mark.transform.SetParent(transform, false);
            mark.transform.localScale = new Vector3(ColumnRadius * 2.2f, BaseMarkHeight, ColumnRadius * 2.2f);
            mark.transform.localPosition = new Vector3(0f, BaseMarkHeight, 0f);
            mark.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.LavaPool);
            Object.Destroy(mark.GetComponent<Collider>());
            warningMark = mark.transform;

            var column = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            column.name = "Flame";
            column.transform.SetParent(transform, false);
            column.transform.localScale = new Vector3(ColumnRadius * 1.6f, 0.01f, ColumnRadius * 1.6f);
            column.transform.localPosition = new Vector3(0f, 0.01f, 0f);
            column.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.LavaPool);
            ApplyTint(column.GetComponent<Renderer>(), new Color(1f, 0.35f, 0.05f));
            Object.Destroy(column.GetComponent<Collider>());
            columnVisual = column.transform;
            columnVisual.gameObject.SetActive(false);

            hitbox = gameObject.AddComponent<BoxCollider>();
            hitbox.isTrigger = true;
            hitbox.enabled = false;
            hitbox.size = new Vector3(ColumnRadius * 1.7f, 0.2f, ColumnRadius * 1.7f);
            hitbox.center = new Vector3(0f, 0.1f, 0f);

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
            AnimateFlame();

            if (transform.position.z < player.position.z - DespawnBehindDistance)
                Destroy(gameObject);
        }

        void UpdatePhase()
        {
            switch (phase)
            {
                case Phase.Warning:
                    float aheadDistance = transform.position.z - player.position.z;
                    if (aheadDistance <= RiseTriggerDistance && aheadDistance >= -1f)
                        BeginRise();
                    break;

                case Phase.Rising:
                    phaseTimer -= Time.deltaTime;
                    float riseProgress = 1f - Mathf.Clamp01(phaseTimer / RiseDuration);
                    ApplyColumnHeight(Mathf.Lerp(0.05f, ColumnHeight, riseProgress));
                    if (phaseTimer <= 0f)
                    {
                        phase = Phase.Active;
                        phaseTimer = ActiveDuration;
                        ApplyColumnHeight(ColumnHeight);
                    }
                    break;

                case Phase.Active:
                    phaseTimer -= Time.deltaTime;
                    ApplyColumnHeight(ColumnHeight);
                    if (phaseTimer <= 0f)
                        Destroy(gameObject);
                    break;
            }
        }

        void BeginRise()
        {
            phase = Phase.Rising;
            phaseTimer = RiseDuration;
            if (columnVisual != null)
                columnVisual.gameObject.SetActive(true);
            if (hitbox != null)
                hitbox.enabled = true;
        }

        void ApplyColumnHeight(float height)
        {
            height = Mathf.Max(0.05f, height);
            if (columnVisual != null)
            {
                columnVisual.localScale = new Vector3(ColumnRadius * 1.6f, height * 0.5f, ColumnRadius * 1.6f);
                columnVisual.localPosition = new Vector3(0f, height * 0.5f, 0f);
            }

            if (hitbox != null)
            {
                hitbox.size = new Vector3(ColumnRadius * 1.7f, height, ColumnRadius * 1.7f);
                hitbox.center = new Vector3(0f, height * 0.5f, 0f);
            }
        }

        void AnimateWarning()
        {
            if (warningMark == null || phase != Phase.Warning)
                return;

            float pulse = 1f + Mathf.Sin(Time.time * 8f) * 0.18f;
            warningMark.localScale = new Vector3(ColumnRadius * 2.2f * pulse, BaseMarkHeight, ColumnRadius * 2.2f * pulse);

            var renderer = warningMark.GetComponent<Renderer>();
            if (renderer == null)
                return;

            float glow = 0.55f + Mathf.Sin(Time.time * 10f) * 0.35f;
            Color color = new Color(1f, 0.25f + glow * 0.2f, 0.05f, 1f);
            Material material = renderer.material;
            material.color = color;
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
        }

        void AnimateFlame()
        {
            if (columnVisual == null || !columnVisual.gameObject.activeSelf || phase == Phase.Warning)
                return;

            float pulse = 1f + Mathf.Sin(Time.time * 18f) * 0.08f;
            Vector3 scale = columnVisual.localScale;
            scale.x = ColumnRadius * 1.6f * pulse;
            scale.z = ColumnRadius * 1.6f * pulse;
            columnVisual.localScale = scale;
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

            var runner = other.GetComponent<RunnerController>();
            if (runner == null)
                return;

            KnightHealth health = other.GetComponent<KnightHealth>();
            if (health == null)
                return;

            int damage = runner.IsGrounded ? GroundedDamage : AirborneDamage;
            health.TakeDamage(damage);
            runner.ApplyPhaseObstacleHit();
            contactCooldownTimer = ContactCooldown;
        }

        void ResolvePlayer()
        {
            var runner = FindFirstObjectByType<RunnerController>();
            if (runner != null)
                player = runner.transform;
        }
    }
}
