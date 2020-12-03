using UnityEngine;
using System.Collections;
using System;
using XR;

namespace NibiruTask
{
    public class ControllerAndroid
    {

        public static bool isPause = false;


    static class GetAndroid
    {
        public static AndroidJavaClass usbClass;
        public static AndroidJavaObject usbContext;

        static GetAndroid()
            {
#if UNITY_ANDROID && !UNITY_EDITOR
            usbClass = new AndroidJavaClass("ruiyue.controller.sdk.ControllerUnity");///参数为安卓包名，根据不同包具体设置

            AndroidJavaClass unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");///unity主类名，固定，不需要修改
            usbContext = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
#endif
            }
        }

    protected static AndroidJavaClass GetAndroidClass
    {
        get
        {
            return GetAndroid.usbClass;
        }
    }

    protected static AndroidJavaObject GetAndroidContext
    {
        get 
        {
            return GetAndroid.usbContext;
        }
    }

        public static void onStart()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
		if(GetAndroidClass != null){
			GetAndroidClass.CallStatic("onStart", GetAndroidContext);
		}
#endif
        }

        public static void onResume()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
		if(GetAndroidClass != null){
			GetAndroidClass.CallStatic("onResume");
		}
#endif
        }

        public static void onPause()
        {
            if (isPause)
            {
#if UNITY_ANDROID && !UNITY_EDITOR
			if(GetAndroidClass != null){
		        GetAndroidClass.CallStatic("onPause");
			}
#endif
            }
            else
            {
                onResume();
            }
            isPause = !isPause;

        }

        public static void onStop()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
		if(GetAndroidClass != null){
			GetAndroidClass.CallStatic("onStop");
		}
#endif
        }

        static int[] keyAction;
        static int framecount=0;

        public static int[] getQuatKeyAction(int deviceId)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return GetAndroidClass.CallStatic<int[]>("getKeyAction", new object[] { deviceId });
#endif
            return new int[256];
        }

        public static int[] getKeyAction(int deviceId)
        {
#if UNITY_ANDROID //&& !UNITY_EDITOR
            if (GetAndroidClass != null)
            {
                bool isNoloLeft = isDeviceConn((int)CDevice.NOLO_TYPE.LEFT);
                bool isNoloRight = isDeviceConn((int)CDevice.NOLO_TYPE.RIGHT);

                if (isNoloLeft || isNoloRight)
                {
                    // nolo 键值
                    int[] keyStateLeft = null;
                    int[] keyStateRight = null;
                    if (isNoloRight)
                    {
                        keyStateRight = getKeyState((int)CDevice.NOLO_TYPE.RIGHT, deviceId);
                    }

                    if (isNoloLeft)
                    {
                        keyStateLeft = getKeyState((int)CDevice.NOLO_TYPE.LEFT, deviceId);
                    }

                    int[] tempKeyAction = new int[256];
                    if (keyStateLeft != null && keyStateRight != null)
                    {
                        for (int i = 0; i < CKeyEvent.KeyCodeIds.Length; i++)
                        {
                            int keyCode = CKeyEvent.KeyCodeIds[i];
                            tempKeyAction[keyCode] = 1;
                            if (keyStateLeft[keyCode] == 0 || keyStateRight[keyCode] == 0)
                            {
                                tempKeyAction[keyCode] = 0;
                            }
                        }
                    }
                    else if (keyStateLeft != null || keyStateRight != null)
                    {
                        int[] tempKeyState = keyStateLeft != null ? keyStateLeft : keyStateRight;
                        for (int i = 0; i < CKeyEvent.KeyCodeIds.Length; i++)
                        {
                            int keyCode = CKeyEvent.KeyCodeIds[i];
                            tempKeyAction[keyCode] = 1;
                            if (tempKeyState[keyCode] == 0)
                            {
                                tempKeyAction[keyCode] = 0;
                            }
                        }
                    }


                    return tempKeyAction;
                }
                else
                {
                    return GetAndroidClass.CallStatic<int[]>("getKeyAction", new object[] { deviceId });
                }
            }
#endif
            if (keyAction == null)
            {
                keyAction = new int[256];
                for (int i = 0; i < 256; i++)
                {
                    keyAction[i] = 1;
                }
            }
            return keyAction;
        }

        // deviceId=1, key=键值，value=0 按下/1 抬起
        public static int[] getKeyState(int type,int deviceId)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		if(GetAndroidClass != null){
		return GetAndroidClass.CallStatic<int []>("getKeyState", new object[] {type, deviceId});
		}
