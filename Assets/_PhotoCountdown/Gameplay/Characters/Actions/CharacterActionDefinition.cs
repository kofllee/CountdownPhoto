using System;
using UnityEngine;

namespace _PhotoCountdown.Gameplay.Characters.Actions
{
    [CreateAssetMenu(fileName = "Action_", menuName = "Photo Countdown/Character Action")]
    public class CharacterActionDefinition : ScriptableObject
    {
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite[] _frames;
        [SerializeField, Min(0.01f)] private float _framesPerSecond = 6f;
        [SerializeField] private Sprite _icon;

        public string DisplayName => _displayName;
        public Sprite Icon => _icon;
        public int FrameCount => _frames?.Length ?? 0;

        public Sprite GetFrameAt(double elapsedTime)
        {
            if (_frames == null || _frames.Length == 0)
                return null;

            if (_frames.Length == 1)
                return _frames[0];

            float time = Mathf.Max(0f, (float)elapsedTime);
            int frameIndex = Mathf.FloorToInt(time * _framesPerSecond);
            return _frames[frameIndex % _frames.Length];
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(_displayName))
                throw new InvalidOperationException($"{name} has no display name.");

            if (_frames == null || _frames.Length == 0)
                throw new InvalidOperationException($"{name} has no animation frames.");

            foreach (Sprite frame in _frames)
            {
                if (frame == null)
                    throw new InvalidOperationException($"{name} contains a missing frame.");
            }

            if (_framesPerSecond <= 0f)
                throw new InvalidOperationException($"{name} has an invalid frame rate.");
        }
    }
}