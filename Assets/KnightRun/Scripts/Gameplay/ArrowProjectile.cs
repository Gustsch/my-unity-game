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

        static readonly Vector3 ShaftBaseScale = new Vector3(0.07f, 0.07f, 0.55f);
        static readonly Vector3 TipBaseScale = new Vector3(0.09f, 0.09f, 0.14f);

        Vector3 startPosition;
        float damage;
        float areaMultiplier;
        float lifetime;
        float ownTravelDistance;
        float maxTravelDistance;
        bool hasHit;

        public static ArrowProjectile Spawn(Vector3 position, float damage, float areaMultiplier)
        {
            areaMultiplier = Mathf.Max(1f, areaMultiplier);

            var go = new GameObject("Arrow");
            go.transform.position = position;
            go.transform.rotation = Quaternion.identity;

            var shaft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shaft.name = "Shaft";
            shaft.transform.SetParent(go.transform, false);
            shaft.transform.localScale = ShaftBaseScale * areaMultiplier;
            shaft.transform.localPosition = new Vector3(0f, 0f, 0.28f * areaMultiplier);
            shaft.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.LogObstacle);
            Object.Destroy(shaft.GetComponent<Collider>());

            var tip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tip.name = "Tip";
            tip.transform.SetParent(go.transform, false);
            tip.transform.localScale = TipBaseScale * areaMultiplier;
            tip.transform.localPosition = new Vector3(0f, 0f, 0.62f * areaMultiplier);
            tip.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.KnightSword);
            Object.Destroy(tip.GetComponent<Collider>());

            var arrow = go.AddComponent<ArrowProjectile>();
            arrow.startPosition = position;
            arrow.damage = Mathf.Max(0.01f, damage);
            arrow.areaMultiplier = areaMultiplier;
            arrow.maxTravelDistance = RunForwardMotion.GetScaledProjectileRange(BaseMaxTravelDistance);
            return arrow;
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
