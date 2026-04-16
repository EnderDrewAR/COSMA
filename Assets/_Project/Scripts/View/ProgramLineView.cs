using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

/// <summary>
/// One numbered slot in the ProgramPanel. Drop target.
/// Owns a ProgramCommandView child (shows/hides based on data).
/// On drop: updates ProgramModel → refreshes visuals.
/// </summary>
public class ProgramLineView : MonoBehaviour,
    IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Identity")]
    public int lineIndex;   // 0-based (line 01 = index 0)

    [Header("Child References (wired by SceneBuilder)")]
    [SerializeField] private TextMeshProUGUI  _numberLabel;
    [SerializeField] private GameObject       _emptyState;       // dash shown when empty
    [SerializeField] private ProgramCommandView _commandView;    // shown when filled
    [SerializeField] private Image            _highlightImage;   // hover glow
    [SerializeField] private Image            _background;

    [Header("Colors")]
    [SerializeField] private Color _normalColor    = new Color(0.09f, 0.09f, 0.13f, 1f);
    [SerializeField] private Color _highlightColor = new Color(0.15f, 0.35f, 0.60f, 0.35f);

    private ProgramModel _model;
    private Coroutine    _highlightCoroutine;

    // ── Lifecycle ────────────────────────────────────────────────────────────

    private void Awake()
    {
        _model = FindObjectOfType<ProgramModel>();
        if (_model == null)
            Debug.LogWarning("[ProgramLineView] ProgramModel not found in scene.");
    }

    private void Start()
    {
        SetupNumber();
        Refresh();

        if (_model != null)
            _model.OnSlotChanged += OnModelSlotChanged;
    }

    private void OnDestroy()
    {
        if (_model != null)
            _model.OnSlotChanged -= OnModelSlotChanged;
    }

    // ── Drop handling ────────────────────────────────────────────────────────

    public void OnDrop(PointerEventData eventData)
    {
        var dc = DragDropController.Instance;
        if (dc == null || !dc.IsDragging) return;

        // Write to model
        _model?.SetCommand(lineIndex, dc.DraggedDefinition);

        // Notify controller: drop succeeded
        dc.EndDrag(true);

        SetHighlight(false, immediate: true);
    }

    // ── Hover feedback during drag ────────────────────────────────────────────

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (DragDropController.Instance != null && DragDropController.Instance.IsDragging)
            SetHighlight(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetHighlight(false);
    }

    // ── Refresh from model ────────────────────────────────────────────────────

    public void Refresh()
    {
        if (_model == null) return;
        var data = _model.GetSlot(lineIndex);
        UpdateVisual(data);
    }

    private void OnModelSlotChanged(int index)
    {
        if (index == lineIndex) Refresh();
    }

    private void UpdateVisual(ProgramCommandData data)
    {
        bool hasCmd = data != null && !data.IsEmpty;

        if (_emptyState != null)
            _emptyState.SetActive(!hasCmd);

        if (_commandView != null)
        {
            if (hasCmd)
                _commandView.Show(data.definition, data.lineParam);
            else
                _commandView.Hide();
        }
    }

    // ── Highlight ────────────────────────────────────────────────────────────

    private void SetHighlight(bool on, bool immediate = false)
    {
        if (_highlightImage == null) return;

        if (_highlightCoroutine != null) StopCoroutine(_highlightCoroutine);
        _highlightCoroutine = StartCoroutine(AnimateHighlight(on, immediate));
    }

    private IEnumerator AnimateHighlight(bool on, bool immediate)
    {
        Color start  = _highlightImage.color;
        Color target = on ? _highlightColor : new Color(_highlightColor.r, _highlightColor.g, _highlightColor.b, 0f);

        if (immediate) { _highlightImage.color = target; yield break; }

        float t = 0f;
        float dur = 0.12f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / dur;
            _highlightImage.color = Color.Lerp(start, target, Mathf.Clamp01(t));
            yield return null;
        }
        _highlightImage.color = target;
    }

    // ── Execution highlight (driven by UIController) ──────────────────────────

    private static readonly Color _execColor = new Color(1f, 0.55f, 0.10f, 0.30f);

    /// <summary>
    /// Orange glow while this line is being executed.
    /// Called by UIController.HandleLineExecuted.
    /// </summary>
    public void SetExecutionHighlight(bool on)
    {
        if (_highlightImage == null) return;
        if (_highlightCoroutine != null) StopCoroutine(_highlightCoroutine);
        Color target = on
            ? _execColor
            : new Color(_execColor.r, _execColor.g, _execColor.b, 0f);
        _highlightCoroutine = StartCoroutine(AnimateToColor(_highlightImage.color, target, 0.15f));
    }

    private IEnumerator AnimateToColor(Color from, Color to, float dur)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / dur;
            if (_highlightImage != null)
                _highlightImage.color = Color.Lerp(from, to, Mathf.Clamp01(t));
            yield return null;
        }
        if (_highlightImage != null) _highlightImage.color = to;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void SetupNumber()
    {
        if (_numberLabel != null)
            _numberLabel.text = (lineIndex + 1).ToString("D2");
    }

    // Called by SceneBuilder to wire all child references
    public void WireReferences(TextMeshProUGUI num, GameObject emptyState,
        ProgramCommandView cmdView, Image highlight, Image bg)
    {
        _numberLabel    = num;
        _emptyState     = emptyState;
        _commandView    = cmdView;
        _highlightImage = highlight;
        _background     = bg;
    }
}
