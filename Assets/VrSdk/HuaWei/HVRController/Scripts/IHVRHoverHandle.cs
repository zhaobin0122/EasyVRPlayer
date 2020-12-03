using UnityEngine.EventSystems;

public interface IHVRHoverHandle : IEventSystemHandler
{
    void OnHvrPointerHover(PointerEventData eventData);
}
