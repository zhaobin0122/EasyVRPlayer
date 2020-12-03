using UnityEngine.EventSystems;

/// This script extends the standard Unity EventSystem events with Gvr specific events.
public static class HVRExecuteEventsExtension {
  private static readonly ExecuteEvents.EventFunction<IHVRHoverHandle> s_HoverHandler = Execute;

  private static void Execute(IHVRHoverHandle handler, BaseEventData eventData) {
        handler.OnHvrPointerHover(ExecuteEvents.ValidateEventData<PointerEventData>(eventData));
  }

  public static ExecuteEvents.EventFunction<IHVRHoverHandle> pointerHoverHandler {
        
        get
        {
            return s_HoverHandler;
        }
  }
}
