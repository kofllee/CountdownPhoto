using UnityEngine;

namespace _PhotoCountdown.Presentation.Audio
{
    public sealed class SceneMusicPlayer : AudioClient
    {
        [SerializeField] private AudioClip _music;
        [SerializeField] private bool _restartWhenSceneOpens = false;

        protected override void OnAudioInitialized()
        {
            if (_music)
                Audio.PlayMusic(_music, _restartWhenSceneOpens);
        }
    }
}