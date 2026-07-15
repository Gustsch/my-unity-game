using KnightRun.Core;
using KnightRun.Progression;
using KnightRun.World;
using UnityEngine;

namespace KnightRun.Gameplay
{
    public class BombProjectile : MonoBehaviour
    {
        const float ThrowDuration = 0.55f;
        const float ArcHeight = 3.5f;
        const float ExplosionHeight = 8f;

        Vector3 startPosition;
        Vector3 landPosition;
        float damage;
        float explosionRadius;
        float timer;
        bool exploded;

        public static void Throw(Vector3 from, Vector3 landPosition, float damage, float areaMultiplier)
        {
            areaMultiplier = Mathf.Max(1f, areaMultiplier);

            var go = new GameObject("Bomb");
            go.name = "Bomb";
            WeaponAssetVisual.Create(
                "Bomb",
                go.transform,
                0.55f,
                Quaternion.Euler(-90f, 0f, 0f),
                Vector3.zero);

            var bomb = go.AddComponent<BombProjectile>();
            bomb.startPosition = from;
            bomb.landPosition = landPosition;
            bomb.damage = Mathf.Max(0.01f, damage);
            bomb.explosionRadius = SkillPool.BombBaseExplosionRadius * areaMultiplier;
            go.transform.position = from;
        }

        void Update()
        {
            if (exploded)
                return;

            GameManager gameManager = GameManager.Instance;
            if (gameManager == null || gameManager.State != GameState.Running)
                return;

            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / ThrowDuration);

            Vector3 runDelta = RunForwardMotion.GetDelta();
            startPosition += runDelta;
            landPosition += runDelta;

            if (t < 1f)
            {
                Vector3 position = Vector3.Lerp(startPosition, landPosition, t);
                position.y += Mathf.Sin(t * Mathf.PI) * ArcHeight;
                transform.position = position;
                return;
            }

            Explode();
        }

        void Explode()
        {
            exploded = true;

            Vector3 center = new Vector3(landPosition.x, ExplosionHeight * 0.5f, landPosition.z);
            Vector3 halfExtents = new Vector3(explosionRadius, ExplosionHeight * 0.5f, explosionRadius);

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

                CombatTarget.TryApplyDamage(hit, damage);
            }

            Destroy(gameObject);
        }
    }
}
