import type {
  PlayerSkills,
  PlayerInventory,
  PlayerProfile,
  SkillData,
  TutorialProgress,
} from '../../shared/types';
import { ALL_SKILLS } from '../../shared/types';

export const createDefaultSkills = (): PlayerSkills => ({
  skills: ALL_SKILLS.map(
    (name): SkillData => ({
      name,
      xp: 0,
      level: 1,
    })
  ),
  lastSaveTimestamp: Date.now(),
  activeSkill: null,
});

export const createDefaultInventory = (): PlayerInventory => ({
  items: [],
  maxSlots: 20,
});

export const createDefaultProfile = (username: string): PlayerProfile => ({
  username,
  totalLevel: 5, // 5 skills × level 1
  totalXp: 0,
  arenaKills: 0,
  joinedTimestamp: Date.now(),
});

export const createDefaultTutorial = (): TutorialProgress => ({
  completed: false,
  currentStep: 0,
  completedAt: null,
});
