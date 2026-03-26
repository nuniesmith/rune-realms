# Rune Realms — Unity Editor Build Guide

> **Single source of truth** for building the Rune Realms Unity project from an empty scene to a deployable WebGL build.
> Every hex color, anchor preset, pixel size, component field, and Inspector value is specified exactly.
> Follow each part in order with the Unity Editor open.

---

## Table of Contents

- [Part 0: Pre-flight Checklist](#part-0-pre-flight-checklist)
- [Part 1: Scene Setup (Core GameObjects)](#part-1-scene-setup-core-gameobjects)
- [Part 2: Tab Panels](#part-2-tab-panels)
- [Part 3: Tab Bar](#part-3-tab-bar)
- [Part 4: Skills Tab](#part-4-skills-tab)
- [Part 5: Arena Tab](#part-5-arena-tab)
- [Part 6: Inventory Tab](#part-6-inventory-tab)
- [Part 7: Leaderboard Tab](#part-7-leaderboard-tab)
- [Part 8: Offline Popup](#part-8-offline-popup)
- [Part 9: Tutorial Overlay](#part-9-tutorial-overlay)
- [Part 10: Create Prefabs](#part-10-create-prefabs)
- [Part 11: Create EnemyData ScriptableObjects](#part-11-create-enemydata-scriptableobjects)
- [Part 12: Wire ALL Manager References](#part-12-wire-all-manager-references)
- [Part 13: Build Settings](#part-13-build-settings)
- [Part 14: WebGL Build (Two-Step Process)](#part-14-webgl-build-two-step-process)
- [Part 15: Color Reference Table](#part-15-color-reference-table)
- [Part 16: Font Reference](#part-16-font-reference)
- [Part 17: Final Checklist](#part-17-final-checklist)

---

## Part 0: Pre-flight Checklist

Before you create any GameObjects, verify the following three things. **Do not proceed until all three pass.**

### 0.1 — Verify all 16 scripts compiled

1. Open the Unity Editor and wait for the compilation spinner (bottom-right) to finish.
2. Open **Window → General → Console** (Ctrl+Shift+C / Cmd+Shift+C).
3. Click "Clear" to dismiss old messages.
4. Confirm there are **zero red error messages**. Warnings (yellow) are acceptable.
5. The 16 scripts that must compile without error:

| # | Path | Namespace.Class |
|---|------|-----------------|
| 1 | `Assets/Scripts/Audio/AudioConstants.cs` | `RuneRealms.Audio.AudioConstants` |
| 2 | `Assets/Scripts/Audio/AudioManager.cs` | `RuneRealms.Audio.AudioManager` |
| 3 | `Assets/Scripts/Audio/MuteToggle.cs` | `RuneRealms.Audio.MuteToggle` |
| 4 | `Assets/Scripts/Combat/CombatManager.cs` | `RuneRealms.Combat.CombatManager` |
| 5 | `Assets/Scripts/Combat/EnemyData.cs` | `RuneRealms.Combat.EnemyData` |
| 6 | `Assets/Scripts/Combat/EnemyDatabase.cs` | `RuneRealms.Combat.EnemyDatabase` |
| 7 | `Assets/Scripts/Core/DevvitBridge.cs` | `RuneRealms.Core.DevvitBridge` |
| 8 | `Assets/Scripts/Core/GameManager.cs` | `RuneRealms.Core.GameManager` |
| 9 | `Assets/Scripts/Data/GameData.cs` | `RuneRealms.Data.*` (multiple types) |
| 10 | `Assets/Scripts/Inventory/InventoryManager.cs` | `RuneRealms.Inventory.InventoryManager` |
| 11 | `Assets/Scripts/Leaderboard/LeaderboardManager.cs` | `RuneRealms.Leaderboard.LeaderboardManager` |
| 12 | `Assets/Scripts/Skills/IdleManager.cs` | `RuneRealms.Skills.IdleManager` |
| 13 | `Assets/Scripts/UI/FloatingText.cs` | `RuneRealms.UI.FloatingText` |
| 14 | `Assets/Scripts/UI/OfflinePopup.cs` | `RuneRealms.UI.OfflinePopup` |
| 15 | `Assets/Scripts/UI/TabManager.cs` | `RuneRealms.UI.TabManager` |
| 16 | `Assets/Scripts/UI/TutorialManager.cs` | `RuneRealms.UI.TutorialManager` |

### 0.2 — Verify Newtonsoft.Json package is installed

1. Open **Window → Package Manager**.
2. In the top-left dropdown, select **In Project**.
3. Look for **Newtonsoft Json** (or `com.unity.nuget.newtonsoft-json`).
4. If missing:
   - Click the **+** button → **Add package by name…**
   - Name: `com.unity.nuget.newtonsoft-json`
   - Click **Add** and wait for import.

### 0.3 — Verify TextMeshPro essentials are imported

1. Open **Window → Package Manager** → verify **TextMeshPro** is listed under **In Project**.
2. If you see a "TMP Importer" dialog pop up at any point, click **Import TMP Essentials**.
3. If the dialog never appeared, try creating a temporary UI → Text - TextMeshPro object. Unity will prompt you to import essentials. Do so, then delete the temporary object.
4. Confirm `Assets/TextMesh Pro/` folder exists in the Project panel.

> ✅ All three checks pass? Proceed to Part 1.

---

## Part 1: Scene Setup (Core GameObjects)

### 1.1 — Create or open MainScene

1. **File → New Scene** (or open `Assets/Scenes/MainScene.unity` if it already exists).
2. **File → Save Scene As…** → navigate to `Assets/Scenes/` → name it `MainScene` → Save.
3. Delete the default **Main Camera** and **Directional Light** from the Hierarchy (the Canvas will handle rendering for this 2D UI-only game).
   - If you need a camera for Canvas rendering, keep one Main Camera with:
     - Clear Flags: **Solid Color**
     - Background: `#1A1410` (R:26, G:20, B:16, A:255)
     - Culling Mask: **Everything**

### 1.2 — Create DevvitBridge

1. **Right-click Hierarchy → Create Empty**.
2. Rename it to exactly: `DevvitBridge` (case-sensitive — JavaScript calls `SendMessage('DevvitBridge', ...)` by this name).
3. Reset Transform: Position (0,0,0), Rotation (0,0,0), Scale (1,1,1).
4. **Add Component → search "DevvitBridge"** → attach `DevvitBridge.cs` (namespace: `RuneRealms.Core`).
5. No Inspector fields need to be set — it's self-configuring as a singleton.

### 1.3 — Create GameManager

1. **Right-click Hierarchy → Create Empty**.
2. Rename to: `GameManager`.
3. Reset Transform.
4. **Add Component → search "GameManager"** → attach `GameManager.cs` (namespace: `RuneRealms.Core`).
5. Inspector fields to set now:
   - **Auto Save Interval**: `30`
6. The remaining fields (Idle Manager, Combat Manager, etc.) will be wired in [Part 12](#part-12-wire-all-manager-references).

### 1.4 — Create AudioManager

1. **Right-click Hierarchy → Create Empty**.
2. Rename to: `AudioManager`.
3. Reset Transform.
4. **Add Component → search "AudioManager"** → attach `AudioManager.cs` (namespace: `RuneRealms.Audio`).
5. Audio sources are created automatically at runtime if not assigned.
6. Audio clip fields are optional — leave them as `None` for now. Assign `.ogg` or `.wav` clips later if you have them.

### 1.5 — Create MainCanvas

1. **Right-click Hierarchy → UI → Canvas**.
2. Rename to: `MainCanvas`.
3. Select `MainCanvas` and configure in the Inspector:

**Canvas component:**
| Field | Value |
|-------|-------|
| Render Mode | **Screen Space - Overlay** |
| Sort Order | `0` |

**Canvas Scaler component** (auto-added with Canvas):
| Field | Value |
|-------|-------|
| UI Scale Mode | **Scale With Screen Size** |
| Reference Resolution | X: `1080`, Y: `1920` |
| Screen Match Mode | **Match Width Or Height** |
| Match | `0.5` |

**Graphic Raycaster component** (auto-added): leave defaults.

4. **Add Component → search "TabManager"** → attach `TabManager.cs` (namespace: `RuneRealms.UI`).
   - Leave all fields empty for now — they'll be wired in Part 3 and Part 12.

5. A child `EventSystem` GameObject is auto-created. Keep it.

> **Hierarchy so far:**
> ```
> DevvitBridge
> GameManager
> AudioManager
> MainCanvas
>   └── EventSystem
> ```

---

## Part 2: Tab Panels

Create **6 panels** directly under `MainCanvas`. Each becomes a major screen or overlay.

### 2.1 — Create the 4 Tab Content Panels

Repeat these steps for each of the 4 tab panels:

| Panel Name | Script to Attach | Script Namespace |
|------------|-----------------|------------------|
| `SkillsTab` | `IdleManager.cs` | `RuneRealms.Skills` |
| `ArenaTab` | `CombatManager.cs` | `RuneRealms.Combat` |
| `InventoryTab` | `InventoryManager.cs` | `RuneRealms.Inventory` |
| `LeaderboardTab` | `LeaderboardManager.cs` | `RuneRealms.Leaderboard` |

**For each panel:**

1. **Right-click `MainCanvas` → UI → Panel**.
2. Rename to the panel name from the table above.
3. Select the panel and configure in the Inspector:

**RectTransform:**
| Field | Value |
|-------|-------|
| Anchor Preset | **stretch-stretch** (hold Alt and click the bottom-right icon in the Anchor Presets grid) |
| Left | `0` |
| Right | `0` |
| Top | `0` |
| Bottom | `80` ← leaves room for the TabBar |

**Image component** (auto-added with Panel):
| Field | Value |
|-------|-------|
| Color | `#2A2218` (R:42, G:34, B:24, A:255) |

4. **Add Component → CanvasGroup**.
   - Leave Alpha at `1`, Interactable ✓, Blocks Raycasts ✓ (TabManager controls these at runtime).

5. **Add Component** → attach the script from the table above (e.g., `IdleManager` on `SkillsTab`).

### 2.2 — Create OfflinePopup Panel

1. **Right-click `MainCanvas` → UI → Panel**.
2. Rename to: `OfflinePopup`.
3. Configure RectTransform:

| Field | Value |
|-------|-------|
| Anchor Preset | **stretch-stretch** |
| Left / Right / Top / Bottom | all `0` |

4. **Image component**:

| Field | Value |
|-------|-------|
| Color | `#000000` (R:0, G:0, B:0, **A:150** ← semi-transparent overlay) |

5. **Add Component → search "OfflinePopup"** → attach `OfflinePopup.cs` (namespace: `RuneRealms.UI`).
6. **Uncheck the checkbox** next to the GameObject name in the Inspector to **disable it by default**.

### 2.3 — Create TutorialOverlay Panel

1. **Right-click `MainCanvas` → UI → Panel**.
2. Rename to: `TutorialOverlay`.
3. Configure RectTransform:

| Field | Value |
|-------|-------|
| Anchor Preset | **stretch-stretch** |
| Left / Right / Top / Bottom | all `0` |

4. **Image component**:

| Field | Value |
|-------|-------|
| Color | `#000000` (R:0, G:0, B:0, **A:180** ← darker semi-transparent overlay) |

5. **Add Component → CanvasGroup**. Leave defaults (Alpha 1, Interactable ✓, Blocks Raycasts ✓ — the script manages these).
6. **Add Component → search "TutorialManager"** → attach `TutorialManager.cs` (namespace: `RuneRealms.UI`).
7. **Uncheck the checkbox** next to the GameObject name to **disable it by default**.

> **Hierarchy so far:**
> ```
> DevvitBridge
> GameManager
> AudioManager
> MainCanvas
>   ├── EventSystem
>   ├── SkillsTab          (Panel + CanvasGroup + IdleManager)
>   ├── ArenaTab           (Panel + CanvasGroup + CombatManager)
>   ├── InventoryTab       (Panel + CanvasGroup + InventoryManager)
>   ├── LeaderboardTab     (Panel + CanvasGroup + LeaderboardManager)
>   ├── OfflinePopup       (Panel + OfflinePopup.cs, DISABLED)
>   └── TutorialOverlay    (Panel + CanvasGroup + TutorialManager, DISABLED)
> ```

---

## Part 3: Tab Bar

### 3.1 — Create TabBar Panel

1. **Right-click `MainCanvas` → UI → Panel**.
2. Rename to: `TabBar`.
3. Move it **below all tab panels** in the Hierarchy (so it renders on top).
4. Configure RectTransform:

| Field | Value |
|-------|-------|
| Anchor Preset | **bottom-stretch** (Alt+click: bottom row, full-width) |
| Left | `0` |
| Right | `0` |
| Height | `80` |
| Pivot | X: `0.5`, Y: `0` |
| Pos Y | `0` |

5. **Image component**:

| Field | Value |
|-------|-------|
| Color | `#1A1410` (R:26, G:20, B:16, A:255) |

6. **Add Component → Horizontal Layout Group**:

| Field | Value |
|-------|-------|
| Padding | Left: `4`, Right: `4`, Top: `4`, Bottom: `4` |
| Spacing | `4` |
| Child Alignment | **Middle Center** |
| Control Child Size | Width: ✓, Height: ✓ |
| Use Child Scale | Width: ☐, Height: ☐ |
| Child Force Expand | Width: ✓, Height: ✓ |

### 3.2 — Create 4 Tab Buttons

Create the following buttons **as children of `TabBar`** in this order:

| Button Name | Label Text |
|-------------|-----------|
| `SkillsTabBtn` | Skills |
| `ArenaTabBtn` | Arena |
| `InventoryTabBtn` | Inventory |
| `LeaderboardTabBtn` | Leaders |

**For each button:**

1. **Right-click `TabBar` → UI → Button - TextMeshPro**.
   - If prompted to import TMP Essentials, click **Import TMP Essentials**.
2. Rename the button to the name from the table above.
3. **Button's Image component** (on the button GameObject itself):

| Field | Value |
|-------|-------|
| Color | `#5C4A2A` (R:92, G:74, B:42, A:255) |

4. **Expand the button** in the Hierarchy to find its child `Text (TMP)`.
5. Select the `Text (TMP)` child and configure:

| Field | Value |
|-------|-------|
| Text | The label from the table (e.g., `Skills`) |
| Font Size | `14` |
| Font Style | **Bold** (click the **B** button) |
| Color | `#B8A88A` (R:184, G:168, B:138, A:255) |
| Alignment | **Center + Middle** (horizontal center, vertical middle) |
| Overflow | **Ellipsis** |

### 3.3 — Wire TabManager on MainCanvas

1. Select `MainCanvas` in the Hierarchy.
2. Find the **Tab Manager** component in the Inspector.
3. Set the **Tabs** array:
   - Size: `4`

| Index | Name | Tab Button | Content Panel | Tab Icon | Active Color | Inactive Color |
|-------|------|-----------|---------------|----------|-------------|----------------|
| 0 | `Skills` | drag `SkillsTabBtn` | drag `SkillsTab` (CanvasGroup) | drag `SkillsTabBtn`'s Image | `#D4A438` (R:212, G:164, B:56, A:255) | `#5C4A2A` (R:92, G:74, B:42, A:255) |
| 1 | `Arena` | drag `ArenaTabBtn` | drag `ArenaTab` (CanvasGroup) | drag `ArenaTabBtn`'s Image | `#D4A438` | `#5C4A2A` |
| 2 | `Inventory` | drag `InventoryTabBtn` | drag `InventoryTab` (CanvasGroup) | drag `InventoryTabBtn`'s Image | `#D4A438` | `#5C4A2A` |
| 3 | `Leaders` | drag `LeaderboardTabBtn` | drag `LeaderboardTab` (CanvasGroup) | drag `LeaderboardTabBtn`'s Image | `#D4A438` | `#5C4A2A` |

> **How to drag CanvasGroup references:**
> The `contentPanel` field expects a `CanvasGroup`. Drag the panel GameObject (e.g., `SkillsTab`) from the Hierarchy into the field — Unity will auto-resolve the `CanvasGroup` component on it.

> **How to drag Tab Icon (Image):**
> The `tabIcon` field expects an `Image`. Drag the button itself (e.g., `SkillsTabBtn`) — Unity will resolve the `Image` component. This is the Image whose color will be toggled between active/inactive colors.

4. Set **Fade Speed**: `8`.

---

## Part 4: Skills Tab

### 4.1 — Skills Title

1. **Right-click `SkillsTab` → UI → Text - TextMeshPro**.
2. Rename to: `SkillsTitle`.
3. Configure RectTransform:

| Field | Value |
|-------|-------|
| Anchor Preset | **top-center** |
| Pivot | X: `0.5`, Y: `1` |
| Pos X | `0` |
| Pos Y | `0` |
| Width | `400` |
| Height | `50` |

4. TMP Settings:

| Field | Value |
|-------|-------|
| Text | `Skills` |
| Font Size | `28` |
| Font Style | **Bold** |
| Color | `#D4A438` (R:212, G:164, B:56, A:255) |
| Alignment | **Center + Middle** |

### 4.2 — Skill Grid (ScrollView)

1. **Right-click `SkillsTab` → UI → Scroll View**.
2. Rename to: `SkillGrid`.
3. Configure RectTransform:

| Field | Value |
|-------|-------|
| Anchor Preset | **stretch-stretch** |
| Left | `0` |
| Right | `0` |
| Top | `70` ← below the title |
| Bottom | `10` |

4. **Scroll Rect component** (on `SkillGrid`):

| Field | Value |
|-------|-------|
| Horizontal | ☐ (unchecked) |
| Vertical | ✓ |
| Movement Type | **Clamped** |
| Scroll Sensitivity | `20` |

5. **Image component** (on `SkillGrid`):

| Field | Value |
|-------|-------|
| Color | `#2A2218` with Alpha `0` (fully transparent — let the panel behind show through) |

6. **Delete the child `Scrollbar Horizontal`** from the Hierarchy (right-click → Delete).
7. You can also delete `Scrollbar Vertical` if you want a clean look, or keep it with the following settings:
   - Image Color on the scrollbar background: `#1A1410` A:100
   - Handle Image Color: `#5C4A2A` A:200

### 4.3 — Configure Viewport → Content

1. Expand `SkillGrid → Viewport → Content` in the Hierarchy.
2. Select `Content`.
3. Configure RectTransform:

| Field | Value |
|-------|-------|
| Anchor Preset | **top-stretch** (top edge, full width) |
| Pivot | X: `0.5`, Y: `1` |
| Left | `0` |
| Right | `0` |
| Pos Y | `0` |

4. **Add Component → Vertical Layout Group**:

| Field | Value |
|-------|-------|
| Padding | Left: `16`, Right: `16`, Top: `8`, Bottom: `8` |
| Spacing | `8` |
| Child Alignment | **Upper Center** |
| Control Child Size | Width: ✓, Height: ✓ |
| Use Child Scale | Width: ☐, Height: ☐ |
| Child Force Expand | Width: ✓, Height: ☐ |

5. **Add Component → Content Size Fitter**:

| Field | Value |
|-------|-------|
| Horizontal Fit | **Unconstrained** |
| Vertical Fit | **Preferred Size** |

### 4.4 — Create 8 Skill Slots

You will create **8 skill slot panels** as children of `Content`. Each has the same structure.

Here is the complete list:

| # | Slot Name | Display Name | Category | XP Per Tick |
|---|-----------|-------------|----------|-------------|
| 0 | `WoodcuttingSlot` | Woodcutting | Gathering | 10 |
| 1 | `FishingSlot` | Fishing | Gathering | 12 |
| 2 | `MiningSlot` | Mining | Gathering | 8 |
| 3 | `CookingSlot` | Cooking | Production | 9 |
| 4 | `SmithingSlot` | Smithing | Production | 11 |
| 5 | `AttackSlot` | Attack | Combat | 7 |
| 6 | `StrengthSlot` | Strength | Combat | 8 |
| 7 | `DefenceSlot` | Defence | Combat | 6 |

**Repeat the following steps for EACH of the 8 slots:**

#### Step A: Create the slot panel

1. **Right-click `Content` (under Viewport) → UI → Panel**.
2. Rename to the **Slot Name** from the table (e.g., `WoodcuttingSlot`).
3. **Add Component → Layout Element**:

| Field | Value |
|-------|-------|
| Min Height | `80` |
| Preferred Height | `80` |
| Flexible Height | `-1` (leave unchecked / default) |

4. **Image component**:

| Field | Value |
|-------|-------|
| Color | `#3A3125` (R:58, G:49, B:37, A:255) |

5. **Add Component → Button**. Leave default settings (this makes the whole slot clickable).

#### Step B: Create SkillIcon child

1. **Right-click the slot (e.g., `WoodcuttingSlot`) → UI → Image**.
2. Rename to: `SkillIcon`.
3. Configure RectTransform:

| Field | Value |
|-------|-------|
| Anchor Preset | **left-center** (middle-left) |
| Pivot | X: `0.5`, Y: `0.5` |
| Pos X | `40` |
| Pos Y | `0` |
| Width | `48` |
| Height | `48` |

4. **Image component**:

| Field | Value |
|-------|-------|
| Color | `#FFFFFF` (white — placeholder until actual icons are assigned) |
| Source Image | `None` (will show as white square) |
| Preserve Aspect | ✓ |

#### Step C: Create SkillName child

1. **Right-click the slot → UI → Text - TextMeshPro**.
2. Rename to: `SkillName`.
3. Configure RectTransform:

| Field | Value |
|-------|-------|
| Anchor Preset | **left-center** (middle-left) |
| Pivot | X: `0`, Y: `0.5` |
| Pos X | `80` |
| Pos Y | `8` |
| Width | `140` |
| Height | `30` |

4. TMP Settings:

| Field | Value |
|-------|-------|
| Text | The **Display Name** from the table (e.g., `Woodcutting`) |
| Font Size | `14` |
| Font Style | **Normal** |
| Color | `#B8A88A` (R:184, G:168, B:138, A:255) |
| Alignment | **Left + Middle** |

#### Step D: Create LevelText child

1. **Right-click the slot → UI → Text - TextMeshPro**.
2. Rename to: `LevelText`.
3. Configure RectTransform:

| Field | Value |
|-------|-------|
| Anchor Preset | **right-center** (middle-right) |
| Pivot | X: `1`, Y: `0.5` |
| Pos X | `-24` |
| Pos Y | `8` |
| Width | `80` |
| Height | `30` |

4. TMP Settings:

| Field | Value |
|-------|-------|
| Text | `Lv.1` |
| Font Size | `18` |
| Font Style | **Bold** |
| Color | `#D4A438` (R:212, G:164, B:56, A:255) |
| Alignment | **Right + Middle** |

#### Step E: Create ProgressBar child (Slider)

1. **Right-click the slot → UI → Slider**.
2. Rename to: `ProgressBar`.
3. Configure RectTransform:

| Field | Value |
|-------|-------|
| Anchor Preset | **bottom-stretch** (bottom edge, full width) |
| Pivot | X: `0.5`, Y: `0` |
| Left | `16` |
| Right | `16` |
| Height | `8` |
| Pos Y | `6` |

4. **Slider component**:

| Field | Value |
|-------|-------|
| Interactable | ☐ (unchecked — display only) |
| Min Value | `0` |
| Max Value | `1` |
| Value | `0` |
| Whole Numbers | ☐ (unchecked) |
| Direction | **Left To Right** |

5. **Delete the child `Handle Slide Area`** from the Hierarchy (right-click → Delete).

6. Select the `Background` child of the Slider:

| Field | Value |
|-------|-------|
| Image Color | `#1A1410` (R:26, G:20, B:16, A:255) |

7. Expand `Fill Area → Fill` child:

| Field | Value |
|-------|-------|
| Image Color | `#00AAFF` (R:0, G:170, B:255, A:255) — XP Blue |

#### Step F: Create ActiveGlow child

1. **Right-click the slot → UI → Image**.
2. Rename to: `ActiveGlow`.
3. Configure RectTransform:

| Field | Value |
|-------|-------|
| Anchor Preset | **stretch-stretch** (fill parent completely) |
| Left / Right / Top / Bottom | all `0` |

4. **Image component**:

| Field | Value |
|-------|-------|
| Color | `#D4A438` with **Alpha 100** (R:212, G:164, B:56, A:100) |
| Raycast Target | ☐ (unchecked — so clicks pass through to the Button) |

5. **Uncheck the GameObject checkbox** to **disable it by default**. The `IdleManager` script enables it at runtime when the skill is active.

#### Final slot Hierarchy (example for WoodcuttingSlot):

```
WoodcuttingSlot   (Panel + Layout Element + Button)
├── SkillIcon     (Image, 48×48, left-center)
├── SkillName     (TMP, "Woodcutting", left-center)
├── LevelText     (TMP, "Lv.1", right-center)
├── ProgressBar   (Slider, bottom-stretch, 8px tall)
│   ├── Background
│   └── Fill Area
│       └── Fill
└── ActiveGlow    (Image, stretch-stretch, DISABLED)
```

> **Repeat Steps A–F for all 8 skills.** The only differences per slot are the **Slot Name** and the **Display Name** text in the `SkillName` TMP.

### 4.5 — Wire IdleManager on SkillsTab

1. Select `SkillsTab` in the Hierarchy.
2. Find the **Idle Manager** component in the Inspector.
3. Set **Tick Interval**: `1` (default).
4. Set **Xp Per Tick** array:
   - Size: `8`
   - Element 0: `10` (Woodcutting)
   - Element 1: `12` (Fishing)
   - Element 2: `8` (Mining)
   - Element 3: `9` (Cooking)
   - Element 4: `11` (Smithing)
   - Element 5: `7` (Attack)
   - Element 6: `8` (Strength)
   - Element 7: `6` (Defence)

5. Set **Skill Slots** array:
   - Size: `8`

| Index | skillName | button | progressBar | levelText | nameText | iconImage | activeIndicator |
|-------|-----------|--------|------------|-----------|----------|-----------|-----------------|
| 0 | `woodcutting` | drag `WoodcuttingSlot` | drag `WoodcuttingSlot/ProgressBar` (Slider) | drag `WoodcuttingSlot/LevelText` | drag `WoodcuttingSlot/SkillName` | drag `WoodcuttingSlot/SkillIcon` | drag `WoodcuttingSlot/ActiveGlow` |
| 1 | `fishing` | drag `FishingSlot` | drag `FishingSlot/ProgressBar` | drag `FishingSlot/LevelText` | drag `FishingSlot/SkillName` | drag `FishingSlot/SkillIcon` | drag `FishingSlot/ActiveGlow` |
| 2 | `mining` | drag `MiningSlot` | drag `MiningSlot/ProgressBar` | drag `MiningSlot/LevelText` | drag `MiningSlot/SkillName` | drag `MiningSlot/SkillIcon` | drag `MiningSlot/ActiveGlow` |
| 3 | `cooking` | drag `CookingSlot` | drag `CookingSlot/ProgressBar` | drag `CookingSlot/LevelText` | drag `CookingSlot/SkillName` | drag `CookingSlot/SkillIcon` | drag `CookingSlot/ActiveGlow` |
| 4 | `smithing` | drag `SmithingSlot` | drag `SmithingSlot/ProgressBar` | drag `SmithingSlot/LevelText` | drag `SmithingSlot/SkillName` | drag `SmithingSlot/SkillIcon` | drag `SmithingSlot/ActiveGlow` |
| 5 | `attack` | drag `AttackSlot` | drag `AttackSlot/ProgressBar` | drag `AttackSlot/LevelText` | drag `AttackSlot/SkillName` | drag `AttackSlot/SkillIcon` | drag `AttackSlot/ActiveGlow` |
| 6 | `strength` | drag `StrengthSlot` | drag `StrengthSlot/ProgressBar` | drag `StrengthSlot/LevelText` | drag `StrengthSlot/SkillName` | drag `StrengthSlot/SkillIcon` | drag `StrengthSlot/ActiveGlow` |
| 7 | `defence` | drag `DefenceSlot` | drag `DefenceSlot/ProgressBar` | drag `DefenceSlot/LevelText` | drag `DefenceSlot/SkillName` | drag `DefenceSlot/SkillIcon` | drag `DefenceSlot/ActiveGlow` |

> **Important:** The `skillName` strings must be **lowercase** and match exactly what the server sends. The `button` field expects a `Button` component — drag the slot panel itself (which has the Button component). The `activeIndicator` expects a `GameObject` — drag the `ActiveGlow` Image child.

---

## Part 5: Arena Tab

### 5.1 — Overall Structure

Build this hierarchy under `ArenaTab`:

```
ArenaTab                          (Panel + CanvasGroup + CombatManager)
├── CombatPanel                   (Panel, enabled)
│   ├── PlayerSection             (Panel)
│   │   ├── PlayerHpBar           (Slider)
│   │   ├── PlayerHpText          (TMP)
│   │   └── PlayerStatsText       (TMP)
│   ├── EnemySection              (Panel)
│   │   ├── EnemyIcon             (Image)
│   │   ├── EnemyNameText         (TMP)
│   │   ├── EnemyHpBar           (Slider)
│   │   └── EnemyHpText          (TMP)
│   ├── CombatInfo                (Panel)
│   │   ├── WaveText              (TMP)
│   │   ├── KillCountText         (TMP)
│   │   ├── LootText              (TMP)
│   │   └── XpRewardText          (TMP)
│   └── ActionButtons             (Panel)
│       ├── AttackBtn             (Button-TMP)
│       ├── SpecialBtn            (Button-TMP)
│       ├── HealBtn               (Button-TMP)
│       └── RunBtn                (Button-TMP)
└── VictoryPanel                  (Panel, DISABLED by default)
    ├── ResultsTitle              (TMP)
    ├── ResultsSummary            (TMP)
    └── StartBattleBtn            (Button-TMP)
```

### 5.2 — CombatPanel

1. **Right-click `ArenaTab` → UI → Panel**.
2. Rename to: `CombatPanel`.
3. RectTransform: **stretch-stretch**, all offsets `0`.
4. Image Color: `#2A2218` A:`0` (transparent — inherits from parent).

### 5.3 — PlayerSection

1. **Right-click `CombatPanel` → UI → Panel**.
2. Rename to: `PlayerSection`.
3. RectTransform:

| Field | Value |
|-------|-------|
| Anchor Preset | **top-stretch** |
| Left | `16` |
| Right | `16` |
| Height | `120` |
| Pos Y | `0` (at top) |

4. Image Color: `#3A3125` A:255.

#### PlayerHpBar

1. **Right-click `PlayerSection` → UI → Slider**.
2. Rename to: `PlayerHpBar`.
3. RectTransform:

| Field | Value |
|-------|-------|
| Anchor Preset | **top-stretch** |
| Left | `16` |
| Right | `16` |
| Height | `20` |
| Pos Y | `-12` |

4. Slider settings:

| Field | Value |
|-------|-------|
| Interactable | ☐ |
| Min Value | `0` |
| Max Value | `100` |
| Value | `50` |
| Whole Numbers | ✓ |

5. Delete `Handle Slide Area`.
6. Background Image Color: `#1A1410`.
7. `Fill Area → Fill` Image Color: `#00CC00` (R:0, G:204, B:0, A:255) — Health Green.

#### PlayerHpText

1. **Right-click `PlayerSection` → UI → Text - TextMeshPro**.
2. Rename to: `PlayerHpText`.
3. RectTransform: Anchor **top-center**, Pos Y: `-14`, Width: `200`, Height: `20`.
4. TMP: Text `50/50`, Size `14`, Color `#FFFFFF`, Alignment **Center + Middle**, Bold: ☐.

#### PlayerStatsText

1. **Right-click `PlayerSection` → UI → Text - TextMeshPro**.
2. Rename to: `PlayerStatsText`.
3. RectTransform: Anchor **top-center**, Pos Y: `-44`, Width: `350`, Height: `25`.
4. TMP: Text `ATK: 10  DEF: 5  SPL: 20`, Size `12`, Color `#B8A88A`, Alignment **Center + Middle**.

### 5.4 — EnemySection

1. **Right-click `CombatPanel` → UI → Panel**.
2. Rename to: `EnemySection`.
3. RectTransform:

| Field | Value |
|-------|-------|
| Anchor Preset | **top-stretch** |
| Left | `16` |
| Right | `16` |
| Height | `160` |
| Pos Y | `-130` (below PlayerSection) |

4. Image Color: `#3A3125` A:255.

#### EnemyIcon

1. **Right-click `EnemySection` → UI → Image**.
2. Rename to: `EnemyIcon`.
3. RectTransform: Anchor **top-center**, Pos Y: `-16`, Width: `64`, Height: `64`.
4. Image Color: `#FFFFFF`. Preserve Aspect: ✓.

#### EnemyNameText

1. **Right-click `EnemySection` → UI → Text - TextMeshPro**.
2. Rename to: `EnemyNameText`.
3. RectTransform: Anchor **top-center**, Pos Y: `-84`, Width: `300`, Height: `28`.
4. TMP: Text `Goblin`, Size `16`, **Bold**, Color `#D4A438`, Alignment **Center + Middle**.

#### EnemyHpBar

1. **Right-click `EnemySection` → UI → Slider**.
2. Rename to: `EnemyHpBar`.
3. RectTransform: Anchor **bottom-stretch**, Left `16`, Right `16`, Height `20`, Pos Y: `30`.
4. Slider: Interactable ☐, Min `0`, Max `100`, Value `30`, Whole Numbers ✓.
5. Delete `Handle Slide Area`.
6. Background Color: `#1A1410`.
7. Fill Color: `#CC0000` (R:204, G:0, B:0, A:255) — Damage Red.

#### EnemyHpText

1. **Right-click `EnemySection` → UI → Text - TextMeshPro**.
2. Rename to: `EnemyHpText`.
3. RectTransform: Anchor **bottom-center**, Pos Y: `32`, Width: `200`, Height: `20`.
4. TMP: Text `30/30`, Size `14`, Color `#FFFFFF`, Alignment **Center + Middle**.

### 5.5 — CombatInfo

1. **Right-click `CombatPanel` → UI → Panel**.
2. Rename to: `CombatInfo`.
3. RectTransform:

| Field | Value |
|-------|-------|
| Anchor Preset | **middle-stretch** |
| Left | `16` |
| Right | `16` |
| Height | `180` |
| Pos Y | `-40` |

4. Image Color: `#2A2218` A:`0` (transparent).

#### WaveText

1. **Right-click `CombatInfo` → UI → Text - TextMeshPro**.
2. Rename to: `WaveText`.
3. RectTransform: Anchor **top-center**, Pos Y: `0`, Width: `300`, Height: `36`.
4. TMP: Text `Wave 1`, Size `20`, **Bold**, Color `#D4A438`, Alignment **Center + Middle**.

#### KillCountText

1. **Right-click `CombatInfo` → UI → Text - TextMeshPro**.
2. Rename to: `KillCountText`.
3. RectTransform: Anchor **top-center**, Pos Y: `-40`, Width: `300`, Height: `24`.
4. TMP: Text `Kills: 0`, Size `14`, Color `#B8A88A`, Alignment **Center + Middle**.

#### LootText

1. **Right-click `CombatInfo` → UI → Text - TextMeshPro**.
2. Rename to: `LootText`.
3. RectTransform: Anchor **top-center**, Pos Y: `-68`, Width: `400`, Height: `24`.
4. TMP: Text `` (empty), Size `14`, Color `#00FF88` (R:0, G:255, B:136, A:255) — Heal Green, Alignment **Center + Middle**.

#### XpRewardText

1. **Right-click `CombatInfo` → UI → Text - TextMeshPro**.
2. Rename to: `XpRewardText`.
3. RectTransform: Anchor **top-center**, Pos Y: `-96`, Width: `400`, Height: `24`.
4. TMP: Text `` (empty), Size `14`, Color `#00AAFF` (R:0, G:170, B:255, A:255) — XP Blue, Alignment **Center + Middle**.

### 5.6 — ActionButtons

1. **Right-click `CombatPanel` → UI → Panel**.
2. Rename to: `ActionButtons`.
3. RectTransform:

| Field | Value |
|-------|-------|
| Anchor Preset | **bottom-stretch** |
| Left | `16` |
| Right | `16` |
| Height | `80` |
| Pos Y | `16` |

4. Image Color: `#2A2218` A:`0` (transparent).
5. **Add Component → Horizontal Layout Group**:

| Field | Value |
|-------|-------|
| Padding | Left: `8`, Right: `8`, Top: `8`, Bottom: `8` |
| Spacing | `8` |
| Child Alignment | **Middle Center** |
| Control Child Size | Width: ✓, Height: ✓ |
| Child Force Expand | Width: ✓, Height: ✓ |

Create 4 buttons as children of `ActionButtons`:

| Button Name | Label Text | Text Color |
|-------------|-----------|------------|
| `AttackBtn` | Attack | `#FFCC00` (R:255, G:204, B:0) |
| `SpecialBtn` | Special | `#FFCC00` |
| `HealBtn` | Heal | `#00FF88` (R:0, G:255, B:136) |
| `RunBtn` | Run | `#CC0000` (R:204, G:0, B:0) |

**For each button:**
1. **Right-click `ActionButtons` → UI → Button - TextMeshPro**.
2. Rename per the table.
3. Button Image Color: `#5C4A2A`.
4. Child `Text (TMP)`: Text per table, Size `14`, **Bold**, Color per table, Alignment **Center + Middle**.

### 5.7 — VictoryPanel

1. **Right-click `ArenaTab` → UI → Panel**.
2. Rename to: `VictoryPanel`.
3. RectTransform: **stretch-stretch**, all offsets `0`.
4. Image Color: `#3A3125` A:255.
5. **Uncheck the checkbox** next to the name to **disable by default**.

#### ResultsTitle

1. **Right-click `VictoryPanel` → UI → Text - TextMeshPro**.
2. Rename to: `ResultsTitle`.
3. RectTransform: Anchor **top-center**, Pos Y: `-60`, Width: `400`, Height: `50`.
4. TMP: Text `Arena`, Size `28`, **Bold**, Color `#D4A438`, Alignment **Center + Middle**.

#### ResultsSummary

1. **Right-click `VictoryPanel` → UI → Text - TextMeshPro**.
2. Rename to: `ResultsSummary`.
3. RectTransform: Anchor **center**, Pos Y: `30`, Width: `500`, Height: `80`.
4. TMP: Text `Defeat monsters to earn loot!`, Size `16`, Color `#B8A88A`, Alignment **Center + Middle**, Enable **Text Wrapping**.

#### StartBattleBtn

1. **Right-click `VictoryPanel` → UI → Button - TextMeshPro**.
2. Rename to: `StartBattleBtn`.
3. RectTransform: Anchor **center**, Pos Y: `-60`, Width: `250`, Height: `60`.
4. Button Image Color: `#3A3125`.
5. **Add Component → Outline** (UnityEngine.UI.Outline):

| Field | Value |
|-------|-------|
| Effect Color | `#D4A438` A:255 |
| Effect Distance | X: `2`, Y: `-2` |

6. Child `Text (TMP)`: Text `Start Battle`, Size `18`, **Bold**, Color `#FFCC00`, Alignment **Center + Middle**.

### 5.8 — Wire CombatManager on ArenaTab

Select `ArenaTab` → find the **Combat Manager** component:

| Inspector Field | Drag From Hierarchy |
|----------------|-------------------|
| Enemy Database | (wire in Part 11 after creating ScriptableObjects) |
| Player Hp Bar | `CombatPanel/PlayerSection/PlayerHpBar` |
| Player Hp Text | `CombatPanel/PlayerSection/PlayerHpText` |
| Player Stats Text | `CombatPanel/PlayerSection/PlayerStatsText` |
| Enemy Hp Bar | `CombatPanel/EnemySection/EnemyHpBar` |
| Enemy Hp Text | `CombatPanel/EnemySection/EnemyHpText` |
| Enemy Name Text | `CombatPanel/EnemySection/EnemyNameText` |
| Enemy Icon | `CombatPanel/EnemySection/EnemyIcon` |
| Attack Button | `CombatPanel/ActionButtons/AttackBtn` |
| Special Button | `CombatPanel/ActionButtons/SpecialBtn` |
| Heal Button | `CombatPanel/ActionButtons/HealBtn` |
| Run Button | `CombatPanel/ActionButtons/RunBtn` |
| Wave Text | `CombatPanel/CombatInfo/WaveText` |
| Kill Count Text | `CombatPanel/CombatInfo/KillCountText` |
| Loot Text | `CombatPanel/CombatInfo/LootText` |
| Xp Reward Text | `CombatPanel/CombatInfo/XpRewardText` |
| Combat Panel | `CombatPanel` (the GameObject) |
| Victory Panel | `VictoryPanel` (the GameObject) |
| Start Battle Button | `VictoryPanel/StartBattleBtn` |

---

## Part 6: Inventory Tab

### 6.1 — Structure

Build this hierarchy under `InventoryTab`:

```
InventoryTab                     (Panel + CanvasGroup + InventoryManager)
├── InventoryTitle               (TMP)
├── SlotCountText                (TMP)
├── SlotGrid                     (ScrollView)
│   └── Viewport
│       └── Content             (Grid Layout Group)
│           └── (slots created at runtime from prefab)
└── ItemDetailPopup              (Panel, DISABLED)
    ├── DetailName               (TMP)
    ├── DetailCategory           (TMP)
    ├── DetailQuantity           (TMP)
    ├── RarityBorder             (Image)
    ├── UseButton                (Button-TMP)
    └── CloseButton              (Button-TMP)
```

### 6.2 — InventoryTitle

1. **Right-click `InventoryTab` → UI → Text - TextMeshPro**.
2. Rename to: `InventoryTitle`.
3. RectTransform: Anchor **top-center**, Pivot (0.5, 1), Pos Y: `0`, Width: `400`, Height: `50`.
4. TMP: Text `Inventory`, Size `28`, **Bold**, Color `#D4A438`, Alignment **Center + Middle**.

### 6.3 — SlotCountText

1. **Right-click `InventoryTab` → UI → Text - TextMeshPro**.
2. Rename to: `SlotCountText`.
3. RectTransform: Anchor **top-center**, Pos Y: `-50`, Width: `200`, Height: `24`.
4. TMP: Text `0/20 slots`, Size `14`, Color `#B8A88A`, Alignment **Center + Middle**.

### 6.4 — SlotGrid (ScrollView)

1. **Right-click `InventoryTab` → UI → Scroll View**.
2. Rename to: `SlotGrid`.
3. RectTransform:

| Field | Value |
|-------|-------|
| Anchor Preset | **stretch-stretch** |
| Top | `80` |
| Bottom | `10` |
| Left | `0` |
| Right | `0` |

4. Scroll Rect: Horizontal ☐, Vertical ✓.
5. Image Color: transparent (`#2A2218` A:`0`).
6. Delete `Scrollbar Horizontal`.

7. Select `SlotGrid → Viewport → Content`:
   - RectTransform: Anchor **top-stretch**, Pivot (0.5, 1), Left `0`, Right `0`.
   - **Add Component → Grid Layout Group**:

| Field | Value |
|-------|-------|
| Padding | Left: `16`, Right: `16`, Top: `16`, Bottom: `16` |
| Cell Size | X: `120`, Y: `120` |
| Spacing | X: `8`, Y: `8` |
| Start Corner | **Upper Left** |
| Start Axis | **Horizontal** |
| Child Alignment | **Upper Center** |
| Constraint | **Fixed Column Count** |
| Constraint Count | `4` |

   - **Add Component → Content Size Fitter**: Vertical Fit: **Preferred Size**.

### 6.5 — ItemDetailPopup

1. **Right-click `InventoryTab` → UI → Panel**.
2. Rename to: `ItemDetailPopup`.
3. RectTransform:

| Field | Value |
|-------|-------|
| Anchor Preset | **center** |
| Width | `400` |
| Height | `350` |

4. Image Color: `#3A3125` A:255.
5. **Uncheck the checkbox** to **disable by default**.

#### RarityBorder

1. **Right-click `ItemDetailPopup` → UI → Image**.
2. Rename to: `RarityBorder`.
3. RectTransform: **stretch-stretch**, all offsets `0`.
4. Image Color: `#999999` A:100 (grey default, changed at runtime per rarity).
5. **Raycast Target**: ☐ (so clicks pass through to the buttons below).
6. **Move `RarityBorder` to be the FIRST child** of `ItemDetailPopup` in the Hierarchy (so it renders behind).

#### DetailName

1. **Right-click `ItemDetailPopup` → UI → Text - TextMeshPro**.
2. Rename to: `DetailName`.
3. RectTransform: Anchor **top-center**, Pos Y: `-30`, Width: `350`, Height: `36`.
4. TMP: Text `Item Name`, Size `20`, **Bold**, Color `#D4A438`, Alignment **Center + Middle**.

#### DetailCategory

1. **Right-click `ItemDetailPopup` → UI → Text - TextMeshPro**.
2. Rename to: `DetailCategory`.
3. RectTransform: Anchor **top-center**, Pos Y: `-70`, Width: `350`, Height: `24`.
4. TMP: Text `Category`, Size `14`, Color `#B8A88A`, Alignment **Center + Middle**.

#### DetailQuantity

1. **Right-click `ItemDetailPopup` → UI → Text - TextMeshPro**.
2. Rename to: `DetailQuantity`.
3. RectTransform: Anchor **top-center**, Pos Y: `-100`, Width: `350`, Height: `24`.
4. TMP: Text `Quantity: 1`, Size `14`, Color `#B8A88A`, Alignment **Center + Middle**.

#### UseButton

1. **Right-click `ItemDetailPopup` → UI → Button - TextMeshPro**.
2. Rename to: `UseButton`.
3. RectTransform: Anchor **bottom-center**, Pos X: `-80`, Pos Y: `40`, Width: `140`, Height: `50`.
4. Button Image Color: `#00CC00` (Health Green).
5. Child `Text (TMP)`: Text `Use`, Size `16`, **Bold**, Color `#FFFFFF`, Alignment **Center + Middle**.

#### CloseButton

1. **Right-click `ItemDetailPopup` → UI → Button - TextMeshPro**.
2. Rename to: `CloseButton`.
3. RectTransform: Anchor **bottom-center**, Pos X: `80`, Pos Y: `40`, Width: `140`, Height: `50`.
4. Button Image Color: `#CC0000` (Damage Red).
5. Child `Text (TMP)`: Text `Close`, Size `16`, **Bold**, Color `#FFFFFF`, Alignment **Center + Middle**.

---

## Part 7: Leaderboard Tab

### 7.1 — Structure

```
LeaderboardTab                   (Panel + CanvasGroup + LeaderboardManager)
├── LeaderboardTitle             (TMP)
├── TypeButtons                  (Panel + Horizontal Layout)
│   ├── TotalLevelBtn            (Button-TMP)
│   ├── TotalXpBtn               (Button-TMP)
│   └── ArenaKillsBtn            (Button-TMP)
├── EntryList                    (ScrollView)
│   └── Viewport
│       └── Content             (Vertical Layout Group)
│           └── (entries created at runtime from prefab)
└── PlayerRankText               (TMP)
```

### 7.2 — LeaderboardTitle

1. **Right-click `LeaderboardTab` → UI → Text - TextMeshPro**.
2. Rename to: `LeaderboardTitle`.
3. RectTransform: Anchor **top-center**, Pivot (0.5, 1), Pos Y: `0`, Width: `400`, Height: `50`.
4. TMP: Text `Leaderboard`, Size `28`, **Bold**, Color `#D4A438`, Alignment **Center + Middle**.

### 7.3 — TypeButtons

1. **Right-click `LeaderboardTab` → UI → Panel**.
2. Rename to: `TypeButtons`.
3. RectTransform:

| Field | Value |
|-------|-------|
| Anchor Preset | **top-stretch** |
| Left | `16` |
| Right | `16` |
| Height | `50` |
| Pos Y | `-55` |

4. Image Color: `#2A2218` A:`0` (transparent).
5. **Add Component → Horizontal Layout Group**:

| Field | Value |
|-------|-------|
| Padding | Left: `4`, Right: `4`, Top: `4`, Bottom: `4` |
| Spacing | `8` |
| Child Alignment | **Middle Center** |
| Control Child Size | Width: ✓, Height: ✓ |
| Child Force Expand | Width: ✓, Height: ✓ |

Create 3 buttons:

| Button Name | Label Text |
|-------------|-----------|
| `TotalLevelBtn` | Total Level |
| `TotalXpBtn` | Total XP |
| `ArenaKillsBtn` | Arena Kills |

For each:
1. **Right-click `TypeButtons` → UI → Button - TextMeshPro**.
2. Rename per table.
3. Button Image Color: `#5C4A2A`.
4. Child TMP: Text per table, Size `13`, **Bold**, Color `#B8A88A`, Alignment **Center + Middle**.

### 7.4 — EntryList (ScrollView)

1. **Right-click `LeaderboardTab` → UI → Scroll View**.
2. Rename to: `EntryList`.
3. RectTransform:

| Field | Value |
|-------|-------|
| Anchor Preset | **stretch-stretch** |
| Top | `115` |
| Bottom | `50` |
| Left | `16` |
| Right | `16` |

4. Scroll Rect: Horizontal ☐, Vertical ✓.
5. Image Color: transparent.
6. Delete `Scrollbar Horizontal`.

7. Select `EntryList → Viewport → Content`:
   - RectTransform: Anchor **top-stretch**, Pivot (0.5, 1), Left `0`, Right `0`.
   - **Add Component → Vertical Layout Group**:

| Field | Value |
|-------|-------|
| Padding | Left: `4`, Right: `4`, Top: `4`, Bottom: `4` |
| Spacing | `4` |
| Child Alignment | **Upper Center** |
| Control Child Size | Width: ✓, Height: ✓ |
| Child Force Expand | Width: ✓, Height: ☐ |

   - **Add Component → Content Size Fitter**: Vertical Fit: **Preferred Size**.

### 7.5 — PlayerRankText

1. **Right-click `LeaderboardTab` → UI → Text - TextMeshPro**.
2. Rename to: `PlayerRankText`.
3. RectTransform: Anchor **bottom-center**, Pivot (0.5, 0), Pos Y: `10`, Width: `400`, Height: `36`.
4. TMP: Text `Your Rank: Unranked`, Size `16`, **Bold**, Color `#D4A438`, Alignment **Center + Middle**.

### 7.6 — Wire LeaderboardManager

Select `LeaderboardTab` → find the **Leaderboard Manager** component:

| Inspector Field | Drag From Hierarchy |
|----------------|-------------------|
| Total Level Button | `TypeButtons/TotalLevelBtn` |
| Total Xp Button | `TypeButtons/TotalXpBtn` |
| Arena Kills Button | `TypeButtons/ArenaKillsBtn` |
| Entry Container | `EntryList/Viewport/Content` (the Transform) |
| Entry Prefab | (wire in Part 10 after creating the prefab) |
| Player Rank Text | `PlayerRankText` |

---

## Part 8: Offline Popup

The `OfflinePopup` panel was already created in [Part 2.2](#22--create-offlinepopup-panel). Now add its internal UI.

### 8.1 — Structure

```
OfflinePopup                     (Panel + OfflinePopup.cs, DISABLED)
└── PopupCard                    (Panel)
    ├── TitleText                (TMP)
    ├── GainsText                (TMP)
    └── CollectButton            (Button-TMP)
```

### 8.2 — PopupCard

1. **Enable `OfflinePopup` temporarily** (check the checkbox) so you can edit its children.
2. **Right-click `OfflinePopup` → UI → Panel**.
3. Rename to: `PopupCard`.
4. RectTransform:

| Field | Value |
|-------|-------|
| Anchor Preset | **center** |
| Width | `500` |
| Height | `350` |

5. Image Color: `#3A3125` A:255.
6. **Add Component → Outline** (UnityEngine.UI.Outline):

| Field | Value |
|-------|-------|
| Effect Color | `#D4A438` A:255 |
| Effect Distance | X: `2`, Y: `-2` |

### 8.3 — TitleText

1. **Right-click `PopupCard` → UI → Text - TextMeshPro**.
2. Rename to: `TitleText`.
3. RectTransform: Anchor **top-center**, Pos Y: `-30`, Width: `450`, Height: `40`.
4. TMP: Text `While you were away...`, Size `22`, **Bold**, Color `#D4A438`, Alignment **Center + Middle**.

### 8.4 — GainsText

1. **Right-click `PopupCard` → UI → Text - TextMeshPro**.
2. Rename to: `GainsText`.
3. RectTransform: Anchor **center**, Pos Y: `10`, Width: `450`, Height: `180`.
4. TMP: Text `` (empty — populated at runtime), Size `16`, Color `#B8A88A`, Alignment **Center + Top**, Enable **Text Wrapping**.

### 8.5 — CollectButton

1. **Right-click `PopupCard` → UI → Button - TextMeshPro**.
2. Rename to: `CollectButton`.
3. RectTransform: Anchor **bottom-center**, Pos Y: `30`, Width: `200`, Height: `50`.
4. Button Image Color: `#D4A438`.
5. Child `Text (TMP)`: Text `Collect!`, Size `18`, **Bold**, Color `#1A1410` (dark on gold), Alignment **Center + Middle**.

### 8.6 — Wire OfflinePopup.cs

Select `OfflinePopup` → find the **Offline Popup** component:

| Inspector Field | Drag From Hierarchy |
|----------------|-------------------|
| Popup Panel | `PopupCard` (the GameObject) |
| Title Text | `PopupCard/TitleText` |
| Gains Text | `PopupCard/GainsText` |
| Collect Button | `PopupCard/CollectButton` |

### 8.7 — Re-disable OfflinePopup

**Uncheck the checkbox** next to `OfflinePopup` in the Inspector to disable it again.

---

## Part 9: Tutorial Overlay

The `TutorialOverlay` panel was already created in [Part 2.3](#23--create-tutorialoverlay-panel). Now add its internal UI.

### 9.1 — Structure

```
TutorialOverlay                  (Panel + CanvasGroup + TutorialManager, DISABLED)
└── DialogPanel                  (Panel)
    ├── TitleLabel               (Text or TMP — see note below)
    ├── DescriptionLabel         (Text or TMP)
    ├── StepCounter              (Text or TMP)
    ├── NextButton               (Button)
    │   └── NextButtonLabel      (Text or TMP)
    └── SkipButton               (Button)
        └── SkipButtonLabel      (Text)
```

### 9.2 — IMPORTANT: Text Type Decision

The `TutorialManager.cs` script uses **`UnityEngine.UI.Text`** (Legacy Text), NOT TextMeshPro. You have two options:

#### Option A: Use Legacy Text (matches the script as-is)

Use **Right-click → UI → Legacy → Text** for all labels in this section. This is the quickest path.

#### Option B: Update the script to use TMP

If you prefer TextMeshPro everywhere, modify `TutorialManager.cs`:

1. Add `using TMPro;` at the top of the file.
2. Replace every `Text` field type with `TMP_Text`:
   - `[SerializeField] private Text titleLabel;` → `[SerializeField] private TMP_Text titleLabel;`
   - `[SerializeField] private Text descriptionLabel;` → `[SerializeField] private TMP_Text descriptionLabel;`
   - `[SerializeField] private Text stepCounterLabel;` → `[SerializeField] private TMP_Text stepCounterLabel;`
   - `[SerializeField] private Text nextButtonLabel;` → `[SerializeField] private TMP_Text nextButtonLabel;`
3. Remove the `using UnityEngine.UI;` import for `Text` (keep it for `Button`, `Image` etc.).
4. Save, let Unity recompile, then use TMP objects below.

> **This guide documents both.** Instructions marked **(Legacy)** use `UI → Legacy → Text`. Instructions marked **(TMP)** use `UI → Text - TextMeshPro`. Pick one approach and be consistent.

### 9.3 — DialogPanel

1. **Enable `TutorialOverlay` temporarily** (check the checkbox).
2. **Right-click `TutorialOverlay` → UI → Panel**.
3. Rename to: `DialogPanel`.
4. RectTransform:

| Field | Value |
|-------|-------|
| Anchor Preset | **center** |
| Width | `600` |
| Height | `400` |

5. Image Color: `#3A3125` A:255.
6. **Add Component → Outline**:

| Field | Value |
|-------|-------|
| Effect Color | `#D4A438` A:255 |
| Effect Distance | X: `2`, Y: `-2` |

### 9.4 — TitleLabel

**(Legacy):** Right-click `DialogPanel` → UI → Legacy → Text.
**(TMP):** Right-click `DialogPanel` → UI → Text - TextMeshPro.

1. Rename to: `TitleLabel`.
2. RectTransform: Anchor **top-center**, Pos Y: `-30`, Width: `500`, Height: `40`.
3. Text: `Welcome to Rune Realms!`, Size `24`, **Bold**, Color `#D4A438`, Alignment: **Center**.

### 9.5 — DescriptionLabel

**(Legacy):** Right-click `DialogPanel` → UI → Legacy → Text.
**(TMP):** Right-click `DialogPanel` → UI → Text - TextMeshPro.

1. Rename to: `DescriptionLabel`.
2. RectTransform: Anchor **center**, Pos Y: `10`, Width: `540`, Height: `200`.
3. Text: (placeholder text), Size `16`, Color `#B8A88A`, Alignment: **Center**, Word Wrap / Text Wrapping: ✓.
4. **(Legacy)** Vertical Overflow: **Overflow** or **Truncate**.

### 9.6 — StepCounter

**(Legacy):** Right-click `DialogPanel` → UI → Legacy → Text.
**(TMP):** Right-click `DialogPanel` → UI → Text - TextMeshPro.

1. Rename to: `StepCounter`.
2. RectTransform: Anchor **top-right**, Pos X: `-20`, Pos Y: `-12`, Width: `60`, Height: `24`.
3. Text: `1/7`, Size `12`, Color `#5C4A2A`, Alignment: **Right**.

### 9.7 — NextButton

1. **Right-click `DialogPanel` → UI → Button** (Legacy Button, not Button-TMP — TutorialManager expects `UnityEngine.UI.Button` which both types satisfy; the label type matters more).
   - If using **Option A (Legacy)**: Right-click → UI → Legacy → Button. This auto-creates a child `Text` (Legacy).
   - If using **Option B (TMP)**: Right-click → UI → Button - TextMeshPro. This auto-creates a child `Text (TMP)`.
2. Rename button to: `NextButton`.
3. RectTransform: Anchor **bottom-right**, Pos X: `-60`, Pos Y: `30`, Width: `120`, Height: `45`.
4. Button Image Color: `#D4A438`.
5. Rename the child text to: `NextButtonLabel`.
6. Text: `Next`, Size `16`, **Bold**, Color `#1A1410`, Alignment: **Center**.

### 9.8 — SkipButton

1. Create another button the same way as NextButton.
2. Rename to: `SkipButton`.
3. RectTransform: Anchor **bottom-left**, Pos X: `60`, Pos Y: `30`, Width: `100`, Height: `40`.
4. Button Image Color: `#5C4A2A`.
5. Child text: `Skip`, Size `14`, Color `#B8A88A`, Alignment: **Center**.

### 9.9 — Wire TutorialManager

Select `TutorialOverlay` → find the **Tutorial Manager** component:

| Inspector Field | Drag From Hierarchy |
|----------------|-------------------|
| Overlay Canvas Group | `TutorialOverlay` (itself — the CanvasGroup on this GO) |
| Dark Overlay | `TutorialOverlay` (itself — the Image component) |
| Dialog Panel | `DialogPanel` (RectTransform) |
| Title Label | `DialogPanel/TitleLabel` |
| Description Label | `DialogPanel/DescriptionLabel` |
| Step Counter Label | `DialogPanel/StepCounter` |
| Next Button | `DialogPanel/NextButton` |
| Next Button Label | `DialogPanel/NextButton/NextButtonLabel` |
| Skip Button | `DialogPanel/SkipButton` |
| Highlight Frame | `None` (leave empty unless you create a highlight rect) |
| Fade Duration | `0.3` |

The **Steps** list auto-populates with 7 default tutorial steps at runtime if left empty. You can also manually populate them in the Inspector.

### 9.10 — Re-disable TutorialOverlay

**Uncheck the checkbox** next to `TutorialOverlay` to disable it again.

---

## Part 10: Create Prefabs

### 10.1 — Create the Prefabs folder

If `Assets/Prefabs/` doesn't exist yet, create it:
- Right-click in the Project panel → **Create → Folder** → name it `Prefabs`.

### 10.2 — InventorySlot Prefab

**Build it in the scene first, then drag to Assets/Prefabs/.**

1. **Right-click `MainCanvas` → UI → Panel** (create it temporarily under MainCanvas, NOT inside InventoryTab).
2. Rename to: `InventorySlot`.
3. RectTransform: Width `120`, Height `120` (the Grid Layout will override this, but set it for the prefab).
4. Image Color: `#3A3125` A:255.
5. **Add Component → Button**. Leave defaults.

#### ItemName child

1. **Right-click `InventorySlot` → UI → Text - TextMeshPro**.
2. Rename to: `ItemName` (the script uses `GetComponentInChildren<TextMeshProUGUI>()` to find this).
3. RectTransform: Anchor **center** (stretch not needed), Pos X: `0`, Pos Y: `5`, Width: `110`, Height: `60`.
4. TMP: Text `` (empty), Size `11`, Color `#B8A88A`, Alignment **Center + Middle**, Enable **Text Wrapping**.

#### Quantity child

1. **Right-click `InventorySlot` → UI → Text - TextMeshPro**.
2. Rename to: **exactly** `Quantity` (the script finds this by name via `transform.Find("Quantity")`).
3. RectTransform: Anchor **bottom-right**, Pos X: `-8`, Pos Y: `8`, Width: `50`, Height: `20`.
4. TMP: Text `x1`, Size `10`, Color `#FFFFFF`, Alignment **Right + Bottom**.

#### Save as Prefab

1. **Drag `InventorySlot` from the Hierarchy** into the `Assets/Prefabs/` folder in the Project panel.
2. A prefab file appears. Confirm it's blue in the Hierarchy.
3. **Delete `InventorySlot` from the Hierarchy** (right-click → Delete). The prefab in `Assets/Prefabs/` persists.

### 10.3 — LeaderboardEntry Prefab

**Build it in the scene first, then drag to Assets/Prefabs/.**

1. **Right-click `MainCanvas` → UI → Panel**.
2. Rename to: `LeaderboardEntry`.
3. RectTransform: Anchor **top-stretch** (stretch width), Height `40`.
4. Image Color: `#3A3125` A:255.
5. **Add Component → Layout Element**: Min Height `40`, Preferred Height `40`.
6. **Add Component → Horizontal Layout Group**:

| Field | Value |
|-------|-------|
| Padding | Left: `8`, Right: `8`, Top: `4`, Bottom: `4` |
| Spacing | `8` |
| Child Alignment | **Middle Left** |
| Control Child Size | Width: ☐, Height: ✓ |
| Child Force Expand | Width: ☐, Height: ✓ |

#### RankText child

1. **Right-click `LeaderboardEntry` → UI → Text - TextMeshPro**.
2. Rename to: `RankText`.
3. **Add Component → Layout Element**: Min Width `50`, Preferred Width `50`.
4. TMP: Text `#1`, Size `16`, **Bold**, Color `#D4A438`, Alignment **Left + Middle**.

> **Important:** The `LeaderboardManager` script uses `GetComponentsInChildren<TextMeshProUGUI>()` and expects 3 TMP children in order: `[0]` = Rank, `[1]` = Username, `[2]` = Score. So the **Hierarchy order matters**.

#### UsernameText child

1. **Right-click `LeaderboardEntry` → UI → Text - TextMeshPro**.
2. Rename to: `UsernameText`.
3. **Add Component → Layout Element**: Flexible Width `1` (this makes it fill remaining space).
4. TMP: Text `Player`, Size `14`, Color `#B8A88A`, Alignment **Left + Middle**.

#### ScoreText child

1. **Right-click `LeaderboardEntry` → UI → Text - TextMeshPro**.
2. Rename to: `ScoreText`.
3. **Add Component → Layout Element**: Min Width `80`, Preferred Width `80`.
4. TMP: Text `0`, Size `14`, Color `#B8A88A`, Alignment **Right + Middle**.

#### Save as Prefab

1. **Drag `LeaderboardEntry` from the Hierarchy** into `Assets/Prefabs/`.
2. **Delete `LeaderboardEntry` from the Hierarchy**.

### 10.4 — Wire Prefabs to Managers

Now that prefabs exist:

1. Select `InventoryTab` → **Inventory Manager** component:
   - **Slot Prefab**: drag `Assets/Prefabs/InventorySlot` from the Project panel.

2. Select `LeaderboardTab` → **Leaderboard Manager** component:
   - **Entry Prefab**: drag `Assets/Prefabs/LeaderboardEntry` from the Project panel.

---

## Part 11: Create EnemyData ScriptableObjects

### 11.1 — Create the ScriptableObjects folder

If `Assets/ScriptableObjects/` doesn't already exist, create it. It should already exist based on the project structure.

### 11.2 — Create 8 EnemyData assets

For each enemy, do:

1. Right-click in `Assets/ScriptableObjects/` → **Create → Rune Realms → Enemy Data**.
2. Name the asset per the table below.
3. Fill in the Inspector fields using the values from `Assets/Data/EnemyDefinitions.json`:

| Asset Name | Enemy Name | Difficulty | Min Wave | Max HP | Damage | Attack Interval | XP Reward | Special Ability |
|-----------|-----------|-----------|---------|--------|--------|----------------|-----------|----------------|
| `Chicken` | Chicken | Easy | 1 | 15 | 2 | 2.5 | 5 | _(empty)_ |
| `Goblin` | Goblin | Easy | 1 | 30 | 5 | 2.0 | 15 | _(empty)_ |
| `Skeleton` | Skeleton | Medium | 3 | 50 | 8 | 1.8 | 30 | _(empty)_ |
| `DarkWizard` | Dark Wizard | Medium | 3 | 40 | 12 | 2.5 | 35 | `Casts firebolt every 3rd attack, dealing double damage.` |
| `GiantSpider` | Giant Spider | Medium | 3 | 45 | 7 | 1.5 | 25 | `Fast attack speed. Venom has a 15% chance to apply poison (2 damage per tick for 3 ticks).` |
| `HillGiant` | Hill Giant | Hard | 5 | 80 | 10 | 2.2 | 50 | `Ground slam every 4th attack hits through defense.` |
| `BlackKnight` | Black Knight | Hard | 5 | 100 | 14 | 1.6 | 65 | `Shield bash every 5th attack stuns the player for 1 second.` |
| `Dragon` | Dragon | Boss | 8 | 200 | 20 | 2.8 | 150 | `Dragonfire breath every 3rd attack deals area damage and ignores 50% of defense.` |

For each asset, also populate the **Loot Table** list. Refer to `Assets/Data/EnemyDefinitions.json` for the exact loot entries. Example for Goblin:

**Goblin Loot Table** (Size: 2):
| # | Item Id | Item Name | Rarity | Drop Chance | Min Qty | Max Qty |
|---|---------|-----------|--------|------------|---------|---------|
| 0 | `goblin_bones` | Goblin Bones | common | 0.80 | 1 | 2 |
| 1 | `bronze_dagger` | Bronze Dagger | common | 0.15 | 1 | 1 |

**Chicken Loot Table** (Size: 3):
| # | Item Id | Item Name | Rarity | Drop Chance | Min Qty | Max Qty |
|---|---------|-----------|--------|------------|---------|---------|
| 0 | `feather` | Feather | common | 1.00 | 1 | 5 |
| 1 | `raw_chicken` | Raw Chicken | common | 0.80 | 1 | 1 |
| 2 | `egg` | Egg | uncommon | 0.10 | 1 | 1 |

**Skeleton Loot Table** (Size: 3):
| # | Item Id | Item Name | Rarity | Drop Chance | Min Qty | Max Qty |
|---|---------|-----------|--------|------------|---------|---------|
| 0 | `bones` | Bones | common | 0.90 | 1 | 3 |
| 1 | `iron_sword` | Iron Sword | uncommon | 0.20 | 1 | 1 |
| 2 | `shield_half` | Shield Half | rare | 0.05 | 1 | 1 |

**Dark Wizard Loot Table** (Size: 3):
| # | Item Id | Item Name | Rarity | Drop Chance | Min Qty | Max Qty |
|---|---------|-----------|--------|------------|---------|---------|
| 0 | `air_rune` | Air Rune | common | 0.70 | 5 | 15 |
| 1 | `wizard_hat` | Wizard Hat | uncommon | 0.15 | 1 | 1 |
| 2 | `staff_of_fire` | Staff of Fire | rare | 0.03 | 1 | 1 |

**Giant Spider Loot Table** (Size: 2):
| # | Item Id | Item Name | Rarity | Drop Chance | Min Qty | Max Qty |
|---|---------|-----------|--------|------------|---------|---------|
| 0 | `spider_silk` | Spider Silk | common | 0.75 | 1 | 3 |
| 1 | `venom_sac` | Venom Sac | uncommon | 0.20 | 1 | 1 |

**Hill Giant Loot Table** (Size: 4):
| # | Item Id | Item Name | Rarity | Drop Chance | Min Qty | Max Qty |
|---|---------|-----------|--------|------------|---------|---------|
| 0 | `big_bones` | Big Bones | common | 1.00 | 1 | 2 |
| 1 | `giant_key` | Giant Key | rare | 0.08 | 1 | 1 |
| 2 | `rune_helm` | Rune Helm | rare | 0.05 | 1 | 1 |
| 3 | `limpwurt_root` | Limpwurt Root | uncommon | 0.30 | 1 | 2 |

**Black Knight Loot Table** (Size: 3):
| # | Item Id | Item Name | Rarity | Drop Chance | Min Qty | Max Qty |
|---|---------|-----------|--------|------------|---------|---------|
| 0 | `black_platebody` | Black Platebody | uncommon | 0.25 | 1 | 1 |
| 1 | `black_sword` | Black Sword | uncommon | 0.20 | 1 | 1 |
| 2 | `knights_shield` | Knight's Shield | rare | 0.08 | 1 | 1 |

**Dragon Loot Table** (Size: 5):
| # | Item Id | Item Name | Rarity | Drop Chance | Min Qty | Max Qty |
|---|---------|-----------|--------|------------|---------|---------|
| 0 | `dragon_bones` | Dragon Bones | uncommon | 1.00 | 1 | 3 |
| 1 | `dragon_hide` | Dragon Hide | uncommon | 0.60 | 1 | 2 |
| 2 | `dragon_scimitar` | Dragon Scimitar | rare | 0.10 | 1 | 1 |
| 3 | `dragon_visage` | Dragon Visage | legendary | 0.01 | 1 | 1 |
| 4 | `dragonfire_shield` | Dragonfire Shield | legendary | 0.005 | 1 | 1 |

> **Icon field:** Leave as `None` for all enemies unless you have sprite assets. The script handles null icons gracefully.

### 11.3 — Create EnemyDatabase asset

1. Right-click in `Assets/ScriptableObjects/` → **Create → Rune Realms → Enemy Database**.
2. Name it: `EnemyDatabase`.
3. Select it → in the Inspector, set the **Enemies** list:
   - Size: `8`
   - Drag each of the 8 EnemyData assets from `Assets/ScriptableObjects/` into the list **in this order:**
     1. `Chicken`
     2. `Goblin`
     3. `Skeleton`
     4. `DarkWizard`
     5. `GiantSpider`
     6. `HillGiant`
     7. `BlackKnight`
     8. `Dragon`

### 11.4 — Wire to CombatManager

1. Select `ArenaTab` in the Hierarchy.
2. In the **Combat Manager** component, find the **Enemy Database** field.
3. Drag `Assets/ScriptableObjects/EnemyDatabase` from the Project panel into this field.
4. The **Enemy Types** list (legacy fallback) can be left empty since the database takes priority.

---

## Part 12: Wire ALL Manager References

This is the most critical step. Every cross-reference must be correct.

### 12.1 — GameManager Inspector

Select `GameManager` in the Hierarchy:

| Field | Target | How to Wire |
|-------|--------|------------|
| **Idle Manager** | `SkillsTab` | Drag `SkillsTab` from Hierarchy (resolves `IdleManager` component) |
| **Combat Manager** | `ArenaTab` | Drag `ArenaTab` from Hierarchy (resolves `CombatManager` component) |
| **Inventory Manager** | `InventoryTab` | Drag `InventoryTab` (resolves `InventoryManager` component) |
| **Leaderboard Manager** | `LeaderboardTab` | Drag `LeaderboardTab` (resolves `LeaderboardManager` component) |
| **Tab Manager** | `MainCanvas` | Drag `MainCanvas` (resolves `TabManager` component) |
| **Offline Popup** | `OfflinePopup` | Drag `OfflinePopup` (resolves `OfflinePopup` component) |
| **Tutorial Manager** | `TutorialOverlay` | Drag `TutorialOverlay` (resolves `TutorialManager` component) |
| **Auto Save Interval** | `30` | Type the number |

### 12.2 — InventoryManager on InventoryTab

Select `InventoryTab`:

| Field | Target |
|-------|--------|
| **Slot Container** | `InventoryTab/SlotGrid/Viewport/Content` (the Transform) |
| **Slot Prefab** | `Assets/Prefabs/InventorySlot` (drag from Project panel) |
| **Slot Count Text** | `InventoryTab/SlotCountText` |
| **Detail Popup** | `InventoryTab/ItemDetailPopup` (the GameObject) |
| **Detail Name** | `ItemDetailPopup/DetailName` |
| **Detail Category** | `ItemDetailPopup/DetailCategory` |
| **Detail Quantity** | `ItemDetailPopup/DetailQuantity` |
| **Detail Rarity Border** | `ItemDetailPopup/RarityBorder` (the Image) |
| **Use Button** | `ItemDetailPopup/UseButton` |
| **Close Popup Button** | `ItemDetailPopup/CloseButton` |

Rarity Colors (defaults should be fine, but verify):
| Field | Color |
|-------|-------|
| Common Color | R:0.6, G:0.6, B:0.6, A:1 (`#999999`) |
| Uncommon Color | R:0.2, G:0.8, B:0.2, A:1 (`#33CC33`) |
| Rare Color | R:0.2, G:0.4, B:1.0, A:1 (`#3366FF`) |
| Legendary Color | R:1.0, G:0.6, B:0.0, A:1 (`#FF9900`) |

### 12.3 — LeaderboardManager on LeaderboardTab

Select `LeaderboardTab`:

| Field | Target |
|-------|--------|
| **Total Level Button** | `TypeButtons/TotalLevelBtn` |
| **Total Xp Button** | `TypeButtons/TotalXpBtn` |
| **Arena Kills Button** | `TypeButtons/ArenaKillsBtn` |
| **Entry Container** | `EntryList/Viewport/Content` (the Transform) |
| **Entry Prefab** | `Assets/Prefabs/LeaderboardEntry` (drag from Project panel) |
| **Player Rank Text** | `PlayerRankText` |

Leaderboard Colors (defaults should be fine):
| Field | Color |
|-------|-------|
| Gold Color | R:1.0, G:0.84, B:0.0, A:1 (`#FFD700`) |
| Silver Color | R:0.75, G:0.75, B:0.75, A:1 (`#BFBFBF`) |
| Bronze Color | R:0.80, G:0.50, B:0.20, A:1 (`#CC8033`) |
| Normal Color | R:0.72, G:0.66, B:0.54, A:1 (`#B8A88A`) |

### 12.4 — CombatManager on ArenaTab

Should already be wired from Part 5.8. Verify all fields are assigned (none show `None (Object)` or `Missing`).

### 12.5 — OfflinePopup on OfflinePopup

Should already be wired from Part 8.6. Verify.

### 12.6 — TutorialManager on TutorialOverlay

Should already be wired from Part 9.9. Verify.

### 12.7 — TabManager on MainCanvas

Should already be wired from Part 3.3. Verify all 4 tabs have their button, content panel, and icon.

---

## Part 13: Build Settings

### 13.1 — Switch Platform to WebGL

1. **File → Build Profiles** (or File → Build Settings in older Unity versions).
2. Select **Web** (WebGL) from the platform list.
3. Click **Switch Platform** if it's not already selected. Wait for reimport.

### 13.2 — Add Scene to Build

1. In the Build Profiles window, under **Scenes In Build**:
   - Click **Add Open Scenes** (or drag `Assets/Scenes/MainScene.unity` into the list).
   - Ensure it's checked ✓ and has index `0`.

### 13.3 — Player Settings

Open **Edit → Project Settings → Player** (or click **Player Settings** in the Build Profiles window):

**Product Name & Company:**
| Field | Value |
|-------|-------|
| Company Name | `Rune Realms` |
| Product Name | `RuneRealms` ← **no spaces** (determines output filenames) |

**Resolution and Presentation:**
| Field | Value |
|-------|-------|
| Default Canvas Width | `1080` |
| Default Canvas Height | `1920` |
| Run In Background | ✓ |

**Other Settings:**
| Field | Value |
|-------|-------|
| Strip Engine Code | ✓ |
| Managed Stripping Level | **Medium** (or Low if you get reflection errors) |

**Publishing Settings:**
| Field | Value |
|-------|-------|
| Decompression Fallback | ✓ **Enabled** |
| Data Caching | ✓ **Enabled** |
| Compression Format | (see Part 14 — changes per build step) |

---

## Part 14: WebGL Build (Two-Step Process)

Unity WebGL builds for Devvit require a **two-step build process**: compressed data/wasm files, and uncompressed framework/loader files.

### Step 1: Build Compressed Assets

1. Go to **Player Settings → Publishing Settings**.
2. Set **Compression Format**: **GZip**.
3. **File → Build And Run** (or Build Profiles → Build).
4. Choose an output folder: create a folder named `BuildGzip` outside the project.
5. Wait for the build to complete.
6. From the `BuildGzip/Build/` folder, **copy these 2 files** to your Devvit project:
   - `RuneRealms.data.unityweb` → copy to `public/Build/RuneRealms.data.unityweb`
   - `RuneRealms.wasm.unityweb` → copy to `public/Build/RuneRealms.wasm.unityweb`

### Step 2: Build Uncompressed Framework

1. Go back to **Player Settings → Publishing Settings**.
2. Set **Compression Format**: **Disabled**.
3. Build again to a different folder: `BuildRaw`.
4. Wait for the build to complete.
5. From the `BuildRaw/Build/` folder, **copy these 2 files** to your Devvit project:
   - `RuneRealms.framework.js` → copy to `public/Build/RuneRealms.framework.js`
   - `RuneRealms.loader.js` → copy to `public/Build/RuneRealms.loader.js`

### Step 3: Verify Build Files

After both steps, your `public/Build/` folder should contain exactly these 4 files:

```
public/Build/
├── RuneRealms.data.unityweb      ← from GZip build
├── RuneRealms.framework.js       ← from Disabled build
├── RuneRealms.loader.js          ← from Disabled build
└── RuneRealms.wasm.unityweb      ← from GZip build
```

### Step 4: Test Locally

From the `rune-realms` project root:

```bash
npm run dev
```

Open the playtest URL. The load sequence should be:
1. Themed splash page loads.
2. Click "Enter the Realm".
3. Unity WebGL loads (gold progress bar with rotating tips).
4. Game initializes → Skills tab shown → tap a skill to start training.

### Important Build Notes

- The **Product Name** MUST be `RuneRealms` (no spaces). This determines the output filenames (`RuneRealms.data.unityweb`, etc.). If the product name doesn't match, the loader won't find the files.
- If builds are too large, ensure **Strip Engine Code** is enabled and consider switching **Managed Stripping Level** to **High** (test carefully for reflection-based issues with Newtonsoft.Json).
- WebGL builds do NOT support threads or SIMD by default. Leave those settings off unless you specifically need them.
- If you see "Unable to parse Build/RuneRealms.framework.js" errors in the browser console, you likely mixed up compressed and uncompressed files. The `.framework.js` and `.loader.js` must be **uncompressed**.

---

## Part 15: Color Reference Table

| Token | Hex | R | G | B | Alpha | Usage |
|-------|-----|---|---|---|-------|-------|
| Background | `#1A1410` | 26 | 20 | 16 | 255 | Main scene background, TabBar bg, progress bar tracks |
| Panel | `#3A3125` | 58 | 49 | 37 | 255 | Cards, skill slots, popup panels, prefabs |
| Panel Dark | `#2A2218` | 42 | 34 | 24 | 255 | Tab panel backgrounds |
| Border / Inactive | `#5C4A2A` | 92 | 74 | 42 | 255 | Panel borders, inactive tab buttons, skip button |
| Gold | `#D4A438` | 212 | 164 | 56 | 255 | Headings, active tabs, level text, outlines, gold buttons |
| Gold Bright | `#FFCC00` | 255 | 204 | 0 | 255 | Action button text, highlights |
| Parchment | `#B8A88A` | 184 | 168 | 138 | 255 | Body text, descriptions, inactive button text |
| XP Blue | `#00AAFF` | 0 | 170 | 255 | 255 | XP progress bar fill, XP reward text |
| Health Green | `#00CC00` | 0 | 204 | 0 | 255 | Player HP bar fill, Use button background |
| Damage Red | `#CC0000` | 204 | 0 | 0 | 255 | Enemy HP bar fill, Run button text, Close button bg |
| Heal Green | `#00FF88` | 0 | 255 | 136 | 255 | Heal button text, loot text |
| White | `#FFFFFF` | 255 | 255 | 255 | 255 | HP text, quantity text, button labels on colored bg |
| Overlay Light | `#000000` | 0 | 0 | 0 | 150 | OfflinePopup semi-transparent background |
| Overlay Dark | `#000000` | 0 | 0 | 0 | 180 | TutorialOverlay semi-transparent background |
| Active Glow | `#D4A438` | 212 | 164 | 56 | 100 | Skill slot active indicator overlay |
| Dark on Gold | `#1A1410` | 26 | 20 | 16 | 255 | Text on gold-colored buttons (Collect, Next) |

---

## Part 16: Font Reference

All text in the project uses **TextMeshPro default font** (Liberation Sans SDF), unless Legacy Text is used for TutorialManager.

| Context | Font Size | Style | Example |
|---------|----------|-------|---------|
| Page headings | `28` | **Bold** | "Skills", "Inventory", "Arena", "Leaderboard" |
| Sub-headings | `20` | **Bold** | "Wave 1", "While you were away..." |
| Popup titles | `22`–`24` | **Bold** | Tutorial step titles |
| Body text | `14`–`16` | Regular | Descriptions, summaries, stat text |
| Labels / Captions | `12`–`13` | Regular | Step counter, small info text |
| Button text | `14`–`18` | **Bold** | "Attack", "Start Battle", "Collect!" |
| Level numbers | `18` | **Bold** | "Lv.1", "Lv.42" |
| Skill names | `14` | Regular | "Woodcutting", "Fishing" |
| HP text | `14` | Regular | "50/50", "30/30" |
| Quantity text | `10` | Regular | "x1", "x5" |
| Prefab item name | `11` | Regular | Inventory slot item names |
| Tab bar labels | `14` | **Bold** | "Skills", "Arena", "Inventory", "Leaders" |

**Minimum touch target:** All interactive elements (buttons, skill slots, tab buttons) should be at least **80px tall** in the reference resolution (1080×1920) to ensure comfortable tapping on mobile.

---

## Part 17: Final Checklist

Go through **every item** before building. Check each box mentally (or print this out and check physically).

### Scripts & Packages

- [ ] All 16 `.cs` files compiled without errors (Console is clean of red messages)
- [ ] `com.unity.nuget.newtonsoft-json` is installed (Package Manager → In Project)
- [ ] TextMeshPro Essentials are imported (`Assets/TextMesh Pro/` folder exists)

### Scene Hierarchy

- [ ] `DevvitBridge` exists as a root GameObject with exact name `DevvitBridge`
- [ ] `DevvitBridge` has `DevvitBridge.cs` attached
- [ ] `GameManager` exists with `GameManager.cs` attached
- [ ] `AudioManager` exists with `AudioManager.cs` attached
- [ ] `MainCanvas` exists with Canvas (Screen Space - Overlay) + Canvas Scaler (1080×1920, Match 0.5)
- [ ] `MainCanvas` has `TabManager.cs` attached
- [ ] `EventSystem` exists (child of MainCanvas or standalone)

### Tab Panels

- [ ] `SkillsTab` has: Panel + CanvasGroup + `IdleManager.cs`, Bottom offset `80`
- [ ] `ArenaTab` has: Panel + CanvasGroup + `CombatManager.cs`, Bottom offset `80`
- [ ] `InventoryTab` has: Panel + CanvasGroup + `InventoryManager.cs`, Bottom offset `80`
- [ ] `LeaderboardTab` has: Panel + CanvasGroup + `LeaderboardManager.cs`, Bottom offset `80`
- [ ] `OfflinePopup` has: Panel + `OfflinePopup.cs`, **disabled by default**
- [ ] `TutorialOverlay` has: Panel + CanvasGroup + `TutorialManager.cs`, **disabled by default**

### Tab Bar

- [ ] `TabBar` exists at bottom of MainCanvas, Height `80`, color `#1A1410`
- [ ] `TabBar` has Horizontal Layout Group with spacing `4`, padding `4` all sides
- [ ] 4 tab buttons exist: `SkillsTabBtn`, `ArenaTabBtn`, `InventoryTabBtn`, `LeaderboardTabBtn`
- [ ] Each button has Image color `#5C4A2A` and child TMP text
- [ ] `TabManager` on MainCanvas has 4 tabs wired (button, content, icon, colors)

### Skills Tab

- [ ] `SkillsTitle` TMP text "Skills" at top, 28pt, bold, `#D4A438`
- [ ] `SkillGrid` ScrollView with horizontal scrolling disabled
- [ ] `Content` has Vertical Layout Group (spacing 8) + Content Size Fitter (Preferred)
- [ ] 8 skill slots exist in this order: Woodcutting, Fishing, Mining, Cooking, Smithing, Attack, Strength, Defence
- [ ] Each slot has: Layout Element (80×80), Image `#3A3125`, Button component
- [ ] Each slot has 5 children: SkillIcon, SkillName, LevelText, ProgressBar, ActiveGlow
- [ ] All ProgressBar sliders: Interactable ☐, Handle deleted, Fill color `#00AAFF`, BG `#1A1410`
- [ ] All ActiveGlow: `#D4A438` A:100, disabled by default, Raycast Target ☐
- [ ] `IdleManager` Skill Slots array has 8 entries, all correctly wired
- [ ] `IdleManager` XP Per Tick: 10, 12, 8, 9, 11, 7, 8, 6
- [ ] All `skillName` strings are lowercase: woodcutting, fishing, mining, cooking, smithing, attack, strength, defence

### Arena Tab

- [ ] `CombatPanel` exists (enabled) with PlayerSection, EnemySection, CombatInfo, ActionButtons
- [ ] Player HP bar: fill `#00CC00`, Interactable ☐, Handle deleted
- [ ] Enemy HP bar: fill `#CC0000`, Interactable ☐, Handle deleted
- [ ] 4 action buttons: AttackBtn, SpecialBtn, HealBtn, RunBtn — all color `#5C4A2A`
- [ ] `VictoryPanel` exists, **disabled by default**, with StartBattleBtn
- [ ] `CombatManager` has all 19 UI fields wired (no `None` or `Missing`)

### Inventory Tab

- [ ] `InventoryTitle` TMP text "Inventory" at top
- [ ] `SlotCountText` TMP text "0/20 slots"
- [ ] `SlotGrid` ScrollView with Content using Grid Layout Group (120×120 cells, 4 columns)
- [ ] `ItemDetailPopup` exists, **disabled by default**, with 6 children
- [ ] `InventoryManager` has all fields wired including prefab and popup elements

### Leaderboard Tab

- [ ] `LeaderboardTitle` TMP text "Leaderboard" at top
- [ ] `TypeButtons` panel with 3 buttons: TotalLevelBtn, TotalXpBtn, ArenaKillsBtn
- [ ] `EntryList` ScrollView with Content using Vertical Layout Group
- [ ] `PlayerRankText` TMP at bottom
- [ ] `LeaderboardManager` has all fields wired including prefab

### Offline Popup

- [ ] `PopupCard` panel (500×350) centered inside `OfflinePopup`
- [ ] TitleText, GainsText, CollectButton all exist and are wired
- [ ] `OfflinePopup` is **disabled**

### Tutorial Overlay

- [ ] `DialogPanel` (600×400) centered inside `TutorialOverlay`
- [ ] TitleLabel, DescriptionLabel, StepCounter, NextButton, SkipButton all exist
- [ ] Text type matches script (Legacy `Text` or `TMP_Text` if modified)
- [ ] `TutorialManager` has all fields wired
- [ ] `TutorialOverlay` is **disabled**

### Prefabs

- [ ] `Assets/Prefabs/InventorySlot` exists with Button, ItemName (TMP), Quantity (TMP named exactly "Quantity")
- [ ] `Assets/Prefabs/LeaderboardEntry` exists with 3 TMP children in order: Rank, Username, Score
- [ ] Both prefabs are wired into their respective managers

### ScriptableObjects

- [ ] 8 `EnemyData` assets exist in `Assets/ScriptableObjects/`
- [ ] 1 `EnemyDatabase` asset exists with all 8 enemies in its list
- [ ] `EnemyDatabase` is wired to `CombatManager`'s Enemy Database field
- [ ] Loot tables are populated for all 8 enemies

### Cross-References (GameManager)

- [ ] GameManager → Idle Manager = SkillsTab
- [ ] GameManager → Combat Manager = ArenaTab
- [ ] GameManager → Inventory Manager = InventoryTab
- [ ] GameManager → Leaderboard Manager = LeaderboardTab
- [ ] GameManager → Tab Manager = MainCanvas
- [ ] GameManager → Offline Popup = OfflinePopup
- [ ] GameManager → Tutorial Manager = TutorialOverlay
- [ ] GameManager → Auto Save Interval = 30

### Build Settings

- [ ] Platform is set to **Web** (WebGL)
- [ ] `MainScene` is in the build scenes list at index 0
- [ ] Product Name = `RuneRealms` (no spaces)
- [ ] Decompression Fallback = Enabled
- [ ] Run In Background = Enabled
- [ ] Strip Engine Code = Enabled
- [ ] Data Caching = Enabled

### Build Output

- [ ] `public/Build/RuneRealms.data.unityweb` exists (from GZip build)
- [ ] `public/Build/RuneRealms.wasm.unityweb` exists (from GZip build)
- [ ] `public/Build/RuneRealms.framework.js` exists (from Disabled build)
- [ ] `public/Build/RuneRealms.loader.js` exists (from Disabled build)
- [ ] `npm run dev` starts successfully and the game loads in the browser

---

## Appendix: Complete Hierarchy Reference

For quick reference, here is the final complete Hierarchy tree:

```
DevvitBridge                         [DevvitBridge.cs]
GameManager                          [GameManager.cs]
AudioManager                         [AudioManager.cs]
MainCanvas                           [Canvas, CanvasScaler, GraphicRaycaster, TabManager.cs]
├── EventSystem
├── SkillsTab                        [Panel, Image, CanvasGroup, IdleManager.cs]
│   ├── SkillsTitle                  [TMP]
│   └── SkillGrid                    [ScrollView]
│       └── Viewport
│           └── Content              [VerticalLayoutGroup, ContentSizeFitter]
│               ├── WoodcuttingSlot  [Panel, Image, LayoutElement, Button]
│               │   ├── SkillIcon    [Image]
│               │   ├── SkillName    [TMP "Woodcutting"]
│               │   ├── LevelText    [TMP "Lv.1"]
│               │   ├── ProgressBar  [Slider]
│               │   └── ActiveGlow   [Image, DISABLED]
│               ├── FishingSlot      [...same children...]
│               ├── MiningSlot       [...same children...]
│               ├── CookingSlot      [...same children...]
│               ├── SmithingSlot     [...same children...]
│               ├── AttackSlot       [...same children...]
│               ├── StrengthSlot     [...same children...]
│               └── DefenceSlot      [...same children...]
├── ArenaTab                         [Panel, Image, CanvasGroup, CombatManager.cs]
│   ├── CombatPanel                  [Panel]
│   │   ├── PlayerSection            [Panel]
│   │   │   ├── PlayerHpBar          [Slider]
│   │   │   ├── PlayerHpText         [TMP "50/50"]
│   │   │   └── PlayerStatsText      [TMP "ATK: 10  DEF: 5  SPL: 20"]
│   │   ├── EnemySection             [Panel]
│   │   │   ├── EnemyIcon            [Image]
│   │   │   ├── EnemyNameText        [TMP "Goblin"]
│   │   │   ├── EnemyHpBar           [Slider]
│   │   │   └── EnemyHpText          [TMP "30/30"]
│   │   ├── CombatInfo               [Panel]
│   │   │   ├── WaveText             [TMP "Wave 1"]
│   │   │   ├── KillCountText        [TMP "Kills: 0"]
│   │   │   ├── LootText             [TMP ""]
│   │   │   └── XpRewardText         [TMP ""]
│   │   └── ActionButtons            [Panel, HorizontalLayoutGroup]
│   │       ├── AttackBtn            [Button-TMP "Attack"]
│   │       ├── SpecialBtn           [Button-TMP "Special"]
│   │       ├── HealBtn              [Button-TMP "Heal"]
│   │       └── RunBtn               [Button-TMP "Run"]
│   └── VictoryPanel                 [Panel, DISABLED]
│       ├── ResultsTitle             [TMP "Arena"]
│       ├── ResultsSummary           [TMP "Defeat monsters..."]
│       └── StartBattleBtn           [Button-TMP "Start Battle"]
├── InventoryTab                     [Panel, Image, CanvasGroup, InventoryManager.cs]
│   ├── InventoryTitle               [TMP "Inventory"]
│   ├── SlotCountText                [TMP "0/20 slots"]
│   ├── SlotGrid                     [ScrollView]
│   │   └── Viewport
│   │       └── Content              [GridLayoutGroup, ContentSizeFitter]
│   └── ItemDetailPopup              [Panel, DISABLED]
│       ├── RarityBorder             [Image]
│       ├── DetailName               [TMP]
│       ├── DetailCategory           [TMP]
│       ├── DetailQuantity           [TMP]
│       ├── UseButton                [Button-TMP "Use"]
│       └── CloseButton              [Button-TMP "Close"]
├── LeaderboardTab                   [Panel, Image, CanvasGroup, LeaderboardManager.cs]
│   ├── LeaderboardTitle             [TMP "Leaderboard"]
│   ├── TypeButtons                  [Panel, HorizontalLayoutGroup]
│   │   ├── TotalLevelBtn            [Button-TMP "Total Level"]
│   │   ├── TotalXpBtn               [Button-TMP "Total XP"]
│   │   └── ArenaKillsBtn            [Button-TMP "Arena Kills"]
│   ├── EntryList                    [ScrollView]
│   │   └── Viewport
│   │       └── Content              [VerticalLayoutGroup, ContentSizeFitter]
│   └── PlayerRankText               [TMP "Your Rank: Unranked"]
├── OfflinePopup                     [Panel, OfflinePopup.cs, DISABLED]
│   └── PopupCard                    [Panel, Outline]
│       ├── TitleText                [TMP "While you were away..."]
│       ├── GainsText                [TMP ""]
│       └── CollectButton            [Button-TMP "Collect!"]
├── TutorialOverlay                  [Panel, CanvasGroup, TutorialManager.cs, DISABLED]
│   └── DialogPanel                  [Panel, Outline]
│       ├── TitleLabel               [Text/TMP]
│       ├── DescriptionLabel         [Text/TMP]
│       ├── StepCounter              [Text/TMP "1/7"]
│       ├── NextButton               [Button]
│       │   └── NextButtonLabel      [Text/TMP "Next"]
│       └── SkipButton               [Button]
│           └── SkipButtonLabel      [Text "Skip"]
└── TabBar                           [Panel, Image, HorizontalLayoutGroup]
    ├── SkillsTabBtn                 [Button-TMP "Skills"]
    ├── ArenaTabBtn                  [Button-TMP "Arena"]
    ├── InventoryTabBtn              [Button-TMP "Inventory"]
    └── LeaderboardTabBtn            [Button-TMP "Leaders"]
```

---

## Appendix: Troubleshooting

### "DevvitBridge not found"
The `GameManager` logs this if there is no GameObject named exactly `DevvitBridge` in the scene. Check spelling and capitalization. It must be a root-level GameObject, not nested under another.

### "Newtonsoft.Json not found"
Install via Package Manager: `com.unity.nuget.newtonsoft-json`. If you're on an older Unity version, you may need to add it to `Packages/manifest.json` manually.

### Slider Handle still shows
You must manually delete the `Handle Slide Area` child from each Slider used as a display-only bar (HP bars, XP progress bars). The Slider component will still function without it.

### Buttons not responding
- Verify there's an `EventSystem` in the scene.
- Verify the `Canvas` has a `Graphic Raycaster` component.
- Check that no invisible overlay (like a disabled panel's remnant) is blocking raycasts. Ensure `ActiveGlow` Images have **Raycast Target** unchecked.

### Skills don't resume after reload
The `IdleManager` resumes the active skill from `PlayerSkills.activeSkill`. If this field is null or empty, no skill auto-starts. Verify the server is saving and returning this field.

### Build is too large
- Enable **Strip Engine Code** in Player Settings.
- Set **Managed Stripping Level** to **Medium** or **High**.
- Remove unused packages from Package Manager.
- Use `.ogg` format for audio clips (smaller than `.wav`).
- Avoid large textures — use compressed formats or reduce resolution.

### "Unable to parse Build/RuneRealms.framework.js"
You accidentally used the GZip-compressed version of the framework file. The `.framework.js` and `.loader.js` files MUST come from the **Disabled** compression build (Step 2 of Part 14).

### Floating text appears at wrong position
The `FloatingText.Spawn()` method uses `transform.position` from the skill slot button. In Screen Space - Overlay canvas mode, this should work correctly. If text appears off-screen, ensure the Canvas is set to **Screen Space - Overlay** (not World Space).

---

_This guide replaces the previous `SETUP.md`. Last updated for Rune Realms Unity project with 16 scripts, 8 skills, 8 enemies, and WebGL deployment to Devvit._