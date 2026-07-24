using _PhotoCountdown.Gameplay.Characters;
using _PhotoCountdown.Gameplay.Characters.Actions;
using _PhotoCountdown.Gameplay.Slots;

namespace _PhotoCountdown.Gameplay.Photography
{
    public class CharacterPhotoState
    {
        public LevelCharacter Character { get; }
        public CharacterSlot Slot { get; }
        public CharacterActionDefinition Action { get; }

        public CharacterPhotoState(LevelCharacter character, CharacterSlot slot, CharacterActionDefinition action)
        {
            Character = character;
            Slot = slot;
            Action = action;
        }
    }
}