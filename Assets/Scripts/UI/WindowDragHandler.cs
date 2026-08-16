using UnityEngine;
using UnityEngine.EventSystems;

public sealed class WindowDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    [SerializeField] private RectTransform targetWindow;
    private Canvas canvas;

    public void Configure(RectTransform window, Canvas parentCanvas)
    {
        targetWindow = window;
        canvas = parentCanvas;
    }

    public void OnBeginDrag(PointerEventData eventData) { }

    public void OnDrag(PointerEventData eventData)
    {
        if (targetWindow == null || canvas == null) return;
        targetWindow.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }
}
