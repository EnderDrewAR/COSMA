using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Visual-only drag preview. Lives in DragGhostLayer (top Canvas layer).
/// Created by DragDropController.BeginDrag. Never placed in ProgramLine.
/// Destroyed on drag end.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class DragGhostView : MonoBehaviour
{
    [SerializeField] private Image            _background;
    [SerializeField] private Image            _accentBar;
    [SerializeField] private TextMeshProUGUI  _label;

    private Canvas    _rootCanvas;
    private RectTransform _rt;
    private CanvasGroup   _cg;

    // ── Factory ──────────────────────────────────────────────────────────────

    public static DragGhostView Create(CommandDefinition def, Transform parent)
    {
        // Build ghost GameObject
        var go  = new GameObject("DragGhost", typeof(RectTransform),
                                              typeof(CanvasRenderer),
                                              typeof(Image),
                                              typeof(CanvasGroup));
        go.transform.SetParent(parent, false);
        go.layer = LayerMask.NameToLayer("UI");

        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(190, 40);
        rt.pivot     = new Vector2(0.5f, 0.5f);

        var bg = go.GetComponent<Image>();
        bg.color = new Color(0.12f, 0.12f, 0.18f, 0.96f);

        // Accent bar (left edge)
        var barGO  = new GameObject("Bar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        barGO.transform.SetParent(go.transform, false);
        barGO.layer = LayerMask.NameToLayer("UI");
        var barRT = barGO.GetComponent<RectTransform>();
        barRT.anchorMin = new Vector2(0, 0);
        barRT.anchorMax = new Vector2(0, 1);
        barRT.pivot     = new Vector2(0, 0.5f);
        barRT.anchoredPosition = Vector2.zero;
        barRT.sizeDelta = new Vector2(4, 0);
        var barImg = barGO.GetComponent<Image>();
        barImg.color = def.accentColor;

        // Label
        var lblGO  = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        lblGO.transform.SetParent(go.transform, false);
        lblGO.layer = LayerMask.NameToLayer("UI");
        var lblRT = lblGO.GetComponent<RectTransform>();
        lblRT.anchorMin = new Vector2(0, 0);
        lblRT.anchorMax = new Vector2(1, 1);
        lblRT.pivot     = new Vector2(0.5f, 0.5f);
        lblRT.offsetMin = new Vector2(14, 0);
        lblRT.offsetMax = new Vector2(-4, 0);
        var lbl = lblGO.GetComponent<TextMeshProUGUI>();
        lbl.text      = def.displayName;
        lbl.fontSize  = 13;
        lbl.color     = Color.white;
        lbl.alignment = TextAlignmentOptions.MidlineLeft;
        lbl.enableWordWrapping = false;

        var view = go.AddComponent<DragGhostView>();
        view._background = bg;
        view._accentBar  = barImg;
        view._label      = lbl;
        view._rt         = rt;
        view._cg         = go.GetComponent<CanvasGroup>();

        // Ghost doesn't block raycasts (slots must receive them)
        view._cg.blocksRaycasts = false;
        view._cg.alpha          = 0.88f;

        // Root canvas for screen→canvas conversion
        var canvas = parent.GetComponentInParent<Canvas>();
        if (canvas != null)
            view._rootCanvas = canvas.rootCanvas;

        // Entrance animation
        view.StartCoroutine(view.ScaleIn());

        return view;
    }

    // ── Per-frame position update ─────────────────────────────────────────────

    public void UpdatePosition(Vector2 screenPos)
    {
        if (_rt == null || _rootCanvas == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rootCanvas.GetComponent<RectTransform>(),
            screenPos,
            _rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _rootCanvas.worldCamera,
            out Vector2 localPoint);

        _rt.anchoredPosition = localPoint;
    }

    // ── Destroy ──────────────────────────────────────────────────────────────

    public void SelfDestroy()
    {
        StartCoroutine(ScaleOutAndDestroy());
    }

    // ── Animations (coroutine-based, no DOTween required) ────────────────────

    private IEnumerator ScaleIn()
    {
        transform.localScale = Vector3.one * 0.7f;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / 0.1f;
            float s = Mathf.Lerp(0.7f, 1f, EaseOut(t));
            transform.localScale = Vector3.one * s;
            yield return null;
        }
        transform.localScale = Vector3.one;
    }

    private IEnumerator ScaleOutAndDestroy()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / 0.08f;
            float s = Mathf.Lerp(1f, 0.6f, EaseIn(t));
            if (_cg != null) _cg.alpha = Mathf.Lerp(0.88f, 0f, EaseIn(t));
            transform.localScale = Vector3.one * s;
            yield return null;
        }
        Destroy(gameObject);
    }

    private static float EaseOut(float t) => 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f);
    private static float EaseIn(float t)  => Mathf.Pow(Mathf.Clamp01(t), 2f);
}
