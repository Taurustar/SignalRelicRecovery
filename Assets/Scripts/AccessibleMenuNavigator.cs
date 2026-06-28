using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SignalRelicRecovery
{
    /// <summary>
    /// Watches the EventSystem selection and announces focused UI items for accessibility.
    /// Also plays a context clip when a new panel is opened.
    /// </summary>
    public class AccessibleMenuNavigator : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private AnnouncementManager announcementManager;

        private GameObject _lastSelected;
        private Selectable _pendingFirstSelectable;

        private bool IsAccessible => gameManager != null && gameManager.AccessibilityMode;

        private void Update()
        {
            if (!IsAccessible)
                return;

            var current = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            if (current != _lastSelected)
            {
                _lastSelected = current;
                if (current != null)
                    AnnounceSelection(current);
            }
        }

        /// <summary>
        /// Plays a panel context clip and then selects the first item so it is announced.
        /// If accessibility mode is off, only the first item is selected.
        /// </summary>
        public void OpenPanel(Selectable firstSelectable, AudioClip contextClip, string contextText)
        {
            _pendingFirstSelectable = firstSelectable;

            if (!IsAccessible || announcementManager == null)
            {
                SelectFirst();
                return;
            }

            if (contextClip != null)
            {
                announcementManager.Announce(contextText ?? string.Empty, contextClip, () =>
                {
                    SelectFirst();
                });
            }
            else
            {
                SelectFirst();
            }
        }

        private void SelectFirst()
        {
            if (_pendingFirstSelectable != null && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(_pendingFirstSelectable.gameObject);
                _lastSelected = _pendingFirstSelectable.gameObject;
            }
            _pendingFirstSelectable = null;
        }

        private void AnnounceSelection(GameObject selected)
        {
            if (announcementManager == null || selected == null)
                return;

            var item = selected.GetComponent<AccessibleMenuItem>();
            if (item == null)
            {
                // Fall back to any text on the selected object.
                var text = selected.GetComponentInChildren<Text>();
                if (text != null)
                    announcementManager.Announce(text.text);
                return;
            }

            string announcement = item.GetAnnouncementText();
            if (item.AnnouncementClip != null)
                announcementManager.Announce(announcement, item.AnnouncementClip);
            else
                announcementManager.Announce(announcement);
        }

        /// <summary>
        /// Announces arbitrary text through the configured announcement manager.
        /// </summary>
        public void Announce(string text)
        {
            announcementManager?.Announce(text);
        }
    }
}
