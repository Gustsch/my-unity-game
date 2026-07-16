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
        const float ForestScaleMin = 1.05f;
        const float ForestScaleMax = 1.35f;
        const float CaveRockTargetHeightMin = 1.25f;
        const float CaveRockTargetHeightMax = 1.7f;

        Transform player;
        GameManager gameManager;
        float contactCooldownTimer;

        public static PathTreeObstacle Spawn(Transform parent, Vector3 worldPosition, GameObject visualPrefab)
        {
            return Spawn(parent, worldPosition, visualPrefab, fitCaveRockSize: false);
        }

        public static PathTreeObstacle Spawn(
            Transform parent,
            Vector3 worldPosition,
            GameObject visualPrefab,
            bool fitCaveRockSize)
        {
            var go = new GameObject(fitCaveRockSize ? "CaveRockObstacle" : "ForestObstacle");
            go.transform.SetParent(parent, false);
            go.transform.position = worldPosition;

            var tree = go.AddComponent<PathTreeObstacle>();
            tree.BuildVisual(visualPrefab, fitCaveRockSize);
            return tree;
        }

        void BuildVisual(GameObject visualPrefab, bool fitCaveRockSize)
        {
            GameObject visual = SimpleNatureCatalog.InstantiateVisual(visualPrefab, transform);
            if (visual != null)
            {
                visual.name = visualPrefab.name;
                visual.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

                if (fitCaveRockSize)
                    FitVisualHeight(transform, visual, Random.Range(CaveRockTargetHeightMin, CaveRockTargetHeightMax));
                else
                    visual.transform.localScale = Vector3.one * Random.Range(ForestScaleMin, ForestScaleMax);
            }

            var hitbox = gameObject.AddComponent<BoxCollider>();
            hitbox.isTrigger = true;

            if (visual == null || !TryGetVisualBounds(visual, out Bounds bounds))
            {
                hitbox.size = fitCaveRockSize
                    ? new Vector3(1.1f, 1.35f, 1.1f)
                    : new Vector3(0.85f, 1.1f, 0.85f);
                hitbox.center = new Vector3(0f, hitbox.size.y * 0.5f, 0f);
                return;
            }

            Vector3 localCenter = transform.InverseTransformPoint(bounds.center);
            if (fitCaveRockSize)
            {
                hitbox.size = new Vector3(
                    Mathf.Clamp(bounds.size.x, 0.95f, 2.2f),
                    Mathf.Clamp(bounds.size.y, 1.1f, 2.0f),
                    Mathf.Clamp(bounds.size.z, 0.95f, 2.2f));
            }
            else
            {
                hitbox.size = new Vector3(
                    Mathf.Clamp(bounds.size.x, 0.7f, 1.6f),
                    Mathf.Clamp(bounds.size.y, 0.65f, 1.8f),
                    Mathf.Clamp(bounds.size.z, 0.7f, 1.6f));
            }

            hitbox.center = new Vector3(localCenter.x, hitbox.size.y * 0.5f, localCenter.z);
        }

        static void FitVisualHeight(Transform obstacleRoot, GameObject visual, float targetHeight)
        {
            if (!TryGetVisualBounds(visual, out Bounds bounds))
            {
                visual.transform.localScale = Vector3.one * targetHeight;
                return;
            }

            float currentHeight = Mathf.Max(0.01f, bounds.size.y);
            float scale = targetHeight / currentHeight;
            visual.transform.localScale = Vector3.one * scale;

            // Keep the rock sitting on the ground after rescale.
            if (!TryGetVisualBounds(visual, out Bounds scaledBounds))
                return;

            float deltaY = obstacleRoot.position.y - scaledBounds.min.y;
            visual.transform.position += Vector3.up * deltaY;
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
