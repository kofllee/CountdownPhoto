using _PhotoCountdown.Gameplay.Characters.Actions;
using _PhotoCountdown.Gameplay.Levels;
using _PhotoCountdown.Gameplay.Neighbors;
using UnityEngine;

namespace _PhotoCountdown.Gameplay.Characters.Behaviours
{
    public class CharacterBehaviourController : MonoBehaviour
    {
        private LevelCharacter _character;
        private LevelClock _clock;
        private NeighborResolver _neighbors;
        private CharacterBehaviour[] _behaviours;
        private CharacterBehaviourResolver _resolver;
        private bool _isInitialized;

        public CharacterBehaviour CurrentBehaviour => _character.CurrentBehaviour;

        public double BehaviourElapsedTime
        {
            get
            {
                if (_character.CurrentBehaviour == null)
                    return 0d;

                return _clock.Time - _character.BehaviourStartedAt;
            }
        }

        public CharacterBehaviourPhase CurrentPhase
        {
            get
            {
                CharacterBehaviour behaviour = _character.CurrentBehaviour;

                if (behaviour == null)
                    return null;

                return behaviour.GetPhaseAt(BehaviourElapsedTime);
            }
        }

        public CharacterActionDefinition CurrentAction => CurrentPhase?.Action;

        public double CurrentPhaseRemainingTime
        {
            get
            {
                CharacterBehaviour behaviour = _character.CurrentBehaviour;

                if (behaviour == null)
                    return 0d;

                return behaviour.GetPhaseRemainingTime(BehaviourElapsedTime);
            }
        }

        public void Init(LevelCharacter character, LevelClock clock, NeighborResolver neighbors)
        {
            _character = character;
            _clock = clock;
            _neighbors = neighbors;
            _resolver = new CharacterBehaviourResolver();

            _behaviours = GetComponentsInChildren<CharacterBehaviour>(true);

            if (_behaviours.Length == 0)
                throw new MissingReferenceException($"{name} has no behaviours.");

            foreach (CharacterBehaviour behaviour in _behaviours)
                behaviour.Init();

            _isInitialized = true;
            ResolveBehaviour();
        }

        private void Update()
        {
            if (!_isInitialized)
                return;

            ResolveBehaviour();
        }
        
        private void ResolveBehaviour()
        {
            CharacterBehaviourContext context = new CharacterBehaviourContext(_character, _neighbors);

            CharacterBehaviour behaviour = _resolver.Resolve(_behaviours, context);

            if (!behaviour)
            {
                throw new MissingReferenceException($"{name} has no matching character behaviour.");
            }

            _character.SetBehaviour(behaviour, _clock.Time);
        }

    }
}