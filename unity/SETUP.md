# Rune Realms — Unity Project Setup Guide

> Step-by-step instructions for setting up the Unity project, wiring the scene hierarchy, and building to WebGL for Devvit.

---

## Prerequisites

- **Unity 6.2+** (Unity Hub → Install latest Unity 6 LTS)
- **Devvit Unity Template** cloned: `git clone https://github.com/reddit/devvit-unity-project.git`
- **TextMeshPro** (included by default in Unity 6)
- **Newtonsoft.Json** (install via Package Manager → `com.unity.nuget.newtonsoft-json`)

---

## Step 1: Import Scripts

1. Open the cloned Devvit Unity project in Unity Hub.
2. Delete the contents of `Assets/Scenes/SampleScene` (clear the default objects).
3. Copy the entire `Scripts/` folder from this repo's `unity/Assets/Scripts/` into your Unity project's `Assets/Scripts/`.

Your `Assets/Scripts/` folder should look like:

```
Assets/Scripts/
├── Combat/
│   ├── CombatManager.cs
│   └── EnemyData.cs
├── Core/
│   ├── DevvitBridge.cs
│   └── GameManager.cs
├── Data/
│   └── GameData.cs
├── Inventory/
│   ├── InventoryManager.cs
├── Leaderboard/
│   └── LeaderboardManager.cs
├── Skills/
│   └── IdleManager.cs
└── UI/
    ├── FloatingText.cs
    ├── OfflinePopup.cs
    └── TabManager.cs
```

4. Wait for Unity to compile. If you get a Newtonsoft.Json error:
   - Window → Package Manager → `+` → Add package by name → `com.unity.nuget.newtonsoft-json`

---

## Step 2: Create the Scene Hierarchy

Create a new scene: `Assets/Scenes/MainScene.unity` and build the following hierarchy.

### 2.1 Root GameObjects

```
MainScene
├── DevvitBridge          ← Empty GO, attach DevvitBridge.cs
├── GameManager           ← Empty GO, attach GameManager.cs
├── MainCanvas            ← Canvas (Screen Space - Overlay)
│   ├── SkillsTab         ← Panel with CanvasGroup, attach IdleManager.cs
│   ├── ArenaTab          ← Panel with CanvasGroup, attach CombatManager.cs
│   ├── InventoryTab      ← Panel with CanvasGroup, attach InventoryManager.cs
│   ├── LeaderboardTab    ← Panel with CanvasGroup, attach LeaderboardManager.cs
│   ├── TabBar            ← Horizontal Layout at bottom
│   └── OfflinePopup      ← Panel (disabled by default), attach OfflinePopup.cs
├── EventSystem           ← Auto-created with Canvas
```

### 2.2 DevvitBridge GameObject

| Property | Value |
|----------|-------|
| Name | `DevvitBridge` (MUST match this exact name — JS calls `SendMessage('DevvitBridge', ...)`) |
| Script | `DevvitBridge.cs` |

### 2.3 GameManager GameObject

| Property | Value |
|----------|-------|
| Name | `GameManager` |
| Script | `GameManager.cs` |

Drag-assign these references in the Inspector:

| Field | Target |
|-------|--------|
| Idle Manager | `SkillsTab` (IdleManager component) |
| Combat Manager | `ArenaTab` (CombatManager component) |
| Inventory Manager | `InventoryTab` (InventoryManager component) |
| Leaderboard Manager | `LeaderboardTab` (LeaderboardManager component) |
| Tab Manager | `MainCanvas` or TabBar (TabManager component) |
| Offline Popup | `OfflinePopup` (OfflinePopup component) |
| Auto Save Interval | `30` |

### 2.4 MainCanvas

| Setting | Value |
|---------|-------|
| Render Mode | Screen Space - Overlay |
| Canvas Scaler → UI Scale Mode | Scale With Screen Size |
| Canvas Scaler → Reference Resolution | 1080 × 1920 (portrait) |
| Canvas Scaler → Match Width Or Height | 0.5 |

Attach `TabManager.cs` to `MainCanvas` (or a dedicated TabController child).

---

## Step 3: Build the Tab Bar

Create a horizontal layout at the bottom of the canvas:

