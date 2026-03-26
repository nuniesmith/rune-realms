import { Hono } from 'hono';
import { context, redis, reddit } from '@devvit/web/server';
import type {
  InitResponse,
  SaveSkillsRequest,
  SaveSkillsResponse,
  SaveArenaResultRequest,
  SaveArenaResultResponse,
  GetInventoryResponse,
  UseItemRequest,
  UseItemResponse,
  GetLeaderboardResponse,
  SaveTutorialRequest,
  SaveTutorialResponse,
  ErrorResponse,
} from '../../shared/api';
import type {
  PlayerSkills,
  PlayerInventory,
  PlayerProfile,
  LeaderboardType,
  LeaderboardEntry,
  InventoryItem,
  TutorialProgress,
} from '../../shared/types';
import {
  skillsKey,
  inventoryKey,
  profileKey,
  leaderboardKey,
  tutorialKey,
} from '../core/redis-keys';
import {
  createDefaultSkills,
  createDefaultInventory,
  createDefaultProfile,
  createDefaultTutorial,
} from '../core/defaults';
import {
  applyOfflineXp,
  calculateTotalLevel,
  calculateTotalXp,
  levelForXp,
} from '../core/game-logic';

/*
 * Redis Key Schema:
 *   rune-realms:{postId}:{username}:skills    → JSON string of PlayerSkills
 *   rune-realms:{postId}:{username}:inventory → JSON string of PlayerInventory
 *   rune-realms:{postId}:{username}:profile   → JSON string of PlayerProfile
 *   rune-realms:{postId}:leaderboard:total-level → Sorted set (username → score)
 *   rune-realms:{postId}:leaderboard:total-xp    → Sorted set (username → score)
 *   rune-realms:{postId}:leaderboard:arena-kills → Sorted set (username → score)
 *   rune-realms:{postId}:{username}:tutorial      → JSON string of TutorialProgress
 */

export const api = new Hono();

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

const getUsername = async (): Promise<string> => {
  const username = await reddit.getCurrentUsername();
  return username ?? 'anonymous';
};

const loadJson = async <T>(key: string): Promise<T | null> => {
  const raw = await redis.get(key);
  if (!raw) return null;
  return JSON.parse(raw) as T;
};

const saveJson = async <T>(key: string, data: T): Promise<void> => {
  await redis.set(key, JSON.stringify(data));
};

const updateLeaderboards = async (
  postId: string,
  username: string,
  skills: PlayerSkills,
  profile: PlayerProfile
): Promise<void> => {
  const totalLevel = calculateTotalLevel(skills);
  const totalXp = calculateTotalXp(skills);

  await Promise.all([
    redis.zAdd(leaderboardKey(postId, 'total-level'), {
      member: username,
      score: totalLevel,
    }),
    redis.zAdd(leaderboardKey(postId, 'total-xp'), {
      member: username,
      score: totalXp,
    }),
    redis.zAdd(leaderboardKey(postId, 'arena-kills'), {
      member: username,
      score: profile.arenaKills,
    }),
  ]);
};

// ---------------------------------------------------------------------------
// GET /api/init
// ---------------------------------------------------------------------------

api.get('/init', async (c) => {
  const { postId } = context;

  if (!postId) {
    console.error('API Init Error: postId not found in devvit context');
    return c.json<ErrorResponse>(
      {
        status: 'error',
        message: 'postId is required but missing from context',
      },
      400
    );
  }

  try {
    const username = await getUsername();

    // Fetch snoovatar
    let snoovatarUrl = '';
    if (username !== 'anonymous' && context.userId) {
      try {
        const user = await reddit.getUserById(context.userId);
        if (user) {
          snoovatarUrl = (await user.getSnoovatarUrl()) ?? '';
        }
      } catch {
        // Snoovatar is optional — don't fail init over it
      }
    }

    // Load existing data from Redis
    const [
      existingSkills,
      existingInventory,
      existingProfile,
      existingTutorial,
    ] = await Promise.all([
      loadJson<PlayerSkills>(skillsKey(postId, username)),
      loadJson<PlayerInventory>(inventoryKey(postId, username)),
      loadJson<PlayerProfile>(profileKey(postId, username)),
      loadJson<TutorialProgress>(tutorialKey(postId, username)),
    ]);

    const isNewPlayer = !existingSkills;

    // Use existing data or create defaults
    let skills = existingSkills ?? createDefaultSkills();
    const inventory = existingInventory ?? createDefaultInventory();
    let profile = existingProfile ?? createDefaultProfile(username);
    const tutorial = existingTutorial ?? createDefaultTutorial();

    // Calculate offline XP gains for returning players
    let offlineXpGained: Record<string, number> = {};
    if (!isNewPlayer) {
      const result = applyOfflineXp(skills, Date.now());
      skills = result.updatedSkills;
      offlineXpGained = result.xpGained;

      // Update profile totals
      profile = {
        ...profile,
        totalLevel: calculateTotalLevel(skills),
        totalXp: calculateTotalXp(skills),
      };
    }

    // Save (possibly updated) data back to Redis
    await Promise.all([
      saveJson(skillsKey(postId, username), skills),
      saveJson(inventoryKey(postId, username), inventory),
      saveJson(profileKey(postId, username), profile),
      saveJson(tutorialKey(postId, username), tutorial),
    ]);

    // Update leaderboards
    await updateLeaderboards(postId, username, skills, profile);

    return c.json<InitResponse>({
      type: 'init',
      postId,
      username,
      snoovatarUrl,
      isNewPlayer,
      skills,
      inventory,
      profile,
      offlineXpGained,
      tutorial,
    });
  } catch (error) {
    console.error(`API Init Error for post ${postId}:`, error);
    const message =
      error instanceof Error
        ? `Initialization failed: ${error.message}`
        : 'Unknown error during initialization';
    return c.json<ErrorResponse>({ status: 'error', message }, 500);
  }
});

