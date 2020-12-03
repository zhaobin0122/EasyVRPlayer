using System;
using System.Collections;
using System.Collections.Generic;
using Pvr_UnitySDKAPI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PicoHelper : ISdkCamera, ICanvas, IHandShank, IToast
{
    private bool isTraggerLongPressed;
    private string picoCameraPrefabResourcePath = "Sdk/Pico/Prefabs/Pvr_UnitySDK";

    public void BindSdkCameraForCanvas(GameObject canvas)
    {
        GameObject obj = GameObject.Find("Head");
        canvas.GetComponent<Canvas>().worldCamera = obj.GetComponent<Camera>();
        canvas.AddComponent<Pvr_UICanvas>();
    }

    public GameObject GetSdkCamera()
    {
        return (GameObject)Resources.Load(picoCameraPrefabResourcePath);
    }

    public Camera GetLeftSdkCamera()
    {
        return GameObject.Find("LeftEye").GetComponent<Camera>();
    }

    public Camera GetRightSdkCamera()
    {
        return GameObject.Find("RightEye").GetComponent<Camera>();
    }

    public bool IsBackKeyClick()
    {
        
        return Pvr_UnitySDKAPI.Controller.UPvr_GetKeyClick(0, Pvr_UnitySDKAPI.Pvr_KeyCode.APP)
            || Pvr_UnitySDKAPI.Controller.UPvr_GetKeyClick(1, Pvr_UnitySDKAPI.Pvr_KeyCode.APP)
            || IsEscapeKeyClick();
    }

    private float lastEscapeKeyDownTime;
    public bool IsEscapeKeyClick()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            lastEscapeKeyDownTime = Time.realtimeSinceStartup;
        }

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (Time.realtimeSinceStartup - lastEscapeKeyDownTime < 0.5f)
            {
                return true;
            }
        }

        return false;
    }

    private float lastJoystickButton0Time;
    public bool IsJoystickButton0Click()
    {
        if (Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.Joystick2Button0))
        {
            lastJoystickButton0Time = Time.realtimeSinceStartup;
        }

        if (Input.GetKeyUp(KeyCode.JoystickButton0) || Input.GetKeyUp(KeyCode.Joystick2Button0))
        {
            if (Time.realtimeSinceStartup - lastJoystickButton0Time < 0.5f)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsHomeKeyClick()
    {
        return Pvr_UnitySDKAPI.Controller.UPvr_GetKeyClick(0, Pvr_UnitySDKAPI.Pvr_KeyCode.HOME)
            || Pvr_UnitySDKAPI.Controller.UPvr_GetKeyClick(1, Pvr_UnitySDKAPI.Pvr_KeyCode.HOME);
    }

    private bool isTouchBegin;
    private float touchBeginTime;
    private float touchSwipeCheckTime = 0.5f;
    private float touchSwipeCheckDistance = 30;
    private Vector2 touchBegin;
    private Vector2 touchEnd;
    public HandShankManager.SwipeDirection GetSwipeDirection()
    {
        if (Pvr_UnitySDKAPI.Controller.UPvr_IsTouching(0))
        {
            if (!isTouchBegin)
            {
                isTouchBegin = true;
                touchBeginTime = Time.realtimeSinceStartup;
                touchBegin = Pvr_UnitySDKAPI.Controller.UPvr_GetTouchPadPosition(0);
            }
            touchEnd = Pvr_UnitySDKAPI.Controller.UPvr_GetTouchPadPosition(0);
        }
        else if (Pvr_UnitySDKAPI.Controller.UPvr_IsTouching(1))
        {
            if (!isTouchBegin)
            {
                isTouchBegin = true;
                touchBeginTime = Time.realtimeSinceStartup;
                touchBegin = Pvr_UnitySDKAPI.Controller.UPvr_GetTouchPadPosition(1);
            }
            touchEnd = Pvr_UnitySDKAPI.Controller.UPvr_GetTouchPadPosition(1);
        }
        else
        {
            if (isTouchBegin)
            {
                isTouchBegin = false;
                float dy = touchEnd.y - touchBegin.y;
                float dx = touchEnd.x - touchBegin.x;

                if (Math.Abs(dy) >= Math.Abs(dx))
                {
                    if (Time.realtimeSinceStartup - touchBeginTime < touchSwipeCheckTime && Math.Abs(dy) > touchSwipeCheckDistance)
                    {
                        if (dy < 0)
                        {
                            return HandShankManager.SwipeDirection.SwipeLeft;
                        }
                        else if (dy > 0)
                        {
                            return HandShankManager.SwipeDirection.SwipeRight;
                        }
                    }
                }
                else {
                    if (Time.realtimeSinceStartup - touchBeginTime < touchSwipeCheckTime && Math.Abs(dx) > touchSwipeCheckDistance)
                    {
                        if (dx < 0)
                        {
                            return HandShankManager.SwipeDirection.SwipeDown;
                        }
                        else if (dx > 0)
                        {
                            return HandShankManager.SwipeDirection.SwipeUp;
                        }
                    }
                }
            }
        }

        Pvr_UnitySDKAPI.SwipeDirection swipeDirection = Pvr_UnitySDKAPI.SwipeDirection.No;
        if (Controller.UPvr_GetMainHandNess() == 0)
        {
            swipeDirection = Pvr_UnitySDKAPI.Controller.UPvr_GetSwipeDirection(0);
        }
        else if (Controller.UPvr_GetMainHandNess() == 1)
        {
            swipeDirection = Pvr_UnitySDKAPI.Controller.UPvr_GetSwipeDirection(1);
        }

        if (swipeDirection == Pvr_UnitySDKAPI.SwipeDirection.SwipeLeft)
        {
            return HandShankManager.SwipeDirection.SwipeLeft;
        }
        else if (swipeDirection == Pvr_UnitySDKAPI.SwipeDirection.SwipeRight)
        {
            return HandShankManager.SwipeDirection.SwipeRight;
        }
        else if (swipeDirection == Pvr_UnitySDKAPI.SwipeDirection.SwipeUp)
        {
            return HandShankManager.SwipeDirection.SwipeUp;
        }
        else if (swipeDirection == Pvr_UnitySDKAPI.SwipeDirection.SwipeDown)
        {
            return HandShankManager.SwipeDirection.SwipeDown;
        }
        return HandShankManager.SwipeDirection.No;
    }

    public bool IsTouchPadClick()
    {
        return Pvr_UnitySDKAPI.Controller.UPvr_GetKeyClick(0, Pvr_UnitySDKAPI.Pvr_KeyCode.TOUCHPAD)
            || Pvr_UnitySDKAPI.Controller.UPvr_GetKeyClick(1, Pvr_UnitySDKAPI.Pvr_KeyCode.TOUCHPAD);
    }

    public bool IsTraggerKeyClick()
    {
        return Pvr_UnitySDKAPI.Controller.UPvr_GetKeyClick(0, Pvr_UnitySDKAPI.Pvr_KeyCode.TRIGGER)
            || Pvr_UnitySDKAPI.Controller.UPvr_GetKeyClick(1, Pvr_UnitySDKAPI.Pvr_KeyCode.TRIGGER);
    }

    public bool IsTraggerKeyDown()
    {
        return Pvr_UnitySDKAPI.Controller.UPvr_GetKeyDown(0, Pvr_UnitySDKAPI.Pvr_KeyCode.TRIGGER)
            || Pvr_UnitySDKAPI.Controller.UPvr_GetKeyDown(1, Pvr_UnitySDKAPI.Pvr_KeyCode.TRIGGER);
    }

    public bool IsTraggerKeyUp()
    {
        return Pvr_UnitySDKAPI.Controller.UPvr_GetKeyUp(0, Pvr_UnitySDKAPI.Pvr_KeyCode.TRIGGER)
            || Pvr_UnitySDKAPI.Controller.UPvr_GetKeyUp(1, Pvr_UnitySDKAPI.Pvr_KeyCode.TRIGGER);
    }

    public bool IsTraggerLongPressed()
    {
        bool b = Pvr_UnitySDKAPI.Controller.UPvr_GetKeyLongPressed(0, Pvr_UnitySDKAPI.Pvr_KeyCode.TRIGGER)
            || Pvr_UnitySDKAPI.Controller.UPvr_GetKeyLongPressed(1, Pvr_UnitySDKAPI.Pvr_KeyCode.TRIGGER);
        if (b)
        {
            isTraggerLongPressed = b;
        }
        return b;
    }

    public bool IsTraggerLongPressedUp()
    {
        bool isTraggerKeyUp = IsTraggerKeyUp();
        bool isTraggerLongPressedUp = isTraggerKeyUp && isTraggerLongPressed;
        if (isTraggerKeyUp)
        {
            isTraggerLongPressed = false;
        }
        return isTraggerLongPressedUp;
    }

    public void SetLeftSdkCameraCullingMask(int cullingMask)
    {
        Pvr_UnitySDKEye pvr_UnitySDKEye = GameObject.Find("LeftEye").GetComponent<Pvr_UnitySDKEye>();
        pvr_UnitySDKEye.updateEyeCameraOriginCullingMask(cullingMask);
    }

    public void SetRightSdkCameraCullingMask(int cullingMask)
    {
        Pvr_UnitySDKEye pvr_UnitySDKEye = GameObject.Find("RightEye").GetComponent<Pvr_UnitySDKEye>();
        pvr_UnitySDKEye.updateEyeCameraOriginCullingMask(cullingMask);
    }

    public void ShowToast(string text, int delayCancelTime)
    {
        GameObject toastGameObject =  GameObject.Find("ToastRootObject");
        ToastViewManager toastViewManager = toastGameObject.GetComponent<ToastViewManager>();
        toastViewManager.ShowToast(text, delayCancelTime);
    }

    public void CancelToast()
    {
        GameObject toastGameObject = GameObject.Find("ToastRootObject");
        ToastViewManager toastViewManager = toastGameObject.GetComponent<ToastViewManager>();
        toastViewManager.CancelToast();
    }

    public bool IsBackKeyDown()
    {
        throw new NotImplementedException();
    }

    public bool IsBackKeyUp()
    {
        throw new NotImplementedException();
    }

    public bool IsDontDestroyCamera()
    {
        return true;
    }
}
