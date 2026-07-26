using _PhotoCountdown.Gameplay.Photography;
using _PhotoCountdown.Presentation.Audio;
using UnityEngine;

namespace _PhotoCountdown.Presentation.Photography
{
    public sealed class PhotoCaptureSoundPresenter : MonoBehaviour
    {
        [SerializeField] private PhotoCaptureController _capture;
        [SerializeField] private AudioCuePlayer _captureSound;

        private void OnEnable()
        {
            if (_capture)
                _capture.PhotoCaptureStarted += PlaySound;
        }

        private void OnDisable()
        {
            if (_capture)
                _capture.PhotoCaptureStarted -= PlaySound;
        }

        private void PlaySound()
        {
            _captureSound.Play();
        }

        private void Awake()
        {
            if (!_capture)
                throw new MissingReferenceException($"{name} has no photo capture.");

            if (!_captureSound)
                throw new MissingReferenceException($"{name} has no capture sound.");
        }
    }
}