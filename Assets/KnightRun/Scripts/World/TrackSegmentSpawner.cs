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

        readonly Queue<TrackSegment> activeSegments = new Queue<TrackSegment>();
        RunnerController player;
        RunPhaseManager phaseManager;
        float nextSpawnZ;
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

            if (activeSegments.Count == 0)
                return;

            TrackSegment oldest = activeSegments.Peek();
            if (player.transform.position.z - segmentLength > oldest.transform.position.z + segmentLength)
                RecycleSegment();
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

            if (Random.value < settings.obstacleChance)
                SpawnEnemy(segment, settings);

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

        void SpawnEnemy(TrackSegment segment, RunPhaseSettings settings)
        {
            Vector3 worldPosition = segment.transform.position + new Vector3(
                GetSpawnX(settings),
                0f,
                segmentLength * 0.55f);

            var enemyGo = new GameObject("Enemy");
            enemyGo.transform.SetParent(transform, false);
            enemyGo.transform.position = worldPosition;

            var enemy = enemyGo.AddComponent<Enemy>();
            enemy.Build();
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
            segmentCounter = 0;

            for (int i = 0; i < segmentsAhead; i++)
                SpawnSegment();
        }
    }
}
