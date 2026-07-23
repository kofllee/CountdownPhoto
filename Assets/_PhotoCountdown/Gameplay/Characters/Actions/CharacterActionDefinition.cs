using UnityEngine;

namespace _PhotoCountdown.Gameplay.Characters.Actions
{
    [CreateAssetMenu(fileName = "Action_", menuName = "Photo Countdown/Character Action")]
    public class CharacterActionDefinition : ScriptableObject
    {
        [SerializeField] private string _displayName;
        [SerializeField] private CharacterActionType _type;
        [SerializeField] private AnimationClip _animation;
        [SerializeField] private Color _debugColor = Color.white;
        
        public string DisplayName => _displayName;
        public CharacterActionType Type => _type;
        public AnimationClip Animation => _animation;
        public Color DebugColor => _debugColor;

    }
}