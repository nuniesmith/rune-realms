namespace RuneRealms.Audio
{
    /// <summary>
    /// Static constants for the audio system.
    /// Centralizes PlayerPrefs keys, pitch ranges, and documents clip assignments.
    /// </summary>
    public static class AudioConstants
    {
        // ── PlayerPrefs Keys ────────────────────────────────────────────

        /// <summary>PlayerPrefs key for the mute toggle (int: 0 = unmuted, 1 = muted).</summary>
        public const string PrefKeyMuted = "RuneRealms_Audio_Muted";

        /// <summary>PlayerPrefs key for SFX volume (float: 0.0 – 1.0).</summary>
        public const string PrefKeySfxVolume = "RuneRealms_Audio_SfxVolume";

        /// <summary>PlayerPrefs key for music/ambient volume (float: 0.0 – 1.0).</summary>
        public const string PrefKeyMusicVolume = "RuneRealms_Audio_MusicVolume";

        // ── Pitch Variation ─────────────────────────────────────────────

        /// <summary>Default base pitch for all SFX (1.0 = normal).</summary>
        public const float DefaultPitch = 1.0f;

        /// <summary>Minimum pitch multiplier for skill tick sounds (slightly lower).</summary>
        public const float SkillTickPitchMin = 0.90f;

        /// <summary>Maximum pitch multiplier for skill tick sounds (slightly higher).</summary>
        public const float SkillTickPitchMax = 1.15f;

        /// <summary>Minimum pitch multiplier for attack hit sounds.</summary>
        public const float AttackPitchMin = 0.93f;

        /// <summary>Maximum pitch multiplier for attack hit sounds.</summary>
        public const float AttackPitchMax = 1.07f;

        /// <summary>Minimum pitch multiplier for loot drop sounds.</summary>
        public const float LootPitchMin = 0.95f;

        /// <summary>Maximum pitch multiplier for loot drop sounds.</summary>
        public const float LootPitchMax = 1.10f;

        /// <summary>Minimum pitch multiplier for button/UI click sounds.</summary>
        public const float ButtonClickPitchMin = 0.97f;

        /// <summary>Maximum pitch multiplier for button/UI click sounds.</summary>
        public const float ButtonClickPitchMax = 1.03f;

        // ── Volume Defaults ─────────────────────────────────────────────

        /// <summary>Default SFX volume when no saved preference exists.</summary>
        public const float DefaultSfxVolume = 0.7f;

        /// <summary>Default music/ambient volume when no saved preference exists.</summary>
        public const float DefaultMusicVolume = 0.5f;

        // ── Audio Clip Assignment Guide ─────────────────────────────────
        //
        // Assign AudioClips in the AudioManager Inspector under these groups:
        //
        // ┌─────────────────────────────────────────────────────────────┐
        // │  GROUP: Skill Sounds                                       │
        // ├─────────────────────────────────────────────────────────────┤
        // │  skillTickDefault   → Short tick/chime (fallback)          │
        // │  skillTickMining    → Pickaxe hit / stone clink            │
        // │  skillTickFishing   → Water splash / reel click            │
        // │  skillTickCooking   → Sizzle / bubble                      │
        // │  skillTickSmithing  → Anvil clang / hammer tap             │
        // │  skillTickWoodcutting → Axe chop / wood crack              │
        // │  levelUpClip        → Triumphant jingle / fanfare          │
        // ├─────────────────────────────────────────────────────────────┤
        // │  GROUP: Combat Sounds                                      │
        // ├─────────────────────────────────────────────────────────────┤
        // │  attackHitClip      → Sword slash / blunt impact           │
        // │  specialAttackClip  → Heavier impact / magic burst         │
        // │  healClip           → Soft chime / sparkle                 │
        // │  enemyDeathClip     → Defeat thud / dissolve               │
        // │  playerDeathClip    → Low failure tone / collapse           │
        // │  combatStartClip   → Battle horn / drum hit                │
        // │  waveClearClip      → Victory sting / wave complete        │
        // ├─────────────────────────────────────────────────────────────┤
        // │  GROUP: UI Sounds                                          │
        // ├─────────────────────────────────────────────────────────────┤
        // │  buttonClickClip   → Soft click / tap                      │
        // │  lootDropClip      → Coin jingle / item pickup             │
        // │  itemUseClip       → Potion gulp / consume sound           │
        // └─────────────────────────────────────────────────────────────┘
        //
        // Recommended sources for free clips:
        //   • https://opengameart.org
        //   • https://freesound.org
        //   • Unity Asset Store (search "free SFX")
        //
        // Keep clips short (< 2 seconds for SFX) and in .ogg or .wav format.
        // WebGL builds work best with compressed .ogg files for smaller bundles.
    }
}
