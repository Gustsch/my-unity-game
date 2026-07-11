using System;
using KnightRun.Meta;
using KnightRun.Core;
using KnightRun.Progression;
using UnityEngine;

namespace KnightRun.Player
{
    public class KnightHealth : MonoBehaviour
    {
        public const int BaseMaxHealth = 1000;

        HeroUpgradeStats upgradeStats;
        int resurrectionsRemaining;
        int lastHealedKillThreshold;

        HeroUpgradeStats UpgradeStats
        {
            get
            {
                if (upgradeStats == null)
                    upgradeStats = GetComponent<HeroUpgradeStats>();
                return upgradeStats;
            }
        }

        public int MaxHealth =>
            BaseMaxHealth + MetaBonuses.BonusMaxHealth +
            (UpgradeStats != null ? UpgradeStats.BonusMaxHealth : 0);
        int currentHealth;

        public int CurrentHealth => currentHealth;
        public int ResurrectionsRemaining => resurrectionsRemaining;

        public event Action<int, int> OnHealthChanged;

        void Awake()
        {
            BindUpgradeStats();
        }

        void Start()
        {
            BindUpgradeStats();
            ResetHealth();

            if (GameManager.Instance != null)
                GameManager.Instance.OnEnemiesDefeatedChanged += HandleEnemiesDefeatedChanged;
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

            if (GameManager.Instance != null)
                GameManager.Instance.OnEnemiesDefeatedChanged -= HandleEnemiesDefeatedChanged;
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

        void HandleEnemiesDefeatedChanged(int totalDefeated)
        {
            if (GameManager.Instance == null || GameManager.Instance.State != GameState.Running)
                return;

            int healingPerTen = MetaBonuses.HealingPerTenKills;
            if (healingPerTen <= 0 || currentHealth <= 0)
                return;

            int killThreshold = totalDefeated / 10;
            if (killThreshold <= lastHealedKillThreshold)
                return;

            int healTriggers = killThreshold - lastHealedKillThreshold;
            lastHealedKillThreshold = killThreshold;
            Heal(healTriggers * healingPerTen);
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

            if (currentHealth <= 0 && !TryResurrect())
                GameManager.Instance.TriggerGameOver();
        }

        public void KillInstantly()
        {
            if (GameManager.Instance == null)
                return;

            if (GameManager.Instance.State != GameState.Running)
                return;

            if (currentHealth <= 0)
                return;

            currentHealth = 0;
            OnHealthChanged?.Invoke(currentHealth, MaxHealth);

            if (!TryResurrect())
                GameManager.Instance.TriggerGameOver();
        }

        bool TryResurrect()
        {
            if (resurrectionsRemaining <= 0)
                return false;

            resurrectionsRemaining--;
            currentHealth = Mathf.Max(1, Mathf.CeilToInt(MaxHealth * 0.5f));
            OnHealthChanged?.Invoke(currentHealth, MaxHealth);
            return true;
        }

        public void Heal(int amount)
        {
            if (amount <= 0 || currentHealth <= 0)
                return;

            currentHealth = Mathf.Min(MaxHealth, currentHealth + amount);
            OnHealthChanged?.Invoke(currentHealth, MaxHealth);
        }

        public void ResetHealth()
        {
            resurrectionsRemaining = MetaBonuses.RessurectionsPerRun;
            lastHealedKillThreshold = 0;
            currentHealth = MaxHealth;
            OnHealthChanged?.Invoke(currentHealth, MaxHealth);
        }
    }
}
