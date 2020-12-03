/*************************************************************
 * 
 *  Copyright(c) 2017 Lyrobotix.Co.Ltd.All rights reserved.
 *  NoloVR_Controller.cs
 *   
*************************************************************/

using UnityEngine;
using NoloClientCSharp;

public class NoloVR_Controller {

    public static bool isTurnAround = false;
    static Vector3 recPosition = Vector3.zero;

    //button mask
    public class ButtonMask
    {
        public static uint GetButtonMask(NoloButtonID button)
        {
            //6Dof游戏使用3Dof手柄
            if(NoloVR_Plugins.GetTrackModel() == 3)
            {
                switch ((NoloC1ButtonID)button)
                {
                    case NoloC1ButtonID.TouchPad:
                        return 1 << 1;
                    case NoloC1ButtonID.Trigger:
                        return 1 << 0;
                    case NoloC1ButtonID.System:
                        return 1 << 2;
                    case NoloC1ButtonID.SystemLongPress:
                        return 1 << 3;
                    case NoloC1ButtonID.Back:
                        return 1 << 4;
                    case NoloC1ButtonID.VolumeDown:
                        return 1 << 6;
                    case NoloC1ButtonID.VolumeUp:
                        return 1 << 7;
                    default:
                        return 0;
                }
            }
            else if (NoloVR_Plugins.GetTrackModel() == 6)
            {
                switch (button)
                {
                    case NoloButtonID.TouchPad:
                        return 1 << 0;
                    case NoloButtonID.Trigger:
                        return 1 << 1;
                    case NoloButtonID.Menu:
                        return 1 << 2;
                    case NoloButtonID.System:
                        return 1 << 3;
                    case NoloButtonID.Grip:
                        return 1 << 4;
                    default:
                        return 0;
                }
            }
            return 0;
        }
    }
    //touch mask
    public class TouchMask
    {
        public static uint GetTouchMask(NoloTouchID touch)
        {
            if (NoloVR_System.GetInstance().trackModel == NoloVR_Manager.TrackModel.Track_6dof)
            {
                switch (touch)
                {
                    case NoloTouchID.TouchPad:
                        return 1 << 0;
                    case NoloTouchID.Trigger:
                        return 1 << 1;
                    default:
                        return 0;
                }
            }
            else
            {
                switch (touch)
                {
                    case NoloTouchID.TouchPad:
                        return 1 << 0;
                    default:
                        return 0;
                }
                return 0;
            }
        }
    }
    //device message
    public class NoloDevice
    {
        public NoloDevice(int num)
        {
            index = num;
        }
        public int index { get; private set; }
        private Nolo_ControllerStates controllerStates, preControllerStates;
        private Nolo_Transform pose;
        private bool connectStatus;
        private int electricity;

        public Nolo_Transform GetPose()
        {
            Update();
            return pose;
        }
        public int GetNoloDeviceElectricity()
        {
            Update();
            return electricity;
        }
        public bool GetNoloDeviceConnectStatus()
        {
            Update();
            return connectStatus;
        }
        public bool GetNoloButtonPressed(uint buttonMask)
        {
            Update();
            return (controllerStates.buttons & buttonMask) != 0;
        }
        public bool GetNoloButtonDown(uint buttonMask)
        {
            Update();
            return (controllerStates.buttons & buttonMask) != 0 && (preControllerStates.buttons & buttonMask) == 0;
        }
        public bool GetNoloButtonUp(uint buttonMask)
        {
            Update();
            return (controllerStates.buttons & buttonMask) == 0 && (preControllerStates.buttons & buttonMask) != 0;
        }

        public bool GetNoloButtonPressed(NoloButtonID button)
        {
            return GetNoloButtonPressed(ButtonMask.GetButtonMask(button));
        }
        public bool GetNoloButtonDown(NoloButtonID button)
        {
            return GetNoloButtonDown(ButtonMask.GetButtonMask(button));
        }
        public bool GetNoloButtonUp(NoloButtonID button)
        {
            return GetNoloButtonUp(ButtonMask.GetButtonMask(button));
        }

