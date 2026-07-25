using System.Collections;
using UnityEngine;

namespace _PhotoCountdown.Presentation.Flow
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class SceneTransitionOverlay : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField, Min(0.01f)] private float _fadeOutDuration = 0.18f;
        [SerializeField, Min(0.01f)] private float _fadeInDuration = 0.18f;

        private void Awake()
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();

            ValidateReferences();

            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        public IEnumerator FadeOut()
        {
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;

            yield return FadeTo(1f, _fadeOutDuration);

            // Даём Unity отрисовать хотя бы один полностью закрытый кадр.
            yield return null;
        }

        public IEnumerator FadeIn()
        {
            yield return FadeTo(0f, _fadeInDuration);

            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        private IEnumerator FadeTo(float targetAlpha, float duration)
        {
            float startAlpha = _canvasGroup.alpha;

            if (Mathf.Approximately(startAlpha, targetAlpha))
            {
                _canvasGroup.alpha = targetAlpha;
                yield break;
            }

            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;

                float progress = Mathf.Clamp01(elapsedTime / duration);
                float easedProgress = progress * progress * (3f - 2f * progress);

                _canvasGroup.alpha = Mathf.LerpUnclamped(
                    startAlpha, targetAlpha, easedProgress);

                yield return null;
            }

            _canvasGroup.alpha = targetAlpha;
        }

        private void ValidateReferences()
        {
            if (_canvasGroup == null)
                throw new MissingReferenceException($"{name} has no canvas group.");
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }
#endif
    }
}