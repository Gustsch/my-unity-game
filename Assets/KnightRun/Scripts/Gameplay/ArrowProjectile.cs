using System.Collections.Generic;
using KnightRun.Core;
using KnightRun.World;
using UnityEngine;

namespace KnightRun.Gameplay
{
    public class ArrowProjectile : MonoBehaviour
    {
        const float Speed = 40f;
        const float MaxLifetime = 7f;
        const float BaseMaxTravelDistance = 100f;
        const float BaseHitWidth = 0.35f;
        const float BaseHitDepth = 0.9f;
        const float HitboxHeight = 8f;

        Vector3 startPosition;
        float damage;
        float areaMultiplier;
        float lifetime;
        float ownTravelDistance;
        float maxTravelDistance;
        int remainingHits;
        readonly HashSet<int> hitTargets = new HashSet<int>();

        public static ArrowProjectile Spawn(Vector3 position, float damage, float areaMultiplier, int pierceExtraHits = 0)
        {
            areaMultiplier = Mathf.Max(1f, areaMultiplier);

            var go = new GameObject("Arrow");
            go.transform.position = position;
            go.transform.rotation = Quaternion.identity;

            WeaponAssetVisual.Create(
                pierceExtraHits > 0 ? "ArrowPiercing" : "Arrow",
                go.transform,
                0.8f * areaMultiplier,
                Quaternion.Euler(90f, 0f, 0f),
                new Vector3(0f, 0f, 0.4f * areaMultiplier));

            var arrow = go.AddComponent<ArrowProjectile>();
            arrow.startPosition = position;
            arrow.damage = Mathf.Max(0.01f, damage);
            arrow.areaMultiplier = areaMultiplier;
            arrow.maxTravelDistance = RunForwardMotion.GetScaledProjectileRange(BaseMaxTravelDistance);
            arrow.remainingHits = 1 + Mathf.Max(0, pierceExtraHits);
            return arrow;
        }

        void Update()
        {
            if (remainingHits <= 0)
                return;

            GameManager gameManager = GameManager.Instance;
            if (gameManager == null || gameManager.State != GameState.Running)
                return;

            lifetime += Time.deltaTime;
            if (lifetime >= MaxLifetime)
            {
                Destroy(gameObject);
                return;
            }

            transform.position += Vector3.forward * Speed * Time.deltaTime;
            transform.position += RunForwardMotion.GetDelta();
            ownTravelDistance += Speed * Time.deltaTime;

            if (ProjectileTrackBounds.IsBeyondWalls(transform.position))
            {
                Destroy(gameObject);
                return;
            }

            if (ownTravelDistance >= maxTravelDistance)
            {
                Destroy(gameObject);
                return;
            }

            DetectHits();
        }

        void DetectHits()
        {
            float hitWidth = BaseHitWidth * areaMultiplier;
            float hitDepth = BaseHitDepth * areaMultiplier;
            Vector3 center = new Vector3(
                transform.position.x,
                HitboxHeight * 0.5f,
                transform.position.z + hitDepth * 0.25f);
            Vector3 halfExtents = new Vector3(hitWidth * 0.5f, HitboxHeight * 0.5f, hitDepth * 0.5f);

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

                if (CombatTarget.TryApplyDamage(hit, damage, hitTargets))
                {
                    remainingHits--;
                    if (remainingHits <= 0)
                    {
                        Destroy(gameObject);
                        return;
                    }
                }
            }
        }
    }
}
