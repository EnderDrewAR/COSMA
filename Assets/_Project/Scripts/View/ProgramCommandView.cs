using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Displays ONE command instance inside a ProgramLineView.
/// Always a child of ProgramLineView. Shown/hidden by parent.
/// NEVER dragged itself — only the ghost moves.
/// </summary>
public class ProgramCommandView : MonoBehaviour
{
    [Header("References (auto-wired by SceneBuilder)")]
    [SerializeField] private Image            _background;
    [SerializeField] private Image            _accentBar;
    [SerializeField] private TextMeshProUGUI  _label;

    private CanvasGroup _cg;

    private void Awake()
    {
        _cg = GetComponent<CanvasGroup>();
        if (_cg == null) _cg = gameObject.AddComponent<CanvasGroup>();
    }

    // ── Public API ───────────────────────────────────────────────────────────

    /// <summary>Show this command view with the given definition.</summary>
    public void Show(CommandDefinition def, int param = 0)
    {
        gameObject.SetActive(true);
        _cg.alpha = 1f;

        if (_label != null)
        {
            _label.text = def.hasLineParam
                ? $"{def.displayName}  →{param}"
                : def.displayName;
        }

        if (_accentBar != null)
            _accentBar.color = def.accentColor;

        if (_background != null)
            _background.color = new Color(0.14f, 0.14f, 0.21f, 1f);

        StopAllCoroutines();
        StartCoroutine(AppearAnim());
    }

    /// <summary>Hide this command view (slot is empty).</summary>
    public void Hide()
    {
        StopAllCoroutines();
        gameObject.SetActive(false);
        if (_cg != null) _cg.alpha = 0f;
    }

    // ── Appearance animation ─────────────────────────────────────────────────

    private IEnumerator AppearAnim()
    {
        transform.localScale = Vector3.one * 0.75f;
        _cg.alpha = 0f;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / 0.18f;
            float e = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f);
            transform.localScale = Vector3.one * Mathf.Lerp(0.75f, 1f, e);
            _cg.alpha            = Mathf.Lerp(0f, 1f, e);
            yield return null;
        }

        transform.localScale = Vector3.one;
        _cg.alpha = 1f;
    }

    // ── Editor wiring (called by SceneBuilder) ────────────────────────────────

    public void WireReferences(Image bg, Image bar, TextMeshProUGUI lbl)
    {
        _background = bg;
        _accentBar  = bar;
        _label      = lbl;
    }
}
