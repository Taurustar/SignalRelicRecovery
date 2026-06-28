using UnityEngine;
using UnityEngine.UI;

namespace SignalRelicRecovery
{
    /// <summary>
    /// Describes a UI selectable for the accessibility menu navigator.
    /// Attach to buttons, toggles, or any other selectable that should be announced.
    /// </summary>
    public class AccessibleMenuItem : MonoBehaviour
    {
        [Tooltip("Spoken label. If empty, the first Text component on this object or its children is used.")]
        [SerializeField] private string label;

        [Tooltip("Spoken hint, e.g. what key activates this item.")]
        [SerializeField] private string hint = "Press Enter to select.";

        [Tooltip("Optional voice clip played when this item receives focus.")]
        [SerializeField] private AudioClip announcementClip;

        public string Label => string.IsNullOrEmpty(label) ? ReadLabelFromText() : label;
        public string Hint => hint;
        public AudioClip AnnouncementClip => announcementClip;

        private string ReadLabelFromText()
        {
            var text = GetComponentInChildren<Text>();
            return text != null ? text.text : gameObject.name;
        }

        public string GetAnnouncementText()
        {
            string full = Label;
            if (!string.IsNullOrEmpty(Hint))
                full += ". " + Hint;
            return full;
        }
    }
}
