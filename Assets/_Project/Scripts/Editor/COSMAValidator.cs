#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// COSMA → Validate Wiring
/// ─────────────────────────────────────────────────────────────────────────
/// Checks every serialized reference needed for the program to run correctly.
/// Run this whenever something behaves unexpectedly.
/// Green = OK.  Yellow = warning (may still work).  Red = will break at runtime.
/// </summary>
public static class COSMAValidator
{
    [MenuItem("COSMA/Validate Wiring")]
    public static void ValidateWiring()
    {
        int errors   = 0;
        int warnings = 0;

        Debug.Log("──────────── COSMA Wiring Validation ────────────");

        // ── ProgramModel ──────────────────────────────────────────────────────
        var model = Object.FindObjectOfType<ProgramModel>();
        if (model == null)
        {
            LogError("ProgramModel not found in scene. " +
                     "Run COSMA → Rebuild UI Scene.");
            errors++;
        }
        else
        {
            LogOk($"ProgramModel found: '{model.name}'  ({ProgramModel.SLOT_COUNT} slots)");
        }

        // ── SatelliteState ────────────────────────────────────────────────────
        var sat = Object.FindObjectOfType<SatelliteState>();
        if (sat == null)
        {
            LogError("SatelliteState not found in scene. " +
                     "Run COSMA → Rebuild UI Scene.");
            errors++;
        }
        else
        {
            LogOk($"SatelliteState found: '{sat.name}'");
        }

        // ── DragDropController ────────────────────────────────────────────────
        var ddc = Object.FindObjectOfType<DragDropController>();
        if (ddc == null)
        {
            LogError("DragDropController not found in scene.");
            errors++;
        }
        else
        {
            LogOk($"DragDropController found: '{ddc.name}'");
            if (ddc.GetGhostLayer() == null)
                LogWarn("DragDropController.GhostLayer is null — " +
                        "drag ghosts will fall back to scene-search (usually fine).",
                        ref warnings);
        }

        // ── ProgramExecutor ───────────────────────────────────────────────────
        var executor = Object.FindObjectOfType<ProgramExecutor>();
        if (executor == null)
        {
            LogError("ProgramExecutor not found in scene.");
            errors++;
        }
        else
        {
            LogOk($"ProgramExecutor found: '{executor.name}'");

            if (executor.model == null)
            {
                LogError("ProgramExecutor.model is null → program cannot read slot data.");
                errors++;
            }
            else
            {
                LogOk("  ProgramExecutor.model assigned");
            }

            if (executor.satellite == null)
            {
                LogError("ProgramExecutor.satellite is null → " +
                         "commands execute but satellite state never changes.");
                errors++;
            }
            else
            {
                LogOk("  ProgramExecutor.satellite assigned");
            }
        }

        // ── UIController ──────────────────────────────────────────────────────
        var ui = Object.FindObjectOfType<UIController>();
        if (ui == null)
        {
            LogError("UIController not found in scene.");
            errors++;
        }
        else
        {
            LogOk($"UIController found: '{ui.name}'");

            if (ui.executor == null)
            {
                LogError("UIController.executor is null → " +
                         "Run/Step/Stop buttons will do nothing.");
                errors++;
            }

            if (ui.stopBtn      == null) LogWarn("UIController.stopBtn is null.", ref warnings);
            if (ui.runBtn       == null) LogWarn("UIController.runBtn is null.", ref warnings);
            if (ui.stepBtn      == null) LogWarn("UIController.stepBtn is null.", ref warnings);
            if (ui.messagePanel == null) LogWarn("UIController.messagePanel is null — feedback messages won't show.", ref warnings);

            int lineCount = ui.programLines?.Length ?? 0;
            if (lineCount != ProgramModel.SLOT_COUNT)
            {
                LogError($"UIController.programLines has {lineCount} entries, " +
                         $"expected {ProgramModel.SLOT_COUNT}. " +
                         "Execution highlights won't work.");
                errors++;
            }
            else
            {
                int nullLines = 0;
                for (int i = 0; i < lineCount; i++)
                    if (ui.programLines[i] == null) nullLines++;

                if (nullLines > 0)
                {
                    LogError($"UIController.programLines has {nullLines} null entries " +
                             "(lines without a ProgramLineView assigned).");
                    errors++;
                }
                else
                {
                    LogOk($"  UIController.programLines: all {lineCount} entries assigned");
                }
            }
        }

        // ── ProgramLineViews ──────────────────────────────────────────────────
        var lineViews = Object.FindObjectsByType<ProgramLineView>(FindObjectsSortMode.None);
        if (lineViews.Length != ProgramModel.SLOT_COUNT)
        {
            LogError($"Found {lineViews.Length} ProgramLineViews in scene, " +
                     $"expected {ProgramModel.SLOT_COUNT}. " +
                     "Run COSMA → Rebuild UI Scene.");
            errors++;
        }
        else
        {
            LogOk($"ProgramLineView count: {lineViews.Length} ✓");
        }

        // ── CommandPoolItemViews ──────────────────────────────────────────────
        var poolItems = Object.FindObjectsByType<CommandPoolItemView>(FindObjectsSortMode.None);
        LogOk($"CommandPoolItemViews found: {poolItems.Length}");

        int nullDefs = 0;
        foreach (var item in poolItems)
            if (item.definition == null) nullDefs++;

        if (nullDefs > 0)
        {
            LogError($"{nullDefs} CommandPoolItemView(s) have a null CommandDefinition — " +
                     "drag-and-drop will fail for those tiles.");
            errors++;
        }

        // ── CommandDefinition assets ──────────────────────────────────────────
        int cmdTypeCount = System.Enum.GetValues(typeof(CommandType)).Length;
        string[] guids = AssetDatabase.FindAssets("t:CommandDefinition",
                                                   new[] { "Assets/_Project/Data/Commands" });
        if (guids.Length < cmdTypeCount)
        {
            LogWarn($"Only {guids.Length}/{cmdTypeCount} CommandDefinition assets found in " +
                    "Assets/_Project/Data/Commands. " +
                    "Run COSMA → Rebuild UI Scene to create them.",
                    ref warnings);
        }
        else
        {
            LogOk($"CommandDefinition assets: {guids.Length}/{cmdTypeCount} ✓");
        }

        // ── Summary ───────────────────────────────────────────────────────────
        Debug.Log("─────────────────────────────────────────────────");

        if (errors == 0 && warnings == 0)
        {
            Debug.Log("[COSMA] ✓ All references valid — no issues found.");
            EditorUtility.DisplayDialog("COSMA Validator",
                "✓  All wiring is valid!\n\nNo errors or warnings found.\nGame should run correctly.",
                "OK");
        }
        else if (errors == 0)
        {
            Debug.Log($"[COSMA] ⚠  Wiring OK with {warnings} warning(s). See Console.");
            EditorUtility.DisplayDialog("COSMA Validator",
                $"⚠  {warnings} warning(s) found.\n\nNo hard errors — game may still run.\nSee Console for details.",
                "OK");
        }
        else
        {
            Debug.LogError($"[COSMA] ✗  {errors} error(s), {warnings} warning(s). See Console.");
            EditorUtility.DisplayDialog("COSMA Validator",
                $"✗  {errors} error(s) found!\n\n" +
                "The game will not work correctly until these are fixed.\n\n" +
                "Quick fix: COSMA → Rebuild UI Scene\n\nSee Console for details.",
                "OK");
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static void LogOk(string msg)    => Debug.Log     ($"  ✓  {msg}");
    static void LogError(string msg) => Debug.LogError($"  ✗  {msg}");

    static void LogWarn(string msg, ref int warnCount)
    {
        Debug.LogWarning($"  ⚠  {msg}");
        warnCount++;
    }
}
#endif
