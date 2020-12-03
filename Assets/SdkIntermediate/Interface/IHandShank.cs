using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHandShank
{
    //back键是否点击
    bool IsBackKeyClick();
    //back键是否按下
    bool IsBackKeyDown();
    //back键是否提起
    bool IsBackKeyUp();
    //home键是否按下
    bool IsHomeKeyClick();
    //获取触摸板滑动方向
    HandShankManager.SwipeDirection GetSwipeDirection();
    //滑动面板是否按下
    bool IsTouchPadClick();
    //扳机键是否点击
    bool IsTraggerKeyClick();
    //扳机键是否按下
    bool IsTraggerKeyDown();
    //扳机键是否抬起
    bool IsTraggerKeyUp();
    //扳机键是否长按
    bool IsTraggerLongPressed();
    ////扳机键是否长按抬起
    bool IsTraggerLongPressedUp();
    //头盔返回按键是否点击
    bool IsEscapeKeyClick();
    //头盔确定按键是否点击
    bool IsJoystickButton0Click();
}
