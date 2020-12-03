namespace XR
{
    public class CKeyEvent
    {
        public static int ACTION_DOWN = 0;
        public static int ACTION_UP = 1;
        public static int ACTION_MOVE = 2;

        public static int KEYCODE_DPAD_UP = 19;
        public static int KEYCODE_DPAD_DOWN = 20;
        public static int KEYCODE_DPAD_LEFT = 21;
        public static int KEYCODE_DPAD_RIGHT = 22;
        public static int KEYCODE_DPAD_CENTER = 23;
        public static int KEYCODE_VOLUME_UP = 24;
        public static int KEYCODE_VOLUME_DOWN = 25;
        public static int KEYCODE_BUTTON_Y = 100;
        public static int KEYCODE_BUTTON_B = 97;
        public static int KEYCODE_BUTTON_A = 96;
        public static int KEYCODE_BUTTON_X = 99;
        public static int KEYCODE_BUTTON_L1 = 102;
        public static int KEYCODE_BUTTON_R1 = 103;
        public static int KEYCODE_BUTTON_L2 = 104;
        public static int KEYCODE_BUTTON_R2 = 105;
        public static int KEYCODE_BUTTON_THUMBL = 106;
        public static int KEYCODE_BUTTON_THUMBR = 107;
        public static int KEYCODE_BUTTON_START = 108;
        public static int KEYCODE_BUTTON_SELECT = 109;
        public static int KEYCODE_BUTTON_NIBIRU = 110;
        public static int KEYCODE_BUTTON_HOME = 3;
        public static int KEYCODE_BUTTON_APP = 255;
        public static int KEYCODE_BACK = 255;

        public static int KEYCODE_NF_1 = 144;
        public static int KEYCODE_NF_2 = 145;

        public static int KEYCODE_CONTROLLER_TOUCHPAD_TOUCH = 254;
        public static int KEYCODE_CONTROLLER_TRIGGER = 103;
        public static int KEYCODE_CONTROLLER_MENU = 255;
        public static int KEYCODE_CONTROLLER_TOUCHPAD = 23;
        public static int KEYCODE_CONTROLLER_VOLUMN_DOWN = 25;
        public static int KEYCODE_CONTROLLER_VOLUMN_UP = 24;
        public static int KEYCODE_3DOF_CONTROLLER_TRIGGER = 105;

        public static int NONE = 0;
        public static int NOLO_LEFT = 1;
        public static int NOLO_RIGHT = 2;

        public static bool IsTriggerKeyCode(int keycode)
        {
            return keycode == KEYCODE_3DOF_CONTROLLER_TRIGGER || keycode == KEYCODE_CONTROLLER_TRIGGER;
        }

        public static int[] KeyCodeIds = new int[]
        {
        KEYCODE_DPAD_UP,
        KEYCODE_DPAD_DOWN,
        KEYCODE_DPAD_LEFT,
        KEYCODE_DPAD_RIGHT,
        KEYCODE_DPAD_CENTER,
        KEYCODE_VOLUME_UP,
        KEYCODE_VOLUME_DOWN,
        KEYCODE_BUTTON_Y,
        KEYCODE_BUTTON_B,
        KEYCODE_BUTTON_A,
        KEYCODE_BUTTON_X,
        KEYCODE_BUTTON_L1,
        KEYCODE_BUTTON_R1,
        KEYCODE_BUTTON_L2,
        KEYCODE_BUTTON_R2,
        KEYCODE_BUTTON_THUMBL,
        KEYCODE_BUTTON_THUMBR,
        KEYCODE_BUTTON_START,
        KEYCODE_BUTTON_SELECT,
        KEYCODE_BUTTON_NIBIRU,
        KEYCODE_BUTTON_HOME,
        KEYCODE_BUTTON_APP,
        KEYCODE_CONTROLLER_TOUCHPAD_TOUCH,
        KEYCODE_NF_1,
        KEYCODE_NF_2
            };

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
    }

}