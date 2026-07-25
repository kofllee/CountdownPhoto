using System;
using UnityEngine;

namespace _PhotoCountdown.Gameplay.Levels
{
    public sealed class LevelClock : MonoBehaviour
    {
        private double _startedAt;
        private double _pausedTime;

        public bool HasStarted { get; private set; }
        public bool IsRunning { get; private set; }

        public double Time
        {
            get
            {
                if (!HasStarted)
                    return 0d;

                return IsRunning
                    ? UnityEngine.Time.timeAsDouble - _startedAt
                    : _pausedTime;
            }
        }

        public void StartClock()
        {
            if (HasStarted)
                throw new InvalidOperationException($"{name} is already started.");

            _startedAt = UnityEngine.Time.timeAsDouble;
            _pausedTime = 0d;
            HasStarted = true;
            IsRunning = true;
        }

        public void PauseClock()
        {
            if (!IsRunning)
                return;

            _pausedTime = Time;
            IsRunning = false;
        }

        public void ResumeClock()
        {
            if (!HasStarted || IsRunning)
                return;

            _startedAt = UnityEngine.Time.timeAsDouble - _pausedTime;
            IsRunning = true;
        }
    }
}