        public bool GetNoloTouchPressed(uint touchMask)
        {
            Update();
            return (controllerStates.touches & touchMask) !=0;
        }
        public bool GetNoloTouchDown(uint touchMask)
        {
            Update();
            return (controllerStates.touches & touchMask) != 0 && (preControllerStates.touches & touchMask) == 0;
        }
        public bool GetNoloTouchUp(uint touchMask)
        {
            Update();
            return (controllerStates.touches & touchMask) == 0 && (preControllerStates.touches & touchMask) != 0;
        }

        public bool GetNoloTouchPressed(NoloTouchID touch)
        {
            return GetNoloTouchPressed(TouchMask.GetTouchMask(touch));
        }
        public bool GetNoloTouchDown(NoloTouchID touch)
        {
            return GetNoloTouchDown(TouchMask.GetTouchMask(touch));
        }
        public bool GetNoloTouchUp(NoloTouchID touch)
        {
            return GetNoloTouchUp(TouchMask.GetTouchMask(touch));
        }

        //touch axis return vector2 x(-1~1)y(-1,1)
        public Vector2 GetAxis(NoloTouchID axisIndex = NoloTouchID.TouchPad)
        {
            Update();
            if (axisIndex == NoloTouchID.TouchPad)
            {
                return new Vector2(controllerStates.touchpadAxis.x, controllerStates.touchpadAxis.y);
            }
            if (axisIndex == NoloTouchID.Trigger)
            {
                return new Vector2(controllerStates.rAxis1.x, controllerStates.rAxis1.y);
            }
            return Vector2.zero;
        }

        private int currentRealTrackDevices = 0; //当前拿到的设备是3或6dof
        private int lastRealTrackDevices;   //上一帧拿到的设备是3或6dof
        private int currentReceiveCount = 0;
        private int maxReceiveCount = 10;
        private bool isGetDevices = false;

        private int preFrame = -1;
        public void Update()
        {
            if (Time.frameCount != preFrame)
            {
                preFrame = Time.frameCount;
                preControllerStates = controllerStates;
                if (NoloVR_Playform.GetInstance().GetPlayformError() == NoloError.None && NoloVR_Playform.GetInstance().GetAuthentication())
                {
                    controllerStates = NoloVR_Plugins.GetControllerStates(index);
                    electricity = NoloVR_Plugins.GetElectricity(index);
                    connectStatus = NoloVR_Plugins.GetNoloConnectStatus(index);
                    float yaw = real_yaw * 57.3f;
                    pose = NoloVR_Plugins.GetPose(index);
                    if (index == 0)
                    {
                        //pose.pos += pose.rot * new Vector3(0, 0.08f, 0.062f);
                        pose.rot = Quaternion.Euler(new Vector3(0, -yaw, 0));
                        //pose.pos -= pose.rot * new Vector3(0, 0.08f, 0.062f);
                    }
                    if (isTurnAround)
                    {
                        if (NoloVR_Controller.recPosition == Vector3.zero)
                        {
                            NoloVR_Controller.recPosition = NoloVR_Plugins.GetPose(0).pos;
                        }
                        var rot = pose.rot.eulerAngles;
                        rot += new Vector3(0, 180 + yaw, 0);
                        pose.rot = Quaternion.Euler(rot);
                        Vector3 revec = Quaternion.Euler(new Vector3(0, 180 + yaw, 0)) * pose.pos + NoloVR_Controller.recPosition;
                        pose.pos.x = revec.x;
                        pose.pos.z = revec.z;
                        pose.vecVelocity.x = -pose.vecVelocity.x;
                        pose.vecVelocity.z = -pose.vecVelocity.z;
                        return;
                    }
                }
                /*if(NoloVR_System.GetInstance().realTrackDevices !=3 && NoloVR_System.GetInstance().realTrackDevices != 6)
                {
                    NoloVR_System.GetInstance().realTrackDevices = NoloVR_Plugins.GetTrackModel();
                    Debug.Log("realTrackDevices:" + NoloVR_System.GetInstance().realTrackDevices);
                }*/
                currentRealTrackDevices = NoloVR_System.GetInstance().realTrackDevices;
                if (NoloVR_System.GetInstance().realTrackDevices != 3 && NoloVR_System.GetInstance().realTrackDevices != 6)
                    return;

                if (currentRealTrackDevices != lastRealTrackDevices)
                {
                    lastRealTrackDevices = currentRealTrackDevices;
                    isGetDevices = false;
                }
                else
                {
                    if (!isGetDevices)
                    {
                        currentReceiveCount++;
                        if (currentReceiveCount >= maxReceiveCount)
                        {
                            NOLO_Events.Send(NOLO_Events.EventsType.GetTrackModel);
                            isGetDevices = true;
                        }
                    }
                }
            }
        }

