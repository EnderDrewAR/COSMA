using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

/// <summary>
/// A command in the pool — the TEMPLATE source.
/// Dragging it creates a ghost; the item itself NEVER moves.
/// One CommandPoolItemView per CommandDefinition in the panel.
/// </summary>
public class CommandPoolItemView : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    [Header("Data")]
    public CommandDefinition definition;

    [Header("Visual References (wired by SceneBuilder)")]
    [SerializeField] private Image           _background;
    [SerializeField] private Image           _accentBar;
    [SerializeField] private TextMeshProUGUI _label;

    [Header("Colors")]
    [SerializeField] private Color _normalBg  = new Color(0.13f, 0.13f, 0.19f, 1f);
    [SerializeField] private Color _hoverBg   = new Color(0.20f, 0.20f, 0.28f, 1f);
    [SerializeField] private Color _pressBg   = new Color(0.08f, 0.08f, 0.12f, 1f);

    private CanvasGroup _cg;

    private void Awake()
    {
        _cg = GetComponent<CanvasGroup>();
        if (_cg == null) _cg = gameObject.AddComponent<CanvasGroup>();
    }

    private void Start()
    {
        ApplyDefinition();
    }

    public void ApplyDefinition()
    {
        if (definition == null) return;
        if (_label != null)     _label.text  = definition.displayName;
        if (_accentBar != null) _accentBar.color = definition.accentColor;
        if (_background != null) _background.color = _normalBg;
    }

    // ── Drag ─────────────────────────────────────────────────────────────────

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (definition == null) return;

        // Pool item stays put — controller creates ghost
        DragDropController.Instance?.BeginDrag(definition, eventData.position);

        // Dim the source slightly so user knows dragging started
        if (_background != null)
            _background.color = new Color(_normalBg.r, _normalBg.g, _normalBg.b, 0.5f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Ghost moves, not this object
        DragDropController.Instance?.UpdateDrag(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // If not dropped on a slot, controller was already told to cancel
        DragDropController.Instance?.EndDrag(false);

        // Restore appearance
        if (_background != null)
            StartCoroutine(RestoreColor());
    }

    private IEnumerator RestoreColor()
    {
        Color start = _background.color;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / 0.2f;
            _background.color = Color.Lerp(start, _normalBg, Mathf.Clamp01(t));
            yield return null;
        }
        _background.color = _normalBg;
    }

    // ── Hover ─────────────────────────────────────────────────────────────────

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_background != null)
            StartCoroutine(AnimateBg(_background.color, _hoverBg, 0.1f));
        // Slight scale
        StartCoroutine(AnimateScale(1f, 1.03f, 0.1f));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_background != null)
            StartCoroutine(AnimateBg(_background.color, _normalBg, 0.1f));
        StartCoroutine(AnimateScale(transform.localScale.x, 1f, 0.1f));
    }

    private IEnumerator AnimateBg(Color from, Color to, float dur)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / dur;
            if (_background != null)
                _background.color = Color.Lerp(from, to, Mathf.Clamp01(t));
            yield return null;
        }
    }

    private IEnumerator AnimateScale(float from, float to, float dur)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / dur;
            transform.localScale = Vector3.one * Mathf.Lerp(from, to, Mathf.Clamp01(t));
            yield return null;
        }
        transform.localScale = Vector3.one * to;
    }

    // ── Wiring ────────────────────────────────────────────────────────────────

    public void WireReferences(Image bg, Image bar, TextMeshProUGUI lbl)
    {
        _background = bg;
        _accentBar  = bar;
        _label      = lbl;
    }
}
