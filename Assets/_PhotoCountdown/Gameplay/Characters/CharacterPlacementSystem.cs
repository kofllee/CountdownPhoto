using System.Collections.Generic;
using _PhotoCountdown.Gameplay.Slots;
using UnityEngine;

namespace _PhotoCountdown.Gameplay.Characters
{
    public class CharacterPlacementSystem
    {
        private readonly IReadOnlyList<LevelCharacter> characters;

        public CharacterPlacementSystem(IReadOnlyList<LevelCharacter> characters)
        {
            this.characters = characters;
        }

        public void Place(LevelCharacter character, CharacterSlot targetSlot)
        {
            if (character.CurrentSlot == targetSlot)
                return;

            LevelCharacter targetCharacter = FindAtSlot(targetSlot);

            if (!targetCharacter)
            {
                character.SetSlot(targetSlot);
                return;
            }

            CharacterSlot previousSlot = character.CurrentSlot;
            character.SetSlot(targetSlot);
            targetCharacter.SetSlot(previousSlot);
        }
        
        private LevelCharacter FindAtSlot(CharacterSlot slot)
        {
            foreach (LevelCharacter character in characters)
            {
                if (character.CurrentSlot == slot)
                    return character;
            }

            return null;
        }
    }
}