using System;
using _PhotoCountdown.Core.Settings;
using UnityEngine;
using UnityEngine.Audio;

namespace _PhotoCountdown.Presentation.Audio
{
    public sealed class GameAudio : MonoBehaviour
    {
        private const float MinimumDecibels = -80f;

        [Header("Mixer")]
        [SerializeField] private AudioMixer _mixer;
        [SerializeField] private string _masterParameter = "MasterVolume";
        [SerializeField] private string _musicParameter = "MusicVolume";
        [SerializeField] private string _effectsParameter = "EffectsVolume";

        [Header("Sources")]
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _effectsSource;

        [Header("Common sounds")]
        [SerializeField] private AudioClip _buttonClickSound;

        private GameSettings _settings;
        private bool _isInitialized;

        public AudioClip ButtonClickSound => _buttonClickSound;

        public void Init(GameSettings settings)
        {
            if (_isInitialized)
                throw new InvalidOperationException($"{name} is already initialized.");

            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            ValidateReferences();

            _settings = settings;
            _settings.Changed += ApplyVolumes;
            _isInitialized = true;

            ApplyVolumes();
        }

        public void PlayEffect(AudioClip clip, float volume = 1f)
        {
            if (!_isInitialized || !clip)
                return;

            _effectsSource.PlayOneShot(clip, Mathf.Clamp01(volume));
        }

        public void PlayMusic(AudioClip clip, bool restart = false)
        {
            if (!_isInitialized || !clip)
                return;

            if (!restart && _musicSource.isPlaying && _musicSource.clip == clip)
                return;

            _musicSource.clip = clip;
            _musicSource.loop = true;
            _musicSource.Play();
        }

        public void StopMusic()
        {
            if (!_isInitialized)
                return;

            _musicSource.Stop();
            _musicSource.clip = null;
        }

        private void ApplyVolumes()
        {
            _mixer.SetFloat(_masterParameter, ToDecibels(_settings.MasterVolume));
            _mixer.SetFloat(_musicParameter, ToDecibels(_settings.MusicVolume));
            _mixer.SetFloat(_effectsParameter, ToDecibels(_settings.EffectsVolume));
        }

        private static float ToDecibels(float linearVolume)
        {
            if (linearVolume <= 0.0001f)
                return MinimumDecibels;

            return Mathf.Log10(linearVolume) * 20f;
        }

        private void OnDestroy()
        {
            if (_settings != null)
                _settings.Changed -= ApplyVolumes;
        }

        private void ValidateReferences()
        {
            if (!_mixer)
                throw new MissingReferenceException($"{name} has no audio mixer.");

            if (!_musicSource)
                throw new MissingReferenceException($"{name} has no music source.");

            if (!_effectsSource)
                throw new MissingReferenceException($"{name} has no effects source.");

            if (!_buttonClickSound)
                throw new MissingReferenceException($"{name} has no button click sound.");
        }
    }
}