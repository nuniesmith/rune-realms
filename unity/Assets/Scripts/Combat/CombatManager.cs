using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RuneRealms.Data;
using RuneRealms.Core;

namespace RuneRealms.Combat
{
    /// <summary>
    /// Manages arena combat. Wave-based system where enemies get progressively harder.
    /// Skill levels affect combat stats. Attach to the Arena tab panel.
    /// </summary>
    public class CombatManager : MonoBehaviour
    {
        [Header("Enemy Config")]
        [SerializeField] private List<EnemyData> enemyTypes;

        [Tooltip("Optional: assign an EnemyDatabase for wave-aware enemy selection. Falls back to enemyTypes list if unset.")]
        [SerializeField] private EnemyDatabase enemyDatabase;

        [Header("Player UI")]
        [SerializeField] private Slider playerHpBar;
        [SerializeField] private TextMeshProUGUI playerHpText;
        [SerializeField] private TextMeshProUGUI playerStatsText;

        [Header("Enemy UI")]
        [SerializeField] private Slider enemyHpBar;
        [SerializeField] private TextMeshProUGUI enemyHpText;
        [SerializeField] private TextMeshProUGUI enemyNameText;
        [SerializeField] private Image enemyIcon;

        [Header("Combat UI")]
        [SerializeField] private Button attackButton;
        [SerializeField] private Button specialButton;
        [SerializeField] private Button healButton;
        [SerializeField] private Button runButton;
        [SerializeField] private TextMeshProUGUI waveText;
        [SerializeField] private TextMeshProUGUI killCountText;
        [SerializeField] private TextMeshProUGUI lootText;
        [SerializeField] private TextMeshProUGUI xpRewardText;

        [Header("State")]
        [SerializeField] private GameObject combatPanel;
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private Button startBattleButton;

        // Combat-relevant skill names used for XP reward distribution
        private static readonly string[] CombatSkills = { "mining", "smithing", "woodcutting", "attack", "strength", "defence" };

        // Combat state
        private int playerMaxHp;
        private int playerCurrentHp;
        private int playerDamage;
        private int playerDefense;
        private int playerSpecialDamage;
        private int healAmount;

        private int currentWave = 0;
        private int sessionKills = 0;
        private int sessionDamageDealt = 0;
        private int sessionXpEarned = 0;
        private List<LootItem> sessionLoot = new List<LootItem>();

        private EnemyData currentEnemyData;
        private int enemyCurrentHp;
        private Coroutine enemyAttackCoroutine;
        private bool isInCombat;

        // Reference to player skills so we can grant XP on kills
        private PlayerSkills playerSkills;

        public void Initialize(PlayerSkills skills, PlayerProfile profile)
        {
            playerSkills = skills;
            CalculatePlayerStats(skills);
            UpdatePlayerUI();

            // Hook up buttons
            if (attackButton != null)
            {
                attackButton.onClick.RemoveAllListeners();
                attackButton.onClick.AddListener(OnAttack);
            }
            if (specialButton != null)
            {
                specialButton.onClick.RemoveAllListeners();
                specialButton.onClick.AddListener(OnSpecialAttack);
            }
            if (healButton != null)
            {
                healButton.onClick.RemoveAllListeners();
                healButton.onClick.AddListener(OnHeal);
            }
            if (runButton != null)
            {
                runButton.onClick.RemoveAllListeners();
                runButton.onClick.AddListener(OnRun);
            }
            if (startBattleButton != null)
            {
                startBattleButton.onClick.RemoveAllListeners();
                startBattleButton.onClick.AddListener(StartNewRun);
            }

            // Start in non-combat state
            SetCombatUI(false);
        }

