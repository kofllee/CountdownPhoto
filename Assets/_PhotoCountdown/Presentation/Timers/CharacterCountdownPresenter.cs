using _PhotoCountdown.Gameplay.Characters.Actions;
using _PhotoCountdown.Gameplay.Characters.Behaviours;
using TMPro;
using UnityEngine;

namespace _PhotoCountdown.Presentation.Timers
{
    public class CharacterCountdownPresenter : MonoBehaviour
    {
        private static readonly int FillAmountId = Shader.PropertyToID("_FillAmount");

        [SerializeField] private CharacterBehaviourController _controller;
        [SerializeField] private GameObject _viewRoot;
        [SerializeField] private SpriteRenderer _ringFill;
        [SerializeField] private TMP_Text _timeText;
        [SerializeField] private SpriteRenderer _actionIcon;
        [SerializeField] private Color _ringColor = Color.white;

        private MaterialPropertyBlock _ringProperties;
        private int _shownSeconds = -1;
        private Sprite _shownIcon;

        private void Awake()
        {
            ValidateReferences();

            _timeText.enabled = false;
            _actionIcon.enabled = false;
            _viewRoot.SetActive(false);
            _ringProperties = new MaterialPropertyBlock();
        }

        private void Update()
        {
            if (!_controller.IsInitialized || _controller.IsDragging)
            {
                _viewRoot.SetActive(false);
                return;
            }

            CharacterCountdownInfo countdown = _controller.Countdown;

            if (!countdown.IsVisible)
            {
                _viewRoot.SetActive(false);
                return;
            }

            _viewRoot.SetActive(true);
            SetRing(countdown.Fill01);

            if (countdown.Display == CharacterCountdownDisplay.Seconds)
                ShowSeconds(countdown.RemainingTime);
            else
                ShowIcon(countdown.Action);
        }

        private void ShowSeconds(double remainingTime)
        {
            _timeText.enabled = true;
            _actionIcon.enabled = false;

            int seconds = Mathf.Max(0, Mathf.CeilToInt((float)remainingTime));

            if (_shownSeconds == seconds)
                return;

            _shownSeconds = seconds;
            _timeText.text = seconds.ToString();
        }

        private void ShowIcon(CharacterActionDefinition action)
        {
            _timeText.enabled = false;
            _actionIcon.enabled = true;

            if (!action.Icon)
                throw new MissingReferenceException($"{action.name} has no countdown icon.");

            if (_shownIcon == action.Icon)
                return;

            _shownIcon = action.Icon;
            _actionIcon.sprite = action.Icon;
        }

        private void SetRing(float fill)
        {
            _ringFill.color = _ringColor;
            _ringFill.GetPropertyBlock(_ringProperties);
            _ringProperties.SetFloat(FillAmountId, Mathf.Clamp01(fill));
            _ringFill.SetPropertyBlock(_ringProperties);
        }

        private void ValidateReferences()
        {
            if (_controller == null)
                throw new MissingReferenceException($"{name} has no behaviour controller.");

            if (_viewRoot == null)
                throw new MissingReferenceException($"{name} has no countdown view root.");

            if (_ringFill == null)
                throw new MissingReferenceException($"{name} has no ring fill.");

            if (_timeText == null)
                throw new MissingReferenceException($"{name} has no countdown text.");

            if (_actionIcon == null)
                throw new MissingReferenceException($"{name} has no action icon.");
        }
    }
}