using System;
using System.Collections.Generic;
using _PhotoCountdown.Gameplay.Photography;
using _PhotoCountdown.Presentation.UI;
using UnityEngine;
using UnityEngine.UI;

namespace _PhotoCountdown.Presentation.Flow
{
    public sealed class LevelPhotoStackPresenter : MonoBehaviour
    {
        [Header("Container")]
        [SerializeField] private Button _photosButton;
        [SerializeField] private BlockParentDrag _blockParentDrag;
        [SerializeField] private GameObject _emptyView;
        [SerializeField] private Image _stackSpriteImage;

        [Header("Stack Sprites")]
        [SerializeField] private Sprite _onePhotoSprite;
        [SerializeField] private Sprite _twoPhotosSprite;
        [SerializeField] private Sprite _threePhotosSprite;

        [Header("Photos")]
        [SerializeField] private PhotoView _frontPhoto;
        [SerializeField] private PhotoView _middlePhoto;
        [SerializeField] private PhotoView _backPhoto;

        private readonly List<Texture2D> _createdTextures = new();
        private readonly List<Sprite> _createdSprites = new();

        public void Show(IEnumerable<PhotoResult> photos, PhotoAlbumStorage storage)
        {
            if (photos == null)
                throw new ArgumentNullException(nameof(photos));

            if (storage == null)
                throw new ArgumentNullException(nameof(storage));

            ValidateReferences();
            ClearPhotos();

            List<PhotoResult> sortedPhotos = new List<PhotoResult>(photos);
            sortedPhotos.Sort(CompareNewestFirst);

            PhotoView[] views = { _frontPhoto, _middlePhoto, _backPhoto };
            int shownCount = 0;

            foreach (PhotoResult photo in sortedPhotos)
            {
                if (shownCount >= views.Length)
                    break;

                if (!TryCreateSprite(photo, storage, out Sprite sprite))
                    continue;

                views[shownCount].Show(sprite);
                shownCount++;
            }

            UpdateContainer(shownCount);
        }

        private void UpdateContainer(int photoCount)
        {
            bool hasPhotos = photoCount > 0;

            _emptyView.SetActive(!hasPhotos);
            _photosButton.interactable = hasPhotos;
            _blockParentDrag.enabled = hasPhotos;
            _stackSpriteImage.enabled = hasPhotos;

            if (!hasPhotos)
                return;

            _stackSpriteImage.sprite = photoCount switch
            {
                1 => _onePhotoSprite,
                2 => _twoPhotosSprite,
                _ => _threePhotosSprite
            };
        }

        private bool TryCreateSprite(PhotoResult photo, PhotoAlbumStorage storage, out Sprite sprite)
        {
            sprite = null;
            Texture2D texture = null;

            try
            {
                byte[] imageBytes = storage.LoadImageBytes(photo.Image);
                texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);

                if (!texture.LoadImage(imageBytes))
                {
                    Destroy(texture);
                    Debug.LogError($"Failed to decode photo {photo.Id}.");
                    return false;
                }

                texture.name = $"Photo_{photo.Id}";
                sprite = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    100f);

                sprite.name = $"PhotoSprite_{photo.Id}";

                _createdTextures.Add(texture);
                _createdSprites.Add(sprite);
                return true;
            }
            catch (Exception exception)
            {
                if (texture != null)
                    Destroy(texture);

                Debug.LogError($"Failed to load photo {photo.Id}: {exception}");
                return false;
            }
        }

        private static int CompareNewestFirst(PhotoResult first, PhotoResult second)
        {
            return second.CapturedAtUtcTicks.CompareTo(first.CapturedAtUtcTicks);
        }

        private void ClearPhotos()
        {
            _frontPhoto?.Hide();
            _middlePhoto?.Hide();
            _backPhoto?.Hide();

            foreach (Sprite sprite in _createdSprites)
                Destroy(sprite);

            foreach (Texture2D texture in _createdTextures)
                Destroy(texture);

            _createdSprites.Clear();
            _createdTextures.Clear();
        }

        private void OnDestroy()
        {
            ClearPhotos();
        }

        private void ValidateReferences()
        {
            if (_photosButton == null)
                throw new MissingReferenceException($"{name} has no photos button.");

            if (_blockParentDrag == null)
                throw new MissingReferenceException($"{name} has no block parent drag.");

            if (_emptyView == null)
                throw new MissingReferenceException($"{name} has no empty view.");

            if (_stackSpriteImage == null)
                throw new MissingReferenceException($"{name} has no stack sprite image.");

            if (_onePhotoSprite == null)
                throw new MissingReferenceException($"{name} has no one photo sprite.");

            if (_twoPhotosSprite == null)
                throw new MissingReferenceException($"{name} has no two photos sprite.");

            if (_threePhotosSprite == null)
                throw new MissingReferenceException($"{name} has no three photos sprite.");

            ValidatePhoto(_frontPhoto, "front");
            ValidatePhoto(_middlePhoto, "middle");
            ValidatePhoto(_backPhoto, "back");
        }

        private void ValidatePhoto(PhotoView photo, string photoName)
        {
            if (photo == null)
                throw new MissingReferenceException($"{name} has no {photoName} photo.");

            photo.Validate(name, photoName);
        }

        [Serializable]
        private sealed class PhotoView
        {
            [SerializeField] private GameObject _root;
            [SerializeField] private Image _image;

            public void Show(Sprite sprite)
            {
                _image.sprite = sprite;
                _root.SetActive(true);
            }

            public void Hide()
            {
                if (_image != null)
                    _image.sprite = null;

                if (_root != null)
                    _root.SetActive(false);
            }

            public void Validate(string ownerName, string photoName)
            {
                if (_root == null)
                    throw new MissingReferenceException($"{ownerName} {photoName} has no root.");

                if (_image == null)
                    throw new MissingReferenceException($"{ownerName} {photoName} has no image.");
            }
        }
    }
}