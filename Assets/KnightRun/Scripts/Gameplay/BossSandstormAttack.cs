using KnightRun.Core;
using KnightRun.Player;
using KnightRun.World;
using UnityEngine;

namespace KnightRun.Gameplay
{
    public class BossSandstormAttack : MonoBehaviour
    {
        public const float StormDuration = 4f;
        public const float SpineWarning = 0.9f;
        public const int SpineDamage = 300;
        public const int SpineCount = 3;
        public const float PhaseSpineInterval = 1.35f;

        public bool IsPhaseStorm { get; private set; }

        Transform player;
        GameManager gameManager;
        float lifeTimer;
        float spineTimer;
        float lockedX;
        int spinesSpawned;
        Transform[] fogPanels;

        public static BossSandstormAttack Spawn(Transform player)
        {
            var go = new GameObject("BossSandstorm");
            go.transform.position = player.position;

            var attack = go.AddComponent<BossSandstormAttack>();
            attack.Build(player, phaseStorm: false);
            return attack;
        }

        public static BossSandstormAttack EnsurePhaseStorm(Transform player)
        {
            BossSandstormAttack existing = FindFirstObjectByType<BossSandstormAttack>();
            if (existing != null)
            {
                existing.MakePhaseStorm(player);
                return existing;
            }

            var go = new GameObject("DesertSandstorm");
            go.transform.position = player.position;

            var attack = go.AddComponent<BossSandstormAttack>();
            attack.Build(player, phaseStorm: true);
            return attack;
        }

        public static void StopPhaseStorm()
        {
            foreach (BossSandstormAttack attack in FindObjectsByType<BossSandstormAttack>(FindObjectsSortMode.None))
            {
                if (attack != null)
                    Object.Destroy(attack.gameObject);
            }
        }

        void Build(Transform playerTransform, bool phaseStorm)
        {
            player = playerTransform;
            IsPhaseStorm = phaseStorm;
            lockedX = player.position.x;
            lifeTimer = phaseStorm ? float.PositiveInfinity : StormDuration;
            spineTimer = phaseStorm ? PhaseSpineInterval * 0.5f : SpineWarning;
            spinesSpawned = 0;
            fogPanels = new Transform[4];

            for (int i = 0; i < fogPanels.Length; i++)
            {
                var panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                panel.name = $"SandFog_{i}";
                panel.transform.SetParent(transform, false);
                panel.transform.localScale = new Vector3(3.2f, 2.8f, 0.4f);
                panel.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.GroundVolcano);
                ApplyTint(panel.GetComponent<Renderer>(), new Color(0.72f, 0.55f, 0.28f, 0.7f));
                Object.Destroy(panel.GetComponent<Collider>());
                fogPanels[i] = panel.transform;
            }
        }

        void MakePhaseStorm(Transform playerTransform)
        {
            if (playerTransform != null)
                player = playerTransform;

            IsPhaseStorm = true;
            lifeTimer = float.PositiveInfinity;
            if (spineTimer <= 0f)
                spineTimer = PhaseSpineInterval;
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
        }

        void Update()
        {
            if (gameManager == null || gameManager.State != GameState.Running)
                return;

            if (player == null)
            {
                var runner = FindFirstObjectByType<RunnerController>();
                if (runner == null)
                {
                    Destroy(gameObject);
                    return;
                }

                player = runner.transform;
            }

            UpdateFog();
            UpdateSpines();

            if (IsPhaseStorm)
                return;

            lifeTimer -= Time.deltaTime;
            if (lifeTimer <= 0f)
                Destroy(gameObject);
        }

        void UpdateFog()
        {
            float z = player.position.z + 4f;
            float sway = Mathf.Sin(Time.time * 2.4f) * 0.4f;
            float[] xs = { -5.5f + sway, -2.2f - sway * 0.5f, 2.2f + sway * 0.5f, 5.5f - sway };
            for (int i = 0; i < fogPanels.Length; i++)
            {
                if (fogPanels[i] == null)
                    continue;
                fogPanels[i].position = new Vector3(player.position.x + xs[i], 1.4f, z + i * 0.35f);
            }
        }

