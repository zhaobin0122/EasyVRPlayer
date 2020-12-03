using UnityEngine;
using UnityEngine.EventSystems;
using HVRCORE;
public class HVRInputModule : BaseInputModule
{

    private PointerEventData m_PointerEventData;
    private Camera m_UiCamera;
    private GameObject m_CurrentDragging;
    private GameObject m_ClickedDownObj;
    private GameObject m_LastGazeObj;
    private GameObject m_NowGazeObj;
    private Vector2 m_TouchDownPos;
    private Vector2 m_TouchUpPos;
    private Vector2 m_DifferPos;
    private static Vector3 m_MousePosition;
    private static float m_MoveMagnitude = 3.0f;

    private HVRLinePointer m_LinePointer;

    public override void Process()
    {
        if (HVRController.m_RightEventCamera == null) {
            return;
        }
        m_UiCamera = HVRController.m_RightEventCamera.GetComponent<Camera>();
        PointerEventData eventData = GetResultByGaze();
        if (eventData == null)
        {
            return;
        }
        if (eventData.enterEventCamera != null)
        {
            m_LinePointer = eventData.enterEventCamera.GetComponent<HVRLinePointer>();
        }

        UpdateCurrentObject();

        Camera camera = this.m_PointerEventData.enterEventCamera;
        m_LastGazeObj = m_NowGazeObj;
        m_NowGazeObj = eventData.pointerCurrentRaycast.gameObject;

        UpdateAnchorPos(m_LastGazeObj);

        if (m_NowGazeObj == null && m_NowGazeObj != m_LastGazeObj)
        {
            eventSystem.SetSelectedGameObject(null);
        }
        if (Application.platform == RuntimePlatform.Android)
        {
            if (IsRightControllerButtonDown() || IsLeftControllerButtonDown())
            {
                this.HandleTrigger();
            }
            else if (IsRightControllerButtonUp() || IsLeftControllerButtonUp())
            {
                this.HandlePendingClick();
            }
        }
        else if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            if (Input.GetMouseButtonDown(0))
            {
                this.HandleTrigger();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                this.HandlePendingClick();
            }
        }

