using System;
using System.Collections;
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
        [SerializeField] private RectTransform _photoCard;

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

        [Header("Flash")]
        [SerializeField] private Image _flashImage;
        [SerializeField, Min(0f)] private float _flashHoldDuration = 0.06f;
        [SerializeField, Min(0.01f)] private float _flashFadeDuration = 0.45f;

        [Header("Photo Card Animation")]
        [SerializeField, Min(0f)] private float _cardDelay = 0.05f;
        [SerializeField, Min(0.01f)] private float _cardEnterDuration = 0.55f;
        [SerializeField, Min(0f)] private float _cardStartOffset = 900f;
        [SerializeField, Range(-90f, 90f)] private float _cardStartAngle = 45f;

        private LevelDefinition _level;
        private PhotoCaptureController _photoCapture;
        private PhotoAlbumStorage _storage;
        private Action _backRequested;
        private Action _retryRequested;
        private Action _nextRequested;
        private Texture2D _createdTexture;
        private Sprite _createdSprite;
        private Coroutine _flashRoutine;
        private Coroutine _resultRoutine;
        private Vector2 _photoCardDefaultPosition;
        private float _photoCardDefaultAngle;
        private bool _flashFinished = true;
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

            _photoCardDefaultPosition = _photoCard.anchoredPosition;
            _photoCardDefaultAngle = NormalizeAngle(_photoCard.localEulerAngles.z);

            _photoCapture.PhotoCaptureStarted += HandlePhotoCaptureStarted;
            _photoCapture.PhotoCaptured += Show;
            _backButton.onClick.AddListener(OpenLevelSelect);
            _retryButton.onClick.AddListener(ReloadLevel);
            _nextButton.onClick.AddListener(OpenNextLevel);

            _gameHudRoot.SetActive(true);
            _resultRoot.SetActive(false);

            SetFlashAlpha(0f);
            _flashImage.gameObject.SetActive(false);

            ClearFailures();
            ResetPhotoCard();

            _isInitialized = true;
        }

        private void HandlePhotoCaptureStarted()
        {
            StopFlashRoutine();

            _flashFinished = false;
            _flashImage.gameObject.SetActive(true);
            _flashImage.transform.SetAsLastSibling();
            SetFlashAlpha(1f);

            _flashRoutine = StartCoroutine(PlayFlash());
        }

        private void Show(PhotoResult photo, IReadOnlyList<string> visibleFailures)
        {
            if (photo == null)
                throw new ArgumentNullException(nameof(photo));

            if (photo.LevelId != _level.Id)
                throw new InvalidOperationException($"Photo {photo.Id} belongs to another level.");

            StopResultRoutine();
            _resultRoutine = StartCoroutine(ShowResult(photo, visibleFailures));
        }

        private IEnumerator PlayFlash()
        {
            if (_flashHoldDuration > 0f)
                yield return new WaitForSecondsRealtime(_flashHoldDuration);

            float elapsedTime = 0f;

            while (elapsedTime < _flashFadeDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;

                float progress = Mathf.Clamp01(elapsedTime / _flashFadeDuration);
                float easedProgress = SmoothStep(progress);

                SetFlashAlpha(1f - easedProgress);
                yield return null;
            }

            SetFlashAlpha(0f);
            _flashImage.gameObject.SetActive(false);

            _flashFinished = true;
            _flashRoutine = null;
        }

        private IEnumerator ShowResult(
            PhotoResult photo,
            IReadOnlyList<string> visibleFailures)
        {
            LoadPhoto(photo);
            _levelNameText.text = _level.DisplayName;

            ApplyStars(photo.Rank);

            if (photo.Rank == LevelRank.Failed)
                ApplyFailures(visibleFailures);
            else
                ClearFailures();

            SetButtonsInteractable(false, photo.UnlocksNextLevel);
            PreparePhotoCard();

            while (!_flashFinished)
                yield return null;

            if (_cardDelay > 0f)
                yield return new WaitForSecondsRealtime(_cardDelay);

            _gameHudRoot.SetActive(false);
            _resultRoot.SetActive(true);

            Vector2 startPosition = _photoCard.anchoredPosition;
            float startAngle = NormalizeAngle(_photoCard.localEulerAngles.z);
            float elapsedTime = 0f;

            while (elapsedTime < _cardEnterDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;

                float progress = Mathf.Clamp01(elapsedTime / _cardEnterDuration);
                float positionProgress = EaseOutCubic(progress);
                float rotationProgress = SmoothStep(progress);

                _photoCard.anchoredPosition = Vector2.LerpUnclamped(
                    startPosition,
                    _photoCardDefaultPosition,
                    positionProgress);

                float angle = Mathf.LerpUnclamped(
                    startAngle,
                    _photoCardDefaultAngle,
                    rotationProgress);

                _photoCard.localRotation = Quaternion.Euler(0f, 0f, angle);

                yield return null;
            }

            ResetPhotoCard();
            SetButtonsInteractable(true, photo.UnlocksNextLevel);
            _resultRoutine = null;
        }

        private void PreparePhotoCard()
        {
            _photoCard.anchoredPosition =
                _photoCardDefaultPosition + Vector2.up * _cardStartOffset;

            float angle = _photoCardDefaultAngle + _cardStartAngle;
            _photoCard.localRotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void ResetPhotoCard()
        {
            _photoCard.anchoredPosition = _photoCardDefaultPosition;
            _photoCard.localRotation =
                Quaternion.Euler(0f, 0f, _photoCardDefaultAngle);
        }

        private void ApplyStars(LevelRank rank)
        {
            _firstStar.sprite =
                rank >= LevelRank.OneStar ? _goldStarSprite : _grayStarSprite;

            _secondStar.sprite =
                rank >= LevelRank.TwoStars ? _goldStarSprite : _grayStarSprite;

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

        private void SetButtonsInteractable(bool interactable, bool nextUnlocked)
        {
            _backButton.interactable = interactable;
            _retryButton.interactable = interactable;
            _nextButton.interactable = interactable && nextUnlocked;
        }

        private void SetFlashAlpha(float alpha)
        {
            Color color = _flashImage.color;
            color.a = alpha;
            _flashImage.color = color;
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

        private void StopFlashRoutine()
        {
            if (_flashRoutine == null)
                return;

            StopCoroutine(_flashRoutine);
            _flashRoutine = null;
        }

        private void StopResultRoutine()
        {
            if (_resultRoutine == null)
                return;

            StopCoroutine(_resultRoutine);
            _resultRoutine = null;
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

        private static float SmoothStep(float progress)
        {
            return progress * progress * (3f - 2f * progress);
        }

        private static float EaseOutCubic(float progress)
        {
            float remaining = 1f - progress;
            return 1f - remaining * remaining * remaining;
        }

        private static float NormalizeAngle(float angle)
        {
            return angle > 180f ? angle - 360f : angle;
        }

        private void OnDestroy()
        {
            StopFlashRoutine();
            StopResultRoutine();

            if (_photoCapture != null)
            {
                _photoCapture.PhotoCaptureStarted -= HandlePhotoCaptureStarted;
                _photoCapture.PhotoCaptured -= Show;
            }

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

            if (!_photoCard)
                throw new MissingReferenceException($"{name} has no photo card.");

            if (!_firstStar || !_secondStar)
                throw new MissingReferenceException($"{name} has missing star images.");

            if (!_grayStarSprite || !_goldStarSprite)
                throw new MissingReferenceException($"{name} has missing star sprites.");

            if (_failureTexts == null || _failureTexts.Length != 3)
                throw new MissingReferenceException($"{name} must have three failure texts.");

            foreach (TMP_Text failureText in _failureTexts)
            {
                if (!failureText)
                    throw new MissingReferenceException($"{name} has a missing failure text.");
            }

            if (!_backButton || !_retryButton || !_nextButton)
                throw new MissingReferenceException($"{name} has missing result buttons.");

            if (!_flashImage)
                throw new MissingReferenceException($"{name} has no flash image.");
        }
    }
}