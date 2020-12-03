using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace NoloClientCSharp
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Nolo_Vector2
    {
        public float x;
        public float y;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct Nolo_Vector3
    {
        public float x;
        public float y;
        public float z;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct Nolo_Quaternion
    {
        public float x;
        public float y;
        public float z;
        public float w;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct Nolo_Pose
    {
        public Nolo_Vector3 pos;
        public Nolo_Quaternion rot;
        public Nolo_Vector3 vecVelocity;
        public Nolo_Vector3 vecAngularVelocity;
        public int status;
        public bool bDeviceIsConnected;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct Nolo_ControllerStates
    {
        public uint buttons;
        public uint touches;
        public Nolo_Vector2 touchpadAxis;
        public Nolo_Vector2 rAxis1;
        public Nolo_Vector2 rAxis2;
        public Nolo_Vector2 rAxis3;
        public Nolo_Vector2 rAxis4;
    }

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate void DisConnectedCallBack();
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate void ConnectedCallBack();

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate void ConnectedStatusCallBackFunc(int status);

    public class NoloClientSo
    {
        public const string dllName = "libNoloVR";

        [DllImport(dllName, EntryPoint = "getElectricityByDeviceType")]
        public static extern int GetElectricityByDeviceType(int type);

        [DllImport(dllName, EntryPoint = "getPoseByDeviceType", CallingConvention = CallingConvention.StdCall)]
        public static extern Nolo_Pose GetPoseByDeviceType(int type);

        [DllImport(dllName, EntryPoint = "getControllerStatesByDeviceType")]
        public static extern Nolo_ControllerStates GetControllerStatesByDeviceType(int type);

        [DllImport(dllName, EntryPoint = "triggerHapticPulse")]
        public static extern bool Nolovr_TriggerHapticPulse(int type, int intensity);

        //[DllImport(dllName, EntryPoint = "setHmdType")]
        //public static extern void SetHmdType(int hmdType);

        [DllImport(dllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetConnectedStatus")]
        public static extern int SetConnectedStatus([MarshalAs(UnmanagedType.FunctionPtr)]ConnectedStatusCallBackFunc nfun);

        [DllImport(dllName, EntryPoint = "getElectricityNumberByDeviceType")]
        public static extern int GetElectricityNumberByDeviceType(int type);

        [DllImport(dllName, EntryPoint = "setPredictionTime")]
        public static extern int SetPredictionTime(int predictionTime);

        [DllImport(dllName, EntryPoint = "getNoloHardwareVersionByDeviceType")]
        public static extern int GetNoloHardwareVersionByDeviceType(int type);

        [DllImport(dllName, EntryPoint = "getNoloSoftwareVersionByDeviceType")]
        public static extern float GetNoloSoftwareVersionByDeviceType(int type);

        [DllImport(dllName, EntryPoint = "getNoloSoVersion")]
        public static extern int GetNoloSoVersion();

        [DllImport(dllName, EntryPoint = "getNoloDoF")]
        public static extern int GetNoloTrackModel();
    }
}
