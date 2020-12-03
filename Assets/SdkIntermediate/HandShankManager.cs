using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandShankManager : IHandShank
{
    //触摸板滑动方向
    public enum SwipeDirection
    {
        No = 0,
        SwipeUp = 1,
        SwipeDown = 2,
        SwipeLeft = 3,
        SwipeRight = 4
    }

    private static HandShankManager handShankManager = new HandShankManager();

    public static HandShankManager GetInstance()
    {
        return handShankManager;
    }

    public bool IsBackKeyClick()
    {
        IHandShank handShank  = (IHandShank)TargetSdkManager.GetTargetSdkHelperInstance();
        return handShank.IsBackKeyClick();
    }

    public bool IsHomeKeyClick()
    {
        IHandShank handShank = (IHandShank)TargetSdkManager.GetTargetSdkHelperInstance();
        return handShank.IsHomeKeyClick();
    }

    public bool IsTouchPadClick()
    {
        IHandShank handShank = (IHandShank)TargetSdkManager.GetTargetSdkHelperInstance();
        return handShank.IsTouchPadClick();
    }

    public SwipeDirection GetSwipeDirection()
    {
        IHandShank handShank = (IHandShank)TargetSdkManager.GetTargetSdkHelperInstance();
        return handShank.GetSwipeDirection();
    }

    public bool IsTraggerKeyClick()
    {
        IHandShank handShank = (IHandShank)TargetSdkManager.GetTargetSdkHelperInstance();
        return handShank.IsTraggerKeyClick();
    }

    public bool IsTraggerKeyDown()
    {
        IHandShank handShank = (IHandShank)TargetSdkManager.GetTargetSdkHelperInstance();
        return handShank.IsTraggerKeyDown();
    }

    public bool IsTraggerKeyUp()
    {
        IHandShank handShank = (IHandShank)TargetSdkManager.GetTargetSdkHelperInstance();
        return handShank.IsTraggerKeyUp();
    }

    public bool IsTraggerLongPressed()
    {
        IHandShank handShank = (IHandShank)TargetSdkManager.GetTargetSdkHelperInstance();
        return handShank.IsTraggerLongPressed();
    }

    public bool IsTraggerLongPressedUp()
    {
        IHandShank handShank = (IHandShank)TargetSdkManager.GetTargetSdkHelperInstance();
        handShank.IsTraggerLongPressed();
        return handShank.IsTraggerLongPressedUp();
    }

    public bool IsEscapeKeyClick()
    {
        IHandShank handShank = (IHandShank)TargetSdkManager.GetTargetSdkHelperInstance();
        return handShank.IsEscapeKeyClick();
    }

    public bool IsJoystickButton0Click()
    {
        IHandShank handShank = (IHandShank)TargetSdkManager.GetTargetSdkHelperInstance();
        return handShank.IsJoystickButton0Click();
    }

    public bool IsBackKeyDown()
    {
        IHandShank handShank = (IHandShank)TargetSdkManager.GetTargetSdkHelperInstance();
        return handShank.IsBackKeyDown();
    }

    public bool IsBackKeyUp()
    {
        IHandShank handShank = (IHandShank)TargetSdkManager.GetTargetSdkHelperInstance();
        return handShank.IsBackKeyUp();
    }
}
