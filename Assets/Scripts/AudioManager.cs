using UnityEngine;

namespace SignalRelicRecovery
{
    /// <summary>
    /// Central place for one-shot sound effects and ambient audio.
    /// Keeps gameplay code from juggling AudioSource references.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private AudioSource sfxSource;

        [Header("SFX Clips")]
        [SerializeField] private AudioClip correctClip;
        [SerializeField] private AudioClip wrongClip;
        [SerializeField] private AudioClip focusChangeClip;
        [SerializeField] private AudioClip roundStartClip;
        [SerializeField] private AudioClip resultsClip;

        public static AudioManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void PlayCorrect() => PlayOneShot(correctClip);
        public void PlayWrong() => PlayOneShot(wrongClip);
        public void PlayFocusChange() => PlayOneShot(focusChangeClip);
        public void PlayRoundStart() => PlayOneShot(roundStartClip);
        public void PlayResults() => PlayOneShot(resultsClip);

        private void PlayOneShot(AudioClip clip)
        {
            if (clip == null || sfxSource == null) return;
            sfxSource.PlayOneShot(clip, config.sfxVolume);
        }
    }
}
