using KnightRun.Core;
using KnightRun.Player;
using KnightRun.World;
using UnityEngine;

namespace KnightRun.Gameplay
{
    public class FreezePickup : MonoBehaviour
    {
        public const float PickupRadius = 1.1f;
        public const float DespawnBehindDistance = 12f;
        public const float FloatHeight = 1f;
        public const float FreezeDuration = 3f;

        static readonly Color PickupColor = new Color(0.25f, 0.55f, 1f);

        Transform meshTransform;
        Transform player;

        public static void Spawn(Vector3 position)
        {
            var go = new GameObject("FreezePickup");
            go.transform.position = new Vector3(position.x, 0f, position.z);
            go.AddComponent<FreezePickup>().Build();
        }

        void Build()
        {
            var mesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mesh.name = "FreezeMesh";
            mesh.transform.SetParent(transform, false);
            mesh.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
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
                meshTransform.Rotate(Vector3.up, 120f * Time.deltaTime, Space.World);

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

            EnemyFreezeController.FreezeAll(FreezeDuration);
            Destroy(gameObject);
        }
    }
}
