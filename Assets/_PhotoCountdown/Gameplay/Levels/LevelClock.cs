using UnityEngine;

namespace _PhotoCountdown.Gameplay.Levels
{
    public class LevelClock : MonoBehaviour
    {
        private double _startedAt;
        private bool _isRunning;

        public double Time
        {
            get
            {
                if (!_isRunning)
                    return 0d;

                return UnityEngine.Time.timeAsDouble - _startedAt;
            }
        }

        public void StartClock()
        {
            _startedAt = UnityEngine.Time.timeAsDouble;
            _isRunning = true;
        }
    }
}