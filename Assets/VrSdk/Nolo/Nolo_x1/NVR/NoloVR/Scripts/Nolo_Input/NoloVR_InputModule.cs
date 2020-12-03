using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using Nvr.Internal;
using NibiruTask;

public class NoloVR_InputModule : PointerInputModule
{
    public List<NoloVR_SimplePointer> pointers = new List<NoloVR_SimplePointer>();


    private bool PointerValid()
    {

        foreach (var pointer in pointers)
        {
            if (pointer.PointerActive())
                return true;
        }
        return false;
    }

    protected override void OnEnable()
    {
        //var standaloneInputModule = GetComponent<StandaloneInputModule>();
        //if (standaloneInputModule != null && standaloneInputModule.enabled)
        //{
        //    standaloneInputModule.enabled = false;
        //}

        base.OnEnable();
    }
    public override void Process()
    {
#if UNITY_ANDROID
        if ((PlayerCtrl.Instance.GamepadEnabled && NibiruTaskApi.IsQuatConn()) || ControllerAndroid.IsNoloConn())
#else
            if(true)
#endif
        {
            // android-controller
            CastRayFromGamepad();
            if (isShowGaze)
            {
                NvrViewer.Instance.GazeApi(GazeTag.Hide, "");
                isShowGaze = false;
            }
        }
        else
        {
            // gaze
            if (!isShowGaze)
            {
                NvrViewer.Instance.SwitchApplicationReticle(true);
                isShowGaze = true;
                pointerData.pointerPress = null;
                HandlePointerExitAndEnter(pointerData, null);
            }

            // Save the previous Game Object
            GameObject gazeObjectPrevious = GetCurrentGameObject();
            CastRayFromGaze();
            UpdateCurrentObject();
            UpdateReticle(gazeObjectPrevious);
        }


        if (!pointerData.eligibleForClick &&
                 (NvrViewer.Instance.Triggered || Input.GetMouseButtonDown(0)))
        {
            // New trigger action. ok键->click
            HandleTrigger();
            NvrViewer.Instance.Triggered = false;
        }
        else if (!NvrViewer.Instance.Triggered && !Input.GetMouseButton(0))
        {
            // Check if there is a pending click to handle.
            HandlePendingClick();
        }
        else if (pointerData.eligibleForClick && NvrViewer.Instance.Triggered)
        {
            NvrViewer.Instance.Triggered = false;
        }

        foreach (var pointer in pointers)
        {
            if (pointer.gameObject.activeInHierarchy && pointer.enabled)
            {
                List<RaycastResult> results = new List<RaycastResult>();
                if (pointer.PointerActive())
                {
                    results = CheckRaycasts(pointer);
                }
                //Process events;
                Hover(pointer, results);
                //Down(pointer, results);
                Click(pointer, results);
                Scroll(pointer, results);
            }
        }
    }

    private List<RaycastResult> CheckRaycasts(NoloVR_SimplePointer pointer)
    {
        var raycastResult = new RaycastResult();
        raycastResult.worldPosition = pointer.transform.position;
        raycastResult.worldNormal = pointer.transform.forward;

        pointer.pointerEventData.pointerCurrentRaycast = raycastResult;
        List<RaycastResult> raycasts = new List<RaycastResult>();
        eventSystem.RaycastAll(pointer.pointerEventData, raycasts);
        return raycasts;
    }

    private bool CheckTransformTree(Transform target, Transform source)
    {
        if (target == null)
        {
            return false;
        }

        if (target.Equals(source))
        {
            return true;
        }

        return CheckTransformTree(target.transform.parent, source);
    }

    private bool NoValidCollision(NoloVR_SimplePointer pointer, List<RaycastResult> results)
    {
        return (results.Count == 0 || !CheckTransformTree(results[0].gameObject.transform, pointer.pointerEventData.pointerEnter.transform));
    }

    private bool IsHovering(NoloVR_SimplePointer pointer)
    {
        foreach (var hoveredObject in pointer.pointerEventData.hovered)
        {
            if (pointer.pointerEventData.pointerEnter && hoveredObject && CheckTransformTree(hoveredObject.transform, pointer.pointerEventData.pointerEnter.transform))
            {
                return true;
            }
        }
        return false;
    }

