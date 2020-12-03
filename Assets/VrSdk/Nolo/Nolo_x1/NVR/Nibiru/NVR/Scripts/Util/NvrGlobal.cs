// Copyright 2016 Nibiru. All rights reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
///  全局变量
/// </summary>
namespace Nvr.Internal
{
    class NvrGlobal
    {
        public class Permission
        {
            public static string CAMERA = "android.permission.CAMERA";
            public static string WRITE_EXTERNAL_STORAGE = "android.permission.WRITE_EXTERNAL_STORAGE";
            public static string READ_EXTERNAL_STORAGE = "android.permission.READ_EXTERNAL_STORAGE";
            public static string ACCESS_COARSE_LOCATION = "android.permission.ACCESS_COARSE_LOCATION";
            public static string ACCESS_NETWORK_STATE = "android.permission.ACCESS_NETWORK_STATE";
            public static string WRITE_SETTINGS = "android.permission.WRITE_SETTINGS";
            public static string BLUETOOTH = "android.permission.BLUETOOTH";
            public static string BLUETOOTH_ADMIN = "android.permission.BLUETOOTH_ADMIN";
            public static string INTERNET = "android.permission.INTERNET";
            public static string GET_TASKS = "android.permission.GET_TASKS";
            public static string RECORD_AUDIO = "android.permission.RECORD_AUDIO";
            public static string READ_PHONE_STATE = "android.permission.READ_PHONE_STATE";
        }
        public static NibiruService nibiruService = null;
        /// <summary>
        ///  默认的z距离
        /// </summary>
        public static float defaultGazeDistance = 50;

        // 是否初始化陀螺仪接口
        public static bool trackerInited = false;
        // nvr是否初始化
        public static bool nvrStarted = false;
        // 运行环境是否支持dtr
        public static bool supportDtr = false;
        // 加载哪个库
        public static bool useNvrSo = false;
        // 畸变开关
        public static bool distortionEnabled = true;

        public static float refreshRate = -1;
        // DFT光学参数 {inter_lens_distance,vertical_distance_to_lens_center,screen_to_lens_distance,fov,distortion_coef_x,distortion_coef_y,screen_width,screen_height
        //  border_size_meters,screen_size_x,screen_size_y,inter_lens_distance_v2,screen_to_lens_distance_v2,distortion_coef_v2_k1,distortion_coef_v2_k2,lens_ipd,distortion_mesh_x,distortion_mesh_y}
        public static float[] dftProfileParams = new float[21];

        public static float fovNear = -1;
        public static float fovFar = -1;

        // 渠道标识
        public static string channelCode = "";
        // JAR版本号
        public static int jarVersion = -1;
        // SO版本号
        public static int soVersion = -1;
        // 平台ID
        public static int platformID = -1;
        // 平台性能等级
        public static int platPerformanceLevel = -1;

        // 当前白点选中物体距离
        public static float focusObjectDistance = defaultGazeDistance;

        public static bool isVR9Platform = false;

        public static string KeyEvent_KEYCODE_ALT_LEFT = "57";// 左侧按键
        public static string KeyEvent_KEYCODE_MEDIA_RECORD = "130";// 右侧按键
    }

    public enum PERFORMANCE
    {   // H8&V700=0,RK3288&S900&Intel T3=1,RK3399&Samsung7420&Intel T4 &MTK=2
        LOW = 0,
        NORMAL =1,
        HIGH = 2,
    }

    public enum PLATFORM
    {
        GENERAL = 0x0000,
        RK_3288_CG = 0x0001,
        ACT_S900 = 0x0002,
        SAMSUNG = 0x0003,
        INTEL_T3 = 0x0004,
        INTEL_T4 = 0x0005,
        MTK_X20 = 0x0006,
        QUALCOMM = 0x0007,
        RK_3399 = 0x0008,
        SAMSUNG_8890VR = 0x0009,
        PLATFORM_SAMSUNG_8895 = 0x000a,  //10
        PLATFORM_VR9 =  0x000b  //11
    }

    public enum JARVERSION
    {
       // 161228 [增加版本号]
       JAR_161228 = 161228,

    }

    public enum SOVERSION
    {
        // 161228 [增加版本号]
        SO_1228 = 161228,
    }

   public enum GazeTag
    {
        Show = 0,
        Hide = 1,
        Set_Distance = 2,
        Set_Size = 3,
        Set_Color = 4
    }

    public enum GazeSize
    {
        Original = 0,
        Large = 1,
        Medium = 2,
        Small= 3
    }

