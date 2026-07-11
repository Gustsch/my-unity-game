using KnightRun;
using KnightRun.Core;
using KnightRun.Player;
using KnightRun.Progression;
using UnityEngine;
using UnityEngine.UI;

namespace KnightRun.UI
{
    public class RunUI : MonoBehaviour
    {
        public static RunUI Instance { get; private set; }

        GameObject canvasRoot;
        Text titleText;
        Text scoreText;
        Text phaseText;
        Text healthText;
        Text enemiesDefeatedText;
        Text hintText;
        Text stateText;
        Image xpBarFill;
        Text xpBarText;

        GameObject pausePanel;
        Text pauseTitleText;

        GameManager gameManager;
        RunPhaseManager phaseManager;
        KnightHealth knightHealth;
        UpgradeManager upgradeManager;

        public void Build()
        {
            Instance = this;
            UiFactory.EnsureEventSystem();
            canvasRoot = new GameObject("RunUI");
            canvasRoot.transform.SetParent(transform, false);
            var canvas = canvasRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            UiFactory.ConfigureScaler(canvasRoot.AddComponent<CanvasScaler>());
            canvasRoot.AddComponent<GraphicRaycaster>();

            titleText = CreateText(canvasRoot.transform, "Knight Run", 42, TextAnchor.UpperCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f));
            phaseText = CreateText(canvasRoot.transform, "Floresta Encantada", 24, TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -90f));
            healthText = CreateText(canvasRoot.transform, "Vida: 100", 24, TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -130f));
            enemiesDefeatedText = CreateText(canvasRoot.transform, "Inimigos: 0", 24, TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -170f));
            BuildXpBar(canvasRoot.transform);
            scoreText = CreateText(canvasRoot.transform, "Score: 0", 28, TextAnchor.UpperRight, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-20f, -90f));
            UiFactory.CreateAnchoredButton(
                canvasRoot.transform,
                "PAUSAR",
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-20f, -135f),
                new Vector2(120f, 44f),
                TogglePause,
                20).GetComponent<Button>();
            hintText = CreateText(canvasRoot.transform, "A/D: mover | Espaco: pular | S: deslizar | P: pausar | M: menu", 18, TextAnchor.LowerCenter, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 30f));
            stateText = CreateText(canvasRoot.transform, string.Empty, 30, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);
            BuildPausePanel(canvasRoot.transform);

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

            upgradeManager = UpgradeManager.Instance;
            if (upgradeManager != null)
            {
                upgradeManager.OnXpChanged += UpdateXpBar;
                UpdateXpBar(upgradeManager.XpTowardNextLevel, upgradeManager.XpRequiredForNextLevel);
            }
        }

        public void Show()
        {
            if (canvasRoot != null)
                canvasRoot.SetActive(true);
        }

        public void Hide()
        {
            if (canvasRoot != null)
                canvasRoot.SetActive(false);
        }

        void Update()
        {
            if (gameManager == null)
                return;

            if (gameManager.State == GameState.Running && (KnightInput.GetKeyDown(KeyCode.P) || KnightInput.GetKeyDown(KeyCode.Escape)))
                gameManager.PauseRun();

            if (gameManager.State == GameState.Paused && (KnightInput.GetKeyDown(KeyCode.P) || KnightInput.GetKeyDown(KeyCode.Escape)))
                gameManager.ResumeRun();

            if (gameManager.State == GameState.Running && KnightInput.GetKeyDown(KeyCode.M))
                GameBootstrap.ReturnToMainMenu();

            if (gameManager.State == GameState.GameOver && KnightInput.GetKeyDown(KeyCode.R))
                GameBootstrap.StartRunFromMenu();

            if (gameManager.State == GameState.GameOver && KnightInput.GetKeyDown(KeyCode.M))
                GameBootstrap.ReturnToMainMenu();
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

            if (upgradeManager != null)
                upgradeManager.OnXpChanged -= UpdateXpBar;
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

        void UpdateXpBar(int current, int required)
        {
            if (xpBarFill != null)
                xpBarFill.fillAmount = required > 0 ? Mathf.Clamp01((float)current / required) : 0f;

            if (xpBarText != null)
                xpBarText.text = $"XP {current}/{required}";
        }

        void BuildXpBar(Transform parent)
        {
            var backgroundGo = new GameObject("XpBarBackground");
            backgroundGo.transform.SetParent(parent, false);

            var backgroundRect = backgroundGo.AddComponent<RectTransform>();
            backgroundRect.anchorMin = new Vector2(0.5f, 1f);
            backgroundRect.anchorMax = new Vector2(0.5f, 1f);
            backgroundRect.pivot = new Vector2(0.5f, 1f);
            backgroundRect.anchoredPosition = new Vector2(0f, -205f);
            backgroundRect.sizeDelta = new Vector2(520f, 24f);

            var backgroundImage = backgroundGo.AddComponent<Image>();
            backgroundImage.color = new Color(0.08f, 0.1f, 0.16f, 0.92f);
            backgroundImage.raycastTarget = false;

            var fillGo = new GameObject("XpBarFill");
            fillGo.transform.SetParent(backgroundGo.transform, false);

            var fillRect = fillGo.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(2f, 2f);
            fillRect.offsetMax = new Vector2(-2f, -2f);

            xpBarFill = fillGo.AddComponent<Image>();
            xpBarFill.color = new Color(0.4f, 0.82f, 1f, 0.95f);
            xpBarFill.type = Image.Type.Filled;
            xpBarFill.fillMethod = Image.FillMethod.Horizontal;
            xpBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            xpBarFill.fillAmount = 0f;
            xpBarFill.raycastTarget = false;

            var textGo = new GameObject("XpBarText");
            textGo.transform.SetParent(backgroundGo.transform, false);

            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            xpBarText = textGo.AddComponent<Text>();
            xpBarText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            xpBarText.fontSize = 15;
            xpBarText.alignment = TextAnchor.MiddleCenter;
            xpBarText.color = Color.white;
            xpBarText.raycastTarget = false;
            xpBarText.text = "XP 0/1";
        }

        void TogglePause()
        {
            gameManager?.TogglePause();
        }

        void BuildPausePanel(Transform parent)
        {
            pausePanel = UiFactory.CreatePanel(parent, "PausePanel", new Color(0f, 0f, 0f, 0.72f));
            pausePanel.SetActive(false);

            pauseTitleText = UiFactory.CreateText(
                pausePanel.transform,
                "PAUSADO",
                48,
                TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 60f),
                new Vector2(500f, 90f));

            UiFactory.CreateAnchoredButton(
                pausePanel.transform,
                "CONTINUAR",
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -30f),
                new Vector2(280f, 56f),
                TogglePause);

            UiFactory.CreateAnchoredButton(
                pausePanel.transform,
                "MENU",
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -110f),
                new Vector2(280f, 56f),
                GameBootstrap.ReturnToMainMenu);
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
                case GameState.MainMenu:
                    stateText.text = string.Empty;
                    hintText.enabled = false;
                    break;
                case GameState.Ready:
                    stateText.text = string.Empty;
                    hintText.enabled = true;
                    break;
                case GameState.Running:
                    stateText.text = string.Empty;
                    hintText.enabled = true;
                    if (pausePanel != null)
                        pausePanel.SetActive(false);
                    break;
                case GameState.Paused:
                    stateText.text = string.Empty;
                    hintText.enabled = false;
                    if (pausePanel != null)
                        pausePanel.SetActive(true);
                    break;
                case GameState.ChoosingUpgrade:
                    stateText.text = string.Empty;
                    hintText.enabled = false;
                    break;
                case GameState.GameOver:
                    stateText.text = "Game Over!\nMoedas salvas!\nR = jogar de novo | M = menu";
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
