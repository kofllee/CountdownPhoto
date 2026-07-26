using _PhotoCountdown.Gameplay.Interactions;
using UnityEngine;

namespace _PhotoCountdown.Gameplay.Objectives
{
    public sealed class InteractionAbsentObjective : PhotoObjective
    {
        [SerializeField] private TimedDisappearInteraction _interaction;
        [SerializeField] private bool _mustBeAbsent = true;

        public override void Validate()
        {
            base.Validate();

            if (!_interaction)
                throw new MissingReferenceException($"{name} has no interaction.");
        }

        public override bool IsCompleted(PhotoEvaluationContext context)
        {
            bool absent = _interaction.IsAbsentAt(context.Time);
            return absent == _mustBeAbsent;
        }
    }
}