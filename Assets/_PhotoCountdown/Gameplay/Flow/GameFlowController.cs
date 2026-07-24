using System;
using System.Collections;
using _PhotoCountdown.Gameplay.Levels;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _PhotoCountdown.Gameplay.Flow
{
    public sealed class GameFlowController : MonoBehaviour
    {
        [SerializeField] private string _mainMenuSceneName = "MainMenu";
        [SerializeField] private string _levelSelectSceneName = "LevelSelect";

        private GameSession _session;
        private bool _isInitialized;
        private bool _isTransitioning;

        public LevelDefinition CurrentLevel { get; private set; }
        public bool IsTransitioning => _isTransitioning;

        public void Init(GameSession session)
        {
            if (_isInitialized)
                throw new InvalidOperationException($"{name} is already initialized.");

            if (session == null)
                throw new ArgumentNullException(nameof(session));

            if (string.IsNullOrWhiteSpace(_mainMenuSceneName))
                throw new MissingReferenceException($"{name} has no main menu scene name.");

            if (string.IsNullOrWhiteSpace(_levelSelectSceneName))
                throw new MissingReferenceException($"{name} has no level select scene name.");

            _session = session;
            _isInitialized = true;
        }

        public void OpenMainMenu()
        {
            if (BeginLoad(_mainMenuSceneName))
                CurrentLevel = null;
        }

        public void OpenLevelSelect()
        {
            if (BeginLoad(_levelSelectSceneName))
                CurrentLevel = null;
        }

        public void OpenLevel(LevelDefinition level)
        {
            if (!level)
                throw new ArgumentNullException(nameof(level));

            if (!_session.IsLevelUnlocked(level))
                return;

            if (BeginLoad(level.SceneName))
                CurrentLevel = level;
        }

        public void ReloadCurrentLevel()
        {
            if (!CurrentLevel)
                return;

            BeginLoad(CurrentLevel.SceneName);
        }

        public void OpenNextLevel()
        {
            if (!CurrentLevel)
                return;

            LevelDefinition nextLevel = _session.Levels.GetNext(CurrentLevel);

            if (nextLevel == null)
            {
                OpenLevelSelect();
                return;
            }

            OpenLevel(nextLevel);
        }

        private bool BeginLoad(string sceneName)
        {
            if (!_isInitialized)
                throw new InvalidOperationException($"{name} is not initialized.");

            if (_isTransitioning)
                return false;

            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                throw new InvalidOperationException(
                    $"Scene {sceneName} is not included in the current build profile.");
            }

            _isTransitioning = true;
            StartCoroutine(LoadSceneRoutine(sceneName));
            return true;
        }

        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

            if (operation == null)
            {
                _isTransitioning = false;
                throw new InvalidOperationException($"Failed to start loading {sceneName}.");
            }

            while (!operation.isDone)
                yield return null;

            try
            {
                Scene scene = SceneManager.GetActiveScene();

                if (scene.name != sceneName)
                    throw new InvalidOperationException($"Loaded unexpected scene {scene.name}.");

                GameSceneEntry entry = FindSceneEntry(scene);
                entry.Init(_session, this);
            }
            finally
            {
                _isTransitioning = false;
            }
        }

        private static GameSceneEntry FindSceneEntry(Scene scene)
        {
            GameSceneEntry foundEntry = null;

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                GameSceneEntry[] entries = root.GetComponentsInChildren<GameSceneEntry>(true);

                foreach (GameSceneEntry entry in entries)
                {
                    if (foundEntry)
                    {
                        throw new InvalidOperationException(
                            $"Scene {scene.name} contains several game scene entries.");
                    }

                    foundEntry = entry;
                }
            }

            if (!foundEntry)
                throw new InvalidOperationException($"Scene {scene.name} contains no game scene entry.");

            return foundEntry;
        }
    }
}