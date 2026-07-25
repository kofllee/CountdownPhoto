using System;
using System.Collections.Generic;
using _PhotoCountdown.Gameplay.Levels;
using _PhotoCountdown.Gameplay.Photography;
using UnityEngine;
using UnityEngine.UI;

namespace _PhotoCountdown.Presentation.Flow
{
    public sealed class LevelSelectItem : MonoBehaviour
    {
        [SerializeField] private LevelDefinition _level;
        [SerializeField] private Button _button;
        [SerializeField] private GameObject _lockedView;

        [Header("Stars")]
        [SerializeField] private Image _star01;
        [SerializeField] private Image _star02;
        [SerializeField] private Sprite _grayStarSprite;
        [SerializeField] private Sprite _goldStarSprite;

        [Header("Photos")]
        [SerializeField] private LevelPhotoStackPresenter _photoStack;

        private Action<LevelDefinition> _selected;
        private bool _isInitialized;

        public LevelDefinition Level => _level;

        public void Init(
            LevelRank rank,
            bool unlocked,
            IEnumerable<PhotoResult> photos,
            PhotoAlbumStorage photoStorage,
            Action<LevelDefinition> selected)
        {
            if (_isInitialized)
                throw new InvalidOperationException($"{name} is already initialized.");

            ValidateReferences();

            _selected = selected ?? throw new ArgumentNullException(nameof(selected));

            _button.interactable = unlocked;
            _lockedView.SetActive(!unlocked);

            UpdateStars(rank);
            _photoStack.Show(photos, photoStorage);

            _button.onClick.AddListener(SelectLevel);
            _isInitialized = true;
        }

        private void UpdateStars(LevelRank rank)
        {
            _star01.sprite = rank >= LevelRank.OneStar ? _goldStarSprite : _grayStarSprite;
            _star02.sprite = rank >= LevelRank.TwoStars ? _goldStarSprite : _grayStarSprite;
        }

        private void SelectLevel()
        {
            _selected.Invoke(_level);
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(SelectLevel);
        }

        private void ValidateReferences()
        {
            if (_level == null)
                throw new MissingReferenceException($"{name} has no level definition.");

            if (_button == null)
                throw new MissingReferenceException($"{name} has no button.");

            if (_lockedView == null)
                throw new MissingReferenceException($"{name} has no locked view.");

            if (_star01 == null)
                throw new MissingReferenceException($"{name} has no first star.");

            if (_star02 == null)
                throw new MissingReferenceException($"{name} has no second star.");

            if (_grayStarSprite == null)
                throw new MissingReferenceException($"{name} has no gray star sprite.");

            if (_goldStarSprite == null)
                throw new MissingReferenceException($"{name} has no gold star sprite.");

            if (_photoStack == null)
                throw new MissingReferenceException($"{name} has no photo stack.");
        }
    }
}