```
TabBar (RectTransform: bottom-anchored, height 80px)
├── HorizontalLayoutGroup (spacing: 4, child force expand width)
├── SkillsTabBtn    ← Button + Image
├── ArenaTabBtn     ← Button + Image
├── InventoryTabBtn ← Button + Image
├── LeaderboardBtn  ← Button + Image
```

### TabManager Setup

On the `TabManager` component, create 4 tab entries:

| Index | Name | Tab Button | Content Panel | Active Color | Inactive Color |
|-------|------|------------|---------------|--------------|----------------|
| 0 | Skills | SkillsTabBtn | SkillsTab (CanvasGroup) | #D4A438 (gold) | #5C4A2A (brown) |
| 1 | Arena | ArenaTabBtn | ArenaTab (CanvasGroup) | #D4A438 | #5C4A2A |
| 2 | Inventory | InventoryTabBtn | InventoryTab (CanvasGroup) | #D4A438 | #5C4A2A |
| 3 | Leaderboard | LeaderboardBtn | LeaderboardTab (CanvasGroup) | #D4A438 | #5C4A2A |

**Each content panel MUST have a `CanvasGroup` component** — the TabManager toggles `alpha`, `interactable`, and `blocksRaycasts`.

---

## Step 4: Build the Skills Tab

```
SkillsTab (Panel + CanvasGroup + IdleManager.cs)
├── SkillsTitle (TextMeshPro: "Skills")
├── SkillGrid (Vertical Layout Group, spacing 8, padding 16)
│   ├── WoodcuttingSlot
│   │   ├── Button (full slot is clickable, min height 80px)
│   │   ├── SkillIcon (Image — axe icon or placeholder)
│   │   ├── SkillName (TextMeshPro: "Woodcutting")
│   │   ├── LevelText (TextMeshPro: "Lv.1")
│   │   ├── ProgressBar (Slider — fill area colored #00AAFF)
│   │   └── ActiveGlow (Image/Outline — hidden by default)
│   ├── FishingSlot (same structure)
│   ├── MiningSlot (same structure)
│   ├── CookingSlot (same structure)
│   └── SmithingSlot (same structure)
```

### IdleManager Setup

On the `IdleManager` component, set up `skillSlots` (size 5):

| Index | Skill Name | Button | Progress Bar | Level Text | Name Text | Active Indicator |
|-------|-----------|--------|--------------|------------|-----------|-----------------|
| 0 | woodcutting | WoodcuttingSlot/Button | .../ProgressBar | .../LevelText | .../SkillName | .../ActiveGlow |
| 1 | fishing | FishingSlot/Button | ... | ... | ... | ... |
| 2 | mining | MiningSlot/Button | ... | ... | ... | ... |
| 3 | cooking | CookingSlot/Button | ... | ... | ... | ... |
| 4 | smithing | SmithingSlot/Button | ... | ... | ... | ... |

XP Per Tick values (adjustable):
- Woodcutting: 10
- Fishing: 12
- Mining: 8
- Cooking: 9
- Smithing: 11

### Skill Slot Styling

| Element | Style |
|---------|-------|
| Slot background | `#3A3125` (dark brown panel) |
| Slot border | `#5C4A2A` with 2px outline |
| Skill name text | `#B8A88A` (parchment), 14pt |
| Level text | `#D4A438` (gold), 18pt bold |
| Progress bar track | `#2A2218` (very dark) |
| Progress bar fill | `#00AAFF` (XP blue) |
| Active glow | `#D4A438` at 40% alpha, pulsing (use Animation or script) |
| Minimum slot height | 80px (touch-friendly) |

---

## Step 5: Build the Arena Tab

