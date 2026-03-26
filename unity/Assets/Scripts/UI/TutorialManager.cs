using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RuneRealms.Core;
using RuneRealms.Data;

namespace RuneRealms.UI
{
    /// <summary>
    /// Manages a multi-step tutorial overlay for new players.
    /// Attach to a dedicated Canvas/Panel in the scene.
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        [Serializable]
        public struct TutorialStep
        {
            public string title;
            [TextArea(2, 4)]
            public string description;
            public string buttonText;
            [Tooltip("Optional — highlight this RectTransform during the step")]
            public RectTransform highlightTarget;
        }

        [Header("UI References")]
        [SerializeField] private CanvasGroup overlayCanvasGroup;
        [SerializeField] private Image darkOverlay;
        [SerializeField] private RectTransform dialogPanel;
        [SerializeField] private Text titleLabel;
        [SerializeField] private Text descriptionLabel;
        [SerializeField] private Text stepCounterLabel;
        [SerializeField] private Button nextButton;
        [SerializeField] private Text nextButtonLabel;
        [SerializeField] private Button skipButton;

        [Header("Highlight")]
        [SerializeField] private RectTransform highlightFrame;

        [Header("Animation")]
        [SerializeField] private float fadeDuration = 0.3f;

        [Header("Steps (auto-populated if empty)")]
        [SerializeField] private List<TutorialStep> steps = new List<TutorialStep>();

        private int currentStepIndex;
        private TutorialProgress tutorialProgress;
        private Coroutine fadeCoroutine;
        private bool isShowing;

        /// <summary>Fired when the tutorial finishes or is skipped.</summary>
        public event Action OnTutorialCompleted;

        private void Awake()
        {
            if (steps == null || steps.Count == 0)
            {
                PopulateDefaultSteps();
            }

            // Wire buttons
            if (nextButton != null)
                nextButton.onClick.AddListener(OnStepComplete);

            if (skipButton != null)
                skipButton.onClick.AddListener(SkipTutorial);

            // Start hidden
            if (overlayCanvasGroup != null)
            {
                overlayCanvasGroup.alpha = 0f;
                overlayCanvasGroup.interactable = false;
                overlayCanvasGroup.blocksRaycasts = false;
            }

            if (highlightFrame != null)
                highlightFrame.gameObject.SetActive(false);
        }

        private void PopulateDefaultSteps()
        {
            steps = new List<TutorialStep>
            {
                new TutorialStep
                {
                    title = "Welcome to Rune Realms!",
                    description = "Embark on an idle adventure — train skills, fight monsters, and climb the leaderboards. Let's show you around!",
                    buttonText = "Next",
                    highlightTarget = null
                },
                new TutorialStep
                {
                    title = "Train Your Skills",
                    description = "Tap a skill to start training. Each skill earns XP over time and levels up automatically.",
                    buttonText = "Next",
                    highlightTarget = null // Assign the Skills tab RectTransform in the Inspector
                },
                new TutorialStep
                {
                    title = "Idle & Offline Gains",
                    description = "Skills level up over time — even while you're away! Come back later to collect your offline XP.",
                    buttonText = "Next",
                    highlightTarget = null
                },
                new TutorialStep
                {
                    title = "Enter the Arena",
                    description = "Visit the Arena to fight monsters and earn loot. The further you go, the better the rewards!",
                    buttonText = "Next",
                    highlightTarget = null // Assign the Arena tab RectTransform in the Inspector
                },
                new TutorialStep
                {
                    title = "Your Inventory",
                    description = "Check your Inventory for items and equipment you've collected from the Arena.",
                    buttonText = "Next",
                    highlightTarget = null // Assign the Inventory tab RectTransform in the Inspector
                },
                new TutorialStep
                {
                    title = "Leaderboards",
                    description = "Compete on the Leaderboard! See how your total level, XP, and arena kills stack up against other players.",
                    buttonText = "Next",
                    highlightTarget = null // Assign the Leaderboard tab RectTransform in the Inspector
                },
                new TutorialStep
                {
                    title = "You're Ready!",
                    description = "That's everything you need to know. Start your adventure and become the greatest in the realm!",
                    buttonText = "Let's go!",
                    highlightTarget = null
                }
            };
        }

        // -----------------------------------------------------------------
        // Public API
        // -----------------------------------------------------------------

