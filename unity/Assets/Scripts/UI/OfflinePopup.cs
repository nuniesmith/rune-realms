using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RuneRealms.UI
{
    /// <summary>
    /// Shows a popup when the player returns with offline XP gains.
    /// "While you were away, you gained..."
    /// Attach to a popup panel (disabled by default).
    /// </summary>
    public class OfflinePopup : MonoBehaviour
    {
        [SerializeField] private GameObject popupPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI gainsText;
        [SerializeField] private Button collectButton;

        private void Awake()
        {
            if (collectButton != null)
            {
                collectButton.onClick.RemoveAllListeners();
                collectButton.onClick.AddListener(Hide);
            }

            if (popupPanel != null)
                popupPanel.SetActive(false);
        }

        public void Show(Dictionary<string, int> xpGained)
        {
            if (popupPanel == null) return;
            if (xpGained == null || xpGained.Count == 0) return;

            if (titleText != null)
                titleText.text = "While you were away...";

            var sb = new StringBuilder();
            int totalXp = 0;

            foreach (var kvp in xpGained)
            {
                string skillName = char.ToUpper(kvp.Key[0]) + kvp.Key.Substring(1);
                sb.AppendLine($"{skillName}: +{kvp.Value} XP");
                totalXp += kvp.Value;
            }

            sb.AppendLine();
            sb.AppendLine($"Total: +{totalXp} XP");

            if (gainsText != null)
                gainsText.text = sb.ToString();

            popupPanel.SetActive(true);
        }

        public void Hide()
        {
            if (popupPanel != null)
                popupPanel.SetActive(false);
        }
    }
}
