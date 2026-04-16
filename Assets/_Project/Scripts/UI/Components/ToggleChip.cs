using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class ToggleChip : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private TextMeshProUGUI _label;

    [Header("Active State")]
    [SerializeField] private Color _activeBackground = new Color(1f, 0.42f, 0.21f, 1f);
    [SerializeField] private Color _activeTextColor = Color.white;

    [Header("Inactive State")]
    [SerializeField] private Color _inactiveBackground = new Color(0.15f, 0.15f, 0.15f, 0.5f);
    [SerializeField] private Color _inactiveTextColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    [Header("Settings")]
    [SerializeField] private bool _isActive;
    [SerializeField] private float _transitionDuration = 0.15f;

    [Header("Events")]
    public UnityEvent<bool> OnValueChanged;

    private Color _targetBgColor;
    private Color _targetTextColor;
    private Button _button;

    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (_isActive == value) return;
            _isActive = value;
            UpdateTargetColors();
            OnValueChanged?.Invoke(_isActive);
        }
    }

    private void Awake()
    {
        _button = GetComponent<Button>();
        if (_button == null)
            _button = gameObject.AddComponent<Button>();

        _button.transition = Selectable.Transition.None;
        _button.onClick.AddListener(ToggleState);

        UpdateTargetColors();
        ApplyImmediate();
    }

    private void Update()
    {
        if (_backgroundImage == null) return;

        float t = 1f - Mathf.Pow(0.001f, Time.unscaledDeltaTime / _transitionDuration);
        _backgroundImage.color = Color.Lerp(_backgroundImage.color, _targetBgColor, t);

        if (_label != null)
            _label.color = Color.Lerp(_label.color, _targetTextColor, t);
    }

    public void ToggleState()
    {
        IsActive = !_isActive;
    }

    public void SetLabel(string text)
    {
        if (_label != null)
            _label.text = text;
    }

    public void ApplyTheme(UITheme theme, bool useBlueAccent = false)
    {
        if (theme == null) return;
        _activeBackground = useBlueAccent ? theme.accentBlue : theme.accentOrange;
        _activeTextColor = theme.textPrimary;
        _inactiveBackground = theme.buttonNormal;
        _inactiveTextColor = theme.textMuted;
        UpdateTargetColors();
    }

    private void UpdateTargetColors()
    {
        _targetBgColor = _isActive ? _activeBackground : _inactiveBackground;
        _targetTextColor = _isActive ? _activeTextColor : _inactiveTextColor;
    }

    private void ApplyImmediate()
    {
        if (_backgroundImage != null)
            _backgroundImage.color = _isActive ? _activeBackground : _inactiveBackground;
        if (_label != null)
            _label.color = _isActive ? _activeTextColor : _inactiveTextColor;
    }
}
