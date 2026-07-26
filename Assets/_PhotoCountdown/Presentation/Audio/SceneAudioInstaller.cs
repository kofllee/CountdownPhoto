using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _PhotoCountdown.Presentation.Audio
{
    public sealed class SceneAudioInstaller : MonoBehaviour
    {
        private bool _isInitialized;

        public void Init(GameAudio audio)
        {
            if (_isInitialized)
                throw new InvalidOperationException($"{name} is already initialized.");

            if (!audio)
                throw new ArgumentNullException(nameof(audio));

            Scene scene = gameObject.scene;

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                AudioClient[] clients = root.GetComponentsInChildren<AudioClient>(true);

                foreach (AudioClient client in clients)
                    client.InitAudio(audio);
            }

            _isInitialized = true;
        }
    }
}