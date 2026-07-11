using KnightRun.Core;
using KnightRun.Player;
using KnightRun.World;
using UnityEngine;

namespace KnightRun.Gameplay
{
    public class FoodPickup : MonoBehaviour
    {
        public const float PickupRadius = 1.1f;
        public const float DespawnBehindDistance = 12f;
        public const float FloatHeight = 1f;
        public const int HealAmount = 150;

        static readonly Color PickupColor = new Color(0.58f, 0.36f, 0.14f);

        Transform meshTransform;
        Transform player;

        public static void Spawn(Vector3 position)
        {
            var go = new GameObject("FoodPickup");
            go.transform.position = new Vector3(position.x, 0f, position.z);
            go.AddComponent<FoodPickup>().Build();
        }

        void Build()
        {
            var mesh = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            mesh.name = "FoodMesh";
            mesh.transform.SetParent(transform, false);
            mesh.transform.localScale = Vector3.one * 0.5f;
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
            {
                float pulse = 1f + Mathf.Sin(Time.time * 5f) * 0.08f;
                meshTransform.localScale = Vector3.one * 0.5f * pulse;
            }

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

            other.GetComponent<KnightHealth>()?.Heal(HealAmount);
            Destroy(gameObject);
        }
    }
}
