using System;
using System.Collections.Generic;
using _PhotoCountdown.Gameplay.Levels;

namespace _PhotoCountdown.Gameplay.Photography
{
    public class PhotoAlbum
    {
        private readonly List<PhotoResult> _photos = new();

        public event Action<PhotoResult> PhotoAdded;

        public IReadOnlyList<PhotoResult> Photos => _photos;

        public void Add(PhotoResult photo)
        {
            if (photo == null)
                throw new ArgumentNullException(nameof(photo));

            _photos.Add(photo);
            PhotoAdded?.Invoke(photo);
        }

        public IEnumerable<PhotoResult> GetLevelPhotos(string levelId)
        {
            foreach (PhotoResult photo in _photos)
            {
                if (photo.LevelId == levelId)
                    yield return photo;
            }
        }

        public LevelRank GetBestRank(string levelId)
        {
            LevelRank bestRank = LevelRank.Failed;

            foreach (PhotoResult photo in GetLevelPhotos(levelId))
            {
                if (photo.Rank > bestRank)
                    bestRank = photo.Rank;
            }

            return bestRank;
        }

        public int GetLevelPhotoCount(string levelId)
        {
            int count = 0;

            foreach (PhotoResult photo in GetLevelPhotos(levelId))
                count++;

            return count;
        }
    }
}