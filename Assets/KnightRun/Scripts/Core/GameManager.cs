using System;
using UnityEngine;

namespace KnightRun.Core
{
    public enum GameState
    {
        Ready,
        Running,
        GameOver
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState State { get; private set; } = GameState.Ready;
        public float Distance { get; private set; }
        public int Score { get; private set; }
        public int Coins { get; private set; }
        public int EnemiesDefeated { get; private set; }
        public float CurrentSpeed { get; private set; }

        public event Action<GameState> OnStateChanged;
        public event Action<float> OnDistanceChanged;
        public event Action<int> OnScoreChanged;
        public event Action<int> OnCoinsChanged;
        public event Action<int> OnEnemiesDefeatedChanged;

        [SerializeField] float baseSpeed = 8f;
        [SerializeField] float maxSpeed = 18f;
        [SerializeField] float speedRampPerSecond = 0.08f;

        RunPhaseManager phaseManager;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        void Start()
        {
            phaseManager = FindFirstObjectByType<RunPhaseManager>();
            CurrentSpeed = baseSpeed;
        }

        void Update()
        {
            if (State != GameState.Running)
                return;

            float multiplier = phaseManager != null ? phaseManager.CurrentSettings.speedMultiplier : 1f;
            CurrentSpeed = Mathf.Min(maxSpeed, CurrentSpeed + speedRampPerSecond * Time.deltaTime);
            float delta = CurrentSpeed * multiplier * Time.deltaTime;

            Distance += delta;
            Score = Mathf.FloorToInt(Distance * 10f) + Coins * 5;

            OnDistanceChanged?.Invoke(Distance);
            OnScoreChanged?.Invoke(Score);
        }

        public void StartRun()
        {
            if (State == GameState.Running)
                return;

            State = GameState.Running;
            OnStateChanged?.Invoke(State);
        }

        public void AddCoin(int amount = 1)
        {
            Coins += amount;
            Score = Mathf.FloorToInt(Distance * 10f) + Coins * 5;
            OnCoinsChanged?.Invoke(Coins);
            OnScoreChanged?.Invoke(Score);
        }

        public void AddEnemyDefeated(int amount = 1)
        {
            EnemiesDefeated += amount;
            OnEnemiesDefeatedChanged?.Invoke(EnemiesDefeated);
        }

        public void TriggerGameOver()
        {
            if (State == GameState.GameOver)
                return;

            State = GameState.GameOver;
            OnStateChanged?.Invoke(State);
        }

        public void RestartRun()
        {
            Distance = 0f;
            Score = 0;
            Coins = 0;
            EnemiesDefeated = 0;
            CurrentSpeed = baseSpeed;
            State = GameState.Ready;

            OnDistanceChanged?.Invoke(Distance);
            OnScoreChanged?.Invoke(Score);
            OnCoinsChanged?.Invoke(Coins);
            OnEnemiesDefeatedChanged?.Invoke(EnemiesDefeated);
            OnStateChanged?.Invoke(State);
        }
    }
}
