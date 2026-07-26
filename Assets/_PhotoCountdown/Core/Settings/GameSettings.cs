using System;
using UnityEngine;

namespace _PhotoCountdown.Core.Settings
{
    public sealed class GameSettings
    {
        public const float DefaultMasterVolume = 1f;
        public const float DefaultMusicVolume = 0.8f;
        public const float DefaultEffectsVolume = 1f;

        private float _masterVolume;
        private float _musicVolume;
        private float _effectsVolume;

        public event Action Changed;

        public float MasterVolume => _masterVolume;
        public float MusicVolume => _musicVolume;
        public float EffectsVolume => _effectsVolume;

        public GameSettings(float masterVolume, float musicVolume, float effectsVolume)
        {
            _masterVolume = Mathf.Clamp01(masterVolume);
            _musicVolume = Mathf.Clamp01(musicVolume);
            _effectsVolume = Mathf.Clamp01(effectsVolume);
        }

        public void SetMasterVolume(float value)
        {
            SetVolume(ref _masterVolume, value);
        }

        public void SetMusicVolume(float value)
        {
            SetVolume(ref _musicVolume, value);
        }

        public void SetEffectsVolume(float value)
        {
            SetVolume(ref _effectsVolume, value);
        }

        public void Reset()
        {
            bool changed =
                !Mathf.Approximately(_masterVolume, DefaultMasterVolume) ||
                !Mathf.Approximately(_musicVolume, DefaultMusicVolume) ||
                !Mathf.Approximately(_effectsVolume, DefaultEffectsVolume);

            _masterVolume = DefaultMasterVolume;
            _musicVolume = DefaultMusicVolume;
            _effectsVolume = DefaultEffectsVolume;

            if (changed)
                Changed?.Invoke();
        }

        private void SetVolume(ref float currentValue, float value)
        {
            float clampedValue = Mathf.Clamp01(value);

            if (Mathf.Approximately(currentValue, clampedValue))
                return;

            currentValue = clampedValue;
            Changed?.Invoke();
        }
    }
}