```
ArenaTab (Panel + CanvasGroup + CombatManager.cs)
├── CombatPanel (active during combat)
│   ├── PlayerSection
│   │   ├── PlayerHpBar (Slider — green fill #00CC00)
│   │   ├── PlayerHpText (TextMeshPro: "50/50")
│   │   └── PlayerStatsText (TextMeshPro: "ATK: 12  DEF: 3  SPL: 23")
│   ├── EnemySection
│   │   ├── EnemyIcon (Image)
│   │   ├── EnemyNameText (TextMeshPro: "Goblin")
│   │   ├── EnemyHpBar (Slider — red fill #CC0000)
│   │   └── EnemyHpText (TextMeshPro: "30/30")
│   ├── CombatInfo
│   │   ├── WaveText (TextMeshPro: "Wave 1")
│   │   ├── KillCountText (TextMeshPro: "Kills: 0")
│   │   └── LootText (TextMeshPro: "")
│   └── ActionButtons (Horizontal Layout, min height 80px per button)
│       ├── AttackBtn (Button: "Attack" — #5C4A2A bg, #FFCC00 text)
│       ├── SpecialBtn (Button: "Special" — #6B3A2A bg, #FF8800 text)
│       ├── HealBtn (Button: "Heal" — #2A5C3A bg, #00FF88 text)
│       └── RunBtn (Button: "Run" — #4A4A4A bg, #CCCCCC text)
├── VictoryPanel (shown when not in combat)
│   ├── ResultsTitle (TextMeshPro: "Arena")
│   ├── ResultsSummary (TextMeshPro — filled dynamically)
│   └── StartBattleBtn (Button: "Start Battle!" — #5C4A2A bg, gold border)
```

### CombatManager Setup

| Field | Target |
|-------|--------|
| Enemy Types | List of `EnemyData` ScriptableObjects (see below) |
| Player Hp Bar | PlayerHpBar Slider |
| Player Hp Text | PlayerHpText TMP |
| Player Stats Text | PlayerStatsText TMP |
| Enemy Hp Bar | EnemyHpBar Slider |
| Enemy Hp Text | EnemyHpText TMP |
| Enemy Name Text | EnemyNameText TMP |
| Enemy Icon | EnemyIcon Image |
| Attack Button | AttackBtn |
| Special Button | SpecialBtn |
| Heal Button | HealBtn |
| Run Button | RunBtn |
| Wave Text | WaveText TMP |
| Kill Count Text | KillCountText TMP |
| Loot Text | LootText TMP |
| Combat Panel | CombatPanel GO |
| Victory Panel | VictoryPanel GO |
| Start Battle Button | StartBattleBtn |

### Create Enemy ScriptableObjects

Right-click in Project → Create → Rune Realms → Enemy Data.

Create 3–5 enemies:

| Enemy | Max HP | Damage | Attack Interval | Loot |
|-------|--------|--------|-----------------|------|
| **Goblin** | 30 | 5 | 2.0s | Goblin Bones (common, 80%), Bronze Dagger (uncommon, 15%) |
| **Skeleton** | 50 | 8 | 1.8s | Bones (common, 90%), Iron Sword (uncommon, 20%), Shield Half (rare, 3%) |
| **Dark Wizard** | 40 | 12 | 2.5s | Runes (common, 70%), Staff (uncommon, 25%), Wizard Hat (rare, 5%) |
| **Hill Giant** | 80 | 10 | 2.2s | Big Bones (common, 90%), Giant Key (rare, 8%), Rune Helm (legendary, 1%) |
| **Dragon** | 150 | 18 | 3.0s | Dragon Bones (common, 95%), Dragon Hide (uncommon, 40%), Dragon Visage (legendary, 0.5%) |

---

## Step 6: Build the Inventory Tab

```
InventoryTab (Panel + CanvasGroup + InventoryManager.cs)
├── InventoryTitle (TextMeshPro: "Inventory")
├── SlotCountText (TextMeshPro: "0/20 slots")
├── SlotGrid (Grid Layout Group — 4 columns, cell size 80×80, spacing 8)
│   └── (Slots are instantiated at runtime from prefab)
├── ItemDetailPopup (Panel — disabled by default)
│   ├── DetailName (TextMeshPro: item name)
│   ├── DetailCategory (TextMeshPro: "Weapon")
│   ├── DetailQuantity (TextMeshPro: "Quantity: 1")
│   ├── RarityBorder (Image — colored by rarity)
│   ├── UseButton (Button: "Use")
│   └── CloseButton (Button: "✕")
```

### Create Inventory Slot Prefab

Create a prefab `Assets/Prefabs/InventorySlot.prefab`:

```
InventorySlot (Button + Image component)
├── ItemName (TextMeshPro — centered, 10pt, #B8A88A)
└── Quantity (TextMeshPro — bottom-right corner, 9pt, #FFFFFF)
```

