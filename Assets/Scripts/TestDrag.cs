
using UnityEngine;
using UnityEngine.EventSystems;

public class TestDrag : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField]
    protected RectTransform Dragged;
    protected Vector3 AnchorGap;

    public void OnPointerDown(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            Dragged,
            eventData.position,
            eventData.pressEventCamera,
            out var worldPoint
            );
        AnchorGap = Dragged.position - worldPoint;
        Dragged.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
            Dragged,
            eventData.position,
            eventData.pressEventCamera,
            out var worldPoint))
        {
            Dragged.position = worldPoint + AnchorGap;
        }
    }
}
