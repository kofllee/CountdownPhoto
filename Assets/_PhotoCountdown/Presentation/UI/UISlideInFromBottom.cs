using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace _PhotoCountdown.Presentation.UI
{
    [DisallowMultipleComponent]
    public sealed class UISlideInFromBottom : MonoBehaviour
    {
        [Header("Board")]
        [SerializeField] private RectTransform _target;
        [SerializeField, Min(0f)] private float _startOffset = 1100f;
        [SerializeField, Min(0.01f)] private float _enterDuration = 0.55f;
        [SerializeField, Min(0.01f)] private float _exitDuration = 0.4f;
        [SerializeField, Min(0f)] private float _enterDelay = 0.05f;

        [Header("Scroll")]
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField, Min(0.01f)] private float _scrollResetDuration = 0.25f;

        private Coroutine _enterAnimation;
        private Vector2 _shownPosition;
        private Vector2 _hiddenPosition;
        private bool _hasPosition;

        private void Awake()
        {
            if (_target == null)
                _target = transform as RectTransform;

            ValidateReferences();
        }

        public void PlayIn()
        {
            StopEnterAnimation();
            CapturePositions();

            ResetScrollImmediately();

            _target.anchoredPosition = _hiddenPosition;
            _enterAnimation = StartCoroutine(PlayInRoutine());
        }

        public IEnumerator PlayOut()
        {
            StopEnterAnimation();

            if (!_hasPosition)
                CapturePositions();

            yield return ScrollToStartRoutine();
            yield return SlideOutRoutine();
        }

        private IEnumerator PlayInRoutine()
        {
            if (_enterDelay > 0f)
            {
                float delayTime = 0f;

                while (delayTime < _enterDelay)
                {
                    delayTime += Time.unscaledDeltaTime;
                    yield return null;
                }
            }

            float elapsedTime = 0f;

            while (elapsedTime < _enterDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;

                float progress = Mathf.Clamp01(elapsedTime / _enterDuration);
                float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);

                _target.anchoredPosition = Vector2.LerpUnclamped(
                    _hiddenPosition, _shownPosition, easedProgress);

                yield return null;
            }

            _target.anchoredPosition = _shownPosition;
            _enterAnimation = null;
        }

        private IEnumerator ScrollToStartRoutine()
        {
            PrepareScrollLayout();

            RectTransform content = _scrollRect.content;
            Vector2 startPosition = content.anchoredPosition;

            _scrollRect.verticalNormalizedPosition = 1f;
            Canvas.ForceUpdateCanvases();

            Vector2 targetPosition = content.anchoredPosition;
            content.anchoredPosition = startPosition;

            bool wasEnabled = _scrollRect.enabled;
            _scrollRect.enabled = false;

            float elapsedTime = 0f;

            while (elapsedTime < _scrollResetDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;

                float progress = Mathf.Clamp01(elapsedTime / _scrollResetDuration);
                float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);

                content.anchoredPosition = Vector2.LerpUnclamped(
                    startPosition, targetPosition, easedProgress);

                yield return null;
            }

            content.anchoredPosition = targetPosition;
            Canvas.ForceUpdateCanvases();

            _scrollRect.enabled = wasEnabled;
            _scrollRect.StopMovement();
            _scrollRect.velocity = Vector2.zero;
            _scrollRect.verticalNormalizedPosition = 1f;

            yield return null;

            _scrollRect.verticalNormalizedPosition = 1f;
        }

        private IEnumerator SlideOutRoutine()
        {
            Vector2 startPosition = _target.anchoredPosition;
            float elapsedTime = 0f;

            while (elapsedTime < _exitDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;

                float progress = Mathf.Clamp01(elapsedTime / _exitDuration);
                float easedProgress = progress * progress * progress;

                _target.anchoredPosition = Vector2.LerpUnclamped(
                    startPosition, _hiddenPosition, easedProgress);

                yield return null;
            }

            _target.anchoredPosition = _hiddenPosition;
        }

        private void ResetScrollImmediately()
        {
            PrepareScrollLayout();

            _scrollRect.verticalNormalizedPosition = 1f;
            Canvas.ForceUpdateCanvases();

            _scrollRect.StopMovement();
            _scrollRect.velocity = Vector2.zero;
            _scrollRect.verticalNormalizedPosition = 1f;
        }

        private void PrepareScrollLayout()
        {
            Canvas.ForceUpdateCanvases();

            if (_scrollRect.viewport != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.viewport);

            LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.content);
            Canvas.ForceUpdateCanvases();

            _scrollRect.StopMovement();
            _scrollRect.velocity = Vector2.zero;
        }

        private void CapturePositions()
        {
            _shownPosition = _target.anchoredPosition;
            _hiddenPosition = _shownPosition + Vector2.down * _startOffset;
            _hasPosition = true;
        }

        private void StopEnterAnimation()
        {
            if (_enterAnimation == null)
                return;

            StopCoroutine(_enterAnimation);
            _enterAnimation = null;
        }

        private void OnDisable()
        {
            StopEnterAnimation();
        }

        private void ValidateReferences()
        {
            if (_target == null)
                throw new MissingReferenceException($"{name} has no animation target.");

            if (_scrollRect == null)
                throw new MissingReferenceException($"{name} has no scroll rect.");

            if (_scrollRect.content == null)
                throw new MissingReferenceException($"{name} scroll rect has no content.");
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _target = transform as RectTransform;
            _scrollRect = GetComponentInChildren<ScrollRect>();
        }
#endif
    }
}