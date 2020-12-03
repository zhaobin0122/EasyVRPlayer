using System;
using UnityEngine;
using XR;

namespace NibiruTask
{
    public class InteractionManager
    {
        public struct ControllerConfig
        {
            public string modelPath;
            public float[] modelPosition;
            public float[] modelRotation;
            public float[] modelScale;
            public float[] batteryPosition;
            public float[] batteryRotation;
            public float[] batteryScale;
            public float[] rayStartPosition;
            public float[] rayEndPosition;
            public string sourceString;
            public string objPath;
            public string mtlPath;
            public string pngPath;
        }

        public static AndroidJavaObject managerContext;

        static ControllerConfig mControllerConfig;
        static void InitManagerContext()
        {
            if (managerContext == null)
            {     
                for(int i=0; i < 256; i++)
                {
                    // 默认up
                    keystate[i] = 1;
                    keystateLeft[i] = 1;
                    keystateRight[i] = 1;
                }
#if UNITY_ANDROID && !UNITY_EDITOR
                managerContext = new AndroidJavaObject("com.nibiru.lib.xr.unity.NibiruInteractionSDK");
#endif
            }
        }

        private static int cacheEnabled = -1;
        public static bool IsInteractionSDKEnabled()
        {
            InitManagerContext();

            if (managerContext == null) return false;
            if (cacheEnabled < 0)
            {
                cacheEnabled = managerContext.Call<bool>("isInteractionSDKEnabled") ? 1 : 0;
            }
            return cacheEnabled == 1;
        }

        public static int GetControllerDeviceType()
        {
            InitManagerContext();

            if (managerContext == null) return -1;
            return managerContext.Call<int>("getControllerModelType");
        }

        public static bool IsSupportControllerModel()
        {
            InitManagerContext();

            if (managerContext == null) return false;
            bool isSpt = managerContext.Call<bool>("isSupportControllerModel");
            if (!isSpt) return false;
            ControllerConfig cfg = GetControllerConfig();
            return cfg.modelPath != null;
        }

        /// <summary>
        ///  [0]=obj
        ///  [1]=mtl
        ///  [2]=png
        /// </summary>
        /// <returns></returns>
        public static ControllerConfig GetControllerConfig()
        {
            if (Application.isEditor)
            {
                mControllerConfig.modelPath = Application.dataPath + "/NibiruTask/Resources/Controller/Objs";
                mControllerConfig.objPath = mControllerConfig.modelPath + "/controller_model.obj";
                mControllerConfig.mtlPath = mControllerConfig.modelPath + "/controller_model.mtl";
                mControllerConfig.pngPath = mControllerConfig.modelPath + "/controller_model.png";
                mControllerConfig.modelPosition = new float[3];
                mControllerConfig.modelRotation = new float[3];
                mControllerConfig.modelScale = new float[] { 1,1,1};
                mControllerConfig.batteryPosition = new float[3];
                mControllerConfig.batteryRotation = new float[3];
                mControllerConfig.batteryScale = new float[] { 1, 1, 1 };
                mControllerConfig.rayStartPosition = new float[3];
                mControllerConfig.rayEndPosition = new float[3];
            }
            InitManagerContext();
            if(managerContext != null && mControllerConfig.modelPath == null)
            {
                string source = managerContext.Call<string>("getControllerModelConfig", 1);
                if (source == null)
                {
                    mControllerConfig.modelPath = null;
                    return mControllerConfig;
                }
                string[] dataArray = source.Split(',');
                mControllerConfig.modelPath = dataArray[0];

                mControllerConfig.modelPosition = new float[3];
                mControllerConfig.modelPosition[0] = float.Parse(dataArray[1]);
                mControllerConfig.modelPosition[1] = float.Parse(dataArray[2]);
                mControllerConfig.modelPosition[2] = float.Parse(dataArray[3]);

                mControllerConfig.modelRotation = new float[3];
                mControllerConfig.modelRotation[0] = float.Parse(dataArray[4]);
                mControllerConfig.modelRotation[1] = float.Parse(dataArray[5]);
                mControllerConfig.modelRotation[2] = float.Parse(dataArray[6]);

                mControllerConfig.modelScale = new float[3];
                mControllerConfig.modelScale[0] = float.Parse(dataArray[7]);
                mControllerConfig.modelScale[1] = float.Parse(dataArray[8]);
                mControllerConfig.modelScale[2] = float.Parse(dataArray[9]);

                mControllerConfig.batteryPosition = new float[3];
                mControllerConfig.batteryPosition[0] = float.Parse(dataArray[10]);
                mControllerConfig.batteryPosition[1] = float.Parse(dataArray[11]);
                mControllerConfig.batteryPosition[2] = float.Parse(dataArray[12]);

                mControllerConfig.batteryRotation = new float[3];
                mControllerConfig.batteryRotation[0] = float.Parse(dataArray[13]);
                mControllerConfig.batteryRotation[1] = float.Parse(dataArray[14]);
                mControllerConfig.batteryRotation[2] = float.Parse(dataArray[15]);

                mControllerConfig.batteryScale = new float[3];
                mControllerConfig.batteryScale[0] = float.Parse(dataArray[16]);
                mControllerConfig.batteryScale[1] = float.Parse(dataArray[17]);
                mControllerConfig.batteryScale[2] = float.Parse(dataArray[18]);

                mControllerConfig.rayStartPosition = new float[3];
                mControllerConfig.rayStartPosition[0] = float.Parse(dataArray[19]);
                mControllerConfig.rayStartPosition[1] = float.Parse(dataArray[20]);
                mControllerConfig.rayStartPosition[2] = float.Parse(dataArray[21]);

                mControllerConfig.rayEndPosition = new float[3];
                mControllerConfig.rayEndPosition[0] = float.Parse(dataArray[22]);
                mControllerConfig.rayEndPosition[1] = float.Parse(dataArray[23]);
                mControllerConfig.rayEndPosition[2] = float.Parse(dataArray[24]);

                // obj/mtl/png
                mControllerConfig.objPath = mControllerConfig.modelPath + "/controller_model.obj";
                mControllerConfig.mtlPath = mControllerConfig.modelPath + "/controller_model.mtl";
                mControllerConfig.pngPath = mControllerConfig.modelPath + "/controller_model.png";
            }
            return mControllerConfig;
        }

