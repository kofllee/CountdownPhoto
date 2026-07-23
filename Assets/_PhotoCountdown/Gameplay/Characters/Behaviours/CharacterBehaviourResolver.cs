using System.Collections.Generic;

namespace _PhotoCountdown.Gameplay.Characters.Behaviours
{
    public class CharacterBehaviourResolver
    {
        public CharacterBehaviour Resolve(IReadOnlyList<CharacterBehaviour> behaviours, CharacterBehaviourContext context)
        {
            CharacterBehaviour bestBehaviour = null;

            foreach (CharacterBehaviour behaviour in behaviours)
            {
                if (!behaviour.Matches(context))
                    continue;

                if (bestBehaviour == null || behaviour.Priority > bestBehaviour.Priority)
                {
                    bestBehaviour = behaviour;
                }
            }

            return bestBehaviour;
        }
    }
}