#endif
            return new int[256];
        }


        public static float[] getMotion(int deviceId)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
		if(GetAndroidClass != null){
			return GetAndroidClass.CallStatic<float []>("getMotion", new object[] { deviceId});
		}
#endif
            return new float[4];
        }

    // 3dof手柄姿态数据
	public static float[] getQuat(int deviceId)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
		if(GetAndroidClass != null){
			return GetAndroidClass.CallStatic<float []>("getQuat", new object[] { deviceId});
		}
#endif
            return new float[4];
        }

        public static int getGesture(int deviceId)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
		if(GetAndroidClass != null){
			return GetAndroidClass.CallStatic<int>("getGesture", new object[] { deviceId});
		}
#endif
            return 0xFF;
        }

        public static int getBlink(int deviceId)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
		if(GetAndroidClass != null){
			return GetAndroidClass.CallStatic<int>("getBlink", new object[] { deviceId});
		}
#endif
            return 0xFF;
        }

        public static int[] getBrain(int deviceId)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
		if(GetAndroidClass != null){
			return GetAndroidClass.CallStatic<int []>("getBrain", new object[] { deviceId});
		}
#endif
            return null;
        }

        public static void setFocus(bool focus)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
		if(GetAndroidClass != null){
			GetAndroidClass.CallStatic("setFocus", new object[] { focus });
		}
#endif
        }

        static TimeSpan ts1;
        static TimeSpan ts2;
        static bool isquated = false;
        static bool isQuat = false;
        static int checkQuatConnSeconds = 1;
        static int disconnectQuatCount = 0;

        public static bool isQuatConn()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (InteractionManager.Is3DofControllerConnected())
            {
                return true;
            }

            if (!isquated)
            {
                if (GetAndroidClass != null) {
                    isQuat = GetAndroidClass.CallStatic<bool>("isQuatConn");
                    ts1 = new TimeSpan(DateTime.Now.Ticks);
                    isquated = true;
                    return isQuat;
                }
                ts1 = new TimeSpan(DateTime.Now.Ticks);
                isquated = true;
            }
            if(ts1!=null)
            {
                ts2 = new TimeSpan(DateTime.Now.Ticks);
                TimeSpan ts = ts2.Subtract(ts1.Duration());
                if(ts.Seconds>=checkQuatConnSeconds)
                {
                    ts1= ts2;
                    isQuat = GetAndroidClass.CallStatic<bool>("isQuatConn");
                    // Debug.Log("isQuatConn---called=" + isQuat + "/" );

                    disconnectQuatCount = isQuat? 0 : (disconnectQuatCount+1);
                    if(disconnectQuatCount >= 60){
                         checkQuatConnSeconds = 5;
                    } else if(disconnectQuatCount >=120) {
                        checkQuatConnSeconds = 8;
                    } else {
                        checkQuatConnSeconds = 2;
                     }
                    // Debug.Log("=======>CallStatic isQuatConn");
                    return isQuat;
                }
                else
                {
                    return isQuat;
                }
            }

#endif
            if (PlayerCtrl.Instance != null && PlayerCtrl.Instance.debugInEditor) return true;
            return false;
        }

        public static void enableDebug(bool debug)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
		if(GetAndroidClass != null){
			GetAndroidClass.CallStatic("enableDebug",new object[]{debug});
		}
#endif
        }

        public static int[] getDeviceIds()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
		if(GetAndroidClass != null){
			return GetAndroidClass.CallStatic<int []>("getDeviceIds");
		}
#endif
            return new int[] { };
        }

        public static int getDeviceType()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
		if(GetAndroidClass != null){
			return GetAndroidClass.CallStatic<int>("getDeviceType");
		}
