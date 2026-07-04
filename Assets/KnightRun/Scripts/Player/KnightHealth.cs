using System;
using KnightRun.Core;
using UnityEngine;

namespace KnightRun.Player
{
    public class KnightHealth : MonoBehaviour
    {
        public const int MaxHealth = 100;
        public const int EnemyTouchDamage = 40;

        public int CurrentHealth { get; private set; } = MaxHealth;

        public event Action<int, int> OnHealthChanged;

        public void TakeDamage(int amount)
        {
            if (GameManager.Instance == null || GameManager.Instance.State != GameState.Running)
                return;

            CurrentHealth -= amount;
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

            if (CurrentHealth <= 0)
                GameManager.Instance.TriggerGameOver();
        }

        public void ResetHealth()
        {
            CurrentHealth = MaxHealth;
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }
    }
}
