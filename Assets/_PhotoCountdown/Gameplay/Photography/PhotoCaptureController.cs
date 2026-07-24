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
        private LevelDefinition _level;
        private PhotoAlbum _album;
        private LevelClock _clock;
        private LevelCharacter[] _characters;
        private CharacterBehaviourController[] _behaviourControllers;
        private PhotoObjective[] _objectives;
        private PhotoRankEvaluator _rankEvaluator;
        private bool _isInitialized;

        public event Action<PhotoResult> PhotoCaptured;

        public bool CanTakePhoto
        {
            get
            {
                if (!_isInitialized)
                    return false;

                foreach (CharacterBehaviourController controller in _behaviourControllers)
                {
                    if (controller.IsDragging)
                        return false;
                }

                return true;
            }
        }

        public void Init(
            LevelDefinition level,
            PhotoAlbum album,
            LevelClock clock,
            LevelCharacter[] characters,
            PhotoObjective[] objectives,
            PhotoRankEvaluator rankEvaluator)
        {
            if (_isInitialized)
                throw new InvalidOperationException($"{name} is already initialized.");

            if (!level)
                throw new ArgumentNullException(nameof(level));

            if (album == null)
                throw new ArgumentNullException(nameof(album));

            if (!clock)
                throw new MissingReferenceException($"{name} received no level clock.");

            if (characters == null || characters.Length == 0)
                throw new MissingReferenceException($"{name} received no characters.");

            if (objectives == null || objectives.Length == 0)
                throw new MissingReferenceException($"{name} received no objectives.");

            if (!rankEvaluator)
                throw new MissingReferenceException($"{name} received no rank evaluator.");

            _level = level;
            _album = album;
            _clock = clock;
            _characters = (LevelCharacter[])characters.Clone();
            _objectives = (PhotoObjective[])objectives.Clone();
            _rankEvaluator = rankEvaluator;
            _behaviourControllers = new CharacterBehaviourController[_characters.Length];

            for (int i = 0; i < _characters.Length; i++)
            {
                LevelCharacter character = _characters[i];

                if (!character)
                    throw new MissingReferenceException($"{name} received a missing character.");

                CharacterBehaviourController controller = character.GetComponent<CharacterBehaviourController>();
                
                if (!controller)
                {
                    throw new MissingComponentException($"{character.name} needs CharacterBehaviourController.");
                }

                if (!controller.IsInitialized)
                {
                    throw new InvalidOperationException($"{character.name} behaviour is not initialized.");
                }

                _behaviourControllers[i] = controller;
            }

            foreach (PhotoObjective objective in _objectives)
            {
                if (!objective)
                    throw new MissingReferenceException($"{name} received a missing objective.");

                objective.Validate();
            }

            _rankEvaluator.Validate(_objectives);
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
            PhotoEvaluation evaluation = new PhotoEvaluation(_objectives, context);
            PhotoObjectiveResult[] objectiveResults = evaluation.CreateResults();
            PhotoSnapshot snapshot = new PhotoSnapshot(photoTime, objectiveResults);
            LevelRank rank = _rankEvaluator.Evaluate(evaluation);

            PhotoResult photo = new PhotoResult(_level.Id, DateTime.UtcNow.Ticks, snapshot, rank);

            _album.Add(photo);
            PhotoCaptured?.Invoke(photo);

#if UNITY_EDITOR
            Debug.Log($"Photo #{_album.GetLevelPhotoCount(_level.Id)}: {_level.Id}, {rank}.", this);
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
    }
}