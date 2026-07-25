using PhotoCountdown.Presentation.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _PhotoCountdown.Presentation.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class ButtonVisualFeedback : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler,
        IPointerUpHandler, ISelectHandler, IDeselectHandler
    {
        [Header("References")]
        [SerializeField] private RectTransform scaleTarget;
        [SerializeField] private Graphic colorTarget;
        [SerializeField] private DashedUIImageOutline outline;

        [Header("Scale")]
        [SerializeField, Min(1f)] private float pressedScale = 1.05f;
        [SerializeField, Min(0f)] private float scaleSpeed = 18f;

        [Header("Button Colors")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color hoveredColor = Color.white;
        [SerializeField] private Color pressedColor = new(0.9f, 0.9f, 0.9f);
        [SerializeField] private Color disabledColor = new(1f, 1f, 1f, 0.5f);
        [SerializeField, Min(0f)] private float colorSpeed = 18f;

        [Header("Outline")]
        [SerializeField] private Color normalOutlineColor = Color.white;
        [SerializeField] private Color pressedOutlineColor = new(1f, 0.75f, 0.25f);
        [SerializeField] private bool showOutlineWhenSelected = true;
        [SerializeField] private bool restartOutlineAnimation = true;

        private Button button;
        private Vector3 defaultScale;
        private Vector3 targetScale;
        private Color targetColor;
        private bool isHovered;
        private bool isSelected;
        private bool isPressed;

        private void Awake()
        {
            CacheReferences();

            defaultScale = scaleTarget.localScale;
            targetScale = defaultScale;
            targetColor = normalColor;

            button.transition = Selectable.Transition.None;
            colorTarget.color = normalColor;
            outline.SetColor(normalOutlineColor);
            outline.Hide();
        }

        private void OnEnable()
        {
            CacheReferences();

            defaultScale = scaleTarget.localScale;
            targetScale = defaultScale;
            targetColor = button.interactable ? normalColor : disabledColor;

            isHovered = false;
            isSelected = false;
            isPressed = false;

            button.transition = Selectable.Transition.None;
            scaleTarget.localScale = defaultScale;
            colorTarget.color = targetColor;
            outline.SetColor(normalOutlineColor);
            UpdateVisualState();
        }

        private void OnDisable()
        {
            if (scaleTarget != null)
                scaleTarget.localScale = defaultScale;

            if (colorTarget != null)
                colorTarget.color = normalColor;

            if (outline != null)
            {
                outline.SetColor(normalOutlineColor);
                outline.Hide();
            }

            isHovered = false;
            isSelected = false;
            isPressed = false;
        }

        private void Update()
        {
            if (!button.interactable && (isHovered || isPressed))
            {
                isHovered = false;
                isPressed = false;
                UpdateVisualState();
            }

            float scaleFactor = 1f - Mathf.Exp(-scaleSpeed * Time.unscaledDeltaTime);
            float colorFactor = 1f - Mathf.Exp(-colorSpeed * Time.unscaledDeltaTime);

            scaleTarget.localScale = Vector3.Lerp(scaleTarget.localScale, targetScale, scaleFactor);
            colorTarget.color = Color.Lerp(colorTarget.color, targetColor, colorFactor);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!button.interactable)
                return;

            isHovered = true;
            UpdateVisualState();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            isPressed = false;
            isSelected = false;
            UpdateVisualState();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!button.interactable || eventData.button != PointerEventData.InputButton.Left)
                return;

            isPressed = true;
            UpdateVisualState();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            isPressed = false;
            UpdateVisualState();
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (!button.interactable)
                return;

            isSelected = true;
            UpdateVisualState();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            isSelected = false;
            isPressed = false;
            UpdateVisualState();
        }

        private void CacheReferences()
        {
            if (button == null)
                button = GetComponent<Button>();

            if (scaleTarget == null)
                scaleTarget = transform as RectTransform;

            if (colorTarget == null)
                colorTarget = button.targetGraphic;

            if (outline == null)
                outline = FindOutline();

            if (scaleTarget == null)
                throw new MissingReferenceException($"{name} has no scale target.");

            if (colorTarget == null)
                throw new MissingReferenceException($"{name} has no color target.");

            if (outline == null)
                throw new MissingReferenceException(
                    $"{name} has no {nameof(DashedUIImageOutline)}.");
        }

        private DashedUIImageOutline FindOutline()
        {
            DashedUIImageOutline foundOutline = GetComponent<DashedUIImageOutline>();

            if (foundOutline != null)
                return foundOutline;

            if (button != null && button.targetGraphic != null)
            {
                foundOutline = button.targetGraphic.GetComponent<DashedUIImageOutline>();

                if (foundOutline != null)
                    return foundOutline;
            }

            return GetComponentInChildren<DashedUIImageOutline>(true);
        }

        private void UpdateVisualState()
        {
            bool interactable = button != null && button.interactable;

            targetScale = interactable && isPressed
                ? defaultScale * pressedScale
                : defaultScale;

            if (!interactable)
                targetColor = disabledColor;
            else if (isPressed)
                targetColor = pressedColor;
            else if (isHovered || showOutlineWhenSelected && isSelected)
                targetColor = hoveredColor;
            else
                targetColor = normalColor;

            UpdateOutline(interactable);
        }

        private void UpdateOutline(bool interactable)
        {
            if (outline == null)
                return;

            bool shouldShow = interactable && isHovered;

            outline.SetColor(isPressed ? pressedOutlineColor : normalOutlineColor);

            if (shouldShow)
            {
                if (!outline.IsVisible && restartOutlineAnimation)
                    outline.RestartAnimation();

                outline.Show();
            }
            else
            {
                outline.Hide();
            }
        }

#if UNITY_EDITOR
        private void Reset()
        {
            button = GetComponent<Button>();
            scaleTarget = transform as RectTransform;
            colorTarget = button.targetGraphic;
            outline = FindOutline();

            button.transition = Selectable.Transition.None;

            if (colorTarget != null)
                normalColor = colorTarget.color;

            if (outline != null)
                outline.Hide();
        }
#endif
    }
}