using KnightRun.Core;
using KnightRun.World;
using UnityEngine;

namespace KnightRun.Gameplay
{
    public class ThrowingAxeProjectile : MonoBehaviour
    {
        const float MaxLifetime = 4f;
        const float MaxTravelDistance = 42f;
        const float BaseHitRadius = 0.5f;
        const float HitboxHeight = 8f;

        Vector3 startPosition;
        Vector3 direction;
        float damage;
        float speed;
        float areaMultiplier;
        float lifetime;
        bool hasHit;

        public static ThrowingAxeProjectile Spawn(
            Vector3 position,
            Vector3 direction,
            float damage,
            float speed,
            float areaMultiplier)
        {
            areaMultiplier = Mathf.Max(1f, areaMultiplier);
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.001f)
                direction = Vector3.forward;
            direction.Normalize();

            var go = new GameObject("ThrowingAxe");
            go.transform.position = position;
            go.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

            var head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.name = "AxeHead";
            head.transform.SetParent(go.transform, false);
            head.transform.localScale = new Vector3(0.34f, 0.08f, 0.22f) * areaMultiplier;
            head.transform.localPosition = new Vector3(0f, 0f, 0.18f * areaMultiplier);
            head.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.KnightSword);
            Object.Destroy(head.GetComponent<Collider>());

            var handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            handle.name = "AxeHandle";
            handle.transform.SetParent(go.transform, false);
            handle.transform.localScale = new Vector3(0.08f, 0.08f, 0.28f) * areaMultiplier;
            handle.transform.localPosition = new Vector3(0f, 0f, -0.12f * areaMultiplier);
            handle.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.LogObstacle);
            Object.Destroy(handle.GetComponent<Collider>());

            var projectile = go.AddComponent<ThrowingAxeProjectile>();
            projectile.startPosition = position;
            projectile.direction = direction;
            projectile.damage = Mathf.Max(0.01f, damage);
            projectile.speed = Mathf.Max(4f, speed);
            projectile.areaMultiplier = areaMultiplier;
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

            transform.position += direction * speed * Time.deltaTime;
            transform.position += RunForwardMotion.GetDelta();
            transform.Rotate(Vector3.right, 540f * Time.deltaTime, Space.Self);

            if ((transform.position - startPosition).sqrMagnitude >= MaxTravelDistance * MaxTravelDistance)
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
    }
}
