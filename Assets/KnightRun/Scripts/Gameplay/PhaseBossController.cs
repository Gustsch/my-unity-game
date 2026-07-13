using KnightRun.Core;
using KnightRun.Meta;
using KnightRun.Player;
using UnityEngine;

namespace KnightRun.Gameplay
{
    public class PhaseBossController : MonoBehaviour
    {
        public static PhaseBossController Instance { get; private set; }

        public const int BossHealthMultiplier = 100;
        public const float BossAheadDistance = 35f;

        public bool IsBossFightActive => activeBoss != null;
        public Boss ActiveBoss => activeBoss;
        public bool HasSpawnedBossForCurrentPhase =>
            phaseManager != null
            && bossSpawnedForPhaseIndex == phaseManager.CurrentPhaseIndex
            && (activeBoss != null || bossDefeatedForPhase);
        public bool IsBlockingPhaseAdvance => HasSpawnedBossForCurrentPhase && !bossDefeatedForPhase;
        public bool ShouldBlockEnemySpawns =>
            phaseManager != null && phaseManager.IsPhaseRunComplete;

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
            if (phaseManager == null || gameManager == null)
                return;

            if (gameManager.State != GameState.Running)
                return;

            if (activeBoss != null)
                return;

            int phaseIndex = phaseManager.CurrentPhaseIndex;
            if (bossSpawnedForPhaseIndex == phaseIndex && bossDefeatedForPhase)
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
            float aheadDistance = BossAheadDistance;
            float spawnZ = player.transform.position.z + aheadDistance;

            var bossGo = new GameObject("PhaseBoss");
            bossGo.transform.position = new Vector3(spawnX, 0f, spawnZ);

            var boss = bossGo.AddComponent<Boss>();
            boss.Build();

            int averageEnemyHealth = EnemyCombatStats.GetAverageEnemyHealth(settings);
            int bossHealth = averageEnemyHealth * BossHealthMultiplier;
            boss.Initialize(bossHealth, aheadDistance);
            boss.OnDefeated += HandleBossDefeated;

            activeBoss = boss;
            bossSpawnedForPhaseIndex = phaseIndex;
            bossDefeatedForPhase = false;
        }

        void HandleBossDefeated(Boss boss)
        {
            if (activeBoss == boss)
                activeBoss = null;

            bossDefeatedForPhase = true;
            CharacterUnlockProgress.MarkBossDefeated(bossSpawnedForPhaseIndex);
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
    }
}
