using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace _PhotoCountdown.Input
{
    [DisallowMultipleComponent]
    public sealed class CharacterSpriteHitArea : MonoBehaviour
    {
        [SerializeField] private Transform _visualRoot;

        private readonly List<Vector2> _shapePoints = new();
        private readonly HashSet<Sprite> _spritesWithoutShape = new();
        private SpriteRenderer[] _renderers;

        private void Awake()
        {
            if (!_visualRoot)
                throw new MissingReferenceException($"{name} has no visual root.");

            _renderers = _visualRoot.GetComponentsInChildren<SpriteRenderer>(true);

            if (_renderers.Length == 0)
                throw new MissingComponentException($"{name} has no SpriteRenderer inside visual root.");
        }

        public bool TryGetTopHit(Vector3 worldPosition, out SpriteRenderer hitRenderer)
        {
            hitRenderer = null;

            foreach (SpriteRenderer renderer in _renderers)
            {
                if (!ContainsOpaquePoint(renderer, worldPosition))
                    continue;

                if (!hitRenderer || IsRenderedAbove(renderer, hitRenderer))
                    hitRenderer = renderer;
            }

            return hitRenderer;
        }

        public static bool IsRenderedAbove(SpriteRenderer first, SpriteRenderer second)
        {
            SortingGroup firstGroup = first.GetComponentInParent<SortingGroup>();
            SortingGroup secondGroup = second.GetComponentInParent<SortingGroup>();

            int firstLayer = SortingLayer.GetLayerValueFromID(
                firstGroup ? firstGroup.sortingLayerID : first.sortingLayerID);
            int secondLayer = SortingLayer.GetLayerValueFromID(
                secondGroup ? secondGroup.sortingLayerID : second.sortingLayerID);

            if (firstLayer != secondLayer)
                return firstLayer > secondLayer;

            int firstOrder = firstGroup ? firstGroup.sortingOrder : first.sortingOrder;
            int secondOrder = secondGroup ? secondGroup.sortingOrder : second.sortingOrder;

            if (firstOrder != secondOrder)
                return firstOrder > secondOrder;

            if (first.sortingOrder != second.sortingOrder)
                return first.sortingOrder > second.sortingOrder;

            return first.transform.GetSiblingIndex() > second.transform.GetSiblingIndex();
        }

        private bool ContainsOpaquePoint(SpriteRenderer renderer, Vector3 worldPosition)
        {
            if (!renderer || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
                return false;

            if (!renderer.sprite || renderer.color.a <= 0.001f)
                return false;

            Sprite sprite = renderer.sprite;
            Vector2 localPosition = renderer.transform.InverseTransformPoint(worldPosition);

            if (renderer.flipX)
                localPosition.x = -localPosition.x;

            if (renderer.flipY)
                localPosition.y = -localPosition.y;

            if (!sprite.bounds.Contains(localPosition))
                return false;

            int shapeCount = sprite.GetPhysicsShapeCount();

            if (shapeCount == 0)
            {
                WarnMissingPhysicsShape(sprite);
                return false;
            }

            for (int shapeIndex = 0; shapeIndex < shapeCount; shapeIndex++)
            {
                _shapePoints.Clear();
                sprite.GetPhysicsShape(shapeIndex, _shapePoints);

                if (ContainsPoint(_shapePoints, localPosition))
                    return true;
            }

            return false;
        }

        private void WarnMissingPhysicsShape(Sprite sprite)
        {
            if (!_spritesWithoutShape.Add(sprite))
                return;

            Debug.LogWarning(
                $"{name}: sprite {sprite.name} has no Physics Shape and cannot receive drag clicks.", this);
        }

        private static bool ContainsPoint(IReadOnlyList<Vector2> polygon, Vector2 point)
        {
            if (polygon.Count < 3)
                return false;

            bool inside = false;
            int previousIndex = polygon.Count - 1;

            for (int index = 0; index < polygon.Count; index++)
            {
                Vector2 current = polygon[index];
                Vector2 previous = polygon[previousIndex];
                bool crossesY = current.y > point.y != previous.y > point.y;

                if (crossesY)
                {
                    float intersectionX = (previous.x - current.x) *
                        (point.y - current.y) / (previous.y - current.y) + current.x;

                    if (point.x < intersectionX)
                        inside = !inside;
                }

                previousIndex = index;
            }

            return inside;
        }
    }
}
