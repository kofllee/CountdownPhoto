using System;
using System.Collections.Generic;
using _PhotoCountdown.Gameplay.Levels;

namespace _PhotoCountdown.Gameplay.Photography
{
    public class PhotoAlbum
    {
        private readonly List<PhotoResult> _photos = new();
        private readonly HashSet<string> _photoIds = new();

        public IReadOnlyList<PhotoResult> Photos => _photos;

        public void Add(PhotoResult photo)
        {
            if (photo == null)
                throw new ArgumentNullException(nameof(photo));

            if (!_photoIds.Add(photo.Id))
                throw new InvalidOperationException($"Photo {photo.Id} already exists.");

            _photos.Add(photo);
        }

        public bool Contains(string photoId)
        {
            return _photoIds.Contains(photoId);
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