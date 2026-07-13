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

        public static PathTreeObstacle Spawn(Transform parent, Vector3 worldPosition)
        {
            var go = new GameObject("PathTree");
            go.transform.SetParent(parent, false);
            go.transform.position = worldPosition;

            var tree = go.AddComponent<PathTreeObstacle>();
            tree.BuildVisual();
            return tree;
        }

        void BuildVisual()
        {
            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trunk.name = "Trunk";
            trunk.transform.SetParent(transform, false);
            trunk.transform.localScale = new Vector3(0.55f, 1.35f, 0.55f);
            trunk.transform.localPosition = new Vector3(0f, 0.675f, 0f);
            trunk.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.TreeTrunk);
            Object.Destroy(trunk.GetComponent<Collider>());

            var top = GameObject.CreatePrimitive(PrimitiveType.Cube);
            top.name = "Leaves";
            top.transform.SetParent(transform, false);
            top.transform.localScale = new Vector3(1.35f, 1.2f, 1.35f);
            top.transform.localPosition = new Vector3(0f, 1.85f, 0f);
            top.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.TreeLeaves);
            Object.Destroy(top.GetComponent<Collider>());

            var hitbox = gameObject.AddComponent<BoxCollider>();
            hitbox.isTrigger = true;
            hitbox.size = new Vector3(0.75f, 1.5f, 0.75f);
            hitbox.center = new Vector3(0f, 0.75f, 0f);
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
