using _PhotoCountdown.Presentation.UI;
using UnityEngine;
using UnityEngine.UI;

namespace _PhotoCountdown.Gameplay.Flow
{
    public sealed class MainMenuSceneEntry : GameSceneEntry
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private SettingsPanelPresenter _settingsPanel;
        [SerializeField] private CreditsPanelPresenter _creditsPanel;

        private GameFlowController _flow;

        protected override void OnInit(GameSession session, GameFlowController flow)
        {
            ValidateReferences();

            _flow = flow;

            _settingsPanel.Init(session);
            _creditsPanel.Init();

            _playButton.onClick.AddListener(OpenLevelSelect);

            if (flow.ConsumeCreditsOpenRequest())
                _creditsPanel.Open();
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

        private void ValidateReferences()
        {
            if (!_playButton)
                throw new MissingReferenceException($"{name} has no play button.");

            if (!_settingsPanel)
                throw new MissingReferenceException($"{name} has no settings panel.");

            if (!_creditsPanel)
                throw new MissingReferenceException($"{name} has no credits panel.");
        }
    }
}