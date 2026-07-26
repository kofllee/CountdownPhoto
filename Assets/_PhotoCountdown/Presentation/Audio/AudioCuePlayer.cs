using UnityEngine;

namespace _PhotoCountdown.Presentation.Audio
{
    public sealed class AudioCuePlayer : AudioClient
    {
        [SerializeField] private AudioClip[] _sounds;
        [SerializeField, Range(0f, 1f)] private float _chance = 1f;
        [SerializeField, Range(0f, 1f)] private float _volume = 1f;

        public void Play()
        {
            if (!IsAudioInitialized || _sounds == null || _sounds.Length == 0)
                return;

            if (Random.value > _chance)
                return;

            AudioClip clip = _sounds[Random.Range(0, _sounds.Length)];

            if (clip)
                Audio.PlayEffect(clip, _volume);
        }
    }
}