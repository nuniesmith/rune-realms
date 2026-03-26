using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RuneRealms.Data;
using RuneRealms.Core;

namespace RuneRealms.Skills
{
    /// <summary>
    /// Manages idle skill training. Each skill ticks XP every second when active.
    /// Attach to the Skills tab panel in the scene.
    /// </summary>
    public class IdleManager : MonoBehaviour
    {
        [Header("Skill UI Slots")]
        [SerializeField] private List<SkillSlotUI> skillSlots;

        [Header("Settings")]
        [SerializeField] private float tickInterval = 1f;
        [SerializeField] private int[] xpPerTick = { 10, 12, 8, 9, 11 }; // wc, fish, mine, cook, smith

        private PlayerSkills playerSkills;
        private int activeSkillIndex = -1;
        private Coroutine tickCoroutine;

        [System.Serializable]
        public class SkillSlotUI
        {
            public string skillName;
            public Button button;
            public Slider progressBar;
            public TextMeshProUGUI levelText;
            public TextMeshProUGUI nameText;
            public Image iconImage;
            public GameObject activeIndicator; // Glow/pulse effect
        }

        public void Initialize(PlayerSkills skills)
        {
            playerSkills = skills;
            UpdateAllUI();

            // Hook up button clicks
            for (int i = 0; i < skillSlots.Count; i++)
            {
                int index = i; // Capture for closure
                if (skillSlots[i].button != null)
                {
                    skillSlots[i].button.onClick.RemoveAllListeners();
                    skillSlots[i].button.onClick.AddListener(() => StartSkill(index));
                }
            }

            // Resume active skill if one was saved
            if (!string.IsNullOrEmpty(skills.activeSkill))
            {
                int savedIndex = FindSkillIndex(skills.activeSkill);
                if (savedIndex >= 0)
                {
                    StartSkill(savedIndex);
                }
            }
        }

        public void StartSkill(int index)
        {
            if (index < 0 || index >= skillSlots.Count) return;
            if (playerSkills == null) return;

            // Stop current skill
            if (tickCoroutine != null)
            {
                StopCoroutine(tickCoroutine);
                if (activeSkillIndex >= 0 && activeSkillIndex < skillSlots.Count)
                {
                    SetSlotActive(activeSkillIndex, false);
                }
            }

            // Start new skill
            activeSkillIndex = index;
            playerSkills.activeSkill = playerSkills.skills[index].name;
            SetSlotActive(index, true);
            tickCoroutine = StartCoroutine(SkillTickCoroutine(index));

            // Update GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CurrentSkills = playerSkills;
            }
        }

        public void StopSkill()
        {
            if (tickCoroutine != null)
            {
                StopCoroutine(tickCoroutine);
                tickCoroutine = null;
            }

            if (activeSkillIndex >= 0 && activeSkillIndex < skillSlots.Count)
            {
                SetSlotActive(activeSkillIndex, false);
            }

            activeSkillIndex = -1;
            playerSkills.activeSkill = null;
        }

        private IEnumerator SkillTickCoroutine(int index)
        {
            while (true)
            {
                yield return new WaitForSeconds(tickInterval);

                if (index >= playerSkills.skills.Count) yield break;

                var skill = playerSkills.skills[index];
                int xpGain = (index < xpPerTick.Length) ? xpPerTick[index] : 10;
                int oldLevel = skill.level;

                skill.xp += xpGain;
                skill.level = 1 + skill.xp / 100;

                // Update UI
                UpdateSlotUI(index, skill);

                // Show floating XP text
                ShowXpGain(index, xpGain);

                // Level up!
                if (skill.level > oldLevel)
                {
                    OnLevelUp(index, skill);
                }

                // Update GameManager reference
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.CurrentSkills = playerSkills;
                }
            }
        }

        private void UpdateSlotUI(int index, SkillData skill)
        {
            if (index >= skillSlots.Count) return;
            var slot = skillSlots[index];

            if (slot.levelText != null)
                slot.levelText.text = $"Lv.{skill.level}";

            if (slot.progressBar != null)
                slot.progressBar.value = (skill.xp % 100) / 100f;

            if (slot.nameText != null)
                slot.nameText.text = FormatSkillName(skill.name);
        }

        private void UpdateAllUI()
        {
            if (playerSkills == null) return;

            for (int i = 0; i < playerSkills.skills.Count && i < skillSlots.Count; i++)
            {
                UpdateSlotUI(i, playerSkills.skills[i]);
                SetSlotActive(i, false);
            }
        }

        private void SetSlotActive(int index, bool active)
        {
            if (index >= skillSlots.Count) return;
            var slot = skillSlots[index];

            if (slot.activeIndicator != null)
                slot.activeIndicator.SetActive(active);
        }

        private void ShowXpGain(int index, int amount)
        {
            if (index >= skillSlots.Count) return;
            var slot = skillSlots[index];

            // Spawn floating text at the skill slot position
            if (slot.button != null)
            {
                UI.FloatingText.Spawn($"+{amount} XP", slot.button.transform.position, Color.cyan);
            }
        }

        private void OnLevelUp(int index, SkillData skill)
        {
            Debug.Log($"[IdleManager] {skill.name} leveled up to {skill.level}!");

            // Spawn level-up floating text
            if (index < skillSlots.Count && skillSlots[index].button != null)
            {
                UI.FloatingText.Spawn($"LEVEL {skill.level}!", skillSlots[index].button.transform.position, Color.yellow);
            }

            // Auto-save on level up
            GameManager.Instance?.SaveAll();
        }

        private int FindSkillIndex(string skillName)
        {
            if (playerSkills == null) return -1;
            for (int i = 0; i < playerSkills.skills.Count; i++)
            {
                if (playerSkills.skills[i].name == skillName) return i;
            }
            return -1;
        }

        private string FormatSkillName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "";
            return char.ToUpper(name[0]) + name.Substring(1);
        }
    }
}
