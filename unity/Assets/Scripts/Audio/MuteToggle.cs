using UnityEngine;
using UnityEngine.UI;

namespace RuneRealms.Audio
{
    /// <summary>
    /// UI component for controlling audio mute state and volume.
    /// Attach to a GameObject that contains a Toggle (for mute) and/or a Slider (for volume).
    /// Automatically syncs visual state with <see cref="AudioManager"/> on enable.
    /// </summary>
    public class MuteToggle : MonoBehaviour
    {
        [Header("Mute Toggle")]
        [Tooltip("Optional UI Toggle for muting/unmuting all audio.")]
        [SerializeField] private Toggle muteToggle;

        [Tooltip("Icon shown when audio is unmuted (speaker with waves).")]
        [SerializeField] private GameObject unmutedIcon;

        [Tooltip("Icon shown when audio is muted (speaker with X).")]
        [SerializeField] private GameObject mutedIcon;

        [Header("SFX Volume")]
        [Tooltip("Optional slider to control SFX volume (0 – 1).")]
        [SerializeField] private Slider sfxVolumeSlider;

        [Header("Music Volume")]
        [Tooltip("Optional slider to control music/ambient volume (0 – 1).")]
        [SerializeField] private Slider musicVolumeSlider;

        [Header("Fallback Button")]
        [Tooltip("If no Toggle is assigned, you can use a plain Button to toggle mute on click.")]
        [SerializeField] private Button muteButton;

        private void OnEnable()
        {
            SyncFromManager();
            BindListeners();
        }

        private void OnDisable()
        {
            UnbindListeners();
        }

        // ── Binding ─────────────────────────────────────────────────────

        private void BindListeners()
        {
            if (muteToggle != null)
            {
                muteToggle.onValueChanged.AddListener(OnMuteToggleChanged);
            }

            if (muteButton != null)
            {
                muteButton.onClick.AddListener(OnMuteButtonClicked);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            }

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }
        }

        private void UnbindListeners()
        {
            if (muteToggle != null)
            {
                muteToggle.onValueChanged.RemoveListener(OnMuteToggleChanged);
            }

            if (muteButton != null)
            {
                muteButton.onClick.RemoveListener(OnMuteButtonClicked);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
            }

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
            }
        }

        // ── Event Handlers ──────────────────────────────────────────────

        /// <summary>
        /// Called when the UI Toggle value changes.
        /// Toggle.isOn == true means "sound ON" (unmuted).
        /// </summary>
        private void OnMuteToggleChanged(bool isOn)
        {
            if (AudioManager.Instance == null) return;

            // isOn == true → unmuted, isOn == false → muted
            AudioManager.Instance.IsMuted = !isOn;
            UpdateIcons(!isOn);
        }

        /// <summary>
        /// Called when the fallback Button is clicked. Toggles mute state.
        /// </summary>
        private void OnMuteButtonClicked()
        {
            if (AudioManager.Instance == null) return;

            bool newMuted = !AudioManager.Instance.IsMuted;
            AudioManager.Instance.IsMuted = newMuted;

            // Keep the Toggle in sync if both are assigned
            if (muteToggle != null)
            {
                muteToggle.SetIsOnWithoutNotify(!newMuted);
            }

            UpdateIcons(newMuted);

            // Play a click so the user hears feedback when unmuting
            if (!newMuted)
            {
                AudioManager.Instance.PlayButtonClick();
            }
        }

        /// <summary>
        /// Called when the SFX volume slider value changes.
        /// </summary>
        private void OnSfxVolumeChanged(float value)
        {
            if (AudioManager.Instance == null) return;
            AudioManager.Instance.SfxVolume = value;
        }

        /// <summary>
        /// Called when the music volume slider value changes.
        /// </summary>
        private void OnMusicVolumeChanged(float value)
        {
            if (AudioManager.Instance == null) return;
            AudioManager.Instance.MusicVolume = value;
        }

        // ── Sync & Visuals ──────────────────────────────────────────────

        /// <summary>
        /// Reads current state from <see cref="AudioManager"/> and updates all UI elements
        /// without triggering their change callbacks.
        /// </summary>
        private void SyncFromManager()
        {
            if (AudioManager.Instance == null)
            {
                Debug.LogWarning("[MuteToggle] AudioManager.Instance is null. " +
                                 "Make sure AudioManager exists in the scene and initializes before this component.");
                return;
            }

            bool muted = AudioManager.Instance.IsMuted;

            // Sync toggle without firing the callback
            if (muteToggle != null)
            {
                muteToggle.SetIsOnWithoutNotify(!muted);
            }

            // Sync SFX slider
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.minValue = 0f;
                sfxVolumeSlider.maxValue = 1f;
                sfxVolumeSlider.SetValueWithoutNotify(AudioManager.Instance.SfxVolume);
            }

            // Sync music slider
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.minValue = 0f;
                musicVolumeSlider.maxValue = 1f;
                musicVolumeSlider.SetValueWithoutNotify(AudioManager.Instance.MusicVolume);
            }

            UpdateIcons(muted);
        }

        /// <summary>
        /// Shows the correct speaker icon based on the muted state.
        /// </summary>
        private void UpdateIcons(bool muted)
        {
            if (unmutedIcon != null)
            {
                unmutedIcon.SetActive(!muted);
            }

            if (mutedIcon != null)
            {
                mutedIcon.SetActive(muted);
            }
        }
    }
}