    //private bool ShouldIgnoreElement(GameObject obj, string ignoreCanvasWithTagOrClass, VRTK_TagOrScriptPolicyList canvasTagOrScriptListPolicy)
    //{
    //    var canvas = obj.GetComponentInParent<Canvas>();
    //    if (!canvas)
    //    {
    //        return false;
    //    }

    //    return (Utilities.TagOrScriptCheck(canvas.gameObject, canvasTagOrScriptListPolicy, ignoreCanvasWithTagOrClass));
    //}

    private void Hover(NoloVR_SimplePointer pointer, List<RaycastResult> results)
    {
        if (pointer.pointerEventData.pointerEnter)
        {
            //if (ShouldIgnoreElement(pointer.pointerEventData.pointerEnter, pointer.ignoreCanvasWithTagOrClass, pointer.canvasTagOrScriptListPolicy))
            //{
            //    return;
            //}
            //Debug.Log("当前Enter:" + pointer.pointerEventData.pointerEnter.name+"   "+results.Count);
            if (NoValidCollision(pointer, results) && !results.Exists((x) => x.gameObject == pointer.pointerEventData.pointerEnter))
            {
                //Debug.Log("执行退出函数:" + pointer.pointerEventData.pointerEnter.name);
                ExecuteEvents.ExecuteHierarchy(pointer.pointerEventData.pointerEnter, pointer.pointerEventData, ExecuteEvents.pointerExitHandler);
                pointer.pointerEventData.hovered.Remove(pointer.pointerEventData.pointerEnter);
                pointer.pointerEventData.pointerEnter = null;
            }
        }
        else
        {
            foreach (var result in results)
            {
                //if (ShouldIgnoreElement(result.gameObject, pointer.ignoreCanvasWithTagOrClass, pointer.canvasTagOrScriptListPolicy))
                //{
                //    continue;
                //}

                var target = ExecuteEvents.ExecuteHierarchy(result.gameObject, pointer.pointerEventData, ExecuteEvents.pointerEnterHandler);
                if (target != null)
                {
                    var selectable = target.GetComponent<Selectable>();
                    if (selectable)
                    {
                        var noNavigation = new Navigation();
                        noNavigation.mode = Navigation.Mode.None;
                        selectable.navigation = noNavigation;
                    }

                    pointer.OnUIPointerElementEnter(pointer.SetUIPointerEvent(target, pointer.hoveringElement));
                    pointer.hoveringElement = target;
                    pointer.pointerEventData.pointerCurrentRaycast = result;
                    pointer.pointerEventData.pointerEnter = target;
                    pointer.pointerEventData.hovered.Add(pointer.pointerEventData.pointerEnter);
                    break;
                }
                else
                {
                    if (result.gameObject != pointer.hoveringElement)
                    {
                        pointer.OnUIPointerElementEnter(pointer.SetUIPointerEvent(result.gameObject, pointer.hoveringElement));
                    }
                    pointer.hoveringElement = result.gameObject;
                }
            }

            if (pointer.hoveringElement && results.Count == 0)
            {
                pointer.OnUIPointerElementExit(pointer.SetUIPointerEvent(null, pointer.hoveringElement));
                pointer.hoveringElement = null;
            }
        }
    }

