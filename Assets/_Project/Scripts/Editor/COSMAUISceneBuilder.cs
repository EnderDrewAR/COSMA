#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// COSMA UI Scene Builder
/// Автоматически создаёт весь UI в сцене при открытии Unity.
/// Запускается один раз — после этого флаг COSMA_UI_BUILT сохраняется.
/// Можно запустить вручную: COSMA → Rebuild UI Scene
/// </summary>
[InitializeOnLoad]
public static class COSMAUISceneBuilder
{
    private const string BUILT_KEY = "COSMA_UI_BUILT_v2";

    static COSMAUISceneBuilder()
    {
        EditorApplication.delayCall += TryAutoSetup;
    }

    private static void TryAutoSetup()
    {
        if (SessionState.GetBool(BUILT_KEY, false)) return;

        string activeScene = EditorSceneManager.GetActiveScene().name;
        if (activeScene != "SampleScene") return;

        SessionState.SetBool(BUILT_KEY, true);
        BuildUI();
    }

    [MenuItem("COSMA/Rebuild UI Scene")]
    public static void BuildUI()
    {
        var scene = EditorSceneManager.GetActiveScene();
        GameObject canvasGO = FindOrCreateCanvas();

        // Удаляем старые панели если есть (HUDLayer очищаем)
        GameObject hudLayer = FindOrCreate(canvasGO, "HUDLayer");
        RectTransform hudRT = SetupStretchRT(hudLayer, canvasGO.GetComponent<RectTransform>());

        // Создаём все панели верхнего уровня
        CreateMissionPanel(hudLayer);
        CreateRightProgrammingRoot(hudLayer);
        CreateBottomControlPanel(hudLayer);
        CreateBottomActionPanel(hudLayer);
        CreateMessagePanel(hudLayer);
        CreatePopupLayer(hudLayer);
        CreateDragGhostLayer(hudLayer);

        // Сохраняем сцену
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("[COSMA] UI Scene build complete! Check Hierarchy for new panels.");
        EditorUtility.DisplayDialog("COSMA UI", "UI успешно создан в сцене!\n\nОткрой Hierarchy — все панели готовы.", "OK");
    }

    // ─────────────────────────────────────────────────
    //  CANVAS
    // ─────────────────────────────────────────────────

    private static GameObject FindOrCreateCanvas()
    {
        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (var c in canvases)
            if (c.renderMode == RenderMode.ScreenSpaceOverlay)
                return c.gameObject;

        // Создаём Canvas если нет
        var go = new GameObject("Canvas");
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        go.AddComponent<GraphicRaycaster>();
        return go;
    }

    // ─────────────────────────────────────────────────
    //  MISSION PANEL  (top-left, 420x180)
    // ─────────────────────────────────────────────────

    private static void CreateMissionPanel(GameObject parent)
    {
        var panel = FindOrCreate(parent, "MissionPanel");
        var rt = SetupRT(panel, parent.GetComponent<RectTransform>(),
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(20, -20), new Vector2(420, 180));

        AddDarkImage(panel, new Color(0.08f, 0.08f, 0.12f, 0.88f));

        // Vertical Layout
        var vlg = GetOrAdd<VerticalLayoutGroup>(panel);
        vlg.padding = new RectOffset(14, 14, 12, 12);
        vlg.spacing = 6;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;

        // Header row
        var headerRow = FindOrCreate(panel, "MissionHeader");
        SetupHorizontalRow(headerRow, panel, 28);

        var iconLabel = FindOrCreate(headerRow, "MissionIcon");
        SetupLabel(iconLabel, headerRow, "\u25C6 MISSION", 13,
            new Color(1f, 0.42f, 0.21f, 1f), FontStyles.Bold);
        SetLayoutElement(iconLabel, flexibleWidth: 1);

        var statusDot = FindOrCreate(headerRow, "StatusDot");
        SetupLabel(statusDot, headerRow, "\u25CF ONLINE", 11,
            new Color(0.2f, 0.9f, 0.4f, 1f), FontStyles.Normal);
        SetLayoutElement(statusDot, preferredWidth: 80);

        // Title
        var titleGO = FindOrCreate(panel, "MissionTitle");
        SetupLabel(titleGO, panel, "ORBIT ALIGNMENT SEQUENCE", 16,
            Color.white, FontStyles.Bold);
        SetLayoutElement(titleGO, preferredHeight: 24);

        // Description
        var descGO = FindOrCreate(panel, "MissionDescription");
        var descTMP = SetupLabel(descGO, panel,
            "Выровняй солнечные панели спутника для максимальной зарядки. Запусти программу из 13 команд.",
            11, new Color(0.72f, 0.72f, 0.78f, 1f), FontStyles.Normal);
        descTMP.enableWordWrapping = true;
        SetLayoutElement(descGO, preferredHeight: 52);
    }

