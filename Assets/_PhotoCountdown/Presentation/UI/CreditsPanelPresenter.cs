using System;
using System.Collections;
using _PhotoCountdown.Presentation.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace _PhotoCountdown.Presentation.UI
{
    public sealed class CreditsPanelPresenter : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button _openButton;
        [SerializeField] private Button _closeButton;

        [Header("Animation")]
        [SerializeField] private RectTransform _panel;
        [SerializeField] private CanvasGroup _overlay;
        [SerializeField] private Vector2 _hiddenOffset = new Vector2(0f, -800f);
        [SerializeField, Min(0.01f)] private float _animationDuration = 0.25f;

        [Header("Sound")]
        [SerializeField] private AudioCuePlayer _openSound;

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

        public void Init()
        {
            if (_isInitialized)
                throw new InvalidOperationException($"{name} is already initialized.");

            _openButton.onClick.AddListener(Open);
            _closeButton.onClick.AddListener(Close);

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

            _isOpen = false;
            StartAnimation(false);
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

            _openButton.onClick.RemoveListener(Open);
            _closeButton.onClick.RemoveListener(Close);
        }

        private void ValidateReferences()
        {
            if (!_openButton)
                throw new MissingReferenceException($"{name} has no open button.");

            if (!_closeButton)
                throw new MissingReferenceException($"{name} has no close button.");

            if (!_panel)
                throw new MissingReferenceException($"{name} has no credits panel.");

            if (!_overlay)
                throw new MissingReferenceException($"{name} has no overlay.");

            if (!_openSound)
                throw new MissingReferenceException($"{name} has no open sound.");

            if (_animationDuration <= 0f)
                throw new InvalidOperationException($"{name} has invalid animation duration.");
        }
    }
}