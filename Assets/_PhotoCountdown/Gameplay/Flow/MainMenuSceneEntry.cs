using _PhotoCountdown.Presentation.UI;
using UnityEngine;
using UnityEngine.UI;

namespace _PhotoCountdown.Gameplay.Flow
{
    public class MainMenuSceneEntry : GameSceneEntry
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private SettingsPanelPresenter _settingsPanel;

        private GameFlowController _flow;

        protected override void OnInit(GameSession session, GameFlowController flow)
        {
            if (!_playButton)
                throw new MissingReferenceException($"{name} has no play button.");

            if (!_settingsPanel)
                throw new MissingReferenceException($"{name} has no settings panel.");

            _flow = flow;
            _settingsPanel.Init(session);
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