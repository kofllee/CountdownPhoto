using UnityEngine;
using UnityEngine.UI;

namespace _PhotoCountdown.Gameplay.Flow
{
    public class MainMenuSceneEntry : GameSceneEntry
    {
        [SerializeField] private Button _playButton;

        private GameFlowController _flow;

        protected override void OnInit(GameSession session, GameFlowController flow)
        {
            if (!_playButton)
                throw new MissingReferenceException($"{name} has no play button.");

            _flow = flow;
            _playButton.onClick.AddListener(OpenLevelSelect);
        }

        private void OnDestroy()
        {
            if (_playButton)
                _playButton.onClick.RemoveListener(OpenLevelSelect);
        }

        private void OpenLevelSelect()
        {
            _flow.OpenLevelSelect();
        }
    }
}