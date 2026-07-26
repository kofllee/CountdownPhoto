using System;
using System.Collections;
using _PhotoCountdown.Gameplay.Flow;
using _PhotoCountdown.Presentation.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace _PhotoCountdown.Presentation.UI
{
    public sealed class SettingsPanelPresenter : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button _openButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _deleteDataButton;

        [Header("Sliders")]
        [SerializeField] private Slider _masterSlider;
        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Slider _effectsSlider;

        [Header("Animation")]
        [SerializeField] private RectTransform _panel;
        [SerializeField] private CanvasGroup _overlay;
        [SerializeField] private Vector2 _hiddenOffset = new Vector2(0f, -800f);
        [SerializeField, Min(0.01f)] private float _animationDuration = 0.25f;

        [Header("Sound")]
        [SerializeField] private AudioCuePlayer _openSound;

        private GameSession _session;
        private Coroutine _animation;
        private Vector2 _shownPosition;
        private Vector2 _hiddenPosition;
        private bool _isInitialized;
        private bool _isOpen;

        private void Awake()
        {
            ValidateReferences();

            _shownPosition = _panel.anchoredPosition;
            _hiddenPosition = _shownPosition + _hiddenOffset;

            SetClosedImmediately();
        }

        public void Init(GameSession session)
        {
            if (_isInitialized)
                throw new InvalidOperationException($"{name} is already initialized.");

            _session = session ?? throw new ArgumentNullException(nameof(session));

            ConfigureSlider(_masterSlider);
            ConfigureSlider(_musicSlider);
            ConfigureSlider(_effectsSlider);
            RefreshSliders();

            _openButton.onClick.AddListener(Open);
            _closeButton.onClick.AddListener(Close);
            _deleteDataButton.onClick.AddListener(DeleteAllData);

            _masterSlider.onValueChanged.AddListener(SetMasterVolume);
            _musicSlider.onValueChanged.AddListener(SetMusicVolume);
            _effectsSlider.onValueChanged.AddListener(SetEffectsVolume);

            _isInitialized = true;
        }

        public void Open()
        {
            if (!_isInitialized || _isOpen)
                return;

            _isOpen = true;
            StartAnimation(true);
            _openSound.Play();
        }

        public void Close()
        {
            if (!_isInitialized || !_isOpen)
                return;

            _session.SaveSettings();
            _isOpen = false;
            StartAnimation(false);
        }

        private void DeleteAllData()
        {
            _session.DeleteAllData();
            RefreshSliders();
        }

        private void SetMasterVolume(float value)
        {
            _session.Settings.SetMasterVolume(value);
        }

        private void SetMusicVolume(float value)
        {
            _session.Settings.SetMusicVolume(value);
        }

        private void SetEffectsVolume(float value)
        {
            _session.Settings.SetEffectsVolume(value);
        }

        private void RefreshSliders()
        {
            _masterSlider.SetValueWithoutNotify(_session.Settings.MasterVolume);
            _musicSlider.SetValueWithoutNotify(_session.Settings.MusicVolume);
            _effectsSlider.SetValueWithoutNotify(_session.Settings.EffectsVolume);
        }

        private static void ConfigureSlider(Slider slider)
        {
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
        }

        private void StartAnimation(bool opening)
        {
            if (_animation != null)
                StopCoroutine(_animation);

            _animation = StartCoroutine(Animate(opening));
        }

        private IEnumerator Animate(bool opening)
        {
            Vector2 startPosition = _panel.anchoredPosition;
            Vector2 targetPosition = opening ? _shownPosition : _hiddenPosition;
            float startAlpha = _overlay.alpha;
            float targetAlpha = opening ? 1f : 0f;

            if (opening)
            {
                _overlay.blocksRaycasts = true;
                _overlay.interactable = true;
            }

            float elapsed = 0f;

            while (elapsed < _animationDuration)
            {
                elapsed += Time.unscaledDeltaTime;

                float progress = Mathf.Clamp01(elapsed / _animationDuration);
                float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);

                _panel.anchoredPosition =
                    Vector2.LerpUnclamped(startPosition, targetPosition, easedProgress);

                _overlay.alpha = Mathf.Lerp(startAlpha, targetAlpha, easedProgress);
                yield return null;
            }

            _panel.anchoredPosition = targetPosition;
            _overlay.alpha = targetAlpha;

            if (!opening)
            {
                _overlay.blocksRaycasts = false;
                _overlay.interactable = false;
            }

            _animation = null;
        }

        private void SetClosedImmediately()
        {
            _panel.anchoredPosition = _hiddenPosition;
            _overlay.alpha = 0f;
            _overlay.blocksRaycasts = false;
            _overlay.interactable = false;
            _isOpen = false;
        }

        private void OnDestroy()
        {
            if (!_isInitialized)
                return;

            _session.SaveSettings();

            _openButton.onClick.RemoveListener(Open);
            _closeButton.onClick.RemoveListener(Close);
            _deleteDataButton.onClick.RemoveListener(DeleteAllData);

            _masterSlider.onValueChanged.RemoveListener(SetMasterVolume);
            _musicSlider.onValueChanged.RemoveListener(SetMusicVolume);
            _effectsSlider.onValueChanged.RemoveListener(SetEffectsVolume);
        }

        private void ValidateReferences()
        {
            if (!_openButton)
                throw new MissingReferenceException($"{name} has no open button.");

            if (!_closeButton)
                throw new MissingReferenceException($"{name} has no close button.");

            if (!_deleteDataButton)
                throw new MissingReferenceException($"{name} has no delete data button.");

            if (!_masterSlider || !_musicSlider || !_effectsSlider)
                throw new MissingReferenceException($"{name} has missing sliders.");

            if (!_panel)
                throw new MissingReferenceException($"{name} has no settings panel.");

            if (!_overlay)
                throw new MissingReferenceException($"{name} has no overlay.");

            if (!_openSound)
                throw new MissingReferenceException($"{name} has no open sound.");

            if (_animationDuration <= 0f)
                throw new InvalidOperationException($"{name} has invalid animation duration.");
        }
    }
}