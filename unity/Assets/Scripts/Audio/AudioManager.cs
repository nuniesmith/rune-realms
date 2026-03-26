using UnityEngine;

namespace RuneRealms.Audio
{
    /// <summary>
    /// Singleton audio manager for Rune Realms.
    /// Manages two AudioSources (SFX + Music/Ambient) and exposes
    /// named convenience methods for every game event that needs sound.
    /// Persists mute/volume state via PlayerPrefs.
    /// Attach to a persistent "AudioManager" GameObject in the scene.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        // ── Singleton ───────────────────────────────────────────────────

        public static AudioManager Instance { get; private set; }

        // ── Audio Sources (added at runtime or assigned in Inspector) ───

        [Header("Audio Sources")]
        [Tooltip("Source used for one-shot SFX. Created automatically if left empty.")]
        [SerializeField] private AudioSource sfxSource;

        [Tooltip("Source used for looping music / ambient. Created automatically if left empty.")]
        [SerializeField] private AudioSource musicSource;

        // ── Skill Sounds ────────────────────────────────────────────────

        [Header("Skill Sounds")]
        [Tooltip("Fallback tick sound when no per-skill clip is assigned.")]
        [SerializeField] private AudioClip skillTickDefault;

        [Tooltip("Pickaxe hit / stone clink for mining ticks.")]
        [SerializeField] private AudioClip skillTickMining;

        [Tooltip("Water splash / reel click for fishing ticks.")]
        [SerializeField] private AudioClip skillTickFishing;

        [Tooltip("Sizzle / bubble for cooking ticks.")]
        [SerializeField] private AudioClip skillTickCooking;

        [Tooltip("Anvil clang / hammer tap for smithing ticks.")]
        [SerializeField] private AudioClip skillTickSmithing;

        [Tooltip("Axe chop / wood crack for woodcutting ticks.")]
        [SerializeField] private AudioClip skillTickWoodcutting;

        [Tooltip("Triumphant jingle played on any skill level-up.")]
        [SerializeField] private AudioClip levelUpClip;

        // ── Combat Sounds ───────────────────────────────────────────────

        [Header("Combat Sounds")]
        [Tooltip("Sword slash / blunt impact for normal attacks.")]
        [SerializeField] private AudioClip attackHitClip;

        [Tooltip("Heavier impact / magic burst for special attacks.")]
        [SerializeField] private AudioClip specialAttackClip;

        [Tooltip("Soft chime / sparkle for healing.")]
        [SerializeField] private AudioClip healClip;

        [Tooltip("Defeat thud / dissolve when an enemy dies.")]
        [SerializeField] private AudioClip enemyDeathClip;

        [Tooltip("Low failure tone / collapse when the player dies.")]
        [SerializeField] private AudioClip playerDeathClip;

        [Tooltip("Battle horn / drum hit when combat starts.")]
        [SerializeField] private AudioClip combatStartClip;

        [Tooltip("Victory sting when a wave is cleared.")]
        [SerializeField] private AudioClip waveClearClip;

        // ── UI Sounds ───────────────────────────────────────────────────

        [Header("UI Sounds")]
        [Tooltip("Soft click / tap for buttons and tabs.")]
        [SerializeField] private AudioClip buttonClickClip;

        [Tooltip("Coin jingle / item pickup for loot drops.")]
        [SerializeField] private AudioClip lootDropClip;

        [Tooltip("Potion gulp / consume sound when using an item.")]
        [SerializeField] private AudioClip itemUseClip;

        // ── Music / Ambient ─────────────────────────────────────────────

        [Header("Music / Ambient")]
        [Tooltip("Optional ambient loop that plays in the background.")]
        [SerializeField] private AudioClip ambientLoop;

        // ── State ───────────────────────────────────────────────────────

        private bool isMuted;
        private float sfxVolume;
        private float musicVolume;
        private bool userHasInteracted;

        // ── Properties ──────────────────────────────────────────────────

        /// <summary>
        /// Global mute toggle. Persisted to PlayerPrefs.
        /// </summary>
        public bool IsMuted
        {
            get => isMuted;
            set
            {
                isMuted = value;
                PlayerPrefs.SetInt(AudioConstants.PrefKeyMuted, isMuted ? 1 : 0);
                PlayerPrefs.Save();
                ApplyVolumes();
            }
        }

