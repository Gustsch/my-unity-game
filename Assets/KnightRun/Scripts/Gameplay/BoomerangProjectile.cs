using System;
using KnightRun.Core;
using KnightRun.World;
using UnityEngine;

namespace KnightRun.Gameplay
{
    public class BoomerangProjectile : MonoBehaviour
    {
        const float MaxLifetime = 14f;
        const float BaseHitRadius = 0.45f;
        const float HitboxHeight = 8f;
        const float ReturnArrivalDistance = 0.55f;
        const float PhaseSpeedBoost = 0.65f;

        enum Phase
        {
            Outbound,
            Returning
        }

        Transform player;
        Transform target;
        float damage;
        float speed;
        float areaMultiplier;
        float lifetime;
        bool hasHit;
        bool returnNotified;
        Phase phase;
        Action onReturned;

        public static BoomerangProjectile Spawn(
            Vector3 position,
            Transform player,
            Transform target,
            float damage,
            float speed,
            float areaMultiplier,
            Action onReturned)
        {
            areaMultiplier = Mathf.Max(1f, areaMultiplier);

            var go = new GameObject("Boomerang");
            go.transform.position = position;
            go.transform.rotation = Quaternion.identity;

            CreateBlade(go.transform, new Vector3(0.42f, 0.05f, 0.12f) * areaMultiplier, Quaternion.Euler(0f, 0f, 18f));
            CreateBlade(go.transform, new Vector3(0.18f, 0.05f, 0.3f) * areaMultiplier, Quaternion.Euler(0f, 72f, 0f));
            CreateBlade(go.transform, new Vector3(0.08f, 0.08f, 0.08f) * areaMultiplier, Quaternion.identity);

            var projectile = go.AddComponent<BoomerangProjectile>();
            projectile.player = player;
            projectile.target = target;
            projectile.damage = Mathf.Max(0.01f, damage);
            projectile.speed = Mathf.Max(4f, speed * (1f + (RunForwardMotion.GetPhaseSpeedMultiplier() - 1f) * PhaseSpeedBoost));
            projectile.areaMultiplier = areaMultiplier;
            projectile.onReturned = onReturned;
            projectile.phase = Phase.Outbound;
            return projectile;
        }

        void Update()
        {
            GameManager gameManager = GameManager.Instance;
            if (gameManager == null || gameManager.State != GameState.Running)
                return;

            lifetime += Time.deltaTime;
            if (lifetime >= MaxLifetime)
            {
                CompleteReturn();
                return;
            }

            if (phase == Phase.Outbound)
                UpdateOutbound();
            else
                UpdateReturning();
        }

        void UpdateOutbound()
        {
            if (target == null)
            {
                BeginReturn();
                return;
            }

            Vector3 targetPosition = target.position;
            targetPosition.y = transform.position.y;

            MoveToward(targetPosition);
            transform.Rotate(Vector3.up, 720f * Time.deltaTime, Space.Self);
            TryHit();

            if ((transform.position - targetPosition).sqrMagnitude <= ReturnArrivalDistance * ReturnArrivalDistance)
                BeginReturn();
        }

        void UpdateReturning()
        {
            if (player == null)
            {
                CompleteReturn();
                return;
            }

            Vector3 returnPosition = player.position + Vector3.up;
            returnPosition.y = transform.position.y;

            MoveToward(returnPosition);
            transform.Rotate(Vector3.up, 900f * Time.deltaTime, Space.Self);

            if ((transform.position - returnPosition).sqrMagnitude <= ReturnArrivalDistance * ReturnArrivalDistance)
                CompleteReturn();
        }

        void MoveToward(Vector3 destination)
        {
            Vector3 offset = destination - transform.position;
            offset.y = 0f;
            if (offset.sqrMagnitude < 0.0001f)
                return;

            Vector3 step = offset.normalized * speed * Time.deltaTime;
            if (step.sqrMagnitude > offset.sqrMagnitude)
                step = offset;

            transform.position += step;
            transform.position += RunForwardMotion.GetDelta();

            if (offset.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(offset.normalized, Vector3.up);
        }

        void TryHit()
        {
            if (hasHit)
                return;

            float hitRadius = BaseHitRadius * areaMultiplier;
            Vector3 center = new Vector3(transform.position.x, HitboxHeight * 0.5f, transform.position.z);
            Vector3 halfExtents = new Vector3(hitRadius, HitboxHeight * 0.5f, hitRadius);

            Collider[] hits = Physics.OverlapBox(
                center,
                halfExtents,
                Quaternion.identity,
                ~0,
                QueryTriggerInteraction.Collide);

            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Player"))
                    continue;

                if (CombatTarget.TryApplyDamage(hit, damage))
                {
                    hasHit = true;
                    return;
                }
            }
        }

        void BeginReturn()
        {
            phase = Phase.Returning;
        }

        void CompleteReturn()
        {
            if (!returnNotified)
            {
                returnNotified = true;
                onReturned?.Invoke();
            }

            Destroy(gameObject);
        }

        void OnDestroy()
        {
            if (!returnNotified)
            {
                returnNotified = true;
                onReturned?.Invoke();
            }
        }

        static void CreateBlade(Transform parent, Vector3 scale, Quaternion rotation)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.SetParent(parent, false);
            go.transform.localScale = scale;
            go.transform.localRotation = rotation;
            go.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.LogObstacle);
            UnityEngine.Object.Destroy(go.GetComponent<Collider>());
        }
    }
}
