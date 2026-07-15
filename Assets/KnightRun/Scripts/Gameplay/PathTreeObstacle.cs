using KnightRun.Core;
using KnightRun.Player;
using KnightRun.World;
using UnityEngine;

namespace KnightRun.Gameplay
{
    public class PathTreeObstacle : MonoBehaviour
    {
        public const int ContactDamage = 300;
        public const float DespawnBehindDistance = 12f;

        const float ContactCooldown = 0.6f;

        Transform player;
        GameManager gameManager;
        float contactCooldownTimer;

        public static PathTreeObstacle Spawn(Transform parent, Vector3 worldPosition, GameObject visualPrefab)
        {
            var go = new GameObject("ForestObstacle");
            go.transform.SetParent(parent, false);
            go.transform.position = worldPosition;

            var tree = go.AddComponent<PathTreeObstacle>();
            tree.BuildVisual(visualPrefab);
            return tree;
        }

        void BuildVisual(GameObject visualPrefab)
        {
            GameObject visual = SimpleNatureCatalog.InstantiateVisual(visualPrefab, transform);
            if (visual != null)
            {
                visual.name = visualPrefab.name;
                visual.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                visual.transform.localScale = Vector3.one * Random.Range(1.05f, 1.35f);
            }

            var hitbox = gameObject.AddComponent<BoxCollider>();
            hitbox.isTrigger = true;

            if (visual == null || !TryGetVisualBounds(visual, out Bounds bounds))
            {
                hitbox.size = new Vector3(0.85f, 1.1f, 0.85f);
                hitbox.center = new Vector3(0f, 0.55f, 0f);
                return;
            }

            Vector3 localCenter = transform.InverseTransformPoint(bounds.center);
            hitbox.size = new Vector3(
                Mathf.Clamp(bounds.size.x, 0.7f, 1.6f),
                Mathf.Clamp(bounds.size.y, 0.65f, 1.8f),
                Mathf.Clamp(bounds.size.z, 0.7f, 1.6f));
            hitbox.center = new Vector3(localCenter.x, hitbox.size.y * 0.5f, localCenter.z);
        }

        static bool TryGetVisualBounds(GameObject visual, out Bounds bounds)
        {
            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                bounds = default;
                return false;
            }

            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            return true;
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

            if (transform.position.z < player.position.z - DespawnBehindDistance)
                Destroy(gameObject);
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
            if (!other.CompareTag("Player") || contactCooldownTimer > 0f)
                return;

            KnightHealth health = other.GetComponent<KnightHealth>();
            if (health == null)
                return;

            health.TakeDamage(ContactDamage);
            other.GetComponent<RunnerController>()?.ApplyPhaseObstacleHit();
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