// ---------------------------------------------------------------------------
// POST /api/save-skills
// ---------------------------------------------------------------------------

api.post('/save-skills', async (c) => {
  const { postId } = context;

  if (!postId) {
    return c.json<ErrorResponse>(
      { status: 'error', message: 'postId is required' },
      400
    );
  }

  let body: SaveSkillsRequest;
  try {
    body = await c.req.json<SaveSkillsRequest>();
  } catch {
    return c.json<ErrorResponse>(
      { status: 'error', message: 'Invalid JSON body' },
      400
    );
  }

  try {
    const username = await getUsername();
    const { skills } = body;

    if (!skills || !skills.skills || !Array.isArray(skills.skills)) {
      return c.json<ErrorResponse>(
        { status: 'error', message: 'Invalid skills data' },
        400
      );
    }

    // Validate and recalculate levels from XP to prevent cheating
    const validatedSkills: PlayerSkills = {
      ...skills,
      skills: skills.skills.map((s) => ({
        ...s,
        xp: Math.max(0, Math.floor(s.xp)),
        level: levelForXp(Math.max(0, Math.floor(s.xp))),
      })),
      lastSaveTimestamp: Date.now(),
    };

    // Save skills
    await saveJson(skillsKey(postId, username), validatedSkills);

    // Update profile totals
    const existingProfile = await loadJson<PlayerProfile>(
      profileKey(postId, username)
    );
    const profile = existingProfile ?? createDefaultProfile(username);
    const updatedProfile: PlayerProfile = {
      ...profile,
      totalLevel: calculateTotalLevel(validatedSkills),
      totalXp: calculateTotalXp(validatedSkills),
    };
    await saveJson(profileKey(postId, username), updatedProfile);

    // Update leaderboards
    await updateLeaderboards(postId, username, validatedSkills, updatedProfile);

    return c.json<SaveSkillsResponse>({
      type: 'save-skills',
      success: true,
      message: 'Skills saved successfully',
    });
  } catch (error) {
    console.error(`API Save Skills Error for post ${postId}:`, error);
    const message =
      error instanceof Error
        ? `Failed to save skills: ${error.message}`
        : 'Unknown error saving skills';
    return c.json<SaveSkillsResponse>(
      { type: 'save-skills', success: false, message },
      500
    );
  }
});

// ---------------------------------------------------------------------------
// POST /api/save-arena-result
// ---------------------------------------------------------------------------

