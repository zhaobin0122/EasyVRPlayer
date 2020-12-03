using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToastManager : IToast
{
    private static ToastManager toastManager = new ToastManager();

    public static ToastManager GetInstance()
    {
        return toastManager;
    }

    public void CancelToast()
    {
        IToast toast = (IToast)TargetSdkManager.GetTargetSdkHelperInstance();
        toast.CancelToast();
    }

    public void ShowToast(string text, int delayCancelTime)
    {
        IToast toast= (IToast)TargetSdkManager.GetTargetSdkHelperInstance();
        toast.ShowToast(text, delayCancelTime);
    }
}
