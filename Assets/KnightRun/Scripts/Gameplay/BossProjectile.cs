using KnightRun.Core;
using KnightRun.Player;
using KnightRun.World;
using UnityEngine;

namespace KnightRun.Gameplay
{
    public class BossProjectile : MonoBehaviour
    {
        const float Speed = 14f;
        const float MaxLifetime = 6f;
        const float HitRadius = 0.55f;

        static readonly Color LowColor = new Color(0.95f, 0.45f, 0.1f);
        static readonly Color MediumColor = new Color(0.95f, 0.82f, 0.15f);
        static readonly Color HighColor = new Color(0.75f, 0.35f, 0.95f);

        Vector3 direction;
        int damage;
        float lifetime;
        BossAttackBand attackBand;
        float travelHeight;
        float hitHalfHeight;

        public static void Spawn(Vector3 origin, Vector3 targetPosition, int damage, BossAttackBand band)
        {
            Vector3 direction = targetPosition - origin;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.001f)
                direction = Vector3.back;
            direction.Normalize();

            float travelY;
            float halfHeight;
            Color color;
            Vector3 scale;
            switch (band)
            {
                case BossAttackBand.High:
                    travelY = 2.75f;
                    halfHeight = 0.42f;
                    color = HighColor;
                    scale = new Vector3(0.62f, 0.62f, 0.62f);
                    break;
                case BossAttackBand.Medium:
                    travelY = 1.45f;
                    halfHeight = 0.34f;
                    color = MediumColor;
                    scale = new Vector3(0.58f, 0.42f, 0.58f);
                    break;
                default:
                    travelY = 0.55f;
                    halfHeight = 0.32f;
                    color = LowColor;
                    scale = new Vector3(0.72f, 0.28f, 0.72f);
                    break;
            }

            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "BossProjectile";
            go.transform.position = new Vector3(origin.x, travelY, origin.z);
            go.transform.localScale = scale;

            var renderer = go.GetComponent<Renderer>();
            renderer.sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.VolcanoRock);
            Material material = renderer.material;
            material.color = color;
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);

            var collider = go.GetComponent<SphereCollider>();
            collider.isTrigger = true;

            var projectile = go.AddComponent<BossProjectile>();
            projectile.direction = direction;
            projectile.damage = Mathf.Max(1, damage);
            projectile.attackBand = band;
            projectile.travelHeight = travelY;
            projectile.hitHalfHeight = halfHeight;
        }

        public static bool TryBreak(Collider hit)
        {
            if (hit == null)
                return false;

            BossProjectile projectile = hit.GetComponent<BossProjectile>() ?? hit.GetComponentInParent<BossProjectile>();
            if (projectile == null)
                return false;

            UnityEngine.Object.Destroy(projectile.gameObject);
            return true;
        }

        void Update()
        {
            GameManager gameManager = GameManager.Instance;
            if (gameManager == null || gameManager.State != GameState.Running)
                return;

            lifetime += Time.deltaTime;
            if (lifetime >= MaxLifetime)
            {
                Destroy(gameObject);
                return;
            }

            float speed = attackBand == BossAttackBand.Low ? Speed * 0.85f : Speed;
            transform.position += direction * speed * Time.deltaTime;
            transform.position += RunForwardMotion.GetDelta();
            DetectHits();
        }

        void DetectHits()
        {
            var runner = FindFirstObjectByType<RunnerController>();
            if (runner == null)
                return;

            if (!TryGetPlayerVerticalSpan(runner, out float playerMinY, out float playerMaxY))
                return;

            float projectileMinY = travelHeight - hitHalfHeight;
            float projectileMaxY = travelHeight + hitHalfHeight;
            if (projectileMaxY < playerMinY || projectileMinY > playerMaxY)
                return;

            Vector3 playerCenter = runner.transform.position + runner.GetComponent<CharacterController>().center;
            Vector3 delta = new Vector3(
                transform.position.x - playerCenter.x,
                0f,
                transform.position.z - playerCenter.z);

            if (delta.sqrMagnitude > HitRadius * HitRadius)
                return;

            if (!ShouldHitPlayerState(runner, attackBand))
                return;

            KnightHealth health = runner.GetComponent<KnightHealth>();
            if (health != null)
                health.TakeDamage(damage);

            Destroy(gameObject);
        }

        static bool ShouldHitPlayerState(RunnerController runner, BossAttackBand band)
        {
            return band switch
            {
                BossAttackBand.Low => runner.IsGrounded && !runner.IsSliding,
                BossAttackBand.High => !runner.IsGrounded && !runner.IsSliding,
                _ => runner.IsGrounded && !runner.IsSliding
            };
        }

        static bool TryGetPlayerVerticalSpan(RunnerController runner, out float minY, out float maxY)
        {
            minY = 0f;
            maxY = 0f;

            CharacterController controller = runner.GetComponent<CharacterController>();
            if (controller == null)
                return false;

            Vector3 center = runner.transform.position + controller.center;
            float halfHeight = controller.height * 0.5f;
            minY = center.y - halfHeight;
            maxY = center.y + halfHeight;
            return true;
        }
    }
}
