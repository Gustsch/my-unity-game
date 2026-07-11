using System.Collections.Generic;
using KnightRun.Player;
using UnityEngine;
using UnityEngine.UI;

namespace KnightRun.UI
{
    public class MenuNavigation
    {
        static readonly Color NormalColor = new Color(0.16f, 0.2f, 0.3f, 0.95f);
        static readonly Color SelectedColor = new Color(0.28f, 0.42f, 0.62f, 0.98f);

        readonly List<Button> buttons = new List<Button>();
        int selectedIndex;

        public void SetButtons(IEnumerable<Button> items)
        {
            buttons.Clear();
            if (items == null)
            {
                RefreshVisuals();
                return;
            }

            buttons.AddRange(items);
            selectedIndex = 0;
            RefreshVisuals();
        }

        public void Clear()
        {
            buttons.Clear();
            selectedIndex = 0;
        }

        public void HandleInput(bool active)
        {
            if (!active || buttons.Count == 0)
                return;

            if (KnightInput.GetKeyDown(KeyCode.UpArrow) || KnightInput.GetKeyDown(KeyCode.W))
            {
                selectedIndex = (selectedIndex - 1 + buttons.Count) % buttons.Count;
                RefreshVisuals();
            }

            if (KnightInput.GetKeyDown(KeyCode.DownArrow) || KnightInput.GetKeyDown(KeyCode.S))
            {
                selectedIndex = (selectedIndex + 1) % buttons.Count;
                RefreshVisuals();
            }

            if (KnightInput.GetKeyDown(KeyCode.Return) || KnightInput.GetKeyDown(KeyCode.Space))
                buttons[selectedIndex].onClick.Invoke();
        }

        public void RefreshVisuals()
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                if (buttons[i] == null)
                    continue;

                Image image = buttons[i].GetComponent<Image>();
                if (image != null)
                    image.color = i == selectedIndex ? SelectedColor : NormalColor;
            }
        }
    }
}
