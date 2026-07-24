using System;
using _PhotoCountdown.Gameplay.Levels;

namespace _PhotoCountdown.Gameplay.Photography
{
    public class PhotoResult
    {
        public string Id { get; }
        public string LevelId { get; }
        public long CapturedAtUtcTicks { get; }
        public PhotoSnapshot Snapshot { get; }
        public LevelRank Rank { get; }

        public DateTime CapturedAtUtc => new DateTime(CapturedAtUtcTicks, DateTimeKind.Utc);

        public bool UnlocksNextLevel => Rank >= LevelRank.OneStar;

        public PhotoResult(string levelId, long capturedAtUtcTicks, PhotoSnapshot snapshot, LevelRank rank)
        {
            if (string.IsNullOrWhiteSpace(levelId))
                throw new ArgumentException("Photo has no level id.", nameof(levelId));

            Id = Guid.NewGuid().ToString("N");
            LevelId = levelId;
            CapturedAtUtcTicks = capturedAtUtcTicks;
            Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
            Rank = rank;
        }
    }
}