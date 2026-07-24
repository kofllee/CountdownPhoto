using UnityEngine;

namespace _PhotoCountdown.Gameplay.Objectives
{
    public abstract class PhotoObjective : MonoBehaviour
    {
        [SerializeField] private string _description;
        [SerializeField, TextArea(2, 4)] private string _failureMessage;
        
        public string Description => _description;
        public string FailureMessage => string.IsNullOrWhiteSpace(_failureMessage) ? _description : _failureMessage;

        public virtual void Validate()
        {
            if (string.IsNullOrWhiteSpace(_description))
                throw new MissingReferenceException($"{name} has no objective description.");
        }

        public abstract bool IsCompleted(PhotoEvaluationContext context);
    }
}