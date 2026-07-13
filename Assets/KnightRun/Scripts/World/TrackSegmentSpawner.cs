using System.Collections.Generic;
using KnightRun.Core;
using KnightRun.Gameplay;
using KnightRun.Player;
using UnityEngine;

namespace KnightRun.World
{
    public class TrackSegmentSpawner : MonoBehaviour
    {
        [SerializeField] int segmentsAhead = 6;
        [SerializeField] float segmentLength = 20f;

        const int MinEnemiesPerSegment = 5;
        const int MaxEnemiesPerSegment = 8;
        const float ContinuousSpawnInterval = 0.45f;
        const float MinEnemySpawnAheadOfPlayer = 34f;
        const float ContinuousSpawnMinAhead = 34f;
        const float ContinuousSpawnMaxAhead = 52f;

        readonly Queue<TrackSegment> activeSegments = new Queue<TrackSegment>();
        RunnerController player;
        RunPhaseManager phaseManager;
        float nextSpawnZ;
        float continuousSpawnTimer;
        int segmentCounter;

        void Start()
        {
            player = FindFirstObjectByType<RunnerController>();
            phaseManager = RunPhaseManager.Instance;

            if (phaseManager != null)
                phaseManager.OnPhaseChanged += HandlePhaseChanged;

            nextSpawnZ = 0f;
            for (int i = 0; i < segmentsAhead; i++)
                SpawnSegment();
        }

        void OnDestroy()
        {
            if (phaseManager != null)
                phaseManager.OnPhaseChanged -= HandlePhaseChanged;
        }

        void Update()
        {
            if (player == null)
                return;

            while (player.transform.position.z + segmentLength * 2f > nextSpawnZ)
                SpawnSegment();

            UpdateContinuousSpawn();

            if (activeSegments.Count == 0)
                return;

            TrackSegment oldest = activeSegments.Peek();
            if (player.transform.position.z - segmentLength > oldest.transform.position.z + segmentLength)
                RecycleSegment();
        }

        void UpdateContinuousSpawn()
        {
            if (GameManager.Instance == null || GameManager.Instance.State != GameState.Running)
                return;

            if (ShouldBlockEnemySpawns())
                return;

            RunPhaseSettings settings = GetCurrentSettings();
            float spawnMultiplier = GetEnemySpawnMultiplier(settings);

            continuousSpawnTimer -= Time.deltaTime;
            while (continuousSpawnTimer <= 0f)
            {
                continuousSpawnTimer += ContinuousSpawnInterval;
                if (Random.value <= spawnMultiplier)
                    SpawnEnemyAheadOfPlayer();
            }
        }

        static bool ShouldBlockEnemySpawns()
        {
            PhaseBossController bossController = PhaseBossController.Instance;
            return bossController != null && bossController.ShouldBlockEnemySpawns;
        }

        static float GetEnemySpawnMultiplier(RunPhaseSettings settings)
        {
            return Mathf.Clamp(settings.enemySpawnMultiplier, 0f, 1f);
        }

        RunPhaseSettings GetCurrentSettings()
        {
            return phaseManager != null
                ? phaseManager.CurrentSettings
                : RunPhaseDefaults.All[0];
        }

        void SpawnSegment()
        {
            var settings = phaseManager != null
                ? phaseManager.CurrentSettings
                : RunPhaseDefaults.All[0];

            var segmentGo = new GameObject($"Segment_{segmentCounter++}");
            segmentGo.transform.SetParent(transform, false);
            var segment = segmentGo.AddComponent<TrackSegment>();
            segment.Length = segmentLength;
            segment.Build(settings, nextSpawnZ);

            if (segmentCounter > 2)
                PopulateSegment(segment, settings);

            activeSegments.Enqueue(segment);
            nextSpawnZ += segmentLength;
        }

        void PopulateSegment(TrackSegment segment, RunPhaseSettings settings)
        {
            if (ShouldBlockEnemySpawns())
                return;

            var contentRoot = new GameObject("Content").transform;
            contentRoot.SetParent(segment.transform, false);

            float spawnMultiplier = GetEnemySpawnMultiplier(settings);
            int enemyCount = Random.Range(MinEnemiesPerSegment, MaxEnemiesPerSegment + 1);
            for (int i = 0; i < enemyCount; i++)
            {
                if (Random.value > spawnMultiplier)
                    continue;

                if (!TryRollEnemySpawnZ(segment, out float spawnZ))
                    break;

                SpawnEnemyAt(settings, GetSpawnX(settings), spawnZ);
            }

            if (Random.value < 0.65f)
                SpawnCoinLine(contentRoot, settings);
        }

