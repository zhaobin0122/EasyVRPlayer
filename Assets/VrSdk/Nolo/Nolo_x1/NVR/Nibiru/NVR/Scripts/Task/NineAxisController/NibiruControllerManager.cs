using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System;
namespace NibiruTask
{
    public class NibiruControllerManager
    {
        public static int KEYCODE_UP = 19;
        public static int KEYCODE_DOWN = 20;
        public static int KEYCODE_LEFT = 21;
        public static int KEYCODE_RIGHT = 22;
        public static int KEYCODE_UP_LEFT = 125;
        public static int KEYCODE_UP_RIGHT = 126;
        public static int KEYCODE_DOWN_LEFT = 127;
        public static int KEYCODE_DOWN_RIGHT = 128;
        public static int KEYCODE_BUTTON_Y = 96;
        public static int KEYCODE_BUTTON_B = 97;
        public static int KEYCODE_BUTTON_X = 98;
        public static int KEYCODE_BUTTON_A = 99;
        public static int KEYCODE_BUTTON_L1 = 102;
        public static int KEYCODE_BUTTON_R1 = 103;
        public static int KEYCODE_BUTTON_L2 = 104;
        public static int KEYCODE_BUTTON_R2 = 105;
        public static int KEYCODE_BUTTON_THUMBL = 106;
        public static int KEYCODE_BUTTON_THUMBR = 107;
        public static int KEYCODE_BUTTON_START = 108;
        public static int KEYCODE_BUTTON_SELECT = 109;
        public static int KEYCODE_BUTTON_HOME = 3;

        public static int KEYCODE_BUTTON_VOL_DOWN = 25;
        public static int KEYCODE_BUTTON_VOL_UP = 24;
        public static int KEYCODE_BUTTON_ENTER = 23;
        public static int KEYCODE_BUTTON_BACK = 4;
        public static int KEYCODE_BUTTON_MEDIA_NEXT = 87;
        public static int KEYCODE_BUTTON_MEDIA_PREVIOUS = 88;
        public static int KEYCODE_BUTTON_MEDIA_PLAY_PAUSE = 85;
        public static int KEYCODE_BUTTON_NIBIRU = 170;
        public static int KEYCODE_BUTTON_CURSOR = 0;
        public static int KEYCODE_SHOW_CURSOR = 171;
        public static int KEYCODE_TOUCH_TOGGLE = 172;
        public static int KEYCODE_TOUCH_DOWNUP = 173;
        public static int KEYCODE_BUTTON_1 = 201;
        public static int KEYCODE_BUTTON_2 = 202;
        public static int KEYCODE_BUTTON_3 = 203;
        public static int KEYCODE_BUTTON_4 = 204;
        public static int KEYCODE_BUTTON_5 = 205;
        public static int KEYCODE_BUTTON_6 = 206;
        public static int KEYCODE_BUTTON_7 = 207;
        public static int KEYCODE_BUTTON_8 = 208;
        public static int KEYCODE_BUTTON_9 = 209;
        public static int KEYCODE_BUTTON_10 = 210;


        //gForceCode
        public static int GESTURE_RELAX = 0; //放松
        public static int GESTURE_GIST = 1; //握拳
        public static int GESTURE_SPREAD_FINGERS = 2; //伸掌
        public static int GESTURE_WAVE_TOWARD_PALM = 3; //屈腕
        public static int GESTURE_WAVE_BACKWARD_PALM = 4; //伸腕
        public static int GESTURE_TUCK_FINGERS = 5; //空捏
        public static int GESTURE_SHOOT = 6; //开枪
        public static int GESTURE_MAX = GESTURE_SHOOT; //
        public static int GESTURE_UNKNOWN = 255; //未知手势

        //XiaoGCode
        public static int GESTURE_SLIP_UP = 19;
        public static int GESTURE_SLIP_DOWN = 20;
        public static int GESTURE_SLIP_LEFT = 21;
        public static int GESTURE_SLIP_RIGHT = 22;

        /*	private int ACTION_DOWN = 0;
            private int ACTION_UP = 1;*/
        public static int[] keystate = new int[256];



        //键值转换数组  HID  ->   Nibiru
        //	public int[] keyChange = new int[1000];

        //多手柄键值转换数组  HID  ->  Nibiru

        //	public int[,] moreKayChange = new int[100,1000];

        //	public string[] keyOFlock = new string[255];

        //谷歌标准按键
        /*	private int GKEYCODE_BUTTON_A = 96;
            private int GKEYCODE_BUTTON_B = 97;
            private int GKEYCODE_BUTTON_X = 99;
            private int GKEYCODE_BUTTON_Y = 100;
            private int GKEYCODE_BUTTON_L1 = 102;
            private int GKEYCODE_BUTTON_R1 = 103;
            private int GKEYCODE_BUTTON_L2 = 104;
            private int GKEYCODE_BUTTON_R2 = 105;
            private int GKEYCODE_BUTTON_SELECT = 109;
            private int GKEYCODE_BUTTON_START = 108;
         */




    }
}