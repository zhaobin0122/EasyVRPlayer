using System;
using System.Text;
using System.Runtime.InteropServices;
using UnityEngine;
using NoloClientCSharp;
//Version:V_0_1_RC


namespace NOLO
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Nolo_Pose
    {
        public NVector3 pos;
        public NQuaternion rot;
        public NVector3 vecVelocity;
        public NVector3 vecAngularVelocity;
    }

    public class NOLOClient_V2_API
    {
        public static Nolo_Transform GetPoseByDeviceType(int deviceIndex) {
            Update();
            Nolo_Transform result = new Nolo_Transform();
            switch (deviceIndex)
            {
                case 0:
                    result = new Nolo_Transform(noloData.hmdData.HMDPosition,
                        noloData.hmdData.HMDRotation,
                        noloData.NoloSensorData.vecHVelocity,
                        noloData.NoloSensorData.vecHAngularVelocity) ;
                    break;
                case 1:
                    result = new Nolo_Transform(noloData.leftData.Position,
                        noloData.leftData.Rotation,
                        noloData.NoloSensorData.vecLVelocity,
                        noloData.NoloSensorData.vecLAngularVelocity);
                    break;
                case 2:
                    result = new Nolo_Transform(noloData.rightData.Position,
                        noloData.rightData.Rotation,
                        noloData.NoloSensorData.vecRVelocity,
                        noloData.NoloSensorData.vecRAngularVelocity);
                    break;
                case 3:
                    break;
                default:
                    break;
            }
            return result;
        }

        public static Nolo_ControllerStates GetControllerStatesByDeviceType(int deviceIndex)
        {
            Update();
            Nolo_ControllerStates result= new Nolo_ControllerStates();
            switch (deviceIndex)
            {
                case 1:
                    result.buttons = noloData.leftData.Buttons;
                    result.touches = noloData.leftData.Touched;
                    result.touchpadAxis.x = noloData.leftData.TouchAxis.x;
                    result.touchpadAxis.y = noloData.leftData.TouchAxis.y;
                    break;
                case 2:
                    result.buttons = noloData.rightData.Buttons;
                    result.touches = noloData.rightData.Touched;
                    result.touchpadAxis.x = noloData.rightData.TouchAxis.x;
                    result.touchpadAxis.y = noloData.rightData.TouchAxis.y;
                    break;
                default:
                    break;
            }
            return result;
        }

        public static int GetElectricityByDeviceType(int deviceIndex)
        {
            int battery = GetElectricityNumberByDeviceType(deviceIndex);
            if (battery > 0 && battery < 8)
            {
                return 1;
            }
            else if (battery >= 8 && battery < 40)
            {
                return 2;
            }
            else if (battery >= 40 && battery < 60)
            {
                return 3;
            }
            else if (battery >= 60 && battery < 80)
            {
                return 4;
            }
            else if (battery >= 80 && battery < 254)
            {
                return 5;
            }
            else
            {
                return 0;
            }
        }

        public static int GetElectricityNumberByDeviceType(int deviceIndex)
        {
            Update();
            int battery = 0;
            switch (deviceIndex)
            {
                case 0:
                    battery = 100;
                    break;
                case 1:
                    battery = noloData.leftData.Battery;
                    break;
                case 2:
                    battery = noloData.rightData.Battery;
                    break;
                case 3:
                    battery = noloData.bsData.BaseStationPower;
                    break;
                default:
                    break;
            }
            return battery;
        }

        public static bool GetNoloConnectStatus(int deviceIndex)
        {
            int battery = GetElectricityByDeviceType(deviceIndex);
            if(battery > 0)
            {
                return true;
            }
            return false;
        }

        public static void TriggerHapticPulse(ENoloDeviceType deviceType, int intensity)
        {
            NoloClientLib.TriggerHapticPulse(deviceType, intensity);
        }

        static int preFrame = -1;
        static NoloData noloData = new NoloData();
        public static void Update()
        {
            if (Time.frameCount != preFrame)
            {
                preFrame = Time.frameCount;
                noloData = NoloClientLib.GetNoloData();
            }
        }
    }

    public class NOLOClientForAndroid_V2_API
    {
        public static Nolo_Transform GetPoseByDeviceType(int deviceIndex)
        {

            return new Nolo_Transform(Update(deviceIndex));
        }

        public static bool GetNoloConnectStatus(int deviceIndex)
        {
            return Update(deviceIndex).bDeviceIsConnected;
        }

        public static void TriggerHapticPulse(ENoloDeviceType deviceType, int intensity)
        {
            NoloClientLib.TriggerHapticPulse(deviceType, intensity);
        }

        static int preFrame = -1;
        //缓存每帧的数据
        static NoloClientCSharp.Nolo_Pose[] noloPoses = new NoloClientCSharp.Nolo_Pose[4];

        static NoloClientCSharp.Nolo_Pose Update(int id)
        {
            //if (Time.frameCount != preFrame)
            {
                preFrame = Time.frameCount;
                noloPoses[id] = NoloClientSo.GetPoseByDeviceType(id);
            }

            return noloPoses[id];
        }
    }

}