api.post('/save-arena-result', async (c) => {
  const { postId } = context;

  if (!postId) {
    return c.json<ErrorResponse>(
      { status: 'error', message: 'postId is required' },
      400
    );
  }

  let body: SaveArenaResultRequest;
  try {
    body = await c.req.json<SaveArenaResultRequest>();
  } catch {
    return c.json<ErrorResponse>(
      { status: 'error', message: 'Invalid JSON body' },
      400
    );
  }

  try {
    const username = await getUsername();
    const { result } = body;

    if (!result || typeof result.kills !== 'number') {
      return c.json<ErrorResponse>(
        { status: 'error', message: 'Invalid arena result data' },
        400
      );
    }

    // Load current inventory and profile
    const [existingInventory, existingProfile] = await Promise.all([
      loadJson<PlayerInventory>(inventoryKey(postId, username)),
      loadJson<PlayerProfile>(profileKey(postId, username)),
    ]);

    const inventory = existingInventory ?? createDefaultInventory();
    const profile = existingProfile ?? createDefaultProfile(username);

    // Add loot items to inventory (respect max slots)
    const newItems: InventoryItem[] = [];
    for (const lootItem of result.loot) {
      // Check if item already exists in inventory (stack it)
      const existingItem = inventory.items.find(
        (i) => i.itemId === lootItem.itemId
      );
      if (existingItem) {
        existingItem.quantity += lootItem.quantity;
        newItems.push({ ...existingItem });
      } else if (inventory.items.length < inventory.maxSlots) {
        // Add new item if there's room
        const invItem: InventoryItem = {
          itemId: lootItem.itemId,
          name: lootItem.name,
          quantity: lootItem.quantity,
          category: 'material', // Default category for loot — Unity can specify
          rarity: lootItem.rarity,
        };
        inventory.items.push(invItem);
        newItems.push(invItem);
      }
      // If inventory is full and item doesn't stack, it's silently dropped
    }

    // Update profile
    const updatedProfile: PlayerProfile = {
      ...profile,
      arenaKills: profile.arenaKills + Math.max(0, Math.floor(result.kills)),
    };

    // Save everything
    await Promise.all([
      saveJson(inventoryKey(postId, username), inventory),
      saveJson(profileKey(postId, username), updatedProfile),
    ]);

    // Update arena kills leaderboard
    await redis.zAdd(leaderboardKey(postId, 'arena-kills'), {
      member: username,
      score: updatedProfile.arenaKills,
    });

    return c.json<SaveArenaResultResponse>({
      type: 'save-arena-result',
      success: true,
      newItems,
      totalKills: updatedProfile.arenaKills,
    });
  } catch (error) {
    console.error(`API Arena Result Error for post ${postId}:`, error);
    const message =
      error instanceof Error
        ? `Failed to save arena result: ${error.message}`
        : 'Unknown error saving arena result';
    return c.json<ErrorResponse>({ status: 'error', message }, 500);
  }
});

// ---------------------------------------------------------------------------
// GET /api/inventory
// ---------------------------------------------------------------------------

api.get('/inventory', async (c) => {
  const { postId } = context;

  if (!postId) {
    return c.json<ErrorResponse>(
      { status: 'error', message: 'postId is required' },
      400
    );
  }

  try {
    const username = await getUsername();
    const inventory =
      (await loadJson<PlayerInventory>(inventoryKey(postId, username))) ??
      createDefaultInventory();

    return c.json<GetInventoryResponse>({
      type: 'get-inventory',
      inventory,
    });
  } catch (error) {
    console.error(`API Get Inventory Error for post ${postId}:`, error);
    const message =
      error instanceof Error
        ? `Failed to get inventory: ${error.message}`
        : 'Unknown error getting inventory';
    return c.json<ErrorResponse>({ status: 'error', message }, 500);
  }
});

// ---------------------------------------------------------------------------
// POST /api/inventory/use
// ---------------------------------------------------------------------------

api.post('/inventory/use', async (c) => {
  const { postId } = context;

  if (!postId) {
    return c.json<ErrorResponse>(
      { status: 'error', message: 'postId is required' },
      400
    );
  }

  let body: UseItemRequest;
  try {
    body = await c.req.json<UseItemRequest>();
  } catch {
    return c.json<ErrorResponse>(
      { status: 'error', message: 'Invalid JSON body' },
      400
    );
  }

  try {
    const username = await getUsername();
    const { itemId } = body;

    if (!itemId) {
      return c.json<ErrorResponse>(
        { status: 'error', message: 'itemId is required' },
        400
      );
    }

    const inventory =
      (await loadJson<PlayerInventory>(inventoryKey(postId, username))) ??
      createDefaultInventory();

    const itemIndex = inventory.items.findIndex((i) => i.itemId === itemId);
    if (itemIndex === -1) {
      return c.json<UseItemResponse>({
        type: 'use-item',
        success: false,
        inventory,
        message: 'Item not found in inventory',
      });
    }

    const item = inventory.items[itemIndex];
    if (!item) {
      return c.json<UseItemResponse>({
        type: 'use-item',
        success: false,
        inventory,
        message: 'Item not found in inventory',
      });
    }

    // Only consumables can be used
    if (item.category !== 'consumable') {
      return c.json<UseItemResponse>({
        type: 'use-item',
        success: false,
        inventory,
        message: 'This item cannot be used',
      });
    }

    // Decrement quantity, remove if zero
    item.quantity -= 1;
    if (item.quantity <= 0) {
      inventory.items.splice(itemIndex, 1);
    }

    await saveJson(inventoryKey(postId, username), inventory);

    return c.json<UseItemResponse>({
      type: 'use-item',
      success: true,
      inventory,
      message: `Used ${item.name}`,
    });
  } catch (error) {
    console.error(`API Use Item Error for post ${postId}:`, error);
    const message =
      error instanceof Error
        ? `Failed to use item: ${error.message}`
        : 'Unknown error using item';
    return c.json<ErrorResponse>({ status: 'error', message }, 500);
  }
});

