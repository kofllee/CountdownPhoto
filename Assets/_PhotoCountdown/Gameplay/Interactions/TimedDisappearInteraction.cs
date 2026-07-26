using System;
using _PhotoCountdown.Gameplay.Levels;
using UnityEngine;

namespace _PhotoCountdown.Gameplay.Interactions
{
    public sealed class TimedDisappearInteraction : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private float _leaveDuration = 0.5f;
        [SerializeField, Min(0.01f)] private float _absentDuration = 5f;

        private LevelClock _clock;
        private double _leaveStartedAt = double.NegativeInfinity;
        private double _returnAt = double.NegativeInfinity;

        public bool IsInitialized => _clock != null;
        public bool CanActivate => IsInitialized && _clock.IsRunning &&
                                   !IsCycleActiveAt(_clock.Time);
        public double CurrentTime => IsInitialized ? _clock.Time : 0d;
        public float LeaveDuration => _leaveDuration;
        public float AbsentDuration => _absentDuration;

        public void Init(LevelClock clock)
        {
            if (IsInitialized)
                throw new InvalidOperationException($"{name} is already initialized.");

            if (!clock)
                throw new MissingReferenceException($"{name} received no level clock.");

            if (_leaveDuration <= 0f)
                throw new InvalidOperationException($"{name} has an invalid leave duration.");

            if (_absentDuration <= 0f)
                throw new InvalidOperationException($"{name} has an invalid absent duration.");

            _clock = clock;
        }

        public void Activate()
        {
            if (!IsInitialized)
                throw new InvalidOperationException($"{name} is not initialized.");

            if (!CanActivate)
                return;

            _leaveStartedAt = _clock.Time;
            _returnAt = _leaveStartedAt + _leaveDuration + _absentDuration;
        }

        public bool IsLeavingAt(double time)
        {
            return IsCycleActiveAt(time) && time < _leaveStartedAt + _leaveDuration;
        }

        public bool IsAbsentAt(double time)
        {
            return IsCycleActiveAt(time) && time >= _leaveStartedAt + _leaveDuration;
        }

        public double GetLeaveElapsedAt(double time)
        {
            return IsLeavingAt(time) ? Math.Max(0d, time - _leaveStartedAt) : 0d;
        }

        public double GetReturnRemainingAt(double time)
        {
            return IsAbsentAt(time) ? Math.Max(0d, _returnAt - time) : 0d;
        }

        private bool IsCycleActiveAt(double time)
        {
            return time >= _leaveStartedAt && time < _returnAt;
        }
    }
}