        ProcessDrag(eventData);
        ProcessHover(m_LastGazeObj, eventData);
        ProcessMove(m_PointerEventData);
    }

    private bool IsLeftControllerButtonDown()
    {
        if (HVRController.m_LeftController == null || !HVRController.m_LeftController.IsAvailable())
        {
            return false;
        }
        if ((HVRController.m_LeftController.IsButtonDown(ButtonType.ButtonConfirm) || HVRController.m_LeftController.IsButtonDown(ButtonType.ButtonTrigger)))
        {
            return true;
        }
        return false;
    }

    private bool IsLeftControllerButtonUp()
    {
        if (HVRController.m_LeftController == null || !HVRController.m_LeftController.IsAvailable())
        {
            return false;
        }
        if ((HVRController.m_LeftController.IsButtonUp(ButtonType.ButtonConfirm) || HVRController.m_LeftController.IsButtonUp(ButtonType.ButtonTrigger)))
        {
            return true;
        }
        return false;
    }
    private bool IsRightControllerButtonDown()
    {
        if (HVRController.m_RightController == null || !HVRController.m_RightController.IsAvailable())
        {
            return false;
        }
        if ((HVRController.m_RightController.IsButtonDown(ButtonType.ButtonConfirm) || HVRController.m_RightController.IsButtonDown(ButtonType.ButtonTrigger)))
        {
            return true;
        }
        return false;
    }

    private bool IsRightControllerButtonUp()
    {
        if (HVRController.m_RightController == null || !HVRController.m_RightController.IsAvailable())
        {
            return false;
        }
        if ((HVRController.m_RightController.IsButtonUp(ButtonType.ButtonConfirm) || HVRController.m_RightController.IsButtonUp(ButtonType.ButtonTrigger)))
        {
            return true;
        }
        return false;
    }

    private GameObject GetCurrentGameObject()
    {
        if (this.m_PointerEventData != null && this.m_PointerEventData.enterEventCamera != null)
        {
            return this.m_PointerEventData.pointerCurrentRaycast.gameObject;
        }

        return null;
    }

    /// <summary>
    /// Obtain the first object by ray
    /// </summary>
    private PointerEventData GetResultByGaze()
    {
        Vector2 pointerPosition;
        pointerPosition.x = m_UiCamera.pixelWidth / 2;
        pointerPosition.y = m_UiCamera.pixelHeight / 2;
        if (m_PointerEventData == null)
        {
            m_PointerEventData = new PointerEventData(this.eventSystem);
        }
        m_PointerEventData.Reset();
        // m_PointerEventData.delta = Vector2.zero;
        m_PointerEventData.position = pointerPosition;
        // m_PointerEventData.scrollDelta = Vector2.zero;

        eventSystem.RaycastAll(m_PointerEventData, m_RaycastResultCache);
        RaycastResult raycastResult = FindFirstRaycast(m_RaycastResultCache);

        if (raycastResult.gameObject != null && raycastResult.worldPosition == Vector3.zero)
        {
            raycastResult.worldPosition = this.GetIntersectionPosition(this.m_PointerEventData.enterEventCamera, raycastResult);

        }

        m_PointerEventData.pointerCurrentRaycast = raycastResult;

        m_RaycastResultCache.Clear();
        return m_PointerEventData;
    }

    private void UpdateCurrentObject()
    {

        GameObject target = m_PointerEventData.pointerCurrentRaycast.gameObject;

        HandlePointerExitAndEnter(m_PointerEventData, target); //Controller sending enter and exit events when a new enter target is found.

        GameObject select = ExecuteEvents.GetEventHandler<ISelectHandler>(target);

        if (select == eventSystem.currentSelectedGameObject)
        {
            ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, GetBaseEventData(), ExecuteEvents.updateSelectedHandler);
        }
        else
        {
            eventSystem.SetSelectedGameObject(null, m_PointerEventData);
        }
    }

    private void UpdateAnchorPos(GameObject previousGazedObject)
    {
        if (m_LinePointer == null)
        {
            return;
        }
        GameObject currentGazeObject = this.GetCurrentGameObject();

        Vector3 intersectionPosition = this.m_PointerEventData.pointerCurrentRaycast.worldPosition;
        bool isInteractive = this.m_PointerEventData.pointerPress != null ||
                             ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentGazeObject) != null;

        // Hack here,use remote to control pointer
        if (currentGazeObject == previousGazedObject)
        {
            if (currentGazeObject != null)
            {
                m_LinePointer.OnLineHover(intersectionPosition, true);
            }
        }
        else
        {
            if (previousGazedObject != null)
            {
                m_LinePointer.OnLineExit(intersectionPosition, false);
            }

            if (currentGazeObject != null)
            {
                m_LinePointer.OnLineEnter(intersectionPosition, true);
            }
        }
    }

    private void HandlePendingClick()
    {
        if (!this.m_PointerEventData.eligibleForClick)
        {
            return;
        }

        GameObject mhitObject = this.m_PointerEventData.pointerCurrentRaycast.gameObject;

        if (m_CurrentDragging)
        {
            ExecuteEvents.Execute(m_CurrentDragging, this.m_PointerEventData, ExecuteEvents.endDragHandler);
            // if (hitObject != null)  {   }

            ExecuteEvents.ExecuteHierarchy(m_CurrentDragging, m_PointerEventData, ExecuteEvents.dropHandler);

            m_PointerEventData.pointerDrag = null;
            m_CurrentDragging = null;
        }

        if (m_ClickedDownObj)
        {
            m_MousePosition -= Input.mousePosition;
            if (Application.platform == RuntimePlatform.Android)
            {
                if (m_LinePointer != null)
                {
                    if (m_LinePointer.controllerIndex == ControllerIndex.RIGHT_CONTROLLER)
                    {
                        HVRController.m_RightController.GetTouchpadTouchPos(ref m_TouchUpPos);
                    }
                    else
                    {
                        HVRController.m_LeftController.GetTouchpadTouchPos(ref m_TouchUpPos);
                    }
                }

                m_DifferPos = m_TouchDownPos - m_TouchUpPos;
            }
            else if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                m_DifferPos = Vector2.zero;
            }

            GameObject clickedUpObj = ExecuteEvents.ExecuteHierarchy(m_ClickedDownObj, this.m_PointerEventData, ExecuteEvents.pointerUpHandler);
            if (m_ClickedDownObj == clickedUpObj && (m_MousePosition.magnitude < m_MoveMagnitude) && (m_DifferPos.magnitude < 0.1f))
            {
                ExecuteEvents.Execute(m_ClickedDownObj, this.m_PointerEventData, ExecuteEvents.pointerClickHandler);
            }
            else
            {
                ExecuteEvents.Execute(m_ClickedDownObj, this.m_PointerEventData, ExecuteEvents.pointerUpHandler);
            }

            this.m_PointerEventData.pressPosition = Vector3.zero;
            this.m_PointerEventData.pointerPress = null;
            this.m_PointerEventData.rawPointerPress = null;
            this.m_PointerEventData.eligibleForClick = false;
            this.m_PointerEventData.clickCount = 0;
            m_ClickedDownObj = null;
            this.m_PointerEventData.useDragThreshold = false;
        }
    }

    private void HandleTrigger()
    {
        GameObject mtriggerObject = this.m_PointerEventData.pointerCurrentRaycast.gameObject;

        this.m_PointerEventData.pressPosition = this.m_PointerEventData.position;
        this.m_PointerEventData.pointerPressRaycast = this.m_PointerEventData.pointerCurrentRaycast;

        this.m_PointerEventData.pointerPress = ExecuteEvents.ExecuteHierarchy(mtriggerObject, this.m_PointerEventData, ExecuteEvents.pointerDownHandler)
            ?? ExecuteEvents.GetEventHandler<IPointerClickHandler>(mtriggerObject);

        if (Application.platform == RuntimePlatform.Android)
        {
            if (m_LinePointer.controllerIndex == ControllerIndex.RIGHT_CONTROLLER)
            {
                HVRController.m_RightController.GetTouchpadTouchPos(ref m_TouchDownPos);
            }
            else
            {
                HVRController.m_LeftController.GetTouchpadTouchPos(ref m_TouchDownPos);
            }
        }
        m_MousePosition = Input.mousePosition;
        m_ClickedDownObj = this.m_PointerEventData.pointerPress;
        this.m_PointerEventData.rawPointerPress = mtriggerObject;
        this.m_PointerEventData.eligibleForClick = true;
        this.m_PointerEventData.delta = Vector2.zero;
        this.m_PointerEventData.dragging = false;
        this.m_PointerEventData.useDragThreshold = true;
        this.m_PointerEventData.clickCount = 1;
        this.m_PointerEventData.clickTime = Time.unscaledTime;
    }

    private void ProcessDrag(PointerEventData eventData)
    {
        GameObject mDragObj = eventData.pointerCurrentRaycast.gameObject;
        if (this.m_PointerEventData.useDragThreshold)
        {
            bool isBeginDrag = ExecuteEvents.Execute(m_ClickedDownObj, eventData, ExecuteEvents.beginDragHandler);
            if (isBeginDrag)
            {
                eventData.pointerDrag = m_ClickedDownObj;
                m_CurrentDragging = m_ClickedDownObj;
            }
        }
        if (m_CurrentDragging)
        {
            ExecuteEvents.Execute(m_CurrentDragging, eventData, ExecuteEvents.dragHandler);

        }
    }

    private void ProcessHover(GameObject lastObject, PointerEventData eventData)
    {
        GameObject mCurrentObj = eventData.pointerCurrentRaycast.gameObject;

        if (mCurrentObj == lastObject && mCurrentObj != null)
        {
            ExecuteEvents.ExecuteHierarchy(mCurrentObj, eventData, HVRExecuteEventsExtension.pointerHoverHandler);
            ExecuteEvents.Execute(mCurrentObj, eventData, HVRExecuteEventsExtension.pointerHoverHandler);
        }
    }

    private void ProcessMove(PointerEventData eventData)
    {
        if (m_LinePointer == null)
        {
            return;
        }
        IController controller = null;
        if (m_LinePointer.controllerIndex == ControllerIndex.RIGHT_CONTROLLER)
        {
            controller = HVRController.m_RightController;
        }
        else
        {
            controller = HVRController.m_LeftController;
        }
        if (controller == null || !controller.IsAvailable())
        {
            return;
        }
        GameObject nowGazedObj = m_PointerEventData.pointerCurrentRaycast.gameObject;
        GameObject selectedDownObj;
        if (nowGazedObj != null)
        {
            if (controller.IsTouchpadTouchDown())
            {
                selectedDownObj = null;
                selectedDownObj = ExecuteEvents.ExecuteHierarchy(nowGazedObj, eventData, ExecuteEvents.selectHandler);
                eventSystem.SetSelectedGameObject(selectedDownObj);

            }
            else if (controller.IsTouchpadTouchUp())
            {
                selectedDownObj = null;
            }
            Vector2 touchPos = new Vector2(0.0f, 0.0f);
            if (eventSystem.currentSelectedGameObject)
            {
                controller.GetTouchpadTouchPos(ref touchPos);
                AxisEventData axisData = GetAxisEventData(touchPos.x - 0.5f, -(touchPos.y - 0.5f), 0.0f);
                axisData.moveDir = MoveDirection.None;

                if (controller.IsTouchpadSwipeLeft())
                {
                    axisData.moveDir = MoveDirection.Left;
                }
                else if (controller.IsTouchpadSwipeRight())
                {
                    axisData.moveDir = MoveDirection.Right;
                }
                else if (controller.IsTouchpadSwipeUp())
                {
                    axisData.moveDir = MoveDirection.Up;

                }
                else if (controller.IsTouchpadSwipeDown())
                {
                    axisData.moveDir = MoveDirection.Down;
                }

                if (axisData.moveDir != MoveDirection.None)
                {
                    ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, axisData, ExecuteEvents.moveHandler);
                }

            }
        }
    }


    private Vector3 GetIntersectionPosition(Camera cam, RaycastResult raycastResult)
    {
        // Check for camera
        if (cam == null)
        {
            return Vector3.zero;
        }

        float intersectionDistance = raycastResult.distance + cam.nearClipPlane;
        Vector3 intersectionPosition = cam.transform.position + (cam.transform.forward * intersectionDistance);
        return intersectionPosition;
    }
}
