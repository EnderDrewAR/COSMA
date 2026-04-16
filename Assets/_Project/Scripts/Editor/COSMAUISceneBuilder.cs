#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

/// <summary>
/// COSMA UI Scene Builder v4
/// ──────────────────────────────────────────────────────────────────────────
/// Auto-runs when SampleScene opens (once per session).
/// Manual rebuild: COSMA → Rebuild UI Scene
///
/// What it creates
/// ───────────────
/// Canvas / HUDLayer
///   MissionPanel          — top-right, static mission text
///   CommandPoolPanel      — draggable command tiles (CommandPoolItemView × 8)
///   ProgramPanel          — 13 drop-target ProgramLineView slots
///   BottomControlPanel    — Stop / Run / Step buttons
///   BottomActionPanel     — Undo / Clear action buttons
///   MessagePanel          — timed feedback text (hidden by default)
///   PopupLayer            — full-screen overlay (hidden)
///   DragGhostLayer        — top-most Canvas layer for ghost rendering
///
/// SceneManagers (scene-root GO)
///   ProgramModel          — 13-slot data model
///   SatelliteState        — satellite flag model + command executor
///   DragDropController    — singleton drag state, ghost lifecycle
///   ProgramExecutor       — run/step/pause program loop
///   UIController          — wires buttons → executor, highlights, messages
/// </summary>
[InitializeOnLoad]
public static class COSMAUISceneBuilder
{
    private const string SESSION_KEY = "COSMA_UI_BUILT_v4";

    // ── Palette ───────────────────────────────────────────────────────────────
    static readonly Color BG_PANEL    = new Color(0.06f, 0.06f, 0.09f, 0.92f);
    static readonly Color BG_HEADER   = new Color(0.10f, 0.10f, 0.15f, 1.00f);
    static readonly Color BG_ROW_A    = new Color(0.09f, 0.09f, 0.13f, 1.00f);
    static readonly Color BG_ROW_B    = new Color(0.11f, 0.11f, 0.16f, 1.00f);
    static readonly Color BG_ITEM     = new Color(0.13f, 0.13f, 0.19f, 1.00f);
    static readonly Color BG_BTN      = new Color(0.12f, 0.12f, 0.18f, 1.00f);
    static readonly Color ORANGE      = new Color(1.00f, 0.42f, 0.21f, 1.00f);
    static readonly Color BLUE        = new Color(0.00f, 0.66f, 0.91f, 1.00f);
    static readonly Color RED         = new Color(0.85f, 0.22f, 0.22f, 1.00f);
    static readonly Color GREEN       = new Color(0.22f, 0.80f, 0.42f, 1.00f);
    static readonly Color YELLOW      = new Color(0.95f, 0.85f, 0.20f, 1.00f);
    static readonly Color CYAN        = new Color(0.20f, 0.85f, 0.90f, 1.00f);
    static readonly Color PURPLE      = new Color(0.65f, 0.30f, 0.92f, 1.00f);
    static readonly Color TEXT_PRI    = Color.white;
    static readonly Color TEXT_SEC    = new Color(0.72f, 0.72f, 0.78f, 1.00f);
    static readonly Color TEXT_MUTED  = new Color(0.38f, 0.38f, 0.46f, 1.00f);

    // ── Auto-run entry points ─────────────────────────────────────────────────

    static COSMAUISceneBuilder()
    {
        EditorApplication.delayCall          += TryAutoSetup;
        EditorSceneManager.sceneOpened       += OnSceneOpened;
    }

    static void OnSceneOpened(UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode)
    {
        if (scene.name != "SampleScene") return;
        if (SessionState.GetBool(SESSION_KEY, false)) return;
        SessionState.SetBool(SESSION_KEY, true);
        BuildUI();
    }

