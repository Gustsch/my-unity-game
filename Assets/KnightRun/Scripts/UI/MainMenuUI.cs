using System;
using System.Collections.Generic;
using KnightRun;
using KnightRun.Meta;
using KnightRun.Player;
using UnityEngine;
using UnityEngine.UI;

namespace KnightRun.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        public static MainMenuUI Instance { get; private set; }

        GameObject canvasRoot;
        GameObject mainPanel;
        GameObject shopPanel;
        GameObject charactersPanel;
        GameObject optionsPanel;
        GameObject highScoresPanel;

        Text coinsText;
        Text characterText;
        Text shopCoinsText;
        Text charactersSummaryText;
        Text optionsText;
        Text highScoresText;

        Transform shopItemsRoot;
        Transform charactersItemsRoot;
        Button shopBackButton;
        Button charactersBackButton;
        Button optionsLanguageButton;
        Button optionsResetButton;
        bool resetSaveConfirmationPending;

        readonly MenuNavigation mainNavigation = new MenuNavigation();
        readonly MenuNavigation shopNavigation = new MenuNavigation();
        readonly MenuNavigation charactersNavigation = new MenuNavigation();
        readonly MenuNavigation optionsNavigation = new MenuNavigation();
        readonly MenuNavigation highScoresNavigation = new MenuNavigation();

        MetaProgression metaProgression;

        public void Build()
        {
            Instance = this;
            canvasRoot = UiFactory.CreateCanvas(transform, "MainMenuCanvas", 20);

            mainPanel = BuildMainPanel(canvasRoot.transform);
            shopPanel = BuildShopPanel(canvasRoot.transform);
            charactersPanel = BuildCharactersPanel(canvasRoot.transform);
            optionsPanel = BuildOptionsPanel(canvasRoot.transform);
            highScoresPanel = BuildHighScoresPanel(canvasRoot.transform);

            metaProgression = MetaProgression.Instance;
            if (metaProgression != null)
                metaProgression.OnMetaChanged += RefreshMetaDisplay;

            CharacterSelection.OnCharacterChanged += RefreshCharacterDisplay;
            Localization.OnLanguageChanged += HandleLanguageChanged;

            ShowMainPanel();
            HandleLanguageChanged();
            RefreshMetaDisplay();
            RefreshCharacterDisplay();
            RefreshShopDisplay();
        }

        void Update()
        {
            if (canvasRoot == null || !canvasRoot.activeSelf)
                return;

            if (mainPanel != null && mainPanel.activeSelf)
                mainNavigation.HandleInput(true);
            else if (shopPanel != null && shopPanel.activeSelf)
                shopNavigation.HandleInput(true);
            else if (charactersPanel != null && charactersPanel.activeSelf)
                charactersNavigation.HandleInput(true);
            else if (optionsPanel != null && optionsPanel.activeSelf)
                optionsNavigation.HandleInput(true);
            else if (highScoresPanel != null && highScoresPanel.activeSelf)
                highScoresNavigation.HandleInput(true);

            if (KnightInput.GetKeyDown(KeyCode.Escape))
            {
                if (shopPanel != null && shopPanel.activeSelf)
                    ShowMainPanel();
                else if (charactersPanel != null && charactersPanel.activeSelf)
                    ShowMainPanel();
                else if (optionsPanel != null && optionsPanel.activeSelf)
                    ShowMainPanel();
                else if (highScoresPanel != null && highScoresPanel.activeSelf)
                    ShowMainPanel();
            }
        }

        void OnDestroy()
        {
            if (metaProgression != null)
                metaProgression.OnMetaChanged -= RefreshMetaDisplay;

            CharacterSelection.OnCharacterChanged -= RefreshCharacterDisplay;
            Localization.OnLanguageChanged -= HandleLanguageChanged;
        }

        public void Show()
        {
            if (canvasRoot != null)
                canvasRoot.SetActive(true);
            ShowMainPanel();
            RefreshMetaDisplay();
            RefreshCharacterDisplay();
        }

        public void Hide()
        {
            if (canvasRoot != null)
                canvasRoot.SetActive(false);
        }

        GameObject BuildMainPanel(Transform parent)
        {
            var panel = UiFactory.CreatePanel(parent, "MainPanel", new Color(0.04f, 0.06f, 0.1f, 0.92f));

            UiFactory.CreateText(panel.transform, "Knight Run", 56, TextAnchor.UpperCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -50f), new Vector2(700f, 90f));

            coinsText = UiFactory.CreateText(panel.transform, "Moedas: 0", 24, TextAnchor.UpperCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -120f), new Vector2(500f, 50f));

            characterText = UiFactory.CreateText(panel.transform, "Personagem: Cavaleiro", 22, TextAnchor.UpperCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -155f), new Vector2(500f, 40f));

            var buttons = new List<Button>
            {
                UiFactory.CreateButton(panel.transform, Localization.T("ui.play"), new Vector2(0f, 70f), () => GameBootstrap.StartRunFromMenu()),
                UiFactory.CreateButton(panel.transform, Localization.T("ui.characters"), new Vector2(0f, 0f), ShowCharactersPanel),
                UiFactory.CreateButton(panel.transform, Localization.T("ui.shop"), new Vector2(0f, -70f), ShowShopPanel),
                UiFactory.CreateButton(panel.transform, Localization.T("ui.options"), new Vector2(0f, -140f), ShowOptionsPanel),
                UiFactory.CreateButton(panel.transform, Localization.T("ui.highscores"), new Vector2(0f, -210f), ShowHighScoresPanel),
                UiFactory.CreateButton(panel.transform, Localization.T("ui.exit"), new Vector2(0f, -280f), ExitGame)
            };
            mainNavigation.SetButtons(buttons);

            return panel;
        }

        GameObject BuildShopPanel(Transform parent)
        {
            var panel = UiFactory.CreatePanel(parent, "ShopPanel", new Color(0.05f, 0.08f, 0.12f, 0.96f));
            panel.SetActive(false);

            UiFactory.CreateText(panel.transform, Localization.T("ui.shop"), 48, TextAnchor.UpperCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(600f, 80f));

            shopCoinsText = UiFactory.CreateText(panel.transform, "Moedas: 0", 24, TextAnchor.UpperCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -95f), new Vector2(500f, 50f));

            shopItemsRoot = UiFactory.CreateScrollArea(panel.transform, "ShopScroll", 150f, 90f);

            shopBackButton = UiFactory.CreateAnchoredButton(
                panel.transform,
                Localization.T("ui.back"),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 40f),
                new Vector2(320f, 56f),
                ShowMainPanel).GetComponent<Button>();

            shopNavigation.SetButtons(new[] { shopBackButton });
            return panel;
        }

        GameObject BuildCharactersPanel(Transform parent)
        {
            var panel = UiFactory.CreatePanel(parent, "CharactersPanel", new Color(0.05f, 0.08f, 0.12f, 0.96f));
            panel.SetActive(false);

            UiFactory.CreateText(panel.transform, Localization.T("ui.characters"), 48, TextAnchor.UpperCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(700f, 80f));

            charactersSummaryText = UiFactory.CreateText(panel.transform, string.Empty, 22, TextAnchor.UpperCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -95f), new Vector2(700f, 50f));

            charactersItemsRoot = UiFactory.CreateScrollArea(panel.transform, "CharactersScroll", 150f, 90f);

            charactersBackButton = UiFactory.CreateAnchoredButton(
                panel.transform,
                Localization.T("ui.back"),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 40f),
                new Vector2(320f, 56f),
                ShowMainPanel).GetComponent<Button>();

            charactersNavigation.SetButtons(new[] { charactersBackButton });
            return panel;
        }

        GameObject BuildOptionsPanel(Transform parent)
        {
            var panel = UiFactory.CreatePanel(parent, "OptionsPanel", new Color(0.05f, 0.08f, 0.12f, 0.96f));
            panel.SetActive(false);

            UiFactory.CreateText(panel.transform, Localization.T("ui.options"), 48, TextAnchor.UpperCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(600f, 80f));

            optionsText = UiFactory.CreateText(panel.transform, string.Empty, 24, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(760f, 320f));

            optionsLanguageButton = UiFactory.CreateAnchoredButton(
                panel.transform,
                Localization.GetLanguageButtonLabel(),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 180f),
                new Vector2(320f, 56f),
                Localization.ToggleLanguage).GetComponent<Button>();

            optionsResetButton = UiFactory.CreateAnchoredButton(
                panel.transform,
                Localization.T("ui.reset_save"),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 110f),
                new Vector2(320f, 56f),
                HandleResetSaveClick).GetComponent<Button>();

            optionsNavigation.SetButtons(new[]
            {
                optionsLanguageButton,
                optionsResetButton,
                UiFactory.CreateAnchoredButton(
                    panel.transform,
                    Localization.T("ui.back"),
                    new Vector2(0.5f, 0f),
                    new Vector2(0.5f, 0f),
                    new Vector2(0f, 40f),
                    new Vector2(320f, 56f),
                    ShowMainPanel).GetComponent<Button>()
            });
            return panel;
        }

        GameObject BuildHighScoresPanel(Transform parent)
        {
            var panel = UiFactory.CreatePanel(parent, "HighScoresPanel", new Color(0.05f, 0.08f, 0.12f, 0.96f));
            panel.SetActive(false);

            UiFactory.CreateText(panel.transform, Localization.T("ui.highscores"), 48, TextAnchor.UpperCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(700f, 80f));

            highScoresText = UiFactory.CreateText(panel.transform, string.Empty, 18, TextAnchor.UpperLeft,
                new Vector2(0.05f, 0.15f), new Vector2(0.95f, 0.85f), Vector2.zero, Vector2.zero);

            highScoresNavigation.SetButtons(new[]
            {
                UiFactory.CreateAnchoredButton(
                    panel.transform,
                    Localization.T("ui.back"),
                    new Vector2(0.5f, 0f),
                    new Vector2(0.5f, 0f),
                    new Vector2(0f, 40f),
                    new Vector2(320f, 56f),
                    ShowMainPanel).GetComponent<Button>()
            });
            return panel;
        }

        void ShowMainPanel()
        {
            SetPanelActive(mainPanel);
            mainNavigation.RefreshVisuals();
            RefreshMetaDisplay();
            RefreshCharacterDisplay();
        }

        void ShowShopPanel()
        {
            SetPanelActive(shopPanel);
            shopNavigation.RefreshVisuals();
            RefreshShopDisplay();
        }

        void ShowCharactersPanel()
        {
            SetPanelActive(charactersPanel);
            charactersNavigation.RefreshVisuals();
            RefreshCharactersDisplay();
        }

        void ShowOptionsPanel()
        {
            resetSaveConfirmationPending = false;
            UpdateResetSaveButtonLabel();
            SetPanelActive(optionsPanel);
            optionsNavigation.RefreshVisuals();
            RefreshOptionsDisplay();
        }

        void ShowHighScoresPanel()
        {
            SetPanelActive(highScoresPanel);
            highScoresNavigation.RefreshVisuals();
            RefreshHighScoresDisplay();
        }

        void SetPanelActive(GameObject activePanel)
        {
            if (mainPanel != null) mainPanel.SetActive(mainPanel == activePanel);
            if (shopPanel != null) shopPanel.SetActive(shopPanel == activePanel);
            if (charactersPanel != null) charactersPanel.SetActive(charactersPanel == activePanel);
            if (optionsPanel != null) optionsPanel.SetActive(optionsPanel == activePanel);
            if (highScoresPanel != null) highScoresPanel.SetActive(highScoresPanel == activePanel);
        }

        void RefreshMetaDisplay()
        {
            int coins = metaProgression != null ? metaProgression.TotalCoins : 0;
            if (coinsText != null)
                coinsText.text = Localization.Format("ui.coins", coins);

            if (charactersPanel != null && charactersPanel.activeSelf)
                RefreshCharactersDisplay();
        }

        void RefreshCharacterDisplay()
        {
            CharacterDefinition character = CharacterCatalog.Get(CharacterSelection.SelectedCharacter);
            if (characterText != null)
                characterText.text = Localization.Format(
                    "ui.character",
                    Localization.GetCharacterName(character.Id));

            if (charactersPanel != null && charactersPanel.activeSelf)
                RefreshCharactersDisplay();
        }

        void RefreshShopDisplay()
        {
            int coins = metaProgression != null ? metaProgression.TotalCoins : 0;
            if (shopCoinsText != null)
                shopCoinsText.text = Localization.Format("ui.coins", coins);

            if (shopItemsRoot == null)
                return;

            for (int i = shopItemsRoot.childCount - 1; i >= 0; i--)
                Destroy(shopItemsRoot.GetChild(i).gameObject);

            var shopButtons = new List<Button>();
            foreach (ShopUpgradeId id in Enum.GetValues(typeof(ShopUpgradeId)))
            {
                ShopUpgradeId upgradeId = id;
                int level = ShopCatalog.GetLevel(id);
                bool maxed = !ShopCatalog.CanUpgrade(id);
                int cost = ShopCatalog.GetCost(id);
                string next = maxed
                    ? "MAX"
                    : ShopCatalog.GetDescription(id, level + 1);
                string label = Localization.Format(
                    "ui.shop_item",
                    ShopCatalog.GetName(id),
                    Localization.T("ui.level_abbr"),
                    level,
                    ShopCatalog.MaxLevel,
                    next,
                    Localization.T("ui.cost"),
                    maxed ? "-" : cost.ToString());

                shopButtons.Add(UiFactory.CreateLayoutButton(
                    shopItemsRoot,
                    label,
                    () => TryBuy(upgradeId),
                    96f,
                    20));
            }

            if (shopBackButton != null)
                shopButtons.Add(shopBackButton);

            shopNavigation.SetButtons(shopButtons);
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)shopItemsRoot);
            if (shopPanel != null && shopPanel.activeSelf)
                shopNavigation.RefreshVisuals();
        }

        void RefreshCharactersDisplay()
        {
            int coins = metaProgression != null ? metaProgression.TotalCoins : 0;
            CharacterDefinition selected = CharacterCatalog.Get(CharacterSelection.SelectedCharacter);
            if (charactersSummaryText != null)
            {
                charactersSummaryText.text = Localization.Format(
                    "ui.coins_selected",
                    coins,
                    Localization.GetCharacterName(selected.Id),
                    Localization.GetSkillName(selected.StartingWeapon));
            }

            if (charactersItemsRoot == null)
                return;

            for (int i = charactersItemsRoot.childCount - 1; i >= 0; i--)
                Destroy(charactersItemsRoot.GetChild(i).gameObject);

            var characterButtons = new List<Button>();
            foreach (CharacterDefinition character in CharacterCatalog.All)
            {
                HeroCharacterId characterId = character.Id;
                string label = BuildCharacterButtonLabel(character);

                characterButtons.Add(UiFactory.CreateLayoutButton(
                    charactersItemsRoot,
                    label,
                    () => HandleCharacterAction(characterId),
                    96f,
                    20));
            }

            if (charactersBackButton != null)
                characterButtons.Add(charactersBackButton);

            charactersNavigation.SetButtons(characterButtons);
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)charactersItemsRoot);
            if (charactersPanel != null && charactersPanel.activeSelf)
                charactersNavigation.RefreshVisuals();
        }

        static string BuildCharacterButtonLabel(CharacterDefinition character)
        {
            bool isSelected = CharacterSelection.SelectedCharacter == character.Id;
            bool isOwned = CharacterOwnership.IsOwned(character.Id);
            bool isUnlocked = CharacterCatalog.IsUnlockedForPurchase(character.Id);
            string characterName = Localization.GetCharacterName(character.Id);
            string weaponName = Localization.GetSkillName(character.StartingWeapon);

            string weaponLine = Localization.Format("ui.starting_weapon", weaponName);

            if (!isUnlocked)
            {
                return $"{characterName}\n{weaponLine}\n{Localization.Format("ui.locked", CharacterCatalog.GetUnlockRequirementText(character.Id))}";
            }

            if (!isOwned)
            {
                return $"{characterName}\n{weaponLine}\n{Localization.Format("ui.buy", character.PurchaseCost)}";
            }

            if (isSelected)
                return $"{characterName}\n{weaponLine}\n{Localization.T("ui.selected")}";

            return $"{characterName}\n{weaponLine}\n{Localization.T("ui.select")}";
        }

        void HandleCharacterAction(HeroCharacterId id)
        {
            if (CharacterOwnership.IsOwned(id))
            {
                SelectCharacter(id);
                return;
            }

            if (!CharacterCatalog.IsUnlockedForPurchase(id))
                return;

            if (metaProgression == null)
                return;

            if (metaProgression.TryPurchaseCharacter(id))
            {
                SelectCharacter(id);
                RefreshMetaDisplay();
                RefreshCharactersDisplay();
            }
        }

        void SelectCharacter(HeroCharacterId id)
        {
            CharacterSelection.Select(id);
            RefreshCharacterDisplay();
        }

        void RefreshOptionsDisplay()
        {
            if (optionsText == null)
                return;

            if (resetSaveConfirmationPending)
            {
                optionsText.text = Localization.T("ui.reset_warning");
                return;
            }

            optionsText.text = Localization.T("ui.options_help");
        }

        void HandleResetSaveClick()
        {
            if (!resetSaveConfirmationPending)
            {
                resetSaveConfirmationPending = true;
                UpdateResetSaveButtonLabel();
                RefreshOptionsDisplay();
                optionsNavigation.RefreshVisuals();
                return;
            }

            resetSaveConfirmationPending = false;
            metaProgression?.ResetAllSaveData();
            UpdateResetSaveButtonLabel();
            RefreshOptionsDisplay();
            RefreshShopDisplay();
            RefreshCharacterDisplay();
            RefreshHighScoresDisplay();
            optionsNavigation.RefreshVisuals();
        }

        void UpdateResetSaveButtonLabel()
        {
            if (optionsResetButton == null)
                return;

            Text label = optionsResetButton.GetComponentInChildren<Text>();
            if (label != null)
                label.text = resetSaveConfirmationPending
                    ? Localization.T("ui.confirm_reset")
                    : Localization.T("ui.reset_save");
        }

        void RefreshHighScoresDisplay()
        {
            if (highScoresText == null)
                return;

            if (HighScoreTable.Entries.Count == 0)
            {
                highScoresText.text = Localization.T("ui.highscores_empty");
                return;
            }

            highScoresText.text = Localization.T("ui.highscores_header");
            for (int i = 0; i < HighScoreTable.Entries.Count; i++)
            {
                HighScoreEntry entry = HighScoreTable.Entries[i];
                string date = Localization.FormatRecordDate(entry.recordedAtUtcTicks);
                highScoresText.text +=
                    $"{i + 1,2}.   {entry.score,7}   {entry.distance,7:0}m  {entry.enemies,4}   {date}\n";
            }
        }

        void HandleLanguageChanged()
        {
            Localization.TranslateStaticText(canvasRoot != null ? canvasRoot.transform : null);

            if (optionsLanguageButton != null)
            {
                Text label = optionsLanguageButton.GetComponentInChildren<Text>();
                if (label != null)
                    label.text = Localization.GetLanguageButtonLabel();
            }

            UpdateResetSaveButtonLabel();
            RefreshMetaDisplay();
            RefreshCharacterDisplay();
            RefreshShopDisplay();
            RefreshCharactersDisplay();
            RefreshOptionsDisplay();
            RefreshHighScoresDisplay();
        }

        void TryBuy(ShopUpgradeId id)
        {
            if (metaProgression == null)
                return;

            metaProgression.TryPurchase(id);
            RefreshShopDisplay();
            RefreshMetaDisplay();
        }

        static void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
