using UnityEngine;

namespace SignalRelicRecovery
{
    /// <summary>
    /// Allows mouse/touch users to click a station to focus and select it.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class StationClickHandler : MonoBehaviour
    {
        [SerializeField] private RelicStation relicStation;

        private void OnValidate()
        {
            if (relicStation == null) relicStation = GetComponentInParent<RelicStation>();
        }

        private void OnMouseDown()
        {
            if (relicStation == null) return;

            var focusManager = FindAnyObjectByType<StationFocusManager>();
            focusManager?.SelectStation(relicStation);
        }
    }
}