    private void Click(NoloVR_SimplePointer pointer, List<RaycastResult> results)
    {
        bool isPressed = false;
        if (NoloVR_Plugins.GetTrackModel() == 3)
        {
            pointer.pointerEventData.eligibleForClick = NoloVR_Controller.GetDevice(pointer.currentCtr.GetComponent<NoloVR_TrackedDevice>().deviceType).GetNoloButtonUp((NoloButtonID)NoloC1ButtonID.Trigger) || Input.GetMouseButtonUp(0)||
                NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloButtonUp((NoloButtonID)NoloC1ButtonID.TouchPad);
            isPressed = NoloVR_Controller.GetDevice(pointer.currentCtr.GetComponent<NoloVR_TrackedDevice>().deviceType).GetNoloButtonPressed((NoloButtonID)NoloC1ButtonID.Trigger) || Input.GetMouseButton(0)||
                NoloVR_Controller.GetDevice(pointer.currentCtr.GetComponent<NoloVR_TrackedDevice>().deviceType).GetNoloButtonPressed((NoloButtonID)NoloC1ButtonID.TouchPad);
        }
        else if (NoloVR_Plugins.GetTrackModel() == 6)
        {
            pointer.pointerEventData.eligibleForClick = NoloVR_Controller.GetDevice(pointer.currentCtr.GetComponent<NoloVR_TrackedDevice>().deviceType).GetNoloButtonUp(NoloButtonID.Trigger) || Input.GetMouseButtonUp(0)||
                NoloVR_Controller.GetDevice(pointer.currentCtr.GetComponent<NoloVR_TrackedDevice>().deviceType).GetNoloButtonUp(NoloButtonID.TouchPad);
            isPressed = NoloVR_Controller.GetDevice(pointer.currentCtr.GetComponent<NoloVR_TrackedDevice>().deviceType).GetNoloButtonPressed(NoloButtonID.Trigger) || Input.GetMouseButton(0)||
                 NoloVR_Controller.GetDevice(pointer.currentCtr.GetComponent<NoloVR_TrackedDevice>().deviceType).GetNoloButtonPressed(NoloButtonID.TouchPad);

        }
        else {
            pointer.pointerEventData.eligibleForClick = Input.GetMouseButtonUp(0);
            isPressed = input.GetMouseButton(0);
        }

        if (pointer.pointerEventData.pointerPress)
        {
            //Debug.Log("按下状态为:" + isPressed+"   抬起状态："+ pointer.pointerEventData.eligibleForClick);
            if (isPressed)
            {
                if (!IsHovering(pointer))
                {
                    ExecuteEvents.ExecuteHierarchy(pointer.pointerEventData.pointerPress, pointer.pointerEventData, ExecuteEvents.pointerUpHandler);
                    pointer.pointerEventData.pointerPress = null;
                }
                else
                {
                    ExecuteEvents.ExecuteHierarchy(pointer.pointerEventData.pointerPress, pointer.pointerEventData, ExecuteEvents.pointerDownHandler);

                }
            }
            else
            {
                if (pointer.pointerEventData.eligibleForClick)
                {
                    ExecuteEvents.ExecuteHierarchy(pointer.pointerEventData.pointerPress, pointer.pointerEventData, ExecuteEvents.pointerClickHandler);
                    ExecuteEvents.ExecuteHierarchy(pointer.pointerEventData.pointerPress, pointer.pointerEventData, ExecuteEvents.pointerUpHandler);
                    pointer.pointerEventData.pointerPress = null;
                }
            }
        }
        else if(!pointer.pointerEventData.eligibleForClick)
        {
            if (isPressed)
            {
                foreach (var result in results)
                {
                    //if (ShouldIgnoreElement(result.gameObject, pointer.ignoreCanvasWithTagOrClass, pointer.canvasTagOrScriptListPolicy))
                    //{
                    //    continue;
                    //}

                    var target = ExecuteEvents.ExecuteHierarchy(result.gameObject, pointer.pointerEventData, ExecuteEvents.pointerDownHandler);
                    if (target != null)
                    {
                        pointer.pointerEventData.pressPosition = pointer.pointerEventData.position;
                        pointer.pointerEventData.pointerPressRaycast = result;
                        pointer.pointerEventData.pointerPress = target;
                        break;
                    }
                }
            }
            
        }
        else if (pointer.pointerEventData.eligibleForClick)
        {
            foreach (var result in results)
            {
                //if (ShouldIgnoreElement(result.gameObject, pointer.ignoreCanvasWithTagOrClass, pointer.canvasTagOrScriptListPolicy))
                //{
                //    continue;
                //}

                var target = ExecuteEvents.ExecuteHierarchy(result.gameObject, pointer.pointerEventData, ExecuteEvents.pointerDownHandler);
                if (target != null)
                {
                    pointer.pointerEventData.pressPosition = pointer.pointerEventData.position;
                    pointer.pointerEventData.pointerPressRaycast = result;
                    pointer.pointerEventData.pointerPress = target;
                    break;
                }
            }
        }
    }
    private void Scroll(NoloVR_SimplePointer pointer, List<RaycastResult> results)
    {
        NoloDeviceType currentType = pointer.currentCtr.GetComponent<NoloVR_TrackedDevice>().deviceType;
        pointer.pointerEventData.dragging = (NoloVR_Controller.GetDevice(currentType).GetNoloButtonPressed(NoloButtonID.Trigger)||
            NoloVR_Controller.GetDevice(currentType).GetNoloButtonPressed((NoloButtonID)NoloC1ButtonID.Trigger))
            && pointer.pointerEventData.delta != Vector2.zero;
        if(pointer.pointerEventData.pointerDrag)
        {
            if (!ValidElement(pointer.pointerEventData.pointerDrag))
            {
                pointer.pointerEventData.pointerDrag = null;
                return;
            }
            if (pointer.pointerEventData.dragging)
            {
                if(IsHovering(pointer))
                {
                    if(pointer.pointerEventData.pointerPress.transform.IsChildOf(pointer.pointerEventData.pointerDrag.transform))
                    {
                        ExecuteEvents.ExecuteHierarchy(pointer.pointerEventData.pointerPress, pointer.pointerEventData, ExecuteEvents.pointerUpHandler);
                        pointer.pointerEventData.eligibleForClick = false;
                        pointer.pressToDrag = true;
                    }
                    ExecuteEvents.ExecuteHierarchy(pointer.pointerEventData.pointerDrag, pointer.pointerEventData, ExecuteEvents.dragHandler);
                }
            }
            else
            {
                ExecuteEvents.ExecuteHierarchy(pointer.pointerEventData.pointerDrag, pointer.pointerEventData, ExecuteEvents.dragHandler);
                ExecuteEvents.ExecuteHierarchy(pointer.pointerEventData.pointerDrag, pointer.pointerEventData, ExecuteEvents.endDragHandler);
                foreach (RaycastResult raycast in results)
                {
                    ExecuteEvents.ExecuteHierarchy(raycast.gameObject, pointer.pointerEventData, ExecuteEvents.dropHandler);
                }
                pointer.pointerEventData.pointerDrag = null;
            }

        }
        else if (pointer.pointerEventData.dragging)
        {
            Debug.Log("pointer.pointerEventData.dragging");
            foreach (var result in results)
            {
                if (!ValidElement(result.gameObject))
                {
                    continue;
                }

                ExecuteEvents.ExecuteHierarchy(result.gameObject, pointer.pointerEventData, ExecuteEvents.initializePotentialDrag);
                ExecuteEvents.ExecuteHierarchy(result.gameObject, pointer.pointerEventData, ExecuteEvents.beginDragHandler);
                var target = ExecuteEvents.ExecuteHierarchy(result.gameObject, pointer.pointerEventData, ExecuteEvents.dragHandler);
                if (target != null)
                {
                    pointer.pointerEventData.pointerDrag = target;
                    break;
                }
            }
        }

        //pointer.pointerEventData.scrollDelta = NoloVR_Controller.GetDevice(pointer.deviceType).GetAxis(NoloTouchID.TouchPad);
        //var scrollWheelVisible = false;
        //foreach (RaycastResult result in results)
        //{
        //    if (pointer.pointerEventData.scrollDelta != Vector2.zero)
        //    {
        //        var target = ExecuteEvents.ExecuteHierarchy(result.gameObject, pointer.pointerEventData, ExecuteEvents.scrollHandler);
        //        if (target)
        //        {
        //            scrollWheelVisible = true;
        //        }
        //    }
        //}
    }