    public enum TextureQuality
    {
        Simple = 2,
        Good = 0,
        Better = 3,
        Best = 1
    }

    public enum TextureMSAA
    {
        NONE=1,
        MSAA_2X=2,
        MSAA_4X=4,
        MSAA_8X=8
    }

    public enum FrameRate
    {
        FPS_60 = 60,
        FPS_72 = 72,
        FPS_75 = 75,
        FPS_90 = 90
    }

    public enum FunctionKeyCode
    {
        NF1 = 131,// F1=57    左侧键
        NF2 = 132, // F2=130  右侧键
        TRIGGER = 133, // 手柄trigger
        TOUCHPAD_TOUCH=134, // 手柄touchpad
        MENU=135, // 手柄menu
        TOUCHPAD=136, // 手柄Touchpad
        VOLUMN_DOWN=137,// 手柄音量
        VOLUMN_UP = 138// 手柄音量
    }

    public enum DISPLAY_MODE
    {
        MODE_2D = 0,
        MODE_3D = 1
    }
  

    /// <summary>
    /// 消息ID
    /// </summary>
    public enum MSG_ID
    {
        MSG_verifySucc = 1000,
        MSG_verifyFailed = 1001,
        MSG_onKeyStoreException = 1002,
        MSG_onGestureEvent = 1003,
        MSG_onVoiceBegin = 1004,
        MSG_onVoiceEnd = 1005,
        MSG_onVoiceFinishResult = 1006,
        MSG_onVoiceFinishError = 1007,
        MSG_onVoiceVolume = 1008,
        MSG_onServiceReady = 1009,
        MSG_onHeadPosition = 1010,
        MSG_onMarkerLoadStatus = 1011,
        MSG_onVoiceCancel = 1012,
        MSG_onServerApiReady = 1013,
        MSG_onSysSleepApiReady = 1014,
        //
        MSG_onInteractionKeyEvent = 1015,
        MSG_onInteractionTouchEvent = 1016,
        MSG_onInteractionDeviceConnectEvent = 1017,

        MSG_onGestureHoverEvent = 1018
    }

    public enum MARKER_LOAD_STATUS
    {
        SUCCESS = 1,
        LIB_ERROR = 2,
        CAMERA_BUSY = 3
    }

    /// <summary>
    /// 手势ID
    /// </summary>
    public enum GESTURE_ID
    {
        BASE_ID = 100,
        OPEN_HAND,  //101
        CLOSE_HAND,//102
        PINCH_SIGN,//103
        THUMBS_UP,//104
        //LEFT,//105
        //RIGHT,//106
        //GESTURE_V_SIGN//107
        LOST=113
    }

    /// <summary>
    /// 语音识别的语言类型
    /// </summary>
    public enum VOICE_LANGUAGE
    {
        CHINESE = 1536,
        ENGLISH = 1736
    }

    public enum SERVICE_TYPE
    {
        VOICE = 3,
        SIX_DOF = 5,
        GESTURE = 6
    }

    public enum SENSOR_LOCATION
    {
        HMD,// 头显
        CONTROLLER, // 主机
        NONE
    }

    //头控方式
    public enum HeadControl
    {
        GazeSystem = 0,
        GazeApplication = 1,
        Hover = 2,
        Controller=3
    }

    public enum SENSOR_TYPE
    {
        UNKNOWN,
        ACCELEROMETER,
        GYROSCOPE,
        MAGNETIC_FIELD
    }

    public enum CAMERA_ID
    {
        FRONT = 1,
        BACK = 0
    }

    public enum VIDEO_SIZE
    {
        V480P = 1,
        V720P = 2,
        V1080P = 3
    }

    public enum MARKER_CAMERA_ZOOM
    {
        NED = 16,
        BLL = 0
    }

    public enum TRACKING_MODE
    {
        ROTATION = 0,
        POSITION = 1
    }

    public enum PLUGIN_ID
    {
        SIX_DOF = 1,
        VOICE = 2,
        GESTURE = 3,
        RECORD = 6,
        MARKER = 7,
        BASIS = 8,
        RECOGINIZE = 9
    }

    public enum HMD_TYPE
    {
        VR = 0,
        AR = 1,
        NONE = 2
    }

    public enum ControllerSupportMode
    {
        NONE, ALL, THREE_DOF, NOLO_SIX_DOF, THREE_DOF_AND_NOLO_SIX_DOF
    };

}