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

        public static bool LevelUpEnabled = true;

        public int XpRequiredForNextLevel { get; private set; } = 1;
        public int XpTowardNextLevel { get; private set; }

        public event Action<int, int> OnXpChanged;
        public event Action<UpgradeOffer[]> OnUpgradeOffered;

        GameManager gameManager;
        HeroUpgradeStats heroStats;
        UpgradeOffer[] currentOffer;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            gameManager = GameManager.Instance;
        }

        public void BindHero(HeroUpgradeStats stats)
        {
            heroStats = stats;
        }

        public void CollectXp(int amount)
        {
            if (!LevelUpEnabled || gameManager == null || gameManager.State != GameState.Running)
                return;

            if (amount <= 0)
                return;

            XpTowardNextLevel += amount;

            while (XpTowardNextLevel >= XpRequiredForNextLevel)
            {
                XpTowardNextLevel -= XpRequiredForNextLevel;
                XpRequiredForNextLevel++;
                NotifyXpChanged();
                OfferUpgrades();
                return;
            }

            NotifyXpChanged();
        }

        void NotifyXpChanged()
        {
            OnXpChanged?.Invoke(XpTowardNextLevel, XpRequiredForNextLevel);
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

            UpgradeOffer offer = currentOffer[index];
            if (offer.IsCoinReward)
                gameManager.AddCoin(offer.CoinAmount);
            else
                heroStats?.LevelUpSkill(offer.Skill.Id);

            currentOffer = null;
            gameManager.ResumeFromUpgradeSelection();
        }

        public void ResetProgression()
        {
            currentOffer = null;
            XpRequiredForNextLevel = 1;
            XpTowardNextLevel = 0;
            NotifyXpChanged();

            if (heroStats == null)
                heroStats = FindFirstObjectByType<HeroUpgradeStats>();

            heroStats?.ResetBonuses();
        }
    }
}
