using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SignalRelicRecovery
{
    /// <summary>
    /// High-level game flow: menu, rounds, scoring, timer, and results.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private StationFocusManager focusManager;
        [SerializeField] private GameplayEventLogger eventLogger;
        [SerializeField] private Transform[] stationSpawnPoints;
        [SerializeField] private RelicStation relicStationPrefab;

        [Header("Station Definitions")]
        [SerializeField] private StationDefinition[] stationDefinitions;

        public static GameManager Instance { get; private set; }

        public GameState CurrentState { get; private set; } = GameState.Menu;
        public int CurrentRound { get; private set; } = 0;
        public int CorrectSelections { get; private set; } = 0;
        public int WrongSelections { get; private set; } = 0;
        public int TotalAttempts => CorrectSelections + WrongSelections;
        public float RoundStartTime { get; private set; }
        public float TotalElapsedTime { get; private set; }
        public bool AccessibilityMode { get; set; }
        public RelicStation CurrentTarget { get; private set; }

        public event Action OnGameStarted;
        public event Action OnRoundStarted;
        public event Action OnRoundCompleted;
        public event Action OnGameCompleted;
        public event Action<bool, string> OnSelectionResult; // correct, message

        private readonly List<RelicStation> _activeStations = new();
        private Coroutine _roundTimerCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            AccessibilityMode = config.accessibilityModeDefault;
        }

        private void OnEnable()
        {
            if (focusManager != null)
                focusManager.OnStationSelected += HandleStationSelected;
        }

        private void OnDisable()
        {
            if (focusManager != null)
                focusManager.OnStationSelected -= HandleStationSelected;
        }

        public void StartGame()
        {
            CurrentRound = 0;
            CorrectSelections = 0;
            WrongSelections = 0;
            TotalElapsedTime = 0f;
            CurrentState = GameState.Playing;

            eventLogger?.Clear();
            eventLogger?.Log("Game started.");

            OnGameStarted?.Invoke();
            AudioManager.Instance?.PlayRoundStart();
            StartCoroutine(BeginRoundAfterDelay(config.roundStartDelay));
        }

        public void ReturnToMenu()
        {
            CurrentState = GameState.Menu;
            CleanupStations();
            if (_roundTimerCoroutine != null) StopCoroutine(_roundTimerCoroutine);
        }

        private IEnumerator BeginRoundAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            BeginRound();
        }

        private void BeginRound()
        {
            CleanupStations();

            int stationCount = config.relicsPerRound[Mathf.Min(CurrentRound, config.relicsPerRound.Length - 1)];
            var definitions = PickRandomDefinitions(stationCount);

            for (int i = 0; i < definitions.Count; i++)
            {
                var spawnPoint = stationSpawnPoints[i % stationSpawnPoints.Length];
                var station = Instantiate(relicStationPrefab, spawnPoint.position, spawnPoint.rotation);
                var def = definitions[i];
                station.Configure(def.stationName, def.soundDescriptor, def.audioClip);

                // Round 3: intermittent sounds to test memory and timing.
                if (CurrentRound >= 2)
                    station.SetIntermittentMode(true, 0.8f, 1.6f);

                _activeStations.Add(station);
            }

            CurrentTarget = _activeStations[Random.Range(0, _activeStations.Count)];
            focusManager.SetStations(_activeStations);

            RoundStartTime = Time.time;
            _roundTimerCoroutine = StartCoroutine(RunRoundTimer());

            string targetText = BuildTargetAnnouncement(CurrentTarget);
            AnnouncementManager.Instance?.AnnounceTimed(targetText,
                config.AnnouncementDuration(AccessibilityMode));

            eventLogger?.LogRoundStarted(CurrentRound + 1, config.TotalRounds, CurrentTarget.SoundDescriptor);

            OnRoundStarted?.Invoke();
        }

        private IEnumerator RunRoundTimer()
        {
            while (CurrentState == GameState.Playing)
            {
                TotalElapsedTime += Time.deltaTime;
                yield return null;
            }
        }

        private void HandleStationSelected(RelicStation station)
        {
            if (CurrentState != GameState.Playing) return;

            float timeToSelection = Time.time - RoundStartTime;

            if (station == CurrentTarget)
            {
                CorrectSelections++;
                station.PlayCorrectFeedback();
                AudioManager.Instance?.PlayCorrect();

                string message = AccessibilityMode
                    ? $"Correct. {station.StationName} matches the target."
                    : "Correct!";
                AnnouncementManager.Instance?.AnnounceTimed(message,
                    config.AnnouncementDuration(AccessibilityMode));

                eventLogger?.LogStationSelected(station.StationName, true, timeToSelection);
                OnSelectionResult?.Invoke(true, message);
                StartCoroutine(CompleteRoundAfterDelay(config.roundTransitionDelay));
            }
            else
            {
                WrongSelections++;
                station.PlayWrongFeedback();
                AudioManager.Instance?.PlayWrong();

                string message = AccessibilityMode
                    ? $"Wrong station. That was {station.StationName}, not the target."
                    : "Wrong station. Try again.";
                AnnouncementManager.Instance?.AnnounceTimed(message,
                    config.AnnouncementDuration(AccessibilityMode));

                eventLogger?.LogStationSelected(station.StationName, false, timeToSelection);
                OnSelectionResult?.Invoke(false, message);

                if (!config.allowRetryAfterWrongSelection)
                    StartCoroutine(CompleteRoundAfterDelay(config.roundTransitionDelay));
            }
        }

        private IEnumerator CompleteRoundAfterDelay(float delay)
        {
            CurrentState = GameState.RoundFeedback;
            yield return new WaitForSeconds(delay);

            OnRoundCompleted?.Invoke();
            CurrentRound++;

            if (CurrentRound >= config.TotalRounds)
            {
                CurrentState = GameState.Results;
                AudioManager.Instance?.PlayResults();
                eventLogger?.LogGameCompleted(TotalAttempts, CorrectSelections, WrongSelections, TotalElapsedTime);
                OnGameCompleted?.Invoke();
            }
            else
            {
                CurrentState = GameState.Playing;
                BeginRound();
            }
        }

        private List<StationDefinition> PickRandomDefinitions(int count)
        {
            var available = new List<StationDefinition>(stationDefinitions);
            var result = new List<StationDefinition>();

            for (int i = 0; i < count && available.Count > 0; i++)
            {
                int index = Random.Range(0, available.Count);
                result.Add(available[index]);
                available.RemoveAt(index);
            }

            return result;
        }

        private string BuildTargetAnnouncement(RelicStation target)
        {
            // Difficulty scaling changes how the target is described.
            if (CurrentRound == 0)
            {
                return $"Round 1. Find the {target.StationName}. It is {target.SoundDescriptor}.";
            }
            else if (CurrentRound == 1)
            {
                return $"Round 2. Find the {target.SoundDescriptor} station.";
            }
            else
            {
                return $"Round 3. Listen carefully. Find the {target.SoundDescriptor} station.";
            }
        }

        private void CleanupStations()
        {
            foreach (var station in _activeStations)
            {
                if (station != null) Destroy(station.gameObject);
            }
            _activeStations.Clear();
        }
    }

    public enum GameState
    {
        Menu,
        Playing,
        RoundFeedback,
        Results
    }

    [Serializable]
    public class StationDefinition
    {
        public string stationName;
        [TextArea]
        public string soundDescriptor;
        public AudioClip audioClip;
    }
}
