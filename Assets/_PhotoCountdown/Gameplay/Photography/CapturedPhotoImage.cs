using System;

namespace _PhotoCountdown.Gameplay.Photography
{
    public class CapturedPhotoImage
    {
        public byte[] PngData { get; }
        public int Width { get; }
        public int Height { get; }

        public CapturedPhotoImage(byte[] pngData, int width, int height)
        {
            PngData = pngData ?? throw new ArgumentNullException(nameof(pngData));
            Width = width;
            Height = height;
        }
    }
}