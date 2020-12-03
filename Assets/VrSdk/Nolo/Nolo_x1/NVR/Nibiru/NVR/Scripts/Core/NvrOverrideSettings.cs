using NibiruTask;
using System.Collections.Generic;
using UnityEngine;

namespace Nvr.Internal
{
    public static class NvrOverrideSettings
    {
        public enum PerfLevel
        {
            NoOverride = -1,
            System = 0,
            Minimum = 1,
            Medium = 2,
            Maximum = 3
        };

        public delegate void OnProfileChangedCallback();
        public static OnProfileChangedCallback OnProfileChangedEvent;

        // 回调内添加针对单眼相机的特殊处理脚本
        public delegate void OnEyeCameraInitCallback(NvrViewer.Eye eye, GameObject goParent);
        public static OnEyeCameraInitCallback OnEyeCameraInitEvent;

        public delegate void OnGazeCallback(GameObject gazeObject);
        public static OnGazeCallback OnGazeEvent;
		
		public delegate void OnApplicationQuit();
        public static OnApplicationQuit OnApplicationQuitEvent;

        // 声明回调函数原型，即函数委托了  
        public delegate void onSelectionResult(AndroidJavaObject task);

        //声明电量回调函数原型
        public delegate void onPowerChange(double task);

        // 声明设置信息获取服务绑定回调函数原型  
        public delegate void onServerApiReady(bool isReady);

        public delegate void onDeviceConnectState(int state, CDevice device);

        public delegate void onDeviceInfoCallback(string currentTimezone,
            string currentLanguage, List<string> languageList, string vrVersion, string deviceName);
    }
}
