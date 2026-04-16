using System;
using UnityEngine;

/// <summary>
/// Singleton — owns the drag state.
/// CommandPoolItemView calls BeginDrag/UpdateDrag/EndDrag.
/// ProgramLineView calls RegisterDrop when drop succeeds.
/// NOBODY moves the original pool item.
/// </summary>
public class DragDropController : MonoBehaviour
{
    public static DragDropController Instance { get; private set; }

    // ── State ────────────────────────────────────────────────────────────────
    public bool              IsDragging        { get; private set; }
    public CommandDefinition DraggedDefinition { get; private set; }
    public DragGhostView     CurrentGhost      { get; private set; }

    // ── Events ───────────────────────────────────────────────────────────────
    public event Action<CommandDefinition> OnDragStarted;
    public event Action<bool>              OnDragEnded;   // true = dropped on slot

    [Header("References")]
    [SerializeField] private Transform _ghostLayer;   // DragGhostLayer in Canvas

    // ── Unity lifecycle ──────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ── Public API ───────────────────────────────────────────────────────────

    public void BeginDrag(CommandDefinition def, Vector2 screenPos)
    {
        if (IsDragging) EndDrag(false);

        IsDragging        = true;
        DraggedDefinition = def;

        // Create ghost in DragGhostLayer (above all other UI)
        Transform layer = _ghostLayer != null
            ? _ghostLayer
            : FindGhostLayer();

        CurrentGhost = DragGhostView.Create(def, layer);
        CurrentGhost.UpdatePosition(screenPos);

        OnDragStarted?.Invoke(def);
    }

    public void UpdateDrag(Vector2 screenPos)
    {
        if (!IsDragging) return;
        CurrentGhost?.UpdatePosition(screenPos);
    }

    public void EndDrag(bool droppedOnSlot)
    {
        if (!IsDragging) return;

        IsDragging        = false;
        DraggedDefinition = null;

        CurrentGhost?.SelfDestroy();
        CurrentGhost = null;

        OnDragEnded?.Invoke(droppedOnSlot);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private Transform FindGhostLayer()
    {
        var go = GameObject.Find("DragGhostLayer");
        return go != null ? go.transform : transform;
    }

    public Transform GetGhostLayer()
    {
        return _ghostLayer != null ? _ghostLayer : FindGhostLayer();
    }

    public void SetGhostLayer(Transform t) => _ghostLayer = t;
}
