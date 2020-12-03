using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AndroidLogManager : MonoBehaviour
{
    public static void PrintLog(string tag, string text)
    {
      AndroidJavaClass  androidJavaClass = new AndroidJavaClass("com.zb.inkeVrSdk.VrUtils");
      androidJavaClass.CallStatic("printLog", tag, text);
    }

}
