using System;
using UnityEngine;

namespace _PhotoCountdown.Gameplay.Photography
{
    public class PhotoHighlightTarget : MonoBehaviour
    {
        [SerializeField] private Renderer[] _renderers;
        [SerializeField, Range(0f, 0.15f)] private float _viewportPadding = 0.02f;

        public void Validate()
        {
            if (_renderers == null || _renderers.Length == 0)
                throw new MissingReferenceException($"{name} has no highlight renderers.");

            foreach (Renderer targetRenderer in _renderers)
            {
                if (targetRenderer == null)
                    throw new MissingReferenceException($"{name} has a missing renderer.");
            }
        }

        public bool TryGetViewportRect(Camera camera, out Rect rect)
        {
            if (camera == null)
                throw new ArgumentNullException(nameof(camera));

            Validate();

            Bounds bounds = _renderers[0].bounds;

            for (int i = 1; i < _renderers.Length; i++)
                bounds.Encapsulate(_renderers[i].bounds);

            Vector3 min = bounds.min;
            Vector3 max = bounds.max;

            Vector3[] corners =
            {
                new Vector3(min.x, min.y, min.z),
                new Vector3(min.x, min.y, max.z),
                new Vector3(min.x, max.y, min.z),
                new Vector3(min.x, max.y, max.z),
                new Vector3(max.x, min.y, min.z),
                new Vector3(max.x, min.y, max.z),
                new Vector3(max.x, max.y, min.z),
                new Vector3(max.x, max.y, max.z)
            };

            float minX = float.PositiveInfinity;
            float minY = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float maxY = float.NegativeInfinity;
            bool hasVisiblePoint = false;

            foreach (Vector3 corner in corners)
            {
                Vector3 viewport = camera.WorldToViewportPoint(corner);

                if (viewport.z <= 0f)
                    continue;

                hasVisiblePoint = true;
                minX = Mathf.Min(minX, viewport.x);
                minY = Mathf.Min(minY, viewport.y);
                maxX = Mathf.Max(maxX, viewport.x);
                maxY = Mathf.Max(maxY, viewport.y);
            }

            if (!hasVisiblePoint || maxX <= 0f || maxY <= 0f || minX >= 1f || minY >= 1f)
            {
                rect = default;
                return false;
            }

            minX = Mathf.Clamp01(minX - _viewportPadding);
            minY = Mathf.Clamp01(minY - _viewportPadding);
            maxX = Mathf.Clamp01(maxX + _viewportPadding);
            maxY = Mathf.Clamp01(maxY + _viewportPadding);

            rect = Rect.MinMaxRect(minX, minY, maxX, maxY);
            return rect.width > 0f && rect.height > 0f;
        }
    }
}