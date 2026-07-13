using KnightRun.Core;
using KnightRun.World;
using UnityEngine;

namespace KnightRun.Gameplay
{
    public class ShurikenProjectile : MonoBehaviour
    {
        const float Speed = 30f;
        const float MaxLifetime = 7f;
        const float BaseMaxTravelDistance = 90f;
        const float BaseHitRadius = 0.35f;
        const float HitboxHeight = 8f;

        Vector3 startPosition;
        Vector3 direction;
        float damage;
        float areaMultiplier;
        float lifetime;
        float ownTravelDistance;
        float maxTravelDistance;
        bool hasHit;

        public static ShurikenProjectile Spawn(Vector3 position, float damage, Vector3 direction, float areaMultiplier)
        {
            areaMultiplier = Mathf.Max(1f, areaMultiplier);
            if (direction.sqrMagnitude < 0.001f)
                direction = Vector3.forward;
            direction.Normalize();

            var go = new GameObject("Shuriken");
            go.transform.position = position;
            go.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

            CreateBlade(go.transform, new Vector3(0.28f, 0.04f, 0.06f) * areaMultiplier, Quaternion.Euler(0f, 0f, 45f));
            CreateBlade(go.transform, new Vector3(0.28f, 0.04f, 0.06f) * areaMultiplier, Quaternion.Euler(0f, 0f, -45f));
            CreateBlade(go.transform, new Vector3(0.06f, 0.04f, 0.28f) * areaMultiplier, Quaternion.identity);
            CreateBlade(go.transform, new Vector3(0.06f, 0.04f, 0.28f) * areaMultiplier, Quaternion.Euler(0f, 90f, 0f));
            CreateBlade(go.transform, new Vector3(0.08f, 0.08f, 0.08f) * areaMultiplier, Quaternion.identity);

            var projectile = go.AddComponent<ShurikenProjectile>();
            projectile.startPosition = position;
            projectile.direction = direction;
            projectile.damage = Mathf.Max(0.01f, damage);
            projectile.areaMultiplier = areaMultiplier;
            projectile.maxTravelDistance = RunForwardMotion.GetScaledProjectileRange(BaseMaxTravelDistance);
            return projectile;
        }

        void Update()
        {
            if (hasHit)
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

            transform.position += direction * Speed * Time.deltaTime;
            transform.position += RunForwardMotion.GetDelta();
            ownTravelDistance += Speed * Time.deltaTime;
            transform.Rotate(Vector3.forward, 720f * Time.deltaTime, Space.Self);

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
                    Destroy(gameObject);
                    return;
                }
            }
        }

        static void CreateBlade(Transform parent, Vector3 scale, Quaternion rotation)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.SetParent(parent, false);
            go.transform.localScale = scale;
            go.transform.localRotation = rotation;
            go.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.KnightSword);
            Object.Destroy(go.GetComponent<Collider>());
        }
    }
}
