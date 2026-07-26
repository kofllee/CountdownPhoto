using System;
using _PhotoCountdown.Gameplay.Interactions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _PhotoCountdown.Presentation.Interactions
{
    public sealed class TimedDisappearInteractionPresenter : MonoBehaviour
    {
        [Header("Interaction")]
        [SerializeField] private TimedDisappearInteraction _interaction;

        [Header("Visual")]
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private Sprite _idleSprite;
        [SerializeField] private Sprite[] _leaveFrames;

        [Header("Return timer")]
        [SerializeField] private bool _showTimer = true;
        [SerializeField] private GameObject _timerRoot;
        [SerializeField] private Image _timerFill;
        [SerializeField] private TMP_Text _timerText;

        private void Awake()
        {
            ValidateReferences();
            ShowPresent();
        }

        private void Update()
        {
            if (!_interaction.IsInitialized)
                return;

            double time = _interaction.CurrentTime;

            if (_interaction.IsLeavingAt(time))
            {
                ShowLeaving(time);
                return;
            }

            if (_interaction.IsAbsentAt(time))
            {
                ShowAbsent(time);
                return;
            }

            ShowPresent();
        }

        private void ShowPresent()
        {
            _renderer.sprite = _idleSprite;
            SetTimerVisible(false);
        }

        private void ShowLeaving(double time)
        {
            SetTimerVisible(false);

            int visibleFrameCount = _leaveFrames.Length - 1;
            double elapsedTime = _interaction.GetLeaveElapsedAt(time);
            float progress = Mathf.Clamp01(
                (float)(elapsedTime / _interaction.LeaveDuration));

            int frameIndex = Mathf.FloorToInt(progress * visibleFrameCount);
            frameIndex = Mathf.Min(frameIndex, visibleFrameCount - 1);

            _renderer.sprite = _leaveFrames[frameIndex];
        }

        private void ShowAbsent(double time)
        {
            _renderer.sprite = _leaveFrames[_leaveFrames.Length - 1];

            if (!_showTimer)
            {
                SetTimerVisible(false);
                return;
            }

            SetTimerVisible(true);

            float remaining = (float)_interaction.GetReturnRemainingAt(time);
            _timerFill.fillAmount = Mathf.Clamp01(
                remaining / _interaction.AbsentDuration);
            _timerText.text = Mathf.CeilToInt(remaining).ToString();
        }

        private void SetTimerVisible(bool visible)
        {
            if (!_timerRoot)
                return;

            visible &= _showTimer;

            if (_timerRoot.activeSelf != visible)
                _timerRoot.SetActive(visible);
        }

        private void ValidateReferences()
        {
            if (!_interaction)
                throw new MissingReferenceException($"{name} has no interaction.");

            if (!_renderer)
                throw new MissingReferenceException($"{name} has no sprite renderer.");

            if (!_idleSprite)
                throw new MissingReferenceException($"{name} has no idle sprite.");

            if (_leaveFrames == null || _leaveFrames.Length < 2)
                throw new MissingReferenceException($"{name} has no leave frames.");

            for (int i = 0; i < _leaveFrames.Length - 1; i++)
            {
                if (!_leaveFrames[i])
                {
                    throw new MissingReferenceException(
                        $"{name} leave frame {i} is missing.");
                }
            }

            if (_leaveFrames[_leaveFrames.Length - 1])
            {
                throw new InvalidOperationException(
                    $"{name} last leave frame must be empty.");
            }
        }
    }
}