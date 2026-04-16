using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverEffect : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Image _targetImage;
    [SerializeField] private Color _normalColor = new Color(0.15f, 0.15f, 0.15f, 0.8f);
    [SerializeField] private Color _hoverColor = new Color(0.25f, 0.25f, 0.25f, 0.9f);
    [SerializeField] private Color _pressColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);
    [SerializeField] private float _transitionDuration = 0.15f;
    [SerializeField] private float _pressScale = 0.95f;

    private Color _targetColor;
    private Color _currentColor;
    private Vector3 _originalScale;
    private Vector3 _targetScale;
    private bool _isPointerInside;
    private bool _isPointerDown;

    private void Awake()
    {
        if (_targetImage == null)
            _targetImage = GetComponent<Image>();

        _originalScale = transform.localScale;
        _currentColor = _normalColor;
        _targetColor = _normalColor;
        _targetScale = _originalScale;
    }

    private void OnEnable()
    {
        _currentColor = _normalColor;
        _targetColor = _normalColor;
        _targetScale = _originalScale;
        _isPointerInside = false;
        _isPointerDown = false;
        ApplyImmediate();
    }

    private void Update()
    {
        if (_targetImage == null) return;

        float t = 1f - Mathf.Pow(0.001f, Time.unscaledDeltaTime / _transitionDuration);
        _currentColor = Color.Lerp(_currentColor, _targetColor, t);
        _targetImage.color = _currentColor;

        transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, t);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isPointerInside = true;
        UpdateVisualState();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isPointerInside = false;
        _isPointerDown = false;
        UpdateVisualState();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isPointerDown = true;
        UpdateVisualState();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isPointerDown = false;
        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        if (_isPointerDown && _isPointerInside)
        {
            _targetColor = _pressColor;
            _targetScale = _originalScale * _pressScale;
        }
        else if (_isPointerInside)
        {
            _targetColor = _hoverColor;
            _targetScale = _originalScale;
        }
        else
        {
            _targetColor = _normalColor;
            _targetScale = _originalScale;
        }
    }

    public void ApplyTheme(UITheme theme)
    {
        if (theme == null) return;
        _normalColor = theme.buttonNormal;
        _hoverColor = theme.buttonHover;
        _pressColor = theme.buttonActive;
        UpdateVisualState();
    }

    private void ApplyImmediate()
    {
        if (_targetImage != null)
            _targetImage.color = _normalColor;
        transform.localScale = _originalScale;
    }
}
