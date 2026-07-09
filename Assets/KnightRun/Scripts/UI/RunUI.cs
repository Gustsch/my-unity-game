using KnightRun;
using KnightRun.Core;
using KnightRun.Player;
using UnityEngine;
using UnityEngine.UI;

namespace KnightRun.UI
{
    public class RunUI : MonoBehaviour
    {
        Text titleText;
        Text scoreText;
        Text phaseText;
        Text healthText;
        Text enemiesDefeatedText;
        Text hintText;
        Text stateText;

        GameManager gameManager;
        RunPhaseManager phaseManager;
        KnightHealth knightHealth;

        public void Build()
        {
            var canvasGo = new GameObject("RunUI");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.AddComponent<GraphicRaycaster>();

            titleText = CreateText(canvasGo.transform, "Knight Run", 42, TextAnchor.UpperCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f));
            phaseText = CreateText(canvasGo.transform, "Floresta Encantada", 24, TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -90f));
            healthText = CreateText(canvasGo.transform, "Vida: 100", 24, TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -130f));
            enemiesDefeatedText = CreateText(canvasGo.transform, "Inimigos: 0", 24, TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -170f));
            scoreText = CreateText(canvasGo.transform, "Score: 0", 28, TextAnchor.UpperRight, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-20f, -90f));
            hintText = CreateText(canvasGo.transform, "Clique na aba Game | A/D: mover | Espaco: pular | S: deslizar | Enter: iniciar | R: reiniciar", 18, TextAnchor.LowerCenter, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 30f));
            stateText = CreateText(canvasGo.transform, "Clique na aba Game e pressione ENTER", 30, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);

            gameManager = GameManager.Instance;
            phaseManager = RunPhaseManager.Instance;

            if (gameManager != null)
            {
                gameManager.OnScoreChanged += UpdateScore;
                gameManager.OnStateChanged += UpdateState;
                gameManager.OnEnemiesDefeatedChanged += UpdateEnemiesDefeated;
                UpdateEnemiesDefeated(gameManager.EnemiesDefeated);
            }

            if (phaseManager != null)
                phaseManager.OnPhaseChanged += UpdatePhase;

            knightHealth = FindFirstObjectByType<KnightHealth>();
            if (knightHealth != null)
            {
                knightHealth.OnHealthChanged += UpdateHealth;
                UpdateHealth(knightHealth.CurrentHealth, knightHealth.MaxHealth);
            }
        }

        void Update()
        {
            if (gameManager == null)
                return;

            if (gameManager.State == GameState.Ready && KnightInput.GetKeyDown(KeyCode.Return))
                gameManager.StartRun();

            if (gameManager.State == GameState.GameOver && KnightInput.GetKeyDown(KeyCode.R))
                GameBootstrap.RestartGame();
        }

        void OnDestroy()
        {
            if (gameManager != null)
            {
                gameManager.OnScoreChanged -= UpdateScore;
                gameManager.OnStateChanged -= UpdateState;
                gameManager.OnEnemiesDefeatedChanged -= UpdateEnemiesDefeated;
            }

            if (phaseManager != null)
                phaseManager.OnPhaseChanged -= UpdatePhase;

            if (knightHealth != null)
                knightHealth.OnHealthChanged -= UpdateHealth;
        }

        void UpdateScore(int score)
        {
            if (scoreText != null)
                scoreText.text = $"Score: {score}  |  Moedas: {gameManager.Coins}";
        }

        void UpdateHealth(int current, int max)
        {
            if (healthText != null)
                healthText.text = $"Vida: {Mathf.Max(0, current)}/{max}";
        }

        void UpdateEnemiesDefeated(int count)
        {
            if (enemiesDefeatedText != null)
                enemiesDefeatedText.text = $"Inimigos: {count}";
        }

        void UpdatePhase(RunPhase phase, RunPhaseSettings settings)
        {
            if (phaseText != null)
                phaseText.text = settings.displayName;
        }

        void UpdateState(GameState state)
        {
            if (stateText == null || hintText == null)
                return;

            switch (state)
            {
                case GameState.Ready:
                    stateText.text = "Pressione ENTER para correr!";
                    hintText.enabled = true;
                    break;
                case GameState.Running:
                    stateText.text = string.Empty;
                    hintText.enabled = true;
                    break;
                case GameState.ChoosingUpgrade:
                    stateText.text = string.Empty;
                    hintText.enabled = false;
                    break;
                case GameState.GameOver:
                    stateText.text = "Game Over!\nPressione R para reiniciar";
                    hintText.enabled = false;
                    break;
            }
        }

        static Text CreateText(Transform parent, string content, int fontSize, TextAnchor anchor, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = AnchorToPivot(anchor);
            rect.sizeDelta = new Vector2(900f, 120f);
            rect.anchoredPosition = anchoredPosition;

            var text = go.AddComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.color = Color.white;

            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.75f);
            shadow.effectDistance = new Vector2(1.5f, -1.5f);

            return text;
        }

        static Vector2 AnchorToPivot(TextAnchor anchor)
        {
            return anchor switch
            {
                TextAnchor.UpperLeft => new Vector2(0f, 1f),
                TextAnchor.UpperCenter => new Vector2(0.5f, 1f),
                TextAnchor.UpperRight => new Vector2(1f, 1f),
                TextAnchor.MiddleLeft => new Vector2(0f, 0.5f),
                TextAnchor.MiddleCenter => new Vector2(0.5f, 0.5f),
                TextAnchor.MiddleRight => new Vector2(1f, 0.5f),
                TextAnchor.LowerLeft => new Vector2(0f, 0f),
                TextAnchor.LowerCenter => new Vector2(0.5f, 0f),
                TextAnchor.LowerRight => new Vector2(1f, 0f),
                _ => new Vector2(0.5f, 0.5f)
            };
        }
    }
}
