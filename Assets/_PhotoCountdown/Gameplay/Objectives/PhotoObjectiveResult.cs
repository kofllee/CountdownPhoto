namespace _PhotoCountdown.Gameplay.Objectives
{
    public sealed class PhotoObjectiveResult
    {
        public string Description { get; }
        public bool Completed { get; }

        public PhotoObjectiveResult(string description, bool completed)
        {
            Description = description;
            Completed = completed;
        }
    }
}