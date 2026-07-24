using _PhotoCountdown.Gameplay.Characters;
using _PhotoCountdown.Gameplay.Characters.Actions;
using _PhotoCountdown.Gameplay.Photography;
using UnityEngine;

namespace _PhotoCountdown.Gameplay.Objectives
{
    public class CharacterActionObjective : PhotoObjective
    {
        [SerializeField] private LevelCharacter _character;
        [SerializeField] private CharacterActionDefinition _requiredAction;
        [SerializeField] private bool _mustBeActive = true;

        public override void Validate()
        {
            base.Validate();

            if (_character == null)
                throw new MissingReferenceException($"{name} has no character.");

            if (_requiredAction == null)
                throw new MissingReferenceException($"{name} has no required action.");
        }

        public override bool IsCompleted(PhotoEvaluationContext context)
        {
            CharacterPhotoState state = context.GetCharacter(_character);
            bool active = state.Action == _requiredAction;
            return active == _mustBeActive;
        }
    }
}