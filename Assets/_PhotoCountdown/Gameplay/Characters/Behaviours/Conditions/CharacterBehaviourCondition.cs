using UnityEngine;

namespace _PhotoCountdown.Gameplay.Characters.Behaviours.Conditions
{
    public abstract class CharacterBehaviourCondition : MonoBehaviour
    {
        [SerializeField] private bool _invert;

        public bool IsSatisfied(CharacterBehaviourContext context)
        {
            bool result = Check(context);
            return _invert ? !result : result;
        }

        protected abstract bool Check(CharacterBehaviourContext context);
    }
}