// ---------------------------------------------------------------------------
// GET /api/leaderboard
// ---------------------------------------------------------------------------

const VALID_LEADERBOARD_TYPES: LeaderboardType[] = [
  'total-level',
  'total-xp',
  'arena-kills',
];

api.get('/leaderboard', async (c) => {
  const { postId } = context;

  if (!postId) {
    return c.json<ErrorResponse>(
      { status: 'error', message: 'postId is required' },
      400
    );
  }

  try {
    const typeParam = c.req.query('type') ?? 'total-level';
    const limitParam = c.req.query('limit') ?? '10';

    if (!VALID_LEADERBOARD_TYPES.includes(typeParam as LeaderboardType)) {
      return c.json<ErrorResponse>(
        {
          status: 'error',
          message: `Invalid leaderboard type. Must be one of: ${VALID_LEADERBOARD_TYPES.join(', ')}`,
        },
        400
      );
    }

    const boardType = typeParam as LeaderboardType;
    const limit = Math.min(Math.max(1, parseInt(limitParam, 10) || 10), 50);
    const username = await getUsername();
    const key = leaderboardKey(postId, boardType);

    // Get top entries by rank (highest scores = highest ranks)
    // zRange with negative indices gets entries from the end (highest scores)
    const topEntries = await redis.zRange(key, -limit, -1, { by: 'rank' });

    // topEntries are in ascending order, reverse for descending (highest first)
    topEntries.reverse();

    const entries: LeaderboardEntry[] = topEntries.map((entry, index) => ({
      username: entry.member,
      score: entry.score,
      rank: index + 1,
    }));

    // Get the requesting player's rank
    let playerRank: number | null = null;
    try {
      const rank = await redis.zRank(key, username);
      if (rank !== undefined && rank !== null) {
        // zRank returns 0-based ascending rank; convert to 1-based descending
        const totalMembers = await redis.zCard(key);
        playerRank = totalMembers - rank;
      }
    } catch {
      // Player may not be in the leaderboard yet
    }

    return c.json<GetLeaderboardResponse>({
      type: 'get-leaderboard',
      entries,
      playerRank,
    });
  } catch (error) {
    console.error(`API Leaderboard Error for post ${postId}:`, error);
    const message =
      error instanceof Error
        ? `Failed to get leaderboard: ${error.message}`
        : 'Unknown error getting leaderboard';
    return c.json<ErrorResponse>({ status: 'error', message }, 500);
  }
});

// ---------------------------------------------------------------------------
// POST /api/save-tutorial
// ---------------------------------------------------------------------------

api.post('/save-tutorial', async (c) => {
  const { postId } = context;

  if (!postId) {
    return c.json<ErrorResponse>(
      { status: 'error', message: 'postId is required' },
      400
    );
  }

  let body: SaveTutorialRequest;
  try {
    body = await c.req.json<SaveTutorialRequest>();
  } catch {
    return c.json<ErrorResponse>(
      { status: 'error', message: 'Invalid JSON body' },
      400
    );
  }

  try {
    const username = await getUsername();
    const { tutorial } = body;

    if (
      !tutorial ||
      typeof tutorial.completed !== 'boolean' ||
      typeof tutorial.currentStep !== 'number'
    ) {
      return c.json<ErrorResponse>(
        { status: 'error', message: 'Invalid tutorial data' },
        400
      );
    }

    await saveJson(tutorialKey(postId, username), tutorial);

    return c.json<SaveTutorialResponse>({
      type: 'save-tutorial',
      success: true,
    });
  } catch (error) {
    const message =
      error instanceof Error
        ? `Failed to save tutorial: ${error.message}`
        : 'Unknown error saving tutorial';
    console.error(`API Save Tutorial Error for post ${postId}:`, message);
    return c.json<SaveTutorialResponse>(
      { type: 'save-tutorial', success: false },
      500
    );
  }
});
