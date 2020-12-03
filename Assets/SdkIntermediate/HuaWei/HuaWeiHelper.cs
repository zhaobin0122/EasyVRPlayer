using System.Collections;
using System.Collections.Generic;
using HVRCORE;
using UnityEngine;

public class HuaWeiHelper : ISdkCamera, ICanvas, IHandShank, IToast
{
    private string huaWeiCameraPrefabName = "Sdk/HuaWei/Prefabs/HVR";
    private IController hvrController = null; //手柄控制

    private const float mLongPressDeltaTime = 1.0f;
    private bool isTraggerLongPressed;
    private float mTriggerKeyDownTime = -1;
    private bool isCheckedTraggerLongPressedThisTime;

    public void BindSdkCameraForCanvas(GameObject canvas)
    {
        canvas.AddComponent<HVRGraphicRaycaster>();
        canvas.GetComponent<HVRGraphicRaycaster>().rayObject = GameObject.Find("HVRRightController/EventCamera/LineRender");
    }

    public GameObject GetSdkCamera()
    {
        return (GameObject)Resources.Load(huaWeiCameraPrefabName);
    }

    public Camera GetLeftSdkCamera()
    {
        return HVRLayoutCore.m_LeftCamObj.GetComponent<Camera>();
    }

    public Camera GetRightSdkCamera()
    {
        return HVRLayoutCore.m_RightCamObj.GetComponent<Camera>();
    }

    private float lastBackKeyDownTime;
    public bool IsBackKeyClick()
    {
        GetHVRController();
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

        return false;
    }

    public bool IsBackKeyDown()
    {
        GetHVRController();
        return hvrController != null ? hvrController.IsButtonDown(ButtonType.ButtonBack) : false;
    }

    public bool IsBackKeyUp()
    {
        GetHVRController();
        return hvrController != null ? hvrController.IsButtonUp(ButtonType.ButtonBack) : false;
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
        if (Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            lastJoystickButton0Time = Time.realtimeSinceStartup;
        }

        if (Input.GetKeyUp(KeyCode.JoystickButton0))
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
        return false;
    }

    private void GetHVRController()
    {
        if (hvrController != null)
        {
            return;
        }
        IHelmetHandle helmentHandle = HvrApi.GetHelmetHandle();
        HelmetModel helmetModel = HelmetModel.HVR_HELMET_THIRD_GEN;
        int ret = helmentHandle.GetHelmetInfo(ref helmetModel);
        if (ret == 0)
        {
            IControllerHandle controllerHandler = HvrApi.GetControllerHandle();
            int[] indices = controllerHandler.GetValidIndices();
            if (controllerHandler == null)
            {
                return;
            }
            switch (helmetModel)
            {
                case HelmetModel.HVR_HELMET_FIRST_GEN:
                    hvrController = controllerHandler.GetControllerByIndex(indices[0]);
                    break;
                case HelmetModel.HVR_HELMET_SECOND_GEN:
                case HelmetModel.HVR_HELMET_THIRD_GEN:
                    hvrController = controllerHandler.GetControllerByIndex(indices[1]);
                    break;
                case HelmetModel.HVR_HELMET_NOT_FOUND:
                    hvrController = controllerHandler.GetControllerByIndex(indices[1]);
                    if (null != hvrController)
                    {
                        //if (m_Controller.IsAvailable())
                        //{
                        //}
                    }
                    else
                    {
                        hvrController = controllerHandler.GetControllerByIndex(indices[0]);
                    }
                    break;
                case HelmetModel.HVR_HELMET_UNKNOWN:
                    break;
            }
        }
    }

    public HandShankManager.SwipeDirection GetSwipeDirection()
    {
        if (IsTouchpadSwipeLeft())
        {
            return HandShankManager.SwipeDirection.SwipeLeft;
        }
        else if (IsTouchpadSwipeRight())
        {
            return HandShankManager.SwipeDirection.SwipeRight;
        }
        return HandShankManager.SwipeDirection.No;
    }

    private bool IsTouchpadSwipeLeft()
    {
        GetHVRController();
        return hvrController != null ? hvrController.IsTouchpadSwipeLeft() : false;
    }

    private bool IsTouchpadSwipeRight()
    {
        GetHVRController();
        return hvrController != null ? hvrController.IsTouchpadSwipeRight() : false;
    }

    private float lastTouchPadDownTime;
    public bool IsTouchPadClick()
    {
        GetHVRController();
        if (hvrController == null)
        {
            return false;
        }
        if (hvrController.IsButtonDown(ButtonType.ButtonTouchPad))
        {
            lastTouchPadDownTime = Time.realtimeSinceStartup;
        }

        if (hvrController.IsButtonUp(ButtonType.ButtonTouchPad))
        {
            if (Time.realtimeSinceStartup - lastTouchPadDownTime < 0.5f)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsTraggerKeyClick()
    {
        GetHVRController();
        IsTraggerKeyDown();
        bool isTraggerKeyUp = hvrController != null ? hvrController.IsButtonUp(ButtonType.ButtonTrigger) : false;
        if (isTraggerKeyUp && Time.realtimeSinceStartup - mTriggerKeyDownTime < 0.5f)
        {
           return true;
        }
        return false;
    }

    public bool IsTraggerKeyDown()
    {
        GetHVRController();
        bool isTraggerKeyDown = hvrController != null ? hvrController.IsButtonDown(ButtonType.ButtonTrigger) : false;
        if (isTraggerKeyDown)
        {
            mTriggerKeyDownTime = Time.realtimeSinceStartup;
        }
        return isTraggerKeyDown;
    }

    public bool IsTraggerKeyUp()
    {
        GetHVRController();
        bool isTraggerKeyUp = hvrController != null ? hvrController.IsButtonUp(ButtonType.ButtonTrigger) : false;
        if (isTraggerKeyUp)
        {
            mTriggerKeyDownTime = -1;
            isCheckedTraggerLongPressedThisTime = false;
        }
        return isTraggerKeyUp;
    }


    public bool IsTraggerLongPressed()
    {
        GetHVRController();
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
        HVRLayoutCore.m_LeftCamObj.GetComponent<Camera>().cullingMask = cullingMask;
    }

    public void SetRightSdkCameraCullingMask(int cullingMask)
    {
        HVRLayoutCore.m_RightCamObj.GetComponent<Camera>().cullingMask = cullingMask;
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
