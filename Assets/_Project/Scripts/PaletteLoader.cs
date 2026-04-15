using UnityEngine;
using UnityEngine.UI;

public class PaletteLoader : MonoBehaviour
{
    [SerializeField] private SatellitePalette palette;
    [SerializeField] private GameObject blockPrefab;
    [SerializeField] private Transform container;

    void Start()
    {
        if (palette == null || blockPrefab == null || container == null) return;

        for (int i = 0; i < palette.Blocks.Count; i++)
        {
            var block = palette.Blocks[i];
            var instance = Instantiate(blockPrefab, container);
            var draggable = instance.GetComponent<DraggableItem>();
            if (draggable != null)
            {
                draggable.SetBlockData(block);
                draggable.SetIndex(i + 1);
            }
        }
    }
}