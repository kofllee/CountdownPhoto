using System;
using _PhotoCountdown.Gameplay.Characters;
using _PhotoCountdown.Gameplay.Characters.Actions;
using _PhotoCountdown.Gameplay.Characters.Behaviours;
using _PhotoCountdown.Gameplay.Levels;
using _PhotoCountdown.Gameplay.Objectives;
using UnityEngine;

namespace _PhotoCountdown.Gameplay.Photography
{
    public sealed class PhotoCaptureController : MonoBehaviour
    {
        private LevelClock _clock;
        private LevelCharacter[] _characters;
        private CharacterBehaviourController[] _behaviourControllers;
        private PhotoObjective[] _objectives;
        private bool _isInitialized;

        public event Action<PhotoSnapshot> PhotoCaptured;

        public PhotoSnapshot LastSnapshot { get; private set; }
        public bool HasPhoto => LastSnapshot != null;

        public bool CanTakePhoto
        {
            get
            {
                if (!_isInitialized || HasPhoto)
                    return false;

                foreach (CharacterBehaviourController controller in _behaviourControllers)
                {
                    if (controller.IsDragging)
                        return false;
                }

                return true;
            }
        }

        public void Init(LevelClock clock, LevelCharacter[] characters, PhotoObjective[] objectives)
        {
            if (!clock)
                throw new MissingReferenceException($"{name} received no level clock.");

            if (characters == null || characters.Length == 0)
                throw new MissingReferenceException($"{name} received no characters.");

            if (objectives == null || objectives.Length == 0)
                throw new MissingReferenceException($"{name} received no objectives.");

            _clock = clock;
            _characters = (LevelCharacter[])characters.Clone();
            _objectives = (PhotoObjective[])objectives.Clone();
            _behaviourControllers = new CharacterBehaviourController[_characters.Length];

            for (int i = 0; i < _characters.Length; i++)
            {
                LevelCharacter character = _characters[i];

                if (!character)
                    throw new MissingReferenceException($"{name} received a missing character.");

                CharacterBehaviourController controller =
                    character.GetComponent<CharacterBehaviourController>();

                if (!controller)
                    throw new MissingComponentException($"{character.name} needs CharacterBehaviourController.");

                if (!controller.IsInitialized)
                    throw new InvalidOperationException($"{character.name} behaviour is not initialized.");

                _behaviourControllers[i] = controller;
            }

            foreach (PhotoObjective objective in _objectives)
            {
                if (!objective)
                    throw new MissingReferenceException($"{name} received a missing objective.");

                objective.Validate();
            }

            _isInitialized = true;
        }

        public void TakePhoto()
        {
            if (!_isInitialized)
                throw new InvalidOperationException($"{name} is not initialized.");

            if (!CanTakePhoto)
                return;

            double photoTime = _clock.Time;
            CharacterPhotoState[] characters = CaptureCharacters(photoTime);
            PhotoEvaluationContext context = new PhotoEvaluationContext(photoTime, characters);
            PhotoObjectiveResult[] objectives = EvaluateObjectives(context);

            LastSnapshot = new PhotoSnapshot(photoTime, characters, objectives);
            PhotoCaptured?.Invoke(LastSnapshot);

#if UNITY_EDITOR
            Debug.Log($"Photo captured at {photoTime:F2}. Success: {LastSnapshot.IsSuccessful}", this);
#endif
        }

        private CharacterPhotoState[] CaptureCharacters(double photoTime)
        {
            CharacterPhotoState[] states = new CharacterPhotoState[_characters.Length];

            for (int i = 0; i < _characters.Length; i++)
            {
                LevelCharacter character = _characters[i];
                CharacterActionDefinition action = _behaviourControllers[i].GetActionAt(photoTime);
                states[i] = new CharacterPhotoState(character, character.CurrentSlot, action);
            }

            return states;
        }

        private PhotoObjectiveResult[] EvaluateObjectives(PhotoEvaluationContext context)
        {
            PhotoObjectiveResult[] results = new PhotoObjectiveResult[_objectives.Length];

            for (int i = 0; i < _objectives.Length; i++)
            {
                PhotoObjective objective = _objectives[i];
                bool completed = objective.IsCompleted(context);

                results[i] = new PhotoObjectiveResult(
                    objective.Description,
                    objective.FailureMessage,
                    completed);
            }

            return results;
        }
    }
}