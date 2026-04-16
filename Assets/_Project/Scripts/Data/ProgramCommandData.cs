using System;
using UnityEngine;

/// <summary>
/// One command instance placed in a ProgramLine slot.
/// Pure data — no MonoBehaviour, no UI.
/// Created when player drops a CommandPoolItem onto a ProgramLineView.
/// </summary>
[Serializable]
public class ProgramCommandData
{
    public CommandDefinition definition;
    public int               lineParam;     // used by JumpTo

    public bool IsEmpty => definition == null;

    public ProgramCommandData() { }

    public ProgramCommandData(CommandDefinition def, int param = 0)
    {
        definition = def;
        lineParam  = param;
    }

    public void Clear()
    {
        definition = null;
        lineParam  = 0;
    }

    public ProgramCommandData Clone() => new ProgramCommandData(definition, lineParam);
}
