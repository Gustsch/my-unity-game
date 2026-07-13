using KnightRun;
using KnightRun.Core;
using KnightRun.Gameplay;
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
        RectTransform xpBarFillRect;
        Text xpBarText;
        float displayedXpFill;
        float targetXpFill;

        GameObject bossHpBarRoot;
        Image bossHpBarFill;
        RectTransform bossHpBarFillRect;
        Text bossHpBarText;
        Boss trackedBoss;
        float displayedBossHpFill;
        float targetBossHpFill;

        const float XpBarFillSpeed = 4f;
        const float BossHpBarFillSpeed = 6f;

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
            BuildBossHpBar(canvasRoot.transform);
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

            UpdateXpBarAnimation();
            UpdateBossHpBarTracking();
            UpdateBossHpBarAnimation();
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

            ClearTrackedBoss();
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
            targetXpFill = required > 0 ? Mathf.Clamp01((float)current / required) : 0f;

            if (xpBarText != null)
                xpBarText.text = $"XP {current}/{required}";

            if (Mathf.Approximately(targetXpFill, 0f))
            {
                displayedXpFill = 0f;
                ApplyXpFill(0f);
            }
        }

        void UpdateXpBarAnimation()
        {
            if (xpBarFillRect == null)
                return;

            if (Mathf.Approximately(displayedXpFill, targetXpFill))
                return;

            displayedXpFill = Mathf.MoveTowards(displayedXpFill, targetXpFill, XpBarFillSpeed * Time.deltaTime);
            ApplyXpFill(displayedXpFill);
        }

        void ApplyXpFill(float amount)
        {
            ApplyBarFill(xpBarFillRect, amount);
        }

        void UpdateBossHpBarTracking()
        {
            Boss activeBoss = PhaseBossController.Instance?.ActiveBoss;
            bool barVisible = bossHpBarRoot != null && bossHpBarRoot.activeSelf;

            if (activeBoss == null)
            {
                if (trackedBoss != null || barVisible)
                    SetTrackedBoss(null);
                return;
            }

            if (!ReferenceEquals(trackedBoss, activeBoss))
                SetTrackedBoss(activeBoss);
        }

        void SetTrackedBoss(Boss boss)
        {
            ClearTrackedBoss();
            trackedBoss = boss;

            if (trackedBoss == null)
            {
                HideBossHpBar();
                return;
            }

            trackedBoss.OnHealthChanged += UpdateBossHpBar;
            ShowBossHpBar();
            UpdateBossHpBar(trackedBoss.CurrentHealth, trackedBoss.MaxHealth);
            displayedBossHpFill = targetBossHpFill;
            ApplyBarFill(bossHpBarFillRect, displayedBossHpFill);
        }

        void ClearTrackedBoss()
        {
            if (trackedBoss != null)
                trackedBoss.OnHealthChanged -= UpdateBossHpBar;

            trackedBoss = null;
        }

        void UpdateBossHpBar(float current, float max)
        {
            targetBossHpFill = max > 0f ? Mathf.Clamp01(current / max) : 0f;

            if (bossHpBarText != null)
                bossHpBarText.text = $"BOSS {Mathf.CeilToInt(Mathf.Max(0f, current))}/{Mathf.CeilToInt(max)}";

            if (current <= 0f)
            {
                ClearTrackedBoss();
                HideBossHpBar();
            }
        }

        void UpdateBossHpBarAnimation()
        {
            if (bossHpBarFillRect == null || bossHpBarRoot == null || !bossHpBarRoot.activeSelf)
                return;

            if (Mathf.Approximately(displayedBossHpFill, targetBossHpFill))
                return;

            displayedBossHpFill = Mathf.MoveTowards(
                displayedBossHpFill,
                targetBossHpFill,
                BossHpBarFillSpeed * Time.deltaTime);
            ApplyBarFill(bossHpBarFillRect, displayedBossHpFill);
        }

        void ShowBossHpBar()
        {
            if (bossHpBarRoot != null)
                bossHpBarRoot.SetActive(true);
        }

        void HideBossHpBar()
        {
            if (bossHpBarRoot != null)
                bossHpBarRoot.SetActive(false);

            displayedBossHpFill = 0f;
            targetBossHpFill = 0f;
        }

        static void ApplyBarFill(RectTransform fillRect, float amount)
        {
            if (fillRect == null)
                return;

            amount = Mathf.Clamp01(amount);
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(amount, 1f);
            fillRect.offsetMin = new Vector2(2f, 2f);
            fillRect.offsetMax = amount >= 1f ? new Vector2(-2f, -2f) : new Vector2(0f, -2f);
        }

        void BuildXpBar(Transform parent)
        {
            var backgroundGo = new GameObject("XpBarBackground");
            backgroundGo.transform.SetParent(parent, false);

            var backgroundRect = backgroundGo.AddComponent<RectTransform>();
            backgroundRect.anchorMin = new Vector2(0.5f, 0f);
            backgroundRect.anchorMax = new Vector2(0.5f, 0f);
            backgroundRect.pivot = new Vector2(0.5f, 0f);
            backgroundRect.anchoredPosition = new Vector2(0f, 72f);
            backgroundRect.sizeDelta = new Vector2(520f, 24f);

            var backgroundImage = backgroundGo.AddComponent<Image>();
            backgroundImage.color = new Color(0.08f, 0.1f, 0.16f, 0.92f);
            backgroundImage.raycastTarget = false;

            var trackGo = new GameObject("XpBarTrack");
            trackGo.transform.SetParent(backgroundGo.transform, false);

            var trackRect = trackGo.AddComponent<RectTransform>();
            trackRect.anchorMin = Vector2.zero;
            trackRect.anchorMax = Vector2.one;
            trackRect.offsetMin = new Vector2(2f, 2f);
            trackRect.offsetMax = new Vector2(-2f, -2f);

            var trackImage = trackGo.AddComponent<Image>();
            trackImage.color = new Color(0.04f, 0.06f, 0.1f, 0.95f);
            trackImage.raycastTarget = false;

            var fillGo = new GameObject("XpBarFill");
            fillGo.transform.SetParent(trackGo.transform, false);

            xpBarFillRect = fillGo.AddComponent<RectTransform>();
            xpBarFillRect.anchorMin = Vector2.zero;
            xpBarFillRect.anchorMax = Vector2.zero;
            xpBarFillRect.pivot = new Vector2(0f, 0.5f);
            xpBarFillRect.offsetMin = Vector2.zero;
            xpBarFillRect.offsetMax = Vector2.zero;

            xpBarFill = fillGo.AddComponent<Image>();
            xpBarFill.color = new Color(0.4f, 0.82f, 1f, 0.95f);
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
            displayedXpFill = 0f;
            targetXpFill = 0f;
            ApplyXpFill(0f);
        }

        void BuildBossHpBar(Transform parent)
        {
            bossHpBarRoot = new GameObject("BossHpBarBackground");
            bossHpBarRoot.transform.SetParent(parent, false);

            var backgroundRect = bossHpBarRoot.AddComponent<RectTransform>();
            backgroundRect.anchorMin = new Vector2(0.5f, 1f);
            backgroundRect.anchorMax = new Vector2(0.5f, 1f);
            backgroundRect.pivot = new Vector2(0.5f, 1f);
            backgroundRect.anchoredPosition = new Vector2(0f, -235f);
            backgroundRect.sizeDelta = new Vector2(520f, 28f);

            var backgroundImage = bossHpBarRoot.AddComponent<Image>();
            backgroundImage.color = new Color(0.16f, 0.06f, 0.08f, 0.94f);
            backgroundImage.raycastTarget = false;

            var trackGo = new GameObject("BossHpBarTrack");
            trackGo.transform.SetParent(bossHpBarRoot.transform, false);

            var trackRect = trackGo.AddComponent<RectTransform>();
            trackRect.anchorMin = Vector2.zero;
            trackRect.anchorMax = Vector2.one;
            trackRect.offsetMin = new Vector2(2f, 2f);
            trackRect.offsetMax = new Vector2(-2f, -2f);

            var trackImage = trackGo.AddComponent<Image>();
            trackImage.color = new Color(0.08f, 0.03f, 0.04f, 0.95f);
            trackImage.raycastTarget = false;

            var fillGo = new GameObject("BossHpBarFill");
            fillGo.transform.SetParent(trackGo.transform, false);

            bossHpBarFillRect = fillGo.AddComponent<RectTransform>();
            bossHpBarFillRect.anchorMin = Vector2.zero;
            bossHpBarFillRect.anchorMax = Vector2.zero;
            bossHpBarFillRect.pivot = new Vector2(0f, 0.5f);
            bossHpBarFillRect.offsetMin = Vector2.zero;
            bossHpBarFillRect.offsetMax = Vector2.zero;

            bossHpBarFill = fillGo.AddComponent<Image>();
            bossHpBarFill.color = new Color(0.92f, 0.18f, 0.2f, 0.96f);
            bossHpBarFill.raycastTarget = false;

            var textGo = new GameObject("BossHpBarText");
            textGo.transform.SetParent(bossHpBarRoot.transform, false);

            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            bossHpBarText = textGo.AddComponent<Text>();
            bossHpBarText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            bossHpBarText.fontSize = 16;
            bossHpBarText.fontStyle = FontStyle.Bold;
            bossHpBarText.alignment = TextAnchor.MiddleCenter;
            bossHpBarText.color = Color.white;
            bossHpBarText.raycastTarget = false;
            bossHpBarText.text = "BOSS";

            bossHpBarRoot.SetActive(false);
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
