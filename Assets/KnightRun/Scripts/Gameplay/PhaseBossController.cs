using KnightRun.Core;
using KnightRun.Player;
using UnityEngine;

namespace KnightRun.Gameplay
{
    public class PhaseBossController : MonoBehaviour
    {
        public static PhaseBossController Instance { get; private set; }

        public const int BossHealthMultiplier = 180;
        public const float BossScreenViewportY = 0.82f;
        public const float BossMinAheadDistance = 18f;
        public const float BossFallbackAheadDistance = 28f;

        public bool IsBossFightActive => activeBoss != null;
        public bool HasSpawnedBossForCurrentPhase =>
            phaseManager != null && bossSpawnedForPhaseIndex == phaseManager.CurrentPhaseIndex;
        public bool IsBlockingPhaseAdvance => HasSpawnedBossForCurrentPhase && !bossDefeatedForPhase;

        GameManager gameManager;
        RunPhaseManager phaseManager;
        Boss activeBoss;
        int bossSpawnedForPhaseIndex = -1;
        bool bossDefeatedForPhase;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            gameManager = GameManager.Instance;
            phaseManager = RunPhaseManager.Instance;
        }

        public void TrySpawnBoss()
        {
            if (phaseManager == null || gameManager == null || gameManager.State != GameState.Running)
                return;

            if (activeBoss != null)
                return;

            int phaseIndex = phaseManager.CurrentPhaseIndex;
            if (bossSpawnedForPhaseIndex == phaseIndex)
                return;

            if (!phaseManager.IsPhaseRunComplete)
                return;

            SpawnBoss(phaseManager.CurrentSettings, phaseIndex);
        }

        void SpawnBoss(RunPhaseSettings settings, int phaseIndex)
        {
            var player = FindFirstObjectByType<RunnerController>();
            if (player == null)
                return;

            float spawnX = 0f;
            float spawnZ = GetScreenTopSpawnZ(player.transform.position);
            float screenAheadDistance = spawnZ - player.transform.position.z;

            var bossGo = new GameObject("PhaseBoss");
            bossGo.transform.position = new Vector3(spawnX, 0f, spawnZ);

            var boss = bossGo.AddComponent<Boss>();
            boss.Build();

            int averageEnemyHealth = EnemyCombatStats.GetAverageEnemyHealth(settings);
            int bossHealth = averageEnemyHealth * BossHealthMultiplier;
            boss.Initialize(bossHealth, screenAheadDistance);
            boss.OnDefeated += HandleBossDefeated;

            foreach (Enemy enemy in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
                Destroy(enemy.gameObject);

            activeBoss = boss;
            bossSpawnedForPhaseIndex = phaseIndex;
            bossDefeatedForPhase = false;
        }

        void HandleBossDefeated(Boss boss)
        {
            if (activeBoss == boss)
                activeBoss = null;

            bossDefeatedForPhase = true;
            phaseManager?.AdvanceToNextPhase();
        }

        public void ResetBossState()
        {
            if (activeBoss != null)
                Destroy(activeBoss.gameObject);

            activeBoss = null;
            bossSpawnedForPhaseIndex = -1;
            bossDefeatedForPhase = false;

            foreach (BossProjectile projectile in FindObjectsByType<BossProjectile>(FindObjectsSortMode.None))
                Destroy(projectile.gameObject);
        }

        static float GetScreenTopSpawnZ(Vector3 playerPosition)
        {
            Camera camera = Camera.main;
            if (camera == null)
                return playerPosition.z + BossFallbackAheadDistance;

            Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, BossScreenViewportY, 0f));
            if (Mathf.Abs(ray.direction.y) < 0.0001f)
                return playerPosition.z + BossFallbackAheadDistance;

            float distanceToGround = (0f - ray.origin.y) / ray.direction.y;
            if (distanceToGround <= 0f)
                return playerPosition.z + BossFallbackAheadDistance;

            float spawnZ = ray.GetPoint(distanceToGround).z;
            return Mathf.Max(playerPosition.z + BossMinAheadDistance, spawnZ);
        }
    }
}
