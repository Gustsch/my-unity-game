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
        Text titleText;
        Text[] optionTexts = new Text[3];
        Text hintText;

        UpgradeManager upgradeManager;
        HeroUpgradeStats heroStats;
        SkillDefinition[] currentOptions;

        public void Build()
        {
            var canvas = GetComponentInChildren<Canvas>();
            if (canvas == null)
                return;

            panelRoot = new GameObject("UpgradePanel");
            panelRoot.transform.SetParent(canvas.transform, false);

            var panelImage = panelRoot.AddComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.82f);

            var panelRect = panelRoot.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            titleText = CreateText(panelRoot.transform, "Level Up!", 34, TextAnchor.UpperCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -80f));

            for (int i = 0; i < 3; i++)
            {
                optionTexts[i] = CreateText(panelRoot.transform, string.Empty, 24, TextAnchor.MiddleCenter,
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 70f - i * 95f));
            }

            hintText = CreateText(panelRoot.transform, "Pressione 1, 2 ou 3 para escolher", 20, TextAnchor.LowerCenter,
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

            if (KnightInput.GetKeyDown(KeyCode.Alpha1) || KnightInput.GetKeyDown(KeyCode.Keypad1))
                upgradeManager.SelectUpgrade(0);
            if (KnightInput.GetKeyDown(KeyCode.Alpha2) || KnightInput.GetKeyDown(KeyCode.Keypad2))
                upgradeManager.SelectUpgrade(1);
            if (KnightInput.GetKeyDown(KeyCode.Alpha3) || KnightInput.GetKeyDown(KeyCode.Keypad3))
                upgradeManager.SelectUpgrade(2);
        }

        void OnDestroy()
        {
            if (upgradeManager != null)
                upgradeManager.OnUpgradeOffered -= ShowOffer;
        }

        void ShowOffer(SkillDefinition[] options)
        {
            currentOptions = options;
            panelRoot.SetActive(true);

            if (heroStats == null)
                heroStats = FindFirstObjectByType<HeroUpgradeStats>();

            if (titleText != null && upgradeManager != null)
                titleText.text = $"Level Up! Proximo em {upgradeManager.KillsRequiredForNextLevel} kill(s)";

            for (int i = 0; i < optionTexts.Length; i++)
            {
                if (optionTexts[i] == null)
                    continue;

                if (options != null && i < options.Length && heroStats != null)
                {
                    SkillDefinition skill = options[i];
                    int currentLevel = heroStats.GetLevel(skill.Id);
                    int nextLevel = currentLevel + 1;
                    string category = skill.Category == UpgradeCategory.Weapon ? "Arma" : "Skill";
                    string levelLabel = currentLevel == 0 ? "Nv 0 -> 1" : $"Nv {currentLevel} -> {nextLevel}";
                    optionTexts[i].text =
                        $"{i + 1}. [{category}] {skill.DisplayName} ({levelLabel})\n{SkillPool.GetDescription(skill.Id, nextLevel)}";
                }
                else
                {
                    optionTexts[i].text = string.Empty;
                }
            }
        }

        void HideOffer()
        {
            currentOptions = null;
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

            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.75f);
            shadow.effectDistance = new Vector2(1.5f, -1.5f);

            return text;
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
