using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class Slot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Visual")]
    [SerializeField] private Image _highlightImage;
    [SerializeField] private TextMeshProUGUI _slotNumber;
    [SerializeField] private Color _highlightColor = new Color(0f, 0.66f, 0.91f, 0.3f);

    private int _slotIndex;

    public int SlotIndex
    {
        get => _slotIndex;
        set
        {
            _slotIndex = value;
            if (_slotNumber != null)
                _slotNumber.text = value.ToString("D2");
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (transform.childCount == 0)
        {
            GameObject dropped = eventData.pointerDrag;
            DraggableItem draggableItem = dropped.GetComponent<DraggableItem>();
            draggableItem.ParentAfterDrag = transform;
        }

        SetHighlight(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
            SetHighlight(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetHighlight(false);
    }

    private void SetHighlight(bool active)
    {
        if (_highlightImage != null)
        {
            _highlightImage.color = active
                ? _highlightColor
                : new Color(_highlightColor.r, _highlightColor.g, _highlightColor.b, 0f);
        }
    }
}
