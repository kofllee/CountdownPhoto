using _PhotoCountdown.Gameplay.Slots;
using UnityEngine;

namespace _PhotoCountdown.Gameplay.Characters.Behaviours.Conditions
{
    public class SlotCondition : CharacterBehaviourCondition
    {
        [SerializeField] private CharacterSlot _requiredSlot;

        protected override bool Check(CharacterBehaviourContext context)
        {
            return context.Character.CurrentSlot == _requiredSlot;
        }
    }
}