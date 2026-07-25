using UnityEngine;

namespace _PhotoCountdown.Gameplay.Photography
{
    public readonly struct PhotoIssueRegion
    {
        public Rect NormalizedRect { get; }

        public PhotoIssueRegion(Rect normalizedRect)
        {
            NormalizedRect = normalizedRect;
        }
    }
}