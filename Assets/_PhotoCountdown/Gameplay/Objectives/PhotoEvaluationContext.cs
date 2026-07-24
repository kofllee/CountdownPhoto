using System;
using System.Collections.Generic;
using _PhotoCountdown.Gameplay.Characters;
using _PhotoCountdown.Gameplay.Photography;

namespace _PhotoCountdown.Gameplay.Objectives
{
    public readonly struct PhotoEvaluationContext
    {
        private readonly IReadOnlyList<CharacterPhotoState> _characters;

        public double Time { get; }

        public PhotoEvaluationContext(double time, IReadOnlyList<CharacterPhotoState> characters)
        {
            Time = time;
            _characters = characters ?? throw new ArgumentNullException(nameof(characters));
        }

        public CharacterPhotoState GetCharacter(LevelCharacter character)
        {
            if (!character)
                throw new ArgumentNullException(nameof(character));

            foreach (CharacterPhotoState state in _characters)
            {
                if (state.Character == character)
                    return state;
            }

            throw new InvalidOperationException($"{character.name} is not included in the photo.");
        }

    }
}