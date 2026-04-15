#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

/// <summary>
/// COSMA UI Scene Builder v3 — matches target screenshot exactly.
/// Auto-runs on SampleScene open. Manual: COSMA → Rebuild UI Scene
/// </summary>
[InitializeOnLoad]
public static class COSMAUISceneBuilder
{
    private const string SESSION_KEY = "COSMA_UI_BUILT_v3";

    // ── Colors ──────────────────────────────────────────
    static readonly Color BG_PANEL    = new Color(0.06f, 0.06f, 0.09f, 0.91f);
    static readonly Color BG_HEADER   = new Color(0.10f, 0.10f, 0.15f, 1.00f);
    static readonly Color BG_ROW_A    = new Color(0.09f, 0.09f, 0.13f, 1.00f);
    static readonly Color BG_ROW_B    = new Color(0.11f, 0.11f, 0.16f, 1.00f);
    static readonly Color BG_BTN      = new Color(0.15f, 0.15f, 0.22f, 1.00f);
    static readonly Color BG_BTN_CTRL = new Color(0.12f, 0.12f, 0.18f, 1.00f);
    static readonly Color ORANGE      = new Color(1.00f, 0.42f, 0.21f, 1.00f);
    static readonly Color BLUE        = new Color(0.00f, 0.66f, 0.91f, 1.00f);
    static readonly Color TEXT_PRI    = Color.white;
    static readonly Color TEXT_SEC    = new Color(0.72f, 0.72f, 0.78f, 1.00f);
    static readonly Color TEXT_MUTED  = new Color(0.38f, 0.38f, 0.45f, 1.00f);
    static readonly Color EMPTY_LINE  = new Color(0.28f, 0.28f, 0.35f, 1.00f);

    // ── Entry points ─────────────────────────────────────

    static COSMAUISceneBuilder()
    {
        EditorApplication.delayCall          += TryAutoSetup;
        EditorSceneManager.sceneOpened       += OnSceneOpened;
    }

    static void OnSceneOpened(UnityEngine.SceneManagement.Scene scene,
                               OpenSceneMode mode)
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

    [MenuItem("COSMA/Rebuild UI Scene")]
    public static void BuildUI()
    {
        GameObject canvas = FindOrCreateCanvas();
        GameObject hud    = FindOrCreate(canvas, "HUDLayer");
        StretchFill(hud, canvas);

        // ── Wipe ALL old children so nothing overlaps ──
        CleanChildren(hud);

        // ── Build panels ──────────────────────────────
        BuildMissionPanel(hud);
        BuildModulePanel(hud);
        BuildProgramPanel(hud);
        BuildControlPanel(hud);
        BuildActionPanel(hud);
        BuildMessagePanel(hud);
        EnsurePopupLayer(hud);
        EnsureDragGhostLayer(hud);

        // ── Save ──────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log("[COSMA] UI built and saved.");
        EditorUtility.DisplayDialog("COSMA", "UI создан!\nОткрой Hierarchy и Game View.", "OK");
    }