        static float GetSpawnX(RunPhaseSettings settings)
        {
            if (settings.useLaneMovement)
                return LanePositions[Random.Range(0, LanePositions.Length)];

            return Random.Range(PhaseTrackLayout.GetPlayableMinX(settings), PhaseTrackLayout.GetPlayableMaxX(settings));
        }

        static float[] LanePositions => RunnerController.LanePositions;

        void SpawnEnemyAheadOfPlayer()
        {
            RunPhaseSettings settings = GetCurrentSettings();
            float spawnZ = player.transform.position.z + Random.Range(ContinuousSpawnMinAhead, ContinuousSpawnMaxAhead);
            SpawnEnemyAt(settings, GetSpawnX(settings), spawnZ);
        }

        bool TryRollEnemySpawnZ(TrackSegment segment, out float spawnZ)
        {
            spawnZ = 0f;
            if (player == null)
                return false;

            float minZ = player.transform.position.z + MinEnemySpawnAheadOfPlayer;
            float segmentMinZ = segment.transform.position.z + 2f;
            float segmentMaxZ = segment.transform.position.z + segmentLength - 1f;
            float spawnMinZ = Mathf.Max(segmentMinZ, minZ);

            if (spawnMinZ >= segmentMaxZ)
                return false;

            spawnZ = Random.Range(spawnMinZ, segmentMaxZ);
            return true;
        }

        void SpawnEnemyAt(RunPhaseSettings settings, float x, float z)
        {
            var enemyGo = new GameObject("Enemy");
            enemyGo.transform.SetParent(transform, false);
            enemyGo.transform.position = new Vector3(x, 0f, z);

            var enemy = enemyGo.AddComponent<Enemy>();
            enemy.Build();

            int health = EnemyCombatStats.RollHealthForPhase(settings);
            bool isElite = Random.value < EnemyCombatStats.EliteSpawnChance;

            if (isElite)
                health *= EnemyCombatStats.EliteHealthMultiplier;

            int damage = EnemyCombatStats.GetContactDamageForHealth(health);
            enemy.Initialize(health, damage, isElite);
        }

        void SpawnCoinLine(Transform parent, RunPhaseSettings settings)
        {
            float spawnX = GetSpawnX(settings);
            for (int i = 0; i < 3; i++)
            {
                var coinGo = new GameObject("Coin");
                coinGo.transform.SetParent(parent, false);
                coinGo.transform.localPosition = new Vector3(
                    spawnX,
                    1.1f,
                    4f + i * 2.2f);

                coinGo.AddComponent<Collectible>().BuildPlaceholder();
            }
        }

        void RecycleSegment()
        {
            TrackSegment old = activeSegments.Dequeue();
            Destroy(old.gameObject);
        }

        void HandlePhaseChanged(RunPhase phase, RunPhaseSettings settings)
        {
            UpdateAmbient(settings);

            foreach (TrackSegment segment in activeSegments)
                segment.ApplyLayout(settings);
        }

        static void UpdateAmbient(RunPhaseSettings settings)
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = settings.ambientColor;
        }

        public void ResetSpawner()
        {
            while (activeSegments.Count > 0)
            {
                TrackSegment segment = activeSegments.Dequeue();
                Destroy(segment.gameObject);
            }

            foreach (Enemy enemy in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
                Destroy(enemy.gameObject);

            foreach (Boss boss in FindObjectsByType<Boss>(FindObjectsSortMode.None))
                Destroy(boss.gameObject);

            foreach (ExperienceOrb orb in FindObjectsByType<ExperienceOrb>(FindObjectsSortMode.None))
                Destroy(orb.gameObject);

            foreach (FreezePickup freezePickup in FindObjectsByType<FreezePickup>(FindObjectsSortMode.None))
                Destroy(freezePickup.gameObject);

            foreach (FoodPickup foodPickup in FindObjectsByType<FoodPickup>(FindObjectsSortMode.None))
                Destroy(foodPickup.gameObject);

            foreach (CoinBagPickup coinBag in FindObjectsByType<CoinBagPickup>(FindObjectsSortMode.None))
                Destroy(coinBag.gameObject);

            foreach (ShieldPickup shieldPickup in FindObjectsByType<ShieldPickup>(FindObjectsSortMode.None))
                Destroy(shieldPickup.gameObject);

            foreach (KillAllPickup killAllPickup in FindObjectsByType<KillAllPickup>(FindObjectsSortMode.None))
                Destroy(killAllPickup.gameObject);

            EnemyFreezeController.Reset();
            PlayerShieldController.Reset();

            PhaseBossController.Instance?.ResetBossState();

            nextSpawnZ = 0f;
            continuousSpawnTimer = 0f;
            segmentCounter = 0;

            for (int i = 0; i < segmentsAhead; i++)
                SpawnSegment();
        }
    }
}