    bool ValidElement(GameObject obj)
    {
        var canvasCheck= obj.GetComponentInParent<NoloVR_GraphicRaycaster>();
        return (canvasCheck && canvasCheck.enabled ? true : false);
    }

    protected override void OnDisable()
    {
        var standaloneInputModule = GetComponent<StandaloneInputModule>();
        if (standaloneInputModule != null && standaloneInputModule.enabled == false)
        {
            standaloneInputModule.enabled = true;
        }
        base.OnDisable();
    }


    /// Determines whether gaze input is active in VR Mode only (`true`), or all of the
    /// time (`false`).  Set to false if you plan to use direct screen taps or other
    /// input when not in VR Mode.
    [Tooltip("Whether gaze input is active in VR Mode only (true), or all the time (false).")]
    private bool vrModeOnly = false;

    /// The INvrGazePointer which will be responding to gaze events.
    public static INvrGazePointer gazePointer;

    private PointerEventData pointerData;
    private Vector2 lastHeadPose;

    // Active state
    private bool isActive = false;

    /// Time in seconds between the pointer down and up events sent by a trigger.
    /// Allows time for the UI elements to make their state transitions.
    private const float clickTime = 0.1f;  // Based on default time for a button to animate to Pressed.

    private Vector2 screenCenterVec = Vector2.zero;

