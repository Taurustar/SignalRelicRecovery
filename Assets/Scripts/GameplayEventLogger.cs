using System;
using System.Collections.Generic;
using UnityEngine;

namespace SignalRelicRecovery
{
    /// <summary>
    /// Lightweight gameplay event logger. Records round starts, selections,
    /// correct/wrong results, and game completion. Useful for debugging and
    /// for the required event log / debug panel.
    /// </summary>
    public class GameplayEventLogger : MonoBehaviour
    {
        [SerializeField] private int maxEvents = 50;

        public static GameplayEventLogger Instance { get; private set; }

        public event Action<EventEntry> OnEventLogged;

        private readonly List<EventEntry> _events = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public IReadOnlyList<EventEntry> Events => _events;

        public void LogRoundStarted(int roundNumber, int totalRounds, string targetDescriptor)
        {
            Log($"Round {roundNumber}/{totalRounds} started. Target: {targetDescriptor}");
        }

        public void LogStationSelected(string stationName, bool correct, float timeToSelection)
        {
            string result = correct ? "CORRECT" : "WRONG";
            Log($"Selected '{stationName}' -> {result} ({timeToSelection:F1}s)");
        }

        public void LogGameCompleted(int attempts, int correct, int wrong, float totalTime)
        {
            Log($"Game complete. Attempts: {attempts}, Correct: {correct}, Wrong: {wrong}, Time: {totalTime:F1}s");
        }

        public void Log(string message)
        {
            var entry = new EventEntry
            {
                Timestamp = DateTime.Now.ToString("HH:mm:ss.fff"),
                Message = message
            };

            _events.Add(entry);
            if (_events.Count > maxEvents)
                _events.RemoveAt(0);

            OnEventLogged?.Invoke(entry);
            Debug.Log($"[GameplayEvent] {message}");
        }

        public void Clear()
        {
            _events.Clear();
        }
    }

    [Serializable]
    public struct EventEntry
    {
        public string Timestamp;
        public string Message;

        public override string ToString() => $"[{Timestamp}] {Message}";
    }
}
