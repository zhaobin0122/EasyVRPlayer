using UnityEngine;
using UnityEngine.EventSystems;
using HVRCORE;
public class HVREventListener : UnityEngine.EventSystems.EventTrigger, IHVRHoverHandle
{
    private static readonly string TAG = "Unity_HVREventListener";

    public delegate void VoidDelegate(GameObject go);
    public delegate void EventDelegate(GameObject go, PointerEventData eventData);
    public delegate void AxisEventDelegate(GameObject go, AxisEventData eventData);
    public VoidDelegate onEnter;
    public VoidDelegate onClick;
    public VoidDelegate onDown;
    public VoidDelegate onUp;
    public VoidDelegate onExit;
    public VoidDelegate onBeginDrag;
    public EventDelegate onDrag;
    public EventDelegate onDrop;
    public VoidDelegate onEndDrag;
    public VoidDelegate onUpdateSelectObj;

    public VoidDelegate onHover;

    public AxisEventDelegate onMove;

    static public HVREventListener Get(GameObject go)
    {
        HVREventListener listener = go.GetComponent<HVREventListener>();
        if (listener == null)
        {
            listener = go.AddComponent<HVREventListener>();

        }
        return listener;
    }

    public void OnHvrPointerHover(PointerEventData eventData)
    {
        if (onHover != null)
        {
            onHover(gameObject);
        }
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (onEnter != null)
        {
            onEnter(gameObject);
        }
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        if (onExit != null)
        {
            onExit(gameObject);
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (onDown != null)
        {
            onDown(gameObject);
        }
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if (onUp != null)
        {
            onUp(gameObject);
        }
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (onClick != null)
        {
            onClick(gameObject);
        }
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (onBeginDrag != null)
        {
            onBeginDrag(gameObject);
        }
    }
    private RectTransform canvas;


    public override void OnEndDrag(PointerEventData eventData)
    {
        if (onEndDrag != null)
        {
            onEndDrag(gameObject);
        }
    }


    public override void OnDrag(PointerEventData eventData)
    {
        if (onDrag != null)
        {
            onDrag(gameObject, eventData);
        }
    }

    public override void OnDrop(PointerEventData eventData)
    {
        if (onDrop != null)
        {
            onDrop(gameObject, eventData);
        }
    }

    public override void OnMove(AxisEventData eventData)
    {
        if (onMove != null)
        {
            onMove(gameObject, eventData);
            //HVRLogCore.LOGI(TAG, "movedata: " + eventData.moveVector + " derection: " + eventData.moveDir);
        }
    }

    public override void OnUpdateSelected(BaseEventData eventData)
    {
        if (onUpdateSelectObj != null)
        {
            onUpdateSelectObj(gameObject);
        }
    }
}