| Element | Style |
|---------|-------|
| Slot size | 80×80 px |
| Background (empty) | `#1E1810` at 50% alpha |
| Background (common) | `#666666` |
| Background (uncommon) | `#33CC33` |
| Background (rare) | `#3366FF` |
| Background (legendary) | `#FF9900` |

### InventoryManager Setup

| Field | Target |
|-------|--------|
| Slot Container | SlotGrid Transform |
| Slot Prefab | InventorySlot prefab |
| Slot Count Text | SlotCountText TMP |
| Detail Popup | ItemDetailPopup GO |
| Detail Name | DetailName TMP |
| Detail Category | DetailCategory TMP |
| Detail Quantity | DetailQuantity TMP |
| Detail Rarity Border | RarityBorder Image |
| Use Button | UseButton |
| Close Popup Button | CloseButton |

---

## Step 7: Build the Leaderboard Tab

```
LeaderboardTab (Panel + CanvasGroup + LeaderboardManager.cs)
├── LeaderboardTitle (TextMeshPro: "Leaderboard")
├── TypeButtons (Horizontal Layout)
│   ├── TotalLevelBtn (Button: "Total Level")
│   ├── TotalXpBtn (Button: "Total XP")
│   └── ArenaKillsBtn (Button: "Arena Kills")
├── EntryList (Vertical Layout Group + Content Size Fitter)
│   └── (Entries instantiated at runtime from prefab)
├── PlayerRankText (TextMeshPro: "Your Rank: —", anchored to bottom)
```

### Create Leaderboard Entry Prefab

Create `Assets/Prefabs/LeaderboardEntry.prefab`:

```
LeaderboardEntry (Horizontal Layout Group, height 40px)
├── RankText (TextMeshPro: "#1", width 50px, left-aligned)
├── UsernameText (TextMeshPro: "PlayerName", flex expand)
└── ScoreText (TextMeshPro: "42", width 80px, right-aligned)
```

### LeaderboardManager Setup

| Field | Target |
|-------|--------|
| Total Level Button | TotalLevelBtn |
| Total XP Button | TotalXpBtn |
| Arena Kills Button | ArenaKillsBtn |
| Entry Container | EntryList Transform |
| Entry Prefab | LeaderboardEntry prefab |
| Player Rank Text | PlayerRankText TMP |

---

## Step 8: Build the Offline Popup

```
OfflinePopup (Panel — disabled by default, attach OfflinePopup.cs)
├── Background (Image — #000000 at 60% alpha, full screen, blocks raycasts)
├── PopupCard (Panel — centered, 400×300px, #3A3125 bg, gold border)
│   ├── TitleText (TextMeshPro: "While you were away...", #D4A438, 18pt)
│   ├── GainsText (TextMeshPro: gains list, #B8A88A, 14pt)
│   └── CollectButton (Button: "Collect!", gold style, min height 50px)
```

### OfflinePopup Setup

| Field | Target |
|-------|--------|
| Popup Panel | OfflinePopup GO (or the PopupCard) |
| Title Text | TitleText TMP |
| Gains Text | GainsText TMP |
| Collect Button | CollectButton |

---

## Step 9: Color & Style Reference

Use these consistently across all UI elements:

| Token | Hex | Usage |
|-------|-----|-------|
| Background | `#1A1410` | Main background, canvas clear color |
| Panel | `#3A3125` | Card/panel backgrounds |
| Panel Dark | `#2A2218` | Progress bar tracks, empty slots |
| Border | `#5C4A2A` | Panel borders, inactive tab buttons |
| Gold | `#D4A438` | Headings, active tabs, highlights |
| Gold Bright | `#FFCC00` | Button text, level numbers |
| Parchment | `#B8A88A` | Body text, descriptions |
| White | `#FFFFFF` | Emphasis text, quantities |
| XP Blue | `#00AAFF` | XP progress bars |
| Health Green | `#00CC00` | Player HP bar |
| Damage Red | `#CC0000` | Enemy HP bar, damage numbers |
| Heal Green | `#00FF88` | Heal numbers |
| Muted | `#665544` | Footer text, disabled state |

### Font Recommendations

