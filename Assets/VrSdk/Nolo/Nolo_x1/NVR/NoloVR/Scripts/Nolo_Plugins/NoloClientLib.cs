
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace NoloClientCSharp
{
    //delegate
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void pfnKeyEvent(EControlerButtonType type);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void pfnVoidCallBack();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void pfnDataCallBack(NoloData noloData);

    public enum ECallBackTypes
    {
        eOnZMQConnected = 0,     //pfnVoidCallBack
        eOnZMQDisConnected,      //pfnVoidCallBack
        eOnButtonDoubleClicked,  //pfnKeyEvent

        eOnNewData,              //pfnDataCallBack
        eCallBackCount
    };

    public class NoloClientLib
    {
        [DllImport("NoloClientLib",CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool StartNoloServer(string strName);

        [DllImport("NoloClientLib",CallingConvention = CallingConvention.Cdecl)]
        public static extern void RegisterCallBack(ECallBackTypes callBackType, IntPtr pCallBackFun);

        [DllImport("NoloClientLib")]
        public static extern void SetHmdCenter(NVector3 hmdCenter);

        [DllImport("NoloClientLib")]
        public static extern bool OpenNoloZeroMQ();

        [DllImport("NoloClientLib")]
        public static extern void CloseNoloZeroMQ();

        [DllImport("NoloClientLib")]
        public static extern void TriggerHapticPulse(ENoloDeviceType deviceType, int intensity);

        [DllImport("NoloClientLib")]
        public static extern NoloData GetNoloData();

        [DllImport("NoloClientLib")]
        public static extern Controller GetLeftControllerData();

        [DllImport("NoloClientLib")]
        public static extern Controller GetRightControllerData();

        [DllImport("NoloClientLib")]
        public static extern HMD GetHMDData();

    }
}
