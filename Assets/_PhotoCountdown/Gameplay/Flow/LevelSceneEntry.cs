using System;
using _PhotoCountdown.Gameplay.Levels;
using _PhotoCountdown.Presentation.Flow;
using UnityEngine;
using UnityEngine.UI;

namespace _PhotoCountdown.Gameplay.Flow
{
    public sealed class LevelSceneEntry : GameSceneEntry
    {
        [SerializeField] private LevelDefinition _level;
        [SerializeField] private LevelController _levelController;
        [SerializeField] private PhotoResultPresenter _resultPresenter;
        [SerializeField] private Button _backButton;

        private GameFlowController _flow;

        protected override void OnInit(GameSession session, GameFlowController flow)
        {
            ValidateReferences();

            if (flow.CurrentLevel != _level)
            {
                throw new InvalidOperationException(
                    $"{name} level does not match the level selected by game flow.");
            }

            _flow = flow;

            _levelController.Init(_level, session.Album, session.PhotoStorage);

            _resultPresenter.Init(
                _level,
                _levelController.PhotoCapture,
                session.PhotoStorage,
                flow.OpenLevelSelect,
                flow.ReloadCurrentLevel,
                flow.OpenNextLevel);

            _backButton.onClick.AddListener(OpenLevelSelect);
        }

        private void OnDestroy()
        {
            if (_backButton != null)
                _backButton.onClick.RemoveListener(OpenLevelSelect);
        }

        private void OpenLevelSelect()
        {
            _flow.OpenLevelSelect();
        }

        private void ValidateReferences()
        {
            if (!_level)
                throw new MissingReferenceException($"{name} has no level definition.");

            if (!_levelController)
                throw new MissingReferenceException($"{name} has no level controller.");

            if (!_resultPresenter)
                throw new MissingReferenceException($"{name} has no result presenter.");

            if (!_backButton)
                throw new MissingReferenceException($"{name} has no back button.");
        }
    }
}