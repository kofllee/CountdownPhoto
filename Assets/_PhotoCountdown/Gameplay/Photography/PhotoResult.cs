using System;
using _PhotoCountdown.Gameplay.Levels;

namespace _PhotoCountdown.Gameplay.Photography
{
    public sealed class PhotoResult
    {
        public string Id { get; }
        public string LevelId { get; }
        public long CapturedAtUtcTicks { get; }
        public PhotoSnapshot Snapshot { get; }
        public PhotoImageReference Image { get; }
        public LevelRank Rank { get; }

        public DateTime CapturedAtUtc => new DateTime(CapturedAtUtcTicks, DateTimeKind.Utc);

        public bool UnlocksNextLevel => Rank >= LevelRank.OneStar;

        public PhotoResult(
            string id,
            string levelId,
            long capturedAtUtcTicks,
            PhotoSnapshot snapshot,
            PhotoImageReference image,
            LevelRank rank)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Photo has no id.", nameof(id));

            if (string.IsNullOrWhiteSpace(levelId))
                throw new ArgumentException("Photo has no level id.", nameof(levelId));

            Id = id;
            LevelId = levelId;
            CapturedAtUtcTicks = capturedAtUtcTicks;
            Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
            Image = image ?? throw new ArgumentNullException(nameof(image));
            Rank = rank;
        }
    }
}