    // ══════════════════════════════════════════════════
    //  1. MISSION PANEL   top-right, w=520 h=auto (1920x1080)
    // ══════════════════════════════════════════════════
    static void BuildMissionPanel(GameObject hud)
    {
        var p = FindOrCreate(hud, "MissionPanel");
        AnchorRT(p, hud, new Vector2(1,1), new Vector2(1,1), new Vector2(1,1),
                 new Vector2(-18, -18), new Vector2(520, 0));
        Img(p, BG_PANEL);

        var vlg = Vlg(p, 14, 14, 14, 14, 8);
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth      = true;
        vlg.childControlHeight     = true;

        var csf = Go<ContentSizeFitter>(p);
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Header
        var hdrGO = FindOrCreate(p, "MissionHeader");
        LE(hdrGO, prefH: 22);
        var hdrTMP = Tmp(hdrGO, "Цель миссии:", 13, TEXT_PRI, FontStyles.Bold,
                         TextAlignmentOptions.MidlineLeft);

        // Body
        var bodyGO = FindOrCreate(p, "MissionBody");
        LE(bodyGO, prefH: 100);
        var bodyTMP = Tmp(bodyGO,
            "Провести полную настройку и оптимизацию спутника, чтобы обеспечить " +
            "стабильную работу его сенсоров и передачу данных на орбитальную станцию. " +
            "Выполни все этапы конфигурации: от калибровки программного обеспечения " +
            "до тестирования связи, чтобы миссия прошла без сбоев.",
            11, TEXT_SEC, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        bodyTMP.enableWordWrapping = true;
        bodyTMP.overflowMode       = TextOverflowModes.Overflow;
    }

    // ══════════════════════════════════════════════════
    //  2. MODULE PANEL   right side, w=210 h=auto (1920x1080)
    //     Sits to the LEFT of ProgramPanel
    //     ProgramPanel is 330px wide with 18px margin → left edge at -(330+18+8)=-356
    //     ModulePanel right edge aligns with ProgramPanel left edge − gap
    // ══════════════════════════════════════════════════
    static void BuildModulePanel(GameObject hud)
    {
        var p = FindOrCreate(hud, "ModulePanel");
        // anchor top-right; ProgramPanel is at (-18, ...) with w=330
        // so ModulePanel anchoredPosition.x = -(18 + 330 + 8 + 210) = -566
        AnchorRT(p, hud, new Vector2(1,1), new Vector2(1,1), new Vector2(1,1),
                 new Vector2(-566, -230), new Vector2(210, 0));
        Img(p, BG_PANEL);

        var vlg = Vlg(p, 0, 0, 0, 0, 0);
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth      = true;
        vlg.childControlHeight     = true;

        var csf = Go<ContentSizeFitter>(p);
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        (string name, bool expanded, string[] cmds)[] modules = {
            ("Модуль маховика",       false, new string[0]),
            ("Модуль солнечной батареи", false, new string[0]),
            ("Модуль камеры",         false, new string[0]),
            ("Модуль передатчика",    false, new string[0]),
            ("Модуль базовых команд", true,  new[]{"if","jump","copy to","jump if"}),
        };

        for (int i = 0; i < modules.Length; i++)
        {
            var (mName, mExpanded, cmds) = modules[i];
            BuildModuleRow(p, $"Module_{i:D2}", mName, mExpanded, cmds);
        }
    }

    static void BuildModuleRow(GameObject parent, string id, string label,
                               bool expanded, string[] cmds)
    {
        var row = FindOrCreate(parent, id);
        LE(row, prefH: expanded ? (28 + 32 * ((cmds.Length + 1) / 2)) : 28);
        Img(row, BG_PANEL);

        var vlg = Vlg(row, 0, 0, 0, 0, 0);
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth      = true;
        vlg.childControlHeight     = true;

        // Header row
        var hdr = FindOrCreate(row, "Header");
        LE(hdr, prefH: 28);
        Img(hdr, new Color(0,0,0,0));

        var hlg = Hlg(hdr, 8, 4, 4, 4, 4);
        hlg.childForceExpandWidth  = false;
        hlg.childForceExpandHeight = true;
        hlg.childControlWidth      = true;
        hlg.childControlHeight     = true;
        hlg.childAlignment         = TextAnchor.MiddleLeft;

        // Triangle icon
        var triGO = FindOrCreate(hdr, "Triangle");
        LE(triGO, prefW: 14);
        Tmp(triGO, expanded ? "\u25B3" : "\u25BD", 10,
            expanded ? ORANGE : TEXT_SEC,
            FontStyles.Normal, TextAlignmentOptions.Center);

        // Module name
        var lblGO = FindOrCreate(hdr, "Label");
        LE(lblGO, flexW: 1);
        Tmp(lblGO, label, 11,
            expanded ? ORANGE : TEXT_SEC,
            expanded ? FontStyles.Bold : FontStyles.Normal,
            TextAlignmentOptions.MidlineLeft);

        // Command buttons (only when expanded)
        if (expanded && cmds.Length > 0)
        {
            // Layout in pairs: 2 buttons per row
            for (int r = 0; r < (cmds.Length + 1) / 2; r++)
            {
                var cmdRow = FindOrCreate(row, $"CmdRow_{r}");
                LE(cmdRow, prefH: 30);
                var rowHlg = Hlg(cmdRow, 4, 6, 6, 2, 2);
                rowHlg.childForceExpandWidth  = true;
                rowHlg.childForceExpandHeight = true;
                rowHlg.childControlWidth      = true;
                rowHlg.childControlHeight     = true;
                rowHlg.childAlignment         = TextAnchor.MiddleLeft;

                for (int c = 0; c < 2; c++)
                {
                    int idx = r * 2 + c;
                    if (idx >= cmds.Length) break;
                    var btnGO = FindOrCreate(cmdRow, $"Cmd_{idx}");
                    LE(btnGO, flexW: 1, prefH: 26);
                    var btnImg = Img(btnGO, BG_BTN);
                    var btn = Go<Button>(btnGO);
                    btn.targetGraphic = btnImg;
                    var bc = btn.colors;
                    bc.highlightedColor = new Color(0.25f, 0.25f, 0.38f, 1f);
                    bc.pressedColor     = new Color(0.10f, 0.10f, 0.15f, 1f);
                    btn.colors = bc;
                    Go<CanvasGroup>(btnGO);

                    var lbl = FindOrCreate(btnGO, "Lbl");
                    FillRT(lbl, btnGO);
                    Tmp(lbl, cmds[idx], 11, TEXT_PRI, FontStyles.Normal,
                        TextAlignmentOptions.Center);
                }
            }
        }
    }

    // ══════════════════════════════════════════════════
    //  3. PROGRAM PANEL   right side, w=330 h=620 (1920x1080)
    // ══════════════════════════════════════════════════
    static void BuildProgramPanel(GameObject hud)
    {
        var p = FindOrCreate(hud, "ProgramPanel");
        AnchorRT(p, hud, new Vector2(1,1), new Vector2(1,1), new Vector2(1,1),
                 new Vector2(-18, -230), new Vector2(330, 620));

        Img(p, BG_PANEL);

        var vlg = Vlg(p, 0, 0, 0, 0, 0);
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth      = true;
        vlg.childControlHeight     = true;

        // ── Scroll view ──
        var scroll = FindOrCreate(p, "ProgramScroll");
        LE(scroll, flexH: 1, flexW: 1);
        SetupScroll(scroll, out GameObject content);
        content.name = "ProgramLinesContainer";

        var cVlg = Vlg(content, 0, 0, 0, 0, 0);
        cVlg.childForceExpandWidth  = true;
        cVlg.childForceExpandHeight = false;
        cVlg.childControlWidth      = true;
        cVlg.childControlHeight     = true;

        var csf = Go<ContentSizeFitter>(content);
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Demo: first 3 lines have placeholder commands
        string[] demoCommands = { "copy to", "jump", "jump if", "", "", "", "", "", "", "", "", "", "" };

        for (int i = 1; i <= 13; i++)
            BuildProgramLine(content, i, i <= 3 ? demoCommands[i - 1] : "");

        // ── More indicator "..." ──
        var moreGO = FindOrCreate(p, "MoreDots");
        LE(moreGO, prefH: 28);
        Img(moreGO, BG_PANEL);
        var moreTmp = Tmp(moreGO, "\u2026", 14, TEXT_MUTED, FontStyles.Normal,
                          TextAlignmentOptions.Center);
    }

    static void BuildProgramLine(GameObject parent, int idx, string command)
    {
        var line = FindOrCreate(parent, $"ProgramLine_{idx:D2}");
        LE(line, prefH: 36);
        Img(line, idx % 2 == 0 ? BG_ROW_A : BG_ROW_B);

        var hlg = Hlg(line, 0, 0, 0, 0, 0);
        hlg.childForceExpandWidth  = false;
        hlg.childForceExpandHeight = true;
        hlg.childControlWidth      = true;
        hlg.childControlHeight     = true;
        hlg.childAlignment         = TextAnchor.MiddleLeft;
        hlg.spacing                = 0;

        // Line number
        var numGO = FindOrCreate(line, "Num");
        LE(numGO, prefW: 32);
        Img(numGO, new Color(0,0,0,0));
        var numTmp = Tmp(numGO, idx.ToString("D2"), 12, ORANGE, FontStyles.Bold,
                         TextAlignmentOptions.Center);

        // Content area (command chip or empty line)
        var slotGO = FindOrCreate(line, "Slot");
        LE(slotGO, flexW: 1);
        Img(slotGO, new Color(0,0,0,0));

        var slotHlg = Hlg(slotGO, 0, 4, 4, 4, 4);
        slotHlg.childAlignment         = TextAnchor.MiddleLeft;
        slotHlg.childForceExpandWidth  = false;
        slotHlg.childForceExpandHeight = true;
        slotHlg.childControlWidth      = true;
        slotHlg.childControlHeight     = true;

        if (!string.IsNullOrEmpty(command))
        {
            // Command chip
            var chipGO = FindOrCreate(slotGO, "Chip");
            LE(chipGO, prefH: 24, prefW: Mathf.Max(60, command.Length * 7 + 16));
            Img(chipGO, BG_BTN);
            Go<CanvasGroup>(chipGO);

            var chipHlg = Hlg(chipGO, 0, 8, 8, 0, 0);
            chipHlg.childAlignment         = TextAnchor.MiddleCenter;
            chipHlg.childForceExpandWidth  = true;
            chipHlg.childForceExpandHeight = true;
            chipHlg.childControlWidth      = true;
            chipHlg.childControlHeight     = true;

            var chipLbl = FindOrCreate(chipGO, "Lbl");
            FillRT(chipLbl, chipGO);
            Tmp(chipLbl, command, 11, TEXT_PRI, FontStyles.Normal,
                TextAlignmentOptions.Center);
        }
        else
        {
            // Empty line indicator
            var emptyGO = FindOrCreate(slotGO, "EmptyLine");
            LE(emptyGO, prefW: 60, prefH: 3);
            Img(emptyGO, EMPTY_LINE);
        }
    }

    // ══════════════════════════════════════════════════
    //  4. CONTROL PANEL   bottom-left  □ ▶ ─── ▷ ▷▷  (1920x1080)
    // ══════════════════════════════════════════════════
    static void BuildControlPanel(GameObject hud)
    {
        var p = FindOrCreate(hud, "ControlPanel");
        AnchorRT(p, hud, new Vector2(0,0), new Vector2(0,0), new Vector2(0,0),
                 new Vector2(18, 18), new Vector2(310, 62));
        Img(p, BG_PANEL);

        var hlg = Hlg(p, 8, 8, 8, 8, 8);
        hlg.childForceExpandWidth  = false;
        hlg.childForceExpandHeight = true;
        hlg.childControlWidth      = true;
        hlg.childControlHeight     = true;
        hlg.childAlignment         = TextAnchor.MiddleCenter;

        // □ Stop
        MakeCtrlBtn(p, "StopBtn", "\u25A1", 38);
        // ▶ Play
        MakeCtrlBtn(p, "PlayBtn", "\u25B6", 38);
        // Progress slider
        var sliderGO = FindOrCreate(p, "ProgressSlider");
        LE(sliderGO, prefW: 70, prefH: 16);
        MakeSimpleSlider(sliderGO);
        // ▷ Step
        MakeCtrlBtn(p, "StepBtn",  "\u25B7",     32);
        // ▷▷ Fast step
        MakeCtrlBtn(p, "FastBtn",  "\u25B7\u25B7", 38);
    }

    static void MakeCtrlBtn(GameObject parent, string name, string icon, float w)
    {
        var go = FindOrCreate(parent, name);
        LE(go, prefW: w, prefH: 46);
        var img = Img(go, BG_BTN_CTRL);
        var btn = Go<Button>(go);
        btn.targetGraphic = img;
        var c = btn.colors;
        c.normalColor      = BG_BTN_CTRL;
        c.highlightedColor = new Color(0.22f, 0.22f, 0.32f, 1f);
        c.pressedColor     = new Color(0.08f, 0.08f, 0.12f, 1f);
        btn.colors = c;

        var lbl = FindOrCreate(go, "Lbl");
        FillRT(lbl, go);
        Tmp(lbl, icon, 14, TEXT_PRI, FontStyles.Normal, TextAlignmentOptions.Center);
    }

    static void MakeSimpleSlider(GameObject go)
    {
        var bgImg = Img(go, new Color(0.18f, 0.18f, 0.26f, 1f));

        var fillArea = FindOrCreate(go, "FillArea");
        var fillRT = FillRT(fillArea, go);
        fillRT.offsetMin = new Vector2(4, 4);
        fillRT.offsetMax = new Vector2(-4, -4);

        var fill = FindOrCreate(fillArea, "Fill");
        var fillImg = Img(fill, ORANGE);
        var fillFillRT = fill.GetComponent<RectTransform>();
        fillFillRT.anchorMin = new Vector2(0, 0);
        fillFillRT.anchorMax = new Vector2(0.3f, 1f);
        fillFillRT.sizeDelta = Vector2.zero;
        fillFillRT.anchoredPosition = Vector2.zero;

        var slider = Go<Slider>(go);
        slider.targetGraphic  = bgImg;
        slider.fillRect       = fillFillRT;
        slider.value          = 0f;
        slider.minValue       = 0f;
        slider.maxValue       = 1f;
        slider.wholeNumbers   = false;
        slider.direction      = Slider.Direction.LeftToRight;
    }

    // ══════════════════════════════════════════════════
    //  5. ACTION PANEL   bottom-right  ↩ ⧉ ⧋ 🗑  (1920x1080)
    // ══════════════════════════════════════════════════
    static void BuildActionPanel(GameObject hud)
    {
        var p = FindOrCreate(hud, "ActionPanel");
        AnchorRT(p, hud, new Vector2(1,0), new Vector2(1,0), new Vector2(1,0),
                 new Vector2(-18, 18), new Vector2(270, 62));
        Img(p, BG_PANEL);

        var hlg = Hlg(p, 6, 8, 8, 8, 8);
        hlg.childForceExpandWidth  = true;
        hlg.childForceExpandHeight = true;
        hlg.childControlWidth      = true;
        hlg.childControlHeight     = true;
        hlg.childAlignment         = TextAnchor.MiddleCenter;

        MakeActionBtn(p, "UndoBtn",   "\u21A9", new Color(0.55f, 0.55f, 0.65f, 0.9f));
        MakeActionBtn(p, "CopyBtn",   "\u2398", new Color(0.55f, 0.55f, 0.65f, 0.9f));
        MakeActionBtn(p, "PasteBtn",  "\u2399", new Color(0.55f, 0.55f, 0.65f, 0.9f));
        MakeActionBtn(p, "DeleteBtn", "\u2715", new Color(0.65f, 0.20f, 0.20f, 0.9f));
    }

    static void MakeActionBtn(GameObject parent, string name, string icon, Color col)
    {
        var go = FindOrCreate(parent, name);
        var img = Img(go, col);
        var btn = Go<Button>(go);
        btn.targetGraphic = img;
        var c = btn.colors;
        c.normalColor      = col;
        c.highlightedColor = Color.Lerp(col, Color.white, 0.25f);
        c.pressedColor     = col * 0.7f;
        btn.colors = c;

        var lbl = FindOrCreate(go, "Lbl");
        FillRT(lbl, go);
        Tmp(lbl, icon, 16, TEXT_PRI, FontStyles.Normal, TextAlignmentOptions.Center);
    }

    // ══════════════════════════════════════════════════
    //  6. MESSAGE PANEL   bottom-center (hidden)
    // ══════════════════════════════════════════════════
    static void BuildMessagePanel(GameObject hud)
    {
        var p = FindOrCreate(hud, "MessagePanel");
        AnchorRT(p, hud, new Vector2(0.5f,0), new Vector2(0.5f,0), new Vector2(0.5f,0),
                 new Vector2(0, 72), new Vector2(560, 44));
        Img(p, new Color(0, 0, 0, 0));

        var lbl = FindOrCreate(p, "Text");
        FillRT(lbl, p);
        Tmp(lbl, "", 13, TEXT_PRI, FontStyles.Normal, TextAlignmentOptions.Center);
        p.SetActive(false);
    }

    // ══════════════════════════════════════════════════
    //  7. POPUP / DRAG GHOST LAYERS
    // ══════════════════════════════════════════════════
    static void EnsurePopupLayer(GameObject hud)
    {
        var layer = FindOrCreate(hud, "PopupLayer");
        StretchFill(layer, hud);
        var img = Img(layer, new Color(0,0,0,0));
        img.raycastTarget = false;
        layer.SetActive(false);
    }

    static void EnsureDragGhostLayer(GameObject hud)
    {
        var layer = FindOrCreate(hud, "DragGhostLayer");
        StretchFill(layer, hud);
        var img = Img(layer, new Color(0,0,0,0));
        img.raycastTarget = false;
        var c = Go<Canvas>(layer);
        c.overrideSorting = true;
        c.sortingOrder    = 100;
        Go<GraphicRaycaster>(layer);
    }

    // ══════════════════════════════════════════════════
    //  SCROLL VIEW
    // ══════════════════════════════════════════════════
    static void SetupScroll(GameObject root, out GameObject content)
    {
        Img(root, new Color(0,0,0,0.01f));

        var viewport = FindOrCreate(root, "Viewport");
        StretchFill(viewport, root);
        Img(viewport, new Color(0,0,0,0));
        var mask = Go<Mask>(viewport);
        mask.showMaskGraphic = false;

        var contentGO = FindOrCreate(viewport, "Content");
        var cRT = Go<RectTransform>(contentGO);
        cRT.anchorMin = new Vector2(0,1);
        cRT.anchorMax = new Vector2(1,1);
        cRT.pivot     = new Vector2(0.5f,1f);
        cRT.anchoredPosition = Vector2.zero;
        cRT.sizeDelta = Vector2.zero;

        var sr = Go<ScrollRect>(root);
        sr.viewport        = viewport.GetComponent<RectTransform>();
        sr.content         = cRT;
        sr.vertical        = true;
        sr.horizontal      = false;
        sr.movementType    = ScrollRect.MovementType.Elastic;
        sr.scrollSensitivity = 25f;
        sr.inertia         = true;

        content = contentGO;
    }

    // ══════════════════════════════════════════════════
    //  CANVAS
    // ══════════════════════════════════════════════════
    static GameObject FindOrCreateCanvas()
    {
        foreach (var c in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            if (c.renderMode == RenderMode.ScreenSpaceOverlay) return c.gameObject;

        var go     = new GameObject("Canvas");
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.screenMatchMode    = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        go.AddComponent<GraphicRaycaster>();
        return go;
    }

    // ══════════════════════════════════════════════════
    //  HELPERS
    // ══════════════════════════════════════════════════

    /// <summary>Destroys all direct children of a GameObject (editor-safe).</summary>
    static void CleanChildren(GameObject go)
    {
        var children = new System.Collections.Generic.List<GameObject>();
        foreach (Transform child in go.transform)
            children.Add(child.gameObject);
        foreach (var child in children)
            Object.DestroyImmediate(child);
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

    static void AnchorRT(GameObject go, GameObject parent,
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
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
        return rt;
    }

    static Image Img(GameObject go, Color color)
    {
        Go<CanvasRenderer>(go);
        var img = Go<Image>(go);
        img.color = color;
        return img;
    }

    static VerticalLayoutGroup Vlg(GameObject go,
        int l, int r, int t, int b, float spacing)
    {
        var vlg = Go<VerticalLayoutGroup>(go);
        vlg.padding = new RectOffset(l, r, t, b);
        vlg.spacing = spacing;
        return vlg;
    }

    static HorizontalLayoutGroup Hlg(GameObject go,
        float spacing, int l, int r, int t, int b)
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
        tmp.text       = text;
        tmp.fontSize   = size;
        tmp.color      = color;
        tmp.fontStyle  = style;
        tmp.alignment  = align;
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Ellipsis;
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
