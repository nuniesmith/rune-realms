using System.Collections.Generic;
using UnityEngine;

namespace RuneRealms.Combat
{
    /// <summary>
    /// Central database of all enemy types. Provides wave-based and difficulty-based lookups.
    /// Create via Assets > Create > Rune Realms > Enemy Database.
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyDatabase", menuName = "Rune Realms/Enemy Database")]
    public class EnemyDatabase : ScriptableObject
    {
        [Header("All Enemies")]
        [Tooltip("Master list of every EnemyData asset in the game.")]
        public List<EnemyData> enemies = new List<EnemyData>();

        /// <summary>
        /// Returns all enemies whose minWave requirement is met by the given wave number.
        /// </summary>
        public List<EnemyData> GetEnemiesForWave(int wave)
        {
            var eligible = new List<EnemyData>();
            foreach (var enemy in enemies)
            {
                if (enemy != null && wave >= enemy.minWave)
                {
                    eligible.Add(enemy);
                }
            }
            return eligible;
        }

        /// <summary>
        /// Picks a random enemy eligible for the given wave.
        /// Returns null if no enemies are eligible or the database is empty.
        /// </summary>
        public EnemyData GetRandomEnemy(int wave)
        {
            var eligible = GetEnemiesForWave(wave);
            if (eligible.Count == 0)
            {
                Debug.LogWarning($"[EnemyDatabase] No enemies eligible for wave {wave}.");
                return null;
            }
            return eligible[Random.Range(0, eligible.Count)];
        }

        /// <summary>
        /// Returns all enemies that match the specified difficulty tier.
        /// </summary>
        public List<EnemyData> GetEnemiesByDifficulty(EnemyDifficulty difficulty)
        {
            var filtered = new List<EnemyData>();
            foreach (var enemy in enemies)
            {
                if (enemy != null && enemy.difficulty == difficulty)
                {
                    filtered.Add(enemy);
                }
            }
            return filtered;
        }

        /// <summary>
        /// Picks a random enemy of the specified difficulty.
        /// Returns null if none match.
        /// </summary>
        public EnemyData GetRandomEnemyByDifficulty(EnemyDifficulty difficulty)
        {
            var filtered = GetEnemiesByDifficulty(difficulty);
            if (filtered.Count == 0)
            {
                Debug.LogWarning($"[EnemyDatabase] No enemies with difficulty {difficulty}.");
                return null;
            }
            return filtered[Random.Range(0, filtered.Count)];
        }

        /// <summary>
        /// Returns all enemies eligible for the given wave filtered to a specific difficulty.
        /// Useful for boss-wave logic where you want only Boss-tier enemies from the eligible pool.
        /// </summary>
        public List<EnemyData> GetEnemiesForWave(int wave, EnemyDifficulty difficulty)
        {
            var eligible = new List<EnemyData>();
            foreach (var enemy in enemies)
            {
                if (enemy != null && wave >= enemy.minWave && enemy.difficulty == difficulty)
                {
                    eligible.Add(enemy);
                }
            }
            return eligible;
        }

        /// <summary>
        /// Returns the total number of registered enemies.
        /// </summary>
        public int Count => enemies != null ? enemies.Count : 0;

#if UNITY_EDITOR
        /// <summary>
        /// Editor-only validation to catch misconfigured entries.
        /// </summary>
        private void OnValidate()
        {
            if (enemies == null) return;

            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i] == null)
                {
                    Debug.LogWarning($"[EnemyDatabase] Null entry at index {i} in '{name}'.");
                }
            }
        }
#endif
    }
}
