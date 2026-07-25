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
        
        public CharacterCountdownInfo GetCountdownAt(double elapsedTime)
        {
            double cycleDuration = GetCycleDuration();

            if (cycleDuration <= 0d || _phases.Count == 0)
                return CharacterCountdownInfo.Hidden;

            double cycleTime = elapsedTime % cycleDuration;
            int currentIndex = GetPhaseIndexAt(cycleTime, out double timeIntoPhase);
            CharacterBehaviourPhase currentPhase = _phases[currentIndex];

            if (currentPhase.CountdownDisplay == CharacterCountdownDisplay.Hidden)
                return CharacterCountdownInfo.Hidden;

            double remainingTime = currentPhase.Duration - timeIntoPhase;
            CharacterActionDefinition action = currentPhase.Action;

            if (!action && currentPhase.CountdownDisplay == CharacterCountdownDisplay.ActionIcon)
                action = FindNextAction(currentIndex);

            if (!action && currentPhase.CountdownDisplay == CharacterCountdownDisplay.ActionIcon)
                return CharacterCountdownInfo.Hidden;

            return new CharacterCountdownInfo(
                action,
                currentPhase.CountdownDisplay,
                remainingTime,
                currentPhase.Duration);
        }

        private CharacterActionDefinition FindNextAction(int currentIndex)
        {
            for (int offset = 1; offset < _phases.Count; offset++)
            {
                int index = (currentIndex + offset) % _phases.Count;
                CharacterActionDefinition action = _phases[index].Action;

                if (action)
                    return action;
            }

            return null;
        }
        
        private int GetPhaseIndexAt(double cycleTime, out double timeIntoPhase)
        {
            double phaseStart = 0d;

            for (int i = 0; i < _phases.Count; i++)
            {
                double phaseEnd = phaseStart + _phases[i].Duration;

                if (cycleTime < phaseEnd)
                {
                    timeIntoPhase = cycleTime - phaseStart;
                    return i;
                }

                phaseStart = phaseEnd;
            }

            timeIntoPhase = 0d;
            return _phases.Count - 1;
        }
    }
}