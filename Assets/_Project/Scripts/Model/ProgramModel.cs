using System;
using UnityEngine;

/// <summary>
/// Central data model — holds the 13-slot program.
/// Single source of truth. No UI code here.
/// Broadcast OnChanged when any slot changes.
/// </summary>
public class ProgramModel : MonoBehaviour
{
    public const int SLOT_COUNT = 13;

    [SerializeField]
    private ProgramCommandData[] _slots = new ProgramCommandData[SLOT_COUNT];

    /// <summary>Fired whenever a slot changes. Arg = slot index 0–12.</summary>
    public event Action<int> OnSlotChanged;

    /// <summary>Fired when the whole program is cleared.</summary>
    public event Action OnCleared;

    private void Awake()
    {
        // Ensure all slots are initialised
        for (int i = 0; i < SLOT_COUNT; i++)
            if (_slots[i] == null)
                _slots[i] = new ProgramCommandData();
    }

    // ── Public API ──────────────────────────────────────────────────────────

    public ProgramCommandData GetSlot(int index)
    {
        ValidateIndex(index);
        return _slots[index];
    }

    public void SetCommand(int index, CommandDefinition def, int param = 0)
    {
        ValidateIndex(index);
        _slots[index] = new ProgramCommandData(def, param);
        OnSlotChanged?.Invoke(index);
    }

    public void ClearSlot(int index)
    {
        ValidateIndex(index);
        _slots[index] = new ProgramCommandData();
        OnSlotChanged?.Invoke(index);
    }

    public void ClearAll()
    {
        for (int i = 0; i < SLOT_COUNT; i++)
            _slots[i] = new ProgramCommandData();
        OnCleared?.Invoke();
    }

    public bool IsEmpty(int index)
    {
        ValidateIndex(index);
        return _slots[index].IsEmpty;
    }

    private void ValidateIndex(int i)
    {
        if (i < 0 || i >= SLOT_COUNT)
            throw new ArgumentOutOfRangeException(nameof(i),
                $"Slot index must be 0–{SLOT_COUNT - 1}, got {i}");
    }
}
