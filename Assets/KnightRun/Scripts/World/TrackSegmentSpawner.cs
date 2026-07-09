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
        const float ContinuousSpawnMinAhead = 16f;
        const float ContinuousSpawnMaxAhead = 32f;

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

            continuousSpawnTimer -= Time.deltaTime;
            while (continuousSpawnTimer <= 0f)
            {
                continuousSpawnTimer += ContinuousSpawnInterval;
                SpawnEnemyAheadOfPlayer();
            }
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
            var contentRoot = new GameObject("Content").transform;
            contentRoot.SetParent(segment.transform, false);

            int enemyCount = Random.Range(MinEnemiesPerSegment, MaxEnemiesPerSegment + 1);
            for (int i = 0; i < enemyCount; i++)
            {
                float localZ = Random.Range(2f, segmentLength - 1f);
                SpawnEnemy(segment, settings, localZ);
            }

            if (Random.value < 0.65f)
                SpawnCoinLine(contentRoot, settings);
        }

        static float GetSpawnX(RunPhaseSettings settings)
        {
            if (settings.useLaneMovement)
                return LanePositions[Random.Range(0, LanePositions.Length)];

            return Random.Range(RunnerController.TrackMinX, RunnerController.TrackMaxX);
        }

        static float[] LanePositions => RunnerController.LanePositions;

        void SpawnEnemyAheadOfPlayer()
        {
            RunPhaseSettings settings = GetCurrentSettings();
            float spawnZ = player.transform.position.z + Random.Range(ContinuousSpawnMinAhead, ContinuousSpawnMaxAhead);
            SpawnEnemyAt(settings, GetSpawnX(settings), spawnZ);
        }

        void SpawnEnemy(TrackSegment segment, RunPhaseSettings settings, float localZ)
        {
            Vector3 worldPosition = segment.transform.position + new Vector3(
                GetSpawnX(settings),
                0f,
                localZ);

            SpawnEnemyAt(settings, worldPosition.x, worldPosition.z);
        }

        void SpawnEnemyAt(RunPhaseSettings settings, float x, float z)
        {
            var enemyGo = new GameObject("Enemy");
            enemyGo.transform.SetParent(transform, false);
            enemyGo.transform.position = new Vector3(x, 0f, z);

            var enemy = enemyGo.AddComponent<Enemy>();
            enemy.Build();

            float distance = GameManager.Instance != null ? GameManager.Instance.Distance : 0f;
            int health = EnemyCombatStats.GetMaxHealthForDistance(distance);
            enemy.Initialize(health);
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

            nextSpawnZ = 0f;
            continuousSpawnTimer = 0f;
            segmentCounter = 0;

            for (int i = 0; i < segmentsAhead; i++)
                SpawnSegment();
        }
    }
}
