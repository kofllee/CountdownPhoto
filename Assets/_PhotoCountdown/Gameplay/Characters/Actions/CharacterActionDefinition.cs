using UnityEngine;

namespace _PhotoCountdown.Gameplay.Characters.Actions
{
    [CreateAssetMenu(fileName = "Action_", menuName = "Photo Countdown/Character Action")]
    public class CharacterActionDefinition : ScriptableObject
    {
        [SerializeField] private string _displayName;
        [SerializeField] private AnimationClip _animation;
        [SerializeField] private Sprite _icon;
        [SerializeField] private Color _debugColor = Color.white;
        
        public string DisplayName => _displayName;
        public AnimationClip Animation => _animation;
        public Sprite Icon => _icon;
        public Color DebugColor => _debugColor;

    }
}