import { describe, it, expect } from 'vitest';
import {
  createDefaultSkills,
  createDefaultInventory,
  createDefaultProfile,
  createDefaultTutorial,
} from './defaults';

describe('createDefaultSkills', () => {
  it('returns exactly 8 skills', () => {
    const result = createDefaultSkills();
    expect(result.skills).toHaveLength(8);
  });

  it('all skills start at level 1 with 0 XP', () => {
    const result = createDefaultSkills();
    for (const skill of result.skills) {
      expect(skill.level).toBe(1);
      expect(skill.xp).toBe(0);
    }
  });

  it('contains all expected skill names', () => {
    const result = createDefaultSkills();
    const skillNames = result.skills.map((s) => s.name);
    expect(skillNames).toEqual([
      'woodcutting',
      'fishing',
      'mining',
      'cooking',
      'smithing',
      'attack',
      'strength',
      'defence',
    ]);
  });

  it('activeSkill is null', () => {
    const result = createDefaultSkills();
    expect(result.activeSkill).toBeNull();
  });

  it('lastSaveTimestamp is recent (within 1 second of Date.now())', () => {
    const before = Date.now();
    const result = createDefaultSkills();
    const after = Date.now();

    expect(result.lastSaveTimestamp).toBeGreaterThanOrEqual(before);
    expect(result.lastSaveTimestamp).toBeLessThanOrEqual(after);
    expect(after - result.lastSaveTimestamp).toBeLessThanOrEqual(1000);
  });
});

describe('createDefaultInventory', () => {
  it('returns empty items array', () => {
    const result = createDefaultInventory();
    expect(result.items).toEqual([]);
  });

  it('has maxSlots of 20', () => {
    const result = createDefaultInventory();
    expect(result.maxSlots).toBe(20);
  });
});

describe('createDefaultProfile', () => {
  it('uses the provided username', () => {
    const result = createDefaultProfile('testPlayer');
    expect(result.username).toBe('testPlayer');
  });

  it('has totalLevel 8 (8 skills × level 1)', () => {
    const result = createDefaultProfile('someone');
    expect(result.totalLevel).toBe(8);
  });

  it('has 0 totalXp and 0 arenaKills', () => {
    const result = createDefaultProfile('someone');
    expect(result.totalXp).toBe(0);
    expect(result.arenaKills).toBe(0);
  });

  it('joinedTimestamp is recent (within 1 second of Date.now())', () => {
    const before = Date.now();
    const result = createDefaultProfile('someone');
    const after = Date.now();

    expect(result.joinedTimestamp).toBeGreaterThanOrEqual(before);
    expect(result.joinedTimestamp).toBeLessThanOrEqual(after);
    expect(after - result.joinedTimestamp).toBeLessThanOrEqual(1000);
  });
});

describe('createDefaultTutorial', () => {
  it('completed is false', () => {
    const result = createDefaultTutorial();
    expect(result.completed).toBe(false);
  });

  it('currentStep is 0', () => {
    const result = createDefaultTutorial();
    expect(result.currentStep).toBe(0);
  });

  it('completedAt is null', () => {
    const result = createDefaultTutorial();
    expect(result.completedAt).toBeNull();
  });
});
