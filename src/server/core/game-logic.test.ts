import { describe, it, expect } from 'vitest';
import {
  xpForLevel,
  levelForXp,
  calculateOfflineXp,
  applyOfflineXp,
  calculateTotalLevel,
  calculateTotalXp,
} from './game-logic';
import type { PlayerSkills } from '../../shared/types';

describe('xpForLevel', () => {
  it('returns 0 for level 1', () => {
    expect(xpForLevel(1)).toBe(0);
  });

  it('returns 0 for level 0 or negative', () => {
    expect(xpForLevel(0)).toBe(0);
    expect(xpForLevel(-5)).toBe(0);
  });

  it('returns 100 for level 2', () => {
    expect(xpForLevel(2)).toBe(100);
  });

  it('returns 900 for level 10', () => {
    expect(xpForLevel(10)).toBe(900);
  });

  it('scales linearly: level N requires (N-1)*100', () => {
    for (let level = 1; level <= 50; level++) {
      expect(xpForLevel(level)).toBe((level - 1) * 100);
    }
  });
});

describe('levelForXp', () => {
  it('returns 1 for 0 XP', () => {
    expect(levelForXp(0)).toBe(1);
  });

  it('returns 1 for negative XP', () => {
    expect(levelForXp(-100)).toBe(1);
  });

  it('returns 1 for 99 XP (not yet level 2)', () => {
    expect(levelForXp(99)).toBe(1);
  });

  it('returns 2 for exactly 100 XP', () => {
    expect(levelForXp(100)).toBe(2);
  });

  it('returns 2 for 199 XP', () => {
    expect(levelForXp(199)).toBe(2);
  });

  it('returns 10 for 900 XP', () => {
    expect(levelForXp(900)).toBe(10);
  });

  it('returns 11 for 1000 XP', () => {
    expect(levelForXp(1000)).toBe(11);
  });

  it('is the inverse of xpForLevel', () => {
    for (let level = 1; level <= 100; level++) {
      const xp = xpForLevel(level);
      expect(levelForXp(xp)).toBe(level);
    }
  });

  it('levels up at the exact XP threshold', () => {
    for (let level = 1; level <= 50; level++) {
      const threshold = xpForLevel(level);
      if (level > 1) {
        expect(levelForXp(threshold - 1)).toBe(level - 1);
      }
      expect(levelForXp(threshold)).toBe(level);
    }
  });
});

describe('calculateOfflineXp', () => {
  it('returns empty object when no active skill', () => {
    const result = calculateOfflineXp(1000, 2000, null);
    expect(result).toEqual({});
  });

  it('returns 0 XP for less than 1 minute offline', () => {
    const now = Date.now();
    const thirtySecondsAgo = now - 30 * 1000;
    const result = calculateOfflineXp(thirtySecondsAgo, now, 'woodcutting');
    expect(result).toEqual({});
  });

  it('returns 5 XP for 1 minute offline', () => {
    const now = Date.now();
    const oneMinuteAgo = now - 60 * 1000;
    const result = calculateOfflineXp(oneMinuteAgo, now, 'woodcutting');
    expect(result).toEqual({ woodcutting: 5 });
  });

  it('returns 300 XP for 1 hour offline', () => {
    const now = Date.now();
    const oneHourAgo = now - 60 * 60 * 1000;
    const result = calculateOfflineXp(oneHourAgo, now, 'mining');
    expect(result).toEqual({ mining: 300 });
  });

  it('caps at 8 hours of offline progress (2400 XP)', () => {
    const now = Date.now();
    const twentyFourHoursAgo = now - 24 * 60 * 60 * 1000;
    const result = calculateOfflineXp(twentyFourHoursAgo, now, 'fishing');
    // 8 hours * 60 minutes * 5 XP = 2400
    expect(result).toEqual({ fishing: 2400 });
  });

  it('caps at exactly 8 hours', () => {
    const now = Date.now();
    const eightHoursAgo = now - 8 * 60 * 60 * 1000;
    const nineHoursAgo = now - 9 * 60 * 60 * 1000;

    const resultEight = calculateOfflineXp(eightHoursAgo, now, 'cooking');
    const resultNine = calculateOfflineXp(nineHoursAgo, now, 'cooking');

    expect(resultEight).toEqual({ cooking: 2400 });
    expect(resultNine).toEqual({ cooking: 2400 });
  });

  it('returns empty object if lastSave is in the future', () => {
    const now = Date.now();
    const futureTime = now + 60 * 1000;
    const result = calculateOfflineXp(futureTime, now, 'smithing');
    expect(result).toEqual({});
  });

  it('only grants XP to the active skill', () => {
    const now = Date.now();
    const tenMinutesAgo = now - 10 * 60 * 1000;
    const result = calculateOfflineXp(tenMinutesAgo, now, 'cooking');
    expect(Object.keys(result)).toEqual(['cooking']);
    expect(result['cooking']).toBe(50);
  });
});

