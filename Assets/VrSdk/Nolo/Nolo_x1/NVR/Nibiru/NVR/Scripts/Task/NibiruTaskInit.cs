using NibiruTask;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NibiruTask
{
    public class NibiruTaskInit : MonoBehaviour
    {
        // Use this for initialization
        void Start()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                NibiruTaskApi.Init();
                NibiruTaskApi.addOnServerApiReadyCallback(onServerApiReady);
                NibiruTaskApi.addOnSysSleepApiReadyCallback(onSysSleepApiReady);
            }
        }
  
        public void onSelectionResult(AndroidJavaObject task)
        {
            NibiruTaskApi.GetResultValueFromSelectionTask(task);

        }

        public void onServerApiReady(bool isReady)
        {
            Debug.Log("GetDeviceName:" + NibiruTaskApi.GetDeviceName());
            Debug.Log("GetCurrentTimezone:" + NibiruTaskApi.GetCurrentTimezone());
            ThemeApiData currentTheme = NibiruTaskApi.GetCurrentTheme();
            if (currentTheme != null)
            {
                Debug.Log("CurrentTheme:" + currentTheme.toString());
            }
            List<ThemeApiData> themeList = NibiruTaskApi.GetThemeList();
            if (themeList != null)
            {
                for (int i = 0; i < themeList.Count; i++)
                {
                    Debug.Log(i + "ThemeInfo:" + themeList[i].toString());
                }
            }
        }

        public void onSysSleepApiReady(bool isReady)
        {
            Debug.Log("GetSysSleepTime:" + NibiruTaskApi.GetSysSleepTime());
        }

        public void onDeviceConnectState(int state, CDevice device)
        {
            Debug.Log("onDeviceConnectState:" + state + device.toString());
        }
    }
}