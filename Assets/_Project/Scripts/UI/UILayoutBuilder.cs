using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UILayoutBuilder : MonoBehaviour
{
    [Header("Theme")]
    [SerializeField] private UITheme _theme;
    [SerializeField] private Material _glassMaterial;

    [Header("Existing Panels (assign in Inspector)")]
    [SerializeField] private RectTransform _hudLayer;
    [SerializeField] private RectTransform _workPanel;
    [SerializeField] private RectTransform _paletteBlocksPanel;
    [SerializeField] private RectTransform _controlUnitPanel;

    [Header("Prefabs")]
    [SerializeField] private GameObject _slotPrefab;

    [Header("Satellite Reference")]
    [SerializeField] private Satellite _satellite;

    // Created panels
    private RectTransform _missionPanel;
    private RectTransform _modulePanel;
    private RectTransform _commandPanel;
    private RectTransform _controlPanel;
    private RectTransform _orbitControlPanel;

    // Status components
    private StatusIndicator _signalStatus;
    private ProgressBar _sunSensorBar;

    public void Initialize(UITheme theme)
    {
        _theme = theme;
        BuildUI();
    }

    private void Start()
    {
        if (_theme != null)
            BuildUI();
    }

    private void BuildUI()
    {
        CreateMissionPanel();
        ReconfigureCommandPanel();
        ReconfigureModulePanel();
        ReconfigureControlPanel();
        CreateOrbitControlPanel();
        ApplyGlassToAllPanels();
    }

    // ===== MISSION PANEL (NEW — top-right) =====
    private void CreateMissionPanel()
    {
        _missionPanel = CreatePanel("MissionPanel", _hudLayer);

        // Anchor top-right
        SetAnchors(_missionPanel, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1));
        _missionPanel.anchoredPosition = new Vector2(-20, -20);
        _missionPanel.sizeDelta = new Vector2(400, 0); // height auto via ContentSizeFitter

        // Layout
        var vlg = _missionPanel.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(
            (int)_theme.panelPadding, (int)_theme.panelPadding,
            (int)_theme.panelPadding, (int)_theme.panelPadding
        );
        vlg.spacing = 12;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;

        var csf = _missionPanel.gameObject.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Header
        CreateHeader(_missionPanel, "MISSION OBJECTIVE");

        // Mission description
        var bodyGO = CreateTextElement(_missionPanel, "MissionBody",
            "Monitor satellite orientation and execute command sequence for optimal solar panel alignment.",
            _theme.bodyFont, _theme.bodySize, _theme.textSecondary);

        // Signal status
        var statusGO = new GameObject("SignalStatus", typeof(RectTransform));
        statusGO.transform.SetParent(_missionPanel, false);
        var statusRT = statusGO.GetComponent<RectTransform>();

        // Status layout row
        var statusHLG = statusGO.AddComponent<HorizontalLayoutGroup>();
        statusHLG.spacing = 8;
        statusHLG.childForceExpandWidth = false;
        statusHLG.childForceExpandHeight = true;
        statusHLG.childControlWidth = true;
        statusHLG.childControlHeight = true;

        var statusLE = statusGO.AddComponent<LayoutElement>();
        statusLE.preferredHeight = 24;

        // Dot
        var dotGO = new GameObject("Dot", typeof(RectTransform), typeof(Image));
        dotGO.transform.SetParent(statusRT, false);
        var dotImage = dotGO.GetComponent<Image>();
        dotImage.color = _theme.statusOnline;
        var dotLE = dotGO.AddComponent<LayoutElement>();
        dotLE.preferredWidth = 10;
        dotLE.preferredHeight = 10;

        // Status text
        var statusTextGO = new GameObject("StatusText", typeof(RectTransform), typeof(TextMeshProUGUI));
        statusTextGO.transform.SetParent(statusRT, false);
        var statusTMP = statusTextGO.GetComponent<TextMeshProUGUI>();
        statusTMP.text = "SIGNAL: ONLINE";
        statusTMP.font = _theme.bodyFont != null ? _theme.bodyFont : _theme.headerFont;
        statusTMP.fontSize = _theme.captionSize;
        statusTMP.color = _theme.statusOnline;

        _signalStatus = statusGO.AddComponent<StatusIndicator>();
    }

    // ===== COMMAND PANEL (repurpose Work Panel — right side) =====
    private void ReconfigureCommandPanel()
    {
        if (_workPanel == null) return;

        _commandPanel = _workPanel;
        _commandPanel.name = "CommandPanel";

        // Anchor right, vertically stretched
        SetAnchors(_commandPanel, new Vector2(1, 0.1f), new Vector2(1, 0.85f), new Vector2(1, 0.5f));
        _commandPanel.anchoredPosition = new Vector2(-20, 0);
        _commandPanel.sizeDelta = new Vector2(320, 0);

        // Clear existing children layout if needed, keep content
        var existingVLG = _commandPanel.GetComponent<VerticalLayoutGroup>();
        if (existingVLG == null)
        {
            existingVLG = _commandPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            existingVLG.padding = new RectOffset(
                (int)_theme.panelPadding, (int)_theme.panelPadding,
                (int)_theme.panelPadding, (int)_theme.panelPadding
            );
            existingVLG.spacing = 6;
            existingVLG.childForceExpandWidth = true;
            existingVLG.childForceExpandHeight = false;
            existingVLG.childControlWidth = true;
            existingVLG.childControlHeight = true;
        }

        // Add header at top (insert as first child)
        var header = CreateHeader(_commandPanel, "COMMAND SEQUENCE");
        header.transform.SetAsFirstSibling();

        // Ensure ScrollRect for content below header
        EnsureScrollRect(_commandPanel);
    }

    // ===== MODULE PANEL (repurpose PaletteBlocks — center-right) =====
    private void ReconfigureModulePanel()
    {
        if (_paletteBlocksPanel == null) return;

        _modulePanel = _paletteBlocksPanel;
        _modulePanel.name = "ModulePanel";

        // Anchor center-right
        SetAnchors(_modulePanel, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f));
        _modulePanel.anchoredPosition = new Vector2(-360, 80);
        _modulePanel.sizeDelta = new Vector2(400, 500);
    }

    // ===== CONTROL PANEL (repurpose ControlUnit — bottom-left) =====
    private void ReconfigureControlPanel()
    {
        if (_controlUnitPanel == null) return;

        _controlPanel = _controlUnitPanel;
        _controlPanel.name = "ControlPanel";

        // Anchor bottom-left
        SetAnchors(_controlPanel, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0));
        _controlPanel.anchoredPosition = new Vector2(20, 20);
        _controlPanel.sizeDelta = new Vector2(0, 60);

        // Ensure HorizontalLayoutGroup
        var hlg = _controlPanel.GetComponent<HorizontalLayoutGroup>();
        if (hlg == null)
        {
            hlg = _controlPanel.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.padding = new RectOffset(12, 12, 8, 8);
        }

        var csf = _controlPanel.GetComponent<ContentSizeFitter>();
        if (csf == null)
        {
            csf = _controlPanel.gameObject.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        // Style existing buttons
        StyleExistingButtons(_controlPanel);
    }

    // ===== ORBIT CONTROL PANEL (NEW — bottom-left, above ControlPanel) =====
    private void CreateOrbitControlPanel()
    {
        _orbitControlPanel = CreatePanel("OrbitControlPanel", _hudLayer);

        SetAnchors(_orbitControlPanel, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0));
        _orbitControlPanel.anchoredPosition = new Vector2(20, 100);
        _orbitControlPanel.sizeDelta = new Vector2(200, 160);

        var vlg = _orbitControlPanel.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(10, 10, 10, 10);
        vlg.spacing = 4;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = true;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;

        // Header label
        var headerLE = CreateSmallLabel(_orbitControlPanel, "ORBIT CONTROL");
        headerLE.GetComponent<LayoutElement>().preferredHeight = 20;

        // Row 1: Up button centered
        var row1 = CreateLayoutRow(_orbitControlPanel, "Row1");
        CreateOrbitButton(row1.transform, "\u2191", "RotateUpBtn");

        // Row 2: Left, Reset, Right
        var row2 = CreateLayoutRow(_orbitControlPanel, "Row2");
        CreateOrbitButton(row2.transform, "\u2190", "RotateLeftBtn");
        CreateOrbitButton(row2.transform, "\u21BB", "ResetBtn");
        CreateOrbitButton(row2.transform, "\u2192", "RotateRightBtn");

        // Row 3: Down button centered
        var row3 = CreateLayoutRow(_orbitControlPanel, "Row3");
        CreateOrbitButton(row3.transform, "\u2193", "RotateDownBtn");

        // Wire buttons to satellite if available
        WireOrbitButtons();
    }

    // ===== APPLY GLASS TO ALL PANELS =====
    private void ApplyGlassToAllPanels()
    {
        ApplyGlass(_missionPanel);
        ApplyGlass(_commandPanel);
        ApplyGlass(_modulePanel);
        ApplyGlass(_controlPanel);
        ApplyGlass(_orbitControlPanel);
    }

    private void ApplyGlass(RectTransform panel)
    {
        if (panel == null) return;

        var image = panel.GetComponent<Image>();
        if (image == null)
            image = panel.gameObject.AddComponent<Image>();

        image.color = Color.white; // shader handles tinting

        var glass = panel.GetComponent<GlassPanel>();
        if (glass == null)
            glass = panel.gameObject.AddComponent<GlassPanel>();

        glass.ApplyTheme(_theme);
    }

    // ===== HELPER METHODS =====

    private RectTransform CreatePanel(string name, RectTransform parent)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        go.layer = LayerMask.NameToLayer("UI");
        return go.GetComponent<RectTransform>();
    }

    private GameObject CreateHeader(RectTransform parent, string text)
    {
        var go = new GameObject("Header", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);

        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.font = _theme.headerFont;
        tmp.fontSize = _theme.headerSize;
        tmp.color = _theme.textPrimary;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Left;

        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = _theme.headerSize + 8;

        return go;
    }

    private GameObject CreateTextElement(RectTransform parent, string name, string text,
        TMP_FontAsset font, float size, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);

        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.font = font != null ? font : _theme.headerFont;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.TopLeft;
        tmp.enableWordWrapping = true;

        return go;
    }

    private GameObject CreateSmallLabel(RectTransform parent, string text)
    {
        var go = CreateTextElement(parent, "Label", text,
            _theme.bodyFont, _theme.captionSize, _theme.textMuted);

        var le = go.GetComponent<LayoutElement>();
        if (le == null) le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 20;

        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.UpperCase;

        return go;
    }

    private RectTransform CreateLayoutRow(RectTransform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var hlg = go.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 4;
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = true;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childAlignment = TextAnchor.MiddleCenter;

        return go.GetComponent<RectTransform>();
    }

    private void CreateOrbitButton(Transform parent, string label, string name)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer),
            typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        go.layer = LayerMask.NameToLayer("UI");

        var image = go.GetComponent<Image>();
        image.color = _theme.buttonNormal;

        var button = go.GetComponent<Button>();
        button.transition = Selectable.Transition.None;

        // Add hover effect
        var hover = go.AddComponent<HoverEffect>();
        hover.ApplyTheme(_theme);

        // Label
        var textGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGO.transform.SetParent(go.transform, false);

        var textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.sizeDelta = Vector2.zero;

        var tmp = textGO.GetComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.font = _theme.headerFont;
        tmp.fontSize = 18;
        tmp.color = _theme.textPrimary;
        tmp.alignment = TextAlignmentOptions.Center;

        var le = go.AddComponent<LayoutElement>();
        le.preferredWidth = 50;
        le.preferredHeight = 36;
    }

    private void WireOrbitButtons()
    {
        if (_satellite == null) return;

        WireButton(_orbitControlPanel, "RotateUpBtn", _satellite.RotateUp);
        WireButton(_orbitControlPanel, "RotateDownBtn", _satellite.RotateDown);
        WireButton(_orbitControlPanel, "RotateLeftBtn", _satellite.RotateLeft);
        WireButton(_orbitControlPanel, "RotateRightBtn", _satellite.RotateRight);
        WireButton(_orbitControlPanel, "ResetBtn", _satellite.Reset);
    }

    private void WireButton(RectTransform parent, string buttonName, UnityEngine.Events.UnityAction action)
    {
        var btnTransform = parent.Find(buttonName) ??
                           FindDeep(parent, buttonName);

        if (btnTransform == null) return;

        var button = btnTransform.GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(action);
    }

    private Transform FindDeep(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            var result = FindDeep(child, name);
            if (result != null) return result;
        }
        return null;
    }

    private void StyleExistingButtons(RectTransform panel)
    {
        foreach (Transform child in panel)
        {
            var button = child.GetComponent<Button>();
            if (button == null) continue;

            // Add hover effect
            var hover = child.GetComponent<HoverEffect>();
            if (hover == null)
            {
                hover = child.gameObject.AddComponent<HoverEffect>();
                hover.ApplyTheme(_theme);
            }

            // Style button image
            var image = child.GetComponent<Image>();
            if (image != null)
                image.color = _theme.buttonNormal;

            // Style button text
            var tmp = child.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.font = _theme.bodyFont != null ? _theme.bodyFont : _theme.headerFont;
                tmp.fontSize = _theme.bodySize;
                tmp.color = _theme.textPrimary;
            }

            // Add LayoutElement for consistent sizing
            var le = child.GetComponent<LayoutElement>();
            if (le == null)
            {
                le = child.gameObject.AddComponent<LayoutElement>();
                le.preferredWidth = 100;
                le.preferredHeight = 44;
            }
        }
    }

    private void EnsureScrollRect(RectTransform panel)
    {
        var scrollRect = panel.GetComponent<ScrollRect>();
        if (scrollRect != null) return; // already has scroll

        // Check if there's a child that acts as viewport
        var viewport = panel.Find("Viewport");
        if (viewport == null) return;

        scrollRect = panel.gameObject.AddComponent<ScrollRect>();
        scrollRect.viewport = viewport as RectTransform;
        scrollRect.vertical = true;
        scrollRect.horizontal = false;
        scrollRect.movementType = ScrollRect.MovementType.Elastic;
        scrollRect.scrollSensitivity = 30;

        var content = viewport.Find("Content");
        if (content != null)
            scrollRect.content = content as RectTransform;
    }

    private void SetAnchors(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
    {
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
    }
}
