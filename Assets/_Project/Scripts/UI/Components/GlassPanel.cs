using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[ExecuteAlways]
public class GlassPanel : MonoBehaviour, IMaterialModifier
{
    [SerializeField] private Material _glassMaterial;
    [SerializeField] private Color _tintColor = new Color(0.118f, 0.118f, 0.118f, 0.75f);
    [SerializeField] private Color _borderColor = new Color(0.5f, 0.5f, 0.5f, 0.4f);
    [SerializeField] private float _cornerRadius = 16f;
    [SerializeField] private float _borderWidth = 1.5f;

    private Image _image;
    private Material _modifiedMaterial;

    private static readonly int TintColorID = Shader.PropertyToID("_TintColor");
    private static readonly int BorderColorID = Shader.PropertyToID("_BorderColor");
    private static readonly int CornerRadiusID = Shader.PropertyToID("_CornerRadius");
    private static readonly int BorderWidthID = Shader.PropertyToID("_BorderWidth");
    private static readonly int RectSizeID = Shader.PropertyToID("_RectSize");

    public Color TintColor
    {
        get => _tintColor;
        set { _tintColor = value; SetDirty(); }
    }

    public Color BorderColor
    {
        get => _borderColor;
        set { _borderColor = value; SetDirty(); }
    }

    public float CornerRadius
    {
        get => _cornerRadius;
        set { _cornerRadius = value; SetDirty(); }
    }

    private void Awake()
    {
        _image = GetComponent<Image>();
    }

    private void OnEnable()
    {
        if (_image == null)
            _image = GetComponent<Image>();

        if (_glassMaterial != null)
            _image.material = _glassMaterial;

        SetDirty();
    }

    private void OnDisable()
    {
        CleanupMaterial();
    }

    private void OnDestroy()
    {
        CleanupMaterial();
    }

    private void OnRectTransformDimensionsChange()
    {
        SetDirty();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_image == null)
            _image = GetComponent<Image>();

        if (_glassMaterial != null)
            _image.material = _glassMaterial;

        SetDirty();
    }
#endif

    public Material GetModifiedMaterial(Material baseMaterial)
    {
        if (!isActiveAndEnabled || _glassMaterial == null)
            return baseMaterial;

        if (_modifiedMaterial == null || _modifiedMaterial.shader != baseMaterial.shader)
        {
            CleanupMaterial();
            _modifiedMaterial = new Material(baseMaterial);
            _modifiedMaterial.hideFlags = HideFlags.HideAndDontSave;
        }

        // Get current rect size for SDF calculation
        RectTransform rt = transform as RectTransform;
        Vector2 rectSize = rt != null ? rt.rect.size : new Vector2(300, 200);

        _modifiedMaterial.SetColor(TintColorID, _tintColor);
        _modifiedMaterial.SetColor(BorderColorID, _borderColor);
        _modifiedMaterial.SetFloat(CornerRadiusID, _cornerRadius);
        _modifiedMaterial.SetFloat(BorderWidthID, _borderWidth);
        _modifiedMaterial.SetVector(RectSizeID, new Vector4(rectSize.x, rectSize.y, 0, 0));

        return _modifiedMaterial;
    }

    public void ApplyTheme(UITheme theme)
    {
        if (theme == null) return;
        _tintColor = theme.panelBackground;
        _borderColor = theme.panelBorder;
        _cornerRadius = theme.cornerRadius;
        _borderWidth = theme.borderWidth;
        SetDirty();
    }

    private void SetDirty()
    {
        if (_image != null)
            _image.SetMaterialDirty();
    }

    private void CleanupMaterial()
    {
        if (_modifiedMaterial != null)
        {
            if (Application.isPlaying)
                Destroy(_modifiedMaterial);
            else
                DestroyImmediate(_modifiedMaterial);
            _modifiedMaterial = null;
        }
    }
}
