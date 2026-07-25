using System;
using System.Collections.Generic;
using System.IO;
using _PhotoCountdown.Core.Persistence;
using _PhotoCountdown.Gameplay.Levels;
using _PhotoCountdown.Gameplay.Objectives;
using UnityEngine;

namespace _PhotoCountdown.Gameplay.Photography
{
    public class PhotoAlbumStorage
    {
        private const int CurrentVersion = 1;

        private readonly string _albumDirectory;
        private readonly string _imagesDirectory;
        private readonly string _indexPath;
        private readonly string _temporaryIndexPath;

        public PhotoAlbumStorage(string persistentDataPath)
        {
            if (string.IsNullOrWhiteSpace(persistentDataPath))
                throw new ArgumentException("Persistent data path is empty.");

            _albumDirectory = Path.Combine(persistentDataPath, "PhotoAlbum");
            _imagesDirectory = Path.Combine(_albumDirectory, "Images");
            _indexPath = Path.Combine(_albumDirectory, "album.json");
            _temporaryIndexPath = Path.Combine(_albumDirectory, "album.tmp");
        }

        public PhotoAlbum Load()
        {
            EnsureDirectories();

            PhotoAlbum album = new PhotoAlbum();

            if (!File.Exists(_indexPath))
                return album;

            try
            {
                string json = File.ReadAllText(_indexPath);
                AlbumSaveData data = JsonUtility.FromJson<AlbumSaveData>(json);

                if (data == null)
                    throw new InvalidDataException("Album JSON is empty.");

                if (data.version != CurrentVersion)
                    throw new InvalidDataException($"Unsupported album version {data.version}.");

                if (data.photos == null)
                    return album;

                foreach (PhotoSaveData photoData in data.photos)
                {
                    try
                    {
                        PhotoResult photo = CreatePhoto(photoData);

                        if (!File.Exists(GetImagePath(photo.Image)))
                        {
                            Debug.LogWarning($"Image for photo {photo.Id} is missing.");
                            continue;
                        }

                        album.Add(photo);
                    }
                    catch (Exception exception)
                    {
                        Debug.LogWarning($"Skipped invalid saved photo: {exception.Message}");
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to load photo album: {exception}");

                string backupPath = _indexPath + ".broken-" + DateTime.UtcNow.Ticks;

                try
                {
                    File.Move(_indexPath, backupPath);
                }
                catch (Exception backupException)
                {
                    Debug.LogError($"Failed to preserve broken album: {backupException}");
                }
            }

            return album;
        }

        public void SaveNewPhoto(PhotoAlbum album, PhotoResult photo, byte[] pngData)
        {
            if (album == null)
                throw new ArgumentNullException(nameof(album));

            if (photo == null)
                throw new ArgumentNullException(nameof(photo));

            if (pngData == null || pngData.Length == 0)
                throw new ArgumentException("Photo has no PNG data.", nameof(pngData));

            if (album.Contains(photo.Id))
                throw new InvalidOperationException($"Photo {photo.Id} already exists.");

            EnsureDirectories();

            string imagePath = GetImagePath(photo.Image);

            if (File.Exists(imagePath))
                throw new IOException($"Photo image {photo.Image.FileName} already exists.");

            File.WriteAllBytes(imagePath, pngData);

            try
            {
                AlbumSaveData data = CreateSaveData(album, photo);
                WriteIndex(data);
                album.Add(photo);
                WebGlFileSystemSync.Request();
            }
            catch
            {
                if (File.Exists(imagePath))
                    File.Delete(imagePath);

                throw;
            }
        }

        public byte[] LoadImageBytes(PhotoImageReference image)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            return File.ReadAllBytes(GetImagePath(image));
        }

        private AlbumSaveData CreateSaveData(PhotoAlbum album, PhotoResult addedPhoto)
        {
            AlbumSaveData data = new AlbumSaveData
            {
                version = CurrentVersion,
                photos = new List<PhotoSaveData>()
            };

            foreach (PhotoResult photo in album.Photos)
                data.photos.Add(CreatePhotoData(photo));

            data.photos.Add(CreatePhotoData(addedPhoto));
            return data;
        }

        private static PhotoSaveData CreatePhotoData(PhotoResult photo)
        {
            PhotoSaveData data = new PhotoSaveData
            {
                id = photo.Id,
                levelId = photo.LevelId,
                capturedAtUtcTicks = photo.CapturedAtUtcTicks,
                rank = (int)photo.Rank,
                image = new ImageSaveData
                {
                    fileName = photo.Image.FileName,
                    width = photo.Image.Width,
                    height = photo.Image.Height
                },
                snapshot = new SnapshotSaveData
                {
                    levelTime = photo.Snapshot.LevelTime,
                    objectives = new List<ObjectiveSaveData>(),
                    issueRegions = new List<IssueRegionSaveData>()
                }
            };

            foreach (PhotoObjectiveResult objective in photo.Snapshot.Objectives)
            {
                data.snapshot.objectives.Add(new ObjectiveSaveData
                {
                    description = objective.Description,
                    completed = objective.Completed
                });
            }

            foreach (PhotoIssueRegion region in photo.Snapshot.IssueRegions)
            {
                Rect rect = region.NormalizedRect;

                data.snapshot.issueRegions.Add(new IssueRegionSaveData
                {
                    x = rect.x,
                    y = rect.y,
                    width = rect.width,
                    height = rect.height
                });
            }

            return data;
        }

        private static PhotoResult CreatePhoto(PhotoSaveData data)
        {
            if (data == null)
                throw new InvalidDataException("Photo data is missing.");

            if (data.image == null || data.snapshot == null)
                throw new InvalidDataException($"Photo {data.id} has incomplete data.");

            if (!Enum.IsDefined(typeof(LevelRank), data.rank))
                throw new InvalidDataException($"Photo {data.id} has an invalid rank.");

            List<ObjectiveSaveData> savedObjectives =
                data.snapshot.objectives ?? new List<ObjectiveSaveData>();

            PhotoObjectiveResult[] objectives =
                new PhotoObjectiveResult[savedObjectives.Count];

            for (int i = 0; i < savedObjectives.Count; i++)
            {
                ObjectiveSaveData objective = savedObjectives[i];

                objectives[i] = new PhotoObjectiveResult(
                    objective.description,
                    objective.completed);
            }

            List<IssueRegionSaveData> savedRegions =
                data.snapshot.issueRegions ?? new List<IssueRegionSaveData>();

            PhotoIssueRegion[] regions = new PhotoIssueRegion[savedRegions.Count];

            for (int i = 0; i < savedRegions.Count; i++)
            {
                IssueRegionSaveData region = savedRegions[i];

                regions[i] = new PhotoIssueRegion(new Rect(
                    region.x,
                    region.y,
                    region.width,
                    region.height));
            }

            PhotoSnapshot snapshot = new PhotoSnapshot(
                data.snapshot.levelTime,
                objectives,
                regions);

            PhotoImageReference image = new PhotoImageReference(
                data.image.fileName,
                data.image.width,
                data.image.height);

            return new PhotoResult(
                data.id,
                data.levelId,
                data.capturedAtUtcTicks,
                snapshot,
                image,
                (LevelRank)data.rank);
        }

        private void WriteIndex(AlbumSaveData data)
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(_temporaryIndexPath, json);

            if (File.Exists(_indexPath))
                File.Delete(_indexPath);

            File.Move(_temporaryIndexPath, _indexPath);
        }

