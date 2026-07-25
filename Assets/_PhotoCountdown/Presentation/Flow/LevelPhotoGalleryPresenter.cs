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
    public sealed class LevelPhotoGalleryPresenter : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField] private GameObject _root;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _panel;

        [Header("Photo")]
        [SerializeField] private Image _photoImage;
        [SerializeField] private TMP_Text _levelNameText;
        [SerializeField] private TMP_Text _counterText;

        [Header("Buttons")]
        [SerializeField] private Button _previousButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _closeButton;

        [Header("Animation")]
        [SerializeField, Min(0.01f)] private float _animationDuration = 0.14f;
        [SerializeField, Range(0.5f, 1f)] private float _hiddenScale = 0.94f;

        private readonly List<PhotoResult> _photos = new();

        private PhotoAlbumStorage _storage;
        private Texture2D _createdTexture;
        private Sprite _createdSprite;
        private Coroutine _animation;
        private int _currentIndex;
        private bool _isInitialized;

        public bool IsOpen => _root != null && _root.activeSelf;

        public void Init()
        {
            if (_isInitialized)
                throw new InvalidOperationException($"{name} is already initialized.");

            ValidateReferences();

            _previousButton.onClick.AddListener(ShowPrevious);
            _nextButton.onClick.AddListener(ShowNext);
            _closeButton.onClick.AddListener(Close);

            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            _root.SetActive(false);

            _isInitialized = true;
        }

        public void Open(LevelDefinition level, IReadOnlyList<PhotoResult> photos,
            PhotoAlbumStorage storage)
        {
            if (!_isInitialized)
                throw new InvalidOperationException($"{name} is not initialized.");

            if (level == null)
                throw new ArgumentNullException(nameof(level));

            if (photos == null)
                throw new ArgumentNullException(nameof(photos));

            if (storage == null)
                throw new ArgumentNullException(nameof(storage));

            _photos.Clear();

            for (int i = 0; i < photos.Count; i++)
            {
                if (photos[i] != null)
                    _photos.Add(photos[i]);
            }

            if (_photos.Count == 0)
                return;

            _photos.Sort(CompareNewestFirst);
            _storage = storage;
            _currentIndex = 0;

            StopAnimation();
            ReleaseCreatedPhoto();

            _levelNameText.text = level.DisplayName;
            _root.SetActive(true);

            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            _panel.localScale = Vector3.one * _hiddenScale;

            ShowCurrentPhoto();
            _animation = StartCoroutine(PlayOpenAnimation());
        }

        public void Close()
        {
            if (!IsOpen)
                return;

            StopAnimation();

            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            _animation = StartCoroutine(PlayCloseAnimation());
        }

        public void CloseImmediately()
        {
            StopAnimation();
            ReleaseCreatedPhoto();

            if (_root == null)
                return;

            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            _panel.localScale = Vector3.one;
            _root.SetActive(false);
        }

        private void ShowPrevious()
        {
            if (_photos.Count <= 1)
                return;

            _currentIndex = (_currentIndex - 1 + _photos.Count) % _photos.Count;
            ShowCurrentPhoto();
        }

        private void ShowNext()
        {
            if (_photos.Count <= 1)
                return;

            _currentIndex = (_currentIndex + 1) % _photos.Count;
            ShowCurrentPhoto();
        }

        private void ShowCurrentPhoto()
        {
            ReleaseCreatedPhoto();

            PhotoResult photo = _photos[_currentIndex];
            bool loaded = TryCreateSprite(photo, out _createdTexture, out _createdSprite);

            _photoImage.enabled = loaded;
            _photoImage.sprite = loaded ? _createdSprite : null;
            _counterText.text = $"{_currentIndex + 1} / {_photos.Count}";

            bool hasSeveralPhotos = _photos.Count > 1;
            _previousButton.interactable = hasSeveralPhotos;
            _nextButton.interactable = hasSeveralPhotos;
        }

        private bool TryCreateSprite(PhotoResult photo, out Texture2D texture, out Sprite sprite)
        {
            texture = null;
            sprite = null;

            try
            {
                byte[] imageBytes = _storage.LoadImageBytes(photo.Image);
                texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);

                if (!texture.LoadImage(imageBytes))
                {
                    Destroy(texture);
                    texture = null;

                    Debug.LogError($"Failed to decode photo {photo.Id}.", this);
                    return false;
                }

                texture.name = $"GalleryPhoto_{photo.Id}";

                sprite = Sprite.Create(texture,
                    new Rect(0f, 0f, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f), 100f);

                sprite.name = $"GalleryPhotoSprite_{photo.Id}";
                return true;
            }
            catch (Exception exception)
            {
                if (texture != null)
                    Destroy(texture);

                texture = null;
                sprite = null;

                Debug.LogError($"Failed to load photo {photo.Id}: {exception}", this);
                return false;
            }
        }

        private IEnumerator PlayOpenAnimation()
        {
            float elapsedTime = 0f;

            while (elapsedTime < _animationDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;

                float progress = Mathf.Clamp01(elapsedTime / _animationDuration);
                float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);

                _canvasGroup.alpha = easedProgress;

                float scale = Mathf.LerpUnclamped(_hiddenScale, 1f, easedProgress);
                _panel.localScale = Vector3.one * scale;

                yield return null;
            }

            _canvasGroup.alpha = 1f;
            _panel.localScale = Vector3.one;
            _animation = null;
        }

        private IEnumerator PlayCloseAnimation()
        {
            float startAlpha = _canvasGroup.alpha;
            float startScale = _panel.localScale.x;
            float elapsedTime = 0f;

            while (elapsedTime < _animationDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;

                float progress = Mathf.Clamp01(elapsedTime / _animationDuration);
                float easedProgress = progress * progress * progress;

                _canvasGroup.alpha = Mathf.LerpUnclamped(startAlpha, 0f, easedProgress);

                float scale = Mathf.LerpUnclamped(startScale, _hiddenScale, easedProgress);
                _panel.localScale = Vector3.one * scale;

                yield return null;
            }

            ReleaseCreatedPhoto();

            _canvasGroup.alpha = 0f;
            _panel.localScale = Vector3.one;
            _animation = null;
            _root.SetActive(false);
        }

        private void ReleaseCreatedPhoto()
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

        private void StopAnimation()
        {
            if (_animation == null)
                return;

            StopCoroutine(_animation);
            _animation = null;
        }

        private static int CompareNewestFirst(PhotoResult first, PhotoResult second)
        {
            return second.CapturedAtUtcTicks.CompareTo(first.CapturedAtUtcTicks);
        }

        private void OnDestroy()
        {
            StopAnimation();
            ReleaseCreatedPhoto();

            if (_previousButton != null)
                _previousButton.onClick.RemoveListener(ShowPrevious);

            if (_nextButton != null)
                _nextButton.onClick.RemoveListener(ShowNext);

            if (_closeButton != null)
                _closeButton.onClick.RemoveListener(Close);
        }

        private void ValidateReferences()
        {
            if (_root == null)
                throw new MissingReferenceException($"{name} has no root.");

            if (_canvasGroup == null)
                throw new MissingReferenceException($"{name} has no canvas group.");

            if (_panel == null)
                throw new MissingReferenceException($"{name} has no panel.");

            if (_photoImage == null)
                throw new MissingReferenceException($"{name} has no photo image.");

            if (_levelNameText == null)
                throw new MissingReferenceException($"{name} has no level name text.");

            if (_counterText == null)
                throw new MissingReferenceException($"{name} has no counter text.");

            if (_previousButton == null)
                throw new MissingReferenceException($"{name} has no previous button.");

            if (_nextButton == null)
                throw new MissingReferenceException($"{name} has no next button.");

            if (_closeButton == null)
                throw new MissingReferenceException($"{name} has no close button.");
        }
    }
}