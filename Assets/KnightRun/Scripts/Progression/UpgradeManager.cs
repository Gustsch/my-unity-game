using System;
using KnightRun.Core;
using KnightRun.Player;
using UnityEngine;

namespace KnightRun.Progression
{
    public class UpgradeManager : MonoBehaviour
    {
        public static UpgradeManager Instance { get; private set; }

        public const int OfferCount = 3;

        // Desativado temporariamente para testar spawn intenso de inimigos.
        public static bool LevelUpEnabled = true;

        public int KillsRequiredForNextLevel { get; private set; } = 1;
        public int KillsTowardNextLevel { get; private set; }

        public event Action<SkillDefinition[]> OnUpgradeOffered;

        GameManager gameManager;
        HeroUpgradeStats heroStats;
        SkillDefinition[] currentOffer;
        int lastEnemiesDefeated;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                lastEnemiesDefeated = gameManager.EnemiesDefeated;
                gameManager.OnEnemiesDefeatedChanged += HandleEnemiesDefeatedChanged;
            }
        }

        void OnDestroy()
        {
            if (gameManager != null)
                gameManager.OnEnemiesDefeatedChanged -= HandleEnemiesDefeatedChanged;
        }

        public void BindHero(HeroUpgradeStats stats)
        {
            heroStats = stats;
        }

        void HandleEnemiesDefeatedChanged(int enemiesDefeated)
        {
            if (gameManager == null || gameManager.State != GameState.Running)
                return;

            int newKills = enemiesDefeated - lastEnemiesDefeated;
            lastEnemiesDefeated = enemiesDefeated;

            for (int i = 0; i < newKills; i++)
                RegisterKill();
        }

        void RegisterKill()
        {
            if (!LevelUpEnabled)
                return;

            KillsTowardNextLevel++;

            if (KillsTowardNextLevel < KillsRequiredForNextLevel)
                return;

            KillsTowardNextLevel = 0;
            KillsRequiredForNextLevel++;
            OfferUpgrades();
        }

        public void OfferUpgrades()
        {
            if (gameManager == null || gameManager.State != GameState.Running)
                return;

            if (heroStats == null)
                heroStats = FindFirstObjectByType<HeroUpgradeStats>();

            if (heroStats == null)
                return;

            currentOffer = SkillPool.DrawOffers(heroStats, OfferCount);
            if (currentOffer.Length == 0)
                return;

            gameManager.EnterUpgradeSelection();
            OnUpgradeOffered?.Invoke(currentOffer);
        }

        public void SelectUpgrade(int index)
        {
            if (gameManager == null || gameManager.State != GameState.ChoosingUpgrade)
                return;

            if (currentOffer == null || index < 0 || index >= currentOffer.Length)
                return;

            if (heroStats == null)
                heroStats = FindFirstObjectByType<HeroUpgradeStats>();

            heroStats?.LevelUpSkill(currentOffer[index].Id);

            currentOffer = null;
            gameManager.ResumeFromUpgradeSelection();
        }

        public void ResetProgression()
        {
            currentOffer = null;
            KillsRequiredForNextLevel = 1;
            KillsTowardNextLevel = 0;
            lastEnemiesDefeated = 0;

            if (heroStats == null)
                heroStats = FindFirstObjectByType<HeroUpgradeStats>();

            heroStats?.ResetBonuses();

            if (gameManager != null)
                lastEnemiesDefeated = gameManager.EnemiesDefeated;
        }
    }
}
