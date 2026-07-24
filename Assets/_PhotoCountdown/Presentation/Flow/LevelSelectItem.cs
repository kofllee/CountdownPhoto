using System;
using _PhotoCountdown.Gameplay.Levels;
using UnityEngine;
using UnityEngine.UI;

namespace _PhotoCountdown.Presentation.Flow
{
    public sealed class LevelSelectItem : MonoBehaviour
    {
        [SerializeField] private LevelDefinition _level;
        [SerializeField] private Button _button;
        [SerializeField] private GameObject _lockedView;
        [SerializeField] private GameObject _oneStarView;
        [SerializeField] private GameObject _twoStarView;

        private Action<LevelDefinition> _selected;
        private bool _isInitialized;

        public LevelDefinition Level => _level;

        public void Init(LevelRank rank, bool unlocked, Action<LevelDefinition> selected)
        {
            if (_isInitialized)
                throw new InvalidOperationException($"{name} is already initialized.");

            ValidateReferences();

            _selected = selected ?? throw new ArgumentNullException(nameof(selected));

            _button.interactable = unlocked;
            _lockedView.SetActive(!unlocked);
            _oneStarView.SetActive(rank >= LevelRank.OneStar);
            _twoStarView.SetActive(rank >= LevelRank.TwoStars);
            _button.onClick.AddListener(SelectLevel);

            _isInitialized = true;
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(SelectLevel);
        }

        private void SelectLevel()
        {
            _selected.Invoke(_level);
        }

        private void ValidateReferences()
        {
            if (_level == null)
                throw new MissingReferenceException($"{name} has no level definition.");

            if (_button == null)
                throw new MissingReferenceException($"{name} has no button.");

            if (_lockedView == null)
                throw new MissingReferenceException($"{name} has no locked view.");

            if (_oneStarView == null)
                throw new MissingReferenceException($"{name} has no one-star view.");

            if (_twoStarView == null)
                throw new MissingReferenceException($"{name} has no two-star view.");
        }
    }
}