        public enum NACTION_CONNECT_STATE
        {
            DISCONNECT, CONNECT, CONNECTING, DISCONNECTING
        }

        public enum NACTION_CONTROLLER_ACTION
        {
            DOWN, UP, MOVE
        }
        public enum NACTION_HAND_TYPE
        {
            HAND_LEFT,//左手
            HAND_RIGHT,//右手
            HEAD,
            NONE
        }

        // 检查当前左右手模式
        public static bool IsLeftHandMode() {
            return currentHandMode == HAND_MODE_LEFT && IsLeftControllerConnected();
        }

        public static bool IsRightHandMode()
        {
            return currentHandMode == HAND_MODE_RIGHT && IsRightControllerConnected();
        }

        public static NACTION_HAND_TYPE GetHandTypeByHandMode()
        {
            if (IsLeftHandMode()) return NACTION_HAND_TYPE.HAND_LEFT;
            if (IsRightHandMode()) return NACTION_HAND_TYPE.HAND_RIGHT;
            return NACTION_HAND_TYPE.HAND_RIGHT;
        }

        // hand代表左/右手，hand=0->左手，hand=1->右手，float数组为长度为4的四元数+长度为3的位移
        // 四元数的顺序: x-y-z-w
        public static float[] GetControllerPose(NACTION_HAND_TYPE handType = NACTION_HAND_TYPE.HAND_RIGHT)
        {
            InitManagerContext();
            if (managerContext == null || !IsControllerConnected()) return new float[] { 0, 0, 0, 0, 0, 0, 0 };
           
            if (handType == NACTION_HAND_TYPE.HAND_LEFT && !IsLeftControllerConnected())
            {
                return new float[] { 0, 0, 0, 0, 0, 0, 0 };
            }
            if (handType == NACTION_HAND_TYPE.HAND_RIGHT && !IsRightControllerConnected())
            {
                return new float[] { 0, 0, 0, 0, 0, 0, 0 };
            }
            return managerContext.Call<float[]>("getControllerPose", (int)handType);
        }

        public static bool IsControllerConnected()
        {
            return IsInteractionSDKEnabled() && (IsLeftControllerConnected() || IsRightControllerConnected());
        }

        public static bool Is3DofControllerConnected()
        {
            if (!IsInteractionSDKEnabled()) return false;
            if (currentHandMode == -1) return false;
            if ((currentHandMode == HAND_MODE_LEFT && IsLeftControllerConnected()) ||
               (currentHandMode == HAND_MODE_RIGHT && IsRightControllerConnected()))
            {
                return true;
            }
            return false;
        }

