using _PhotoCountdown.Gameplay.Characters;
using _PhotoCountdown.Gameplay.Characters.Behaviours;
using _PhotoCountdown.Gameplay.Slots;
using _PhotoCountdown.Presentation.Characters;
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
        private CharacterBehaviourController _draggedBehaviour;
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

            if (!hit)
                return;

            LevelCharacter character = hit.GetComponentInParent<LevelCharacter>();

            if (!character)
                return;

            if (!character.CanBeDragged)
            {
                CharacterDragRejectFeedback feedback =
                    character.GetComponent<CharacterDragRejectFeedback>();

                if (!feedback)
                    throw new MissingComponentException(
                        $"{character.name} needs CharacterDragRejectFeedback.");

                feedback.Play();
                return;
            }

            _draggedCharacter = character;
            _draggedMover = _draggedCharacter.GetComponent<CharacterMover>();
            _draggedBehaviour = _draggedCharacter.GetComponent<CharacterBehaviourController>();

            if (!_draggedMover)
                throw new MissingComponentException($"{_draggedCharacter.name} needs CharacterMover.");

            if (!_draggedBehaviour)
                throw new MissingComponentException(
                    $"{_draggedCharacter.name} needs CharacterBehaviourController.");

            _dragOffset = _draggedCharacter.transform.position - pointerPosition;

            _draggedBehaviour.BeginDrag();
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

            if (targetSlot)
                _placement.Place(_draggedCharacter, targetSlot);

            _draggedMover.EndDrag();
            _draggedBehaviour.EndDrag();

            _draggedCharacter = null;
            _draggedMover = null;
            _draggedBehaviour = null;
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