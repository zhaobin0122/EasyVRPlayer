using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToastViewManager : MonoBehaviour
{
    public GameObject toast;
    public Text toastText;

    void Start()
    {
        toast.SetActive(false);
    }

    public void ShowToast(string text, int delayCancelTime)
    {
        toastText.text = text;
        toast.SetActive(true);
       

        CancelInvoke();
        if (delayCancelTime > 0)
        {
            Invoke("CancelToast", delayCancelTime);
        }
    }

    public void CancelToast()
    {
        CancelInvoke();
        toast.SetActive(false);
    }

    void SendShowToast(string toast)
    {
        ShowToast(toast, 1);
    }

}
