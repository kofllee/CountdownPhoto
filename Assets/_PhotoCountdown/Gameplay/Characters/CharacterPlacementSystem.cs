using System.Collections.Generic;
using _PhotoCountdown.Gameplay.Slots;
using UnityEngine;

namespace _PhotoCountdown.Gameplay.Characters
{
    public sealed class CharacterPlacementSystem
    {
        private readonly IReadOnlyList<LevelCharacter> _characters;

        public CharacterPlacementSystem(IReadOnlyList<LevelCharacter> characters)
        {
            _characters = characters;
        }

        public bool CanPlace(LevelCharacter character, CharacterSlot targetSlot)
        {
            if (!character || !targetSlot || !character.CanBeDragged)
                return false;

            if (character.CurrentSlot == targetSlot)
                return false;

            return FindAtSlot(targetSlot) == null;
        }

        public bool CanSwap(LevelCharacter first, LevelCharacter second)
        {
            if (!first || !second || first == second)
                return false;

            return first.CanBeDragged && second.CanBeDragged;
        }

        public void Place(LevelCharacter character, CharacterSlot targetSlot)
        {
            if (!CanPlace(character, targetSlot))
                return;

            character.SetSlot(targetSlot);
        }

        public void Swap(LevelCharacter first, LevelCharacter second)
        {
            if (!CanSwap(first, second))
                return;

            CharacterSlot firstSlot = first.CurrentSlot;
            first.SetSlot(second.CurrentSlot);
            second.SetSlot(firstSlot);
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