        public static bool IsLeftControllerConnected()
        {
            return IsInteractionSDKEnabled() && connectLeftHand == 1;
        }

        public static bool IsRightControllerConnected()
        {
            return IsInteractionSDKEnabled() && connectRightHand == 1;
        }

        public static bool IsNoloControllerConnected(int handType)
        {
            if (!IsNoloDevice)
            {
                return false;
            }

            //Debug.Log("IsNoloControllerConnected." + handType + "," + IsLeftControllerConnected() + "/" + IsRightControllerConnected()
            //    +"/" + connectLeftHand + "/" + connectRightHand);

            if (handType == (int)NACTION_HAND_TYPE.HAND_LEFT)
            {
                return IsLeftControllerConnected();
            }
            if (handType == (int)NACTION_HAND_TYPE.HAND_RIGHT)
            {
                return IsRightControllerConnected();
            }
            return false;
        }

        // 连接状态
        private static int connectLeftHand = -1;
        private static int connectRightHand = -1;
        private static int currentHandMode = -1;
        static int HAND_MODE_RIGHT = 0;
        static int HAND_MODE_LEFT = 1;
        // deviceName=NOLO CV1 HEAD
        public static bool IsNoloDevice { set; get; }

        public static void OnDeviceConnectState(string connectInfo)
        {
            currentHandMode = GetControllerHandMode();
            // msgId_state_handType_deviceName:10/11/12
            string[] data = connectInfo.Split('_');
            int state = int.Parse(data[1]);
            int handType = int.Parse(data[2]);
            string deviceName = data[3];

            if (deviceName != null && deviceName.Contains("NOLO"))
            {
                IsNoloDevice = true;
            }
            Debug.Log("OnDeviceConnectState: " + connectInfo + ", HandMode: " + currentHandMode + "(0=right,1=left)" + ", IsNoloDevice: " + IsNoloDevice);
            if (state == (int)NACTION_CONNECT_STATE.CONNECT && handType == (int)NACTION_HAND_TYPE.HAND_LEFT)
            {
                connectLeftHand = 1;
                Debug.Log("Left Controller Connect");
            }
            else if (state == (int)NACTION_CONNECT_STATE.CONNECT && handType == (int)NACTION_HAND_TYPE.HAND_RIGHT)
            {
                connectRightHand = 1;
                Debug.Log("Right Controller Connect");
            }
            else if (state == (int)NACTION_CONNECT_STATE.DISCONNECT && handType == (int)NACTION_HAND_TYPE.HAND_LEFT)
            {
                connectLeftHand = -1;
                Debug.Log("Left Controller DisConnect");
            }
            else if (state == (int)NACTION_CONNECT_STATE.DISCONNECT && handType == (int)NACTION_HAND_TYPE.HAND_RIGHT)
            {
                connectRightHand = -1;
                Debug.Log("Right Controller DisConnect");
            }
        }

        // private int ACTION_DOWN = 0;
        // private int ACTION_UP = 1;
        // Default is Up
        private static int[] keystate = new int[256];
        private static int[] keystateLeft = new int[256];
        private static int[] keystateRight = new int[256];
        /// <summary>
        /// 
        /// 水平方向： (Left->Right) -1~1
        /// 垂直方向： (Up->Down) -1~1
        /// </summary>
        public static Vector2 TouchPadPosition { get; set; }
        public static Vector2 TouchPadPositionLeft { get; set; }
        public static Vector2 TouchPadPositionRight { get; set; }

        public static void Reset()
        {
            for (int i = 0; i < 256; i++)
            {
                // 默认up
                keystate[i] = CKeyEvent.ACTION_UP;
                keystateLeft[i] = CKeyEvent.ACTION_UP;
                keystateRight[i] = CKeyEvent.ACTION_UP;
            }
            TouchPadPosition = Vector2.zero;
            TouchPadPositionLeft = Vector2.zero;
            TouchPadPositionRight = Vector2.zero;
        }

        public static int[] GetKeyAction()
        {
            int[] tempKeyAction = new int[256];
            Array.Copy(keystate, tempKeyAction, keystate.Length);
            return tempKeyAction;
        }

        public static int[] GetKeyAction(int handType)
        {
            int[] tempKeyAction = new int[256];
       
            if (handType == (int)NACTION_HAND_TYPE.HAND_LEFT)
            {
                Array.Copy(keystateLeft, tempKeyAction, keystateLeft.Length);
            }
            if (handType == (int)NACTION_HAND_TYPE.HAND_RIGHT)
            {
                Array.Copy(keystateRight, tempKeyAction, keystateRight.Length);
            }
            return tempKeyAction;
        }

