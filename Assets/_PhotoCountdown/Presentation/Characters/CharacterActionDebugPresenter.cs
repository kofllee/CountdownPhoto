using _PhotoCountdown.Gameplay.Characters.Actions;
using _PhotoCountdown.Gameplay.Characters.Behaviours;
using UnityEngine;

namespace _PhotoCountdown.Presentation.Characters
{
    public sealed class CharacterActionDebugPresenter : MonoBehaviour
    {
        [SerializeField] private CharacterBehaviourController _controller;
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private Color _idleColor = Color.white;

        private CharacterActionDefinition _shownAction;

        private void Update()
        {
            CharacterActionDefinition action = _controller.CurrentAction;

            if (_shownAction == action)
                return;

            _shownAction = action;
            _renderer.color = !action ? _idleColor : action.DebugColor;
        }
    }
}