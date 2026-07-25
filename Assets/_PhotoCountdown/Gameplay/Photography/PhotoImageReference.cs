using System;

namespace _PhotoCountdown.Gameplay.Photography
{
    public class PhotoImageReference
    {
        public string FileName { get; }
        public int Width { get; }
        public int Height { get; }

        public PhotoImageReference(string fileName, int width, int height)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("Photo image has no file name.", nameof(fileName));

            if (width < 1)
                throw new ArgumentOutOfRangeException(nameof(width));

            if (height < 1)
                throw new ArgumentOutOfRangeException(nameof(height));

            FileName = fileName;
            Width = width;
            Height = height;
        }

    }
}