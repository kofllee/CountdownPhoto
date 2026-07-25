using System;
using System.Collections.Generic;
using _PhotoCountdown.Gameplay.Levels;
using _PhotoCountdown.Gameplay.Photography;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _PhotoCountdown.Presentation.Flow
{
    [DisallowMultipleComponent]
    public sealed class PhotoResultPresenter : MonoBehaviour
    {
        [Header("Roots")]
        [SerializeField] private GameObject _gameHudRoot;
        [SerializeField] private GameObject _resultRoot;

        [Header("Photo")]
        [SerializeField] private Image _photoImage;
        [SerializeField] private TMP_Text _levelNameText;

        [Header("Stars")]
        [SerializeField] private Image _firstStar;
        [SerializeField] private Image _secondStar;
        [SerializeField] private Sprite _grayStarSprite;
        [SerializeField] private Sprite _goldStarSprite;

        [Header("Failures")]
        [SerializeField] private TMP_Text[] _failureTexts;

        [Header("Buttons")]
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _nextButton;

        private LevelDefinition _level;
        private PhotoCaptureController _photoCapture;
        private PhotoAlbumStorage _storage;
        private Action _backRequested;
        private Action _retryRequested;
        private Action _nextRequested;
        private Texture2D _createdTexture;
        private Sprite _createdSprite;
        private bool _isInitialized;

        public void Init(
            LevelDefinition level,
            PhotoCaptureController photoCapture,
            PhotoAlbumStorage storage,
            Action backRequested,
            Action retryRequested,
            Action nextRequested)
        {
            if (_isInitialized)
                throw new InvalidOperationException($"{name} is already initialized.");

            ValidateReferences();

            if (!level)
                throw new ArgumentNullException(nameof(level));

            if (!photoCapture)
                throw new ArgumentNullException(nameof(photoCapture));

            if (storage == null)
                throw new ArgumentNullException(nameof(storage));

            if (backRequested == null)
                throw new ArgumentNullException(nameof(backRequested));

            if (retryRequested == null)
                throw new ArgumentNullException(nameof(retryRequested));

            if (nextRequested == null)
                throw new ArgumentNullException(nameof(nextRequested));

            _level = level;
            _photoCapture = photoCapture;
            _storage = storage;
            _backRequested = backRequested;
            _retryRequested = retryRequested;
            _nextRequested = nextRequested;

            _photoCapture.PhotoCaptured += Show;
            _backButton.onClick.AddListener(OpenLevelSelect);
            _retryButton.onClick.AddListener(ReloadLevel);
            _nextButton.onClick.AddListener(OpenNextLevel);

            _gameHudRoot.SetActive(true);
            _resultRoot.SetActive(false);
            ClearFailures();

            _isInitialized = true;
        }

        private void Show(PhotoResult photo, IReadOnlyList<string> visibleFailures)
        {
            if (photo == null)
                throw new ArgumentNullException(nameof(photo));

            if (photo.LevelId != _level.Id)
                throw new InvalidOperationException($"Photo {photo.Id} belongs to another level.");

            LoadPhoto(photo);
            _levelNameText.text = _level.DisplayName;

            ApplyStars(photo.Rank);

            if (photo.Rank == LevelRank.Failed)
                ApplyFailures(visibleFailures);
            else
                ClearFailures();

            _nextButton.interactable = photo.UnlocksNextLevel;

            _gameHudRoot.SetActive(false);
            _resultRoot.SetActive(true);
        }

        private void ApplyStars(LevelRank rank)
        {
            _firstStar.sprite = rank >= LevelRank.OneStar ? _goldStarSprite : _grayStarSprite;
            _secondStar.sprite = rank >= LevelRank.TwoStars ? _goldStarSprite : _grayStarSprite;

            _firstStar.gameObject.SetActive(true);
            _secondStar.gameObject.SetActive(true);
        }

        private void ApplyFailures(IReadOnlyList<string> failures)
        {
            int visibleCount = Mathf.Min(failures.Count, _failureTexts.Length);

            for (int i = 0; i < _failureTexts.Length; i++)
            {
                bool visible = i < visibleCount;
                TMP_Text failureText = _failureTexts[i];

                failureText.text = visible ? failures[i] : string.Empty;
                failureText.gameObject.SetActive(visible);
            }
        }

        private void ClearFailures()
        {
            foreach (TMP_Text failureText in _failureTexts)
            {
                failureText.text = string.Empty;
                failureText.gameObject.SetActive(false);
            }
        }

        private void LoadPhoto(PhotoResult photo)
        {
            ReleasePhoto();

            try
            {
                byte[] imageBytes = _storage.LoadImageBytes(photo.Image);
                _createdTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);

                if (!_createdTexture.LoadImage(imageBytes))
                    throw new InvalidOperationException($"Failed to decode photo {photo.Id}.");

                _createdTexture.name = $"ResultPhoto_{photo.Id}";
                _createdTexture.wrapMode = TextureWrapMode.Clamp;
                _createdTexture.filterMode = FilterMode.Bilinear;

                _createdSprite = Sprite.Create(
                    _createdTexture,
                    new Rect(0f, 0f, _createdTexture.width, _createdTexture.height),
                    new Vector2(0.5f, 0.5f),
                    100f);

                _createdSprite.name = $"ResultPhotoSprite_{photo.Id}";
                _photoImage.sprite = _createdSprite;
                _photoImage.enabled = true;
            }
            catch (Exception exception)
            {
                ReleasePhoto();
                _photoImage.enabled = false;
                Debug.LogError($"Failed to load result photo {photo.Id}: {exception}", this);
            }
        }

        private void OpenLevelSelect()
        {
            _backRequested();
        }

        private void ReloadLevel()
        {
            _retryRequested();
        }

        private void OpenNextLevel()
        {
            if (_nextButton.interactable)
                _nextRequested();
        }

        private void ReleasePhoto()
        {
            if (_photoImage != null)
                _photoImage.sprite = null;

            if (_createdSprite != null)
                Destroy(_createdSprite);

            if (_createdTexture != null)
                Destroy(_createdTexture);

            _createdSprite = null;
            _createdTexture = null;
        }

        private void OnDestroy()
        {
            if (_photoCapture != null)
                _photoCapture.PhotoCaptured -= Show;

            if (_backButton != null)
                _backButton.onClick.RemoveListener(OpenLevelSelect);

            if (_retryButton != null)
                _retryButton.onClick.RemoveListener(ReloadLevel);

            if (_nextButton != null)
                _nextButton.onClick.RemoveListener(OpenNextLevel);

            ReleasePhoto();
        }

        private void ValidateReferences()
        {
            if (!_gameHudRoot)
                throw new MissingReferenceException($"{name} has no game HUD root.");

            if (!_resultRoot)
                throw new MissingReferenceException($"{name} has no result root.");

            if (!_photoImage)
                throw new MissingReferenceException($"{name} has no photo image.");

            if (!_levelNameText)
                throw new MissingReferenceException($"{name} has no level name text.");

            if (!_firstStar || !_secondStar)
                throw new MissingReferenceException($"{name} has missing star images.");

            if (!_grayStarSprite || !_goldStarSprite)
                throw new MissingReferenceException($"{name} has missing star sprites.");

            if (_failureTexts == null || _failureTexts.Length != 3)
                throw new MissingReferenceException($"{name} must have exactly three failure texts.");

            foreach (TMP_Text failureText in _failureTexts)
            {
                if (!failureText)
                    throw new MissingReferenceException($"{name} has a missing failure text.");
            }

            if (!_backButton || !_retryButton || !_nextButton)
                throw new MissingReferenceException($"{name} has missing result buttons.");
        }
    }
}