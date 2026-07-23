using _PhotoCountdown.Gameplay.Characters;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _PhotoCountdown.Input
{
    public class CharacterDragInput : MonoBehaviour
    {
        [SerializeField] private Camera _sceneCamera;
        [SerializeField] private LayerMask _characterMask;
        [SerializeField] private LayerMask _slotMask;
        
        private CharacterPlacementSystem _placement;
        private LevelCharacter _draggedCharacter;
        private CharacterMover _draggedMover;
        private Vector3 _dragOffset;
        
        public void Init(CharacterPlacementSystem placementSystem)
        {
            _placement = placementSystem;
        }
        
        private void Update()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
                BeginDrag();

            if (_draggedCharacter && Mouse.current.leftButton.isPressed)
                UpdateDrag();

            if (_draggedCharacter && Mouse.current.leftButton.wasReleasedThisFrame)
                EndDrag();
        }
        
        private void BeginDrag()
        {
            Vector3 pointerPosition = GetPointerWorldPosition();
            Collider2D hit = Physics2D.OverlapPoint(pointerPosition, _characterMask);

            if (hit == null)
                return;

            _draggedCharacter = hit.GetComponentInParent<LevelCharacter>();

            if (_draggedCharacter == null)
                return;

            _draggedMover = _draggedCharacter.GetComponent<CharacterMover>();

            if (_draggedMover == null)
                throw new MissingComponentException($"{_draggedCharacter.name} needs CharacterMover.");

            _dragOffset = _draggedCharacter.transform.position - pointerPosition;
            _draggedMover.BeginDrag();
        }
        
        private void UpdateDrag()
        {
            Vector3 dragPosition = GetPointerWorldPosition() + _dragOffset;
            _draggedMover.SetDragPosition(dragPosition);
        }
        
        private void EndDrag()
        {
            CharacterSlot targetSlot = FindSlot(GetPointerWorldPosition());

            if (targetSlot != null)
                _placement.Place(_draggedCharacter, targetSlot);

            _draggedMover.EndDrag();
            _draggedCharacter = null;
            _draggedMover = null;
        }

        private CharacterSlot FindSlot(Vector3 pointerPosition)
        {
            Collider2D hit = Physics2D.OverlapPoint(pointerPosition, _slotMask);
            return !hit ? null : hit.GetComponent<CharacterSlot>();
        }
        
        private Vector3 GetPointerWorldPosition()
        {
            Vector2 screenPosition = Mouse.current.position.ReadValue();
            Vector3 worldPosition = _sceneCamera.ScreenToWorldPoint(screenPosition);
            worldPosition.z = 0f;
            return worldPosition;
        }
    }
}