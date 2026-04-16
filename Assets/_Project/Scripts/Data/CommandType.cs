/// <summary>
/// All command types available in the satellite programming language.
/// Each type maps to exactly one CommandDefinition ScriptableObject.
/// </summary>
public enum CommandType
{
    PowerOn          = 0,
    PowerOff         = 1,
    ReadSunSensors   = 2,
    ReadMagnetometer = 3,
    FaceEarth        = 4,
    FaceSun          = 5,
    JumpTo           = 6,   // has int param: target line index (1-based)
    TakeEarthPhoto   = 7,
}
