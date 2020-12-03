using UnityEngine;
using System.Collections;
namespace NibiruTask
{
    public class CGestureEvent
    {
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
        public static int GESTURE_STATUS_UPDATE = 254; //四元素校准


        //XiaoGCode
        public static int GESTURE_SLIP_UP = 19;
        public static int GESTURE_SLIP_DOWN = 20;
        public static int GESTURE_SLIP_LEFT = 21;
        public static int GESTURE_SLIP_RIGHT = 22;
    }

}