        /// <summary>
        /// Shows the tutorial starting (or resuming) from the given progress.
        /// </summary>
        public void Show(TutorialProgress progress)
        {
            if (progress == null)
            {
                progress = new TutorialProgress
                {
                    completed = false,
                    currentStep = 0,
                    completedAt = -1
                };
            }

            // Already completed — nothing to show
            if (progress.completed)
            {
                Debug.Log("[TutorialManager] Tutorial already completed, skipping.");
                return;
            }

            tutorialProgress = progress;
            currentStepIndex = Mathf.Clamp(progress.currentStep, 0, steps.Count - 1);
            isShowing = true;

            DisplayStep(currentStepIndex);
            FadeIn();
        }

        /// <summary>
        /// Advances to the next step. Called by the "Next" / "Got it!" button.
        /// </summary>
        public void OnStepComplete()
        {
            if (!isShowing) return;

            currentStepIndex++;

            if (currentStepIndex >= steps.Count)
            {
                CompleteTutorial();
                return;
            }

            DisplayStep(currentStepIndex);
            SaveProgress(false);
        }

        /// <summary>
        /// Immediately marks the tutorial as complete and closes the overlay.
        /// </summary>
        public void SkipTutorial()
        {
            Debug.Log("[TutorialManager] Tutorial skipped by player.");
            CompleteTutorial();
        }

        // -----------------------------------------------------------------
        // Internal helpers
        // -----------------------------------------------------------------

        private void DisplayStep(int index)
        {
            if (index < 0 || index >= steps.Count) return;

            var step = steps[index];

            if (titleLabel != null)
                titleLabel.text = step.title;

            if (descriptionLabel != null)
                descriptionLabel.text = step.description;

            if (nextButtonLabel != null)
                nextButtonLabel.text = step.buttonText;

            if (stepCounterLabel != null)
                stepCounterLabel.text = $"{index + 1} / {steps.Count}";

            // Highlight target
            if (highlightFrame != null)
            {
                if (step.highlightTarget != null)
                {
                    highlightFrame.gameObject.SetActive(true);
                    PositionHighlight(step.highlightTarget);
                }
                else
                {
                    highlightFrame.gameObject.SetActive(false);
                }
            }

            Debug.Log($"[TutorialManager] Step {index + 1}/{steps.Count}: {step.title}");
        }

        private void PositionHighlight(RectTransform target)
        {
            if (highlightFrame == null || target == null) return;

            // Parent the highlight frame under the overlay so it renders on top,
            // then match it to the target's world-space rect.
            highlightFrame.position = target.position;
            highlightFrame.sizeDelta = target.sizeDelta * 1.15f; // slight padding
        }

        private void CompleteTutorial()
        {
            isShowing = false;
            SaveProgress(true);
            FadeOut();
            OnTutorialCompleted?.Invoke();

            Debug.Log("[TutorialManager] Tutorial completed!");
        }

        private void SaveProgress(bool completed)
        {
            if (tutorialProgress == null)
            {
                tutorialProgress = new TutorialProgress();
            }

            tutorialProgress.completed = completed;
            tutorialProgress.currentStep = currentStepIndex;

            if (completed)
            {
                tutorialProgress.completedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }

            // Persist via DevvitBridge
            if (DevvitBridge.Instance != null)
            {
                DevvitBridge.Instance.SaveTutorial(tutorialProgress);
            }
            else
            {
                Debug.LogWarning("[TutorialManager] DevvitBridge not found — tutorial progress not saved to server.");
            }
        }

        // -----------------------------------------------------------------
        // Fade animation
        // -----------------------------------------------------------------

        private void FadeIn()
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            fadeCoroutine = StartCoroutine(FadeCanvasGroup(0f, 1f, true));
        }

        private void FadeOut()
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            fadeCoroutine = StartCoroutine(FadeCanvasGroup(1f, 0f, false));
        }

        private IEnumerator FadeCanvasGroup(float from, float to, bool interactableAfter)
        {
            if (overlayCanvasGroup == null) yield break;

            // Make sure raycasts block during the fade-in
            if (interactableAfter)
            {
                overlayCanvasGroup.blocksRaycasts = true;
            }

            float elapsed = 0f;
            overlayCanvasGroup.alpha = from;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                overlayCanvasGroup.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }

            overlayCanvasGroup.alpha = to;
            overlayCanvasGroup.interactable = interactableAfter;
            overlayCanvasGroup.blocksRaycasts = interactableAfter;

            fadeCoroutine = null;
        }
    }
}
