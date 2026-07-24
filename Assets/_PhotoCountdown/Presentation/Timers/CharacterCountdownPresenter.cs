using _PhotoCountdown.Gameplay.Characters.Actions;
using _PhotoCountdown.Gameplay.Characters.Behaviours;
using TMPro;
using UnityEngine;

namespace _PhotoCountdown.Presentation.Timers
{
    public sealed class CharacterCountdownPresenter : MonoBehaviour
    {
        private static readonly int FillAmountId = Shader.PropertyToID("_FillAmount");

        [SerializeField] private CharacterBehaviourController _controller; 
        [SerializeField] private GameObject _viewRoot;
        [SerializeField] private SpriteRenderer _ringFill;
        [SerializeField] private TMP_Text _timeText;
        [SerializeField] private SpriteRenderer _actionIcon;
        [SerializeField] private Color _positiveColor = Color.green;
        [SerializeField] private Color _negativeColor = Color.red;

        private MaterialPropertyBlock _ringProperties;
        private int _shownSeconds = -1;
        private Sprite _shownIcon;

        private void Awake()
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

            _actionIcon.enabled = false;
            _ringProperties = new();
        }

        private void Update()
        {
            if (!_controller.IsInitialized)
            {
                _viewRoot.SetActive(false);
                return;
            }

            CharacterActionDefinition action = _controller.CurrentAction;

            if (action)
                ShowAction(action);
            else
                ShowWaiting();
        }

        private void ShowWaiting()
        {
            CharacterCountdownInfo countdown = _controller.Countdown;

            if (!countdown.IsVisible)
            {
                _viewRoot.SetActive(false);
                return;
            }

            _viewRoot.SetActive(true);
            _timeText.enabled = true;
            _actionIcon.enabled = false;

            SetRing(countdown.Fill01, GetColor(countdown.Type));

            int seconds = Mathf.Max(0, Mathf.CeilToInt((float)countdown.RemainingTime));

            if (_shownSeconds == seconds)
                return;

            _shownSeconds = seconds;
            _timeText.text = seconds.ToString();
        }

        private void ShowAction(CharacterActionDefinition action)
        {
            _viewRoot.SetActive(true);
            _timeText.enabled = false;
            _actionIcon.enabled = true;

            float duration = _controller.CurrentPhaseDuration;
            double remainingTime = _controller.CurrentPhaseRemainingTime;
            float fill = duration <= 0f ? 0f : (float)(remainingTime / duration);

            CharacterCountdownType type = _controller.CurrentPhase.CountdownType;
            SetRing(fill, GetColor(type));

            if (_shownIcon == action.Icon)
                return;

            _shownIcon = action.Icon;
            _actionIcon.sprite = action.Icon;
        }

        private void SetRing(float fill, Color color)
        {
            _ringFill.color = color;
            _ringFill.GetPropertyBlock(_ringProperties);
            _ringProperties.SetFloat(FillAmountId, Mathf.Clamp01(fill));
            _ringFill.SetPropertyBlock(_ringProperties);
        }

        private Color GetColor(CharacterCountdownType type)
        {
            return type == CharacterCountdownType.Positive ? _positiveColor : _negativeColor;
        }
    }
}