- **Headings/Titles**: Use a medieval serif font (search Unity Asset Store for "Medieval Font" or import Google Fonts "Cinzel" as a TMP Font Asset)
- **Body/Numbers**: TextMeshPro default (Liberation Sans) or any clean sans-serif
- **Minimum font size**: 14pt for body, 10pt for labels

---

## Step 10: Build to WebGL & Deploy

### Build Settings

1. **File → Build Profiles → Web** (switch platform if needed)
2. **Player Settings → Publishing Settings:**
   - Decompression Fallback: **Enabled**
3. **Player Settings → Resolution and Presentation:**
   - Default Canvas Width: 1080
   - Default Canvas Height: 1920
   - Run In Background: **Enabled**

### Build Process (Two-Step)

Unity WebGL requires a two-step build for Devvit compatibility:

**Build 1 — Compressed assets:**
1. Player Settings → Publishing Settings → Compression Format: **GZip**
2. Build to a temporary folder (e.g., `WebGLBuild/`)
3. Copy these files to your Devvit project's `public/Build/`:
   - `RuneRealms.data.unityweb`
   - `RuneRealms.wasm.unityweb`

**Build 2 — Uncompressed framework:**
1. Player Settings → Publishing Settings → Compression Format: **Disabled**
2. Build again to the same folder
3. Copy these files to `public/Build/`:
   - `RuneRealms.framework.js`
   - `RuneRealms.loader.js`

### Important Build Notes

- The **Product Name** in Player Settings MUST be `RuneRealms` (this determines the output filenames)
- The **Company Name** should be `Rune Realms` (or your studio name)
- Make sure **Strip Engine Code** is enabled for smaller builds
- Enable **Data Caching** for faster subsequent loads

### Deploy to Devvit

After copying the build files:

```bash
cd /path/to/rune-realms    # Your Devvit project root
npm run dev                 # Starts devvit playtest
```

Open the playtest URL in your browser or the Reddit app. You should see:
1. The themed splash page ("Rune Realms — Skill. Fight. Conquer.")
2. Click "Enter the Realm"
3. Unity loads with gold-themed loading bar and rotating tips
4. Game initializes → Skills tab shown → tap a skill to start training!

---

## Troubleshooting

### "DevvitBridge not found"
- Make sure there's a GameObject named exactly `DevvitBridge` in the scene
- The `DevvitBridge.cs` script must be attached to it
- It must exist at scene load (not instantiated later)

### "Newtonsoft.Json not found"
- Window → Package Manager → `+` → Add package by name → `com.unity.nuget.newtonsoft-json`

### "API calls return errors"
- Unity WebGL runs in a sandboxed iframe — API calls must use **relative URLs** (`/api/init`, not `http://...`)
- Check the browser console (F12) for CORS or network errors
- Make sure `npm run dev` is running in the Devvit project

### "Floating text appears in wrong position"
- `FloatingText.Spawn()` uses world/screen position — make sure the Canvas is in Screen Space Overlay mode
- The position should come from UI element `transform.position` (screen coords)

### "Skills don't resume after reload"
- The server saves `activeSkill` in `PlayerSkills.lastSaveTimestamp`
- `IdleManager.Initialize()` checks for a saved `activeSkill` and resumes it
- Make sure `GameManager.SaveAll()` is being called (check the 30s auto-save loop)

### Build is too large
- Enable **Strip Engine Code** in Player Settings
- Remove unused packages from Package Manager
- Compress textures to lower resolutions
- Use simple materials (Unlit or Mobile shaders)
- Target file sizes: data < 10MB, wasm < 5MB, framework < 1MB

---

## Scene Setup Checklist

Use this checklist to verify everything is wired correctly:

