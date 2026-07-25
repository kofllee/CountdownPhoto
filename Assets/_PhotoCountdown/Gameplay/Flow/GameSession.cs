using System;
using _PhotoCountdown.Gameplay.Levels;
using _PhotoCountdown.Gameplay.Photography;

namespace _PhotoCountdown.Gameplay.Flow
{
    public sealed class GameSession
    {
        public LevelCatalog Levels { get; }
        public PhotoAlbum Album { get; }
        public PhotoAlbumStorage PhotoStorage { get; }

        public GameSession(
            LevelCatalog levels,
            PhotoAlbum album,
            PhotoAlbumStorage photoStorage)
        {
            Levels = levels ?? throw new ArgumentNullException(nameof(levels));
            Album = album ?? throw new ArgumentNullException(nameof(album));
            PhotoStorage = photoStorage ?? throw new ArgumentNullException(nameof(photoStorage));
        }

        public LevelRank GetBestRank(LevelDefinition level)
        {
            ValidateLevel(level);
            return Album.GetBestRank(level.Id);
        }

        public bool IsLevelUnlocked(LevelDefinition level)
        {
            ValidateLevel(level);

            int index = Levels.IndexOf(level);

            if (index < 0)
                throw new InvalidOperationException($"{level.name} is not in the level catalog.");

            if (index == 0)
                return true;

            LevelDefinition previousLevel = Levels.GetAt(index - 1);
            return GetBestRank(previousLevel) >= LevelRank.OneStar;
        }

        private static void ValidateLevel(LevelDefinition level)
        {
            if (!level)
                throw new ArgumentNullException(nameof(level));
        }
    }
}