#endif
            return 0;
        }

	public static float[,] getCFiveDTechEvent(int deviceId = 1){
#if UNITY_ANDROID && !UNITY_EDITOR
		if(GetAndroidClass != null){
			return GetAndroidClass.CallStatic<float [,]>("getCFiveDTechEvent", new object[] { deviceId});
		}
#endif
            return new float[5, 4];
        }

     // {type, action, x, y}
	public static float[] getTouch(int deviceId = 1){
#if UNITY_ANDROID && !UNITY_EDITOR
		if(GetAndroidClass != null){
			return GetAndroidClass.CallStatic<float []>("getTouch", new object[] { deviceId});
		}
#endif
            return new float[4];
        }

    // deviceId=1，{ this.type, this.action, this.x, this.y }
	public static float[] getTouchEvent(int type,int deviceId = 1){
#if UNITY_ANDROID && !UNITY_EDITOR
		if(GetAndroidClass != null){
		return GetAndroidClass.CallStatic<float []>("getTouchEvent", new object[] { type,deviceId});
		}
#endif
            return new float[4];
        }

        public static void setMotionType(int motionType)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
		if(GetAndroidClass != null){
			GetAndroidClass.CallStatic("setMotionType",new object[]{motionType});
		}
#endif
        }

        public static float[] getCSensorEventValue(int type, int deviceId)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
		if(GetAndroidClass != null){
		return GetAndroidClass.CallStatic<float []>("getCSensorEvent", new object[] { type,deviceId});
		}
#endif
            return new float[3];
        }

        public static int getCBike(int deviceId)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
		if(GetAndroidClass != null){
		return GetAndroidClass.CallStatic<int>("getCBike", new object[] {deviceId});
		}
#endif
            return 0;
        }

        public static float[] getCPoseEvent(int type, int deviceId)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
		if(GetAndroidClass != null){
		return GetAndroidClass.CallStatic<float []>("getCPoseEvent", new object[] {type, deviceId});
		}
#endif
            return new float[8];
        }

        // nolo 手柄是否连接
        private static int[] deviceConnCall = new int[5];
        private static int[] isDeviceConnTmp = new int[5] { -1, -1, -1, -1, -1 };
        // 左右手柄调用次数达到120就更新1次状态
        public static bool IsNoloConn()
        {
#if UNITY_EDITOR
            if (PlayerCtrl.Instance != null && PlayerCtrl.Instance.debugInEditor) return true;
#endif
            return isDeviceConn((int)CDevice.NOLO_TYPE.LEFT) || isDeviceConn((int)CDevice.NOLO_TYPE.RIGHT);
        }

        static int NoloTypeToHandType(int noloType)
        {
            return noloType == (int)CDevice.NOLO_TYPE.LEFT ? (int)InteractionManager.NACTION_HAND_TYPE.HAND_LEFT :
                 (int)InteractionManager.NACTION_HAND_TYPE.HAND_RIGHT;
        }

        public static bool isDeviceConn(int noloType)
        {
            if(InteractionManager.IsInteractionSDKEnabled())
            {
                return InteractionManager.IsNoloControllerConnected(NoloTypeToHandType(noloType));
            }

            bool res = false;
            if (deviceConnCall[noloType] >= 250 || isDeviceConnTmp[noloType] < 0)
            {
#if UNITY_ANDROID && !UNITY_EDITOR
		if(GetAndroidClass != null){
		res = GetAndroidClass.CallStatic<bool>("isDeviceConn", new object[] { noloType});
		}
#endif
                isDeviceConnTmp[noloType] = res ? 1 : 0;
                deviceConnCall[noloType] = 0;
                // Debug.Log("isDeviceConn---called=" + res + "/" + noloType);
            }
            else
            {
                res = isDeviceConnTmp[noloType] == 1;
            }

            deviceConnCall[noloType] = deviceConnCall[noloType] + 1;
            return res;
        }

        public static AndroidJavaObject getCSupportDevices()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
		if(GetAndroidClass != null){
		return GetAndroidClass.CallStatic<AndroidJavaObject>("getCSupportDevices");
		}
#endif
            return null;
        }

        public static AndroidJavaObject getCBikeEvent(int deviceId)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
		if(GetAndroidClass != null){
		return GetAndroidClass.CallStatic<AndroidJavaObject>("getCBikeEvent", new object[] { deviceId});
		}
