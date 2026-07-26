using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _PhotoCountdown.Gameplay.Slots
{
    public sealed class CharacterSlot : MonoBehaviour
    {
        [SerializeField] private List<CharacterSlot> _neighbors = new();
        [SerializeField] private Vector2 _dragAreaSize = Vector2.one;
        [SerializeField] private int _sortingOrder;

        public IReadOnlyList<CharacterSlot> Neighbors => _neighbors;
        public int SortingOrder => _sortingOrder;

        public bool IsNeighbor(CharacterSlot characterSlot)
        {
            return _neighbors.Contains(characterSlot);
        }

        public bool Contains(Vector3 worldPosition)
        {
            Vector3 localPosition = transform.InverseTransformPoint(worldPosition);
            Vector2 halfSize = _dragAreaSize * 0.5f;
            return Mathf.Abs(localPosition.x) <= halfSize.x &&
                   Mathf.Abs(localPosition.y) <= halfSize.y;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _dragAreaSize.x = Mathf.Max(0.01f, _dragAreaSize.x);
            _dragAreaSize.y = Mathf.Max(0.01f, _dragAreaSize.y);

            foreach (CharacterSlot neighbor in _neighbors.ToList())
            {
                if (neighbor)
                    neighbor.AddNeighborFromEditor(this);

                if (neighbor == this)
                    _neighbors.Remove(neighbor);
            }
        }

        private void AddNeighborFromEditor(CharacterSlot slot)
        {
            if (!slot || slot == this || _neighbors.Contains(slot))
                return;

            _neighbors.Add(slot);
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif

        private void OnDrawGizmosSelected()
        {
            Matrix4x4 previousMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, _dragAreaSize);
            Gizmos.matrix = previousMatrix;

            foreach (CharacterSlot neighbor in _neighbors)
            {
                if (neighbor)
                    Gizmos.DrawLine(transform.position, neighbor.transform.position);
            }
        }
    }
}
