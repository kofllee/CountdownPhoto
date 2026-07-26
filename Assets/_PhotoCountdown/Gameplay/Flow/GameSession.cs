using System;
using _PhotoCountdown.Core.Settings;
using _PhotoCountdown.Gameplay.Levels;
using _PhotoCountdown.Gameplay.Photography;
using _PhotoCountdown.Presentation.Audio;

namespace _PhotoCountdown.Gameplay.Flow
{
    public sealed class GameSession
    {
        private readonly GameSettingsStorage _settingsStorage;

        public LevelCatalog Levels { get; }
        public PhotoAlbum Album { get; }
        public PhotoAlbumStorage PhotoStorage { get; }
        public GameSettings Settings { get; }
        public GameAudio Audio { get; }

        public GameSession(
            LevelCatalog levels,
            PhotoAlbum album,
            PhotoAlbumStorage photoStorage,
            GameSettings settings,
            GameSettingsStorage settingsStorage,
            GameAudio audio)
        {
            Levels = levels ?? throw new ArgumentNullException(nameof(levels));
            Album = album ?? throw new ArgumentNullException(nameof(album));
            PhotoStorage = photoStorage ?? throw new ArgumentNullException(nameof(photoStorage));
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _settingsStorage =
                settingsStorage ?? throw new ArgumentNullException(nameof(settingsStorage));
            Audio = audio ? audio : throw new ArgumentNullException(nameof(audio));
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

        public void SaveSettings()
        {
            _settingsStorage.Save(Settings);
        }

        public void DeleteAllData()
        {
            PhotoStorage.DeleteAll();
            Album.Clear();

            _settingsStorage.Delete();
            Settings.Reset();
        }

        private static void ValidateLevel(LevelDefinition level)
        {
            if (!level)
                throw new ArgumentNullException(nameof(level));
        }
    }
}