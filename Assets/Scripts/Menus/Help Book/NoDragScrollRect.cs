using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Disables click-and-drag functionality on base scroll rect implementation.
/// </summary>
public class NoDragScrollRect : ScrollRect
{
    public override void OnBeginDrag(PointerEventData eventData) { }
    public override void OnDrag(PointerEventData eventData) { }
    public override void OnEndDrag(PointerEventData eventData) { }
}
