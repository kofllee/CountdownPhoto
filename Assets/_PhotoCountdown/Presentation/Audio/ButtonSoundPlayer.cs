using UnityEngine;
using UnityEngine.UI;

namespace _PhotoCountdown.Presentation.Audio
{
    public sealed class ButtonSoundPlayer : AudioClient
    {
        [SerializeField] private Transform _buttonsRoot;
        [SerializeField] private AudioClip _overrideSound;
        [SerializeField, Range(0f, 1f)] private float _volume = 1f;

        private Button[] _buttons;

        protected override void OnAudioInitialized()
        {
            Transform buttonsRoot = _buttonsRoot ? _buttonsRoot : transform;
            _buttons = buttonsRoot.GetComponentsInChildren<Button>(true);

            foreach (Button button in _buttons)
                button.onClick.AddListener(PlayClick);
        }

        private void PlayClick()
        {
            AudioClip clip = _overrideSound ? _overrideSound : Audio.ButtonClickSound;
            Audio.PlayEffect(clip, _volume);
        }

        private void OnDestroy()
        {
            if (_buttons == null)
                return;

            foreach (Button button in _buttons)
            {
                if (button)
                    button.onClick.RemoveListener(PlayClick);
            }
        }
    }
}