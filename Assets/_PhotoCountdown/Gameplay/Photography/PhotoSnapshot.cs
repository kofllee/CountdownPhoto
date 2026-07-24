using System;
using System.Collections.Generic;
using _PhotoCountdown.Gameplay.Objectives;

namespace _PhotoCountdown.Gameplay.Photography
{
    public sealed class PhotoSnapshot
    {
        public double Time { get; }
        public IReadOnlyList<CharacterPhotoState> Characters { get; }
        public IReadOnlyList<PhotoObjectiveResult> Objectives { get; }

        public bool IsSuccessful
        {
            get
            {
                foreach (PhotoObjectiveResult result in Objectives)
                {
                    if (!result.Completed)
                        return false;
                }

                return true;
            }
        }

        public PhotoSnapshot(double time, CharacterPhotoState[] characters,
            PhotoObjectiveResult[] objectives)
        {
            if (characters == null)
                throw new ArgumentNullException(nameof(characters));

            if (objectives == null)
                throw new ArgumentNullException(nameof(objectives));

            Time = time;
            Characters = Array.AsReadOnly(characters);
            Objectives = Array.AsReadOnly(objectives);
        }
    }
}