using System;
using UnityEngine;

namespace _PhotoCountdown.Presentation.Audio
{
    public abstract class AudioClient : MonoBehaviour
    {
        protected GameAudio Audio { get; private set; }

        public bool IsAudioInitialized => Audio != null;

        public void InitAudio(GameAudio audio)
        {
            if (Audio != null)
                throw new InvalidOperationException($"{name} audio is already initialized.");

            Audio = audio ? audio : throw new ArgumentNullException(nameof(audio));
            OnAudioInitialized();
        }

        protected virtual void OnAudioInitialized()
        {
        }
    }
}