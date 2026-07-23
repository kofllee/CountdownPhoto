using _PhotoCountdown.Gameplay.Characters.Behaviours;
using _PhotoCountdown.Gameplay.Slots;
using UnityEngine;

namespace _PhotoCountdown.Gameplay.Characters
{
    public class LevelCharacter: MonoBehaviour
    {
        [SerializeField] private CharacterSlot _initialSlot;
        
        public CharacterSlot InitialSlot => _initialSlot;
        public CharacterSlot CurrentSlot { get; private set; }
        public CharacterBehaviour CurrentBehaviour { get; private set; }
        public double BehaviourStartedAt { get; private set; }

        public void Init()
        {
            CurrentSlot = _initialSlot;
        }

        public void SetSlot(CharacterSlot slot)
        {
            if (!slot)
                throw new MissingReferenceException($"{name} received a null slot.");

            CurrentSlot = slot;
        }
        
        public void SetBehaviour(CharacterBehaviour behaviour, double time)
        {
            if (CurrentBehaviour == behaviour)
                return;

            CurrentBehaviour = behaviour;
            BehaviourStartedAt = time;
        }
    }
}