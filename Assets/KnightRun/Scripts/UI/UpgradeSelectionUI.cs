using System.Collections.Generic;
using KnightRun.Core;
using KnightRun.Player;
using KnightRun.Progression;
using UnityEngine;
using UnityEngine.UI;

namespace KnightRun.UI
{
    public class UpgradeSelectionUI : MonoBehaviour
    {
        GameObject panelRoot;
        Transform optionButtonsRoot;
        Text titleText;
        Text hintText;

        readonly List<Button> optionButtons = new List<Button>();
        readonly MenuNavigation optionNavigation = new MenuNavigation();

        UpgradeManager upgradeManager;
        HeroUpgradeStats heroStats;
        UpgradeOffer[] currentOptions;

        public void Build()
        {
            UiFactory.EnsureEventSystem();

            var canvas = GetComponentInChildren<Canvas>();
            if (canvas == null)
                return;

            panelRoot = new GameObject("UpgradePanel");
            panelRoot.transform.SetParent(canvas.transform, false);

            var panelImage = panelRoot.AddComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.82f);
            panelImage.raycastTarget = true;

            var panelRect = panelRoot.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            titleText = CreateText(panelRoot.transform, "Level Up!", 34, TextAnchor.UpperCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -80f));

            var buttonsRootGo = new GameObject("OptionButtons");
            buttonsRootGo.transform.SetParent(panelRoot.transform, false);
            var buttonsRootRect = buttonsRootGo.AddComponent<RectTransform>();
            buttonsRootRect.anchorMin = Vector2.zero;
            buttonsRootRect.anchorMax = Vector2.one;
            buttonsRootRect.offsetMin = Vector2.zero;
            buttonsRootRect.offsetMax = Vector2.zero;
            optionButtonsRoot = buttonsRootGo.transform;

            hintText = CreateText(panelRoot.transform, string.Empty, 20, TextAnchor.LowerCenter,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 50f));

            panelRoot.SetActive(false);

            upgradeManager = UpgradeManager.Instance;
            heroStats = FindFirstObjectByType<HeroUpgradeStats>();

            if (upgradeManager != null)
                upgradeManager.OnUpgradeOffered += ShowOffer;
        }

        void Update()
        {
            if (upgradeManager == null || currentOptions == null)
                return;

            if (GameManager.Instance == null || GameManager.Instance.State != GameState.ChoosingUpgrade)
                return;

            optionNavigation.HandleInput(true);

            if (KnightInput.GetKeyDown(KeyCode.Alpha1) || KnightInput.GetKeyDown(KeyCode.Keypad1))
                SelectOption(0);
            if (KnightInput.GetKeyDown(KeyCode.Alpha2) || KnightInput.GetKeyDown(KeyCode.Keypad2))
                SelectOption(1);
            if (KnightInput.GetKeyDown(KeyCode.Alpha3) || KnightInput.GetKeyDown(KeyCode.Keypad3))
                SelectOption(2);
        }

        void OnDestroy()
        {
            if (upgradeManager != null)
                upgradeManager.OnUpgradeOffered -= ShowOffer;
        }

        void ShowOffer(UpgradeOffer[] options)
        {
            currentOptions = options;
            panelRoot.SetActive(true);

            if (heroStats == null)
                heroStats = FindFirstObjectByType<HeroUpgradeStats>();

            if (titleText != null && upgradeManager != null)
                titleText.text = $"Level Up! Proximo em {upgradeManager.XpRequiredForNextLevel - upgradeManager.XpTowardNextLevel} XP";

            if (hintText != null)
                hintText.text = "Setas + Enter, 1/2/3 ou clique para escolher";

            RebuildOptionButtons(options);
        }

        void RebuildOptionButtons(UpgradeOffer[] options)
        {
            ClearOptionButtons();

            if (options == null)
            {
                optionNavigation.Clear();
                return;
            }

            for (int i = 0; i < options.Length; i++)
            {
                int index = i;
                string label = BuildOptionLabel(options[i], i, options);
                Button button = UiFactory.CreateButton(
                    optionButtonsRoot,
                    label,
                    new Vector2(0f, 70f - i * 95f),
                    () => SelectOption(index),
                    new Vector2(760f, 86f),
                    22);
                optionButtons.Add(button);
            }

            optionNavigation.SetButtons(optionButtons);
        }

        string BuildOptionLabel(UpgradeOffer offer, int index, UpgradeOffer[] options)
        {
            if (offer.IsCoinReward)
            {
                return skillSlotsExhausted(options)
                    ? $"{index + 1}. +{offer.CoinAmount} moedas\nTodos os upgrades escolhidos estao no maximo"
                    : $"{index + 1}. +{offer.CoinAmount} moedas\nRecompensa em moedas";
            }

            if (heroStats == null)
                return string.Empty;

            SkillDefinition skill = offer.Skill;
            int currentLevel = heroStats.GetLevel(skill.Id);
            int nextLevel = currentLevel + 1;
            string category = skill.Category == UpgradeCategory.Weapon ? "Arma" : "Skill";
            string levelLabel = currentLevel == 0 ? "Nv 0 -> 1" : $"Nv {currentLevel} -> {nextLevel}";
            return $"{index + 1}. [{category}] {skill.DisplayName} ({levelLabel})\n{SkillPool.GetDescription(skill.Id, nextLevel)}";
        }

        void SelectOption(int index)
        {
            if (upgradeManager == null || currentOptions == null)
                return;

            if (index < 0 || index >= currentOptions.Length)
                return;

            upgradeManager.SelectUpgrade(index);
        }

        void ClearOptionButtons()
        {
            foreach (Button button in optionButtons)
            {
                if (button != null)
                    Destroy(button.gameObject);
            }

            optionButtons.Clear();
            optionNavigation.Clear();
        }

        void HideOffer()
        {
            currentOptions = null;
            ClearOptionButtons();
            if (panelRoot != null)
                panelRoot.SetActive(false);
        }

        void LateUpdate()
        {
            if (panelRoot == null || !panelRoot.activeSelf)
                return;

            if (GameManager.Instance != null && GameManager.Instance.State != GameState.ChoosingUpgrade)
                HideOffer();
        }

        static Text CreateText(Transform parent, string content, int fontSize, TextAnchor anchor, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = AnchorToPivot(anchor);
            rect.sizeDelta = new Vector2(760f, 90f);
            rect.anchoredPosition = anchoredPosition;

            var text = go.AddComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.color = Color.white;
            text.raycastTarget = false;

            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.75f);
            shadow.effectDistance = new Vector2(1.5f, -1.5f);

            return text;
        }

        static bool skillSlotsExhausted(UpgradeOffer[] options)
        {
            if (options == null)
                return true;

            foreach (UpgradeOffer offer in options)
            {
                if (!offer.IsCoinReward)
                    return false;
            }

            return true;
        }

        static Vector2 AnchorToPivot(TextAnchor anchor)
        {
            return anchor switch
            {
                TextAnchor.UpperCenter => new Vector2(0.5f, 1f),
                TextAnchor.MiddleCenter => new Vector2(0.5f, 0.5f),
                TextAnchor.LowerCenter => new Vector2(0.5f, 0f),
                _ => new Vector2(0.5f, 0.5f)
            };
        }
    }
}
