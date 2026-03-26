# Rune Realms — Master Task List

> **Project:** OSRS-inspired idle skilling + combat arena game running inside Reddit posts via Devvit + Unity WebGL.
>
> **Architecture:** Devvit web app (TypeScript/Hono server + HTML/TS client) loads a Unity WebGL build. Game logic lives in Unity (C#). Persistence, auth, and leaderboards live in the Devvit server (TypeScript + Redis). Communication between Unity ↔ Devvit happens via `SendMessage` / `UnityWebRequest` bridge.
>
> **Deploy command:** `npm run dev` (runs `devvit playtest`)

---

## Phase 0 — Project Setup & Branding

### 0.1 Rename & Configure
- [x] Rename project from `arena-realms` → `rune-realms` in `package.json` (`name` field)
- [x] Rename project from `arena-realms` → `rune-realms` in `devvit.json` (`name` field)
- [x] Update `src/server/core/post.ts` — change `title: 'arena-realms'` → `'Rune Realms'`
- [x] Update menu item labels/descriptions in `devvit.json` to say "Rune Realms" instead of "arena-realms"
- [x] Update `<title>` in `src/client/index.html` from "SampleGame" → "Rune Realms"
- [x] Update `<title>` in `src/client/splash.html` from "devvit" → "Rune Realms"

### 0.2 Splash Page Overhaul
- [ ] Replace `public/Snoo_Yes.png` with a Rune Realms logo/icon (or a styled text logo)
- [x] Restyle `src/client/splash.css` with an OSRS-inspired dark fantasy theme (dark background, gold accents, pixel/medieval font)
- [x] Update `src/client/splash.html` — replace boilerplate content with:
  - Game title "Rune Realms"
  - Tagline (e.g., "Skill. Fight. Conquer.")
  - "Play Now" button (replaces "Launch Unity Game")
  - Brief feature list (Idle Skilling • Combat Arena • Leaderboards)
- [x] Update `src/client/splash.ts` — set `titleElement.textContent` to a welcome message like `"Welcome, {username}! Ready to train?"`

### 0.3 Development Environment
- [ ] Clone the Unity Devvit template project: `https://github.com/reddit/devvit-unity-project`
- [ ] Open in Unity Hub (Unity 6.2+), confirm it builds to WebGL
- [ ] Do a test build → copy WebGL output into `public/Build/` → verify `npm run dev` loads it
- [ ] Set up a test subreddit for playtesting (e.g., `r/RuneRealms_dev`)
- [ ] Initialize git repo if not done, push to GitHub

---

## Phase 1 — Shared Data Types (`src/shared/`)

> Define all the types that both the server and client will use. This is the contract between Unity ↔ Devvit.

### 1.1 Player & Skill Types
- [x] Create `src/shared/types.ts` with the following type definitions:

```ts
// === Skills ===
export type SkillName = 'woodcutting' | 'fishing' | 'mining' | 'cooking' | 'smithing';

export type SkillData = {
  name: SkillName;
  xp: number;
  level: number;
};

export type PlayerSkills = {
  skills: SkillData[];
  lastSaveTimestamp: number; // Unix ms — used for offline progress calc
  activeSkill: SkillName | null;
};

// === Combat ===
export type ArenaResult = {
  kills: number;
  loot: LootItem[];
  wavesCleared: number;
  damageDealt: number;
};

export type LootItem = {
  itemId: string;
  name: string;
  quantity: number;
  rarity: 'common' | 'uncommon' | 'rare' | 'legendary';
};

// === Inventory ===
export type InventoryItem = {
  itemId: string;
  name: string;
  quantity: number;
  category: 'weapon' | 'armor' | 'tool' | 'material' | 'consumable';
  rarity: 'common' | 'uncommon' | 'rare' | 'legendary';
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
```

### 1.2 API Request/Response Types
- [x] Update `src/shared/api.ts` — replace the existing placeholder types with Rune Realms-specific types:

```ts
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
};

// --- Skills ---
export type SaveSkillsRequest = {
  type: 'save-skills';
  skills: PlayerSkills;
};

export type SaveSkillsResponse = {
  type: 'save-skills';
  success: boolean;
  offlineXpGained?: Record<string, number>; // skill name → xp gained while offline
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

// --- Leaderboard ---
export type GetLeaderboardRequest = {
  type: 'get-leaderboard';
  leaderboardType: LeaderboardType;
  limit?: number;
};

export type GetLeaderboardResponse = {
  type: 'get-leaderboard';
  entries: LeaderboardEntry[];
  playerRank: number | null;
};
```

---

## Phase 2 — Server API Endpoints (`src/server/`)

> All game persistence goes through Devvit Redis. Each endpoint handles validation, Redis read/write, and returns typed responses.

### 2.1 Redis Key Schema
- [x] Document the Redis key schema (add as comment block in `src/server/routes/api.ts`):

```
Key patterns:
  rune-realms:{postId}:{username}:skills    → JSON string of PlayerSkills
  rune-realms:{postId}:{username}:inventory → JSON string of PlayerInventory
  rune-realms:{postId}:{username}:profile   → JSON string of PlayerProfile
  rune-realms:{postId}:leaderboard:total-level → Sorted set (username → score)
  rune-realms:{postId}:leaderboard:total-xp    → Sorted set (username → score)
  rune-realms:{postId}:leaderboard:arena-kills → Sorted set (username → score)
```

### 2.2 Init Endpoint (Refactor Existing)
- [x] Refactor `GET /api/init` to:
  - Fetch the player's skills, inventory, and profile from Redis
  - If no data exists (new player), create default data:
    - All 5 skills at level 1, 0 XP
    - Empty inventory with 20 max slots
    - Fresh profile with `joinedTimestamp: Date.now()`
  - Calculate offline XP gains (compare `lastSaveTimestamp` to `Date.now()`)
  - Apply offline XP (cap at e.g. 8 hours max)
  - Save updated data back to Redis
  - Return full `InitResponse`

### 2.3 Save Skills Endpoint
- [x] Create `POST /api/save-skills`:
  - Accept `SaveSkillsRequest` body
  - Validate skill data (levels match XP formula, no negative values)
  - Save to Redis key `rune-realms:{postId}:{username}:skills`
  - Update leaderboard sorted sets (`total-level`, `total-xp`)
  - Return `SaveSkillsResponse`

### 2.4 Arena Result Endpoint
- [x] Create `POST /api/save-arena-result`:
  - Accept `SaveArenaResultRequest` body
  - Increment kill count in profile
  - Add loot items to inventory (check max slots)
  - Update `arena-kills` leaderboard
  - Save updated profile + inventory to Redis
  - Return `SaveArenaResultResponse` with new total kills and added items

### 2.5 Inventory Endpoint
- [x] Create `GET /api/inventory`:
  - Return current `PlayerInventory` from Redis
- [x] Create `POST /api/inventory/use`:
  - Accept `{ itemId: string }` — consume a consumable item
  - Decrement quantity, remove if 0
  - Return updated inventory

### 2.6 Leaderboard Endpoint
- [x] Create `GET /api/leaderboard`:
  - Accept query params: `type` (total-level | total-xp | arena-kills), `limit` (default 10)
  - Read from Redis sorted set using `zRange` with negative rank indices (descending)
  - Also fetch the requesting player's rank using `zRank`
  - Return `GetLeaderboardResponse`

### 2.7 Server Helpers
- [x] Create `src/server/core/redis-keys.ts` — centralized key builder functions:
  - `skillsKey(postId, username)` → `rune-realms:{postId}:{username}:skills`
  - `inventoryKey(postId, username)` → `rune-realms:{postId}:{username}:inventory`
  - `profileKey(postId, username)` → `rune-realms:{postId}:{username}:profile`
  - `leaderboardKey(postId, type)` → `rune-realms:{postId}:leaderboard:{type}`
- [x] Create `src/server/core/defaults.ts` — default data factories:
  - `createDefaultSkills()` → `PlayerSkills` with all skills at level 1
  - `createDefaultInventory()` → `PlayerInventory` with empty items, 20 slots
  - `createDefaultProfile(username)` → `PlayerProfile` with zeroed stats
- [x] Create `src/server/core/game-logic.ts` — shared game formulas:
  - `xpForLevel(level: number): number` — XP required to reach a given level (use OSRS-style curve or simplified)
  - `levelForXp(xp: number): number` — calculate level from total XP
  - `calculateOfflineXp(lastSave: number, now: number, activeSkill: SkillName | null): Record<string, number>` — offline gains (capped at 8 hours)

---

## Phase 3 — Unity Game (Separate Unity Project)

> These tasks happen in the Unity project (cloned from `reddit/devvit-unity-project`). The built WebGL files get copied into `public/Build/`.

### 3.1 Project Setup
- [ ] Delete the sample scene content
- [x] Create folder structure: `Assets/Scenes/`, `Assets/Scripts/`, `Assets/Prefabs/`, `Assets/UI/`, `Assets/Art/`
- [ ] Import free low-poly assets (Quaternius, Kenney, Poly Pizza — CC0 licensed):
  - Trees, rocks, ores (for skilling backgrounds)
  - Simple character model (or use capsule placeholder)
  - Enemy models (goblin, skeleton — or colored capsules)
  - Weapon/tool icons
  - UI sprites (buttons, panels, frames)

### 3.2 Main Scene & Tab System
- [ ] Create `MainScene` as the single game scene
- [ ] Build a tab navigation bar (bottom of screen, 4 tabs):
  - **Skills** — idle skilling view
  - **Arena** — combat view
  - **Inventory** — items/gear view
  - **Leaderboard** — top players view
- [x] Implement tab switching via Canvas Groups (alpha + interactable + blocksRaycasts toggle) — `TabManager.cs` created
- [ ] Each tab is a separate Canvas Group under a parent Canvas
- [ ] Style the tab bar OSRS-style (stone texture buttons, gold highlight for active tab)

### 3.3 Idle Skilling System (Unity C#)
- [x] Create `IdleManager.cs` script:
  - Skill array with: name, XP, level, isActive, progress bar ref, level text ref, button ref
  - `StartSkill(int index)` — stops current skill, starts ticking the new one
  - `SkillTick(int index)` coroutine — adds XP every 1 second, updates UI, auto-saves every level-up
  - XP per tick varies by skill (e.g., Woodcutting = 10, Mining = 8, Fishing = 12)
  - Level formula: `level = 1 + xp / 100` (simple) or OSRS curve (advanced)
- [ ] Build the Skills UI in Unity Editor:
  - 5 skill buttons in a vertical scroll or 2×3 grid (Woodcutting, Fishing, Mining, Cooking, Smithing)
  - Each button shows: skill icon, skill name, current level, XP progress bar
  - Active skill has a glowing/pulsing border
  - Big touch-friendly buttons (≥80px height)
- [x] Add visual feedback:
  - "+10 XP" floating text on each tick — `FloatingText.cs` created
  - Progress bar fill animation
  - Level-up flash/particle effect (floating text "LEVEL X!" implemented)
  - Sound effect on level-up (optional, add later)

### 3.4 Combat Arena System (Unity C#)
- [x] Create `CombatManager.cs` script:
  - Player stats: HP, max HP, damage, defense (derived from skill levels)
  - Enemy spawning: wave-based (enemies scale HP by 30% per wave)
  - `OnAttack()` / `OnSpecialAttack()` — deal damage, check for kill
  - `DealDamageToPlayer(int)` — reduce player HP with defense mitigation, check for death
  - `LootDrop()` — random loot table rolls on enemy kill
  - Auto-save after each run (via `GameManager.OnArenaComplete`)
- [x] Create `EnemyData.cs` ScriptableObject:
  - Stats: name, maxHP, damage, attackInterval, loot table with drop chances
  - Create via Assets → Create → Rune Realms → Enemy Data
  - Simple attack AI (coroutine-based auto-attack loop)
  - Enhanced with: description, difficulty enum (Easy/Medium/Hard/Boss), xpReward, minWave, specialAbility
- [x] Create `EnemyDatabase.cs` ScriptableObject:
  - Holds list of all EnemyData assets with helper methods
  - `GetEnemiesForWave()`, `GetRandomEnemy()`, `GetEnemiesByDifficulty()`
- [x] Create enemy definitions JSON reference (`unity/Assets/Data/EnemyDefinitions.json`):
  - 8 enemies across 4 tiers: Chicken, Goblin (Easy), Skeleton, Dark Wizard, Giant Spider (Medium), Hill Giant, Black Knight (Hard), Dragon (Boss)
  - Full loot tables with 29 unique items
- [x] Create item catalog reference (`unity/Assets/Data/ItemCatalog.json`):
  - 29 loot items + 4 consumables (Health Potion, Super Health Potion, Strength Potion, XP Lamp)
  - Each with itemId, name, category, rarity, description, stackable, maxStack
- [ ] Build the Arena UI in Unity Editor:
  - Player health bar (top-left)
  - Enemy health bar(s) (above enemies or top-right)
  - Action buttons: Attack, Special Attack, Heal (use potion from inventory), Run
  - Wave counter ("Wave 3")
  - Kill counter for current session
  - Loot popup on enemy death ("You received: Bronze Sword x1!")
- [x] Skill → Combat integration:
  - Mining level → +damage
  - Fishing level → +max HP
  - Cooking level → better healing from food
  - Smithing level → +defense
  - Woodcutting level → +special attack damage
- [x] CombatManager updated with EnemyDatabase support:
  - Optional `enemyDatabase` field with fallback to `enemyTypes` list
  - XP reward on enemy kill (grants xpReward to random combat skill)
  - XP reward text display during combat

### 3.5 Inventory UI (Unity C#)
- [x] Create `InventoryManager.cs` script:
  - List of items synced with server
  - `AddItem(InventoryItem item)` — add or stack
  - Rarity-colored slots, detail popup with "Use" button for consumables
  - Uses `DevvitBridge.UseItem()` for server-side consumption
- [ ] Build Inventory UI in Unity Editor:
  - Grid layout (4 columns × 5 rows = 20 slots)
  - Each slot shows: item icon, quantity badge, rarity border color
  - Tap item → detail popup (name, description, "Use" or "Drop" buttons)
  - Empty slots shown as dark squares
  - Slot count indicator ("14/20 slots used")
  - [ ] Create `InventorySlot` prefab
  - [ ] Create `ItemDetailPopup` panel

### 3.6 Leaderboard UI (Unity C#)
- [x] Create `LeaderboardManager.cs` script:
  - Fetch leaderboard from server on tab open via `DevvitBridge.FetchLeaderboard()`
  - Parse JSON response into display list
  - Show player's own rank at bottom
  - Gold/silver/bronze coloring for top 3
- [ ] Build Leaderboard UI in Unity Editor:
  - Tab sub-buttons: "Total Level" | "Total XP" | "Arena Kills"
  - Scrollable list of top 10 entries (rank, username, score)
  - Top 3 highlighted with gold/silver/bronze
  - "Your Rank: #42" fixed at bottom
  - [ ] Create `LeaderboardEntry` prefab

### 3.7 Devvit ↔ Unity Communication Bridge
- [x] Create `DevvitBridge.cs` script (attach to a persistent GameObject named "DevvitBridge"):
  - Generic `Get<T>()` / `Post<TReq, TRes>()` HTTP helpers via `UnityWebRequest`
  - Receives `OnInitData(json)` from JS via `SendMessage`
  - Convenience methods: `SaveSkills()`, `SaveArenaResult()`, `FetchLeaderboard()`, `UseItem()`
  - Server base URL is relative (same origin, e.g., `/api/...`)
  - Error handling with logging and optional callbacks
- [x] Create `GameManager.cs` (singleton, orchestrates everything):
  - Listens for `DevvitBridge.OnInitDataReceived` event
  - Distributes init data to all managers
  - Shows offline XP popup via `OfflinePopup.cs`
  - 30-second auto-save loop
  - Saves on tab switch, app pause, app quit
- [x] Create `GameData.cs` — all serializable C# data models matching server types
- [x] Create `OfflinePopup.cs` — "While you were away..." popup with XP gains display

### 3.8 WebGL Build & Export
- [ ] Configure Unity Build Settings:
  - Platform: Web (WebGL)
  - Player Settings → Publishing → Decompression Fallback = enabled
  - Compression: GZip for `.data.unityweb` and `.wasm.unityweb`, Disabled for `.framework.js`
- [ ] Build and copy output files to `public/Build/`:
  - `RuneRealms.data.unityweb`
  - `RuneRealms.framework.js`
  - `RuneRealms.loader.js`
  - `RuneRealms.wasm.unityweb`
- [x] Update `src/client/script.ts`:
  - Change all `SampleGame.*` references to `RuneRealms.*`
  - Update `companyName`, `productName`, `productVersion`

---

## Phase 4 — Client Updates (`src/client/`)

### 4.1 Unity Loader Script
- [x] Update `src/client/script.ts`:
  - Rename all build file references from `SampleGame` → `RuneRealms`
  - Update `companyName` → `"Rune Realms"`
  - Update `productName` → `"Rune Realms"`
  - Update `productVersion` → `"0.1.0"`
- [x] Add Devvit context injection into Unity:
  - After Unity instance loads, fetch `/api/init` and call `unityInstance.SendMessage('DevvitBridge', 'OnInitData', JSON.stringify(initData))`
  - Init data fetched in parallel with Unity load for faster startup

### 4.2 Game Page HTML
- [x] Update `src/client/index.html`:
  - Change title to "Rune Realms"
  - Add dark background color to body (prevent white flash while Unity loads)
  - [x] Style the loading bar to match game theme (gold gradient fill on dark brown track)
  - [x] Add a loading tip/flavor text element (8 OSRS-themed tips rotating every 3 seconds)

### 4.3 Splash Page
- [x] Redesign `src/client/splash.html`:
  - Game logo/title "Rune Realms" in medieval/fantasy style
  - Brief stats preview if returning player (total level, last skill trained)
  - "Enter the Realm" button (styled as stone/gold button)
  - Remove boilerplate template text and links
- [x] Update `src/client/splash.ts`:
  - On load, fetch `/api/init` to check if returning player
  - If returning: show stats (total level, arena kills, inventory count) + offline XP gains
  - If new: show "Begin your adventure! Tap below to enter."
  - Prefetch data so the Unity game loads faster
- [x] Update `src/client/splash.css`:
  - Dark parchment/stone background
  - Gold text for headings
  - OSRS-style button (stone texture, gold border, hover glow)
  - Fantasy font (Google Fonts: "MedievalSharp" or "Cinzel")

---

## Phase 5 — Offline Progress System

### 5.1 Server-Side Calculation
- [x] In `src/server/core/game-logic.ts`:
  - `calculateOfflineXp()` function:
    - Input: `lastSaveTimestamp`, `currentTimestamp`, `activeSkill`
    - Offline XP rate: 5 XP per minute for the active skill (reduced rate vs. active play)
    - Cap: maximum 8 hours of offline progress (2,400 XP max per session)
    - Return a map of `{ skillName: xpGained }`
  - Apply offline XP during `/api/init` before returning data
  - Recalculate levels after applying offline XP

### 5.2 Unity-Side Display
- [x] In `GameManager.cs`:
  - On init response, if `offlineXpGained` is present and non-zero:
    - Show a popup panel: "While you were away..." with per-skill XP gains (`OfflinePopup.cs`)
    - "Collect" button to dismiss
  - Save `lastSaveTimestamp` on every save call (server updates it automatically)

---

## Phase 6 — Leaderboard & Social

### 6.1 Leaderboard Backend
- [x] Implement Redis sorted sets for each leaderboard type
- [x] Update leaderboard scores on every save-skills and save-arena-result call
- [x] Add `GET /api/leaderboard?type={type}&limit={n}` endpoint (see Phase 2.6)

### 6.2 Rare Drop Notifications (Stretch Goal)
- [ ] When a player gets a rare/legendary drop in the arena:
  - Save to a Redis list: `rune-realms:{postId}:feed`
  - Other players can poll `GET /api/feed` to see recent rare drops
  - Display as a scrolling ticker in the arena tab: "🗡️ PlayerX just found a Dragon Scimitar!"

---

## Phase 7 — Polish & Mobile-First

### 7.1 UI/UX Polish
- [ ] All interactive buttons ≥ 80px tap target (mobile-friendly)
- [ ] Portrait orientation layout (Reddit mobile default)
- [ ] Consistent OSRS-inspired color palette:
  - Background: `#2b2b2b` (dark stone)
  - Panel: `#3a3125` (dark brown)
  - Text: `#ffcc00` (gold) and `#ffffff` (white)
  - Buttons: `#5c4a2a` (brown) with `#d4a438` (gold) border
  - Health bar: `#cc0000` (red) / `#00cc00` (green)
  - XP bar: `#00aaff` (blue)
- [ ] Loading states for all async operations (spinner or skeleton UI)
- [ ] Error handling: show toast messages for network failures, retry button
- [ ] Smooth tab transitions (fade or slide)

### 7.2 Audio (Optional — Low Priority)
- [ ] Find free .ogg sound effects (OpenGameArt, Freesound):
  - Skill tick sound (chop, splash, pickaxe hit)
  - Level-up fanfare
  - Combat hit / enemy death
  - Loot pickup
  - Button click
- [x] Add AudioSource components in Unity, wire to game events
  - Created `AudioManager.cs` singleton with 16 serialized AudioClip fields, per-skill tick resolution, pitch variation, WebGL user-interaction gate, PlayerPrefs persistence
  - Created `AudioConstants.cs` with PlayerPrefs keys, pitch ranges, default volumes
  - Created `MuteToggle.cs` UI component (Toggle + volume Slider support)
  - Updated `SETUP.md` with Step 11: Audio Setup (wiring, free asset sources, WebGL limitations)
- [x] Add a mute toggle button in the UI (MuteToggle.cs created with icon swap + volume slider)

### 7.3 Tutorial / First-Time Experience
- [x] On first play (isNewPlayer = true):
  - Show a brief overlay tutorial (7 steps):
    1. "Welcome to Rune Realms!" — intro overview
    2. "Tap a skill to start training" — highlights Skills tab
    3. "Skills level up over time — even while you're away!" — idle/offline
    4. "Visit the Arena to fight monsters and earn loot" — highlights Arena tab
    5. "Check your Inventory for items and equipment" — highlights Inventory tab
    6. "Compete on the Leaderboard!" — highlights Leaderboard tab
    7. "You're ready! Start your adventure!" — completion
  - Created `TutorialManager.cs` with CanvasGroup fade, highlight frame, skip button, step counter
  - Updated `GameManager.cs` to show tutorial for new players
  - Updated `DevvitBridge.cs` with `SaveTutorial()` convenience method
  - Updated `GameData.cs` with TutorialProgress, SaveTutorialRequest, SaveTutorialResponse
- [x] Save `tutorialCompleted: true` in Redis so it doesn't show again
  - Added `TutorialProgress` type to `src/shared/types.ts`
  - Added `SaveTutorialRequest`/`SaveTutorialResponse` to `src/shared/api.ts`
  - Added `tutorial` field to `InitResponse`
  - Added `tutorialKey()` to `redis-keys.ts`
  - Added `createDefaultTutorial()` to `defaults.ts`
  - Added `POST /api/save-tutorial` endpoint to `api.ts`
  - Added `GET /api/init` now loads/returns tutorial state
  - Added tests for `createDefaultTutorial()` (46 total tests passing)

### 7.4 Auto-Save System
- [x] Unity: auto-save every 30 seconds via coroutine in `GameManager.cs`
- [x] Unity: save on tab switch (switching between Skills/Arena/Inventory)
- [x] Unity: save on `OnApplicationPause(true)` (mobile background)
- [x] Unity: save on `OnApplicationQuit()` (closing browser/tab)
- [x] Server: always update `lastSaveTimestamp` on any save endpoint

---

## Phase 8 — Testing & QA

### 8.1 Server Tests
- [x] Write tests for `src/server/core/game-logic.ts`:
  - `xpForLevel()` returns correct values
  - `levelForXp()` is the inverse of `xpForLevel()`
  - `calculateOfflineXp()` caps at 8 hours
  - `calculateOfflineXp()` returns 0 if no active skill
- [ ] Write tests for API endpoints (mock Redis):
  - `/api/init` returns defaults for new player
  - `/api/init` applies offline XP for returning player
  - `/api/save-skills` validates and persists data
  - `/api/save-arena-result` increments kills and adds loot
  - `/api/leaderboard` returns sorted entries

### 8.2 Integration Testing
- [ ] Test full flow: splash → init → skill training → save → reload → offline XP applied
- [ ] Test arena flow: enter → fight → loot drops → inventory updated → leaderboard updated
- [ ] Test on Reddit mobile app (iOS)
- [ ] Test on Reddit mobile app (Android)
- [ ] Test on Reddit desktop (Chrome, Firefox)
- [ ] Test with multiple users simultaneously (different Reddit accounts)

### 8.3 Edge Cases
- [ ] What happens if Redis is empty (brand new post)?
- [ ] What happens if a user has no Reddit username (anonymous)?
- [ ] What happens if inventory is full when loot drops?
- [ ] What happens if network disconnects mid-save?
- [ ] What happens if user opens the game in two tabs?

---

## Phase 9 — Build, Deploy & Publish

### 9.1 First Deployment
- [ ] Final Unity WebGL build with all game features
- [ ] Copy build files to `public/Build/`
- [ ] Update all file references in `src/client/script.ts`
- [x] Run `npm run type-check` — fix any TypeScript errors
- [x] Run `npm run lint` — fix any linting issues
- [x] Run `npm run test` — all tests pass (46 tests passing)
- [ ] Run `npm run dev` — playtest on test subreddit
- [ ] Verify game loads, skills work, arena works, saves persist across page reloads

### 9.2 Publishing
- [ ] Update `README.md` with:
  - Game description and screenshots
  - "How to Play" section
  - Feature list
  - Credits for CC0 assets used
- [ ] Run `npm run deploy` (type-check + lint + test + devvit upload)
- [ ] Submit for Reddit review
- [ ] After approval: share in r/Devvit and r/GamesOnReddit for feedback
- [ ] For public listing: `npx devvit publish --public`

---

## Phase 10 — Monetization (Post-Launch)

### 10.1 Reddit Developer Funds
- [ ] Enroll in the Reddit Developer Program (account settings → Developer → verify)
- [ ] Focus on maximizing Daily Qualified Engagers (daily active players)
- [ ] Target: Tier 1 (500 engagers) → $500 payout
- [ ] Promote in relevant subreddits (r/2007scape, r/gaming, r/incremental_games)

### 10.2 In-App Purchases (Cosmetics Only)
- [ ] Design cosmetic items (skins, visual effects — no pay-to-win):
  - Golden tool skins (axe, pickaxe, rod)
  - Player glow effects / auras
  - Arena themes (different backgrounds)
  - Cosmetic titles ("Dragon Slayer", "Master Skiller")
- [ ] Create product SKUs in Reddit payments dashboard
- [ ] Add `GET /api/orders` endpoint using `payments.getOrders({ sku })` from `@devvit/web/server`
- [ ] Build a "Shop" tab in Unity (5th tab or sub-menu)
- [ ] "Buy with Gold" buttons that trigger the native Reddit purchase UI
- [ ] Test purchases in sandbox mode before going live

---

## Quick Reference — File Map

| File | Purpose |
|------|---------|
| `devvit.json` | App config, entrypoints, menu items, triggers |
| `package.json` | Dependencies, scripts (`npm run dev` etc.) |
| `src/shared/types.ts` | Game data types (skills, inventory, combat) |
| `src/shared/api.ts` | API request/response types |
| `src/server/index.ts` | Hono server entry — mounts all routes |
| `src/server/routes/api.ts` | Game API endpoints (init, save, leaderboard) |
| `src/server/routes/menu.ts` | Reddit menu actions (create post) |
| `src/server/routes/triggers.ts` | App install trigger |
| `src/server/routes/forms.ts` | Form submissions |
| `src/server/core/post.ts` | Post creation helper |
| `src/server/core/redis-keys.ts` | Redis key builder functions ✅ |
| `src/server/core/defaults.ts` | Default player data factories ✅ |
| `src/server/core/game-logic.ts` | Game formulas & offline calc ✅ |
| `src/server/core/game-logic.test.ts` | Unit tests for game logic (32 tests) ✅ |
| `src/server/core/defaults.test.ts` | Unit tests for defaults (14 tests) ✅ |
| `eslint.config.js` | ESLint flat config ✅ |
| `vitest.config.ts` | Vitest test runner config ✅ |
| `unity/Assets/Scripts/Audio/AudioManager.cs` | Singleton audio manager (SFX + music) ✅ |
| `unity/Assets/Scripts/Audio/AudioConstants.cs` | Audio PlayerPrefs keys and pitch constants ✅ |
| `unity/Assets/Scripts/Audio/MuteToggle.cs` | Mute toggle + volume slider UI ✅ |
| `unity/Assets/Scripts/Combat/EnemyDatabase.cs` | ScriptableObject holding all enemy configs ✅ |
| `unity/Assets/Scripts/UI/TutorialManager.cs` | Multi-step tutorial overlay for new players ✅ |
| `unity/Assets/Data/EnemyDefinitions.json` | 8 enemy definitions across 4 difficulty tiers ✅ |
| `unity/Assets/Data/ItemCatalog.json` | 33-item master catalog (loot + consumables) ✅ |
| `src/client/splash.html` | Inline view — shown in Reddit feed |
| `src/client/splash.ts` | Splash page logic |
| `src/client/splash.css` | Splash page styles |
| `src/client/index.html` | Expanded view — loads Unity game |
| `src/client/script.ts` | Unity WebGL loader & bridge |
| `public/Build/*` | Unity WebGL build output files |

---

## Recommended Build Order

> Work through these in order. Each step produces something testable.

1. **Phase 0** — Rename everything, get a clean starting point
2. **Phase 1** — Define all shared types (quick, unlocks everything else)
3. **Phase 2.7** — Build server helpers first (redis-keys, defaults, game-logic)
4. **Phase 2.2** — Refactor `/api/init` with real game data
5. **Phase 2.3** — Save skills endpoint
6. **Phase 3.1–3.3** — Unity: scene setup + idle skilling (first playable!)
7. **Phase 3.7–3.8** — Unity: Devvit bridge + WebGL build → test end-to-end
8. **Phase 4** — Update client loader + splash page
9. **Phase 2.4** — Arena result endpoint
10. **Phase 3.4** — Unity: combat arena (second feature!)
11. **Phase 2.5** — Inventory endpoints
12. **Phase 3.5** — Unity: inventory UI
13. **Phase 5** — Offline progress (the idle game magic)
14. **Phase 2.6 + 6** — Leaderboards
15. **Phase 3.6** — Unity: leaderboard UI
16. **Phase 7** — Polish pass
17. **Phase 8** — Testing
18. **Phase 9** — Deploy and publish
19. **Phase 10** — Monetization (post-launch)

---

## Free Asset Sources (CC0 / Open License)

| Source | What to Get | URL |
|--------|------------|-----|
| Quaternius | Characters, weapons, trees, rocks, animals | https://quaternius.com/ |
| Kenney | Low poly nature, props, UI elements | https://kenney.nl/assets?q=low+poly |
| Poly Pizza | Search "sword", "goblin", "axe", "tree" | https://poly.pizza/ |
| OpenGameArt | 3D low-poly packs, sound effects | https://opengameart.org/ |
| Freesound | Sound effects (chop, splash, combat) | https://freesound.org/ |
| Google Fonts | "MedievalSharp" or "Cinzel" for UI text | https://fonts.google.com/ |