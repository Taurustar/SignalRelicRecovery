using System;
using System.Collections;
using UnityEngine;

namespace SignalRelicRecovery
{
    /// <summary>
    /// Handles on-screen text announcements and optional spoken audio announcements.
    /// Critical for the accessibility path.
    /// </summary>
    public class AnnouncementManager : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private AudioSource voiceSource;

        public static AnnouncementManager Instance { get; private set; }

        public event Action<string> OnAnnouncementText;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// Announces a message via text and optional audio.
        /// </summary>
        public void Announce(string text, AudioClip audioClip = null, bool accessibilityMode = false)
        {
            OnAnnouncementText?.Invoke(text);

            if (audioClip != null && voiceSource != null)
            {
                voiceSource.Stop();
                voiceSource.clip = audioClip;
                voiceSource.volume = config.announcementVolume;
                voiceSource.Play();
            }
        }

        /// <summary>
        /// Announces with a fixed duration (used for non-voiced text that should linger).
        /// </summary>
        public void AnnounceTimed(string text, float duration)
        {
            OnAnnouncementText?.Invoke(text);
            StopAllCoroutines();
            StartCoroutine(ClearAfter(duration));
        }

        private IEnumerator ClearAfter(float duration)
        {
            yield return new WaitForSeconds(duration);
            OnAnnouncementText?.Invoke(string.Empty);
        }
    }
}
