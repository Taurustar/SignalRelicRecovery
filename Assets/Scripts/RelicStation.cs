using System.Collections;
using UnityEngine;
using TMPro;

namespace SignalRelicRecovery
{
    /// <summary>
    /// Represents one relic station in the training area.
    /// Each station has a unique sound, shape, label, and descriptor.
    /// </summary>
    public class RelicStation : MonoBehaviour
    {
        [Header("Identity")]
        [Tooltip("Human-readable name announced to the player.")]
        public string stationName = "Station";

        [Tooltip("Short descriptor used in target conditions, e.g. 'buzzing', 'humming'.")]
        public string soundDescriptor = "humming";

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;

        [Header("Visuals")]
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Renderer highlightRenderer;
        [SerializeField] private TextMeshPro labelText;
        [SerializeField] private ParticleSystem correctParticles;
        [SerializeField] private ParticleSystem wrongParticles;

        [Header("Focus Feedback")]
        [SerializeField] private float focusScaleMultiplier = 1.15f;
        [SerializeField] private float focusAnimationSpeed = 5f;

        private Vector3 _baseScale;
        private bool _isFocused;
        private Color _originalEmission;
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        // Intermittent sound mode for higher difficulty rounds.
        private bool _intermittentMode;
        private float _intermittentOnDuration = 0.8f;
        private float _intermittentOffDuration = 1.6f;
        private Coroutine _intermittentCoroutine;
        private float _currentTargetVolume;

        public string StationName => stationName;
        public string SoundDescriptor => soundDescriptor;
        public AudioSource AudioSource => audioSource;

        private void Awake()
        {
            if (visualRoot != null)
                _baseScale = visualRoot.localScale;
            else
                _baseScale = transform.localScale;

            if (highlightRenderer != null && highlightRenderer.material.HasProperty(EmissionColor))
                _originalEmission = highlightRenderer.material.GetColor(EmissionColor);
        }

        private void Update()
        {
            AnimateFocus();
        }

        public void Configure(string name, string descriptor, AudioClip clip)
        {
            stationName = name;
            soundDescriptor = descriptor;

            if (audioSource != null)
            {
                audioSource.clip = clip;
                audioSource.loop = true;
                audioSource.playOnAwake = false;
                audioSource.enabled = true;
            }

            if (labelText != null)
                labelText.text = name;
        }

        public void SetFocus(bool focused, float targetVolume)
        {
            _isFocused = focused;
            _currentTargetVolume = targetVolume;

            if (audioSource != null && audioSource.enabled && !_intermittentMode)
            {
                audioSource.volume = targetVolume;
                if (!audioSource.isPlaying && audioSource.clip != null)
                    audioSource.Play();
            }

            if (highlightRenderer != null && highlightRenderer.material.HasProperty(EmissionColor))
            {
                highlightRenderer.material.SetColor(EmissionColor,
                    focused ? Color.white * 1.5f : _originalEmission);
            }
        }

        /// <summary>
        /// Enables intermittent playback for harder rounds. The station sound
        /// plays in short bursts, forcing the player to listen and remember.
        /// </summary>
        public void SetIntermittentMode(bool enabled, float onDuration, float offDuration)
        {
            _intermittentMode = enabled;
            _intermittentOnDuration = onDuration;
            _intermittentOffDuration = offDuration;

            if (_intermittentCoroutine != null)
            {
                StopCoroutine(_intermittentCoroutine);
                _intermittentCoroutine = null;
            }

            if (enabled && audioSource != null && audioSource.enabled)
            {
                _intermittentCoroutine = StartCoroutine(IntermittentPlayback());
            }
        }

        private IEnumerator IntermittentPlayback()
        {
            while (_intermittentMode && audioSource != null)
            {
                audioSource.volume = _currentTargetVolume;
                if (audioSource.clip != null)
                    audioSource.Play();

                yield return new WaitForSeconds(_intermittentOnDuration);

                if (audioSource != null)
                    audioSource.Stop();

                yield return new WaitForSeconds(_intermittentOffDuration);
            }
        }

        public void PlayCorrectFeedback()
        {
            if (correctParticles != null) correctParticles.Play();
        }

        public void PlayWrongFeedback()
        {
            if (wrongParticles != null) wrongParticles.Play();
        }

        private void AnimateFocus()
        {
            Vector3 targetScale = _isFocused ? _baseScale * focusScaleMultiplier : _baseScale;
            Transform target = visualRoot != null ? visualRoot : transform;
            target.localScale = Vector3.Lerp(target.localScale, targetScale, Time.deltaTime * focusAnimationSpeed);
        }

        private void OnValidate()
        {
            if (audioSource == null) audioSource = GetComponentInChildren<AudioSource>();
            if (labelText == null) labelText = GetComponentInChildren<TextMeshPro>();
        }
    }
}