    bool isShowGaze = true;

    /// @cond
    public override bool ShouldActivateModule()
    {
        bool activeState = base.ShouldActivateModule();
        // VR模式 或者 vrMoreOnly关闭
        activeState = activeState && (NvrViewer.Instance.VRModeEnabled || !vrModeOnly);

        if (activeState != isActive)
        {
            isActive = activeState;

            // Activate gaze pointer
            if (gazePointer != null)
            {
                if (isActive)
                {
                    gazePointer.OnGazeEnabled();
                }
            }
        }

        return activeState;
    }
    /// @endcond

    public override void DeactivateModule()
    {
        Debug.Log("DeactivateModule");
        DisableGazePointer();
        base.DeactivateModule();
        if (pointerData != null)
        {
            HandlePendingClick();
            HandlePointerExitAndEnter(pointerData, null);
            pointerData.selectedObject = null;
            pointerData = null;
        }
        eventSystem.SetSelectedGameObject(null, GetBaseEventData());
    }

    private void OnApplicationFocus(bool focus)
    {
        if (!focus)
        {
            NvrViewer.Instance.Triggered = false;
            if (pointerData != null && pointerData.selectedObject != null)
            {
                HandlePointerExitAndEnter(pointerData, null);
                eventSystem.SetSelectedGameObject(null, pointerData);
                pointerData.Reset();
            }
        }
    }

    public override bool IsPointerOverGameObject(int pointerId)
    {
        return pointerData != null && pointerData.pointerEnter != null;
    }


    private void CastRayFromGamepad()
    {
        if (pointerData == null)
        {
            pointerData = new PointerEventData(eventSystem);
        }

        // Cast a ray into the scene
        pointerData.Reset();
        if (NvrControllerHelper.ControllerRaycastObject != null)
        {
            pointerData.pointerPress = NvrControllerHelper.ControllerRaycastObject;
            RaycastResult raycastResult = new RaycastResult();
            raycastResult.gameObject = NvrControllerHelper.ControllerRaycastObject;
            pointerData.pointerCurrentRaycast = raycastResult;
            HandlePointerExitAndEnter(pointerData, NvrControllerHelper.ControllerRaycastObject);
        }
        else
        {
            RaycastResult raycastResult = new RaycastResult();
            raycastResult.gameObject = null;
            pointerData.pointerCurrentRaycast = raycastResult;
            pointerData.pointerPress = null;
            HandlePointerExitAndEnter(pointerData, null);
        }

        if (pointerData.pointerCurrentRaycast.gameObject == null && eventSystem.currentSelectedGameObject != null)
        {
            eventSystem.SetSelectedGameObject(null);
        }
    }

    private void CastRayFromGaze()
    {
        Vector2 headPose = NormalizedCartesianToSpherical(NvrViewer.Instance.HeadPose.Orientation * Vector3.forward);

        if (pointerData == null)
        {
            pointerData = new PointerEventData(eventSystem);
            lastHeadPose = headPose;
        }
        Vector2 diff = headPose - lastHeadPose;

        if (screenCenterVec.x == 0)
        {
            screenCenterVec = new Vector2(0.5f * Screen.width, 0.5f * Screen.height);
        }

        // Cast a ray into the scene
        pointerData.Reset();
        pointerData.position = new Vector2(0.5f * Screen.width, 0.5f * Screen.height);

        if (!PointerValid())
        {
            eventSystem.RaycastAll(pointerData, m_RaycastResultCache);
            pointerData.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
            m_RaycastResultCache.Clear();
            pointerData.delta = diff;
            lastHeadPose = headPose;
            if (pointerData.pointerCurrentRaycast.gameObject == null && eventSystem.currentSelectedGameObject != null)
            {
                eventSystem.SetSelectedGameObject(null);
            }
        }
    }

    private void UpdateCurrentObject()
    {
        if (pointerData == null)
        {
            return;
        }
        // Send enter events and update the highlight.

        GameObject rayGameObject = pointerData.pointerCurrentRaycast.gameObject;
        HandlePointerExitAndEnter(pointerData, rayGameObject);
    }


