// === Skills ===
export type SkillName =
  | 'woodcutting'
  | 'fishing'
  | 'mining'
  | 'cooking'
  | 'smithing'
  | 'attack'
  | 'strength'
  | 'defence';

export const ALL_SKILLS: SkillName[] = [
  'woodcutting',
  'fishing',
  'mining',
  'cooking',
  'smithing',
  'attack',
  'strength',
  'defence',
];

export type SkillData = {
  name: SkillName;
  xp: number;
  level: number;
};

export type PlayerSkills = {
  skills: SkillData[];
  lastSaveTimestamp: number;
  activeSkill: SkillName | null;
};

// === Combat ===
export type LootItem = {
  itemId: string;
  name: string;
  quantity: number;
  rarity: 'common' | 'uncommon' | 'rare' | 'legendary';
};

export type ArenaResult = {
  kills: number;
  loot: LootItem[];
  wavesCleared: number;
  damageDealt: number;
};

// === Inventory ===
export type ItemCategory =
  | 'weapon'
  | 'armor'
  | 'tool'
  | 'material'
  | 'consumable';
export type ItemRarity = 'common' | 'uncommon' | 'rare' | 'legendary';

export type InventoryItem = {
  itemId: string;
  name: string;
  quantity: number;
  category: ItemCategory;
  rarity: ItemRarity;
};

export type PlayerInventory = {
  items: InventoryItem[];
  maxSlots: number;
};

// === Player Profile ===
export type PlayerProfile = {
  username: string;
  totalLevel: number;
  totalXp: number;
  arenaKills: number;
  joinedTimestamp: number;
};

// === Leaderboard ===
export type LeaderboardEntry = {
  username: string;
  score: number;
  rank: number;
};

export type LeaderboardType = 'total-level' | 'total-xp' | 'arena-kills';

// === Tutorial ===
export type TutorialProgress = {
  completed: boolean;
  currentStep: number;
  completedAt: number | null; // timestamp
};