    static void TryAutoSetup()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorSceneManager.GetActiveScene().name != "SampleScene") return;
            if (SessionState.GetBool(SESSION_KEY, false)) return;
            SessionState.SetBool(SESSION_KEY, true);
            BuildUI();
        };
    }

    // ── Main entry ────────────────────────────────────────────────────────────

    [MenuItem("COSMA/Rebuild UI Scene")]
    public static void BuildUI()
    {
        // 1. Command definition assets (ScriptableObjects)
        var defs = EnsureCommandDefinitions();

        // 2. Canvas + root HUD layer
        var canvas = FindOrCreateCanvas();
        var hud    = FindOrCreate(canvas, "HUDLayer");
        StretchFill(hud, canvas);
        CleanChildren(hud);          // remove ALL old panels — fresh start

        // 3. UI panels
        BuildMissionPanel(hud);
        BuildCommandPoolPanel(hud, defs);
        var lineViews               = BuildProgramPanel(hud);
        var (stopBtn, runBtn, stepBtn) = BuildControlPanel(hud);
        BuildActionPanel(hud);
        var (msgPanel, msgText)     = BuildMessagePanel(hud);
        EnsurePopupLayer(hud);
        var ghostLayer              = EnsureDragGhostLayer(hud);

        // 4. Manager MonoBehaviours + wiring
        SetupManagers(ghostLayer, lineViews, stopBtn, runBtn, stepBtn, msgPanel, msgText);

        // 5. Persist
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

        Debug.Log("[COSMA v4] Scene built and saved.");
        EditorUtility.DisplayDialog("COSMA v4",
            "Сцена создана!\n\n" +
            "✓  CommandPoolPanel  — 8 команд\n" +
            "✓  ProgramLine_01–13 — в Hierarchy\n" +
            "✓  SceneManagers     — подключены\n\n" +
            "Нажми Play и тащи команды в слоты.",
            "OK");
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  COMMAND DEFINITIONS  (ScriptableObject assets)
    // ══════════════════════════════════════════════════════════════════════════

    static CommandDefinition[] EnsureCommandDefinitions()
    {
        const string ROOT   = "Assets/_Project/Data";
        const string FOLDER = "Assets/_Project/Data/Commands";

        if (!AssetDatabase.IsValidFolder(ROOT))
            AssetDatabase.CreateFolder("Assets/_Project", "Data");
        if (!AssetDatabase.IsValidFolder(FOLDER))
            AssetDatabase.CreateFolder(ROOT, "Commands");

        var defs = new[]
        {
            GetOrCreateCmdDef(CommandType.PowerOn,          "Power On",       ORANGE, false),
            GetOrCreateCmdDef(CommandType.PowerOff,         "Power Off",      RED,    false),
            GetOrCreateCmdDef(CommandType.ReadSunSensors,   "Sun Sensor",     YELLOW, false),
            GetOrCreateCmdDef(CommandType.ReadMagnetometer, "Magnetometer",   CYAN,   false),
            GetOrCreateCmdDef(CommandType.FaceEarth,        "Face Earth",     BLUE,   false),
            GetOrCreateCmdDef(CommandType.FaceSun,          "Face Sun",       YELLOW, false),
            GetOrCreateCmdDef(CommandType.JumpTo,           "Jump To",        PURPLE, true),
            GetOrCreateCmdDef(CommandType.TakeEarthPhoto,   "Take Photo",     GREEN,  false),
        };

        AssetDatabase.SaveAssets();
        return defs;
    }

    static CommandDefinition GetOrCreateCmdDef(
        CommandType type, string displayName, Color accent, bool hasLineParam)
    {
        string path  = $"Assets/_Project/Data/Commands/Cmd_{type}.asset";
        var    asset = AssetDatabase.LoadAssetAtPath<CommandDefinition>(path);
        if (asset != null) return asset;

        asset              = ScriptableObject.CreateInstance<CommandDefinition>();
        asset.type         = type;
        asset.displayName  = displayName;
        asset.accentColor  = accent;
        asset.hasLineParam = hasLineParam;
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  1.  MISSION PANEL   (top-right, static text)
    // ══════════════════════════════════════════════════════════════════════════

    static void BuildMissionPanel(GameObject hud)
    {
        var p = FindOrCreate(hud, "MissionPanel");
        AnchorRT(p, new Vector2(1,1), new Vector2(1,1), new Vector2(1,1),
                 new Vector2(-18, -18), new Vector2(480, 0));
        Img(p, BG_PANEL);
        var vlg = Vlg(p, 14, 14, 12, 12, 6);
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth      = true;
        vlg.childControlHeight     = true;
        var csf = Go<ContentSizeFitter>(p);
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Header
        var hdr = FindOrCreate(p, "Header");
        LE(hdr, prefH: 20);
        Tmp(hdr, "MISSION OBJECTIVE", 11, ORANGE, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);

        // Body
        var body = FindOrCreate(p, "Body");
        LE(body, prefH: 72);
        var t = Tmp(body, "Configure all satellite subsystems in the correct order " +
                          "to establish stable orbital communications before the " +
                          "transmission window closes.",
                    10, TEXT_SEC, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        t.enableWordWrapping = true;
        t.overflowMode = TextOverflowModes.Overflow;
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  2.  COMMAND POOL PANEL   (left-of-ProgramPanel, draggable command tiles)
    // ══════════════════════════════════════════════════════════════════════════

    static void BuildCommandPoolPanel(GameObject hud, CommandDefinition[] defs)
    {
        // ProgramPanel right edge = 1920-18 = 1902, width = 330, left = 1572
        // CommandPoolPanel right = 1572-8 = 1564  →  anchoredX = -(1920-1564) = -356
        // CommandPoolPanel width = 220  →  left = 1564-220 = 1344
        var p = FindOrCreate(hud, "CommandPoolPanel");
        AnchorRT(p, new Vector2(1,1), new Vector2(1,1), new Vector2(1,1),
                 new Vector2(-356, -170), new Vector2(220, 0));
        Img(p, BG_PANEL);

        var vlg = Vlg(p, 0, 0, 0, 0, 1);
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth      = true;
        vlg.childControlHeight     = true;
        var csf = Go<ContentSizeFitter>(p);
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // ── Header ──
        var hdr = FindOrCreate(p, "PoolHeader");
        LE(hdr, prefH: 30);
        Img(hdr, BG_HEADER);
        var hdrHlg = Hlg(hdr, 0, 12, 8, 0, 0);
        hdrHlg.childForceExpandWidth  = true;
        hdrHlg.childForceExpandHeight = true;
        hdrHlg.childControlWidth      = true;
        hdrHlg.childControlHeight     = true;
        var hdrLbl = FindOrCreate(hdr, "Lbl");
        LE(hdrLbl, flexW: 1);
        Tmp(hdrLbl, "COMMANDS", 10, TEXT_MUTED, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);

        // ── Pool items ──
        foreach (var def in defs)
            BuildPoolItem(p, $"PoolItem_{def.type}", def);
    }

    /// <summary>
    /// One draggable tile in the CommandPoolPanel.
    /// Gets CommandPoolItemView wired.
    /// </summary>
    static void BuildPoolItem(GameObject parent, string goName, CommandDefinition def)
    {
        var item = FindOrCreate(parent, goName);
        LE(item, prefH: 38);
        var bg = Img(item, BG_ITEM);
        Go<CanvasGroup>(item);   // used for drag transparency

        var hlg = Hlg(item, 8, 0, 10, 0, 0);
        hlg.childForceExpandWidth  = false;
        hlg.childForceExpandHeight = true;
        hlg.childControlWidth      = true;
        hlg.childControlHeight     = true;
        hlg.childAlignment         = TextAnchor.MiddleLeft;

        // Accent bar (4 px left edge)
        var barGO  = FindOrCreate(item, "Bar");
        LE(barGO, prefW: 4);
        var barImg = Img(barGO, def.accentColor);

        // Spacer
        var spc = FindOrCreate(item, "Spc");
        LE(spc, prefW: 8);
        Img(spc, new Color(0,0,0,0)).raycastTarget = false;

        // Label
        var lblGO = FindOrCreate(item, "Lbl");
        LE(lblGO, flexW: 1);
        var lbl = Tmp(lblGO, def.displayName, 12,
                      TEXT_PRI, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);

        // Wire CommandPoolItemView
        var view = Go<CommandPoolItemView>(item);
        view.definition = def;
        view.WireReferences(bg, barImg, lbl);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  3.  PROGRAM PANEL   (right side — 13 ProgramLineViews with drop targets)
    // ══════════════════════════════════════════════════════════════════════════

    static ProgramLineView[] BuildProgramPanel(GameObject hud)
    {
        var p = FindOrCreate(hud, "ProgramPanel");
        // anchor top-right; top at 170px below canvas top, height 700px
        // bottom edge = 1080-170-700 = 210px from bottom → well above ControlPanel (76px)
        AnchorRT(p, new Vector2(1,1), new Vector2(1,1), new Vector2(1,1),
                 new Vector2(-18, -170), new Vector2(330, 700));
        Img(p, BG_PANEL);

        var vlg = Vlg(p, 0, 0, 0, 0, 0);
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth      = true;
        vlg.childControlHeight     = true;

        // ── Panel header ──
        var panelHdr = FindOrCreate(p, "ProgramHeader");
        LE(panelHdr, prefH: 30);
        Img(panelHdr, BG_HEADER);
        var phHlg = Hlg(panelHdr, 0, 12, 8, 0, 0);
        phHlg.childForceExpandWidth  = true;
        phHlg.childForceExpandHeight = true;
        phHlg.childControlWidth      = true;
        phHlg.childControlHeight     = true;
        var phLbl = FindOrCreate(panelHdr, "Lbl");
        LE(phLbl, flexW: 1);
        Tmp(phLbl, "PROGRAM", 10, TEXT_MUTED, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);

        // ── Scroll view ──
        var scroll = FindOrCreate(p, "ProgramScroll");
        LE(scroll, flexH: 1, flexW: 1);
        SetupScrollView(scroll, out GameObject content);
        content.name = "ProgramLinesContainer";

        var cVlg = Vlg(content, 0, 0, 0, 0, 0);
        cVlg.childForceExpandWidth  = true;
        cVlg.childForceExpandHeight = false;
        cVlg.childControlWidth      = true;
        cVlg.childControlHeight     = true;
        var csf = Go<ContentSizeFitter>(content);
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // ── 13 program lines ──
        var views = new ProgramLineView[ProgramModel.SLOT_COUNT];
        for (int i = 1; i <= ProgramModel.SLOT_COUNT; i++)
            views[i - 1] = BuildProgramLine(content, i);

        return views;
    }

    /// <summary>
    /// Builds one ProgramLine_XX slot and returns its ProgramLineView.
    ///
    /// Hierarchy:
    ///   ProgramLine_XX (Image bg + ProgramLineView + HLG + LE prefH=42)
    ///     Num           (TMP number, LE prefW=34)
    ///     Slot          (HLG flexW=1)
    ///       EmptyState  (TMP "—",  flexW=1 — visible when empty)
    ///       CommandView (ProgramCommandView, flexW=1 — hidden when empty)
    ///         Bar       (Image accent, LE prefW=4)
    ///         Lbl       (TMP label, LE flexW=1)
    ///     HighlightOverlay (Image α=0, ignoreLayout, stretch-fill — for glow)
    /// </summary>
    static ProgramLineView BuildProgramLine(GameObject parent, int idx)
    {
        var line = FindOrCreate(parent, $"ProgramLine_{idx:D2}");
        LE(line, prefH: 42);
        var bg = Img(line, idx % 2 == 0 ? BG_ROW_A : BG_ROW_B);

        var hlg = Hlg(line, 0, 0, 0, 0, 0);
        hlg.childForceExpandWidth  = false;
        hlg.childForceExpandHeight = true;
        hlg.childControlWidth      = true;
        hlg.childControlHeight     = true;
        hlg.childAlignment         = TextAnchor.MiddleLeft;

        // ── Number ──
        var numGO = FindOrCreate(line, "Num");
        LE(numGO, prefW: 34);
        Img(numGO, new Color(0,0,0,0)).raycastTarget = false;
        var numTmp = Tmp(numGO, idx.ToString("D2"), 11,
                         ORANGE, FontStyles.Bold, TextAlignmentOptions.Center);

        // ── Slot (host for EmptyState ↔ CommandView) ──
        var slotGO = FindOrCreate(line, "Slot");
        LE(slotGO, flexW: 1);
        Img(slotGO, new Color(0,0,0,0)).raycastTarget = false;
        var slotHlg = Hlg(slotGO, 0, 4, 6, 0, 0);
        slotHlg.childForceExpandWidth  = true;
        slotHlg.childForceExpandHeight = true;
        slotHlg.childControlWidth      = true;
        slotHlg.childControlHeight     = true;
        slotHlg.childAlignment         = TextAnchor.MiddleLeft;

        // EmptyState — TMP dash, visible when slot is empty
        var emptyGO = FindOrCreate(slotGO, "EmptyState");
        LE(emptyGO, flexW: 1);
        var emptyTmp = Tmp(emptyGO, "\u2014", 12,
                           new Color(0.28f, 0.28f, 0.36f, 1f),
                           FontStyles.Normal, TextAlignmentOptions.MidlineLeft);

        // CommandView — hidden until a command is dropped
        var cmdViewGO = FindOrCreate(slotGO, "CommandView");
        LE(cmdViewGO, flexW: 1);
        var cmdBG = Img(cmdViewGO, new Color(0.14f, 0.14f, 0.21f, 1f));
        Go<CanvasGroup>(cmdViewGO);

        var cmdHlg = Hlg(cmdViewGO, 8, 0, 8, 0, 0);
        cmdHlg.childForceExpandWidth  = false;
        cmdHlg.childForceExpandHeight = true;
        cmdHlg.childControlWidth      = true;
        cmdHlg.childControlHeight     = true;
        cmdHlg.childAlignment         = TextAnchor.MiddleLeft;

        var cmdBarGO  = FindOrCreate(cmdViewGO, "Bar");
        LE(cmdBarGO, prefW: 4);
        var cmdBarImg = Img(cmdBarGO, Color.white);   // overwritten by Show()

        var cmdLblGO  = FindOrCreate(cmdViewGO, "Lbl");
        LE(cmdLblGO, flexW: 1);
        var cmdLbl    = Tmp(cmdLblGO, "", 11,
                            TEXT_PRI, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);

        // Wire ProgramCommandView
        var cmdView = Go<ProgramCommandView>(cmdViewGO);
        cmdView.WireReferences(cmdBG, cmdBarImg, cmdLbl);
        cmdViewGO.SetActive(false);   // hidden until first drop

        // ── HighlightOverlay — must be LAST child to render on top ──
        var hlGO = FindOrCreate(line, "HighlightOverlay");
        var hlLE = Go<LayoutElement>(hlGO);
        hlLE.ignoreLayout = true;           // excluded from HLG flow
        var hlRT = Go<RectTransform>(hlGO);
        hlRT.anchorMin        = Vector2.zero;
        hlRT.anchorMax        = Vector2.one;
        hlRT.sizeDelta        = Vector2.zero;
        hlRT.anchoredPosition = Vector2.zero;
        var hlImg = Img(hlGO, new Color(0f, 0f, 0f, 0f));
        hlImg.raycastTarget = false;

        // ── Wire ProgramLineView ──
        var lineView = Go<ProgramLineView>(line);
        lineView.lineIndex = idx - 1;   // 0-based
        lineView.WireReferences(numTmp, emptyGO, cmdView, hlImg, bg);

        return lineView;
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  4.  BOTTOM CONTROL PANEL   (Stop / Run / Step)
    // ══════════════════════════════════════════════════════════════════════════

    static (Button stop, Button run, Button step) BuildControlPanel(GameObject hud)
    {
        var p = FindOrCreate(hud, "BottomControlPanel");
        AnchorRT(p, new Vector2(0,0), new Vector2(0,0), new Vector2(0,0),
                 new Vector2(18, 18), new Vector2(240, 58));
        Img(p, BG_PANEL);

        var hlg = Hlg(p, 6, 8, 8, 8, 8);
        hlg.childForceExpandWidth  = true;
        hlg.childForceExpandHeight = true;
        hlg.childControlWidth      = true;
        hlg.childControlHeight     = true;
        hlg.childAlignment         = TextAnchor.MiddleCenter;

        var stop = MakeCtrlButton(p, "StopBtn",  "\u25A0", new Color(0.80f, 0.22f, 0.22f, 1f));
        var run  = MakeCtrlButton(p, "RunBtn",   "\u25B6", new Color(0.22f, 0.72f, 0.38f, 1f));
        var step = MakeCtrlButton(p, "StepBtn",  "\u25B7", new Color(0.22f, 0.50f, 0.85f, 1f));

        return (stop, run, step);
    }

    static Button MakeCtrlButton(GameObject parent, string name, string icon, Color accent)
    {
        var go  = FindOrCreate(parent, name);
        var img = Img(go, BG_BTN);
        var btn = Go<Button>(go);
        btn.targetGraphic = img;

        var c = btn.colors;
        c.normalColor      = BG_BTN;
        c.highlightedColor = Color.Lerp(BG_BTN, accent, 0.4f);
        c.pressedColor     = Color.Lerp(BG_BTN, accent, 0.7f);
        c.disabledColor    = new Color(0.12f, 0.12f, 0.18f, 0.4f);
        btn.colors = c;

        // Colored accent bar at top of button
        var barGO  = FindOrCreate(go, "AccentTop");
        var barLE  = Go<LayoutElement>(barGO);
        barLE.ignoreLayout = true;
        var barRT  = Go<RectTransform>(barGO);
        barRT.anchorMin        = new Vector2(0f, 1f);
        barRT.anchorMax        = new Vector2(1f, 1f);
        barRT.pivot            = new Vector2(0.5f, 1f);
        barRT.anchoredPosition = Vector2.zero;
        barRT.sizeDelta        = new Vector2(0, 2);
        var barImg = Img(barGO, accent);
        barImg.raycastTarget = false;

        // Icon label
        var lbl = FindOrCreate(go, "Lbl");
        FillRT(lbl, go);
        Tmp(lbl, icon, 16, TEXT_PRI, FontStyles.Normal, TextAlignmentOptions.Center);

        return btn;
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  5.  BOTTOM ACTION PANEL   (Undo / Clear program)
    // ══════════════════════════════════════════════════════════════════════════

    static void BuildActionPanel(GameObject hud)
    {
        var p = FindOrCreate(hud, "BottomActionPanel");
        AnchorRT(p, new Vector2(1,0), new Vector2(1,0), new Vector2(1,0),
                 new Vector2(-18, 18), new Vector2(200, 58));
        Img(p, BG_PANEL);

        var hlg = Hlg(p, 6, 8, 8, 8, 8);
        hlg.childForceExpandWidth  = true;
        hlg.childForceExpandHeight = true;
        hlg.childControlWidth      = true;
        hlg.childControlHeight     = true;
        hlg.childAlignment         = TextAnchor.MiddleCenter;

        MakeCtrlButton(p, "UndoBtn",  "\u21A9", new Color(0.55f, 0.55f, 0.65f, 1f));
        MakeCtrlButton(p, "ClearBtn", "\u2715", new Color(0.80f, 0.28f, 0.28f, 1f));
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  6.  MESSAGE PANEL   (bottom-center, timed feedback)
    // ══════════════════════════════════════════════════════════════════════════

    static (GameObject panel, TextMeshProUGUI text) BuildMessagePanel(GameObject hud)
    {
        var p = FindOrCreate(hud, "MessagePanel");
        AnchorRT(p, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                 new Vector2(0, 88), new Vector2(500, 40));
        Img(p, new Color(0.06f, 0.06f, 0.09f, 0.88f));

        // Accent left bar
        var bar   = FindOrCreate(p, "Bar");
        var barRT = Go<RectTransform>(bar);
        barRT.anchorMin        = new Vector2(0, 0);
        barRT.anchorMax        = new Vector2(0, 1);
        barRT.pivot            = new Vector2(0, 0.5f);
        barRT.anchoredPosition = Vector2.zero;
        barRT.sizeDelta        = new Vector2(3, 0);
        Img(bar, ORANGE).raycastTarget = false;

        var lbl = FindOrCreate(p, "Text");
        FillRT(lbl, p);
        var tmp = Tmp(lbl, "", 12, TEXT_PRI, FontStyles.Normal, TextAlignmentOptions.MidlineCenter);
        p.SetActive(false);   // hidden until first message

        return (p, tmp);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  7.  POPUP / DRAG-GHOST LAYERS
    // ══════════════════════════════════════════════════════════════════════════

    static void EnsurePopupLayer(GameObject hud)
    {
        var layer = FindOrCreate(hud, "PopupLayer");
        StretchFill(layer, hud);
        var img = Img(layer, new Color(0, 0, 0, 0));
        img.raycastTarget = false;
        layer.SetActive(false);
    }

    static GameObject EnsureDragGhostLayer(GameObject hud)
    {
        var layer = FindOrCreate(hud, "DragGhostLayer");
        StretchFill(layer, hud);
        var img = Img(layer, new Color(0, 0, 0, 0));
        img.raycastTarget = false;

        // Separate Canvas so ghosts always render above everything
        var c = Go<Canvas>(layer);
        c.overrideSorting = true;
        c.sortingOrder    = 100;
        Go<GraphicRaycaster>(layer);

        return layer;
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  8.  SCENE MANAGERS  (ProgramModel, SatelliteState, DragDropController,
    //                        ProgramExecutor, UIController)
    // ══════════════════════════════════════════════════════════════════════════

    static void SetupManagers(
        GameObject          ghostLayer,
        ProgramLineView[]   lineViews,
        Button              stopBtn,
        Button              runBtn,
        Button              stepBtn,
        GameObject          msgPanel,
        TextMeshProUGUI     msgText)
    {
        // Root container
        var root = FindOrCreateSceneRoot("SceneManagers");

        var model    = GetOrAddInChild<ProgramModel>       (root, "ProgramModel");
        var satState = GetOrAddInChild<SatelliteState>     (root, "SatelliteState");
        var ddc      = GetOrAddInChild<DragDropController> (root, "DragDropController");
        var executor = GetOrAddInChild<ProgramExecutor>    (root, "ProgramExecutor");
        var uiCtrl   = GetOrAddInChild<UIController>       (root, "UIController");

        // DragDropController → ghost layer
        ddc.SetGhostLayer(ghostLayer.transform);

        // ProgramExecutor → model + satellite state
        executor.model     = model;
        executor.satellite = satState;

        // UIController → everything
        uiCtrl.executor     = executor;
        uiCtrl.stopBtn      = stopBtn;
        uiCtrl.runBtn       = runBtn;
        uiCtrl.stepBtn      = stepBtn;
        uiCtrl.messagePanel = msgPanel;
        uiCtrl.messageText  = msgText;
        uiCtrl.programLines = lineViews;

        // Mark dirty so Unity serialises all the new object-references
        EditorUtility.SetDirty(ddc);
        EditorUtility.SetDirty(executor);
        EditorUtility.SetDirty(uiCtrl);
        EditorUtility.SetDirty(root);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  SCROLL VIEW helper
    // ══════════════════════════════════════════════════════════════════════════

    static void SetupScrollView(GameObject root, out GameObject content)
    {
        Img(root, new Color(0, 0, 0, 0.01f));

        var viewport = FindOrCreate(root, "Viewport");
        StretchFill(viewport, root);
        Img(viewport, new Color(0, 0, 0, 0));
        var mask = Go<Mask>(viewport);
        mask.showMaskGraphic = false;

        var contentGO = FindOrCreate(viewport, "Content");
        var cRT = Go<RectTransform>(contentGO);
        cRT.anchorMin        = new Vector2(0, 1);
        cRT.anchorMax        = new Vector2(1, 1);
        cRT.pivot            = new Vector2(0.5f, 1f);
        cRT.anchoredPosition = Vector2.zero;
        cRT.sizeDelta        = Vector2.zero;

        var sr = Go<ScrollRect>(root);
        sr.viewport          = viewport.GetComponent<RectTransform>();
        sr.content           = cRT;
        sr.vertical          = true;
        sr.horizontal        = false;
        sr.movementType      = ScrollRect.MovementType.Elastic;
        sr.scrollSensitivity = 30f;
        sr.inertia           = true;

        content = contentGO;
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  CANVAS
    // ══════════════════════════════════════════════════════════════════════════

    static GameObject FindOrCreateCanvas()
    {
        Canvas found = null;
        foreach (var c in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            if (c.renderMode == RenderMode.ScreenSpaceOverlay) { found = c; break; }
        }

        GameObject go;
        if (found != null)
        {
            go = found.gameObject;
        }
        else
        {
            go = new GameObject("Canvas");
            go.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            go.AddComponent<GraphicRaycaster>();
        }

        // Always update scaler to 1920×1080
        var scaler = go.GetComponent<CanvasScaler>() ?? go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight  = 0.5f;

        return go;
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  HELPERS
    // ══════════════════════════════════════════════════════════════════════════

    static void CleanChildren(GameObject go)
    {
        var children = new System.Collections.Generic.List<GameObject>();
        foreach (Transform t in go.transform) children.Add(t.gameObject);
        foreach (var child in children) Object.DestroyImmediate(child);
    }

    static GameObject FindOrCreate(GameObject parent, string name)
    {
        if (parent == null) return new GameObject(name);
        var t = parent.transform.Find(name);
        if (t != null) return t.gameObject;
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.layer = LayerMask.NameToLayer("UI");
        return go;
    }

    static GameObject FindOrCreateSceneRoot(string name)
    {
        var go = GameObject.Find(name);
        if (go != null) return go;
        return new GameObject(name);
    }

    static T GetOrAddInChild<T>(GameObject parent, string childName) where T : Component
    {
        var existing = parent.transform.Find(childName);
        if (existing != null)
        {
            var comp = existing.GetComponent<T>();
            return comp != null ? comp : existing.gameObject.AddComponent<T>();
        }
        var go = new GameObject(childName);
        go.transform.SetParent(parent.transform, false);
        return go.AddComponent<T>();
    }

    // ── RectTransform helpers ─────────────────────────────────────────────────

    static void AnchorRT(GameObject go,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchoredPos, Vector2 size)
    {
        var rt = Go<RectTransform>(go);
        rt.anchorMin        = anchorMin;
        rt.anchorMax        = anchorMax;
        rt.pivot            = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta        = size;
    }

    static void StretchFill(GameObject go, GameObject parent)
    {
        var rt = Go<RectTransform>(go);
        rt.anchorMin        = Vector2.zero;
        rt.anchorMax        = Vector2.one;
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta        = Vector2.zero;
    }

    static RectTransform FillRT(GameObject go, GameObject parent)
    {
        var rt = Go<RectTransform>(go);
        rt.anchorMin        = Vector2.zero;
        rt.anchorMax        = Vector2.one;
        rt.sizeDelta        = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
        return rt;
    }

    // ── Component / graphic helpers ───────────────────────────────────────────

    static Image Img(GameObject go, Color col)
    {
        Go<CanvasRenderer>(go);
        var img = Go<Image>(go);
        img.color = col;
        return img;
    }

    static VerticalLayoutGroup Vlg(GameObject go, int l, int r, int t, int b, float spacing)
    {
        var vlg = Go<VerticalLayoutGroup>(go);
        vlg.padding = new RectOffset(l, r, t, b);
        vlg.spacing = spacing;
        return vlg;
    }

    static HorizontalLayoutGroup Hlg(GameObject go, float spacing, int l, int r, int t, int b)
    {
        var hlg = Go<HorizontalLayoutGroup>(go);
        hlg.padding = new RectOffset(l, r, t, b);
        hlg.spacing = spacing;
        return hlg;
    }

    static TextMeshProUGUI Tmp(GameObject go, string text, float size,
        Color color, FontStyles style, TextAlignmentOptions align)
    {
        Go<CanvasRenderer>(go);
        Go<RectTransform>(go);
        var tmp = Go<TextMeshProUGUI>(go);
        tmp.text                = text;
        tmp.fontSize            = size;
        tmp.color               = color;
        tmp.fontStyle           = style;
        tmp.alignment           = align;
        tmp.enableWordWrapping  = false;
        tmp.overflowMode        = TextOverflowModes.Ellipsis;
        return tmp;
    }

    static void LE(GameObject go,
        float prefW = -1, float prefH = -1,
        float flexW = -1, float flexH = -1)
    {
        var le = Go<LayoutElement>(go);
        if (prefW >= 0) le.preferredWidth  = prefW;
        if (prefH >= 0) le.preferredHeight = prefH;
        if (flexW >= 0) le.flexibleWidth   = flexW;
        if (flexH >= 0) le.flexibleHeight  = flexH;
    }

    static T Go<T>(GameObject go) where T : Component
    {
        var c = go.GetComponent<T>();
        return c != null ? c : go.AddComponent<T>();
    }
}
#endif
