using UnityEngine;

/// <summary>
/// ScriptableObject — immutable template for one command type.
/// Lives in Assets/_Project/ScriptableObjects/Commands/
/// ONE asset per CommandType. Never modified at runtime.
/// </summary>
[CreateAssetMenu(fileName = "Cmd_New", menuName = "COSMA/Command Definition")]
public class CommandDefinition : ScriptableObject
{
    [Header("Identity")]
    public CommandType type;
    public string      displayName;
    [TextArea(2, 4)]
    public string      description;

    [Header("Visual")]
    public Color  accentColor = Color.cyan;
    public Sprite icon;

    [Header("Behaviour")]
    public bool hasLineParam;   // true for JumpTo (user picks a target line)
}
