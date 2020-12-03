using System;
using System.Collections;
using System.Collections.Generic;
using Pvr_UnitySDKAPI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Nolo_x1Helper : ISdkCamera, ICanvas, IHandShank, IToast
{
    private string noloCameraPrefabResourcePath = "Sdk/Nolo/Prefabs/Nolo_x1";
    private const float mLongPressDeltaTime = 1.0f;
    private bool isTraggerLongPressed;
    private float mTriggerKeyDownTime = -1;
    private bool isCheckedTraggerLongPressedThisTime;

    public void BindSdkCameraForCanvas(GameObject canvas)
    {
        GameObject obj = GameObject.Find("MainCamera");
        canvas.GetComponent<Canvas>().worldCamera = obj.GetComponent<Camera>();

        canvas.AddComponent<NoloVR_GraphicRaycaster>();
    }

    public GameObject GetSdkCamera()
    {
        return (GameObject)Resources.Load(noloCameraPrefabResourcePath);
    }

    public Camera GetLeftSdkCamera()
    {
        return GameObject.Find("MainCamera Left").GetComponent<Camera>();
    }

    public Camera GetRightSdkCamera()
    {
        return GameObject.Find("MainCamera Right").GetComponent<Camera>();
    }

    private float lastBackKeyDownTime;
    public bool IsBackKeyDown()
    {
        NoloButtonID noloButtonID = NoloButtonID.Menu;

        if (NoloVR_System.GetInstance().realTrackDevices == 3)
        {
            noloButtonID = (NoloButtonID)NoloC1ButtonID.Back;
        }

        return NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloButtonDown(noloButtonID)
             || NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetNoloButtonDown(noloButtonID);
    }

    public bool IsBackKeyUp()
    {

        NoloButtonID noloButtonID = NoloButtonID.Menu;

        if (NoloVR_System.GetInstance().realTrackDevices == 3)
        {
            noloButtonID = (NoloButtonID)NoloC1ButtonID.Back;
        }

        return NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloButtonUp(noloButtonID)
             || NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetNoloButtonUp(noloButtonID);
    }

    public bool IsBackKeyClick()
    {
        if (IsBackKeyDown())
        {
            lastBackKeyDownTime = Time.realtimeSinceStartup;
        }

        if (IsBackKeyUp())
        {
            if (Time.realtimeSinceStartup - lastBackKeyDownTime < 0.5f)
            {
                return true;
            }
        }

        return IsEscapeKeyClick();
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

    private float lastJoystickButton0DownTime;
    public bool IsJoystickButton0Click()
    {
        if (Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.Joystick2Button0))
        {
            lastJoystickButton0DownTime = Time.realtimeSinceStartup;
        }

        if (Input.GetKeyUp(KeyCode.JoystickButton0) || Input.GetKeyUp(KeyCode.Joystick2Button0))
        {
            if (Time.realtimeSinceStartup - lastJoystickButton0DownTime < 0.5f)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsHomeKeyClick()
    {
        return false;
    }

    private float touchBeginTime;
    private float touchSwipeCheckTime = 0.5f;
    private float touchSwipeCheckDistance = 0.1f;
    private Vector2 touchBegin;
    private Vector2 tempTouchEnd, realTouchEnd;
    private NoloDeviceType noloDeviceType = NoloDeviceType.LeftController;
    public HandShankManager.SwipeDirection GetSwipeDirection()
    {

        if (NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloTouchDown(NoloTouchID.TouchPad))
        {
            noloDeviceType = NoloDeviceType.LeftController;
        }
        else if (NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetNoloTouchDown(NoloTouchID.TouchPad))
        {
            noloDeviceType = NoloDeviceType.RightController;
        }

        if (NoloVR_Controller.GetDevice(noloDeviceType).GetNoloTouchDown(NoloTouchID.TouchPad))
        {
            touchBeginTime = Time.realtimeSinceStartup;
            touchBegin = NoloVR_Controller.GetDevice(noloDeviceType).GetAxis();
        }
        tempTouchEnd = NoloVR_Controller.GetDevice(noloDeviceType).GetAxis();
        if (tempTouchEnd.x != 0 || tempTouchEnd.y != 0)
        {
            realTouchEnd = tempTouchEnd;
        }

        if (NoloVR_Controller.GetDevice(noloDeviceType).GetNoloTouchUp(NoloTouchID.TouchPad))
        {
            float dx = realTouchEnd.x - touchBegin.x;
            float dy = realTouchEnd.y - touchBegin.y;

            if (Math.Abs(dx) >= Math.Abs(dy))
            {
                if (Time.realtimeSinceStartup - touchBeginTime < touchSwipeCheckTime && Math.Abs(dx) > touchSwipeCheckDistance)
                {
                    if (dx < 0)
                    {
                        return HandShankManager.SwipeDirection.SwipeLeft;
                    }
                    else if (dx > 0)
                    {
                        return HandShankManager.SwipeDirection.SwipeRight;
                    }
                }
            }
            else
            {
                if (Time.realtimeSinceStartup - touchBeginTime < touchSwipeCheckTime && Math.Abs(dy) > touchSwipeCheckDistance)
                {
                    if (dy < 0)
                    {
                        return HandShankManager.SwipeDirection.SwipeDown;
                    }
                    else if (dy > 0)
                    {
                        return HandShankManager.SwipeDirection.SwipeUp;
                    }
                }
            }
        }
        return HandShankManager.SwipeDirection.No;
    }

    private float lastTouchPadDownTime;
    public bool IsTouchPadClick()
    {
        NoloButtonID noloButtonID = NoloButtonID.TouchPad;

        if (NoloVR_System.GetInstance().realTrackDevices == 3)
        {
            noloButtonID = (NoloButtonID)NoloC1ButtonID.TouchPad;
        }

        if (NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloButtonDown(noloButtonID)
               || NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetNoloButtonDown(noloButtonID))
        {
            lastTouchPadDownTime = Time.realtimeSinceStartup;
        }

        if (NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloButtonUp(noloButtonID)
            || NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetNoloButtonUp(noloButtonID))
        {
            if (Time.realtimeSinceStartup - lastTouchPadDownTime < 0.5f)
            {
                return true;
            }
        }

        return false;
    }

    private float lastTraggerKeyDownTime;
    public bool IsTraggerKeyClick()
    {
        NoloButtonID noloButtonID = NoloButtonID.Trigger;

        if (NoloVR_System.GetInstance().realTrackDevices == 3)
        {
            noloButtonID = (NoloButtonID)NoloC1ButtonID.Trigger;
        }


        if (NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloButtonDown(noloButtonID)
            || NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetNoloButtonDown(noloButtonID))
        {
            lastTraggerKeyDownTime = Time.realtimeSinceStartup;
        }

        if (NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloButtonUp(noloButtonID)
            || NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetNoloButtonUp(noloButtonID))
        {
            if (Time.realtimeSinceStartup - lastTraggerKeyDownTime < 0.5f)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsTraggerKeyDown()
    {
        NoloButtonID noloButtonID = NoloButtonID.Trigger;

        if (NoloVR_System.GetInstance().realTrackDevices == 3)
        {
            noloButtonID = (NoloButtonID)NoloC1ButtonID.Trigger;
        }


        bool isTraggerKeyDown = NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloButtonDown(noloButtonID)
            || NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetNoloButtonDown(noloButtonID);
        if (isTraggerKeyDown)
        {
            mTriggerKeyDownTime = Time.realtimeSinceStartup;
        }
        return isTraggerKeyDown;
    }

    public bool IsTraggerKeyUp()
    {
        NoloButtonID noloButtonID = NoloButtonID.Trigger;

        if (NoloVR_System.GetInstance().realTrackDevices == 3)
        {
            noloButtonID = (NoloButtonID)NoloC1ButtonID.Trigger;
        }

        bool isTraggerKeyUp = NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloButtonUp(noloButtonID)
            || NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetNoloButtonUp(noloButtonID);
        if (isTraggerKeyUp)
        {
            mTriggerKeyDownTime = -1;
            isCheckedTraggerLongPressedThisTime = false;
        }
        return isTraggerKeyUp;
    }

    public bool IsTraggerLongPressed()
    {
        IsTraggerKeyDown();
        if (!isCheckedTraggerLongPressedThisTime &&
           (mTriggerKeyDownTime > 0 && Time.realtimeSinceStartup - mTriggerKeyDownTime >= mLongPressDeltaTime))
        {
            isTraggerLongPressed = true;
            isCheckedTraggerLongPressedThisTime = true;
            return true;
        }
        return false;
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
        GetLeftSdkCamera().cullingMask = cullingMask;
    }

    public void SetRightSdkCameraCullingMask(int cullingMask)
    {
        GetRightSdkCamera().cullingMask = cullingMask;
    }

    public void ShowToast(string text, int delayCancelTime)
    {
        GameObject toastGameObject = GameObject.Find("ToastRootObject");
        ToastViewManager toastViewManager = toastGameObject.GetComponent<ToastViewManager>();
        toastViewManager.ShowToast(text, delayCancelTime);
    }

    public void CancelToast()
    {
        GameObject toastGameObject = GameObject.Find("ToastRootObject");
        ToastViewManager toastViewManager = toastGameObject.GetComponent<ToastViewManager>();
        toastViewManager.CancelToast();
    }

    public bool IsDontDestroyCamera()
    {
        return true;
    }
}
