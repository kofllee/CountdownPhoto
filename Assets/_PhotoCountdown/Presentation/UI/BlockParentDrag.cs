using UnityEngine;
using UnityEngine.EventSystems;

namespace _PhotoCountdown.Presentation.UI
{
    public sealed class BlockParentDrag : MonoBehaviour, IBeginDragHandler,
        IDragHandler, IEndDragHandler
    {
        public void OnBeginDrag(PointerEventData eventData) { }

        public void OnDrag(PointerEventData eventData) { }

        public void OnEndDrag(PointerEventData eventData) { }
    }
}