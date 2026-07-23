using UnityEngine;
using UnityEngine.Rendering;

namespace _PhotoCountdown.Gameplay.Characters
{
    public class CharacterMover: MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 12f;
        [SerializeField] private SortingGroup _sortingGroup;
        [SerializeField] private int _dragSortingOrder = 1000;

        
        private LevelCharacter _character;
        private int _normalSortingOrder;
        private bool _isDragging;
        private Vector3 _dragPosition;
        
        
        public void Init(LevelCharacter levelCharacter)
        {
            _character = levelCharacter;
            transform.position = _character.CurrentSlot.transform.position;
        }
        
        public void SetDragPosition(Vector3 position)
        {
            _dragPosition = position;
        }
        
        public void BeginDrag()
        {
            _isDragging = true;
            _sortingGroup.sortingOrder = _dragSortingOrder;
        }
        
        public void EndDrag()
        {
            _isDragging = false;
            _sortingGroup.sortingOrder = _normalSortingOrder;
        }
        
        private void Update()
        {
            Vector3 target = _isDragging
                ? _dragPosition
                : _character.CurrentSlot.transform.position;

            float t = 1f - Mathf.Exp(-_moveSpeed * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, target, t);
        }
    }
}