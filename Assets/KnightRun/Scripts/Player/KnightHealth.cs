using System;
using KnightRun.Core;
using KnightRun.Progression;
using UnityEngine;

namespace KnightRun.Player
{
    public class KnightHealth : MonoBehaviour
    {
        public const int BaseMaxHealth = 100;

        HeroUpgradeStats upgradeStats;

        HeroUpgradeStats UpgradeStats
        {
            get
            {
                if (upgradeStats == null)
                    upgradeStats = GetComponent<HeroUpgradeStats>();
                return upgradeStats;
            }
        }

        public int MaxHealth => BaseMaxHealth + (UpgradeStats != null ? UpgradeStats.BonusMaxHealth : 0);
        int currentHealth;

        public int CurrentHealth => currentHealth;

        public event Action<int, int> OnHealthChanged;

        void Awake()
        {
            BindUpgradeStats();
        }

        void Start()
        {
            BindUpgradeStats();
            ResetHealth();
        }

        void BindUpgradeStats()
        {
            if (upgradeStats != null)
                return;

            upgradeStats = GetComponent<HeroUpgradeStats>();
            if (upgradeStats != null)
            {
                upgradeStats.OnSkillLeveled += HandleSkillLeveled;
                upgradeStats.OnBonusesChanged += HandleBonusesChanged;
            }
        }

        void OnDestroy()
        {
            if (upgradeStats != null)
            {
                upgradeStats.OnSkillLeveled -= HandleSkillLeveled;
                upgradeStats.OnBonusesChanged -= HandleBonusesChanged;
            }
        }

        void HandleBonusesChanged()
        {
            currentHealth = Mathf.Min(currentHealth, MaxHealth);
            OnHealthChanged?.Invoke(currentHealth, MaxHealth);
        }

        void HandleSkillLeveled(HeroSkillId id, int level)
        {
            if (id != HeroSkillId.Vigor)
                return;

            currentHealth = Mathf.Min(currentHealth + SkillPool.VigorHealthPerLevel, MaxHealth);
            OnHealthChanged?.Invoke(currentHealth, MaxHealth);
        }

        public void TakeDamage(int amount)
        {
            if (GameManager.Instance == null)
                return;

            if (GameManager.Instance.State != GameState.Running)
                return;

            float reductionPercent = UpgradeStats != null ? UpgradeStats.DamageReductionPercent : 0f;
            int finalDamage = Mathf.Max(1, Mathf.RoundToInt(amount * (1f - reductionPercent)));

            currentHealth -= finalDamage;
            OnHealthChanged?.Invoke(currentHealth, MaxHealth);

            if (currentHealth <= 0)
                GameManager.Instance.TriggerGameOver();
        }

        public void ResetHealth()
        {
            currentHealth = MaxHealth;
            OnHealthChanged?.Invoke(currentHealth, MaxHealth);
        }
    }
}