    // ─────────────────────────────────────────────────
    //  RIGHT PROGRAMMING ROOT  (right side, full height)
    // ─────────────────────────────────────────────────

    private static void CreateRightProgrammingRoot(GameObject parent)
    {
        var root = FindOrCreate(parent, "RightProgrammingRoot");
        SetupRT(root, parent.GetComponent<RectTransform>(),
            new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f),
            new Vector2(-10, 0), new Vector2(680, 0));

        var hlg = GetOrAdd<HorizontalLayoutGroup>(root);
        hlg.padding = new RectOffset(0, 0, 10, 10);
        hlg.spacing = 8;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;

        // Панели внутри root
        CreateCommandPoolPanel(root);
        CreateProgramPanel(root);
    }

    // ─────────────────────────────────────────────────
    //  COMMAND POOL PANEL  (left part of RightRoot, 280px)
    // ─────────────────────────────────────────────────

    private static void CreateCommandPoolPanel(GameObject parent)
    {
        var panel = FindOrCreate(parent, "CommandPoolPanel");
        var rt = panel.GetComponent<RectTransform>();
        if (rt == null) rt = panel.AddComponent<RectTransform>();
        SetLayoutElement(panel, preferredWidth: 280, flexibleHeight: 1);

        AddDarkImage(panel, new Color(0.07f, 0.07f, 0.11f, 0.92f));

        var vlg = GetOrAdd<VerticalLayoutGroup>(panel);
        vlg.padding = new RectOffset(0, 0, 0, 0);
        vlg.spacing = 0;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;

        // ── Header bar ──
        var headerBar = FindOrCreate(panel, "CommandPoolHeader");
        SetLayoutElement(headerBar, preferredHeight: 40);
        AddImage(headerBar, new Color(0.12f, 0.12f, 0.18f, 1f));

        var headerVLG = GetOrAdd<VerticalLayoutGroup>(headerBar);
        headerVLG.childAlignment = TextAnchor.MiddleCenter;
        headerVLG.childForceExpandWidth = true;
        headerVLG.childForceExpandHeight = true;

        var headerLabel = FindOrCreate(headerBar, "CommandPoolTitle");
        SetupLabel(headerLabel, headerBar, "ПУЛ КОМАНД", 13,
            new Color(1f, 0.42f, 0.21f, 1f), FontStyles.Bold);
        var lbl = headerLabel.GetComponent<TextMeshProUGUI>();
        lbl.alignment = TextAlignmentOptions.Center;

        // ── Scroll View ──
        var scrollRoot = FindOrCreate(panel, "CommandPoolScroll");
        SetLayoutElement(scrollRoot, flexibleHeight: 1, flexibleWidth: 1);
        SetupScrollViewFull(scrollRoot, out GameObject content);

        var contentVLG = GetOrAdd<VerticalLayoutGroup>(content);
        contentVLG.padding = new RectOffset(8, 8, 8, 8);
        contentVLG.spacing = 6;
        contentVLG.childForceExpandWidth = true;
        contentVLG.childForceExpandHeight = false;
        contentVLG.childControlWidth = true;
        contentVLG.childControlHeight = true;

        var csf = GetOrAdd<ContentSizeFitter>(content);
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // ── 7 команд ──
        string[] commands = {
            "\u26A1  Питание ВКЛ",
            "\u23FB  Питание ВЫКЛ",
            "\u2600  Считать солн. датчики",
            "\u25B6  Считать магнитометр",
            "\u2316  Повернуть к Земле",
            "\u2609  Повернуть к Солнцу",
            "\u21A9  Jump To",
            "\u25A3  Сделать фото Земли",
        };

        Color[] cmdColors = {
            new Color(0.2f, 0.9f, 0.4f, 1f),
            new Color(0.9f, 0.25f, 0.25f, 1f),
            new Color(1f, 0.75f, 0.2f, 1f),
            new Color(0.3f, 0.7f, 1f, 1f),
            new Color(0.3f, 0.85f, 0.85f, 1f),
            new Color(1f, 0.65f, 0.1f, 1f),
            new Color(0.8f, 0.5f, 1f, 1f),
            new Color(0.4f, 0.9f, 0.7f, 1f),
        };

        for (int i = 0; i < commands.Length; i++)
        {
            CreateCommandPoolItem(content, $"PoolItem_{i:D2}", commands[i], cmdColors[i % cmdColors.Length]);
        }
    }

    private static void CreateCommandPoolItem(GameObject parent, string name, string label, Color accentColor)
    {
        var item = FindOrCreate(parent, name);
        SetLayoutElement(item, preferredHeight: 44);

        // Background
        var img = GetOrAdd<Image>(item);
        img.color = new Color(0.13f, 0.13f, 0.19f, 1f);

        // Make it a Button
        var btn = GetOrAdd<Button>(item);
        var colors = btn.colors;
        colors.normalColor = new Color(0.13f, 0.13f, 0.19f, 1f);
        colors.highlightedColor = new Color(0.20f, 0.20f, 0.28f, 1f);
        colors.pressedColor = new Color(0.10f, 0.10f, 0.14f, 1f);
        btn.colors = colors;
        btn.targetGraphic = img;

        // Also draggable - add CanvasGroup
        var cg = GetOrAdd<CanvasGroup>(item);

        // Accent bar (left side) — use a child Image
        var accentBar = FindOrCreate(item, "AccentBar");
        var accentRT = GetOrAdd<RectTransform>(accentBar);
        var accentImg = GetOrAdd<Image>(accentBar);
        accentImg.color = accentColor;
        accentRT.anchorMin = new Vector2(0, 0);
        accentRT.anchorMax = new Vector2(0, 1);
        accentRT.pivot = new Vector2(0, 0.5f);
        accentRT.anchoredPosition = Vector2.zero;
        accentRT.sizeDelta = new Vector2(4, 0);

        // Label
        var labelGO = FindOrCreate(item, "Label");
        var labelRT = GetOrAdd<RectTransform>(labelGO);
        labelRT.anchorMin = new Vector2(0, 0);
        labelRT.anchorMax = new Vector2(1, 1);
        labelRT.pivot = new Vector2(0.5f, 0.5f);
        labelRT.offsetMin = new Vector2(14, 0);
        labelRT.offsetMax = new Vector2(-4, 0);

        var tmp = GetOrAdd<TextMeshProUGUI>(labelGO);
        tmp.text = label;
        tmp.fontSize = 12;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Ellipsis;

        GetOrAdd<CanvasRenderer>(labelGO);
    }

    // ─────────────────────────────────────────────────
    //  PROGRAM PANEL  (right part of RightRoot, 380px)
    // ─────────────────────────────────────────────────

    private static void CreateProgramPanel(GameObject parent)
    {
        var panel = FindOrCreate(parent, "ProgramPanel");
        var rt = panel.GetComponent<RectTransform>();
        if (rt == null) rt = panel.AddComponent<RectTransform>();
        SetLayoutElement(panel, preferredWidth: 380, flexibleHeight: 1);

        AddDarkImage(panel, new Color(0.06f, 0.06f, 0.10f, 0.93f));

        var vlg = GetOrAdd<VerticalLayoutGroup>(panel);
        vlg.padding = new RectOffset(0, 0, 0, 0);
        vlg.spacing = 0;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;

        // ── Header bar ──
        var headerBar = FindOrCreate(panel, "ProgramHeader");
        SetLayoutElement(headerBar, preferredHeight: 40);
        AddImage(headerBar, new Color(0.10f, 0.10f, 0.16f, 1f));

        var headerHLG = GetOrAdd<HorizontalLayoutGroup>(headerBar);
        headerHLG.padding = new RectOffset(14, 8, 0, 0);
        headerHLG.spacing = 8;
        headerHLG.childAlignment = TextAnchor.MiddleLeft;
        headerHLG.childForceExpandWidth = false;
        headerHLG.childForceExpandHeight = true;
        headerHLG.childControlWidth = true;
        headerHLG.childControlHeight = true;

        var titleGO = FindOrCreate(headerBar, "ProgramTitle");
        var titleTMP = SetupLabel(titleGO, headerBar, "ПРОГРАММА", 13,
            new Color(0f, 0.66f, 0.91f, 1f), FontStyles.Bold);
        SetLayoutElement(titleGO, flexibleWidth: 1);

        var lineCountGO = FindOrCreate(headerBar, "LineCount");
        SetupLabel(lineCountGO, headerBar, "13 строк", 11,
            new Color(0.5f, 0.5f, 0.6f, 1f), FontStyles.Normal);
        SetLayoutElement(lineCountGO, preferredWidth: 60);

        // ── Scroll View ──
        var scrollRoot = FindOrCreate(panel, "ProgramScroll");
        SetLayoutElement(scrollRoot, flexibleHeight: 1, flexibleWidth: 1);
        SetupScrollViewFull(scrollRoot, out GameObject content);

        content.name = "ProgramLinesContainer";

        var contentVLG = GetOrAdd<VerticalLayoutGroup>(content);
        contentVLG.padding = new RectOffset(8, 8, 8, 8);
        contentVLG.spacing = 4;
        contentVLG.childForceExpandWidth = true;
        contentVLG.childForceExpandHeight = false;
        contentVLG.childControlWidth = true;
        contentVLG.childControlHeight = true;

        var csf = GetOrAdd<ContentSizeFitter>(content);
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // ── 13 строк программы ──
        for (int i = 1; i <= 13; i++)
        {
            CreateProgramLine(content, i);
        }
    }

    private static void CreateProgramLine(GameObject parent, int index)
    {
        var line = FindOrCreate(parent, $"ProgramLine_{index:D2}");
        SetLayoutElement(line, preferredHeight: 48);

        // Slot background
        var img = GetOrAdd<Image>(line);
        img.color = new Color(0.11f, 0.11f, 0.17f, 1f);

        // Horizontal layout: line number | command area | delete btn
        var hlg = GetOrAdd<HorizontalLayoutGroup>(line);
        hlg.padding = new RectOffset(0, 0, 0, 0);
        hlg.spacing = 0;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;

        // ── Line number ──
        var numBg = FindOrCreate(line, "NumBg");
        SetLayoutElement(numBg, preferredWidth: 36);
        AddImage(numBg, index % 2 == 0
            ? new Color(0.14f, 0.14f, 0.20f, 1f)
            : new Color(0.12f, 0.12f, 0.18f, 1f));
        var numLabel = FindOrCreate(numBg, "Num");
        var numRT = GetOrAdd<RectTransform>(numLabel);
        numRT.anchorMin = Vector2.zero;
        numRT.anchorMax = Vector2.one;
        numRT.sizeDelta = Vector2.zero;
        var numTMP = GetOrAdd<TextMeshProUGUI>(numLabel);
        numTMP.text = index.ToString("D2");
        numTMP.fontSize = 11;
        numTMP.color = new Color(1f, 0.42f, 0.21f, 1f);
        numTMP.alignment = TextAlignmentOptions.Center;
        numTMP.fontStyle = FontStyles.Bold;
        GetOrAdd<CanvasRenderer>(numLabel);

        // ── Drop slot area ──
        var slot = FindOrCreate(line, "SlotArea");
        SetLayoutElement(slot, flexibleWidth: 1);

        var slotBg = GetOrAdd<Image>(slot);
        slotBg.color = new Color(0.09f, 0.09f, 0.13f, 1f);

        var slotHLG = GetOrAdd<HorizontalLayoutGroup>(slot);
        slotHLG.padding = new RectOffset(10, 6, 4, 4);
        slotHLG.spacing = 6;
        slotHLG.childAlignment = TextAnchor.MiddleLeft;
        slotHLG.childForceExpandWidth = false;
        slotHLG.childForceExpandHeight = true;
        slotHLG.childControlWidth = true;
        slotHLG.childControlHeight = true;

        // Slot placeholder text
        var placeholderGO = FindOrCreate(slot, "Placeholder");
        SetLayoutElement(placeholderGO, flexibleWidth: 1);
        var phRT = GetOrAdd<RectTransform>(placeholderGO);
        var phTMP = GetOrAdd<TextMeshProUGUI>(placeholderGO);
        phTMP.text = "— пусто —";
        phTMP.fontSize = 12;
        phTMP.color = new Color(0.35f, 0.35f, 0.40f, 1f);
        phTMP.alignment = TextAlignmentOptions.MidlineLeft;
        phTMP.fontStyle = FontStyles.Italic;
        GetOrAdd<CanvasRenderer>(placeholderGO);

        // ── Delete button ──
        var delBtnGO = FindOrCreate(line, "DeleteBtn");
        SetLayoutElement(delBtnGO, preferredWidth: 32);
        var delImg = GetOrAdd<Image>(delBtnGO);
        delImg.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        var delBtn = GetOrAdd<Button>(delBtnGO);
        delBtn.targetGraphic = delImg;
        var delColors = delBtn.colors;
        delColors.highlightedColor = new Color(0.6f, 0.1f, 0.1f, 0.5f);
        delBtn.colors = delColors;

        var delLabel = FindOrCreate(delBtnGO, "X");
        var delLabelRT = GetOrAdd<RectTransform>(delLabel);
        delLabelRT.anchorMin = Vector2.zero;
        delLabelRT.anchorMax = Vector2.one;
        delLabelRT.sizeDelta = Vector2.zero;
        var delTMP = GetOrAdd<TextMeshProUGUI>(delLabel);
        delTMP.text = "\u2715";
        delTMP.fontSize = 12;
        delTMP.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
        delTMP.alignment = TextAlignmentOptions.Center;
        GetOrAdd<CanvasRenderer>(delLabel);
    }

    // ─────────────────────────────────────────────────
    //  BOTTOM CONTROL PANEL  (bottom-left, RUN/STOP/STEP)
    // ─────────────────────────────────────────────────

    private static void CreateBottomControlPanel(GameObject parent)
    {
        var panel = FindOrCreate(parent, "BottomControlPanel");
        SetupRT(panel, parent.GetComponent<RectTransform>(),
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0),
            new Vector2(20, 20), new Vector2(320, 56));

        AddDarkImage(panel, new Color(0.08f, 0.08f, 0.12f, 0.92f));

        var hlg = GetOrAdd<HorizontalLayoutGroup>(panel);
        hlg.padding = new RectOffset(8, 8, 8, 8);
        hlg.spacing = 8;
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = true;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;

        // STOP
        CreateControlButton(panel, "StopBtn", "\u25A0  STOP",
            new Color(0.75f, 0.18f, 0.18f, 1f), new Color(0.9f, 0.25f, 0.25f, 1f));

        // RUN
        CreateControlButton(panel, "RunBtn", "\u25B6  RUN",
            new Color(0.18f, 0.65f, 0.25f, 1f), new Color(0.25f, 0.85f, 0.35f, 1f));

        // STEP
        CreateControlButton(panel, "StepBtn", "\u25B8\u25B8 STEP",
            new Color(0.1f, 0.45f, 0.75f, 1f), new Color(0.15f, 0.6f, 0.9f, 1f));

        // RESET
        CreateControlButton(panel, "ResetBtn", "\u21BB RESET",
            new Color(0.25f, 0.25f, 0.35f, 1f), new Color(0.35f, 0.35f, 0.5f, 1f));
    }

    private static void CreateControlButton(GameObject parent, string name, string label,
        Color normalColor, Color hoverColor)
    {
        var btnGO = FindOrCreate(parent, name);
        var img = GetOrAdd<Image>(btnGO);
        img.color = normalColor;

        var btn = GetOrAdd<Button>(btnGO);
        btn.targetGraphic = img;
        var colors = btn.colors;
        colors.normalColor = normalColor;
        colors.highlightedColor = hoverColor;
        colors.pressedColor = normalColor * 0.7f;
        btn.colors = colors;

        var textGO = FindOrCreate(btnGO, "Label");
        var textRT = GetOrAdd<RectTransform>(textGO);
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.sizeDelta = Vector2.zero;
        var tmp = GetOrAdd<TextMeshProUGUI>(textGO);
        tmp.text = label;
        tmp.fontSize = 13;
        tmp.color = Color.white;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        GetOrAdd<CanvasRenderer>(textGO);
    }

    // ─────────────────────────────────────────────────
    //  BOTTOM ACTION PANEL  (orbit controls)
    // ─────────────────────────────────────────────────

    private static void CreateBottomActionPanel(GameObject parent)
    {
        var panel = FindOrCreate(parent, "BottomActionPanel");
        SetupRT(panel, parent.GetComponent<RectTransform>(),
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0),
            new Vector2(20, 84), new Vector2(204, 164));

        AddDarkImage(panel, new Color(0.08f, 0.08f, 0.12f, 0.88f));

        // Header
        var hdr = FindOrCreate(panel, "ActionHeader");
        var hdrRT = GetOrAdd<RectTransform>(hdr);
        hdrRT.anchorMin = new Vector2(0, 1); hdrRT.anchorMax = new Vector2(1, 1);
        hdrRT.pivot = new Vector2(0.5f, 1f);
        hdrRT.anchoredPosition = new Vector2(0, 0);
        hdrRT.sizeDelta = new Vector2(0, 22);
        AddImage(hdr, new Color(0.12f, 0.12f, 0.18f, 1f));
        var hdrTMP = GetOrAdd<TextMeshProUGUI>(FindOrCreate(hdr, "Lbl"));
        hdrTMP.text = "УПРАВЛЕНИЕ";
        hdrTMP.fontSize = 11;
        hdrTMP.color = new Color(0f, 0.66f, 0.91f, 1f);
        hdrTMP.alignment = TextAlignmentOptions.Center;
        hdrTMP.fontStyle = FontStyles.Bold;
        GetOrAdd<CanvasRenderer>(FindOrCreate(hdr, "Lbl"));

        // 3-row grid: up / [left reset right] / down
        float btnSize = 46f;
        float gap = 4f;
        float startX = 34f;

        CreateOrbitBtn(panel, "RotateUpBtn", "\u2191", startX + btnSize + gap, 118f, btnSize);
        CreateOrbitBtn(panel, "RotateLeftBtn", "\u2190", startX, 118f - btnSize - gap, btnSize);
        CreateOrbitBtn(panel, "ResetOrbitBtn", "\u21BB", startX + btnSize + gap, 118f - btnSize - gap, btnSize);
        CreateOrbitBtn(panel, "RotateRightBtn", "\u2192", startX + (btnSize + gap) * 2, 118f - btnSize - gap, btnSize);
        CreateOrbitBtn(panel, "RotateDownBtn", "\u2193", startX + btnSize + gap, 118f - (btnSize + gap) * 2, btnSize);
    }

    private static void CreateOrbitBtn(GameObject parent, string name, string icon,
        float x, float y, float size)
    {
        var btnGO = FindOrCreate(parent, name);
        var rt = GetOrAdd<RectTransform>(btnGO);
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(0, 0);
        rt.pivot = new Vector2(0f, 0f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(size, size);

        var img = GetOrAdd<Image>(btnGO);
        img.color = new Color(0.16f, 0.16f, 0.24f, 1f);

        var btn = GetOrAdd<Button>(btnGO);
        btn.targetGraphic = img;
        var c = btn.colors;
        c.highlightedColor = new Color(0.25f, 0.25f, 0.38f, 1f);
        btn.colors = c;

        var lbl = FindOrCreate(btnGO, "Icon");
        var lblRT = GetOrAdd<RectTransform>(lbl);
        lblRT.anchorMin = Vector2.zero; lblRT.anchorMax = Vector2.one;
        lblRT.sizeDelta = Vector2.zero;
        var tmp = GetOrAdd<TextMeshProUGUI>(lbl);
        tmp.text = icon;
        tmp.fontSize = 18;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        GetOrAdd<CanvasRenderer>(lbl);
    }

    // ─────────────────────────────────────────────────
    //  MESSAGE PANEL  (bottom center, notifications)
    // ─────────────────────────────────────────────────

    private static void CreateMessagePanel(GameObject parent)
    {
        var panel = FindOrCreate(parent, "MessagePanel");
        SetupRT(panel, parent.GetComponent<RectTransform>(),
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(0, 24), new Vector2(600, 52));

        var img = GetOrAdd<Image>(panel);
        img.color = new Color(0.05f, 0.05f, 0.08f, 0.0f); // invisible by default

        var msgGO = FindOrCreate(panel, "MessageText");
        var msgRT = GetOrAdd<RectTransform>(msgGO);
        msgRT.anchorMin = Vector2.zero;
        msgRT.anchorMax = Vector2.one;
        msgRT.sizeDelta = Vector2.zero;
        var msgTMP = GetOrAdd<TextMeshProUGUI>(msgGO);
        msgTMP.text = "";
        msgTMP.fontSize = 13;
        msgTMP.color = Color.white;
        msgTMP.alignment = TextAlignmentOptions.Center;
        GetOrAdd<CanvasRenderer>(msgGO);
    }

    // ─────────────────────────────────────────────────
    //  POPUP LAYER
    // ─────────────────────────────────────────────────

    private static void CreatePopupLayer(GameObject parent)
    {
        var layer = FindOrCreate(parent, "PopupLayer");
        SetupStretchRT(layer, parent.GetComponent<RectTransform>());

        // Transparent blocker
        var img = GetOrAdd<Image>(layer);
        img.color = new Color(0, 0, 0, 0);
        img.raycastTarget = false;
        layer.SetActive(false); // hidden by default
    }

    // ─────────────────────────────────────────────────
    //  DRAG GHOST LAYER
    // ─────────────────────────────────────────────────

    private static void CreateDragGhostLayer(GameObject parent)
    {
        var layer = FindOrCreate(parent, "DragGhostLayer");
        SetupStretchRT(layer, parent.GetComponent<RectTransform>());

        var img = GetOrAdd<Image>(layer);
        img.color = new Color(0, 0, 0, 0);
        img.raycastTarget = false;

        // Sort order — поверх всего
        var canvas = GetOrAdd<Canvas>(layer);
        canvas.overrideSorting = true;
        canvas.sortingOrder = 100;
        GetOrAdd<GraphicRaycaster>(layer);
    }

    // ─────────────────────────────────────────────────
    //  UTILITY HELPERS
    // ─────────────────────────────────────────────────

    private static GameObject FindOrCreate(GameObject parent, string childName)
    {
        if (parent == null) return new GameObject(childName);

        Transform existing = parent.transform.Find(childName);
        if (existing != null) return existing.gameObject;

        var go = new GameObject(childName);
        go.transform.SetParent(parent.transform, false);
        go.layer = LayerMask.NameToLayer("UI");
        return go;
    }

    private static RectTransform SetupRT(GameObject go, RectTransform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchoredPos, Vector2 sizeDelta)
    {
        var rt = GetOrAdd<RectTransform>(go);
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;
        return rt;
    }

    private static RectTransform SetupStretchRT(GameObject go, RectTransform parent)
    {
        var rt = GetOrAdd<RectTransform>(go);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
        return rt;
    }

    private static void SetupScrollViewFull(GameObject scrollRoot, out GameObject content)
    {
        GetOrAdd<RectTransform>(scrollRoot);

        // ScrollRect only on root — no Mask here (Mask goes on viewport child)
        var scrollImg = GetOrAdd<Image>(scrollRoot);
        scrollImg.color = new Color(0, 0, 0, 0.01f);

        // ScrollRect
        var scrollRect = GetOrAdd<ScrollRect>(scrollRoot);
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Elastic;
        scrollRect.scrollSensitivity = 25;
        scrollRect.inertia = true;
        scrollRect.decelerationRate = 0.135f;

        // Viewport (same as scrollRoot for simplicity)
        var viewport = FindOrCreate(scrollRoot, "Viewport");
        SetupStretchRT(viewport, scrollRoot.GetComponent<RectTransform>());
        GetOrAdd<Image>(viewport).color = new Color(0, 0, 0, 0);
        GetOrAdd<Mask>(viewport).showMaskGraphic = false;
        scrollRect.viewport = viewport.GetComponent<RectTransform>();

        // Content
        var contentGO = FindOrCreate(viewport, "Content");
        var contentRT = GetOrAdd<RectTransform>(contentGO);
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1f);
        contentRT.anchoredPosition = Vector2.zero;
        contentRT.sizeDelta = new Vector2(0, 0);

        scrollRect.content = contentRT;
        content = contentGO;
    }

    private static TextMeshProUGUI SetupLabel(GameObject go, GameObject parent,
        string text, float size, Color color, FontStyles style)
    {
        GetOrAdd<RectTransform>(go);
        var tmp = GetOrAdd<TextMeshProUGUI>(go);
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.fontStyle = style;
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Ellipsis;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        GetOrAdd<CanvasRenderer>(go);
        return tmp;
    }

    private static void SetupHorizontalRow(GameObject go, GameObject parent, float height)
    {
        GetOrAdd<RectTransform>(go);
        SetLayoutElement(go, preferredHeight: height, flexibleWidth: 1);
        var hlg = GetOrAdd<HorizontalLayoutGroup>(go);
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.spacing = 6;
        hlg.childAlignment = TextAnchor.MiddleLeft;
    }

    private static void AddImage(GameObject go, Color color)
    {
        GetOrAdd<CanvasRenderer>(go);
        var img = GetOrAdd<Image>(go);
        img.color = color;
    }

    private static void AddDarkImage(GameObject go, Color color)
    {
        GetOrAdd<CanvasRenderer>(go);
        var img = GetOrAdd<Image>(go);
        img.color = color;
    }

    private static void SetLayoutElement(GameObject go,
        float preferredWidth = -1, float preferredHeight = -1,
        float flexibleWidth = -1, float flexibleHeight = -1)
    {
        var le = GetOrAdd<LayoutElement>(go);
        if (preferredWidth >= 0) le.preferredWidth = preferredWidth;
        if (preferredHeight >= 0) le.preferredHeight = preferredHeight;
        if (flexibleWidth >= 0) le.flexibleWidth = flexibleWidth;
        if (flexibleHeight >= 0) le.flexibleHeight = flexibleHeight;
    }

    private static T GetOrAdd<T>(GameObject go) where T : Component
    {
        var comp = go.GetComponent<T>();
        if (comp == null) comp = go.AddComponent<T>();
        return comp;
    }
}
#endif
