using System;
using _PhotoCountdown.Gameplay.Slots;
using UnityEngine;
using UnityEngine.Rendering;

namespace _PhotoCountdown.Gameplay.Characters
{
    public sealed class CharacterMover : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 12f;

        [Header("Optional Sorting")]
        [SerializeField] private SortingGroup _sortingGroup;
        [SerializeField] private int _dragSortingOrder = 1000;

        private LevelCharacter _character;
        private CharacterSlot _sortingSlot;
        private bool _isDragging;
        private Vector3 _dragPosition;

        public bool IsSettled
        {
            get
            {
                if (_character == null || _isDragging)
                    return false;

                Vector3 target = _character.CurrentSlot.transform.position;
                return (transform.position - target).sqrMagnitude <= 0.0001f;
            }
        }

        private void Awake()
        {
            if (!_sortingGroup)
                _sortingGroup = GetComponent<SortingGroup>();

            enabled = false;
        }

        public void Init(LevelCharacter character)
        {
            if (_character)
                throw new InvalidOperationException($"{name} is already initialized.");

            if (!character)
                throw new ArgumentNullException(nameof(character));

            if (!character.CurrentSlot)
                throw new MissingReferenceException($"{character.name} has no current slot.");

            _character = character;
            transform.position = _character.CurrentSlot.transform.position;
            ApplySlotSortingOrder();
            enabled = true;
        }

        public void SetDragPosition(Vector3 position)
        {
            _dragPosition = position;
        }

        public void BeginDrag()
        {
            _isDragging = true;
            _dragPosition = transform.position;

            if (_sortingGroup)
                _sortingGroup.sortingOrder = _dragSortingOrder;
        }

        public void EndDrag()
        {
            _isDragging = false;
            ApplySlotSortingOrder();
        }

        private void Update()
        {
            if (!_isDragging && _sortingSlot != _character.CurrentSlot)
                ApplySlotSortingOrder();

            Vector3 target = _isDragging
                ? _dragPosition
                : _character.CurrentSlot.transform.position;

            float t = 1f - Mathf.Exp(-_moveSpeed * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, target, t);
        }

        private void ApplySlotSortingOrder()
        {
            _sortingSlot = _character.CurrentSlot;

            if (_sortingGroup)
                _sortingGroup.sortingOrder = _sortingSlot.SortingOrder;
        }
    }
}
