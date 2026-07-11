using KnightRun.Core;
using KnightRun.Player;
using KnightRun.Progression;
using KnightRun.World;
using UnityEngine;

namespace KnightRun.Gameplay
{
    public class ExperienceOrb : MonoBehaviour
    {
        public const int DefaultValue = 1;
        public const int EliteValue = 2;

        public static int GetBossLevelXpValue()
        {
            return UpgradeManager.Instance != null
                ? Mathf.Max(1, UpgradeManager.Instance.XpRequiredForNextLevel)
                : 1;
        }
        public const float PickupRadius = 1.4f;
        public const float MagnetRadius = 1.75f;
        public const float DespawnBehindDistance = 12f;
        public const float FloatHeight = 1f;
        public const float PulseSpeed = 4f;
        public const float PulseAmount = 0.12f;
        public const float SpinSpeed = 140f;

        static readonly Color OrbColor = new Color(0.55f, 0.92f, 1f);

        int value;
        Transform meshTransform;
        float pulseTimer;
        Transform player;

        public static void Spawn(Vector3 position, int xpValue = DefaultValue)
        {
            var go = new GameObject("ExperienceOrb");
            go.transform.position = new Vector3(position.x, 0f, position.z);

            var orb = go.AddComponent<ExperienceOrb>();
            orb.Initialize(Mathf.Max(1, xpValue));
        }

        public void Initialize(int xpValue)
        {
            value = xpValue;

            var mesh = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            mesh.name = "OrbMesh";
            mesh.transform.SetParent(transform, false);
            mesh.transform.localScale = Vector3.one * 0.45f;
            mesh.transform.localPosition = Vector3.up * FloatHeight;
            Destroy(mesh.GetComponent<Collider>());

            var renderer = mesh.GetComponent<Renderer>();
            renderer.sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.Coin);
            Material material = renderer.material;
            material.color = OrbColor;
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", OrbColor);
            if (material.HasProperty("_EmissionColor"))
                material.SetColor("_EmissionColor", OrbColor * 1.4f);

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

            pulseTimer += Time.deltaTime;
            if (meshTransform != null)
            {
                float pulse = 1f + Mathf.Sin(pulseTimer * PulseSpeed) * PulseAmount;
                meshTransform.localScale = Vector3.one * 0.45f * pulse;
                meshTransform.Rotate(Vector3.up, SpinSpeed * Time.deltaTime, Space.World);
            }

            if (player != null && transform.position.z < player.position.z - DespawnBehindDistance)
                Destroy(gameObject);

            if (player != null)
                TryMagnetPickup();
        }

        void TryMagnetPickup()
        {
            Vector3 orbCenter = transform.position + Vector3.up * FloatHeight;
            Vector3 playerCenter = player.position + Vector3.up * 1f;
            if (Vector3.Distance(orbCenter, playerCenter) <= MagnetRadius)
                Collect();
        }

        void Collect()
        {
            UpgradeManager.Instance?.CollectXp(value);
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

            Collect();
        }
    }
}