    float lastGazeZ = 0;
    void UpdateReticle(GameObject previousGazedObject)
    {
        if (pointerData == null)
        {
            return;
        }
        Camera camera = pointerData.enterEventCamera; // Get the camera 
        Vector3 intersectionPosition = GetIntersectionPosition();
        GameObject gazeObject = GetCurrentGameObject(); // Get the gaze target

        if (gazeObject != null && NvrOverrideSettings.OnGazeEvent != null)
        {
            NvrOverrideSettings.OnGazeEvent(gazeObject);
        }

        float gazeZ = NvrGlobal.defaultGazeDistance;
        if (gazeObject != null)
        {
            gazeZ = intersectionPosition.z;//  && gazeObject.transform != null &&gazeObject.transform.position.z != 0gazeObject.transform.position.z;
        }

        if (lastGazeZ != gazeZ)
        {
            lastGazeZ = gazeZ;
            // 点
            NvrViewer.Instance.GazeApi(Nvr.Internal.GazeTag.Set_Distance, (-1 * gazeZ).ToString());
        }

        // 记录距离
        NvrGlobal.focusObjectDistance = (int)(Mathf.Abs(gazeZ) * 100) / 100.0f;

        if (gazePointer == null)
        {
            if (gazeObject != null)
            {
                /*INvrGazeResponder mGazeResponder = gazeObject.GetComponent<INvrGazeResponder>();
                if (mGazeResponder != null)
                {
                    mGazeResponder.OnUpdateIntersectionPosition(intersectionPosition);
                }*/
            }
            else
            {
                // Debug.Log("--------------------------gazePointer && gazeObject is null !!!");
            }
            return;
        }

        bool isInteractive = pointerData.pointerPress != null ||
            ExecuteEvents.GetEventHandler<IPointerClickHandler>(gazeObject) != null;

        if (gazeObject != null && gazeObject != previousGazedObject)
        {
            // Debug.LogError("Enter GazeObject=" + gazeObject.name);
            if (previousGazedObject != null)
            {
                // Debug.LogError("Exit GazeObject=" + previousGazedObject.name);
            }
        }
        else if (gazeObject == null && previousGazedObject != null)
        {
            // Debug.LogError("Exit GazeObject=" + previousGazedObject.name);
        }

        if (gazeObject == previousGazedObject)
        {
            if (gazeObject != null && gazePointer != null)
            {
                gazePointer.OnGazeStay(camera, gazeObject, intersectionPosition, isInteractive);
                /*
                INvrGazeResponder mGazeResponder = gazeObject.GetComponent<INvrGazeResponder>();
                if (mGazeResponder != null)
                {
                    mGazeResponder.OnUpdateIntersectionPosition(intersectionPosition);
                }*/
            }
        }
        else
        {
            if (previousGazedObject != null && gazePointer != null)
            {
                gazePointer.OnGazeExit(camera, previousGazedObject);

                if (NvrViewer.Instance != null)
                {
                    if (NvrViewer.Instance.HeadControl == HeadControl.Hover)
                    {
                        if (NvrHeadControl.baseEventData != null)
                        {
                            NvrHeadControl.baseEventData = null;
                            NvrHeadControl.eventGameObject = null;
                        }
                    }
                }
            }

            if (gazeObject != null && gazePointer != null)
            {
                gazePointer.OnGazeStart(camera, gazeObject, intersectionPosition, isInteractive);
                if (NvrViewer.Instance != null)
                {
                    if (NvrViewer.Instance.HeadControl == HeadControl.Hover)
                    {
                        //var go = pointerData.pointerCurrentRaycast.gameObject;
                        NvrHeadControl.baseEventData = pointerData;
                    }
                }
            }
        }
    }

