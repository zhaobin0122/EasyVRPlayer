using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IToast
{
    //显示提示
    void ShowToast(string text, int delayCancelTime);
    //取消显示
    void CancelToast();
}
