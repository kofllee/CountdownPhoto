using System;
using _PhotoCountdown.Gameplay.Characters.Actions;
using UnityEngine;

namespace _PhotoCountdown.Gameplay.Characters.Behaviours
{
    [Serializable]
    public class CharacterBehaviourPhase
    {
        [SerializeField] private CharacterActionDefinition _action;
        [SerializeField] private CharacterCountdownDisplay _countdownDisplay;
        [SerializeField, Min(0.01f)] private float _duration = 1f;

        public CharacterActionDefinition Action => _action;
        public CharacterCountdownDisplay CountdownDisplay => _countdownDisplay;
        public float Duration => _duration;
    }
}