using _PhotoCountdown.Gameplay.Characters;
using _PhotoCountdown.Gameplay.Photography;
using _PhotoCountdown.Gameplay.Slots;
using UnityEngine;

namespace _PhotoCountdown.Gameplay.Objectives
{
    public sealed class CharacterSlotObjective : PhotoObjective
    {
        [SerializeField] private LevelCharacter _character;
        [SerializeField] private CharacterSlot _requiredSlot;

        public override void Validate()
        {
            base.Validate();

            if (_character == null)
                throw new MissingReferenceException($"{name} has no character.");

            if (_requiredSlot == null)
                throw new MissingReferenceException($"{name} has no required slot.");
        }

        public override bool IsCompleted(PhotoEvaluationContext context)
        {
            return context.GetCharacter(_character).Slot == _requiredSlot;
        }
    }
}