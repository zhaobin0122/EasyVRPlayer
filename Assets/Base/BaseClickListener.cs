using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class BaseClickListener : MonoBehaviour
     ,IPointerClickHandler,IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler,  IBeginDragHandler, IDragHandler, IEndDragHandler

{
    //是否有点击焦点
    public static int clickFocusCount;
    private UnityAction mUnityAction;

    // Use this for initialization
    public void Start () {
    }

    //点击
    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (mUnityAction != null)
        {
            mUnityAction.Invoke();
        }
    }

    //点击按下
    public virtual void OnPointerDown(PointerEventData eventData)
    {
      
    }

    //点击抬起
    public virtual void OnPointerUp(PointerEventData eventData)
    {
      
    }

    //点击焦点进入
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        clickFocusCount++;
    }

    //点击焦点离开
    public virtual void OnPointerExit(PointerEventData eventData)
    {
        clickFocusCount--;
    }

    //动态添加clickListener
    public void SetListener(UnityAction unityAction)
    {
        mUnityAction = unityAction;
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        
    }
}