        private void CalculatePlayerStats(PlayerSkills skills)
        {
            // Base stats
            int miningLevel = 1, fishingLevel = 1, cookingLevel = 1, smithingLevel = 1, woodcuttingLevel = 1;
            int attackLevel = 1, strengthLevel = 1, defenceLevel = 1;

            foreach (var skill in skills.skills)
            {
                switch (skill.name)
                {
                    case "mining": miningLevel = skill.level; break;
                    case "fishing": fishingLevel = skill.level; break;
                    case "cooking": cookingLevel = skill.level; break;
                    case "smithing": smithingLevel = skill.level; break;
                    case "woodcutting": woodcuttingLevel = skill.level; break;
                    case "attack": attackLevel = skill.level; break;
                    case "strength": strengthLevel = skill.level; break;
                    case "defence": defenceLevel = skill.level; break;
                }
            }

            // Skill → Combat stat mapping
            playerDamage = 10 + (miningLevel * 2) + (attackLevel * 3);           // Mining + Attack → +damage
            playerMaxHp = 50 + (fishingLevel * 10);                               // Fishing → +max HP
            healAmount = 10 + (cookingLevel * 3);                                 // Cooking → +healing
            playerDefense = miningLevel + (smithingLevel * 2) + (defenceLevel * 2); // Smithing + Defence → +defense
            playerSpecialDamage = 20 + (woodcuttingLevel * 3) + (strengthLevel * 4); // Woodcutting + Strength → +special/crit damage

            playerCurrentHp = playerMaxHp;
        }

        private void StartNewRun()
        {
            currentWave = 0;
            sessionKills = 0;
            sessionDamageDealt = 0;
            sessionXpEarned = 0;
            sessionLoot.Clear();
            playerCurrentHp = playerMaxHp;

            UpdatePlayerUI();
            ClearXpRewardText();
            SetCombatUI(true);
            SpawnNextWave();
        }

        private void SpawnNextWave()
        {
            currentWave++;

            if (waveText != null)
                waveText.text = $"Wave {currentWave}";

            // --- Enemy selection: prefer EnemyDatabase, fall back to legacy enemyTypes list ---
            if (enemyDatabase != null && enemyDatabase.Count > 0)
            {
                currentEnemyData = enemyDatabase.GetRandomEnemy(currentWave);

                if (currentEnemyData == null)
                {
                    Debug.LogError($"[CombatManager] EnemyDatabase returned no enemies for wave {currentWave}!");
                    return;
                }
            }
            else
            {
                // Legacy path: pick from the flat enemyTypes list
                if (enemyTypes == null || enemyTypes.Count == 0)
                {
                    Debug.LogError("[CombatManager] No enemy types configured!");
                    return;
                }

                // Higher waves unlock harder enemies
                int maxEnemyIndex = Mathf.Min(currentWave - 1, enemyTypes.Count - 1);
                int enemyIndex = Random.Range(0, maxEnemyIndex + 1);
                currentEnemyData = enemyTypes[enemyIndex];
            }

            // Scale enemy HP with wave
            float waveMultiplier = 1f + (currentWave - 1) * 0.3f;
            enemyCurrentHp = Mathf.RoundToInt(currentEnemyData.maxHp * waveMultiplier);

            UpdateEnemyUI();
            StartEnemyAttacks();

            Debug.Log($"[CombatManager] Wave {currentWave}: {currentEnemyData.enemyName} (HP: {enemyCurrentHp}, Difficulty: {currentEnemyData.difficulty})");
        }

        // --- Player Actions ---

        public void OnAttack()
        {
            if (!isInCombat) return;

            int damage = playerDamage + Random.Range(-2, 3); // Small variance
            damage = Mathf.Max(1, damage);
            DealDamageToEnemy(damage);

            if (attackButton != null)
                UI.FloatingText.Spawn($"-{damage}", attackButton.transform.position + Vector3.up * 50, Color.white);
        }

        public void OnSpecialAttack()
        {
            if (!isInCombat) return;

            int damage = playerSpecialDamage + Random.Range(-3, 5);
            damage = Mathf.Max(1, damage);
            DealDamageToEnemy(damage);

            if (specialButton != null)
                UI.FloatingText.Spawn($"-{damage}!", specialButton.transform.position + Vector3.up * 50, Color.yellow);
        }

