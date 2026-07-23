using System.Collections.Generic;
using _PhotoCountdown.Gameplay.Characters;
using _PhotoCountdown.Gameplay.Slots;

namespace _PhotoCountdown.Gameplay.Neighbors
{
    public class NeighborResolver
    {
        private readonly IReadOnlyList<LevelCharacter> _characters;

        public NeighborResolver(IReadOnlyList<LevelCharacter> characters)
        {
            _characters = characters;
        }

        public bool AreNeighbors(LevelCharacter first, LevelCharacter second)
        {
            if (first == null || second == null)
                return false;

            CharacterSlot firstSlot = first.CurrentSlot;
            CharacterSlot secondSlot = second.CurrentSlot;

            if (firstSlot == null || secondSlot == null)
                return false;

            return firstSlot.IsNeighbor(secondSlot);
        }

        public LevelCharacter FindAtSlot(CharacterSlot slot)
        {
            foreach (LevelCharacter character in _characters)
            {
                if (character.CurrentSlot == slot)
                    return character;
            }

            return null;
        }
    }
}