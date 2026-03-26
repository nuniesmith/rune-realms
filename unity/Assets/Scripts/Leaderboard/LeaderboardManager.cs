using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RuneRealms.Data;
using RuneRealms.Core;

namespace RuneRealms.Leaderboard
{
    /// <summary>
    /// Displays and refreshes leaderboard data.
    /// Attach to the Leaderboard tab panel.
    /// </summary>
    public class LeaderboardManager : MonoBehaviour
    {
        [Header("Type Selection")]
        [SerializeField] private Button totalLevelButton;
        [SerializeField] private Button totalXpButton;
        [SerializeField] private Button arenaKillsButton;

        [Header("List Display")]
        [SerializeField] private Transform entryContainer; // Parent for entry prefabs
        [SerializeField] private GameObject entryPrefab;    // Prefab for each leaderboard row

        [Header("Player Rank")]
        [SerializeField] private TextMeshProUGUI playerRankText;

        [Header("Colors")]
        [SerializeField] private Color goldColor = new Color(1f, 0.84f, 0f);
        [SerializeField] private Color silverColor = new Color(0.75f, 0.75f, 0.75f);
        [SerializeField] private Color bronzeColor = new Color(0.80f, 0.50f, 0.20f);
        [SerializeField] private Color normalColor = new Color(0.72f, 0.66f, 0.54f);

        private string currentType = "total-level";
        private List<GameObject> entryObjects = new List<GameObject>();

        private void Start()
        {
            if (totalLevelButton != null)
            {
                totalLevelButton.onClick.RemoveAllListeners();
                totalLevelButton.onClick.AddListener(() => SetType("total-level"));
            }
            if (totalXpButton != null)
            {
                totalXpButton.onClick.RemoveAllListeners();
                totalXpButton.onClick.AddListener(() => SetType("total-xp"));
            }
            if (arenaKillsButton != null)
            {
                arenaKillsButton.onClick.RemoveAllListeners();
                arenaKillsButton.onClick.AddListener(() => SetType("arena-kills"));
            }
        }

        public void SetType(string type)
        {
            currentType = type;
            RefreshLeaderboard();
        }

        public void RefreshLeaderboard()
        {
            if (DevvitBridge.Instance == null) return;

            DevvitBridge.Instance.FetchLeaderboard(currentType, 10, OnLeaderboardReceived);
        }

        private void OnLeaderboardReceived(GetLeaderboardResponse response)
        {
            if (response == null) return;

            // Clear existing entries
            foreach (var obj in entryObjects)
            {
                if (obj != null) Destroy(obj);
            }
            entryObjects.Clear();

            if (entryPrefab == null || entryContainer == null) return;

            // Create entries
            foreach (var entry in response.entries)
            {
                var entryObj = Instantiate(entryPrefab, entryContainer);
                entryObjects.Add(entryObj);

                // Expected prefab structure: Rank (TMP), Username (TMP), Score (TMP)
                var texts = entryObj.GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length >= 3)
                {
                    texts[0].text = $"#{entry.rank}";
                    texts[1].text = entry.username;
                    texts[2].text = FormatScore(entry.score);

                    // Color top 3
                    Color rankColor = entry.rank switch
                    {
                        1 => goldColor,
                        2 => silverColor,
                        3 => bronzeColor,
                        _ => normalColor,
                    };

                    texts[0].color = rankColor;
                }
            }

            // Update player rank
            if (playerRankText != null)
            {
                if (response.playerRank.HasValue && response.playerRank.Value > 0)
                    playerRankText.text = $"Your Rank: #{response.playerRank.Value}";
                else
                    playerRankText.text = "Your Rank: Unranked";
            }
        }

        private string FormatScore(float score)
        {
            if (score >= 1000000)
                return $"{score / 1000000f:F1}M";
            if (score >= 1000)
                return $"{score / 1000f:F1}K";
            return $"{score:F0}";
        }
    }
}
