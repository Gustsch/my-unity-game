using KnightRun.Core;
using KnightRun.Player;
using KnightRun.World;
using UnityEngine;

namespace KnightRun.Gameplay
{
    public class CoinBagPickup : MonoBehaviour
    {
        public const float PickupRadius = 1.1f;
        public const float DespawnBehindDistance = 12f;
        public const float FloatHeight = 1f;
        public const int MinCoins = 10;
        public const int MaxCoins = 50;

        static readonly Color PickupColor = new Color(1f, 0.88f, 0.15f);

        int coinAmount;
        Transform meshTransform;
        Transform player;

        public static void Spawn(Vector3 position, int coins)
        {
            var go = new GameObject("CoinBagPickup");
            go.transform.position = new Vector3(position.x, 0f, position.z);
            go.AddComponent<CoinBagPickup>().Build(Mathf.Max(1, coins));
        }

        public static void SpawnRandom(Vector3 position)
        {
            int coins = Random.Range(MinCoins, MaxCoins + 1);
            Spawn(position, coins);
        }

        void Build(int coins)
        {
            coinAmount = coins;

            var mesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mesh.name = "CoinBagMesh";
            mesh.transform.SetParent(transform, false);
            mesh.transform.localScale = new Vector3(0.5f, 0.42f, 0.34f);
            mesh.transform.localPosition = Vector3.up * FloatHeight;
            Destroy(mesh.GetComponent<Collider>());

            var renderer = mesh.GetComponent<Renderer>();
            renderer.sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.Coin);
            Material material = renderer.material;
            material.color = PickupColor;
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", PickupColor);

            meshTransform = mesh.transform;

            var collider = gameObject.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = PickupRadius;
            collider.center = Vector3.up * FloatHeight;
        }

        void Start()
        {
            ResolvePlayer();
        }

        void Update()
        {
            GameManager gameManager = GameManager.Instance;
            if (gameManager == null || gameManager.State != GameState.Running)
                return;

            if (player == null)
                ResolvePlayer();

            if (meshTransform != null)
                meshTransform.Rotate(Vector3.up, 90f * Time.deltaTime, Space.World);

            if (player != null && transform.position.z < player.position.z - DespawnBehindDistance)
                Destroy(gameObject);
        }

        void ResolvePlayer()
        {
            var runner = FindFirstObjectByType<RunnerController>();
            if (runner != null)
                player = runner.transform;
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            GameManager.Instance?.AddCoin(coinAmount);
            Destroy(gameObject);
        }
    }
}
