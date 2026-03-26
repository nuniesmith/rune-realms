import type {
  PlayerSkills,
  PlayerInventory,
  PlayerProfile,
  ArenaResult,
  InventoryItem,
  LeaderboardEntry,
  TutorialProgress,
} from './types';

// --- Init ---
export type InitResponse = {
  type: 'init';
  postId: string;
  username: string;
  snoovatarUrl: string;
  isNewPlayer: boolean;
  skills: PlayerSkills;
  inventory: PlayerInventory;
  profile: PlayerProfile;
  offlineXpGained: Record<string, number>;
  tutorial: TutorialProgress;
};

// --- Skills ---
export type SaveSkillsRequest = {
  type: 'save-skills';
  skills: PlayerSkills;
};

export type SaveSkillsResponse = {
  type: 'save-skills';
  success: boolean;
  message?: string;
};

// --- Arena ---
export type SaveArenaResultRequest = {
  type: 'save-arena-result';
  result: ArenaResult;
};

export type SaveArenaResultResponse = {
  type: 'save-arena-result';
  success: boolean;
  newItems: InventoryItem[];
  totalKills: number;
};

// --- Inventory ---
export type GetInventoryResponse = {
  type: 'get-inventory';
  inventory: PlayerInventory;
};

export type UseItemRequest = {
  type: 'use-item';
  itemId: string;
};

export type UseItemResponse = {
  type: 'use-item';
  success: boolean;
  inventory: PlayerInventory;
  message?: string;
};

// --- Leaderboard ---
export type GetLeaderboardResponse = {
  type: 'get-leaderboard';
  entries: LeaderboardEntry[];
  playerRank: number | null;
};

// --- Tutorial ---
export type SaveTutorialRequest = {
  type: 'save-tutorial';
  tutorial: TutorialProgress;
};

export type SaveTutorialResponse = {
  type: 'save-tutorial';
  success: boolean;
};

// --- Shared Error ---
export type ErrorResponse = {
  status: 'error';
  message: string;
};
