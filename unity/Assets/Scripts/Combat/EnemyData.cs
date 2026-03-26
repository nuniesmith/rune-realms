using System.Collections.Generic;
using UnityEngine;

namespace RuneRealms.Combat
{
    /// <summary>
    /// Difficulty tiers for enemies. Used by EnemyDatabase to filter enemies for wave selection.
    /// </summary>
    public enum EnemyDifficulty
    {
        Easy,
        Medium,
        Hard,
        Boss
    }

    /// <summary>
    /// Static configuration for an enemy type. Create via Assets > Create > Rune Realms > Enemy Data.
    /// </summary>
    [CreateAssetMenu(fileName = "NewEnemy", menuName = "Rune Realms/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("Identity")]
        public string enemyName = "Goblin";
        [TextArea(2, 4)]
        public string description = "A common foe found lurking in dark places.";
        public Sprite icon;

        [Header("Difficulty")]
        public EnemyDifficulty difficulty = EnemyDifficulty.Easy;

        [Tooltip("Minimum wave number this enemy can appear in.")]
        public int minWave = 1;

        [Header("Combat Stats")]
        public int maxHp = 30;
        public int damage = 5;
        public float attackInterval = 2f;

        [Header("Rewards")]
        [Tooltip("XP granted to a combat-relevant skill on kill.")]
        public int xpReward = 15;

        [Header("Special")]
        [Tooltip("Optional description of special behavior (e.g. 'Casts firebolt every 3rd attack').")]
        public string specialAbility;

        [Header("Loot Table")]
        public List<LootDrop> lootTable;

        [System.Serializable]
        public class LootDrop
        {
            public string itemId;
            public string itemName;
            public string rarity = "common"; // "common", "uncommon", "rare", "legendary"
            [Range(0f, 1f)]
            public float dropChance = 0.5f;
            public int minQuantity = 1;
            public int maxQuantity = 1;
        }
    }
}
