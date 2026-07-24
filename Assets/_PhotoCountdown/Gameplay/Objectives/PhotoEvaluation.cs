using System;
using System.Collections.Generic;

namespace _PhotoCountdown.Gameplay.Objectives
{
    public class PhotoEvaluation
    {
        private readonly Dictionary<PhotoObjective, bool> _results = new();
        private readonly PhotoObjective[] _objectives;

        public PhotoEvaluation(PhotoObjective[] objectives, PhotoEvaluationContext context)
        {
            if (objectives == null)
                throw new ArgumentNullException(nameof(objectives));

            _objectives = (PhotoObjective[])objectives.Clone();

            foreach (PhotoObjective objective in _objectives)
            {
                if (objective == null)
                    throw new ArgumentException("Photo evaluation contains a missing objective.");

                _results.Add(objective, objective.IsCompleted(context));
            }
        }

        public bool IsCompleted(PhotoObjective objective)
        {
            if (objective == null)
                throw new ArgumentNullException(nameof(objective));

            if (!_results.TryGetValue(objective, out bool completed))
            {
                throw new InvalidOperationException($"{objective.name} was not included in the photo evaluation.");
            }

            return completed;
        }

        public PhotoObjectiveResult[] CreateResults()
        {
            PhotoObjectiveResult[] results = new PhotoObjectiveResult[_objectives.Length];

            for (int i = 0; i < _objectives.Length; i++)
            {
                PhotoObjective objective = _objectives[i];
                results[i] = new PhotoObjectiveResult(objective.Description, IsCompleted(objective));
            }

            return results;
        }
    }
}