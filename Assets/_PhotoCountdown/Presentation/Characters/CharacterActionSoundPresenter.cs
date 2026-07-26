using _PhotoCountdown.Gameplay.Characters.Actions;
using _PhotoCountdown.Gameplay.Characters.Behaviours;
using _PhotoCountdown.Presentation.Audio;
using UnityEngine;

namespace _PhotoCountdown.Presentation.Characters
{
    public sealed class CharacterActionSoundPresenter : AudioClient
    {
        [SerializeField] private CharacterBehaviourController _controller;
        [SerializeField] private CharacterActionDefinition _idleAction;

        private CharacterActionDefinition _previousAction;
        private bool _isReady;

        private void Update()
        {
            if (!IsAudioInitialized || !_controller.IsInitialized)
                return;

            CharacterActionDefinition action = _controller.CurrentAction;

            if (!action)
                action = _idleAction;

            if (!_isReady)
            {
                _previousAction = action;
                _isReady = true;
                return;
            }

            if (_previousAction == action)
                return;

            _previousAction = action;
            PlayActionSound(action);
        }

        private void PlayActionSound(CharacterActionDefinition action)
        {
            if (!action || Random.value > action.EnterSoundChance)
                return;

            AudioClip sound = action.GetRandomEnterSound();

            if (sound)
                Audio.PlayEffect(sound, action.EnterSoundVolume);
        }

        private void Awake()
        {
            if (!_controller)
                throw new MissingReferenceException($"{name} has no behaviour controller.");

            if (!_idleAction)
                throw new MissingReferenceException($"{name} has no idle action.");
        }
    }
}