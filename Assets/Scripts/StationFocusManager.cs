using System;
using System.Collections.Generic;
using UnityEngine;

namespace SignalRelicRecovery
{
    /// <summary>
    /// Manages which relic station currently has focus and processes player selection.
    /// </summary>
    public class StationFocusManager : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private InputReader inputReader;

        public event Action<RelicStation> OnStationSelected;
        public event Action<RelicStation> OnFocusChanged;

        private readonly List<RelicStation> _stations = new();
        private int _focusedIndex = -1;
        private float _lastFocusChangeTime;
        private float _lastSelectionTime;

        public RelicStation FocusedStation =>
            _stations.Count > 0 && _focusedIndex >= 0 ? _stations[_focusedIndex] : null;

        public void SetStations(List<RelicStation> stations)
        {
            // Clean up old stations.
            foreach (var station in _stations)
            {
                if (station != null)
                    station.SetFocus(false, config.unfocusedStationVolume);
            }

            _stations.Clear();
            if (stations == null || stations.Count == 0)
            {
                _focusedIndex = -1;
                return;
            }

            _stations.AddRange(stations);
            FocusIndex(0);
        }

        private void OnEnable()
        {
            if (inputReader == null) return;
            inputReader.OnFocusPrevious += HandleFocusPrevious;
            inputReader.OnFocusNext += HandleFocusNext;
            inputReader.OnSelect += HandleSelect;
        }

        private void OnDisable()
        {
            if (inputReader == null) return;
            inputReader.OnFocusPrevious -= HandleFocusPrevious;
            inputReader.OnFocusNext -= HandleFocusNext;
            inputReader.OnSelect -= HandleSelect;
        }

        private void HandleFocusPrevious()
        {
            if (Time.time - _lastFocusChangeTime < config.focusMoveCooldown) return;
            if (_stations.Count == 0) return;

            int newIndex = _focusedIndex - 1;
            if (newIndex < 0) newIndex = _stations.Count - 1;
            FocusIndex(newIndex);
            _lastFocusChangeTime = Time.time;
        }

        private void HandleFocusNext()
        {
            if (Time.time - _lastFocusChangeTime < config.focusMoveCooldown) return;
            if (_stations.Count == 0) return;

            int newIndex = (_focusedIndex + 1) % _stations.Count;
            FocusIndex(newIndex);
            _lastFocusChangeTime = Time.time;
        }

        private void HandleSelect()
        {
            if (Time.time - _lastSelectionTime < config.selectionCooldown) return;
            if (FocusedStation == null) return;

            _lastSelectionTime = Time.time;
            OnStationSelected?.Invoke(FocusedStation);
        }

        private void FocusIndex(int index)
        {
            if (_focusedIndex >= 0 && _focusedIndex < _stations.Count)
                _stations[_focusedIndex].SetFocus(false, config.unfocusedStationVolume);

            _focusedIndex = index;
            var station = _stations[_focusedIndex];
            station.SetFocus(true, config.focusedStationVolume);

            AudioManager.Instance?.PlayFocusChange();
            OnFocusChanged?.Invoke(station);
        }

        /// <summary>
        /// Direct selection via mouse/touch. Clicks focus and immediately select.
        /// </summary>
        public void SelectStation(RelicStation station)
        {
            if (Time.time - _lastSelectionTime < config.selectionCooldown) return;

            int index = _stations.IndexOf(station);
            if (index < 0) return;

            FocusIndex(index);
            _lastSelectionTime = Time.time;
            OnStationSelected?.Invoke(station);
        }
    }
}
