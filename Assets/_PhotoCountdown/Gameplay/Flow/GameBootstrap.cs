using System;
using _PhotoCountdown.Core.Settings;
using _PhotoCountdown.Gameplay.Levels;
using _PhotoCountdown.Gameplay.Photography;
using _PhotoCountdown.Presentation.Audio;
using UnityEngine;

namespace _PhotoCountdown.Gameplay.Flow
{
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private LevelCatalog _levelCatalog;
        [SerializeField] private GameFlowController _flow;
        [SerializeField] private GameAudio _audio;

        private GameSession _session;

        private void Awake()
        {
            ValidateReferences();

            if (transform.parent != null)
                throw new InvalidOperationException($"{name} must be a root GameObject.");

            DontDestroyOnLoad(gameObject);

            _levelCatalog.Validate();

            PhotoAlbumStorage photoStorage =
                new PhotoAlbumStorage(Application.persistentDataPath);

            PhotoAlbum album = photoStorage.Load();

            GameSettingsStorage settingsStorage = new GameSettingsStorage();
            GameSettings settings = settingsStorage.Load();

            _audio.Init(settings);

            _session = new GameSession(
                _levelCatalog,
                album,
                photoStorage,
                settings,
                settingsStorage,
                _audio);

            Debug.Log($"Loaded {album.Photos.Count} saved photos.");

            _flow.Init(_session);
            _flow.OpenMainMenu();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
                _session?.SaveSettings();
        }

        private void OnApplicationQuit()
        {
            _session?.SaveSettings();
        }

        private void ValidateReferences()
        {
            if (!_levelCatalog)
                throw new MissingReferenceException($"{name} has no level catalog.");

            if (!_flow)
                throw new MissingReferenceException($"{name} has no game flow controller.");

            if (!_audio)
                throw new MissingReferenceException($"{name} has no game audio.");
        }
    }
}