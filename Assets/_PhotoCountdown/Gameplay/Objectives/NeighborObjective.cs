using _PhotoCountdown.Gameplay.Characters;
using _PhotoCountdown.Gameplay.Photography;
using UnityEngine;

namespace _PhotoCountdown.Gameplay.Objectives
{
    public class NeighborObjective : PhotoObjective
    {
        [SerializeField] private LevelCharacter _first;
        [SerializeField] private LevelCharacter _second;
        [SerializeField] private bool _mustBeNeighbors = true;

        public override void Validate()
        {
            base.Validate();

            if (_first == null)
                throw new MissingReferenceException($"{name} has no first character.");

            if (_second == null)
                throw new MissingReferenceException($"{name} has no second character.");

            if (_first == _second)
                throw new MissingReferenceException($"{name} references the same character twice.");
        }

        public override bool IsCompleted(PhotoEvaluationContext context)
        {
            CharacterPhotoState first = context.GetCharacter(_first);
            CharacterPhotoState second = context.GetCharacter(_second);

            bool neighbors = first.Slot != null && second.Slot != null && first.Slot.IsNeighbor(second.Slot);

            return neighbors == _mustBeNeighbors;
        }
    }
}