        //HapticPulse  parameter must be in 0~100
        public void TriggerHapticPulse(int intensity)
        {
            if (NoloVR_Playform.GetInstance().GetPlayformError() == NoloError.None)
            {
                NoloVR_Plugins.TriggerHapticPulse(index, intensity);
            }
        }
    }
    
    //device manager
    public static NoloDevice[] devices;
    public static NoloDevice GetDevice(NoloDeviceType deviceIndex)
    {
        if (devices == null)
        {
            devices = new NoloDevice[NoloVR_Plugins.trackedDeviceNumber];
            for (int i = 0; i < devices.Length; i++)
            {
                devices[i] = new NoloDevice(i);
            }
        }
        return devices[(int)deviceIndex];
    }
    public static NoloDevice GetDevice(NoloVR_TrackedDevice trackedObject)
    {
        return GetDevice(trackedObject.deviceType);
    }

    //turn around events
    static void TurnAroundEvents(params object[] args)
    {
        isTurnAround = !isTurnAround;
    }

    static float real_yaw = 0;
    static float PI = 3.1415926f;
    //RecenterLeft events
    static void RecenterLeftEvents(params object[] args)
    {
        Vector3 handPosLeft = NoloVR_Plugins.GetPose(1).pos;
        Vector3 handPosRight = NoloVR_Plugins.GetPose(2).pos;
        Vector3 HeadPos = NoloVR_Plugins.GetPose(0).pos;
        Vector3 HandPos = NoloVR_Plugins.GetPose(1).pos;
        if (Vector3.Distance(handPosLeft, handPosRight)<0.2f)
        {
            HandPos = (handPosLeft + handPosRight) / 2;
        }
        if ((HandPos.x - HeadPos.x) > 0)
        {
            real_yaw = Mathf.Atan((HandPos.z - HeadPos.z) / (HandPos.x - HeadPos.x)) - PI / 2;//真实航向角
        }
        else if ((HandPos.x - HeadPos.x) < 0)
        {
            real_yaw = PI / 2 + Mathf.Atan((HandPos.z - HeadPos.z) / (HandPos.x - HeadPos.x));//真实航向角
        }
    }
    //RecenterRight events
    static void RecenterRightEvents(params object[] args)
    {
        Vector3 handPosLeft = NoloVR_Plugins.GetPose(1).pos;
        Vector3 handPosRight = NoloVR_Plugins.GetPose(2).pos;
        Vector3 HandPos = NoloVR_Plugins.GetPose(2).pos;
        Vector3 HeadPos = NoloVR_Plugins.GetPose(0).pos;
        if (Vector3.Distance(handPosLeft, handPosRight) < 0.2f)
        {
            HandPos = (handPosLeft + handPosRight) / 2;
        }
        if ((HandPos.x - HeadPos.x) > 0)
        {
            real_yaw = Mathf.Atan((HandPos.z - HeadPos.z) / (HandPos.x - HeadPos.x)) - PI / 2;//真实航向角
        }
        else if ((HandPos.x - HeadPos.x) < 0)
        {
            real_yaw = PI / 2 + Mathf.Atan((HandPos.z - HeadPos.z) / (HandPos.x - HeadPos.x));//真实航向角
        }
    }
    public static void Listen()
    {
        NOLO_Events.Listen(NOLO_Events.EventsType.TurnAround, TurnAroundEvents);
        //NOLO_Events.Listen(NOLO_Events.EventsType.RecenterLeft, RecenterLeftEvents);
        //NOLO_Events.Listen(NOLO_Events.EventsType.RecenterRight, RecenterRightEvents);
    }
    public static void Remove()
    {
        NOLO_Events.Remove(NOLO_Events.EventsType.TurnAround, TurnAroundEvents);
        //NOLO_Events.Remove(NOLO_Events.EventsType.RecenterLeft, RecenterLeftEvents);
        //NOLO_Events.Remove(NOLO_Events.EventsType.RecenterRight, RecenterRightEvents);
    }
}
