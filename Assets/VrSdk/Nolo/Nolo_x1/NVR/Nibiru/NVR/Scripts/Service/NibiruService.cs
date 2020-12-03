using Nvr.Internal;
using UnityEngine;

namespace Nvr.Internal
{
    public class NibiruService
    {
        private const string NibiruSDKClassName = "com.nibiru.lib.vr.NibiruVR";
        private const string ServiceClassName = "com.nibiru.service.NibiruService";
        protected AndroidJavaObject androidActivity;
        protected AndroidJavaClass nibiruSDKClass;
        protected AndroidJavaObject nibiruOsServiceObject;
        protected AndroidJavaObject nibiruVRServiceObject;
         
 
        protected AndroidJavaObject audioManager;
      

        public void Init()
        {
#if UNITY_ANDROID
            try
            {
                using (AndroidJavaClass player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    androidActivity = player.GetStatic<AndroidJavaObject>("currentActivity");
                    audioManager = androidActivity.Call<AndroidJavaObject>("getSystemService", new AndroidJavaObject("java.lang.String", "audio"));
                }
            }
            catch (AndroidJavaException e)
            {
                androidActivity = null;
                Debug.LogError("Exception while connecting to the Activity: " + e);
                return;
            }

            nibiruSDKClass = BaseAndroidDevice.GetClass(NibiruSDKClassName);

            nibiruOsServiceObject = nibiruSDKClass.CallStatic<AndroidJavaObject>("getNibiruOSService", androidActivity);
            nibiruVRServiceObject = nibiruSDKClass.CallStatic<AndroidJavaObject>("getUsingNibiruVRServiceGL");
         
            // Debug.Log("nibiruOsServiceObject is "+ nibiruOsServiceObject.Call<AndroidJavaObject>("getClass").Call<string>("getName"));
            // Debug.Log("nibiruSensorServiceObject is " + nibiruSensorServiceObject.Call<AndroidJavaObject>("getClass").Call<string>("getName"));

            NibiruTask.NibiruTaskApi.Init();



            // 默认触发请求权限：
            RequsetPermission(new string[] {
                    NvrGlobal.Permission.CAMERA,
                    NvrGlobal.Permission.WRITE_EXTERNAL_STORAGE,
                    NvrGlobal.Permission.READ_EXTERNAL_STORAGE,
                    NvrGlobal.Permission.ACCESS_NETWORK_STATE,
                    NvrGlobal.Permission.ACCESS_COARSE_LOCATION,
                    NvrGlobal.Permission.BLUETOOTH,
                    NvrGlobal.Permission.BLUETOOTH_ADMIN,
                    NvrGlobal.Permission.INTERNET,
                    NvrGlobal.Permission.GET_TASKS,
                });
#endif
        }

        public static int NKEY_SYS_HANDLE = 0;
        public static int NKEY_APP_HANDLE = 1;
        public void RegHandleNKey(int mode)
        {
            if (nibiruVRServiceObject != null)
            {
                RunOnUIThread(androidActivity, new AndroidJavaRunnable(() =>
                {
                    nibiruVRServiceObject.Call("regHandleNKey", mode);
                }));
            }
            else
            {
                Debug.LogError("regHandleNKey failed, nibiruVRServiceObject is null !!!");
            }
        }

        public void SetEnableFPS(bool isEnabled)
        {
            if (nibiruVRServiceObject != null)
            {
                nibiruVRServiceObject.Call("setEnableFPS", isEnabled);
            }
            else
            {
                Debug.LogError("SetEnableFPS failed, nibiruVRServiceObject is null !!!");
            }
        }

        public float[] GetFPS()
        {
            if (nibiruVRServiceObject != null)
            {
                return nibiruVRServiceObject.Call<float[]>("getFPS");
            }
            else
            {
                Debug.LogError("SetEnableFPS failed, nibiruVRServiceObject is null !!!");
            }
            return new float[] { -1, -1 };
        }

        public bool IsSupport6DOF()
        {
            if (nibiruVRServiceObject != null)
            {
                return nibiruVRServiceObject.Call<bool>("isSupport6Dof");
            }
            else
            {
                Debug.LogError("IsSupport6DOF failed, because nibiruVRServiceObject is null !!!");
            }
            return false;
        }

