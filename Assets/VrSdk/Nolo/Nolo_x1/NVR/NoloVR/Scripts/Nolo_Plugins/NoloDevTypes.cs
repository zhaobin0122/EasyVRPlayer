using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace NoloClientCSharp
{
    public enum ENoloDeviceType
    {
        eHmd = 0,
        eLeftController,
        eRightController,
        eBaseStation
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct NVector2
    {
        public float x;
        public float y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NVector3
    {
        public float x;
        public float y;
        public float z;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NQuaternion
    {
        public float x;
        public float y;
        public float z;
        public float w;
    }

    //Types
    public enum EControlerButtonType
    {
        ePadBtn = 0x01,
        eTriggerBtn = 0x02,
        eMenuBtn = 0x04,
        eSystemBtn = 0x08,
        eGripBtn = 0x10,
        ePadTouch = 0x20
    };
    
    [StructLayout(LayoutKind.Sequential)]
    public struct NoloData
    {
        public Controller leftData;
        public Controller rightData;
        public HMD hmdData;
        public BaseStation bsData;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public byte[] expandData;
        public NoloSensorData NoloSensorData;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct Controller
    {
        public int VersionID;
        public NVector3 Position;
        public NQuaternion Rotation;
        public uint Buttons;
        public uint Touched;
        public NVector2 TouchAxis;
        public int Battery;
        public int State;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct HMD
    {
        public int HMDVersionID;
        public NVector3 HMDPosition;
        public NVector3 HMDInitPostion;
        public uint HMDTwoPointDriftAngle;
        public NQuaternion HMDRotation;
        public int HMDState;
    };


    [StructLayout(LayoutKind.Sequential)]
    public struct BaseStation
    {
        public int BaseStationVersionID;
        public int BaseStationPower;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct NoloSensorData
    {
        public NVector3 vecLVelocity;
        public NVector3 vecLAngularVelocity;
        public NVector3 vecRVelocity;
        public NVector3 vecRAngularVelocity;
        public NVector3 vecHVelocity;
        public NVector3 vecHAngularVelocity;
    };

}
