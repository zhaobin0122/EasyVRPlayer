using Pvr_UnitySDKAPI;
using UnityEngine;
using System;

public class Pvr_UnitySDKSensor
{
    public Pvr_UnitySDKSensor()
    {
        Init();
    }

    /************************************    Properties  *************************************/
    #region Properties

    public Pvr_UnitySDKPose SensorValvue;

    public bool HMDUsing = true;

    bool SensorStart = false;
    bool SensorInit = false;
    bool Sensor6dofInit = false;

    Quaternion UnityQuaternion = Quaternion.identity;
    Vector3 EulerAngles = Vector3.zero;
    Vector3 UnityPosition = Vector3.zero;

    Pvr_UnitySDKAPI.Sensorindex sensorIndex = Pvr_UnitySDKAPI.Sensorindex.Default;

    private bool dofClock = false;
    public static Action EyeFovChanged;
    #endregion

    /************************************   Public Interfaces **********************************/
    #region Public Interfaces

    public delegate void Enter3DofModel();
    public static event Enter3DofModel Enter3DofModelEvent;

    public delegate void Exit3DofModel();
    public static event Exit3DofModel Exit3DofModelEvent;

    public void Init()
    {
        Sensor6dofInit = InitUnitySDK6DofSensor();
        SensorInit = InitUnitySDKSensor();
        SensorStart = StartUnitySDKSensor();
    }

    public void SensorUpdate()
    {
        if (GetUnitySDKSensorState())
        {
            Pvr_UnitySDKManager.SDK.HeadPose = new Pvr_UnitySDKPose(UnityPosition, UnityQuaternion);
        }
    }
    public bool InitUnitySDKSensor()
    {
        bool enable = false;
        try
        {
            if (Pvr_UnitySDKAPI.Sensor.UPvr_Init((int)sensorIndex) == 0)
                enable = true;
        }
        catch (System.Exception e)
        {
            PLOG.E("InitUnitySDKSensor ERROR! " + e.Message);
            throw;
        }
        return enable;
    }

    public bool InitUnitySDK6DofSensor()
    {
        bool enable = false;
        try
        {
            int ability6dof = 0;
            int enumindex = (int)Pvr_UnitySDKAPI.GlobalIntConfigs.ABILITY6DOF;
            Pvr_UnitySDKAPI.Render.UPvr_GetIntConfig(enumindex, ref ability6dof);
            if (ability6dof == 1)
            {
                if (Pvr_UnitySDKAPI.Sensor.UPvr_Enable6DofModule(!Pvr_UnitySDKManager.SDK.HmdOnlyrot) == 0)
                {
                    if (!Pvr_UnitySDKManager.SDK.HmdOnlyrot)
                    {
                        enable = true;
                        Pvr_UnitySDKManager.SDK.PVRNeck = false;
                    }
                }
            }
            else
            {
                Debug.LogWarning("This platform does NOT support 6 Dof ! ");
            }
        }
        catch (System.Exception e)
        {
            PLOG.E("InitUnity6DofSDKSensor ERROR! " + e.Message);
            throw;
        }
        return enable;
    }

    public bool StartUnitySDKSensor()
    {
        bool enable = false;
        try
        {
            if (Pvr_UnitySDKAPI.Sensor.UPvr_StartSensor((int)sensorIndex) == 0)
                enable = true;
        }
        catch (System.Exception e)
        {
            PLOG.E("StartUnitySDKSensor ERROR! " + e.Message);
            throw;
        }
        return enable;
    }

    public bool StopUnitySDKSensor()
    {
        bool enable = false;
        try
        {
            if (Pvr_UnitySDKAPI.Sensor.UPvr_StopSensor((int)sensorIndex) == 0)
                enable = true;

        }
        catch (System.Exception e)
        {
            PLOG.E("StopUnitySDKSensor ERROR! " + e.Message);
            throw;
        }
        return enable;
    }

    public bool ResetUnitySDKSensor()
    {
        bool enable = false;
        try
        {
            if (Pvr_UnitySDKAPI.Sensor.UPvr_ResetSensor((int)sensorIndex) == 0)
            {
                enable = true;
                PLOG.E("ResetUnitySDKSensor OK! ");
            }
        }
        catch (System.Exception e)
        {
            PLOG.E("ResetUnitySDKSensor ERROR! " + e.Message);
            throw;
        }
        return enable;
    }

