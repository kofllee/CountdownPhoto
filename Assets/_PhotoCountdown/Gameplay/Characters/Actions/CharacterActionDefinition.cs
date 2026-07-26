using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _PhotoCountdown.Gameplay.Characters.Actions
{
    [CreateAssetMenu(fileName = "Action_", menuName = "Photo Countdown/Character Action")]
    public class CharacterActionDefinition : ScriptableObject
    {
        [SerializeField] private string _displayName;
        [SerializeField] private CharacterSpriteTrack[] _spriteTracks;
        [SerializeField, Min(0.01f)] private float _framesPerSecond = 6f;
        [SerializeField] private Sprite _icon;
        
        [Header("Sound")]
        [SerializeField] private AudioClip[] _enterSounds;
        [SerializeField, Range(0f, 1f)] private float _enterSoundChance = 0.35f;
        [SerializeField, Range(0f, 1f)] private float _enterSoundVolume = 1f;

        public float EnterSoundChance => _enterSoundChance;
        public float EnterSoundVolume => _enterSoundVolume;

        public AudioClip GetRandomEnterSound()
        {
            if (_enterSounds == null || _enterSounds.Length == 0)
                return null;

            return _enterSounds[Random.Range(0, _enterSounds.Length)];
        }

        public string DisplayName => _displayName;
        public Sprite Icon => _icon;
        public int TrackCount => _spriteTracks?.Length ?? 0;

        public Sprite GetFrameAt(int trackIndex, double elapsedTime)
        {
            if (_spriteTracks == null || trackIndex < 0 || trackIndex >= _spriteTracks.Length)
                return null;

            return _spriteTracks[trackIndex].GetFrameAt(elapsedTime, _framesPerSecond);
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(_displayName))
                throw new InvalidOperationException($"{name} has no display name.");

            if (_spriteTracks == null || _spriteTracks.Length == 0)
                throw new InvalidOperationException($"{name} has no sprite tracks.");

            for (int i = 0; i < _spriteTracks.Length; i++)
                _spriteTracks[i].Validate(name, i);

            if (_framesPerSecond <= 0f)
                throw new InvalidOperationException($"{name} has an invalid frame rate.");
            
            if (_enterSounds != null)
            {
                for (int i = 0; i < _enterSounds.Length; i++)
                {
                    if (!_enterSounds[i])
                        throw new InvalidOperationException($"{name} enter sound {i} is missing.");
                }
            }
        }
    }

    [Serializable]
    public class CharacterSpriteTrack
    {
        [SerializeField] private Sprite[] _frames;

        public int FrameCount => _frames?.Length ?? 0;

        public Sprite GetFrameAt(double elapsedTime, float framesPerSecond)
        {
            if (_frames == null || _frames.Length == 0)
                return null;

            if (_frames.Length == 1)
                return _frames[0];

            float time = Mathf.Max(0f, (float)elapsedTime);
            int frameIndex = Mathf.FloorToInt(time * framesPerSecond);
            return _frames[frameIndex % _frames.Length];
        }

        public void Validate(string actionName, int trackIndex)
        {
            if (_frames == null || _frames.Length == 0)
                throw new InvalidOperationException(
                    $"{actionName} sprite track {trackIndex} has no frames.");
        }
    }
}