- [ ] `DevvitBridge` GameObject exists with exact name "DevvitBridge"
- [ ] `DevvitBridge.cs` attached to it
- [ ] `GameManager` GameObject exists with `GameManager.cs`
- [ ] All manager references assigned in GameManager Inspector
- [ ] `MainCanvas` has Canvas Scaler set to 1080×1920 portrait
- [ ] All 4 tab content panels have `CanvasGroup` components
- [ ] `TabManager` has all 4 tabs configured with buttons + panels
- [ ] `IdleManager` has 5 skill slots configured
- [ ] `CombatManager` has at least 1 EnemyData ScriptableObject
- [ ] `CombatManager` has all UI references assigned
- [ ] `InventoryManager` has slot prefab and container assigned
- [ ] `LeaderboardManager` has entry prefab and container assigned
- [ ] `OfflinePopup` panel starts disabled
- [ ] `OfflinePopup` has all text + button references assigned
- [ ] Inventory Slot prefab exists at `Assets/Prefabs/InventorySlot`
- [ ] Leaderboard Entry prefab exists at `Assets/Prefabs/LeaderboardEntry`
- [ ] At least 1 EnemyData asset exists (Create → Rune Realms → Enemy Data)
- [ ] Build target is set to **Web** platform
- [ ] Product Name in Player Settings is `RuneRealms`
- [ ] Newtonsoft.Json package is installed
- [ ] `AudioManager` GameObject exists with `AudioManager.cs` attached
- [ ] Audio clips assigned in AudioManager Inspector (at minimum `skillTickDefault`, `attackHitClip`, `buttonClickClip`)
- [ ] `MuteToggle` wired up in the settings/header UI

---

## Step 11: Audio Setup

Rune Realms uses a singleton `AudioManager` for all sound effects, music, and ambient audio. The system lives in `Assets/Scripts/Audio/` and consists of three scripts:

| Script | Purpose |
|---|---|
| `AudioManager.cs` | Singleton manager — plays SFX, manages volume/mute, persists prefs |
| `AudioConstants.cs` | Static constants for PlayerPrefs keys, pitch ranges, and clip docs |
| `MuteToggle.cs` | UI component for mute toggle and volume sliders |

### 11.1 Create the AudioManager GameObject

1. In the Scene Hierarchy, create an empty GameObject at the root level:
   - **Name**: `AudioManager`
2. Attach `AudioManager.cs` (from `Assets/Scripts/Audio/`) to this GameObject.
3. The script automatically creates two `AudioSource` components at runtime:
   - **SFX Source** — used for one-shot sound effects (`PlayOneShot`)
   - **Music Source** — used for looping background music/ambient
4. Alternatively, add two `AudioSource` components manually and drag them into the `Sfx Source` and `Music Source` Inspector fields for more control over spatial/3D settings.

> **Important:** `AudioManager` uses `DontDestroyOnLoad`, so place it at the scene root — not inside a Canvas or other parent that might get destroyed.

### 11.2 Import & Assign Audio Clips

#### Recommended Free Sound Sources

| Source | URL | License |
|---|---|---|
| OpenGameArt | https://opengameart.org | Various (CC0, CC-BY, GPL) |
| Freesound | https://freesound.org | CC0, CC-BY |
| Unity Asset Store | Search "free SFX" | Per-asset license |
| Kenney.nl | https://kenney.nl/assets | CC0 |

#### Audio Format Tips

- Use **`.ogg`** for WebGL builds — smaller file size, good compression.
- **`.wav`** works but increases build size significantly.
- Keep SFX clips **under 2 seconds** for responsiveness.
- Music/ambient loops can be longer but keep them **under 2 MB** compressed.

#### Recommended Clip Assignments

Select the `AudioManager` GameObject and assign clips in the Inspector. The fields are organized into groups:

**Skill Sounds**
| Field | Recommended Sound | Notes |
|---|---|---|
| `Skill Tick Default` | Short tick / chime | Fallback for any skill without its own clip |
| `Skill Tick Mining` | Pickaxe clink / stone hit | Plays each mining XP tick |
| `Skill Tick Fishing` | Water splash / reel click | Plays each fishing XP tick |
| `Skill Tick Cooking` | Sizzle / bubble | Plays each cooking XP tick |
| `Skill Tick Smithing` | Anvil clang / hammer tap | Plays each smithing XP tick |
| `Skill Tick Woodcutting` | Axe chop / wood crack | Plays each woodcutting XP tick |
| `Level Up Clip` | Triumphant jingle / fanfare | Plays on any skill level-up |