    public bool OptionalResetUnitySDKSensor(int resetRot, int resetPos)
    {
        bool enable = false;
        try
        {
            if (!Pvr_UnitySDKManager.SDK.SixDofPosReset)
            {
                resetPos = 0;
            }
            if (Pvr_UnitySDKAPI.Sensor.UPvr_OptionalResetSensor((int)sensorIndex, resetRot, resetPos) == 0)
            {
                enable = true;
                PLOG.I("PvrLog OptionalResetUnitySDKSensor OK!" + resetRot + resetPos);
            }
        }
        catch (System.Exception e)
        {
            PLOG.E("OptionalResetUnitySDKSensor ERROR! " + e.Message);
            throw;
        }
        return enable;
    }

    public bool GetUnitySDKSensorState()
    {
        bool enable = false;
        if (SensorInit && SensorStart)
        {
            float vfov = 102, hfov = 102;
            float w = 0, x = 0, y = 0, z = 0, px = 0, py = 0, pz = 0;
            try
            {
                int returns = Pvr_UnitySDKAPI.Sensor.UPvr_GetMainSensorState(ref x, ref y, ref z, ref w, ref px, ref py, ref pz, ref vfov, ref hfov, ref Pvr_UnitySDKManager.SDK.RenderviewNumber);
                Pvr_UnitySDKManager.SDK.posStatus = Sensor.UPvr_Get6DofSensorQualityStatus();
                PLOG.D("posStatus=" + Pvr_UnitySDKManager.SDK.posStatus);
                if (returns == 0)
                {
                    if (!Convert.ToBoolean(Pvr_UnitySDKManager.SDK.posStatus & 0x2))
                    {
                        if (!dofClock)
                        {
                            if (Enter3DofModelEvent != null)
                                Enter3DofModelEvent();
                            dofClock = true;
                        }
                    }
                    else
                    {
                        if (dofClock)
                        {
                            if (Exit3DofModelEvent != null)
                                Exit3DofModelEvent();
                            dofClock = false;
                        }
                    }

                    Pvr_UnitySDKManager.SDK.headData = new float[7] { x, y, z, w, px, py, pz };
                    UnityQuaternion = new Quaternion(x, y, -z, -w);
                    if (Pvr_UnitySDKManager.SDK.EyeVFoV != vfov)
                    {
                        Pvr_UnitySDKManager.SDK.EyeVFoV = vfov;
                        if (EyeFovChanged != null)
                        {
                            EyeFovChanged();
                        }
                    }
                    Pvr_UnitySDKManager.SDK.EyeHFoV = hfov;
                    Pvr_UnitySDKManager.SDK.EyesAspect = hfov / vfov;
                    enable = true;

                    if (Pvr_UnitySDKManager.SDK.HmdOnlyrot)
                    {
                        if (Pvr_UnitySDKManager.SDK.PVRNeck)
                        {
                            if (Pvr_UnitySDKManager.SDK.TrackingOrigin == TrackingOrigin.FloorLevel)
                            {
                                UnityPosition = new Vector3(0, py, 0) + UnityQuaternion * Pvr_UnitySDKManager.SDK.neckOffset -
                                                Pvr_UnitySDKManager.SDK.neckOffset.y * Vector3.up;
                            }
                            else
                            {
                                UnityPosition = UnityQuaternion * Pvr_UnitySDKManager.SDK.neckOffset -
                                                Pvr_UnitySDKManager.SDK.neckOffset.y * Vector3.up;
                            }
                        }
                    }
                    else
                    {
                        UnityPosition = new Vector3(px * Pvr_UnitySDKManager.SDK.MovingRatios, py * Pvr_UnitySDKManager.SDK.MovingRatios, -pz * Pvr_UnitySDKManager.SDK.MovingRatios);
                    }
                    PLOG.D("PvrLog 6DoFHead" + "Rotation:" + x + "," + y + "," + -z + "," + -w + "," + "Position:" + px + "," + py + "," + -pz + "," + "eulerAngles:" + UnityQuaternion.eulerAngles);
                }
                if (returns == -1)
                    PLOG.I("PvrLog Sensor update --- GetUnitySDKSensorState  -1 ");
            }
            catch (System.Exception e)
            {
                PLOG.E("GetUnitySDKSensorState ERROR! " + e.Message);
                throw;
            }
        }
        return enable;
    }

    public bool GetUnitySDKPSensorState()
    {
        bool enable = false;
        try
        {
            if (Pvr_UnitySDKAPI.Sensor.UPvr_ResetSensor((int)sensorIndex) == 0)
                enable = true;

        }
        catch (System.Exception e)
        {
            PLOG.E("GetUnitySDKPSensorState ERROR! " + e.Message);
            throw;
        }
        return enable;
    }

    #endregion
}
