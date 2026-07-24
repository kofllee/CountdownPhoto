using System;
using System.Collections.Generic;
using _PhotoCountdown.Gameplay.Objectives;

namespace _PhotoCountdown.Gameplay.Photography
{
    public class PhotoSnapshot
    {
        public double LevelTime { get; }
        public IReadOnlyList<PhotoObjectiveResult> Objectives { get; }

        public PhotoSnapshot(double levelTime, PhotoObjectiveResult[] objectives)
        {
            if (objectives == null)
                throw new ArgumentNullException(nameof(objectives));

            LevelTime = levelTime;
            Objectives = Array.AsReadOnly((PhotoObjectiveResult[])objectives.Clone());
        }
    }
}