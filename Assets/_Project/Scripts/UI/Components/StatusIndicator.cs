using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public enum StatusType
{
    Online,
    Offline,
    Warning
}

public class StatusIndicator : MonoBehaviour
{
    [SerializeField] private Image _dot;
    [SerializeField] private TextMeshProUGUI _statusText;
    [SerializeField] private StatusType _currentStatus = StatusType.Offline;
    [SerializeField] private bool _enablePulse = true;
    [SerializeField] private float _pulseSpeed = 2f;

    private UITheme _theme;
    private Coroutine _pulseCoroutine;

    public StatusType CurrentStatus => _currentStatus;

    private void OnEnable()
    {
        UpdateVisual();
    }

    private void OnDisable()
    {
        StopPulse();
    }

    public void SetStatus(StatusType status)
    {
        _currentStatus = status;
        UpdateVisual();
    }

    public void SetTheme(UITheme theme)
    {
        _theme = theme;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        Color dotColor;
        string text;

        switch (_currentStatus)
        {
            case StatusType.Online:
                dotColor = _theme != null ? _theme.statusOnline : new Color(0.2f, 0.9f, 0.4f, 1f);
                text = "ONLINE";
                break;
            case StatusType.Warning:
                dotColor = _theme != null ? _theme.statusWarning : new Color(1f, 0.8f, 0f, 1f);
                text = "WARNING";
                break;
            default:
                dotColor = _theme != null ? _theme.statusOffline : new Color(0.9f, 0.2f, 0.2f, 1f);
                text = "OFFLINE";
                break;
        }

        if (_dot != null)
            _dot.color = dotColor;

        if (_statusText != null)
        {
            _statusText.text = text;
            _statusText.color = dotColor;
        }

        StopPulse();
        if (_enablePulse && _currentStatus == StatusType.Online && isActiveAndEnabled)
            _pulseCoroutine = StartCoroutine(PulseAnimation());
    }

    private IEnumerator PulseAnimation()
    {
        if (_dot == null) yield break;

        Color baseColor = _dot.color;

        while (true)
        {
            float t = (Mathf.Sin(Time.unscaledTime * _pulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
            float alpha = Mathf.Lerp(0.4f, 1f, t);
            _dot.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            yield return null;
        }
    }

    private void StopPulse()
    {
        if (_pulseCoroutine != null)
        {
            StopCoroutine(_pulseCoroutine);
            _pulseCoroutine = null;
        }
    }
}
