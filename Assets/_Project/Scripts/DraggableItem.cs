using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public CanvasGroup CanvasGroup; 

    public Image Image;
    public Transform ParentAfterDrag 
    { 
        get => _parentAfterDrag;
        set => _parentAfterDrag = value; 
    }
    
    private Transform _parentAfterDrag;
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Begin drag");
        _parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        
        CanvasGroup.blocksRaycasts = false; 
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("Dragging"); 
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("End drag");    
        transform.SetParent(_parentAfterDrag);
        
        CanvasGroup.blocksRaycasts = true; 
    }
}
