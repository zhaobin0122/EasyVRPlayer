using UnityEngine;

namespace NibiruTask
{
    public class CDevice
    {
        //  当前使用哪知手
        public enum HAND_MODE
        {
            LEFT = 1, RIGHT = 0
        }

        public enum NOLO_TYPE
        {
            NONE = 0, LEFT = 1, RIGHT = 2, HEAD = 3
        }


        public static int DEVICE_CUBAND = 11;
        public static int DEVICE_GFORCE = 12;
        public static int DEVICE_XIAOG = 13;
        public static int DEVICE_XIMMERSE = 14;
        public static int DEVICE_FIVED_TECH = 15;
        public static int DEVICE_NINE_WIVI = 16;
        public static int DEVICE_NINE_YUNSHU = 17;
        public static int DEVICE_BIKE = 18;
        public static int DEVICE_DAYDREAM = 19;
        public static int DEVICE_XWG = 20;
        public static int DEVICE_EMDOOR_X1 = 21;
        public static int DEVICE_CLEER = 22;
        public static int DEVICE_REALMAX = 23;
        public static int DEVICE_QIYI = 24;
        public static int DEVICE_PICO = 25;
        public static int DEVICE_NINE_GUN = 101;
        public static int DEVICE_NINE_GAMEPAD = 102;
        public static int DEVICE_NINE_GAMEPAD_NEW = 103;
        public static int DEVICE_EX_SENSOR = 104;
        public static int DEVICE_NINE_YOUJIAN = 105;
        public static int DEVICE_NINE_GAMEPAD3 = 106;
        public static int DEVICE_NINE_EMDOORX1 = 107;
        public static int DEVICE_NOLO_SIXDOF = 108;
        public static int MODE_CONTROLLER = 0;
        public static int MODE_GESTURE = 1;
        public static int MODE_BRAND = 2;
        public static int MODE_BIKE = 3;
        public static int MODE_GUN = 4;
        private AndroidJavaObject bluetoothdevice;
        private AndroidJavaObject usbdevice;
        private bool isQuat;
        private int type;
        private string name;
        private int mode;

        public int getMode()
        {
            return this.mode;
        }

        public void setMode(int mode)
        {
            this.mode = mode;
        }

        public string getName()
        {
            if (name == null || name.Equals(""))
            {
                if (usbdevice != null)
                {
                    return usbdevice.Call<string>("getDeviceName");
                }
                if (bluetoothdevice != null)
                {
                    return bluetoothdevice.Call<string>("getName");
                }
            }
            return this.name;
        }

        public void setName(string name)
        {
            this.name = name;
        }

        public int getType()
        {
            return this.type;
        }

        public void setType(int type)
        {
            this.type = type;
        }

        public bool getisQuat()
        {
            return this.isQuat;
        }

        public void setQuat(bool isQuat)
        {
            this.isQuat = isQuat;
        }

        public void setBdevice(AndroidJavaObject bdevice)
        {
            this.bluetoothdevice = bdevice;
        }

        public void setUdevice(AndroidJavaObject udevice)
        {
            this.usbdevice = udevice;
        }

        public CDevice(AndroidJavaObject device, bool isQuat, int type, int mode)
        {
            this.bluetoothdevice = device;
            this.isQuat = isQuat;
            this.type = type;
            this.mode = mode;
        }

        public CDevice(AndroidJavaObject device, bool isQuat, int type)
        {
            this.usbdevice = device;
            this.isQuat = isQuat;
            this.type = type;
        }

        public CDevice(string name, bool isQuat, int type, int mode)
        {
            this.name = name;
            this.isQuat = isQuat;
            this.type = type;
            this.mode = mode;
        }

        public AndroidJavaObject getBdevice()
        {
            return this.bluetoothdevice;
        }

        public AndroidJavaObject getUdevice()
        {
            return this.usbdevice;
        }

        public string toString()
        {
            return "CDevice [name=" + getName() + ",isQuat=" + isQuat + ",type=" + type + ",mode" + mode + "]";
        }
    }
}

