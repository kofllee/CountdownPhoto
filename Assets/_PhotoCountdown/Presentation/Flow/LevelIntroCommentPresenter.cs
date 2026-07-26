using System;
using System.Collections;
using _PhotoCountdown.Presentation.Audio;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _PhotoCountdown.Presentation.Flow
{
    public sealed class LevelIntroCommentPresenter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject _root;
        [SerializeField] private RectTransform _dialogPanel;
        [SerializeField] private TMP_Text _commentText;
        [SerializeField] private Button _dismissButton;

        [Header("Animation")]
        [SerializeField, Min(0.01f)] private float _showDuration = 0.35f;
        [SerializeField, Min(0.01f)] private float _hideDuration = 0.25f;
        [SerializeField, Min(0f)] private float _hiddenOffset = 450f;
        
        [Header("Sound")]
        [SerializeField] private AudioCuePlayer _showSound;

        private Vector2 _shownPosition;
        private bool _dismissRequested;
        private bool _canDismiss;
        private bool _isInitialized;

        public void Init()
        {
            if (_isInitialized)
                throw new InvalidOperationException($"{name} is already initialized.");

            ValidateReferences();

            _shownPosition = _dialogPanel.anchoredPosition;
            _dismissButton.onClick.AddListener(RequestDismiss);

            _isInitialized = true;
            HideImmediate();
        }

        public IEnumerator ShowAndWait(string comment)
        {
            if (!_isInitialized)
                throw new InvalidOperationException($"{name} is not initialized.");

            if (string.IsNullOrWhiteSpace(comment))
                yield break;

            _commentText.text = comment;
            _dismissRequested = false;
            _canDismiss = false;

            _root.SetActive(true);
            _root.transform.SetAsLastSibling();
            
            _showSound.Play();

            Vector2 hiddenPosition = _shownPosition + Vector2.down * _hiddenOffset;
            _dialogPanel.anchoredPosition = hiddenPosition;

            yield return MovePanel(hiddenPosition, _shownPosition, _showDuration);

            _canDismiss = true;

            while (!_dismissRequested)
                yield return null;

            _canDismiss = false;

            yield return MovePanel(_shownPosition, hiddenPosition, _hideDuration);

            HideImmediate();
        }

        public void HideImmediate()
        {
            _dismissRequested = false;
            _canDismiss = false;

            if (_dialogPanel)
                _dialogPanel.anchoredPosition = _shownPosition;

            if (_root)
                _root.SetActive(false);
        }

        private void RequestDismiss()
        {
            if (_canDismiss)
                _dismissRequested = true;
        }

        private IEnumerator MovePanel(Vector2 from, Vector2 to, float duration)
        {
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;

                float progress = Mathf.Clamp01(elapsedTime / duration);
                float easedProgress = EaseOutCubic(progress);
                _dialogPanel.anchoredPosition =
                    Vector2.LerpUnclamped(from, to, easedProgress);

                yield return null;
            }

            _dialogPanel.anchoredPosition = to;
        }

        private static float EaseOutCubic(float progress)
        {
            float remaining = 1f - progress;
            return 1f - remaining * remaining * remaining;
        }

        private void ValidateReferences()
        {
            if (!_root)
                throw new MissingReferenceException($"{name} has no root.");

            if (!_dialogPanel)
                throw new MissingReferenceException($"{name} has no dialog panel.");

            if (!_commentText)
                throw new MissingReferenceException($"{name} has no comment text.");

            if (!_dismissButton)
                throw new MissingReferenceException($"{name} has no dismiss button.");

            if (_showDuration <= 0f || _hideDuration <= 0f)
                throw new InvalidOperationException($"{name} has invalid animation duration.");
            
            if (!_showSound)
                throw new MissingReferenceException($"{name} has no show sound.");
        }

        private void OnDestroy()
        {
            if (_dismissButton)
                _dismissButton.onClick.RemoveListener(RequestDismiss);
        }
    }
}