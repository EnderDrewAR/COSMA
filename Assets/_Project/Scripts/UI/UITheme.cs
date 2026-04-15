using UnityEngine;
using TMPro;

[CreateAssetMenu(fileName = "UITheme", menuName = "COSMA/UI Theme")]
public class UITheme : ScriptableObject
{
    [Header("Panel Colors")]
    public Color panelBackground = new Color(0.118f, 0.118f, 0.118f, 0.75f);
    public Color panelBorder = new Color(0.5f, 0.5f, 0.5f, 0.4f);

    [Header("Accent Colors")]
    public Color accentOrange = new Color(1f, 0.42f, 0.21f, 1f);
    public Color accentBlue = new Color(0f, 0.66f, 0.91f, 1f);
    public Color accentGreen = new Color(0.2f, 0.9f, 0.4f, 1f);
    public Color accentRed = new Color(0.9f, 0.2f, 0.2f, 1f);

    [Header("Text Colors")]
    public Color textPrimary = Color.white;
    public Color textSecondary = new Color(0.7f, 0.7f, 0.7f, 1f);
    public Color textMuted = new Color(0.5f, 0.5f, 0.5f, 1f);

    [Header("Button Colors")]
    public Color buttonNormal = new Color(0.15f, 0.15f, 0.15f, 0.8f);
    public Color buttonHover = new Color(0.25f, 0.25f, 0.25f, 0.9f);
    public Color buttonActive = new Color(0.1f, 0.1f, 0.1f, 0.95f);
    public Color buttonDisabled = new Color(0.1f, 0.1f, 0.1f, 0.4f);

    [Header("Status Colors")]
    public Color statusOnline = new Color(0.2f, 0.9f, 0.4f, 1f);
    public Color statusOffline = new Color(0.9f, 0.2f, 0.2f, 1f);
    public Color statusWarning = new Color(1f, 0.8f, 0f, 1f);

    [Header("Typography")]
    public TMP_FontAsset headerFont;
    public TMP_FontAsset bodyFont;
    public TMP_FontAsset lightFont;
    public float headerSize = 20f;
    public float bodySize = 14f;
    public float captionSize = 11f;

    [Header("Layout")]
    public float cornerRadius = 16f;
    public float borderWidth = 1.5f;
    public float panelPadding = 16f;
    public float elementSpacing = 8f;
}
