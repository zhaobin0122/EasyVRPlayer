using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NibiruTask
{
    public enum TASK_ACTION
    {
        VIDEO_PLAY = 101,
        OPEN_FILE,
        SHOW_IMAGE,
        SETTINGS,
        EXPLORER,
        DEVICE_DRIVER
    }

    public enum SELECTION_TASK_ACTION
    {
        FILE = 1001
    }

    public enum START_METHOD
    {
        UNKNOWN,
        ACTIVITY,
        ACTIVITY_RESULT,
        BROADCAST
    }

    //定义返回值：成功/失败
    public enum SELECTION_RESULT
    {
        UNKNOWN,
        OK,
        CANCEL,
        ERROR
    }

    public class Video
    {
        //定义参数KEY：播放器功能
        public const string VIDEO_KEY_CONTROL = "video_play_control";
        //定义参数VALUE：播放器功能：打开视频播放器/暂停视频播放/继续视频播放/结束视频播放/快进快退/显示隐藏播控面板
        public const string VIDEO_CONTROL_START = "com.nibiru.videostart";
        public const string VIDEO_CONTROL_PAUSE = "com.nibiru.video.pause";
        public const string VIDEO_CONTROL_RESUME = "com.nibiru.video.resume";
        public const string VIDEO_CONTROL_CLOSE = "com.nibiru.videofinish";
        public const string VIDEO_CONTROL_SEEKTO = "com.nibiru.video.seekto";
        public const string VIDEO_CONTROL_HIDE_CONTROLLER = "com.nibiru.video.hidecontroller";
        //定义参数KEY：播放类型
        public const string VIDEO_KEY_PARAMETERS_TYPE = "TYPE_2DOR3D";
        //定义参数VALUE：播放类型： 2D/3D，对应Intent参数
        public const int VIDEO_PARAMETERS_TYPE_2D = 0;
        public const int VIDEO_PARAMETERS_TYPE_3D = 1;
        //定义参数KEY：播放模式
        public const string VIDEO_KEY_PARAMETERS_MODE = "TYPE_MODEL";
        //定义参数VALUE：播放模式：普通平面/全景/球幕，对应Intent参数
        public const int VIDEO_PARAMETERS_MODE_NORMAL = 0;
        public const int VIDEO_PARAMETERS_MODE_360 = 1;
        public const int VIDEO_PARAMETERS_MODE_180 = 2;
        public const int VIDEO_PARAMETERS_MODE_FULLDOME = 3;
        //定义参数KEY：解码模式
        public const string VIDEO_KEY_PARAMETERS_DECODE = "TYPE_DECODE";
        //定义参数VALUE：解码模式，硬解码/软解码
        public const int VIDEO_PARAMETERS_DECODE_HARDWARE = 0;
        public const int VIDEO_PARAMETERS_DECODE_SOFTWARE = 1;
        //定义参数KEY：视频路径
        public const string VIDEO_KEY_PATH = "PATH";
        //定义参数KEY：开启循环播放
        public const string VIDEO_KEY_LOOP = "LOOP";
        //定义参数VALUE: 开启循环播放: 否/是
        public const int VIDEO_KEY_LOOP_OFF = 0;
        public const int VIDEO_KEY_LOOP_ON = 1;
        /******以下参数用于 VIDEO_CONTROL_SEEKTO ******/
        //定义参数KEY：快进时间
        public const string VIDEO_KEY_SEEKTO_TIME = "time";
        /******以下参数用于 VIDEO_CONTROL_HIDE_CONTROLLER ******/
        //定义参数KEY: 是否显示播控条
        public const string VIDEO_KEY_CONTROLLER = "hideController";
        //定义参数VALUE：是否显示播控条：是/否
        public const string VIDEO_KEY_CONTROLLER_HIDE = "true";
        public const string VIDEO_KEY_CONTROLLER_SHOW = "false";
    }

    public class File
    {
        //定义文件打开功能的Intent Action名称，用于构造Intent
        public const string OPEN_FILE_ACTION_NAME = "com.nibiru.vrfilemanager.action.VEFILEMANAGER";
        //定义参数KEY：文件路径
        public const string OPEN_FILE_KEY_PATH = "path";

        //定义参数KEY：文件类型
        public const string OPEN_FILE_KEY_TYPE = "fileType";
        //定义参数VALUE: 文件类型：视频/图片/apk
        public const int FILE_TYPE_VIDEO = 0;
        public const int FILE_TYPE_IMAGE = 1;
        public const int FILE_TYPE_APK = 2;

        //获取路径返回值的KEY
        public const string FILE_KEY_SELECTION_RESULT = "Path";
    }

    public class Gallery
    {
        //定义图片打开功能的Intent Action名称，用于构造Intent
        public const string SHOW_IMAGE_ACTION_NAME = "com.nibiru.action.IMAGE_SHOW";
        //定义参数KEY：图片路径
        public const string SHOW_IMAGE_KEY_PATH = "path";
        //定义参数KEY：图片格式
        public const string SHOW_IMAGE_KEY_TYPE = "type";
        //定义参数VALUE: 图片的格式: 2D/3D/360度
        public const int SHOW_IMAGE_KEY_2D = 0;
        public const int SHOW_IMAGE_KEY_3D = 1;
        public const int SHOW_IMAGE_KEY_360 = 2;
    }

    public class Setting
    {
        //定义参数的KEY，KEY一般也是Intent实际的Action名称
        public const string SETTINGS_KEY_TYPE = "settings_type";
        //定义参数的VALUE，这里对应各种操作的Intent Action
        public const string SETTINGS_TYPE_MAIN = "android.settings.LANGUAGE_SETTINGS";
        public const string SETTINGS_TYPE_WIFI = "android.nibiru.settings.WIFI_SETTINGS";
        public const string SETTINGS_TYPE_BLUETOOTH = "android.nibiru.settings.BLUE_SETTINGS";
        public const string SETTINGS_TYPE_SYSTEM = "android.nibiru.settings.SYSTEM_SETTINGS";
        public const string SETTINGS_TYPE_GENERAL = "android.nibiru.settings.NORMAL_SETTINGS";
    }

    public class Brower
    {
        //定义参数KEY：网址
        public const string EXPLORER_KEY_URL = "url";
        //定义参数KEY: 是否显示网址栏
        public const string EXPLORER_KEY_ACTIONBAR = "hideActionBar";
        //定义参数VALUE：是否显示网址栏：是/否
        public const string EXPLORER_KEY_ACTIONBAR_HIDE = "true";
        public const string EXPLORER_KEY_ACTIONBAR_SHOW = "false";
    }

}
