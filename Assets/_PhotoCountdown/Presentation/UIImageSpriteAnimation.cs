using UnityEngine;
using UnityEngine.UI;

namespace _PhotoCountdown.Presentation
{

    public sealed class UIImageSpriteAnimation : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private Sprite[] frames;
        [SerializeField, Min(1f)] private float framesPerSecond = 12f;
        [SerializeField] private bool loop = true;
        [SerializeField] private bool playOnEnable = true;

        private float startedAt;
        private bool isPlaying;

        private void OnEnable()
        {
            if (playOnEnable)
                Play();
        }

        private void Update()
        {
            if (!isPlaying || frames.Length == 0)
                return;

            int frame = Mathf.FloorToInt((Time.unscaledTime - startedAt) * framesPerSecond);

            if (loop)
            {
                image.sprite = frames[frame % frames.Length];
                return;
            }

            if (frame >= frames.Length)
            {
                image.sprite = frames[^1];
                isPlaying = false;
                return;
            }

            image.sprite = frames[frame];
        }

        public void Play()
        {
            startedAt = Time.unscaledTime;
            isPlaying = true;

            if (frames.Length > 0)
                image.sprite = frames[0];
        }

        public void Stop()
        {
            isPlaying = false;
        }
    }
}