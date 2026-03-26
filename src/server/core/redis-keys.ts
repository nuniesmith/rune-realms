export const skillsKey = (postId: string, username: string): string =>
  `rune-realms:${postId}:${username}:skills`;

export const inventoryKey = (postId: string, username: string): string =>
  `rune-realms:${postId}:${username}:inventory`;

export const profileKey = (postId: string, username: string): string =>
  `rune-realms:${postId}:${username}:profile`;

export const leaderboardKey = (postId: string, type: string): string =>
  `rune-realms:${postId}:leaderboard:${type}`;

export const feedKey = (postId: string): string => `rune-realms:${postId}:feed`;

export const tutorialKey = (postId: string, username: string): string =>
  `rune-realms:${postId}:${username}:tutorial`;
