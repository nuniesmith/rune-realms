import type { SkillName, PlayerSkills } from '../../shared/types';

const MAX_OFFLINE_HOURS = 8;
const OFFLINE_XP_PER_MINUTE = 5;
const MAX_OFFLINE_MS = MAX_OFFLINE_HOURS * 60 * 60 * 1000;

/**
 * XP required to reach a given level.
 * Simple formula: level N requires (N-1) * 100 total XP.
 * Level 1 = 0 XP, Level 2 = 100 XP, Level 10 = 900 XP, etc.
 */
export const xpForLevel = (level: number): number => {
  if (level <= 1) return 0;
  return (level - 1) * 100;
};

/**
 * Calculate level from total XP.
 * Inverse of xpForLevel.
 */
export const levelForXp = (xp: number): number => {
  if (xp < 0) return 1;
  return 1 + Math.floor(xp / 100);
};

/**
 * Calculate offline XP gains.
 * Only the active skill gains XP while offline.
 * Capped at MAX_OFFLINE_HOURS hours.
 * Returns a map of skill name → XP gained.
 */
export const calculateOfflineXp = (
  lastSaveTimestamp: number,
  currentTimestamp: number,
  activeSkill: SkillName | null
): Record<string, number> => {
  const result: Record<string, number> = {};

  if (!activeSkill) return result;

  const elapsedMs = Math.max(0, currentTimestamp - lastSaveTimestamp);
  const cappedMs = Math.min(elapsedMs, MAX_OFFLINE_MS);
  const offlineMinutes = Math.floor(cappedMs / (60 * 1000));
  const xpGained = offlineMinutes * OFFLINE_XP_PER_MINUTE;

  if (xpGained > 0) {
    result[activeSkill] = xpGained;
  }

  return result;
};

/**
 * Apply offline XP gains to a PlayerSkills object.
 * Returns the updated PlayerSkills and a map of XP gained per skill.
 */
export const applyOfflineXp = (
  skills: PlayerSkills,
  currentTimestamp: number
): { updatedSkills: PlayerSkills; xpGained: Record<string, number> } => {
  const xpGained = calculateOfflineXp(
    skills.lastSaveTimestamp,
    currentTimestamp,
    skills.activeSkill
  );

  const updatedSkillList = skills.skills.map((skill) => {
    const gained = xpGained[skill.name] ?? 0;
    if (gained === 0) return skill;
    const newXp = skill.xp + gained;
    return {
      ...skill,
      xp: newXp,
      level: levelForXp(newXp),
    };
  });

  return {
    updatedSkills: {
      ...skills,
      skills: updatedSkillList,
      lastSaveTimestamp: currentTimestamp,
    },
    xpGained,
  };
};

/**
 * Calculate total level from all skills.
 */
export const calculateTotalLevel = (skills: PlayerSkills): number =>
  skills.skills.reduce((sum, s) => sum + s.level, 0);

/**
 * Calculate total XP from all skills.
 */
export const calculateTotalXp = (skills: PlayerSkills): number =>
  skills.skills.reduce((sum, s) => sum + s.xp, 0);