    private void HandleDrag()
    {
        bool moving = pointerData.IsPointerMoving();

        if (moving && pointerData.pointerDrag != null && !pointerData.dragging)
        {
            ExecuteEvents.Execute(pointerData.pointerDrag, pointerData,
                ExecuteEvents.beginDragHandler);
            pointerData.dragging = true;
        }

        // Drag notification
        if (pointerData.dragging && moving && pointerData.pointerDrag != null)
        {
            // Before doing drag we should cancel any pointer down state
            // And clear selection!
            if (pointerData.pointerPress != pointerData.pointerDrag)
            {
                ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerUpHandler);

                pointerData.eligibleForClick = false;
                pointerData.pointerPress = null;
                pointerData.rawPointerPress = null;
            }
            ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.dragHandler);
        }
    }

    private void HandlePendingClick()
    {
        if (!pointerData.eligibleForClick && !pointerData.dragging)
        {
            return;
        }

        if (gazePointer != null)
        {
            Camera camera = pointerData.enterEventCamera;
            gazePointer.OnGazeTriggerEnd(camera);
        }

        var go = pointerData.pointerCurrentRaycast.gameObject;

        // Send pointer up and click events.
        ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerUpHandler);
        if (pointerData.eligibleForClick)
        {
            if (NvrViewer.Instance != null)
            {
                if (NvrViewer.Instance.HeadControl != HeadControl.Hover || (PlayerCtrl.Instance.IsQuatConn() || ControllerAndroid.IsNoloConn()))
                {
                    ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerClickHandler);
                }
            }
        }
        else if (pointerData.dragging)
        {
            ExecuteEvents.ExecuteHierarchy(go, pointerData, ExecuteEvents.dropHandler);
            ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.endDragHandler);
        }

        // Clear the click state.
        pointerData.pointerPress = null;
        pointerData.rawPointerPress = null;
        pointerData.eligibleForClick = false;
        pointerData.clickCount = 0;
        pointerData.clickTime = 0;
        pointerData.pointerDrag = null;
        pointerData.dragging = false;
    }

    private void HandleTrigger()
    {
        var go = pointerData.pointerCurrentRaycast.gameObject;

        // Send pointer down event.
        pointerData.pressPosition = pointerData.position;
        pointerData.pointerPressRaycast = pointerData.pointerCurrentRaycast;
        pointerData.pointerPress =
          ExecuteEvents.ExecuteHierarchy(go, pointerData, ExecuteEvents.pointerDownHandler)
            ?? ExecuteEvents.GetEventHandler<IPointerClickHandler>(go);

        // Save the drag handler as well
        pointerData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(go);
        if (pointerData.pointerDrag != null)
        {
            ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.initializePotentialDrag);
        }

        // Save the pending click state.
        pointerData.rawPointerPress = go;
        pointerData.eligibleForClick = true;
        pointerData.delta = Vector2.zero;
        pointerData.dragging = false;
        pointerData.useDragThreshold = true;
        pointerData.clickCount = 1;
        pointerData.clickTime = Time.unscaledTime;

        if (gazePointer != null)
        {
            gazePointer.OnGazeTriggerStart(pointerData.enterEventCamera);
        }
    }

    private Vector2 NormalizedCartesianToSpherical(Vector3 cartCoords)
    {
        cartCoords.Normalize();
        if (cartCoords.x == 0)
            cartCoords.x = Mathf.Epsilon;
        float outPolar = Mathf.Atan(cartCoords.z / cartCoords.x);
        if (cartCoords.x < 0)
            outPolar += Mathf.PI;
        float outElevation = Mathf.Asin(cartCoords.y);
        return new Vector2(outPolar, outElevation);
    }

    GameObject GetCurrentGameObject()
    {
        if (pointerData != null && pointerData.enterEventCamera != null)
        {
            return pointerData.pointerCurrentRaycast.gameObject;
        }
        return null;
    }

    Vector3 GetIntersectionPosition()
    {
        // Check for camera
        Camera cam = pointerData.enterEventCamera;
        if (cam == null)
        {
            return Vector3.zero;
        }

        float intersectionDistance = pointerData.pointerCurrentRaycast.distance + cam.nearClipPlane;
        Vector3 intersectionPosition = cam.transform.position + cam.transform.forward * intersectionDistance;

        return intersectionPosition;
    }

    void DisableGazePointer()
    {
        if (gazePointer == null)
        {
            return;
        }

        GameObject currentGameObject = GetCurrentGameObject();
        if (currentGameObject)
        {
            Camera camera = pointerData.enterEventCamera;
            gazePointer.OnGazeExit(camera, currentGameObject);
            if (NvrViewer.Instance != null)
            {
                if (NvrViewer.Instance.HeadControl == HeadControl.Hover)
                {
                    if (NvrHeadControl.baseEventData != null)
                        NvrHeadControl.baseEventData = null;
                }
            }
        }

        gazePointer.OnGazeDisabled();
    }
}
