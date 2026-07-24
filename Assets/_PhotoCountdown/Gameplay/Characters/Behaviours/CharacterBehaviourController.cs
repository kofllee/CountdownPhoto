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

        public CharacterBehaviour CurrentBehaviour => _isInitialized ? _character.CurrentBehaviour : null;

        public double BehaviourElapsedTime
        {
            get
            {
                if (!_isInitialized || !_character.CurrentBehaviour)
                    return 0d;

                return _clock.Time - _character.BehaviourStartedAt;
            }
        }

        public CharacterBehaviourPhase CurrentPhase
        {
            get
            {
                if (!_isInitialized)
                    return null;

                CharacterBehaviour behaviour = _character.CurrentBehaviour;

                if (!behaviour)
                    return null;

                return behaviour.GetPhaseAt(BehaviourElapsedTime);
            }
        }
        
        public double CurrentPhaseRemainingTime
        {
            get
            {
                CharacterBehaviour behaviour = CurrentBehaviour;

                if (behaviour == null)
                    return 0d;

                return behaviour.GetPhaseRemainingTime(BehaviourElapsedTime);
            }
        }

        public float CurrentPhaseDuration
        {
            get
            {
                CharacterBehaviourPhase phase = CurrentPhase;
                return phase == null ? 0f : phase.Duration;
            }
        }

        public CharacterActionDefinition CurrentAction => CurrentPhase?.Action;

        public CharacterCountdownInfo Countdown
        {
            get
            {
                if (!_isInitialized)
                    return CharacterCountdownInfo.Hidden;

                CharacterBehaviour behaviour = _character.CurrentBehaviour;

                if (!behaviour)
                    return CharacterCountdownInfo.Hidden;

                return behaviour.GetCountdownAt(BehaviourElapsedTime);
            }
        }
        
        public bool IsInitialized => _isInitialized;

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