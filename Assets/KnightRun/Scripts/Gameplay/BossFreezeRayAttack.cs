using KnightRun.Core;
using KnightRun.Player;
using KnightRun.World;
using UnityEngine;

namespace KnightRun.Gameplay
{
    public class BossFreezeRayAttack : MonoBehaviour
    {
        public const float TelegraphDuration = 0.85f;
        public const float ChillDuration = 3.5f;
        public const float ChillMoveMultiplier = 0.28f;
        public const float RayLifetime = 1.4f;

        Transform boss;
        Transform player;
        Transform beam;
        Transform warningMark;
        RunnerController runner;
        GameManager gameManager;
        float telegraphTimer;
        float lifeTimer;
        bool hasFired;
        bool chillApplied;

        public static BossFreezeRayAttack Spawn(Transform bossTransform, Transform playerTransform)
        {
            var go = new GameObject("BossFreezeRay");
            go.transform.position = bossTransform.position;

            var attack = go.AddComponent<BossFreezeRayAttack>();
            attack.boss = bossTransform;
            attack.player = playerTransform;
            attack.BuildVisual();
            return attack;
        }

        void BuildVisual()
        {
            telegraphTimer = TelegraphDuration;
            lifeTimer = RayLifetime;

            var mark = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mark.name = "FreezeWarning";
            mark.transform.SetParent(transform, false);
            mark.transform.localScale = new Vector3(1.6f, 0.05f, 1.6f);
            mark.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.Stalactite);
            ApplyTint(mark.GetComponent<Renderer>(), new Color(0.45f, 0.9f, 1f));
            Object.Destroy(mark.GetComponent<Collider>());
            warningMark = mark.transform;

            var ray = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ray.name = "FreezeBeam";
            ray.transform.SetParent(transform, false);
            ray.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.Stalactite);
            ApplyTint(ray.GetComponent<Renderer>(), new Color(0.55f, 0.95f, 1f));
            Object.Destroy(ray.GetComponent<Collider>());
            beam = ray.transform;
            beam.gameObject.SetActive(false);
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

            if (player == null || boss == null)
            {
                ResolvePlayer();
                if (player == null || boss == null)
                {
                    Destroy(gameObject);
                    return;
                }
            }

            if (!hasFired)
            {
                UpdateTelegraph();
                telegraphTimer -= Time.deltaTime;
                if (telegraphTimer <= 0f)
                    FireRay();
                return;
            }

            UpdateBeam();
            if (!chillApplied && runner != null)
            {
                runner.ApplyChill(ChillDuration, ChillMoveMultiplier);
                chillApplied = true;
            }

            lifeTimer -= Time.deltaTime;
            if (lifeTimer <= 0f)
                Destroy(gameObject);
        }

        void UpdateTelegraph()
        {
            if (warningMark == null || player == null)
                return;

            warningMark.position = new Vector3(player.position.x, 0.06f, player.position.z);
            float pulse = 1f + Mathf.Sin(Time.time * 14f) * 0.22f;
            warningMark.localScale = new Vector3(1.6f * pulse, 0.05f, 1.6f * pulse);
        }

        void FireRay()
        {
            hasFired = true;
            if (warningMark != null)
                warningMark.gameObject.SetActive(false);
            if (beam != null)
                beam.gameObject.SetActive(true);
            UpdateBeam();
        }

        void UpdateBeam()
        {
            if (beam == null || boss == null || player == null)
                return;

            Vector3 start = boss.position + Vector3.up * 2.2f;
            Vector3 end = player.position + Vector3.up * 1.1f;
            Vector3 mid = (start + end) * 0.5f;
            float length = Vector3.Distance(start, end);

            beam.position = mid;
            beam.rotation = Quaternion.LookRotation((end - start).normalized, Vector3.up);
            beam.localScale = new Vector3(0.35f, 0.35f, Mathf.Max(0.5f, length));
        }

        void ResolvePlayer()
        {
            if (player == null)
            {
                runner = FindFirstObjectByType<RunnerController>();
                if (runner != null)
                    player = runner.transform;
            }
            else
            {
                runner = player.GetComponent<RunnerController>();
            }
        }
    }
}
