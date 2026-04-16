using System;
using UnityEngine;

/// <summary>
/// Runtime state of the satellite. Changed only by ProgramExecutor.
/// UI/other systems listen to OnStateChanged.
/// </summary>
public class SatelliteState : MonoBehaviour
{
    [Header("Current State")]
    public bool powerOn;
    public bool hasSunData;
    public bool hasEarthData;
    public bool hasMagData;
    public bool facingSun;
    public bool facingEarth;
    public bool photoTaken;

    public event Action OnStateChanged;

    // ── Command execution ────────────────────────────────────────────────────

    /// <summary>Apply one command. Returns false if precondition fails.</summary>
    public bool ExecuteCommand(ProgramCommandData cmd, out string message)
    {
        message = "";
        if (cmd == null || cmd.IsEmpty) { message = "Empty slot"; return false; }

        switch (cmd.definition.type)
        {
            case CommandType.PowerOn:
                powerOn = true;
                message = "Power ON";
                break;

            case CommandType.PowerOff:
                powerOn = false;
                message = "Power OFF";
                break;

            case CommandType.ReadSunSensors:
                if (!powerOn) { message = "No power"; return false; }
                hasSunData = true;
                message = "Sun sensors: read";
                break;

            case CommandType.ReadMagnetometer:
                if (!powerOn) { message = "No power"; return false; }
                hasMagData = true;
                message = "Magnetometer: read";
                break;

            case CommandType.FaceEarth:
                if (!powerOn) { message = "No power"; return false; }
                facingEarth = true;
                facingSun   = false;
                message = "Facing Earth";
                break;

            case CommandType.FaceSun:
                if (!powerOn) { message = "No power"; return false; }
                facingSun   = true;
                facingEarth = false;
                message = "Facing Sun";
                break;

            case CommandType.TakeEarthPhoto:
                if (!powerOn)      { message = "No power"; return false; }
                if (!facingEarth)  { message = "Not facing Earth"; return false; }
                photoTaken   = true;
                hasEarthData = true;
                message = "Photo taken";
                break;

            case CommandType.JumpTo:
                // Handled by executor; just return true
                message = $"Jump → {cmd.lineParam}";
                break;
        }

        OnStateChanged?.Invoke();
        return true;
    }

    public void Reset()
    {
        powerOn = hasSunData = hasEarthData = hasMagData =
            facingSun = facingEarth = photoTaken = false;
        OnStateChanged?.Invoke();
    }
}
