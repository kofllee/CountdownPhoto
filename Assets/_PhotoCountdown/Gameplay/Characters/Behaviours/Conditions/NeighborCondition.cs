using UnityEngine;

namespace _PhotoCountdown.Gameplay.Characters.Behaviours.Conditions
{
    public class NeighborCondition : CharacterBehaviourCondition
    {
        [SerializeField] private LevelCharacter _requiredNeighbor;

        protected override bool Check(CharacterBehaviourContext context)
        {
            return context.Neighbors.AreNeighbors(context.Character, _requiredNeighbor);
        }
    }
}