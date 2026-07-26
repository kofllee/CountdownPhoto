using _PhotoCountdown.Gameplay.Interactions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _PhotoCountdown.Input
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class TimedDisappearInteractionClickInput : MonoBehaviour
    {
        [SerializeField] private Camera _sceneCamera;
        [SerializeField] private TimedDisappearInteraction _interaction;
        [SerializeField] private Collider2D _hitArea;

        private void Reset()
        {
            _hitArea = GetComponent<Collider2D>();
        }

        private void Awake()
        {
            if (!_sceneCamera)
                throw new MissingReferenceException($"{name} has no scene camera.");

            if (!_interaction)
                throw new MissingReferenceException($"{name} has no interaction.");

            if (!_hitArea)
                throw new MissingReferenceException($"{name} has no hit area.");
        }

        private void Update()
        {
            if (!_interaction.CanActivate)
                return;

            if (!Mouse.current.leftButton.wasPressedThisFrame)
                return;

            Vector3 pointerPosition = GetPointerWorldPosition();

            if (!_hitArea.OverlapPoint(pointerPosition))
                return;

            _interaction.Activate();
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