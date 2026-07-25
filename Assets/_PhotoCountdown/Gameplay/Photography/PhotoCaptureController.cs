using System;
using System.Collections;
using System.Collections.Generic;
using _PhotoCountdown.Gameplay.Characters;
using _PhotoCountdown.Gameplay.Characters.Actions;
using _PhotoCountdown.Gameplay.Characters.Behaviours;
using _PhotoCountdown.Gameplay.Levels;
using _PhotoCountdown.Gameplay.Objectives;
using _PhotoCountdown.Input;
using UnityEngine;

namespace _PhotoCountdown.Gameplay.Photography
{
    public sealed class PhotoCaptureController : MonoBehaviour
    {
        [SerializeField] private PhotoImageCapture _imageCapture;

        private LevelDefinition _level;
        private PhotoAlbum _album;
        private PhotoAlbumStorage _storage;
        private LevelClock _clock;
        private CharacterDragInput _dragInput;
        private LevelCharacter[] _characters;
        private CharacterBehaviourController[] _behaviourControllers;
        private CharacterMover[] _movers;
        private PhotoObjective[] _objectives;
        private PhotoRankEvaluator _rankEvaluator;
        private bool _isInitialized;
        private bool _isCapturing;

        public event Action<PhotoResult, IReadOnlyList<string>> PhotoCaptured;

        public PhotoResult LastPhoto { get; private set; }

        public bool CanTakePhoto
        {
            get
            {
                if (!_isInitialized || _isCapturing)
                    return false;

                foreach (CharacterBehaviourController controller in _behaviourControllers)
                {
                    if (controller.IsDragging)
                        return false;
                }

                foreach (CharacterMover mover in _movers)
                {
                    if (!mover.IsSettled)
                        return false;
                }

                return true;
            }
        }

        public void Init(
            LevelDefinition level,
            PhotoAlbum album,
            PhotoAlbumStorage storage,
            LevelClock clock,
            CharacterDragInput dragInput,
            LevelCharacter[] characters,
            PhotoObjective[] objectives,
            PhotoRankEvaluator rankEvaluator)
        {
            if (_isInitialized)
                throw new InvalidOperationException($"{name} is already initialized.");

            if (level == null)
                throw new ArgumentNullException(nameof(level));

            if (album == null)
                throw new ArgumentNullException(nameof(album));

            if (storage == null)
                throw new ArgumentNullException(nameof(storage));

            if (clock == null)
                throw new MissingReferenceException($"{name} received no level clock.");

            if (dragInput == null)
                throw new MissingReferenceException($"{name} received no drag input.");

            if (characters == null || characters.Length == 0)
                throw new MissingReferenceException($"{name} received no characters.");

            if (objectives == null || objectives.Length == 0)
                throw new MissingReferenceException($"{name} received no objectives.");

            if (rankEvaluator == null)
                throw new MissingReferenceException($"{name} received no rank evaluator.");

            if (_imageCapture == null)
                throw new MissingReferenceException($"{name} has no image capture.");

            _level = level;
            _album = album;
            _storage = storage;
            _clock = clock;
            _dragInput = dragInput;
            _characters = (LevelCharacter[])characters.Clone();
            _objectives = (PhotoObjective[])objectives.Clone();
            _rankEvaluator = rankEvaluator;
            _behaviourControllers =
                new CharacterBehaviourController[_characters.Length];
            _movers = new CharacterMover[_characters.Length];

            for (int i = 0; i < _characters.Length; i++)
            {
                LevelCharacter character = _characters[i];

                if (character == null)
                    throw new MissingReferenceException($"{name} received a missing character.");

                CharacterBehaviourController controller =
                    character.GetComponent<CharacterBehaviourController>();
                CharacterMover mover = character.GetComponent<CharacterMover>();

                if (controller == null)
                {
                    throw new MissingComponentException(
                        $"{character.name} needs CharacterBehaviourController.");
                }

                if (mover == null)
                    throw new MissingComponentException($"{character.name} needs CharacterMover.");

                if (!controller.IsInitialized)
                {
                    throw new InvalidOperationException(
                        $"{character.name} behaviour is not initialized.");
                }

                _behaviourControllers[i] = controller;
                _movers[i] = mover;
            }

            foreach (PhotoObjective objective in _objectives)
            {
                if (objective == null)
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

            StartCoroutine(CaptureRoutine());
        }

        private IEnumerator CaptureRoutine()
{
    _isCapturing = true;
    _dragInput.enabled = false;

    bool resultShown = false;
    double photoTime = _clock.Time;
    _clock.PauseClock();

    try
    {
        CharacterPhotoState[] characters = CaptureCharacters(photoTime);
        PhotoEvaluationContext context = new PhotoEvaluationContext(
            photoTime,
            characters);

        PhotoEvaluation evaluation = new PhotoEvaluation(_objectives, context);
        LevelRank rank = _rankEvaluator.Evaluate(evaluation);
        string[] visibleFailures =
            _rankEvaluator.GetVisibleFailureDescriptions(evaluation, rank);

        yield return null;

        CapturedPhotoImage capturedImage = null;
        yield return _imageCapture.Capture(image => capturedImage = image);

        if (capturedImage == null)
            throw new InvalidOperationException("Photo camera returned no image.");

        PhotoObjectiveResult[] objectiveResults = evaluation.CreateResults();
        string photoId = Guid.NewGuid().ToString("N");
        string fileName = photoId + ".png";

        PhotoImageReference image = new PhotoImageReference(
            fileName,
            capturedImage.Width,
            capturedImage.Height);

        PhotoSnapshot snapshot = new PhotoSnapshot(
            photoTime,
            objectiveResults,
            Array.Empty<PhotoIssueRegion>());

        PhotoResult photo = new PhotoResult(
            photoId,
            _level.Id,
            DateTime.UtcNow.Ticks,
            snapshot,
            image,
            rank);

        _storage.SaveNewPhoto(_album, photo, capturedImage.PngData);

        LastPhoto = photo;
        PhotoCaptured?.Invoke(photo, visibleFailures);
        resultShown = true;

#if UNITY_EDITOR
        Debug.Log(
            $"Saved photo #{_album.GetLevelPhotoCount(_level.Id)}: " +
            $"{_level.Id}, {rank}.",
            this);
#endif
    }
    finally
    {
        if (!resultShown)
        {
            _clock.ResumeClock();
            _dragInput.enabled = true;
        }

        _isCapturing = false;
    }
}

        private CharacterPhotoState[] CaptureCharacters(double photoTime)
        {
            CharacterPhotoState[] states = new CharacterPhotoState[_characters.Length];

            for (int i = 0; i < _characters.Length; i++)
            {
                LevelCharacter character = _characters[i];
                CharacterActionDefinition action =
                    _behaviourControllers[i].GetActionAt(photoTime);

                states[i] = new CharacterPhotoState(
                    character,
                    character.CurrentSlot,
                    action);
            }

            return states;
        }
    }
}