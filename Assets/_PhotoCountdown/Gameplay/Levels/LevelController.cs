using System.Collections.Generic;
using _PhotoCountdown.Gameplay.Characters;
using _PhotoCountdown.Gameplay.Characters.Behaviours;
using _PhotoCountdown.Gameplay.Neighbors;
using _PhotoCountdown.Gameplay.Objectives;
using _PhotoCountdown.Gameplay.Photography;
using _PhotoCountdown.Gameplay.Slots;
using _PhotoCountdown.Input;
using UnityEngine;

namespace _PhotoCountdown.Gameplay.Levels
{
    public class LevelController : MonoBehaviour
    {
        [SerializeField] private Transform _levelContentRoot;
        [SerializeField] private CharacterDragInput _dragInput;
        [SerializeField] private LevelClock _clock;
        [SerializeField] private PhotoCaptureController _photoCapture;

        private void Awake()
        {
            ValidateReferences();

            LevelCharacter[] characters = _levelContentRoot.GetComponentsInChildren<LevelCharacter>(true);
            PhotoObjective[] objectives = _levelContentRoot.GetComponentsInChildren<PhotoObjective>(true);

            ValidateCharacters(characters);

            foreach (LevelCharacter character in characters)
                character.Init();

            foreach (LevelCharacter character in characters)
                InitCharacterPresentation(character);

            CharacterPlacementSystem placement = new CharacterPlacementSystem(characters);
            NeighborResolver neighbors = new NeighborResolver(characters);

            _dragInput.Init(placement);
            _clock.StartClock();

            foreach (LevelCharacter character in characters)
                InitCharacterBehaviour(character, neighbors);

            _photoCapture.Init(_clock, characters, objectives);
        }

        private void InitCharacterBehaviour(LevelCharacter character, NeighborResolver neighbors)
        {
            CharacterBehaviourController controller = character.GetComponent<CharacterBehaviourController>();

            if (!controller)
                throw new MissingComponentException($"{character.name} needs CharacterBehaviourController.");

            controller.Init(character, _clock, neighbors);
        }

        private static void InitCharacterPresentation(LevelCharacter character)
        {
            CharacterMover mover = character.GetComponent<CharacterMover>();

            if (!mover)
                throw new MissingComponentException($"{character.name} needs CharacterMover.");

            mover.Init(character);
        }

        private void ValidateReferences()
        {
            if (!_levelContentRoot)
                throw new MissingReferenceException($"{name} has no level content root.");

            if (!_dragInput)
                throw new MissingReferenceException($"{name} has no drag input.");

            if (!_clock)
                throw new MissingReferenceException($"{name} has no level clock.");

            if (!_photoCapture)
                throw new MissingReferenceException($"{name} has no photo capture controller.");
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