describe('applyOfflineXp', () => {
  const makeSkills = (overrides?: Partial<PlayerSkills>): PlayerSkills => ({
    skills: [
      { name: 'woodcutting', xp: 0, level: 1 },
      { name: 'fishing', xp: 0, level: 1 },
      { name: 'mining', xp: 0, level: 1 },
      { name: 'cooking', xp: 0, level: 1 },
      { name: 'smithing', xp: 0, level: 1 },
    ],
    lastSaveTimestamp: Date.now() - 10 * 60 * 1000, // 10 minutes ago
    activeSkill: 'woodcutting',
    ...overrides,
  });

  it('applies offline XP to the active skill', () => {
    const now = Date.now();
    const skills = makeSkills({
      lastSaveTimestamp: now - 10 * 60 * 1000,
      activeSkill: 'woodcutting',
    });

    const { updatedSkills, xpGained } = applyOfflineXp(skills, now);

    expect(xpGained).toEqual({ woodcutting: 50 });
    const wc = updatedSkills.skills.find((s) => s.name === 'woodcutting');
    expect(wc?.xp).toBe(50);
    expect(wc?.level).toBe(1); // 50 XP is still level 1
  });

  it('does not modify other skills', () => {
    const now = Date.now();
    const skills = makeSkills({
      lastSaveTimestamp: now - 10 * 60 * 1000,
      activeSkill: 'mining',
    });

    const { updatedSkills } = applyOfflineXp(skills, now);

    const fishing = updatedSkills.skills.find((s) => s.name === 'fishing');
    const woodcutting = updatedSkills.skills.find((s) => s.name === 'woodcutting');
    expect(fishing?.xp).toBe(0);
    expect(woodcutting?.xp).toBe(0);
  });

  it('levels up correctly with enough offline time', () => {
    const now = Date.now();
    const skills = makeSkills({
      lastSaveTimestamp: now - 60 * 60 * 1000, // 1 hour ago
      activeSkill: 'fishing',
    });

    const { updatedSkills, xpGained } = applyOfflineXp(skills, now);

    expect(xpGained).toEqual({ fishing: 300 });
    const fishing = updatedSkills.skills.find((s) => s.name === 'fishing');
    expect(fishing?.xp).toBe(300);
    expect(fishing?.level).toBe(4); // 300 XP = level 4
  });

  it('adds to existing XP', () => {
    const now = Date.now();
    const skills = makeSkills({
      skills: [
        { name: 'woodcutting', xp: 80, level: 1 },
        { name: 'fishing', xp: 0, level: 1 },
        { name: 'mining', xp: 0, level: 1 },
        { name: 'cooking', xp: 0, level: 1 },
        { name: 'smithing', xp: 0, level: 1 },
      ],
      lastSaveTimestamp: now - 10 * 60 * 1000,
      activeSkill: 'woodcutting',
    });

    const { updatedSkills } = applyOfflineXp(skills, now);

    const wc = updatedSkills.skills.find((s) => s.name === 'woodcutting');
    expect(wc?.xp).toBe(130); // 80 + 50
    expect(wc?.level).toBe(2); // crossed the 100 XP threshold
  });

  it('updates lastSaveTimestamp to current time', () => {
    const now = Date.now();
    const skills = makeSkills({
      lastSaveTimestamp: now - 10 * 60 * 1000,
    });

    const { updatedSkills } = applyOfflineXp(skills, now);

    expect(updatedSkills.lastSaveTimestamp).toBe(now);
  });

  it('returns empty xpGained when no active skill', () => {
    const now = Date.now();
    const skills = makeSkills({
      lastSaveTimestamp: now - 10 * 60 * 1000,
      activeSkill: null,
    });

    const { updatedSkills, xpGained } = applyOfflineXp(skills, now);

    expect(xpGained).toEqual({});
    // All skills should be unchanged
    for (const skill of updatedSkills.skills) {
      expect(skill.xp).toBe(0);
      expect(skill.level).toBe(1);
    }
  });
});

describe('calculateTotalLevel', () => {
  it('returns sum of all skill levels', () => {
    const skills: PlayerSkills = {
      skills: [
        { name: 'woodcutting', xp: 200, level: 3 },
        { name: 'fishing', xp: 500, level: 6 },
        { name: 'mining', xp: 0, level: 1 },
        { name: 'cooking', xp: 100, level: 2 },
        { name: 'smithing', xp: 0, level: 1 },
      ],
      lastSaveTimestamp: Date.now(),
      activeSkill: null,
    };

    expect(calculateTotalLevel(skills)).toBe(13); // 3 + 6 + 1 + 2 + 1
  });

  it('returns 5 for all level 1 skills', () => {
    const skills: PlayerSkills = {
      skills: [
        { name: 'woodcutting', xp: 0, level: 1 },
        { name: 'fishing', xp: 0, level: 1 },
        { name: 'mining', xp: 0, level: 1 },
        { name: 'cooking', xp: 0, level: 1 },
        { name: 'smithing', xp: 0, level: 1 },
      ],
      lastSaveTimestamp: Date.now(),
      activeSkill: null,
    };

    expect(calculateTotalLevel(skills)).toBe(5);
  });
});

describe('calculateTotalXp', () => {
  it('returns sum of all skill XP', () => {
    const skills: PlayerSkills = {
      skills: [
        { name: 'woodcutting', xp: 200, level: 3 },
        { name: 'fishing', xp: 500, level: 6 },
        { name: 'mining', xp: 0, level: 1 },
        { name: 'cooking', xp: 100, level: 2 },
        { name: 'smithing', xp: 50, level: 1 },
      ],
      lastSaveTimestamp: Date.now(),
      activeSkill: null,
    };

    expect(calculateTotalXp(skills)).toBe(850); // 200 + 500 + 0 + 100 + 50
  });

  it('returns 0 for all fresh skills', () => {
    const skills: PlayerSkills = {
      skills: [
        { name: 'woodcutting', xp: 0, level: 1 },
        { name: 'fishing', xp: 0, level: 1 },
        { name: 'mining', xp: 0, level: 1 },
        { name: 'cooking', xp: 0, level: 1 },
        { name: 'smithing', xp: 0, level: 1 },
      ],
      lastSaveTimestamp: Date.now(),
      activeSkill: null,
    };

    expect(calculateTotalXp(skills)).toBe(0);
  });
});
