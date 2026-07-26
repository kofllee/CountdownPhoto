using System.Collections;
using UnityEngine;

namespace _PhotoCountdown.Presentation.Characters
{
    public sealed class CharacterDragRejectFeedback : MonoBehaviour
    {
        [SerializeField] private Transform _visualRoot;
        [SerializeField] private float _duration = 0.22f;
        [SerializeField] private float _distance = 0.12f;
        [SerializeField] private float _cycles = 3f;

        private Coroutine _shakeRoutine;
        private Vector3 _normalLocalPosition;

        private void Awake()
        {
            if (!_visualRoot)
                throw new MissingReferenceException($"{name} has no visual root.");

            _normalLocalPosition = _visualRoot.localPosition;
        }

        public void Play()
        {
            if (_shakeRoutine != null)
                StopCoroutine(_shakeRoutine);

            _visualRoot.localPosition = _normalLocalPosition;
            _shakeRoutine = StartCoroutine(Shake());
        }

        private IEnumerator Shake()
        {
            float elapsedTime = 0f;

            while (elapsedTime < _duration)
            {
                elapsedTime += Time.deltaTime;

                float progress = Mathf.Clamp01(elapsedTime / _duration);
                float strength = 1f - progress;
                float angle = progress * _cycles * Mathf.PI * 2f;
                float offset = Mathf.Sin(angle) * _distance * strength;

                _visualRoot.localPosition = _normalLocalPosition + Vector3.right * offset;
                yield return null;
            }

            _visualRoot.localPosition = _normalLocalPosition;
            _shakeRoutine = null;
        }

        private void OnDisable()
        {
            if (_shakeRoutine != null)
                StopCoroutine(_shakeRoutine);

            if (_visualRoot)
                _visualRoot.localPosition = _normalLocalPosition;

            _shakeRoutine = null;
        }
    }
}