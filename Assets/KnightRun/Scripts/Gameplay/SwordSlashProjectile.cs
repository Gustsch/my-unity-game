using KnightRun.Core;
using KnightRun.World;
using UnityEngine;

namespace KnightRun.Gameplay
{
    public class SwordSlashProjectile : MonoBehaviour
    {
        const float Speed = 46f;
        const float MaxLifetime = 6f;
        const float BaseMaxTravelDistance = 85f;
        const float BaseHitWidth = 1.35f;
        const float BaseHitDepth = 0.55f;
        const float HitboxHeight = 8f;

        static readonly Vector3 BladeBaseScale = new Vector3(1.1f, 0.05f, 0.22f);
        static readonly Vector3 TrailBaseScale = new Vector3(0.75f, 0.03f, 0.14f);

        float damage;
        float areaMultiplier;
        float lifetime;
        float ownTravelDistance;
        float maxTravelDistance;
        bool hasHit;

        public static SwordSlashProjectile Spawn(Vector3 position, float damage, float areaMultiplier)
        {
            areaMultiplier = Mathf.Max(1f, areaMultiplier);

            var go = new GameObject("SwordSlash");
            go.transform.position = position;
            go.transform.rotation = Quaternion.identity;

            var blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blade.name = "SlashBlade";
            blade.transform.SetParent(go.transform, false);
            blade.transform.localScale = BladeBaseScale * areaMultiplier;
            blade.transform.localPosition = new Vector3(0f, 0f, 0.18f * areaMultiplier);
            blade.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.KnightSword);
            Object.Destroy(blade.GetComponent<Collider>());

            var trail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trail.name = "SlashTrail";
            trail.transform.SetParent(go.transform, false);
            trail.transform.localScale = TrailBaseScale * areaMultiplier;
            trail.transform.localPosition = new Vector3(0f, 0f, -0.08f * areaMultiplier);
            trail.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.KnightSword);
            Material trailMaterial = trail.GetComponent<Renderer>().material;
            Color trailColor = trailMaterial.color;
            trailColor.a = 0.65f;
            trailMaterial.color = trailColor;
            if (trailMaterial.HasProperty("_BaseColor"))
            {
                Color baseColor = trailMaterial.GetColor("_BaseColor");
                baseColor.a = 0.65f;
                trailMaterial.SetColor("_BaseColor", baseColor);
            }

            Object.Destroy(trail.GetComponent<Collider>());

            var slash = go.AddComponent<SwordSlashProjectile>();
            slash.damage = Mathf.Max(0.01f, damage);
            slash.areaMultiplier = areaMultiplier;
            slash.maxTravelDistance = RunForwardMotion.GetScaledProjectileRange(BaseMaxTravelDistance);
            return slash;
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
