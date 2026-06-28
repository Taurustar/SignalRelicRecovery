using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SignalRelicRecovery
{
    /// <summary>
    /// Thin wrapper around the project's Input System asset.
    /// Exposes simple C# events so gameplay code stays input-agnostic.
    /// </summary>
    public class InputReader : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputAsset;

        public event Action OnFocusPrevious;
        public event Action OnFocusNext;
        public event Action OnSelect;
        public event Action OnPause;
        public event Action OnAnnounceTarget;

        private InputActionMap _gameplayMap;
        private InputActionMap _uiMap;

        private InputAction _previousAction;
        private InputAction _nextAction;
        private InputAction _selectAction;
        private InputAction _pauseAction;
        private InputAction _announceTargetAction;

        private void OnEnable()
        {
            if (inputAsset == null)
            {
                Debug.LogError("InputReader: no InputActionAsset assigned.", this);
                enabled = false;
                return;
            }

            _gameplayMap = inputAsset.FindActionMap("Player", true);
            _uiMap = inputAsset.FindActionMap("UI", true);

            _previousAction = _gameplayMap.FindAction("Previous", true);
            _nextAction = _gameplayMap.FindAction("Next", true);
            _selectAction = _gameplayMap.FindAction("Interact", true);
            _announceTargetAction = _gameplayMap.FindAction("AnnounceTarget", true);
            _pauseAction = _uiMap.FindAction("Cancel", true);

            _previousAction.performed += OnPreviousPerformed;
            _nextAction.performed += OnNextPerformed;
            _selectAction.performed += OnSelectPerformed;
            _announceTargetAction.performed += OnAnnounceTargetPerformed;
            _pauseAction.performed += OnPausePerformed;

            _gameplayMap.Enable();
            _uiMap.Enable();
        }

        private void OnDisable()
        {
            if (_previousAction != null) _previousAction.performed -= OnPreviousPerformed;
            if (_nextAction != null) _nextAction.performed -= OnNextPerformed;
            if (_selectAction != null) _selectAction.performed -= OnSelectPerformed;
            if (_announceTargetAction != null) _announceTargetAction.performed -= OnAnnounceTargetPerformed;
            if (_pauseAction != null) _pauseAction.performed -= OnPausePerformed;

            _gameplayMap?.Disable();
            _uiMap?.Disable();
        }

        private void OnPreviousPerformed(InputAction.CallbackContext ctx) => OnFocusPrevious?.Invoke();
        private void OnNextPerformed(InputAction.CallbackContext ctx) => OnFocusNext?.Invoke();
        private void OnSelectPerformed(InputAction.CallbackContext ctx) => OnSelect?.Invoke();
        private void OnAnnounceTargetPerformed(InputAction.CallbackContext ctx) => OnAnnounceTarget?.Invoke();
        private void OnPausePerformed(InputAction.CallbackContext ctx) => OnPause?.Invoke();
    }
}
