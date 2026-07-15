using KnightRun.Core;
using KnightRun.Player;
using KnightRun.World;
using UnityEngine;

namespace KnightRun.Gameplay
{
    public class MineHoleObstacle : MonoBehaviour
    {
        public const int ContactDamage = 300;
        public const float DespawnBehindDistance = 12f;

        const float ContactCooldown = 0.6f;
        const float HoleWidth = 1.5f;
        const float HoleLength = 2.4f;

        Transform player;
        GameManager gameManager;
        float contactCooldownTimer;

        public static MineHoleObstacle Spawn(Transform parent, Vector3 worldPosition)
        {
            var go = new GameObject("MineHole");
            go.transform.SetParent(parent, false);
            go.transform.position = worldPosition;

            var hole = go.AddComponent<MineHoleObstacle>();
            hole.BuildVisual();
            return hole;
        }

        void BuildVisual()
        {
            var pit = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pit.name = "Pit";
            pit.transform.SetParent(transform, false);
            pit.transform.localScale = new Vector3(HoleWidth, 0.12f, HoleLength);
            pit.transform.localPosition = new Vector3(0f, -0.04f, 0f);
            pit.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.WallCave);
            ApplyTint(pit.GetComponent<Renderer>(), new Color(0.05f, 0.04f, 0.03f));
            Object.Destroy(pit.GetComponent<Collider>());

            CreateEdge("EdgeLeft", new Vector3(-(HoleWidth * 0.5f + 0.08f), 0.06f, 0f), new Vector3(0.16f, 0.12f, HoleLength + 0.2f));
            CreateEdge("EdgeRight", new Vector3(HoleWidth * 0.5f + 0.08f, 0.06f, 0f), new Vector3(0.16f, 0.12f, HoleLength + 0.2f));
            CreateEdge("EdgeFront", new Vector3(0f, 0.06f, HoleLength * 0.5f + 0.08f), new Vector3(HoleWidth + 0.2f, 0.12f, 0.16f));
            CreateEdge("EdgeBack", new Vector3(0f, 0.06f, -(HoleLength * 0.5f + 0.08f)), new Vector3(HoleWidth + 0.2f, 0.12f, 0.16f));

            var hitbox = gameObject.AddComponent<BoxCollider>();
            hitbox.isTrigger = true;
            hitbox.size = new Vector3(HoleWidth * 0.85f, 0.9f, HoleLength * 0.75f);
            hitbox.center = new Vector3(0f, 0.35f, 0f);
        }

        void CreateEdge(string name, Vector3 localPosition, Vector3 scale)
        {
            var edge = GameObject.CreatePrimitive(PrimitiveType.Cube);
            edge.name = name;
            edge.transform.SetParent(transform, false);
            edge.transform.localScale = scale;
            edge.transform.localPosition = localPosition;
            edge.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.MineRail);
            Object.Destroy(edge.GetComponent<Collider>());
        }

        static void ApplyTint(Renderer renderer, Color color)
        {
            if (renderer == null)
                return;

            Material material = renderer.material;
            material.color = color;
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
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
