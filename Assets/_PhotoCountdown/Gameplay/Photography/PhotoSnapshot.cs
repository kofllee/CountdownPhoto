using System;
using System.Collections.Generic;
using _PhotoCountdown.Gameplay.Objectives;

namespace _PhotoCountdown.Gameplay.Photography
{
    public sealed class PhotoSnapshot
    {
        public double LevelTime { get; }
        public IReadOnlyList<PhotoObjectiveResult> Objectives { get; }
        public IReadOnlyList<PhotoIssueRegion> IssueRegions { get; }

        public PhotoSnapshot(double levelTime, PhotoObjectiveResult[] objectives, PhotoIssueRegion[] issueRegions)
        {
            if (objectives == null)
                throw new ArgumentNullException(nameof(objectives));

            if (issueRegions == null)
                throw new ArgumentNullException(nameof(issueRegions));

            LevelTime = levelTime;
            Objectives = Array.AsReadOnly((PhotoObjectiveResult[])objectives.Clone());
            IssueRegions = Array.AsReadOnly((PhotoIssueRegion[])issueRegions.Clone());
        }
    }
}