using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using System.Collections;

public class CollapsibleSection : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform _headerBar;
    [SerializeField] private RectTransform _contentContainer;
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private RectTransform _arrowIcon;

    [Header("Settings")]
    [SerializeField] private bool _startExpanded = true;
    [SerializeField] private float _animationDuration = 0.25f;

    [Header("Events")]
    public UnityEvent<bool> OnToggled;

    private bool _isExpanded;
    private LayoutElement _layoutElement;
    private float _expandedHeight;
    private Coroutine _animationCoroutine;

    public bool IsExpanded => _isExpanded;

    private void Awake()
    {
        _layoutElement = _contentContainer.GetComponent<LayoutElement>();
        if (_layoutElement == null)
            _layoutElement = _contentContainer.gameObject.AddComponent<LayoutElement>();
    }

    private void Start()
    {
        _isExpanded = _startExpanded;

        // Cache expanded height
        _expandedHeight = LayoutUtility.GetPreferredHeight(_contentContainer);

        // Set initial state without animation
        _contentContainer.gameObject.SetActive(_isExpanded);
        UpdateArrow(_isExpanded);

        // Wire header click
        Button headerButton = _headerBar.GetComponent<Button>();
        if (headerButton == null)
            headerButton = _headerBar.gameObject.AddComponent<Button>();

        headerButton.transition = Selectable.Transition.None;
        headerButton.onClick.AddListener(Toggle);
    }

    public void Toggle()
    {
        SetExpanded(!_isExpanded);
    }

    public void SetExpanded(bool expanded)
    {
        if (_isExpanded == expanded) return;
        _isExpanded = expanded;

        if (_animationCoroutine != null)
            StopCoroutine(_animationCoroutine);

        _animationCoroutine = StartCoroutine(AnimateToggle(expanded));

        OnToggled?.Invoke(_isExpanded);
    }

    public void SetTitle(string title)
    {
        if (_titleText != null)
            _titleText.text = title;
    }

    private IEnumerator AnimateToggle(bool expanding)
    {
        // Recalculate expanded height in case content changed
        if (expanding)
        {
            _contentContainer.gameObject.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_contentContainer);
            _expandedHeight = LayoutUtility.GetPreferredHeight(_contentContainer);
        }

        float elapsed = 0f;
        float startHeight = expanding ? 0f : _expandedHeight;
        float endHeight = expanding ? _expandedHeight : 0f;

        while (elapsed < _animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / _animationDuration);
            float currentHeight = Mathf.Lerp(startHeight, endHeight, t);

            _layoutElement.preferredHeight = currentHeight;
            UpdateArrow(t > 0.5f ? expanding : !expanding);

            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
            yield return null;
        }

        _layoutElement.preferredHeight = endHeight;

        if (!expanding)
            _contentContainer.gameObject.SetActive(false);

        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        _animationCoroutine = null;
    }

    private void UpdateArrow(bool expanded)
    {
        if (_arrowIcon == null) return;
        float targetZ = expanded ? 0f : -90f;
        _arrowIcon.localRotation = Quaternion.Euler(0f, 0f, targetZ);
    }
}
