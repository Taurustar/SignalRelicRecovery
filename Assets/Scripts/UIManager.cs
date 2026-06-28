using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace SignalRelicRecovery
{
    /// <summary>
    /// Handles all UI panels: main menu, in-game HUD, announcements, and results.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private GameManager gameManager;
        [SerializeField] private AnnouncementManager announcementManager;
        [SerializeField] private StationFocusManager focusManager;
        [SerializeField] private InputReader inputReader;
        [SerializeField] private AccessibleMenuNavigator menuNavigator;

        [Header("Panels")]
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject resultsPanel;
        [SerializeField] private GameObject instructionsPanel;
        [SerializeField] private GameObject accessibilitySplashPanel;

        [Header("Menu UI")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button instructionsButton;
        [SerializeField] private Button listenInstructionsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Toggle accessibilityToggle;
        [SerializeField] private Text accessibilityHintText;

        [Header("HUD UI")]
        [SerializeField] private Text roundText;
        [SerializeField] private Text targetText;
        [SerializeField] private Text timerText;
        [SerializeField] private Text focusText;
        [SerializeField] private Text announcementText;

        [Header("Results UI")]
        [SerializeField] private Text resultsText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button menuButton;

        [Header("Instructions UI")]
        [SerializeField] private Button closeInstructionsButton;

        private bool _waitingForActivation;

        private void Start()
        {
            if (startButton != null)
                startButton.onClick.AddListener(OnStartClicked);
            if (instructionsButton != null)
                instructionsButton.onClick.AddListener(OnInstructionsClicked);
            if (listenInstructionsButton != null)
                listenInstructionsButton.onClick.AddListener(OnListenInstructionsClicked);
            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);
            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClicked);
            if (menuButton != null)
                menuButton.onClick.AddListener(OnReturnToMenuClicked);
            if (closeInstructionsButton != null)
                closeInstructionsButton.onClick.AddListener(OnCloseInstructionsClicked);
            if (accessibilityToggle != null)
            {
                accessibilityToggle.isOn = config.accessibilityModeDefault;
                accessibilityToggle.onValueChanged.AddListener(OnAccessibilityToggled);
            }

            if (gameManager != null)
            {
                gameManager.OnGameStarted += OnGameStarted;
                gameManager.OnRoundStarted += OnRoundStarted;
                gameManager.OnSelectionResult += OnSelectionResult;
                gameManager.OnGameCompleted += OnGameCompleted;
            }

            if (announcementManager != null)
                announcementManager.OnAnnouncementText += OnAnnouncement;

            if (inputReader != null)
            {
                inputReader.OnAnnounceTarget += OnRepeatTargetRequested;
                inputReader.OnRestart += OnRestartRequested;
                inputReader.OnReturnToMenu += OnReturnToMenuRequested;
                inputReader.OnPause += OnPauseRequested;
            }

            ApplyAccessibilityTextScale(config.accessibilityModeDefault);
            UpdateListenButtonVisibility(config.accessibilityModeDefault);

            if (config.showIntroSplash)
                ShowAccessibilitySplash();
            else
                ShowMenu();
        }

        private void Update()
        {
            if (_waitingForActivation && Keyboard.current != null)
            {
                var key = config.accessibilityActivationKey;
                if (Keyboard.current[key].wasPressedThisFrame)
                {
                    EnableAccessibilityFromSplash();
                }
            }

            if (gameManager != null && gameManager.CurrentState == GameState.Playing)
            {
                if (timerText != null)
                    timerText.text = $"Time: {gameManager.TotalElapsedTime:F1}s";

                if (focusText != null && gameManager.CurrentState == GameState.Playing)
                {
                    var focused = focusManager != null ? focusManager.FocusedStation : null;
                    focusText.text = focused != null
                        ? $"Focused: {focused.StationName}"
                        : "Focused: none";
                }
            }
        }

        private void OnDestroy()
        {
            if (gameManager != null)
            {
                gameManager.OnGameStarted -= OnGameStarted;
                gameManager.OnRoundStarted -= OnRoundStarted;
                gameManager.OnSelectionResult -= OnSelectionResult;
                gameManager.OnGameCompleted -= OnGameCompleted;
            }

            if (announcementManager != null)
                announcementManager.OnAnnouncementText -= OnAnnouncement;

            if (inputReader != null)
            {
                inputReader.OnAnnounceTarget -= OnRepeatTargetRequested;
                inputReader.OnRestart -= OnRestartRequested;
                inputReader.OnReturnToMenu -= OnReturnToMenuRequested;
                inputReader.OnPause -= OnPauseRequested;
            }
        }

        private bool AccessibilityModeActive => gameManager != null ? gameManager.AccessibilityMode : config.accessibilityModeDefault;

        private void ShowMenu()
        {
            SetPanel(menuPanel, true);
            SetPanel(hudPanel, false);
            SetPanel(resultsPanel, false);
            SetPanel(instructionsPanel, false);
            SetPanel(accessibilitySplashPanel, false);
            _waitingForActivation = false;

            if (AccessibilityModeActive)
                menuNavigator?.OpenPanel(startButton, config.menuContextClip, "Main menu.");
            else
                ClearUISelection();
        }

        private void ShowAccessibilitySplash()
        {
            SetPanel(menuPanel, false);
            SetPanel(hudPanel, false);
            SetPanel(resultsPanel, false);
            SetPanel(instructionsPanel, false);
            SetPanel(accessibilitySplashPanel, true);
            ClearUISelection();
            _waitingForActivation = true;

            if (accessibilityHintText != null)
                accessibilityHintText.text = $"Press {config.accessibilityActivationKey} to enable accessibility mode.";

            if (announcementManager != null && config.introVoiceClip != null)
                announcementManager.Announce(string.Empty, config.introVoiceClip);
        }

        private void EnableAccessibilityFromSplash()
        {
            _waitingForActivation = false;

            if (gameManager != null)
                gameManager.AccessibilityMode = true;

            if (accessibilityToggle != null)
                accessibilityToggle.isOn = true;

            ApplyAccessibilityTextScale(true);
            UpdateListenButtonVisibility(true);

            if (announcementManager != null)
                announcementManager.Announce("Accessibility mode enabled.", config.accessibilityEnabledClip);

            ShowMenu();
        }

        private void ShowHud()
        {
            SetPanel(menuPanel, false);
            SetPanel(hudPanel, true);
            SetPanel(resultsPanel, false);
            SetPanel(instructionsPanel, false);
            ClearUISelection();
        }

        private static void ClearUISelection()
        {
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);
        }

        private void ShowResults()
        {
            SetPanel(menuPanel, false);
            SetPanel(hudPanel, false);
            SetPanel(resultsPanel, true);
            SetPanel(instructionsPanel, false);

            if (resultsText != null && gameManager != null)
            {
                resultsText.text =
                    $"Training Complete!\n\n" +
                    $"Total Attempts: {gameManager.TotalAttempts}\n" +
                    $"Correct: {gameManager.CorrectSelections}\n" +
                    $"Wrong: {gameManager.WrongSelections}\n" +
                    $"Time: {gameManager.TotalElapsedTime:F1}s\n\n" +
                    $"Result: {(gameManager.CorrectSelections >= config.TotalRounds ? "Passed" : "Review Needed")}";
            }

            if (AccessibilityModeActive)
                menuNavigator?.OpenPanel(restartButton, config.resultsContextClip, "Training complete.");
            else
                ClearUISelection();
        }

        private void SetPanel(GameObject panel, bool active)
        {
            if (panel != null) panel.SetActive(active);
        }

        private void OnStartClicked()
        {
            gameManager?.StartGame();
        }

        private void OnInstructionsClicked()
        {
            SetPanel(menuPanel, false);
            SetPanel(instructionsPanel, true);

            if (AccessibilityModeActive)
                menuNavigator?.OpenPanel(closeInstructionsButton, config.instructionsContextClip, "How to Play.");
            else
                ClearUISelection();
        }

        private void OnCloseInstructionsClicked()
        {
            SetPanel(instructionsPanel, false);
            ShowMenu();
        }

        private void OnListenInstructionsClicked()
        {
            if (announcementManager == null || config.instructionsVoiceClip == null)
                return;

            SetMenuButtonsInteractable(false);
            announcementManager.Announce(string.Empty, config.instructionsVoiceClip, () =>
            {
                SetMenuButtonsInteractable(true);
                if (AccessibilityModeActive)
                    menuNavigator?.OpenPanel(startButton, null, null);
                else
                    ClearUISelection();
            });
        }

        private void OnRepeatTargetRequested()
        {
            gameManager?.RepeatTargetAnnouncement();
        }

        private void OnRestartRequested()
        {
            if (gameManager == null) return;
            if (gameManager.CurrentState == GameState.Results || gameManager.CurrentState == GameState.Menu)
                OnRestartClicked();
        }

        private void OnReturnToMenuRequested()
        {
            if (gameManager == null) return;
            if (gameManager.CurrentState == GameState.Playing ||
                gameManager.CurrentState == GameState.Results ||
                gameManager.CurrentState == GameState.RoundFeedback)
            {
                OnReturnToMenuClicked();
            }
        }

        private void OnPauseRequested()
        {
            if (gameManager == null) return;
            if (gameManager.CurrentState == GameState.Playing ||
                gameManager.CurrentState == GameState.Results ||
                gameManager.CurrentState == GameState.RoundFeedback)
            {
                OnReturnToMenuClicked();
            }
        }

        private void OnQuitClicked()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            // WebGL cannot quit; reload the page instead.
            Application.OpenURL(Application.absoluteURL);
