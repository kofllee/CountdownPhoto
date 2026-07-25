using System.Collections.Generic;
using _PhotoCountdown.Gameplay.Photography;
using UnityEngine;

namespace _PhotoCountdown.Gameplay.Objectives
{
    public abstract class PhotoObjective : MonoBehaviour
    {
        [SerializeField] private string _description;
        [SerializeField] private PhotoHighlightTarget[] _issueTargets;

        public string Description => _description;
        public IReadOnlyList<PhotoHighlightTarget> IssueTargets => _issueTargets;

        public virtual void Validate()
        {
            if (string.IsNullOrWhiteSpace(_description))
                throw new MissingReferenceException($"{name} has no objective description.");

            if (_issueTargets == null || _issueTargets.Length == 0)
                throw new MissingReferenceException($"{name} has no issue targets.");

            HashSet<PhotoHighlightTarget> unique = new HashSet<PhotoHighlightTarget>();

            foreach (PhotoHighlightTarget target in _issueTargets)
            {
                if (target == null)
                    throw new MissingReferenceException($"{name} has a missing issue target.");

                if (!unique.Add(target))
                    throw new MissingReferenceException($"{name} contains a duplicate issue target.");

                target.Validate();
            }
        }

        public abstract bool IsCompleted(PhotoEvaluationContext context);
    }
}