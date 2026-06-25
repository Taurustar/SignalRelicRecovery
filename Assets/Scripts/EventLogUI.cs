using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace SignalRelicRecovery
{
    /// <summary>
    /// Displays gameplay events in a scrollable UI panel. Toggled via the HUD button.
    /// </summary>
    public class EventLogUI : MonoBehaviour
    {
        [SerializeField] private GameplayEventLogger eventLogger;
        [SerializeField] private GameObject logPanel;
        [SerializeField] private Text logText;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Button toggleButton;

        private readonly StringBuilder _stringBuilder = new();

        private void Start()
        {
            if (logPanel != null)
                logPanel.SetActive(false);

            if (toggleButton != null)
                toggleButton.onClick.AddListener(ToggleLog);

            if (eventLogger != null)
            {
                eventLogger.OnEventLogged += OnEventLogged;
                RebuildText();
            }
        }

        private void OnDestroy()
        {
            if (toggleButton != null)
                toggleButton.onClick.RemoveListener(ToggleLog);

            if (eventLogger != null)
                eventLogger.OnEventLogged -= OnEventLogged;
        }

        private void ToggleLog()
        {
            if (logPanel != null)
                logPanel.SetActive(!logPanel.activeSelf);
        }

        private void OnEventLogged(EventEntry entry)
        {
            RebuildText();
            ScrollToBottom();
        }

        private void RebuildText()
        {
            if (logText == null || eventLogger == null) return;

            _stringBuilder.Clear();
            foreach (var entry in eventLogger.Events)
            {
                _stringBuilder.AppendLine(entry.ToString());
            }

            logText.text = _stringBuilder.ToString();
        }

        private void ScrollToBottom()
        {
            if (scrollRect != null)
                scrollRect.normalizedPosition = new Vector2(0, 0);
        }
    }
}
