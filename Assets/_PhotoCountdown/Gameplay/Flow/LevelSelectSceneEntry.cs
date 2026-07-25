using System.Collections;
using System.Collections.Generic;
using _PhotoCountdown.Gameplay.Levels;
using _PhotoCountdown.Gameplay.Photography;
using _PhotoCountdown.Presentation.Flow;
using _PhotoCountdown.Presentation.UI;
using UnityEngine;
using UnityEngine.UI;

namespace _PhotoCountdown.Gameplay.Flow
{
    public sealed class LevelSelectSceneEntry : GameSceneEntry
    {
        [SerializeField] private Transform _itemsRoot;
        [SerializeField] private Button _backButton;
        [SerializeField] private UISlideInFromBottom _boardAnimation;
        [SerializeField] private LevelPhotoGalleryPresenter _photoGallery;

        private GameFlowController _flow;

        protected override void OnInit(GameSession session, GameFlowController flow)
        {
            ValidateReferences();

            _flow = flow;
            _backButton.onClick.AddListener(OpenMainMenu);
            _photoGallery.Init();

            LevelSelectItem[] items = _itemsRoot.GetComponentsInChildren<LevelSelectItem>(true);
            ValidateItems(session, items);

            foreach (LevelSelectItem item in items)
            {
                LevelRank rank = session.GetBestRank(item.Level);
                bool unlocked = session.IsLevelUnlocked(item.Level);
                IEnumerable<PhotoResult> photos = session.Album.GetLevelPhotos(item.Level.Id);

                item.Init(rank, unlocked, photos, session.PhotoStorage, OpenLevel, OpenGallery);
            }

            _boardAnimation.PlayIn();
        }

        protected override IEnumerator OnExit()
        {
            _photoGallery.CloseImmediately();
            _backButton.interactable = false;

            yield return _boardAnimation.PlayOut();
        }

        private void OnDestroy()
        {
            if (_backButton != null)
                _backButton.onClick.RemoveListener(OpenMainMenu);
        }

        private void OpenLevel(LevelDefinition level)
        {
            _flow.OpenLevel(level);
        }

        private void OpenGallery(LevelDefinition level, IReadOnlyList<PhotoResult> photos,
            PhotoAlbumStorage storage)
        {
            _photoGallery.Open(level, photos, storage);
        }

        private void OpenMainMenu()
        {
            _flow.OpenMainMenu();
        }

        private void ValidateReferences()
        {
            if (!_itemsRoot)
                throw new MissingReferenceException($"{name} has no items root.");

            if (!_backButton)
                throw new MissingReferenceException($"{name} has no back button.");

            if (!_boardAnimation)
                throw new MissingReferenceException($"{name} has no board animation.");

            if (!_photoGallery)
                throw new MissingReferenceException($"{name} has no photo gallery.");
        }

        private static void ValidateItems(GameSession session, LevelSelectItem[] items)
        {
            if (items.Length == 0)
                throw new MissingReferenceException("Level selection has no level items.");

            HashSet<LevelDefinition> usedLevels = new HashSet<LevelDefinition>();

            foreach (LevelSelectItem item in items)
            {
                if (!item.Level)
                    throw new MissingReferenceException($"{item.name} has no level.");

                if (session.Levels.IndexOf(item.Level) < 0)
                {
                    throw new MissingReferenceException(
                        $"{item.Level.name} is not included in the level catalog.");
                }

                if (!usedLevels.Add(item.Level))
                {
                    throw new MissingReferenceException(
                        $"Several level items reference {item.Level.name}.");
                }
            }

            foreach (LevelDefinition level in session.Levels.Levels)
            {
                if (!usedLevels.Contains(level))
                {
                    throw new MissingReferenceException(
                        $"Level selection has no item for {level.name}.");
                }
            }
        }
    }
}