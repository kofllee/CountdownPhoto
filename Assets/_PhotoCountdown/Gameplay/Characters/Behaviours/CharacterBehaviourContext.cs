using _PhotoCountdown.Gameplay.Neighbors;

namespace _PhotoCountdown.Gameplay.Characters.Behaviours
{
    public readonly struct CharacterBehaviourContext
    {
        public LevelCharacter Character { get; }
        public NeighborResolver Neighbors { get; }

        public CharacterBehaviourContext(LevelCharacter character, NeighborResolver neighbors)
        {
            Character = character;
            Neighbors = neighbors;
        }
    }
}