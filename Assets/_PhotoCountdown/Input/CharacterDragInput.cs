using System.Collections.Generic;
using _PhotoCountdown.Gameplay.Characters;
using _PhotoCountdown.Gameplay.Characters.Behaviours;
using _PhotoCountdown.Gameplay.Slots;
using _PhotoCountdown.Presentation.Characters;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _PhotoCountdown.Input
{
    public sealed class CharacterDragInput : MonoBehaviour
    {
        [SerializeField] private Camera _sceneCamera;

        private CharacterPlacementSystem _placement;
        private CharacterTarget[] _characterTargets;
        private SlotTarget[] _slotTargets;
        private LevelCharacter _draggedCharacter;
        private CharacterMover _draggedMover;
        private CharacterBehaviourController _draggedBehaviour;
        private LevelCharacter _targetCharacter;
        private CharacterSlot _targetSlot;
        private DragTargetHighlightPresenter _targetHighlight;
        private Vector3 _dragOffset;

        public void Init(CharacterPlacementSystem placementSystem,
            IReadOnlyList<LevelCharacter> characters, IReadOnlyList<CharacterSlot> slots)
        {
            _placement = placementSystem;
            _characterTargets = new CharacterTarget[characters.Count];
            _slotTargets = new SlotTarget[slots.Count];

            for (int index = 0; index < characters.Count; index++)
                _characterTargets[index] = CreateCharacterTarget(characters[index]);

            for (int index = 0; index < slots.Count; index++)
                _slotTargets[index] = CreateSlotTarget(slots[index]);
        }

        private void Awake()
        {
            if (!_sceneCamera)
                throw new MissingReferenceException($"{name} has no scene camera.");
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
            LevelCharacter character = FindTopCharacter(pointerPosition, null);

            if (!character)
                return;

            if (!character.CanBeDragged)
            {
                PlayRejectFeedback(character);
                return;
            }

            _draggedCharacter = character;
            _draggedMover = character.GetComponent<CharacterMover>();
            _draggedBehaviour = character.GetComponent<CharacterBehaviourController>();

            if (!_draggedMover)
                throw new MissingComponentException($"{character.name} needs CharacterMover.");

            if (!_draggedBehaviour)
                throw new MissingComponentException(
                    $"{character.name} needs CharacterBehaviourController.");

            _dragOffset = character.transform.position - pointerPosition;
            _draggedBehaviour.BeginDrag();
            _draggedMover.BeginDrag();
            SetFreeSlotHighlights(true);
        }

        private void UpdateDrag()
        {
            Vector3 pointerPosition = GetPointerWorldPosition();
            _draggedMover.SetDragPosition(pointerPosition + _dragOffset);
            UpdateTarget(pointerPosition);
        }

        private void EndDrag()
        {
            if (_targetCharacter)
                _placement.Swap(_draggedCharacter, _targetCharacter);
            else if (_targetSlot)
                _placement.Place(_draggedCharacter, _targetSlot);

            _draggedMover.EndDrag();
            _draggedBehaviour.EndDrag();
            ClearTarget();
            SetFreeSlotHighlights(false);
            ClearDrag();
        }

        private void UpdateTarget(Vector3 pointerPosition)
        {
            LevelCharacter character = FindTopCharacter(pointerPosition, _draggedCharacter);

            if (character)
            {
                if (_placement.CanSwap(_draggedCharacter, character))
                    SetCharacterTarget(character);
                else
                    ClearTarget();

                return;
            }

            CharacterSlot slot = FindClosestFreeSlot(pointerPosition);

            if (slot && _placement.CanPlace(_draggedCharacter, slot))
                SetSlotTarget(slot);
            else
                ClearTarget();
        }

        private LevelCharacter FindTopCharacter(Vector3 pointerPosition,
            LevelCharacter ignoredCharacter)
        {
            LevelCharacter topCharacter = null;
            SpriteRenderer topRenderer = null;

            foreach (CharacterTarget target in _characterTargets)
            {
                if (target.Character == ignoredCharacter)
                    continue;

                if (!target.HitArea.TryGetTopHit(pointerPosition, out SpriteRenderer hitRenderer))
                    continue;

                if (!topRenderer || CharacterSpriteHitArea.IsRenderedAbove(hitRenderer, topRenderer))
                {
                    topCharacter = target.Character;
                    topRenderer = hitRenderer;
                }
            }

            return topCharacter;
        }

        private CharacterSlot FindClosestFreeSlot(Vector3 pointerPosition)
        {
            CharacterSlot closestSlot = null;
            float closestDistance = float.PositiveInfinity;

            foreach (SlotTarget target in _slotTargets)
            {
                if (!target.Slot.Contains(pointerPosition))
                    continue;

                if (_placement.FindAtSlot(target.Slot))
                    continue;

                float distance = (target.Slot.transform.position - pointerPosition).sqrMagnitude;

                if (distance < closestDistance)
                {
                    closestSlot = target.Slot;
                    closestDistance = distance;
                }
            }

            return closestSlot;
        }

        private void SetCharacterTarget(LevelCharacter character)
        {
            CharacterTarget target = FindTarget(character);
            SetTarget(target.Highlight, character, null);
        }

        private void SetSlotTarget(CharacterSlot slot)
        {
            if (_targetSlot == slot && !_targetCharacter)
                return;

            ClearTarget();
            _targetSlot = slot;
        }

        private void SetTarget(DragTargetHighlightPresenter highlight,
            LevelCharacter character, CharacterSlot slot)
        {
            if (_targetHighlight == highlight)
                return;

            ClearTarget();
            _targetCharacter = character;
            _targetSlot = slot;
            _targetHighlight = highlight;
            _targetHighlight.SetHighlighted(true);
        }

        private void ClearTarget()
        {
            if (_targetHighlight)
                _targetHighlight.SetHighlighted(false);

            _targetCharacter = null;
            _targetSlot = null;
            _targetHighlight = null;
        }

        private void SetFreeSlotHighlights(bool highlighted)
        {
            if (_slotTargets == null)
                return;

            foreach (SlotTarget target in _slotTargets)
            {
                bool available = highlighted && _placement.CanPlace(_draggedCharacter, target.Slot);
                target.Highlight.SetHighlighted(available);
            }
        }

        private void ClearDrag()
        {
            _draggedCharacter = null;
            _draggedMover = null;
            _draggedBehaviour = null;
        }

        private CharacterTarget CreateCharacterTarget(LevelCharacter character)
        {
            CharacterSpriteHitArea hitArea = character.GetComponent<CharacterSpriteHitArea>();

            if (!hitArea)
                throw new MissingComponentException($"{character.name} needs CharacterSpriteHitArea.");

            DragTargetHighlightPresenter highlight = character.GetComponent<DragTargetHighlightPresenter>();

            if (character.CanBeDragged && !highlight)
            {
                throw new MissingComponentException(
                    $"{character.name} needs DragTargetHighlightPresenter.");
            }

            return new CharacterTarget(character, hitArea, highlight);
        }

        private static SlotTarget CreateSlotTarget(CharacterSlot slot)
        {
            DragTargetHighlightPresenter highlight = slot.GetComponent<DragTargetHighlightPresenter>();

            if (!highlight)
                throw new MissingComponentException($"{slot.name} needs DragTargetHighlightPresenter.");

            return new SlotTarget(slot, highlight);
        }

        private CharacterTarget FindTarget(LevelCharacter character)
        {
            foreach (CharacterTarget target in _characterTargets)
            {
                if (target.Character == character)
                    return target;
            }

            throw new KeyNotFoundException($"Character target for {character.name} was not initialized.");
        }

        private SlotTarget FindTarget(CharacterSlot slot)
        {
            foreach (SlotTarget target in _slotTargets)
            {
                if (target.Slot == slot)
                    return target;
            }

            throw new KeyNotFoundException($"Slot target for {slot.name} was not initialized.");
        }

        private static void PlayRejectFeedback(LevelCharacter character)
        {
            CharacterDragRejectFeedback feedback = character.GetComponent<CharacterDragRejectFeedback>();

            if (!feedback)
            {
                throw new MissingComponentException(
                    $"{character.name} needs CharacterDragRejectFeedback.");
            }

            feedback.Play();
        }

        private Vector3 GetPointerWorldPosition()
        {
            Vector2 screenPosition = Mouse.current.position.ReadValue();
            Vector3 worldPosition = _sceneCamera.ScreenToWorldPoint(screenPosition);
            worldPosition.z = 0f;
            return worldPosition;
        }

        private void OnDisable()
        {
            ClearTarget();
            SetFreeSlotHighlights(false);

            if (!_draggedCharacter)
                return;

            _draggedMover.EndDrag();
            _draggedBehaviour.EndDrag();
            ClearDrag();
        }

        private sealed class CharacterTarget
        {
            public LevelCharacter Character { get; }
            public CharacterSpriteHitArea HitArea { get; }
            public DragTargetHighlightPresenter Highlight { get; }

            public CharacterTarget(LevelCharacter character, CharacterSpriteHitArea hitArea,
                DragTargetHighlightPresenter highlight)
            {
                Character = character;
                HitArea = hitArea;
                Highlight = highlight;
            }
        }

        private sealed class SlotTarget
        {
            public CharacterSlot Slot { get; }
            public DragTargetHighlightPresenter Highlight { get; }

            public SlotTarget(CharacterSlot slot, DragTargetHighlightPresenter highlight)
            {
                Slot = slot;
                Highlight = highlight;
            }
        }
    }
}