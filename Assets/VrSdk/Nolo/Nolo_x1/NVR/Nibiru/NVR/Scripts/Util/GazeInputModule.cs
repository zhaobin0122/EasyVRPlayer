// The MIT License (MIT)
// Copyright 2016 Nibiru. All rights reserved.
// Copyright (c) 2015, Unity Technologies & Google, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
//   The above copyright notice and this permission notice shall be included in
//   all copies or substantial portions of the Software.
//
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//   FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//   AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//   LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//   OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//   THE SOFTWARE.

using NibiruAxis;
using NibiruTask;
using UnityEngine;
using UnityEngine.EventSystems;

/// This script provides an implemention of Unity's `BaseInputModule` class, so
/// that Canvas-based (_uGUI_) UI elements can be selected by looking at them and
/// pulling the viewer's trigger or touching the screen.
/// This uses the player's gaze and the trigger as a raycast generator.
///
/// To use, attach to the scene's **EventSystem** object.  Be sure to move it above the
/// other modules, such as _TouchInputModule_ and _StandaloneInputModule_, in order
/// for the user's gaze to take priority in the event system.
///
/// Next, set the **Canvas** object's _Render Mode_ to **World Space**, and set its _Event Camera_
/// to a (mono) camera that is controlled by a NvrHead.  If you'd like gaze to work
/// with 3D scene objects, add a _PhysicsRaycaster_ to the gazing camera, and add a
/// component that implements one of the _Event_ interfaces (_EventTrigger_ will work nicely).
/// The objects must have colliders too.
///
/// GazeInputModule emits the following events: _Enter_, _Exit_, _Down_, _Up_, _Click_, _Select_,
/// _Deselect_, and _UpdateSelected_.  Scroll, move, and submit/cancel events are not emitted.
namespace Nvr.Internal
{
    [AddComponentMenu("NVR/GazeInputModule")]
    public class GazeInputModule : BaseInputModule
    {
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

        }
        /// @endcond

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

            eventSystem.RaycastAll(pointerData, m_RaycastResultCache);
            pointerData.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
            m_RaycastResultCache.Clear();
            pointerData.delta = diff;
            lastHeadPose = headPose;
            if(pointerData.pointerCurrentRaycast.gameObject == null && eventSystem.currentSelectedGameObject != null)
            {
                eventSystem.SetSelectedGameObject(null);
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

            if(gazeObject != null && NvrOverrideSettings.OnGazeEvent != null)
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
                if(gazeObject != null)
                {
                    /*INvrGazeResponder mGazeResponder = gazeObject.GetComponent<INvrGazeResponder>();
                    if (mGazeResponder != null)
                    {
                        mGazeResponder.OnUpdateIntersectionPosition(intersectionPosition);
                    }*/
                } else
                {
                    // Debug.Log("--------------------------gazePointer && gazeObject is null !!!");
                }
                return;
            }

            bool isInteractive = pointerData.pointerPress != null ||
                ExecuteEvents.GetEventHandler<IPointerClickHandler>(gazeObject) != null;

            if(gazeObject != null && gazeObject != previousGazedObject)
            {
                // Debug.LogError("Enter GazeObject=" + gazeObject.name);
                if (previousGazedObject != null)
                {
                    // Debug.LogError("Exit GazeObject=" + previousGazedObject.name);
                }
            } else if(gazeObject == null && previousGazedObject != null)
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
}
