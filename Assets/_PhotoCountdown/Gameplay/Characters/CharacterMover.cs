using System;
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
        private int _normalSortingOrder;
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

            if (_sortingGroup)
                _normalSortingOrder = _sortingGroup.sortingOrder;

            transform.position = _character.CurrentSlot.transform.position;
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

            if (_sortingGroup)
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