        /// <summary>
        /// SFX volume (0 – 1). Persisted to PlayerPrefs.
        /// </summary>
        public float SfxVolume
        {
            get => sfxVolume;
            set
            {
                sfxVolume = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(AudioConstants.PrefKeySfxVolume, sfxVolume);
                PlayerPrefs.Save();
                ApplyVolumes();
            }
        }

        /// <summary>
        /// Music / ambient volume (0 – 1). Persisted to PlayerPrefs.
        /// </summary>
        public float MusicVolume
        {
            get => musicVolume;
            set
            {
                musicVolume = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(AudioConstants.PrefKeyMusicVolume, musicVolume);
                PlayerPrefs.Save();
                ApplyVolumes();
            }
        }

        // ── Unity Lifecycle ─────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            EnsureAudioSources();
            LoadPrefs();
            ApplyVolumes();
        }

        private void Start()
        {
            // Start ambient music if one is assigned
            if (ambientLoop != null && musicSource != null)
            {
                musicSource.clip = ambientLoop;
                musicSource.loop = true;
                // Delay actual playback until the user has interacted (WebGL requirement)
                if (userHasInteracted)
                {
                    musicSource.Play();
                }
            }
        }

        private void Update()
        {
            // WebGL requires a user gesture before audio can play.
            // We detect the first click/touch/key and unlock audio.
            if (!userHasInteracted && (Input.anyKeyDown || Input.GetMouseButtonDown(0)))
            {
                userHasInteracted = true;
                OnUserInteracted();
            }
        }

        // ── Public Play Methods ─────────────────────────────────────────

        /// <summary>
        /// Plays an arbitrary AudioClip as a one-shot SFX at the current SFX volume.
        /// </summary>
        public void PlaySFX(AudioClip clip)
        {
            PlaySFXInternal(clip, AudioConstants.DefaultPitch);
        }

        /// <summary>
        /// Plays a normal attack hit sound with slight pitch variation.
        /// </summary>
        public void PlayAttack()
        {
            PlayWithPitchVariation(attackHitClip, AudioConstants.AttackPitchMin, AudioConstants.AttackPitchMax);
        }

        /// <summary>
        /// Plays a special/heavy attack sound with slight pitch variation.
        /// </summary>
        public void PlaySpecialAttack()
        {
            PlayWithPitchVariation(specialAttackClip, AudioConstants.AttackPitchMin, AudioConstants.AttackPitchMax);
        }

        /// <summary>
        /// Plays the level-up fanfare at default pitch.
        /// </summary>
        public void PlayLevelUp()
        {
            PlaySFXInternal(levelUpClip, AudioConstants.DefaultPitch);
        }

        /// <summary>
        /// Plays a skill tick sound. Chooses a per-skill clip when available,
        /// otherwise falls back to <see cref="skillTickDefault"/>.
        /// Applies random pitch variation to avoid monotony.
        /// </summary>
        public void PlaySkillTick(string skillName)
        {
            var clip = ResolveSkillTickClip(skillName);
            PlayWithPitchVariation(clip, AudioConstants.SkillTickPitchMin, AudioConstants.SkillTickPitchMax);
        }

        /// <summary>
        /// Plays the enemy death / defeat sound.
        /// </summary>
        public void PlayEnemyDeath()
        {
            PlaySFXInternal(enemyDeathClip, AudioConstants.DefaultPitch);
        }

        /// <summary>
        /// Plays the player death / failure sound.
        /// </summary>
        public void PlayPlayerDeath()
        {
            PlaySFXInternal(playerDeathClip, AudioConstants.DefaultPitch);
        }

        /// <summary>
        /// Plays the heal / recovery chime.
        /// </summary>
        public void PlayHeal()
        {
            PlaySFXInternal(healClip, AudioConstants.DefaultPitch);
        }

        /// <summary>
        /// Plays the loot drop jingle with slight pitch variation.
        /// </summary>
        public void PlayLoot()
        {
            PlayWithPitchVariation(lootDropClip, AudioConstants.LootPitchMin, AudioConstants.LootPitchMax);
        }

        /// <summary>
        /// Plays a UI button / tab click with subtle pitch variation.
        /// </summary>
        public void PlayButtonClick()
        {
            PlayWithPitchVariation(buttonClickClip, AudioConstants.ButtonClickPitchMin, AudioConstants.ButtonClickPitchMax);
        }

        /// <summary>
        /// Plays the combat start horn / drum hit.
        /// </summary>
        public void PlayCombatStart()
        {
            PlaySFXInternal(combatStartClip, AudioConstants.DefaultPitch);
        }

        /// <summary>
        /// Plays the wave-clear victory sting.
        /// </summary>
        public void PlayWaveClear()
        {
            PlaySFXInternal(waveClearClip, AudioConstants.DefaultPitch);
        }

        /// <summary>
        /// Plays the item-use / consume sound.
        /// </summary>
        public void PlayItemUse()
        {
            PlaySFXInternal(itemUseClip, AudioConstants.DefaultPitch);
        }

        // ── Music Controls ──────────────────────────────────────────────

        /// <summary>
        /// Starts or switches the background music/ambient loop.
        /// </summary>
        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (musicSource == null) return;
            if (clip == null)
            {
                Debug.LogWarning("[AudioManager] PlayMusic called with null clip.");
                return;
            }

            musicSource.clip = clip;
            musicSource.loop = loop;

            if (!isMuted && userHasInteracted)
            {
                musicSource.Play();
            }
        }

        /// <summary>
        /// Stops the current music/ambient track.
        /// </summary>
        public void StopMusic()
        {
            if (musicSource != null)
            {
                musicSource.Stop();
            }
        }

        // ── Internal Helpers ────────────────────────────────────────────

        private void PlaySFXInternal(AudioClip clip, float pitch)
        {
            if (clip == null)
            {
                Debug.LogWarning("[AudioManager] Attempted to play a null AudioClip. Assign it in the Inspector.");
                return;
            }

            if (isMuted || sfxSource == null) return;

            sfxSource.pitch = pitch;
            sfxSource.PlayOneShot(clip, sfxVolume);
        }

        private void PlayWithPitchVariation(AudioClip clip, float pitchMin, float pitchMax)
        {
            if (clip == null)
            {
                Debug.LogWarning("[AudioManager] Attempted to play a null AudioClip. Assign it in the Inspector.");
                return;
            }

            float randomPitch = Random.Range(pitchMin, pitchMax);
            PlaySFXInternal(clip, randomPitch);
        }

        private AudioClip ResolveSkillTickClip(string skillName)
        {
            if (string.IsNullOrEmpty(skillName))
                return skillTickDefault;

            switch (skillName.ToLowerInvariant())
            {
                case "mining":
                    return skillTickMining != null ? skillTickMining : skillTickDefault;
                case "fishing":
                    return skillTickFishing != null ? skillTickFishing : skillTickDefault;
                case "cooking":
                    return skillTickCooking != null ? skillTickCooking : skillTickDefault;
                case "smithing":
                    return skillTickSmithing != null ? skillTickSmithing : skillTickDefault;
                case "woodcutting":
                    return skillTickWoodcutting != null ? skillTickWoodcutting : skillTickDefault;
                default:
                    return skillTickDefault;
            }
        }

        private void EnsureAudioSources()
        {
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }

            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.playOnAwake = false;
                musicSource.loop = true;
            }
        }

        private void LoadPrefs()
        {
            isMuted = PlayerPrefs.GetInt(AudioConstants.PrefKeyMuted, 0) == 1;
            sfxVolume = PlayerPrefs.GetFloat(AudioConstants.PrefKeySfxVolume, AudioConstants.DefaultSfxVolume);
            musicVolume = PlayerPrefs.GetFloat(AudioConstants.PrefKeyMusicVolume, AudioConstants.DefaultMusicVolume);
        }

        private void ApplyVolumes()
        {
            if (sfxSource != null)
            {
                sfxSource.volume = isMuted ? 0f : sfxVolume;
            }

            if (musicSource != null)
            {
                musicSource.volume = isMuted ? 0f : musicVolume;
            }
        }

        /// <summary>
        /// Called once when the first user interaction is detected.
        /// Resumes any music that was waiting for a gesture (WebGL requirement).
        /// </summary>
        private void OnUserInteracted()
        {
            if (musicSource != null && musicSource.clip != null && !musicSource.isPlaying && !isMuted)
            {
                musicSource.Play();
            }
        }
    }
}
