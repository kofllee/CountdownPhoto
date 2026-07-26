using System;
using UnityEngine;

namespace _PhotoCountdown.Gameplay.Levels
{
    [CreateAssetMenu(fileName = "Level_", menuName = "Photo Countdown/Level")]
    public class LevelDefinition : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private string _sceneName;
        [SerializeField] private Sprite _preview;
        [SerializeField, TextArea(2, 5)] private string _introComment;

        public string Id => _id;
        public string DisplayName => _displayName;
        public string SceneName => _sceneName;
        public Sprite Preview => _preview;
        public string IntroComment => _introComment;
        public bool HasIntroComment => !string.IsNullOrWhiteSpace(_introComment);

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(_id))
                throw new InvalidOperationException($"{name} has no level id.");

            if (string.IsNullOrWhiteSpace(_displayName))
                throw new InvalidOperationException($"{name} has no display name.");

            if (string.IsNullOrWhiteSpace(_sceneName))
                throw new InvalidOperationException($"{name} has no scene name.");
        }
    }
}