        void UpdateSpines()
        {
            if (!IsPhaseStorm && spinesSpawned >= SpineCount)
                return;

            spineTimer -= Time.deltaTime;
            if (spineTimer > 0f)
                return;

            SpawnSpine(lockedX);
            if (!IsPhaseStorm)
                spinesSpawned++;

            lockedX = player.position.x;
            spineTimer = IsPhaseStorm ? PhaseSpineInterval : SpineWarning;
        }

        void SpawnSpine(float targetX)
        {
            float z = player.position.z + 10f;
            BossDesertSpine.Spawn(new Vector3(targetX, 0.05f, player.position.z), new Vector3(targetX, 3.5f, z));
        }
    }

    public class BossDesertSpine : MonoBehaviour
    {
        Transform warningMark;
        Transform spine;
        Transform player;
        GameManager gameManager;
        float warningTimer = 0.75f;
        bool falling;
        float fallProgress;
        Vector3 start;
        Vector3 end;
        bool hasHit;

        public static void Spawn(Vector3 impactPoint, Vector3 spawnPoint)
        {
            var go = new GameObject("BossDesertSpine");
            go.transform.position = impactPoint;

            var spineAttack = go.AddComponent<BossDesertSpine>();
            spineAttack.Build(impactPoint, spawnPoint);
        }

        void Build(Vector3 impactPoint, Vector3 spawnPoint)
        {
            start = spawnPoint;
            end = impactPoint + Vector3.up * 0.7f;

            var mark = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mark.name = "SpineWarning";
            mark.transform.SetParent(transform, false);
            mark.transform.position = impactPoint;
            mark.transform.localScale = new Vector3(1.3f, 0.05f, 1.3f);
            mark.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.RockObstacle);
            ApplyTint(mark.GetComponent<Renderer>(), new Color(0.9f, 0.7f, 0.2f));
            Object.Destroy(mark.GetComponent<Collider>());
            warningMark = mark.transform;

            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Spine";
            body.transform.SetParent(transform, false);
            body.transform.position = start;
            body.transform.localScale = new Vector3(0.35f, 1.4f, 0.35f);
            body.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.RockObstacle);
            ApplyTint(body.GetComponent<Renderer>(), new Color(0.55f, 0.38f, 0.18f));
            Object.Destroy(body.GetComponent<Collider>());
            spine = body.transform;
            spine.gameObject.SetActive(false);
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
            var runner = FindFirstObjectByType<RunnerController>();
            if (runner != null)
                player = runner.transform;
        }

        void Update()
        {
            if (gameManager == null || gameManager.State != GameState.Running)
                return;

            if (!falling)
            {
                float pulse = 1f + Mathf.Sin(Time.time * 12f) * 0.2f;
                if (warningMark != null)
                    warningMark.localScale = new Vector3(1.3f * pulse, 0.05f, 1.3f * pulse);

                warningTimer -= Time.deltaTime;
                if (warningTimer <= 0f)
                {
                    falling = true;
                    if (warningMark != null)
                        warningMark.gameObject.SetActive(false);
                    if (spine != null)
                        spine.gameObject.SetActive(true);
                }

                return;
            }

            fallProgress = Mathf.MoveTowards(fallProgress, 1f, Time.deltaTime * 2.8f);
            if (spine != null)
                spine.position = Vector3.Lerp(start, end, fallProgress);

            if (fallProgress >= 1f)
            {
                TryHit();
                Destroy(gameObject, 0.25f);
            }
        }

        void TryHit()
        {
            if (hasHit || player == null)
                return;

            Vector3 delta = player.position - end;
            delta.y = 0f;
            if (delta.sqrMagnitude > 1.15f * 1.15f)
                return;

            var runner = player.GetComponent<RunnerController>();
            if (runner != null && !runner.IsGrounded)
                return;

            KnightHealth health = player.GetComponent<KnightHealth>();
            health?.TakeDamage(BossSandstormAttack.SpineDamage);
            hasHit = true;
        }
    }
}
