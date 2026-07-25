using System;
using _PhotoCountdown.Gameplay.Characters.Actions;
using _PhotoCountdown.Gameplay.Characters.Behaviours;
using UnityEngine;

namespace _PhotoCountdown.Presentation.Characters
{
    public class CharacterActionPresenter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterBehaviourController _controller;
        [SerializeField] private CharacterActionDefinition _idleAction;
        [SerializeField] private Transform _visualRoot;
        [SerializeField] private SpriteRenderer[] _renderers;

        [Header("Pulse")]
        [SerializeField, Min(1f)] private float _pulseScale = 1.08f;
        [SerializeField, Min(0.01f)] private float _pulseDuration = 0.18f;

        private CharacterActionDefinition _shownAction;
        private CharacterBehaviourPhase _shownPhase;
        private Vector3 _baseScale;
        private float _pulseElapsed;
        private bool _isReady;
        private bool _isPulsing;
        private bool _wasDragging;

        private void Awake()
        {
            ValidateReferences();
            _baseScale = _visualRoot.localScale;
        }

        private void Update()
        {
            if (!_controller.IsInitialized)
                return;

            if (!_isReady)
                InitializePresentation();

            CharacterBehaviourPhase phase = _controller.CurrentPhase;
            CharacterActionDefinition action = GetDisplayedAction();

            if (_shownPhase != phase || _shownAction != action)
                ChangeState(phase, action);

            bool isDragging = _controller.IsDragging;

            if (!_wasDragging && isDragging)
                PlayPulse();

            _wasDragging = isDragging;

            ApplyCurrentFrames();
            UpdatePulse();
        }

        private void InitializePresentation()
        {
            _shownPhase = _controller.CurrentPhase;
            _shownAction = GetDisplayedAction();

            ValidateAction(_shownAction);

            _wasDragging = _controller.IsDragging;
            _visualRoot.localScale = _baseScale;

            ApplyCurrentFrames();
            _isReady = true;
        }

        private void ChangeState(CharacterBehaviourPhase phase, CharacterActionDefinition action)
        {
            ValidateAction(action);

            _shownPhase = phase;
            _shownAction = action;

            ApplyCurrentFrames();
            PlayPulse();
        }

        private CharacterActionDefinition GetDisplayedAction()
        {
            CharacterActionDefinition action = _controller.CurrentAction;
            return action ? action : _idleAction;
        }

        private void ApplyCurrentFrames()
        {
            double elapsedTime = _controller.CurrentPhaseElapsedTime;

            for (int i = 0; i < _renderers.Length; i++)
            {
                Sprite frame = _shownAction.GetFrameAt(i, elapsedTime);

                _renderers[i].sprite = frame;
                _renderers[i].color = Color.white;
            }
        }

        private void PlayPulse()
        {
            _pulseElapsed = 0f;
            _isPulsing = true;
        }

        private void UpdatePulse()
        {
            if (!_isPulsing)
                return;

            _pulseElapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(_pulseElapsed / _pulseDuration);
            float weight = Mathf.Sin(progress * Mathf.PI);
            float scale = Mathf.Lerp(1f, _pulseScale, weight);

            _visualRoot.localScale = _baseScale * scale;

            if (progress < 1f)
                return;

            _visualRoot.localScale = _baseScale;
            _isPulsing = false;
        }

        private void OnDisable()
        {
            if (_visualRoot)
                _visualRoot.localScale = _baseScale;

            _isPulsing = false;
        }

        private void ValidateReferences()
        {
            if (_controller == null)
                throw new MissingReferenceException($"{name} has no behaviour controller.");

            if (_idleAction == null)
                throw new MissingReferenceException($"{name} has no idle action.");

            if (_visualRoot == null)
                throw new MissingReferenceException($"{name} has no visual root.");

            if (_renderers == null || _renderers.Length == 0)
                throw new MissingReferenceException($"{name} has no sprite renderers.");

            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] == null)
                    throw new MissingReferenceException($"{name} renderer {i} is missing.");
            }

            if (_pulseScale < 1f)
                throw new InvalidOperationException($"{name} has an invalid pulse scale.");

            if (_pulseDuration <= 0f)
                throw new InvalidOperationException($"{name} has an invalid pulse duration.");

            ValidateAction(_idleAction);
        }

        private void ValidateAction(CharacterActionDefinition action)
        {
            if (action == null)
                throw new MissingReferenceException($"{name} has no action to display.");

            action.Validate();

            if (action.TrackCount != _renderers.Length)
            {
                throw new InvalidOperationException(
                    $"{action.name} has {action.TrackCount} sprite tracks, " +
                    $"but {name} has {_renderers.Length} sprite renderers.");
            }
        }
    }
}