        public void OnHeal()
        {
            if (!isInCombat) return;

            int healed = Mathf.Min(healAmount, playerMaxHp - playerCurrentHp);
            if (healed <= 0) return;

            playerCurrentHp += healed;
            UpdatePlayerUI();

            if (healButton != null)
                UI.FloatingText.Spawn($"+{healed} HP", healButton.transform.position + Vector3.up * 50, Color.green);
        }

        public void OnRun()
        {
            // End the combat run, save results
            EndCombatRun();
        }

        // --- Combat Logic ---

        private void DealDamageToEnemy(int damage)
        {
            enemyCurrentHp -= damage;
            sessionDamageDealt += damage;
            UpdateEnemyUI();

            if (enemyCurrentHp <= 0)
            {
                OnEnemyDefeated();
            }
        }

        private void DealDamageToPlayer(int rawDamage)
        {
            // Apply defense reduction
            int mitigated = Mathf.Max(0, rawDamage - playerDefense / 2);
            int damage = Mathf.Max(1, mitigated); // Always take at least 1

            playerCurrentHp -= damage;
            UpdatePlayerUI();

            if (playerHpBar != null)
                UI.FloatingText.Spawn($"-{damage}", playerHpBar.transform.position, Color.red);

            if (playerCurrentHp <= 0)
            {
                OnPlayerDefeated();
            }
        }

        private void OnEnemyDefeated()
        {
            sessionKills++;
            StopEnemyAttacks();

            // --- Grant XP reward ---
            GrantCombatXp();

            // Roll loot
            if (currentEnemyData != null && currentEnemyData.lootTable != null)
            {
                foreach (var drop in currentEnemyData.lootTable)
                {
                    if (Random.value <= drop.dropChance)
                    {
                        var loot = new LootItem
                        {
                            itemId = drop.itemId,
                            name = drop.itemName,
                            quantity = Random.Range(drop.minQuantity, drop.maxQuantity + 1),
                            rarity = drop.rarity
                        };
                        sessionLoot.Add(loot);

                        if (lootText != null)
                            lootText.text = $"Loot: {loot.name} x{loot.quantity}!";

                        Debug.Log($"[CombatManager] Loot drop: {loot.name} x{loot.quantity} ({loot.rarity})");
                    }
                }
            }

            if (killCountText != null)
                killCountText.text = $"Kills: {sessionKills}";

            // Short delay then next wave
            StartCoroutine(NextWaveDelay());
        }

        /// <summary>
        /// Grants the enemy's xpReward to a random combat-relevant skill.
        /// Combat skills: mining, smithing, woodcutting.
        /// </summary>
        private void GrantCombatXp()
        {
            if (currentEnemyData == null) return;

            int xpReward = currentEnemyData.xpReward;
            if (xpReward <= 0) return;

            // Pick a random combat-relevant skill to reward
            string chosenSkillName = CombatSkills[Random.Range(0, CombatSkills.Length)];

            if (playerSkills != null && playerSkills.skills != null)
            {
                foreach (var skill in playerSkills.skills)
                {
                    if (skill.name == chosenSkillName)
                    {
                        skill.xp += xpReward;
                        sessionXpEarned += xpReward;

                        Debug.Log($"[CombatManager] Granted {xpReward} XP to {chosenSkillName} (total: {skill.xp})");

                        // Show XP reward in UI
                        if (xpRewardText != null)
                            xpRewardText.text = $"+{xpReward} XP ({chosenSkillName})";

                        // Floating text near enemy
                        if (enemyHpBar != null)
                            UI.FloatingText.Spawn($"+{xpReward} XP", enemyHpBar.transform.position + Vector3.up * 30, Color.cyan);

                        break;
                    }
                }
            }
        }

        private IEnumerator NextWaveDelay()
        {
            yield return new WaitForSeconds(1f);
            SpawnNextWave();
        }

