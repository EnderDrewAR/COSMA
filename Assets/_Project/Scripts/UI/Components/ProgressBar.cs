using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProgressBar : MonoBehaviour
{
    [SerializeField] private Image _fillImage;
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private TextMeshProUGUI _valueText;
    [SerializeField] private TextMeshProUGUI _labelText;

    [Header("Settings")]
    [SerializeField] private Color _fillColor = new Color(0f, 0.66f, 0.91f, 1f);
    [SerializeField] private Color _backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.6f);
    [SerializeField] private float _smoothSpeed = 5f;
    [SerializeField] private string _valueFormat = "{0:P0}";

    private float _targetValue;
    private float _currentValue;

    public float Value
    {
        get => _targetValue;
        set => SetValue(value);
    }

    private void Awake()
    {
        if (_fillImage != null)
            _fillImage.type = Image.Type.Filled;

        if (_backgroundImage != null)
            _backgroundImage.color = _backgroundColor;
    }

    private void Update()
    {
        if (Mathf.Abs(_currentValue - _targetValue) > 0.001f)
        {
            _currentValue = Mathf.Lerp(_currentValue, _targetValue, Time.unscaledDeltaTime * _smoothSpeed);
            ApplyFill();
        }
    }

    public void SetValue(float normalized)
    {
        _targetValue = Mathf.Clamp01(normalized);
    }

    public void SetValueImmediate(float normalized)
    {
        _targetValue = Mathf.Clamp01(normalized);
        _currentValue = _targetValue;
        ApplyFill();
    }

    public void SetLabel(string text)
    {
        if (_labelText != null)
            _labelText.text = text;
    }

    public void ApplyTheme(UITheme theme, bool useOrangeAccent = false)
    {
        if (theme == null) return;
        _fillColor = useOrangeAccent ? theme.accentOrange : theme.accentBlue;
        _backgroundColor = new Color(
            theme.panelBackground.r * 0.8f,
            theme.panelBackground.g * 0.8f,
            theme.panelBackground.b * 0.8f,
            0.6f
        );

        if (_fillImage != null)
            _fillImage.color = _fillColor;
        if (_backgroundImage != null)
            _backgroundImage.color = _backgroundColor;
    }

    private void ApplyFill()
    {
        if (_fillImage != null)
        {
            _fillImage.fillAmount = _currentValue;
            _fillImage.color = _fillColor;
        }

        if (_valueText != null)
            _valueText.text = string.Format(_valueFormat, _currentValue);
    }
}
