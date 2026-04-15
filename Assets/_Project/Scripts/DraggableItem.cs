using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public CanvasGroup CanvasGroup;

    public Image Image;

    [Header("Visual Feedback")]
    [SerializeField] private TextMeshProUGUI _indexLabel;
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _nameLabel;
    [SerializeField] private float _dragScale = 1.05f;
    [SerializeField] private float _dragAlpha = 0.85f;

    public Transform ParentAfterDrag
    {
        get => _parentAfterDrag;
        set => _parentAfterDrag = value;
    }

    private Transform _parentAfterDrag;
    private Vector3 _originalScale;

    private void Awake()
    {
        _originalScale = transform.localScale;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();

        CanvasGroup.blocksRaycasts = false;
        CanvasGroup.alpha = _dragAlpha;
        transform.localScale = _originalScale * _dragScale;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(_parentAfterDrag);

        CanvasGroup.blocksRaycasts = true;
        CanvasGroup.alpha = 1f;
        transform.localScale = _originalScale;
    }

    public void SetBlockData(BlockData data)
    {
        if (data == null) return;

        if (_nameLabel != null)
            _nameLabel.text = data.DisplayName;

        if (_iconImage != null && data.icon != null)
        {
            _iconImage.sprite = data.icon;
            _iconImage.enabled = true;
        }
    }

    public void SetIndex(int index)
    {
        if (_indexLabel != null)
            _indexLabel.text = index.ToString("D2");
    }
}