#else
            Application.Quit();
#endif
        }

        private void OnRestartClicked()
        {
            gameManager?.StartGame();
        }

        private void OnReturnToMenuClicked()
        {
            gameManager?.ReturnToMenu();
            ShowMenu();
        }

        private void OnAccessibilityToggled(bool value)
        {
            if (gameManager != null)
                gameManager.AccessibilityMode = value;

            ApplyAccessibilityTextScale(value);
            UpdateListenButtonVisibility(value);
        }

        private void UpdateListenButtonVisibility(bool accessible)
        {
            if (listenInstructionsButton != null)
                listenInstructionsButton.gameObject.SetActive(accessible);
        }

        private void SetMenuButtonsInteractable(bool interactable)
        {
            if (startButton != null) startButton.interactable = interactable;
            if (instructionsButton != null) instructionsButton.interactable = interactable;
            if (listenInstructionsButton != null) listenInstructionsButton.interactable = interactable;
            if (quitButton != null) quitButton.interactable = interactable;
            if (accessibilityToggle != null) accessibilityToggle.interactable = interactable;
        }

        private void ApplyAccessibilityTextScale(bool enabled)
        {
            float scale = enabled ? config.accessibilityTextScale : 1f;
            var texts = FindObjectsByType<Text>();
            foreach (var text in texts)
            {
                text.transform.localScale = new Vector3(scale, scale, 1f);
            }
        }

        private void OnGameStarted()
        {
            ShowHud();

            if (AccessibilityModeActive && announcementManager != null && config.gameplayMenuHintClip != null)
                announcementManager.Announce("Press Escape to return to the main menu.", config.gameplayMenuHintClip);
        }

        private void OnRoundStarted()
        {
            if (roundText != null)
                roundText.text = $"Round {gameManager.CurrentRound + 1} / {config.TotalRounds}";

            if (targetText != null && gameManager.CurrentTarget != null)
                targetText.text = $"Target: {gameManager.CurrentTarget.SoundDescriptor}";
        }

        private void OnSelectionResult(bool correct, string message)
        {
            // Handled by announcement text.
        }

        private void OnGameCompleted()
        {
            ShowResults();
        }

        private void OnAnnouncement(string text)
        {
            if (announcementText != null)
                announcementText.text = text;
        }
    }
}