        private string GetImagePath(PhotoImageReference image)
        {
            if (Path.GetFileName(image.FileName) != image.FileName)
                throw new InvalidDataException("Photo image contains an invalid file name.");

            return Path.Combine(_imagesDirectory, image.FileName);
        }

        private void EnsureDirectories()
        {
            Directory.CreateDirectory(_albumDirectory);
            Directory.CreateDirectory(_imagesDirectory);
        }

        [Serializable]
        private sealed class AlbumSaveData
        {
            public int version;
            public List<PhotoSaveData> photos;
        }

        [Serializable]
        private sealed class PhotoSaveData
        {
            public string id;
            public string levelId;
            public long capturedAtUtcTicks;
            public int rank;
            public ImageSaveData image;
            public SnapshotSaveData snapshot;
        }

        [Serializable]
        private sealed class ImageSaveData
        {
            public string fileName;
            public int width;
            public int height;
        }

        [Serializable]
        private sealed class SnapshotSaveData
        {
            public double levelTime;
            public List<ObjectiveSaveData> objectives;
            public List<IssueRegionSaveData> issueRegions;
        }

        [Serializable]
        private sealed class ObjectiveSaveData
        {
            public string description;
            public bool completed;
        }

        [Serializable]
        private sealed class IssueRegionSaveData
        {
            public float x;
            public float y;
            public float width;
            public float height;
        }
    }
}