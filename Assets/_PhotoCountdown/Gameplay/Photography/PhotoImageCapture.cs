using System;
using System.Collections;
using UnityEngine;

namespace _PhotoCountdown.Gameplay.Photography
{
    public class PhotoImageCapture : MonoBehaviour
    {
        [SerializeField] private Camera _photoCamera;
        [SerializeField] private Vector2Int _resolution = new Vector2Int(960, 540);

        public Camera PhotoCamera => _photoCamera;

        private void Awake()
        {
            if (_photoCamera != null)
                _photoCamera.enabled = false;
        }

        public IEnumerator Capture(Action<CapturedPhotoImage> completed)
        {
            if (completed == null)
                throw new ArgumentNullException(nameof(completed));

            Validate();

            RenderTexture target = RenderTexture.GetTemporary(
                _resolution.x,
                _resolution.y,
                24,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.sRGB);

            RenderTexture previousActive = RenderTexture.active;
            RenderTexture previousTarget = _photoCamera.targetTexture;
            Texture2D texture = null;

            try
            {
                _photoCamera.targetTexture = target;
                _photoCamera.enabled = true;

                yield return new WaitForEndOfFrame();

                _photoCamera.enabled = false;
                RenderTexture.active = target;

                texture = new Texture2D(_resolution.x, _resolution.y, TextureFormat.RGB24, false);

                texture.ReadPixels(new Rect(0f, 0f, _resolution.x, _resolution.y), 0, 0, false);

                texture.Apply(false, false);

                byte[] pngData = ImageConversion.EncodeToPNG(texture);
                completed.Invoke(new CapturedPhotoImage(pngData, _resolution.x, _resolution.y));
            }
            finally
            {
                _photoCamera.enabled = false;
                _photoCamera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;

                if (texture != null)
                    Destroy(texture);

                RenderTexture.ReleaseTemporary(target);
            }
        }

        private void Validate()
        {
            if (_photoCamera == null)
                throw new MissingReferenceException($"{name} has no photo camera.");

            if (_resolution.x < 1 || _resolution.y < 1)
                throw new MissingReferenceException($"{name} has an invalid resolution.");
        }
    }
}