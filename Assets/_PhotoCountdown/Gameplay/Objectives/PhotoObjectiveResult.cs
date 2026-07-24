namespace _PhotoCountdown.Gameplay.Objectives
{
    public sealed class PhotoObjectiveResult
    {
        public string Description { get; }
        public string FailureMessage { get; }
        public bool Completed { get; }

        public PhotoObjectiveResult(string description, string failureMessage, bool completed)
        {
            Description = description;
            FailureMessage = failureMessage;
            Completed = completed;
        }
    }
}