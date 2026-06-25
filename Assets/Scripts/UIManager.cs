using UnityEngine;
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

        [Header("Panels")]
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject resultsPanel;
        [SerializeField] private GameObject instructionsPanel;

        [Header("Menu UI")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button instructionsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Toggle accessibilityToggle;

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

        private void Start()
        {
            ShowMenu();

            if (startButton != null)
                startButton.onClick.AddListener(OnStartClicked);
            if (instructionsButton != null)
                instructionsButton.onClick.AddListener(OnInstructionsClicked);
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
        }

        private void Update()
        {
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
        }

        private void ShowMenu()
        {
            SetPanel(menuPanel, true);
            SetPanel(hudPanel, false);
            SetPanel(resultsPanel, false);
            SetPanel(instructionsPanel, false);
        }

        private void ShowHud()
        {
            SetPanel(menuPanel, false);
            SetPanel(hudPanel, true);
            SetPanel(resultsPanel, false);
            SetPanel(instructionsPanel, false);
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
        }

        private void OnCloseInstructionsClicked()
        {
            SetPanel(instructionsPanel, false);
            SetPanel(menuPanel, true);
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
