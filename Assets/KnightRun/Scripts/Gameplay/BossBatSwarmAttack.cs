using KnightRun.Core;
using KnightRun.Player;
using KnightRun.World;
using UnityEngine;

namespace KnightRun.Gameplay
{
    public class BossBatSwarmAttack : MonoBehaviour
    {
        public const float WarningDuration = 1.1f;
        public const int ContactDamage = 250;
        public const int DiveCount = 5;

        Transform player;
        GameManager gameManager;
        float warningTimer;
        bool hasTriggered;
        bool hasHitPlayer;
        DiveBat[] bats;
        Transform[] warningMarks;

        struct DiveBat
        {
            public Transform transform;
            public float laneX;
            public float progress;
            public bool active;
        }

        public static BossBatSwarmAttack Spawn(Transform player)
        {
            var go = new GameObject("BossBatSwarm");
            go.transform.position = player.position;

            var attack = go.AddComponent<BossBatSwarmAttack>();
            attack.Build(player);
            return attack;
        }

        void Build(Transform playerTransform)
        {
            player = playerTransform;
            warningTimer = WarningDuration;
            float minX = RunnerController.TrackMinX;
            float maxX = RunnerController.TrackMaxX;

            bats = new DiveBat[DiveCount];
            warningMarks = new Transform[DiveCount];

            for (int i = 0; i < DiveCount; i++)
            {
                float t = DiveCount == 1 ? 0.5f : i / (float)(DiveCount - 1);
                float x = Mathf.Lerp(minX, maxX, t);

                var mark = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                mark.name = $"BatWarning_{i}";
                mark.transform.SetParent(transform, false);
                mark.transform.localScale = new Vector3(1.4f, 0.06f, 1.4f);
                mark.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.Enemy);
                ApplyTint(mark.GetComponent<Renderer>(), new Color(0.55f, 0.15f, 0.7f));
                Object.Destroy(mark.GetComponent<Collider>());
                warningMarks[i] = mark.transform;

                var bat = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bat.name = $"DiveBat_{i}";
                bat.transform.SetParent(transform, false);
                bat.transform.localScale = new Vector3(0.55f, 0.4f, 0.65f);
                bat.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.Enemy);
                ApplyTint(bat.GetComponent<Renderer>(), new Color(0.18f, 0.1f, 0.24f));
                Object.Destroy(bat.GetComponent<Collider>());
                bat.SetActive(false);

                bats[i] = new DiveBat
                {
                    transform = bat.transform,
                    laneX = x,
                    progress = 0f,
                    active = false
                };
            }

            SyncToPlayerLine();
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
                return;

            // Mantem o ataque na linha do heroi para correr pra frente nao escapar.
            SyncToPlayerLine();

            if (!hasTriggered)
            {
                PulseWarnings();
                warningTimer -= Time.deltaTime;
                if (warningTimer <= 0f)
                    TriggerDives();
                return;
            }

            bool anyActive = false;
            for (int i = 0; i < bats.Length; i++)
            {
                if (!bats[i].active)
                    continue;

                anyActive = true;
                bats[i].progress = Mathf.MoveTowards(bats[i].progress, 1f, Time.deltaTime * 2.4f);
                bats[i].transform.position = Vector3.Lerp(GetDiveStart(i), GetDiveTarget(i), bats[i].progress);

                if (bats[i].progress >= 1f)
                {
                    TryHitAt(GetDiveTarget(i));
                    bats[i].active = false;
                    bats[i].transform.gameObject.SetActive(false);
                }
            }

            if (!anyActive)
                Destroy(gameObject, 0.2f);
        }

        void SyncToPlayerLine()
        {
            float z = player.position.z;
            transform.position = new Vector3(0f, 0f, z);

            for (int i = 0; i < DiveCount; i++)
            {
                if (warningMarks[i] != null && warningMarks[i].gameObject.activeSelf)
                    warningMarks[i].position = new Vector3(bats[i].laneX, 0.05f, z);

                if (bats[i].active && bats[i].transform != null)
                    bats[i].transform.position = Vector3.Lerp(GetDiveStart(i), GetDiveTarget(i), bats[i].progress);
            }
        }

        Vector3 GetDiveStart(int index)
        {
            return new Vector3(bats[index].laneX, 3.4f, player.position.z + 3.5f);
        }

        Vector3 GetDiveTarget(int index)
        {
            return new Vector3(bats[index].laneX, 0.9f, player.position.z);
        }

        void PulseWarnings()
        {
            float pulse = 1f + Mathf.Sin(Time.time * 12f) * 0.18f;
            for (int i = 0; i < warningMarks.Length; i++)
            {
                if (warningMarks[i] == null)
                    continue;
                warningMarks[i].localScale = new Vector3(1.4f * pulse, 0.06f, 1.4f * pulse);
            }
        }

        void TriggerDives()
        {
            hasTriggered = true;
            for (int i = 0; i < warningMarks.Length; i++)
            {
                if (warningMarks[i] != null)
                    warningMarks[i].gameObject.SetActive(false);
            }

            for (int i = 0; i < bats.Length; i++)
            {
                bats[i].active = true;
                bats[i].progress = 0f;
                bats[i].transform.position = GetDiveStart(i);
                bats[i].transform.gameObject.SetActive(true);
            }
        }

        void TryHitAt(Vector3 hitPoint)
        {
            if (hasHitPlayer || player == null)
                return;

            var runner = player.GetComponent<RunnerController>();
            if (runner == null || runner.IsSliding)
                return;

            Vector3 delta = player.position - hitPoint;
            delta.y = 0f;
            if (delta.sqrMagnitude > 1.1f * 1.1f)
                return;

            KnightHealth health = player.GetComponent<KnightHealth>();
            if (health == null)
                return;

            health.TakeDamage(ContactDamage);
            hasHitPlayer = true;
        }
    }
}
