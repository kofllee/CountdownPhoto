using _PhotoCountdown.Gameplay.Characters.Actions;

namespace _PhotoCountdown.Gameplay.Characters.Behaviours
{
    public readonly struct CharacterCountdownInfo
    {
        public CharacterActionDefinition Action { get; }
        public CharacterCountdownDisplay Display { get; }
        public double RemainingTime { get; }
        public double TotalTime { get; }

        public bool IsVisible
        {
            get
            {
                if (Display == CharacterCountdownDisplay.Hidden)
                    return false;

                return Display != CharacterCountdownDisplay.ActionIcon || Action != null;
            }
        }

        public float Fill01
        {
            get
            {
                if (TotalTime <= 0d)
                    return 0f;

                return (float)(RemainingTime / TotalTime);
            }
        }

        public CharacterCountdownInfo(
            CharacterActionDefinition action,
            CharacterCountdownDisplay display,
            double remainingTime,
            double totalTime)
        {
            Action = action;
            Display = display;
            RemainingTime = remainingTime;
            TotalTime = totalTime;
        }

        public static CharacterCountdownInfo Hidden => default;
    }
}