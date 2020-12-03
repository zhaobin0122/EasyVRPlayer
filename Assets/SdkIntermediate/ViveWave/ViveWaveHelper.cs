using System;
using UnityEngine;

public class ViveWaveHelper : ISdkCamera, ICanvas, IHandShank, IToast
{
    private string viveWaveCameraPrefabResourcePath = "Sdk/ViveWave/Prefabs/ViveWaveCamera";

    private const float mLongPressDeltaTime = 1.0f;
    private bool isTraggerLongPressed;
    private float mTriggerKeyDownTime = -1;
    private bool isCheckedTraggerLongPressedThisTime;

    public void BindSdkCameraForCanvas(GameObject canvas)
    {
        GameObject obj = GameObject.Find("Head");
        canvas.GetComponent<Canvas>().worldCamera = obj.GetComponent<Camera>();

        canvas.AddComponent<WaveVR_AddEventSystemGUI>();
    }

    public GameObject GetSdkCamera()
    {
        return (GameObject)Resources.Load(viveWaveCameraPrefabResourcePath);
    }

    public Camera GetLeftSdkCamera()
    {
        return GameObject.Find("Eye Left").GetComponent<WaveVR_Camera>().GetCamera();
    }

    public Camera GetRightSdkCamera()
    {
        return GameObject.Find("Eye Right").GetComponent<WaveVR_Camera>().GetCamera();
    }

    bool lastBackKeyDownTime;
    public bool IsBackKeyDown()
    {
        throw new NotImplementedException();
    }

    public bool IsBackKeyUp()
    {
        throw new NotImplementedException();
    }

    public bool IsBackKeyClick()
    {
        
        return IsEscapeKeyClick();
    }


    private float lastEscapeKeyDownTime;
    public bool IsEscapeKeyClick()
    {
        if (WaveVR_Controller.Input(WaveVR_Controller.EDeviceType.Head).GetPressDown(wvr.WVR_InputId.WVR_InputId_Alias1_Back)
            || Input.GetKeyDown(KeyCode.Escape))
        {
            lastEscapeKeyDownTime = Time.realtimeSinceStartup;
        }

        if (WaveVR_Controller.Input(WaveVR_Controller.EDeviceType.Head).GetPressUp(wvr.WVR_InputId.WVR_InputId_Alias1_Back)
            || Input.GetKeyUp(KeyCode.Escape))
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
        if (WaveVR_Controller.Input(WaveVR_Controller.EDeviceType.Head).GetPressDown(wvr.WVR_InputId.WVR_InputId_Alias1_Enter) ||
            Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.Joystick2Button0))
        {
            lastJoystickButton0DownTime = Time.realtimeSinceStartup;
        }

        if (WaveVR_Controller.Input(WaveVR_Controller.EDeviceType.Head).GetPressUp(wvr.WVR_InputId.WVR_InputId_Alias1_Enter) ||
            Input.GetKeyUp(KeyCode.JoystickButton0) || Input.GetKeyUp(KeyCode.Joystick2Button0))
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
    public HandShankManager.SwipeDirection GetSwipeDirection()
    {
        if (WaveVR_Controller.Input(WaveVR_Controller.EDeviceType.Dominant).GetTouchDown(wvr.WVR_InputId.WVR_InputId_Alias1_Touchpad))
        {
            touchBeginTime = Time.realtimeSinceStartup;
            touchBegin = WaveVR_Controller.Input(WaveVR_Controller.EDeviceType.Dominant).GetAxis(wvr.WVR_InputId.WVR_InputId_Alias1_Touchpad);
        }
        tempTouchEnd = WaveVR_Controller.Input(WaveVR_Controller.EDeviceType.Dominant).GetAxis(wvr.WVR_InputId.WVR_InputId_Alias1_Touchpad);
        if (tempTouchEnd.x != 0 || tempTouchEnd.y != 0)
        {
            realTouchEnd = tempTouchEnd;
        }

        if (WaveVR_Controller.Input(WaveVR_Controller.EDeviceType.Dominant).GetTouchUp(wvr.WVR_InputId.WVR_InputId_Alias1_Touchpad))
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
            else {
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
        if (WaveVR_Controller.Input(WaveVR_Controller.EDeviceType.Dominant).GetPressDown(wvr.WVR_InputId.WVR_InputId_Alias1_Touchpad)
            || WaveVR_Controller.Input(WaveVR_Controller.EDeviceType.NonDominant).GetPressDown(wvr.WVR_InputId.WVR_InputId_Alias1_Touchpad))
        {
            lastTouchPadDownTime = Time.realtimeSinceStartup;
        }

        if (WaveVR_Controller.Input(WaveVR_Controller.EDeviceType.Dominant).GetPressUp(wvr.WVR_InputId.WVR_InputId_Alias1_Touchpad)
            || WaveVR_Controller.Input(WaveVR_Controller.EDeviceType.NonDominant).GetPressUp(wvr.WVR_InputId.WVR_InputId_Alias1_Touchpad))
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
        if (WaveVR_Controller.Input(WaveVR_Controller.EDeviceType.Dominant).GetPressDown(wvr.WVR_InputId.WVR_InputId_Alias1_Trigger)
            || WaveVR_Controller.Input(WaveVR_Controller.EDeviceType.NonDominant).GetPressDown(wvr.WVR_InputId.WVR_InputId_Alias1_Trigger))
        {
            lastTraggerKeyDownTime = Time.realtimeSinceStartup;
        }

        if (WaveVR_Controller.Input(WaveVR_Controller.EDeviceType.Dominant).GetPressUp(wvr.WVR_InputId.WVR_InputId_Alias1_Trigger)
            || WaveVR_Controller.Input(WaveVR_Controller.EDeviceType.NonDominant).GetPressUp(wvr.WVR_InputId.WVR_InputId_Alias1_Trigger))
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
        bool isTraggerKeyDown = WaveVR_Controller.Input(WaveVR_Controller.EDeviceType.Dominant).GetPressDown(wvr.WVR_InputId.WVR_InputId_Alias1_Trigger)
            || WaveVR_Controller.Input(WaveVR_Controller.EDeviceType.NonDominant).GetPressDown(wvr.WVR_InputId.WVR_InputId_Alias1_Trigger);
        if (isTraggerKeyDown)
        {
            mTriggerKeyDownTime = Time.realtimeSinceStartup;
        }
        return isTraggerKeyDown;
    }

    public bool IsTraggerKeyUp()
    {
        bool isTraggerKeyUp = WaveVR_Controller.Input(WaveVR_Controller.EDeviceType.Dominant).GetPressUp(wvr.WVR_InputId.WVR_InputId_Alias1_Trigger)
            || WaveVR_Controller.Input(WaveVR_Controller.EDeviceType.NonDominant).GetPressUp(wvr.WVR_InputId.WVR_InputId_Alias1_Trigger);
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
        return false;
    }
}
