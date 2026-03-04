using UnityEngine;
using UnityEngine.UI;

public class PaletteLoader : MonoBehaviour
{
    public SatellitePalette palette; // Привяжи asset через инспектор

    void Start()
    {
        foreach (var block in palette.Blocks)
        {
            // Создай кнопку в UI для каждого блока
            // GameObject button = CreateButton(block.DisplayName, block.icon);
        }
    }
}