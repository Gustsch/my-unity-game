using KnightRun.Core;
using KnightRun.Player;
using KnightRun.World;
using UnityEngine;

namespace KnightRun.Gameplay
{
    public class BossRootSnare : MonoBehaviour
    {
        public const float WarningDuration = 1.15f;
        public const float RootDuration = 2.4f;
        public const float SnareRadius = 1.35f;
        public const float DespawnBehindDistance = 16f;

        Transform player;
        Transform warningMark;
        Transform rootsVisual;
        RunnerController runner;
        GameManager gameManager;
        float warningTimer;
        bool hasTriggered;
        bool rootedApplied;

        public static BossRootSnare Spawn(Vector3 worldPosition)
        {
            var go = new GameObject("BossRootSnare");
            go.transform.position = worldPosition;

            var snare = go.AddComponent<BossRootSnare>();
            snare.BuildVisual();
            return snare;
        }

        void BuildVisual()
        {
            var mark = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mark.name = "WarningMark";
            mark.transform.SetParent(transform, false);
            mark.transform.localScale = new Vector3(SnareRadius * 2.2f, 0.06f, SnareRadius * 2.2f);
            mark.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            mark.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.TreeLeaves);
            ApplyTint(mark.GetComponent<Renderer>(), new Color(0.2f, 0.55f, 0.15f));
            Object.Destroy(mark.GetComponent<Collider>());
            warningMark = mark.transform;

            rootsVisual = new GameObject("Roots").transform;
            rootsVisual.SetParent(transform, false);
            rootsVisual.gameObject.SetActive(false);

            CreateRoot("RootA", new Vector3(-0.45f, 0.35f, 0.1f), new Vector3(0.28f, 0.7f, 0.28f), Quaternion.Euler(0f, 20f, 18f));
            CreateRoot("RootB", new Vector3(0.5f, 0.4f, -0.15f), new Vector3(0.32f, 0.85f, 0.32f), Quaternion.Euler(0f, -30f, -22f));
            CreateRoot("RootC", new Vector3(0.05f, 0.25f, 0.45f), new Vector3(0.25f, 0.55f, 0.25f), Quaternion.Euler(12f, 5f, 8f));
            CreateRoot("RootD", new Vector3(-0.1f, 0.3f, -0.5f), new Vector3(0.26f, 0.65f, 0.26f), Quaternion.Euler(-10f, 40f, -12f));

            warningTimer = WarningDuration;
            hasTriggered = false;
        }

        void CreateRoot(string name, Vector3 localPosition, Vector3 scale, Quaternion rotation)
        {
            var root = GameObject.CreatePrimitive(PrimitiveType.Cube);
            root.name = name;
            root.transform.SetParent(rootsVisual, false);
            root.transform.localPosition = localPosition;
            root.transform.localScale = scale;
            root.transform.localRotation = rotation;
            root.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.TreeTrunk);
            Object.Destroy(root.GetComponent<Collider>());
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

            if (!hasTriggered)
            {
                KeepWarningBesidePlayer();
                AnimateWarning();
                warningTimer -= Time.deltaTime;
                if (warningTimer <= 0f)
                    TriggerRoots();
            }
            else if (rootedApplied && runner != null && !runner.IsRooted)
            {
                Destroy(gameObject);
                return;
            }

            if (transform.position.z < player.position.z - DespawnBehindDistance)
                Destroy(gameObject);
        }

        void KeepWarningBesidePlayer()
        {
            Vector3 position = transform.position;
            position.z = player.position.z;
            transform.position = position;
        }

        void AnimateWarning()
        {
            if (warningMark == null)
                return;

            float pulse = 1f + Mathf.Sin(Time.time * 10f) * 0.2f;
            warningMark.localScale = new Vector3(SnareRadius * 2.2f * pulse, 0.06f, SnareRadius * 2.2f * pulse);

            var renderer = warningMark.GetComponent<Renderer>();
            if (renderer == null)
                return;

            float glow = 0.45f + Mathf.Sin(Time.time * 12f) * 0.35f;
            ApplyTint(renderer, new Color(0.15f, 0.35f + glow * 0.4f, 0.1f));
        }

        void TriggerRoots()
        {
            hasTriggered = true;
            if (warningMark != null)
                warningMark.gameObject.SetActive(false);
            if (rootsVisual != null)
                rootsVisual.gameObject.SetActive(true);

            ResolvePlayer();
            if (runner == null || player == null)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 delta = player.position - transform.position;
            delta.y = 0f;
            bool inZone = delta.sqrMagnitude <= SnareRadius * SnareRadius;
            if (inZone && runner.IsGrounded)
            {
                runner.ApplyRoot(RootDuration);
                rootedApplied = true;
            }
            else
            {
                Destroy(gameObject, 0.55f);
            }
        }

        void ResolvePlayer()
        {
            runner = FindFirstObjectByType<RunnerController>();
            if (runner != null)
                player = runner.transform;
        }
    }
}
