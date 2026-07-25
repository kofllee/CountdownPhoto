using System;
using _PhotoCountdown.Gameplay.Levels;
using _PhotoCountdown.Gameplay.Photography;
using UnityEngine;

namespace _PhotoCountdown.Gameplay.Flow
{
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private LevelCatalog _levelCatalog;
        [SerializeField] private GameFlowController _flow;

        private void Awake()
        {
            ValidateReferences();

            if (transform.parent != null)
                throw new InvalidOperationException($"{name} must be a root GameObject.");

            DontDestroyOnLoad(gameObject);

            _levelCatalog.Validate();

            PhotoAlbumStorage storage =
                new PhotoAlbumStorage(Application.persistentDataPath);
            PhotoAlbum album = storage.Load();
            GameSession session = new GameSession(_levelCatalog, album, storage);

            Debug.Log($"Loaded {album.Photos.Count} saved photos.");

            _flow.Init(session);
            _flow.OpenMainMenu();
        }

        private void ValidateReferences()
        {
            if (_levelCatalog == null)
                throw new MissingReferenceException($"{name} has no level catalog.");

            if (_flow == null)
                throw new MissingReferenceException($"{name} has no game flow controller.");
        }
    }
}