        public static void OnCKeyEvent(string keyCodeInfo)
        {
            // msgId_action_keyCode
            Debug.Log("OnCKeyEvent: " + keyCodeInfo);
            string[] data = keyCodeInfo.Split('_');
            int action = int.Parse(data[1]);
            int keyCode = int.Parse(data[2]);
            NACTION_HAND_TYPE handType = int.Parse(data[3]) == 1 ? NACTION_HAND_TYPE.HAND_LEFT : NACTION_HAND_TYPE.HAND_RIGHT;
            // 1=left.2=right
            keystate[keyCode] = action;
            if(handType == NACTION_HAND_TYPE.HAND_LEFT)
            {
                keystateLeft[keyCode] = action;
            } else if(handType == NACTION_HAND_TYPE.HAND_RIGHT)
            {
                keystateRight[keyCode] = action;
            }
        }

        // action : 2=move,1=up
        //        -1
        // -1 --/-- 1
        //      1
        public static void OnCTouchEvent(string touchInfo)
        {
            //1016_2_0.0_0.0_2
            // msgId_action_x_y
            // Debug.Log("OnCTouchEvent: " + touchInfo);
            string[] data = touchInfo.Split('_');
            int action = int.Parse(data[1]);
            float x = (float) Math.Round(double.Parse(data[2]), 4);
            float y = (float) Math.Round(double.Parse(data[3]), 4);
            NACTION_HAND_TYPE handType = int.Parse(data[4]) == 1 ? NACTION_HAND_TYPE.HAND_LEFT : NACTION_HAND_TYPE.HAND_RIGHT;
            TouchPadPosition = new Vector2(x, y);
            TouchPadPositionLeft = new Vector2(x, y);
            TouchPadPositionRight = new Vector2(x, y);
            if (action == CKeyEvent.ACTION_MOVE)
            {
                keystate[CKeyEvent.KEYCODE_CONTROLLER_TOUCHPAD_TOUCH] = CKeyEvent.ACTION_DOWN;
                if (handType == NACTION_HAND_TYPE.HAND_LEFT)
                {
                    keystateLeft[CKeyEvent.KEYCODE_CONTROLLER_TOUCHPAD_TOUCH] = CKeyEvent.ACTION_DOWN;
                }
                else if (handType == NACTION_HAND_TYPE.HAND_RIGHT)
                {
                    keystateRight[CKeyEvent.KEYCODE_CONTROLLER_TOUCHPAD_TOUCH] = CKeyEvent.ACTION_DOWN;
                }
            } else if(action == CKeyEvent.ACTION_UP)
            {
                keystate[CKeyEvent.KEYCODE_CONTROLLER_TOUCHPAD_TOUCH] = CKeyEvent.ACTION_UP;
                if (handType == NACTION_HAND_TYPE.HAND_LEFT)
                {
                    keystateLeft[CKeyEvent.KEYCODE_CONTROLLER_TOUCHPAD_TOUCH] = CKeyEvent.ACTION_UP;
                }
                else if (handType == NACTION_HAND_TYPE.HAND_RIGHT)
                {
                    keystateRight[CKeyEvent.KEYCODE_CONTROLLER_TOUCHPAD_TOUCH] = CKeyEvent.ACTION_UP;
                }
            }
        }

        // 0=left,1=right
        private static int[] powerCallCount = new int[] { -1, -1, -1, -1 };
        private static int[] powerCacheValue = new int[] { -1, -1, -1, -1 };
        // 3dof手柄不区分左右手，返回结果一致
        // 1s=60, 60s更新1次
        public static int GetControllerPower(NACTION_HAND_TYPE handType = NACTION_HAND_TYPE.HAND_RIGHT)
        {
            InitManagerContext();
            if (managerContext == null) return 0;
            if (powerCallCount[(int)handType] < 0 || powerCallCount[(int)handType] > 3600)
            {
                powerCacheValue[(int)handType] = managerContext.Call<int>("getControllerBatteryLevel", (int)handType);
                powerCallCount[(int)handType] = 0;
            }
            powerCallCount[(int)handType] = powerCallCount[(int)handType] + 1;
            return Mathf.Max(powerCacheValue[(int)handType], 0);
        }

        public static int GetControllerHandMode()
        {
            InitManagerContext();
            if (managerContext == null) return -1;
            return managerContext.Call<int>("getControllerHandMode");
        }

    }
}
