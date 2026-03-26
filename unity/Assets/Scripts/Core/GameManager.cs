using System.Collections;
using UnityEngine;
using RuneRealms.Data;
using RuneRealms.UI;

namespace RuneRealms.Core
{
    /// <summary>
    /// Main game orchestrator. Attach to a persistent GameObject in the scene.
    /// Receives init data from DevvitBridge and distributes to all managers.
    /// Handles auto-save every 30 seconds.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Managers")]
        [SerializeField] private Skills.IdleManager idleManager;
        [SerializeField] private Combat.CombatManager combatManager;
        [SerializeField] private Inventory.InventoryManager inventoryManager;
        [SerializeField] private Leaderboard.LeaderboardManager leaderboardManager;
        [SerializeField] private TabManager tabManager;
        [SerializeField] private UI.OfflinePopup offlinePopup;
        [SerializeField] private TutorialManager tutorialManager;

        [Header("Settings")]
        [SerializeField] private float autoSaveInterval = 30f;

        // Current player state
        public string Username { get; private set; }
        public string PostId { get; private set; }
        public bool IsNewPlayer { get; private set; }
        public TutorialProgress CurrentTutorial { get; set; }
        public PlayerSkills CurrentSkills { get; set; }
        public PlayerInventory CurrentInventory { get; set; }
        public PlayerProfile CurrentProfile { get; set; }

        private Coroutine autoSaveCoroutine;
        private bool isInitialized;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // Listen for init data from DevvitBridge
            if (DevvitBridge.Instance != null)
            {
                DevvitBridge.Instance.OnInitDataReceived += HandleInitData;
            }
            else
            {
                Debug.LogError("[GameManager] DevvitBridge not found! Make sure it exists in the scene.");
            }
        }

        private void OnDestroy()
        {
            if (DevvitBridge.Instance != null)
            {
                DevvitBridge.Instance.OnInitDataReceived -= HandleInitData;
            }
        }

        private void HandleInitData(InitResponse data)
        {
            Debug.Log($"[GameManager] Init received for {data.username} (new: {data.isNewPlayer})");

            Username = data.username;
            PostId = data.postId;
            IsNewPlayer = data.isNewPlayer;
            CurrentSkills = data.skills;
            CurrentInventory = data.inventory;
            CurrentProfile = data.profile;
            CurrentTutorial = data.tutorial;

            // Distribute data to managers
            if (idleManager != null)
                idleManager.Initialize(data.skills);

            if (combatManager != null)
                combatManager.Initialize(data.skills, data.profile);

            if (inventoryManager != null)
                inventoryManager.Initialize(data.inventory);

            if (leaderboardManager != null)
                leaderboardManager.RefreshLeaderboard();

            // Show offline XP popup if applicable
            if (!data.isNewPlayer && data.offlineXpGained != null && data.offlineXpGained.Count > 0)
            {
                if (offlinePopup != null)
                    offlinePopup.Show(data.offlineXpGained);
            }

            // Start auto-save
            if (autoSaveCoroutine != null)
                StopCoroutine(autoSaveCoroutine);
            autoSaveCoroutine = StartCoroutine(AutoSaveLoop());

            // Default to skills tab
            if (tabManager != null)
                tabManager.SwitchTab(0);

            // Show tutorial for new or incomplete players
            if (CurrentTutorial != null && !CurrentTutorial.completed)
            {
                if (tutorialManager != null)
                    tutorialManager.Show(CurrentTutorial);
            }

            isInitialized = true;
        }

        private IEnumerator AutoSaveLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(autoSaveInterval);
                SaveAll();
            }
        }

        public void SaveAll()
        {
            if (!isInitialized) return;

            Debug.Log("[GameManager] Auto-saving...");

            // Update timestamps
            CurrentSkills.lastSaveTimestamp = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // Save skills
            DevvitBridge.Instance?.SaveSkills(CurrentSkills);
        }

        public void OnTabChanged(int tabIndex)
        {
            // Save when switching tabs
            SaveAll();

            // Refresh leaderboard when switching to that tab
            if (tabIndex == 3 && leaderboardManager != null)
            {
                leaderboardManager.RefreshLeaderboard();
            }
        }

        // Called by CombatManager when a fight ends
        public void OnArenaComplete(ArenaResult result)
        {
            DevvitBridge.Instance?.SaveArenaResult(result, response =>
            {
                if (response != null && response.success)
                {
                    CurrentProfile.arenaKills = response.totalKills;

                    // Add new items to local inventory
                    if (response.newItems != null)
                    {
                        foreach (var item in response.newItems)
                        {
                            inventoryManager?.AddItem(item);
                        }
                    }
                }
            });
        }

        // Mobile lifecycle
        private void OnApplicationPause(bool paused)
        {
            if (paused && isInitialized)
            {
                SaveAll();
            }
        }

        private void OnApplicationQuit()
        {
            if (isInitialized)
            {
                SaveAll();
            }
        }
    }
}
