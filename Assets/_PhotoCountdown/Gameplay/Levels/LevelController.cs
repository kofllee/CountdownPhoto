using System.Collections.Generic;
using _PhotoCountdown.Gameplay.Characters;
using _PhotoCountdown.Input;
using UnityEngine;

namespace _PhotoCountdown.Gameplay.Levels
{
    public class LevelController : MonoBehaviour
    {
        [SerializeField] private Transform _levelContentRoot;
        [SerializeField] private CharacterDragInput _dragInput;
                
        private void Awake()
        {
            LevelCharacter[] characters = _levelContentRoot.GetComponentsInChildren<LevelCharacter>(true);

            ValidateCharacters(characters);

            foreach (LevelCharacter character in characters)
                character.Init();

            foreach (LevelCharacter character in characters)
                InitCharacterPresentation(character);

            CharacterPlacementSystem placement = new CharacterPlacementSystem(characters);
            _dragInput.Init(placement);
        }
        
        private static void InitCharacterPresentation(LevelCharacter character)
        {
            CharacterMover mover = character.GetComponent<CharacterMover>();

            if (mover == null)
                throw new MissingComponentException($"{character.name} needs CharacterMover.");

            mover.Init(character);
        }
        
        private static void ValidateCharacters(LevelCharacter[] characters)
        {
            if (characters.Length == 0)
                throw new MissingReferenceException("The level has no characters.");

            HashSet<CharacterSlot> occupiedSlots = new HashSet<CharacterSlot>();

            foreach (LevelCharacter character in characters)
            {
                if (!character.InitialSlot)
                    throw new MissingReferenceException($"{character.name} has no initial slot.");

                if (!occupiedSlots.Add(character.InitialSlot))
                    throw new MissingReferenceException($"Several characters use {character.InitialSlot.name}.");
            }
        }

    }
}