using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RuneRealms.Data
{
    // === Skills ===
    [Serializable]
    public class SkillData
    {
        public string name;
        public int xp;
        public int level;
    }

    [Serializable]
    public class PlayerSkills
    {
        public List<SkillData> skills;
        public long lastSaveTimestamp;
        public string activeSkill; // null when no skill is active
    }

    // === Combat ===
    [Serializable]
    public class LootItem
    {
        public string itemId;
        public string name;
        public int quantity;
        public string rarity; // "common", "uncommon", "rare", "legendary"
    }

    [Serializable]
    public class ArenaResult
    {
        public int kills;
        public List<LootItem> loot;
        public int wavesCleared;
        public int damageDealt;
    }

    // === Inventory ===
    [Serializable]
    public class InventoryItem
    {
        public string itemId;
        public string name;
        public int quantity;
        public string category; // "weapon", "armor", "tool", "material", "consumable"
        public string rarity;
    }

    [Serializable]
    public class PlayerInventory
    {
        public List<InventoryItem> items;
        public int maxSlots;
    }

    // === Player Profile ===
    [Serializable]
    public class PlayerProfile
    {
        public string username;
        public int totalLevel;
        public int totalXp;
        public int arenaKills;
        public long joinedTimestamp;
    }

    // === Leaderboard ===
    [Serializable]
    public class LeaderboardEntry
    {
        public string username;
        public float score;
        public int rank;
    }

    // === Tutorial ===
    [Serializable]
    public class TutorialProgress
    {
        public bool completed;
        public int currentStep;
        public long? completedAt; // timestamp, null if not completed
    }

    // === API Responses ===
    [Serializable]
    public class InitResponse
    {
        public string type;
        public string postId;
        public string username;
        public string snoovatarUrl;
        public bool isNewPlayer;
        public PlayerSkills skills;
        public PlayerInventory inventory;
        public PlayerProfile profile;
        public Dictionary<string, int> offlineXpGained;
        public TutorialProgress tutorial;
    }

    [Serializable]
    public class SaveSkillsRequest
    {
        public string type = "save-skills";
        public PlayerSkills skills;
    }

    [Serializable]
    public class SaveSkillsResponse
    {
        public string type;
        public bool success;
        public string message;
    }

    [Serializable]
    public class SaveArenaResultRequest
    {
        public string type = "save-arena-result";
        public ArenaResult result;
    }

    [Serializable]
    public class SaveArenaResultResponse
    {
        public string type;
        public bool success;
        public List<InventoryItem> newItems;
        public int totalKills;
    }

    [Serializable]
    public class GetInventoryResponse
    {
        public string type;
        public PlayerInventory inventory;
    }

    [Serializable]
    public class UseItemRequest
    {
        public string type = "use-item";
        public string itemId;
    }

    [Serializable]
    public class UseItemResponse
    {
        public string type;
        public bool success;
        public PlayerInventory inventory;
        public string message;
    }

    [Serializable]
    public class GetLeaderboardResponse
    {
        public string type;
        public List<LeaderboardEntry> entries;
        public int? playerRank;
    }

    [Serializable]
    public class SaveTutorialRequest
    {
        public string type = "save-tutorial";
        public TutorialProgress tutorial;
    }

    [Serializable]
    public class SaveTutorialResponse
    {
        public string type;
        public bool success;
    }
}
