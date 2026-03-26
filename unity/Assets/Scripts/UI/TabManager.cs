using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RuneRealms.Core;

namespace RuneRealms.UI
{
    /// <summary>
    /// Manages tab navigation between game views.
    /// Each tab has a CanvasGroup that gets toggled on/off.
    /// Attach to the root UI Canvas.
    /// </summary>
    public class TabManager : MonoBehaviour
    {
        [System.Serializable]
        public class Tab
        {
            public string name;
            public Button tabButton;
            public CanvasGroup contentPanel;
            public Image tabIcon;
            public Color activeColor = new Color(0.83f, 0.64f, 0.22f); // Gold #d4a438
            public Color inactiveColor = new Color(0.36f, 0.29f, 0.16f); // Dark brown #5c4a2a
        }

        [SerializeField] private List<Tab> tabs;
        [SerializeField] private float fadeSpeed = 8f;

        private int currentTabIndex = -1;

        private void Start()
        {
            // Hook up button clicks
            for (int i = 0; i < tabs.Count; i++)
            {
                int index = i;
                if (tabs[i].tabButton != null)
                {
                    tabs[i].tabButton.onClick.RemoveAllListeners();
                    tabs[i].tabButton.onClick.AddListener(() => SwitchTab(index));
                }
            }

            // Hide all tabs initially
            foreach (var tab in tabs)
            {
                SetCanvasGroupVisible(tab.contentPanel, false);
            }
        }

        public void SwitchTab(int index)
        {
            if (index < 0 || index >= tabs.Count) return;
            if (index == currentTabIndex) return;

            // Notify GameManager (triggers save)
            GameManager.Instance?.OnTabChanged(index);

            // Hide current tab
            if (currentTabIndex >= 0 && currentTabIndex < tabs.Count)
            {
                SetCanvasGroupVisible(tabs[currentTabIndex].contentPanel, false);
                UpdateTabButtonStyle(currentTabIndex, false);
            }

            // Show new tab
            currentTabIndex = index;
            SetCanvasGroupVisible(tabs[currentTabIndex].contentPanel, true);
            UpdateTabButtonStyle(currentTabIndex, true);

            Debug.Log($"[TabManager] Switched to tab: {tabs[currentTabIndex].name}");
        }

        private void SetCanvasGroupVisible(CanvasGroup group, bool visible)
        {
            if (group == null) return;

            group.alpha = visible ? 1f : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }

        private void UpdateTabButtonStyle(int index, bool active)
        {
            if (index >= tabs.Count) return;
            var tab = tabs[index];

            if (tab.tabIcon != null)
            {
                tab.tabIcon.color = active ? tab.activeColor : tab.inactiveColor;
            }

            // Scale active tab slightly larger
            if (tab.tabButton != null)
            {
                tab.tabButton.transform.localScale = active ? Vector3.one * 1.1f : Vector3.one;
            }
        }

        public int CurrentTab => currentTabIndex;
    }
}
