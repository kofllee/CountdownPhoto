using UnityEngine;

namespace _PhotoCountdown.Gameplay.Characters
{
    public class LevelCharacter: MonoBehaviour
    {
        [SerializeField] private CharacterSlot _initialSlot;
        
        public CharacterSlot InitialSlot => _initialSlot;
        public CharacterSlot CurrentSlot { get; private set; }

        public void Init()
        {
            CurrentSlot = _initialSlot;
        }

        public void SetSlot(CharacterSlot slot)
        {
            if (slot == null)
                throw new MissingReferenceException($"{name} received a null slot.");

            CurrentSlot = slot;
        }
    }
}