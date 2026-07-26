using PhotoCountdown.Presentation;
using UnityEngine;

namespace _PhotoCountdown.Presentation.Characters
{
    [DisallowMultipleComponent]
    public sealed class DragTargetHighlightPresenter : MonoBehaviour
    {
        [SerializeField] private DashedSpriteOutline[] _outlines;
        [SerializeField] private GameObject[] _additionalVisuals;

        private void Awake()
        {
            SetHighlighted(false);
        }

        public void SetHighlighted(bool highlighted)
        {
            foreach (DashedSpriteOutline outline in _outlines)
            {
                if (outline)
                    outline.SetVisible(highlighted);
            }

            foreach (GameObject visual in _additionalVisuals)
            {
                if (visual)
                    visual.SetActive(highlighted);
            }
        }
    }
}