#endif
            return null;
        }

        public static AndroidJavaObject getCSensorEvent(int type, int deviceId)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
		if(GetAndroidClass != null){
		return GetAndroidClass.CallStatic<AndroidJavaObject>("getCSensorEvent2", new object[] { type, deviceId});
		}
#endif
            return null;
        }

        public static AndroidJavaObject getCDevices()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
		if(GetAndroidClass != null){
		return GetAndroidClass.CallStatic<AndroidJavaObject>("getCDevices");
		}
#endif
            return null;
        }

        public static int getHandMode()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
		if(GetAndroidClass != null){
		return GetAndroidClass.CallStatic<int>("getHandMode");
		}
#endif
            return 0;
        }

        public static void connect(AndroidJavaObject bluetoothDevice)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
		if(GetAndroidClass != null){
		GetAndroidClass.CallStatic("connect", bluetoothDevice);
		}
#endif
        }

        public static void disconnect(AndroidJavaObject bluetoothDevice)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
		if(GetAndroidClass != null){
		GetAndroidClass.CallStatic("disconnect", bluetoothDevice);
		}
#endif
        }

    // 连接回调，如果已经连接了再进入app，不会有回调，进入后如果插拔连接，还是有回调
	public static void setOnDeviceListener(OnDeviceListener listener)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		if(GetAndroidClass != null){
		GetAndroidClass.CallStatic("setOnDeviceListener", listener);
		}
#endif
        }

        static TimeSpan powerRefreshTime;
        static int cachePower;
        // 60s更新1次
        public static int getControllerPower()
        {
            bool isFirstTrigger = false;
            if (powerRefreshTime.Ticks == 0)
            {
                isFirstTrigger = true;
                // 初次触发更新
                powerRefreshTime = new TimeSpan(DateTime.Now.Ticks);
            }

            TimeSpan curTime = new TimeSpan(DateTime.Now.Ticks);
            TimeSpan diff = curTime.Subtract(powerRefreshTime).Duration();
            if (isFirstTrigger || diff.Seconds >= 50)
            {
                powerRefreshTime = curTime;
                try
                {
#if UNITY_ANDROID && !UNITY_EDITOR
		if(GetAndroidClass != null){
		    cachePower =  GetAndroidClass.CallStatic<int>("getControllerPower", 1);
		}
#endif
                   Debug.Log("cachePower=" + cachePower);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }


            return Math.Max(0,cachePower);
        }

        static int[] noloCachePower = new int[4];
        public static int getNoloControllerPower(CDevice.NOLO_TYPE type)
        {
            bool isFirstTrigger = false;
            if (powerRefreshTime.Ticks == 0)
            {
                isFirstTrigger = true;
                // 初次触发更新
                powerRefreshTime = new TimeSpan(DateTime.Now.Ticks);
            }

            TimeSpan curTime = new TimeSpan(DateTime.Now.Ticks);
            TimeSpan diff = curTime.Subtract(powerRefreshTime).Duration();
            if (isFirstTrigger || diff.Seconds >= 50)
            {
                powerRefreshTime = curTime;
                try
                {
#if UNITY_ANDROID && !UNITY_EDITOR
		if(GetAndroidClass != null){
		    noloCachePower[ (int) type] =  GetAndroidClass.CallStatic<int>("getNoloBattery", (int) type);
		}
#endif
                    Debug.Log("nolo cachePower=" + noloCachePower[(int)type]);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }


            return Math.Max(0, noloCachePower[(int)type]);
        }


        //摇杆Y值小于-0.9时判断 摇杆向上
        public static bool IsStickUp(float leftStickY)
        {
            return leftStickY < -0.9f;
        }

        //摇杆Y值大于0.9时判断 摇杆向上
        public static bool IsStickDown(float leftStickY)
        {
            return leftStickY > 0.9f;
        }

        //摇杆X值小于-0.9时判断 摇杆向左
        public static bool IsStickLeft(float leftStickY)
        {
            return leftStickY < -0.9f;
        }

        //摇杆X值大于0.9时判断 摇杆向右
        public static bool IsStickRight(float leftStickX)
        {
            return leftStickX > 0.9f;
        }

    }
}