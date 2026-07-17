using KnightRun.Core;
using KnightRun.Player;
using KnightRun.World;
using UnityEngine;

namespace KnightRun.Gameplay
{
    public class VolcanoTrenchObstacle : MonoBehaviour
    {
        public const int ContactDamage = 500;
        public const float DespawnBehindDistance = 14f;

        const float ContactCooldown = 0.6f;
        const float TrenchLength = 2.8f;
        const float EdgeThickness = 0.18f;

        Transform player;
        GameManager gameManager;
        float contactCooldownTimer;
        float trenchWidth;

        public static VolcanoTrenchObstacle Spawn(Transform parent, float z, RunPhaseSettings settings)
        {
            var go = new GameObject("VolcanoTrench");
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(0f, 0f, z);

            var trench = go.AddComponent<VolcanoTrenchObstacle>();
            trench.trenchWidth = PhaseTrackLayout.GetGroundWidth(settings) - 0.4f;
            trench.BuildVisual();

            // Fire column sits on a random X across the playable track (not always center).
            const float edgeMargin = 1.1f;
            float minX = PhaseTrackLayout.GetPlayableMinX(settings) + edgeMargin;
            float maxX = PhaseTrackLayout.GetPlayableMaxX(settings) - edgeMargin;
            float columnX = Random.Range(minX, maxX);
            FireColumnHazard.Spawn(go.transform, new Vector3(columnX, 0f, z));
            return trench;
        }

        void BuildVisual()
        {
            var pit = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pit.name = "LavaTrench";
            pit.transform.SetParent(transform, false);
            pit.transform.localScale = new Vector3(trenchWidth, 0.14f, TrenchLength);
            pit.transform.localPosition = new Vector3(0f, -0.05f, 0f);
            pit.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.LavaPool);
            Object.Destroy(pit.GetComponent<Collider>());

            CreateEdge("EdgeFront", new Vector3(0f, 0.08f, TrenchLength * 0.5f + EdgeThickness * 0.5f),
                new Vector3(trenchWidth + 0.3f, 0.16f, EdgeThickness));
            CreateEdge("EdgeBack", new Vector3(0f, 0.08f, -(TrenchLength * 0.5f + EdgeThickness * 0.5f)),
                new Vector3(trenchWidth + 0.3f, 0.16f, EdgeThickness));

            var hitbox = gameObject.AddComponent<BoxCollider>();
            hitbox.isTrigger = true;
            hitbox.size = new Vector3(trenchWidth * 0.95f, 1f, TrenchLength * 0.7f);
            hitbox.center = new Vector3(0f, 0.4f, 0f);
        }

        void CreateEdge(string name, Vector3 localPosition, Vector3 scale)
        {
            var edge = GameObject.CreatePrimitive(PrimitiveType.Cube);
            edge.name = name;
            edge.transform.SetParent(transform, false);
            edge.transform.localScale = scale;
            edge.transform.localPosition = localPosition;
            edge.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.VolcanoRock);
            Object.Destroy(edge.GetComponent<Collider>());
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

            var runner = other.GetComponent<RunnerController>();
            if (runner == null || !runner.IsGrounded)
                return;

            KnightHealth health = other.GetComponent<KnightHealth>();
            if (health == null)
                return;

            health.TakeDamage(ContactDamage);
            runner.ApplyPhaseObstacleHit();
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
