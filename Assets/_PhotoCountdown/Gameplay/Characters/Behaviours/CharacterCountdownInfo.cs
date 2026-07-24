using _PhotoCountdown.Gameplay.Characters.Actions;

namespace _PhotoCountdown.Gameplay.Characters.Behaviours
{
    public readonly struct CharacterCountdownInfo
    {
        public CharacterActionDefinition Action { get; }
        public CharacterCountdownType Type { get; }
        public double RemainingTime { get; }
        public double TotalTime { get; }

        public bool IsVisible => Action != null && Type != CharacterCountdownType.Hidden;
        
        public float Fill01
        {
            get
            {
                if (TotalTime <= 0d)
                    return 0f;

                return (float)(RemainingTime / TotalTime);
            }
        }

        
        public CharacterCountdownInfo(CharacterActionDefinition action, CharacterCountdownType type, double remainingTime, double totalTime)
        {
            Action = action;
            Type = type;
            RemainingTime = remainingTime;
            TotalTime = totalTime;
        }

        public static CharacterCountdownInfo Hidden => default;
    }
}