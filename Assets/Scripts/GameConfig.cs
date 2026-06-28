using UnityEngine;
using UnityEngine.InputSystem;

namespace SignalRelicRecovery
{
    /// <summary>
    /// Tunable configuration for the prototype. Designers can change values here
    /// without touching code.
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Signal Relic Recovery/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Rounds")]
        [Tooltip("Number of relic stations available in each round. Array length = total rounds.")]
        public int[] relicsPerRound = { 3, 4, 5 };

        [Tooltip("Time between rounds (seconds).")]
        public float roundTransitionDelay = 1.5f;

        [Tooltip("Delay before accepting input at the start of a round.")]
        public float roundStartDelay = 1.0f;

        [Header("Input")]
        [Tooltip("Minimum time between focus changes.")]
        public float focusMoveCooldown = 0.25f;

        [Tooltip("Cooldown after selecting a station.")]
        public float selectionCooldown = 1.0f;

        [Header("Audio")]
        [Tooltip("Volume of the currently focused station.")]
        [Range(0f, 1f)] public float focusedStationVolume = 1f;

        [Tooltip("Volume of unfocused stations.")]
        [Range(0f, 1f)] public float unfocusedStationVolume = 0.35f;

        [Tooltip("Master volume for one-shot SFX.")]
        [Range(0f, 1f)] public float sfxVolume = 0.8f;

        [Tooltip("Master volume for announcements.")]
        [Range(0f, 1f)] public float announcementVolume = 1f;

        [Header("Accessibility")]
        [Tooltip("Default value for accessibility mode on first launch.")]
        public bool accessibilityModeDefault = false;

        [Tooltip("When enabled, announcements stay on screen longer.")]
        public float accessibilityAnnouncementDuration = 4f;

        [Tooltip("Standard announcement duration.")]
        public float standardAnnouncementDuration = 2.5f;

        [Tooltip("Larger text scale used in accessibility mode.")]
        public float accessibilityTextScale = 1.3f;

        [Tooltip("Keyboard key that turns on accessibility mode from the intro splash.")]
        public Key accessibilityActivationKey = Key.Space;

        [Tooltip("Keyboard key that repeats the current target descriptor during accessibility gameplay.")]
        public Key repeatTargetKey = Key.T;

        [Tooltip("Show the accessibility intro splash on startup.")]
        public bool showIntroSplash = true;

        [Tooltip("Voiced introduction played on the intro splash.")]
        public AudioClip introVoiceClip;

        [Tooltip("Voiced instructions played by the Listen to Instructions button.")]
        public AudioClip instructionsVoiceClip;

        [Tooltip("Voiced clips for each target descriptor, parallel to the station definitions list.")]
        public AudioClip[] targetDescriptorClips;

        [Header("Accessible Menu Voice")]
        [Tooltip("Clip played when the main menu opens.")]
        public AudioClip menuContextClip;

        [Tooltip("Clip played when the results panel opens.")]
        public AudioClip resultsContextClip;

        [Tooltip("Clip played when the instructions panel opens.")]
        public AudioClip instructionsContextClip;

        [Tooltip("Clip played at game start reminding how to return to the menu.")]
        public AudioClip gameplayMenuHintClip;

        [Tooltip("Clip played when accessibility mode is enabled from the intro splash.")]
        public AudioClip accessibilityEnabledClip;

        [Header("Gameplay")]
        [Tooltip("If true, the player can keep trying after a wrong selection.")]
        public bool allowRetryAfterWrongSelection = true;

        public int TotalRounds => relicsPerRound.Length;

        public float AnnouncementDuration(bool accessibilityMode)
        {
            return accessibilityMode ? accessibilityAnnouncementDuration : standardAnnouncementDuration;
        }
    }
}