        //4.1 获取屏幕亮度值：
        public int GetBrightnessValue()
        {
            int BrightnessValue = 0;
#if UNITY_ANDROID
            BaseAndroidDevice.CallObjectMethod<int>(ref BrightnessValue, nibiruOsServiceObject, "getBrightnessValue");
#endif
            return BrightnessValue;
        }

        //4.2 调节屏幕亮度：
        public void SetBrightnessValue(int value)
        {
            if (nibiruOsServiceObject == null) return;
#if UNITY_ANDROID
            RunOnUIThread(androidActivity, new AndroidJavaRunnable(() =>
            {
                BaseAndroidDevice.CallObjectMethod(nibiruOsServiceObject, "setBrightnessValue", value, 200.01f);
            }));
#endif
        }

        //4.3 获取当前2D/3D显示模式：
        public DISPLAY_MODE GetDisplayMode()
        {
            if (nibiruOsServiceObject == null) return DISPLAY_MODE.MODE_2D;
            AndroidJavaObject androidObject = nibiruOsServiceObject.Call<AndroidJavaObject>("getDisplayMode");
            int mode = androidObject.Call<int>("ordinal");
            return (DISPLAY_MODE)mode;
        }

        //4.4 切换2D/3D显示模式:
        public void SetDisplayMode(DISPLAY_MODE displayMode)
        {
            if (nibiruOsServiceObject != null)
            {
                RunOnUIThread(androidActivity, new AndroidJavaRunnable(() =>
                {
                    nibiruOsServiceObject.Call("setDisplayMode", (int)displayMode);
                }));
            }
        }

        // 渠道ID
        public string GetChannelCode()
        {
            if (nibiruOsServiceObject == null) return "NULL";
            return nibiruOsServiceObject.Call<string>("getChannelCode");
        }

        // 型号
        public string GetModel()
        {
            if (nibiruOsServiceObject == null) return "NULL";
            return nibiruOsServiceObject.Call<string>("getModel");
        }

        // 系统OS版本
        public string GetOSVersion()
        {
            if (nibiruOsServiceObject == null) return "NULL";
            return nibiruOsServiceObject.Call<string>("getOSVersion");
        }

        // 系统OS版本号
        public int GetOSVersionCode()
        {
            if (nibiruOsServiceObject == null) return -1;
            return nibiruOsServiceObject.Call<int>("getOSVersionCode");
        }

        // 系统服务版本
        public string GetServiceVersionCode()
        {
            if (nibiruOsServiceObject == null) return "NULL";
            return nibiruOsServiceObject.Call<string>("getServiceVersionCode");
        }

        // 获取厂家软件版本：（对应驱动板软件版本号）
        public string GetVendorSWVersion()
        {
            if (nibiruOsServiceObject == null) return "NULL";
            return nibiruOsServiceObject.Call<string>("getVendorSWVersion");
        }

        // 控制touchpad是否显示 value为true表示显示，false表示不显示 
        public void SetEnableTouchCursor(bool isEnable)
        {
            RunOnUIThread(androidActivity, new AndroidJavaRunnable(() =>
            {
                if (nibiruOsServiceObject != null)
                {
                    nibiruOsServiceObject.Call("setEnableTouchCursor", isEnable);
                }
            }));
        }

        // UI线程中运行
        public void RunOnUIThread(AndroidJavaObject activityObj, AndroidJavaRunnable r)
        {
            activityObj.Call("runOnUiThread", r);
        }
        private AndroidJavaObject javaArrayFromCS(string[] values)
        {
            AndroidJavaClass arrayClass = new AndroidJavaClass("java.lang.reflect.Array");
            AndroidJavaObject arrayObject = arrayClass.CallStatic<AndroidJavaObject>("newInstance", new AndroidJavaClass("java.lang.String"), values.Length);
            for (int i = 0; i < values.Length; ++i)
            {
                arrayClass.CallStatic("set", arrayObject, i, new AndroidJavaObject("java.lang.String", values[i]));
            }
            return arrayObject;
        }

        public void RequsetPermission(string[] names)
        {
            if (nibiruOsServiceObject != null)
            {
                nibiruOsServiceObject.Call("requestPermission", javaArrayFromCS(names));
            }
        }
    }
}