        private void OnPlayerDefeated()
        {
            Debug.Log("[CombatManager] Player defeated!");
            playerCurrentHp = 0;
            UpdatePlayerUI();
            EndCombatRun();
        }

        private void EndCombatRun()
        {
            StopEnemyAttacks();
            isInCombat = false;

            // Build arena result and send to server
            var result = new ArenaResult
            {
                kills = sessionKills,
                loot = new List<LootItem>(sessionLoot),
                wavesCleared = currentWave - 1,
                damageDealt = sessionDamageDealt
            };

            GameManager.Instance?.OnArenaComplete(result);

            // Show results
            SetCombatUI(false);

            // Show session XP summary
            if (xpRewardText != null && sessionXpEarned > 0)
                xpRewardText.text = $"Total XP earned: {sessionXpEarned}";

            Debug.Log($"[CombatManager] Run ended. Waves: {result.wavesCleared}, Kills: {result.kills}, Loot items: {result.loot.Count}, XP earned: {sessionXpEarned}");
        }

        // --- Enemy AI ---

        private void StartEnemyAttacks()
        {
            StopEnemyAttacks();
            isInCombat = true;
            enemyAttackCoroutine = StartCoroutine(EnemyAttackLoop());
        }

        private void StopEnemyAttacks()
        {
            if (enemyAttackCoroutine != null)
            {
                StopCoroutine(enemyAttackCoroutine);
                enemyAttackCoroutine = null;
            }
        }

        private IEnumerator EnemyAttackLoop()
        {
            while (isInCombat && enemyCurrentHp > 0)
            {
                float interval = currentEnemyData != null ? currentEnemyData.attackInterval : 2f;
                yield return new WaitForSeconds(interval);

                if (!isInCombat) yield break;

                int damage = currentEnemyData != null ? currentEnemyData.damage : 5;
                damage += Random.Range(-1, 2); // Small variance
                damage = Mathf.Max(1, damage);

                DealDamageToPlayer(damage);
            }
        }

        // --- UI Updates ---

        private void UpdatePlayerUI()
        {
            if (playerHpBar != null)
            {
                playerHpBar.maxValue = playerMaxHp;
                playerHpBar.value = playerCurrentHp;
            }
            if (playerHpText != null)
                playerHpText.text = $"{playerCurrentHp}/{playerMaxHp}";

            if (playerStatsText != null)
                playerStatsText.text = $"ATK: {playerDamage}  DEF: {playerDefense}  SPL: {playerSpecialDamage}";
        }

        private void UpdateEnemyUI()
        {
            if (currentEnemyData == null) return;

            int maxHp = Mathf.RoundToInt(currentEnemyData.maxHp * (1f + (currentWave - 1) * 0.3f));

            if (enemyHpBar != null)
            {
                enemyHpBar.maxValue = maxHp;
                enemyHpBar.value = Mathf.Max(0, enemyCurrentHp);
            }
            if (enemyHpText != null)
                enemyHpText.text = $"{Mathf.Max(0, enemyCurrentHp)}/{maxHp}";

            if (enemyNameText != null)
                enemyNameText.text = currentEnemyData.enemyName;

            if (enemyIcon != null && currentEnemyData.icon != null)
                enemyIcon.sprite = currentEnemyData.icon;
        }

        private void SetCombatUI(bool inCombat)
        {
            if (combatPanel != null) combatPanel.SetActive(inCombat);
            if (victoryPanel != null) victoryPanel.SetActive(!inCombat);

            if (attackButton != null) attackButton.interactable = inCombat;
            if (specialButton != null) specialButton.interactable = inCombat;
            if (healButton != null) healButton.interactable = inCombat;
            if (runButton != null) runButton.interactable = inCombat;
        }

        private void ClearXpRewardText()
        {
            if (xpRewardText != null)
                xpRewardText.text = "";
        }
    }
}