**Combat Sounds**
| Field | Recommended Sound |
|---|---|
| `Attack Hit Clip` | Sword slash / blunt impact |
| `Special Attack Clip` | Heavy impact / magic burst |
| `Heal Clip` | Soft chime / sparkle |
| `Enemy Death Clip` | Defeat thud / dissolve |
| `Player Death Clip` | Low failure tone / collapse |
| `Combat Start Clip` | Battle horn / drum hit |
| `Wave Clear Clip` | Victory sting |

**UI Sounds**
| Field | Recommended Sound |
|---|---|
| `Button Click Clip` | Soft UI click / tap |
| `Loot Drop Clip` | Coin jingle / item pickup |
| `Item Use Clip` | Potion gulp / consume |

**Music / Ambient**
| Field | Recommended Sound |
|---|---|
| `Ambient Loop` | Medieval/fantasy background loop (optional) |

### 11.3 Wire Up the Mute Toggle

1. In your settings panel or header bar, create a UI element for audio controls:
   - Add a **Toggle** (GameObject → UI → Toggle) for the mute button
   - Optionally add one or two **Sliders** (GameObject → UI → Slider) for SFX and music volume
2. Attach `MuteToggle.cs` to the parent GameObject.
3. In the Inspector, assign:
   - `Mute Toggle` → your UI Toggle component
   - `Unmuted Icon` → a child GameObject with a speaker icon (Image)
   - `Muted Icon` → a child GameObject with a muted-speaker icon (Image)
   - `Sfx Volume Slider` → your SFX volume Slider (optional)
   - `Music Volume Slider` → your music volume Slider (optional)
4. If you prefer a simple Button instead of a Toggle, assign the `Mute Button` field instead. Clicking it will toggle mute on/off.

> **Tip:** The Toggle's `isOn = true` means "sound ON" (unmuted). The component handles the inversion internally.

### 11.4 Calling Audio from Game Code

The `AudioManager` exposes named convenience methods. Other scripts call them like this:

```
AudioManager.Instance.PlayAttack();
AudioManager.Instance.PlaySkillTick("mining");
AudioManager.Instance.PlayLevelUp();
AudioManager.Instance.PlayButtonClick();
```

All methods are safe to call even if the clip is not assigned — they log a debug warning and return gracefully.

#### Full Method Reference

| Method | When to Call |
|---|---|
| `PlaySkillTick(string skillName)` | Each XP tick in IdleManager |
| `PlayLevelUp()` | Skill level-up in IdleManager |
| `PlayAttack()` | Normal attack in CombatManager |
| `PlaySpecialAttack()` | Special attack in CombatManager |
| `PlayHeal()` | Heal action in CombatManager |
| `PlayEnemyDeath()` | Enemy defeated in CombatManager |
| `PlayPlayerDeath()` | Player defeated in CombatManager |
| `PlayCombatStart()` | Combat run begins in CombatManager |
| `PlayWaveClear()` | Wave cleared in CombatManager |
| `PlayLoot()` | Loot drop in CombatManager |
| `PlayButtonClick()` | Tab switch in TabManager, any UI button |
| `PlayItemUse()` | Item consumed in InventoryManager |
| `PlaySFX(AudioClip clip)` | Any custom one-shot sound |

### 11.5 WebGL Audio Limitations

⚠️ **WebGL builds require a user interaction (click, tap, or keypress) before any audio can play.** This is a browser security policy, not a Unity limitation.

The `AudioManager` handles this automatically:
- It listens for the first `Input.anyKeyDown` or `Input.GetMouseButtonDown(0)` in `Update()`.
- Once detected, it sets `userHasInteracted = true` and starts any queued music.
- SFX calls made before the first interaction are silently skipped (no errors).

**What this means in practice:**
- The splash screen / inline view on Reddit will typically provide the first click.
- Background music will begin playing after the user's first interaction.
- No additional code is needed — the system handles the unlock automatically.

### 11.6 Volume & Mute Persistence

Volume and mute settings are saved to `PlayerPrefs` automatically:

| Key | Type | Default |
|---|---|---|
| `RuneRealms_Audio_Muted` | int (0/1) | `0` (unmuted) |
| `RuneRealms_Audio_SfxVolume` | float (0–1) | `0.7` |
| `RuneRealms_Audio_MusicVolume` | float (0–1) | `0.5` |

These persist across browser sessions via IndexedDB (Unity WebGL's PlayerPrefs backend).