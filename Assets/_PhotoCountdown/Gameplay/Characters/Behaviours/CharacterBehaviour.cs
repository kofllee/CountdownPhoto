using System.Collections.Generic;
using _PhotoCountdown.Gameplay.Characters.Actions;
using _PhotoCountdown.Gameplay.Characters.Behaviours.Conditions;
using UnityEngine;

namespace _PhotoCountdown.Gameplay.Characters.Behaviours
{
    public class CharacterBehaviour : MonoBehaviour
    {
        [SerializeField] private int _priority;
        [SerializeField] private List<CharacterBehaviourPhase> _phases = new();
        
        private CharacterBehaviourCondition[] _conditions;

        public int Priority => _priority;
        public IReadOnlyList<CharacterBehaviourPhase> Phases => _phases;

        public void Init()
        {
            _conditions = GetComponents<CharacterBehaviourCondition>();

            if (_phases.Count == 0)
                throw new MissingReferenceException($"{name} has no phases.");

            foreach (CharacterBehaviourPhase phase in _phases)
            {
                if (phase.Duration <= 0f)
                    throw new MissingReferenceException($"{name} contains a phase with invalid duration.");
            }
        }
        
        public bool Matches(CharacterBehaviourContext context)
        {
            foreach (CharacterBehaviourCondition condition in _conditions)
            {
                if (!condition.IsSatisfied(context))
                    return false;
            }

            return true;
        }
        
        public CharacterBehaviourPhase GetPhaseAt(double elapsedTime)
        {
            double cycleDuration = GetCycleDuration();

            if (cycleDuration <= 0d)
                return null;

            double cycleTime = elapsedTime % cycleDuration;
            double phaseEnd = 0d;

            foreach (CharacterBehaviourPhase phase in _phases)
            {
                phaseEnd += phase.Duration;

                if (cycleTime < phaseEnd)
                    return phase;
            }

            return _phases[^1];
        }
        
        public CharacterActionDefinition GetActionAt(double elapsedTime)
        {
            return GetPhaseAt(elapsedTime)?.Action;
        }

        public double GetPhaseRemainingTime(double elapsedTime)
        {
            double cycleDuration = GetCycleDuration();

            if (cycleDuration <= 0d)
                return 0d;

            double cycleTime = elapsedTime % cycleDuration;
            double phaseEnd = 0d;

            foreach (CharacterBehaviourPhase phase in _phases)
            {
                phaseEnd += phase.Duration;

                if (cycleTime < phaseEnd)
                    return phaseEnd - cycleTime;
            }

            return 0d;
        }

        public double GetCycleDuration()
        {
            double duration = 0d;

            foreach (CharacterBehaviourPhase phase in _phases)
                duration += phase.Duration;

            return duration;
        }
    }
}