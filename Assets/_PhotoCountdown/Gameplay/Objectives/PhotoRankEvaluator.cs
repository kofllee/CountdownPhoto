using System;
using System.Collections.Generic;
using _PhotoCountdown.Gameplay.Levels;
using UnityEngine;

namespace _PhotoCountdown.Gameplay.Objectives
{
    public sealed class PhotoRankEvaluator : MonoBehaviour
    {
        [SerializeField] private PhotoObjective[] _oneStarRequirements;
        [SerializeField] private PhotoObjective[] _twoStarRequirements;

        public LevelRank Evaluate(PhotoEvaluation evaluation)
        {
            if (evaluation == null)
                throw new ArgumentNullException(nameof(evaluation));

            bool completedOneStar = Matches(_oneStarRequirements, evaluation);

            if (completedOneStar && Matches(_twoStarRequirements, evaluation))
                return LevelRank.TwoStars;

            if (completedOneStar)
                return LevelRank.OneStar;

            return LevelRank.Failed;
        }

        public string[] GetVisibleFailureDescriptions(
            PhotoEvaluation evaluation,
            LevelRank rank)
        {
            if (evaluation == null)
                throw new ArgumentNullException(nameof(evaluation));

            if (rank != LevelRank.Failed)
                return Array.Empty<string>();

            List<string> failures = new List<string>();

            foreach (PhotoObjective objective in _oneStarRequirements)
            {
                if (!evaluation.IsCompleted(objective))
                    failures.Add(objective.Description);
            }

            return failures.ToArray();
        }

        public void Validate(IReadOnlyList<PhotoObjective> availableObjectives)
        {
            if (availableObjectives == null)
                throw new ArgumentNullException(nameof(availableObjectives));

            HashSet<PhotoObjective> available = new HashSet<PhotoObjective>(
                availableObjectives);

            ValidateRequirements(_oneStarRequirements, "one-star", available);
            ValidateRequirements(_twoStarRequirements, "two-star", available);
        }

        private static bool Matches(
            PhotoObjective[] requirements,
            PhotoEvaluation evaluation)
        {
            foreach (PhotoObjective objective in requirements)
            {
                if (!evaluation.IsCompleted(objective))
                    return false;
            }

            return true;
        }

        private void ValidateRequirements(
            PhotoObjective[] requirements,
            string rankName,
            HashSet<PhotoObjective> available)
        {
            if (requirements == null || requirements.Length == 0)
                throw new MissingReferenceException($"{name} has no {rankName} requirements.");

            HashSet<PhotoObjective> unique = new HashSet<PhotoObjective>();

            foreach (PhotoObjective objective in requirements)
            {
                if (!objective)
                {
                    throw new MissingReferenceException(
                        $"{name} contains a missing {rankName} requirement.");
                }

                if (!available.Contains(objective))
                {
                    throw new MissingReferenceException(
                        $"{objective.name} is outside the level objective collection.");
                }

                if (!unique.Add(objective))
                {
                    throw new MissingReferenceException(
                        $"{name} contains duplicate requirement {objective.name}.");
                }
            }
        }
    }
}