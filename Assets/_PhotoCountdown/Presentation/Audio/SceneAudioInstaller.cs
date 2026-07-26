using System;
using UnityEngine;

namespace _PhotoCountdown.Presentation.Audio
{
    public sealed class SceneAudioInstaller : MonoBehaviour
    {
        [SerializeField] private Transform _audioClientsRoot;

        private bool _isInitialized;

        public void Init(GameAudio audio)
        {
            if (_isInitialized)
                throw new InvalidOperationException($"{name} is already initialized.");

            if (!audio)
                throw new ArgumentNullException(nameof(audio));

            Transform clientsRoot = _audioClientsRoot ? _audioClientsRoot : transform;
            AudioClient[] clients = clientsRoot.GetComponentsInChildren<AudioClient>(true);

            foreach (AudioClient client in clients)
                client.InitAudio(audio);

            _isInitialized = true;
        }
    }
}