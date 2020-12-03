/*************************************************************
 * 
 *  Copyright(c) 2017 Lyrobotix.Co.Ltd.All rights reserved.
 *  NoloVR_Manager.cs
 *   
*************************************************************/

using NoloClientCSharp;
using System.Collections;
using UnityEngine;
using UnityEngine.VR;

public class NoloVR_Manager : MonoBehaviour
{
    public string appKey;
    public GameObject VRCamera;

    public TrackModel gameTrackModel = TrackModel.Track_6dof;
    //TurnAroundButtonType turnAroundButtonType = TurnAroundButtonType.Menu;
    public bool useDefaultHeight = true;
    public float defaultHeight = 0f;

    public GameObject leftController;
    public GameObject rightController;
    [HideInInspector]
    public NoloVR_TrackedDevice[] objects;

    public enum TrackModel
    {
        Track_3dof = 3,//3dof 游戏模式
        Track_6dof = 6,//6dof 游戏模式
    }

    void Awake()
    {
        NoloVR_System.GetInstance().objects = GameObject.FindObjectsOfType<NoloVR_TrackedDevice>();
        NoloVR_System.GetInstance().VRCamera = this.VRCamera;
        NoloVR_System.GetInstance().trackModel = gameTrackModel;
        if (useDefaultHeight)
        {
            NoloVR_System.GetInstance().defaultHeight = defaultHeight;
        }
        else
        {
            NoloVR_System.GetInstance().defaultHeight = 0;
        }
    }
    void Start()
    {
        NoloVR_Playform.GetInstance().Authentication(appKey);
    }
    public void OnClickButton()
    {
        NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).TriggerHapticPulse(100);
        Debug.Log("LeftController Trigger Pressed");
    }
    void Update()
    {
        //非X1一体机一键转身和双击标定需要实现
        //if (turnAroundButtonType!= TurnAroundButtonType.Null)
        //{
        //TurnAroundEventsMonitor();
        //}
        //Recenter();

        leftController.SetActive(NoloVR_Plugins.GetNoloConnectStatus(1));
        rightController.SetActive(NoloVR_Plugins.GetNoloConnectStatus(2));
    }
    /*
    private int leftcontrollerTurn_PreFrame = -1;
    private int rightcontrollerTurn_PreFrame = -1;
    private int turnAroundSpacingFrame = 20;
    void TurnAroundEventsMonitor()
    {
        //leftcontroller double click system button
        if (NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloButtonUp((uint)1 << (int)turnAroundButtonType))
        {
            if (Time.frameCount - leftcontrollerTurn_PreFrame <= turnAroundSpacingFrame)
            {
                NOLO_Events.Send(NOLO_Events.EventsType.TurnAround);
                leftcontrollerTurn_PreFrame = -1;
            }
            else
            {
                leftcontrollerTurn_PreFrame = Time.frameCount;
            }
        }
        //rightcontroller double click system button
        if (NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetNoloButtonUp((uint)1 << (int)turnAroundButtonType))
        {
            if (Time.frameCount - rightcontrollerTurn_PreFrame <= turnAroundSpacingFrame)
            {
                NOLO_Events.Send(NOLO_Events.EventsType.TurnAround);
                rightcontrollerTurn_PreFrame = -1;
            }
            else
            {
                rightcontrollerTurn_PreFrame = Time.frameCount;
            }
        }
    }
    */
    /*
    void Recenter()
    {
#if NOLO_6DOF
        //leftcontroller double click system button
        if (NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloButtonDown(NoloButtonID.DoubleClickSystem))
        {
            NOLO_Events.Send(NOLO_Events.EventsType.RecenterLeft);
        }
        //rightcontroller double click system button
        if (NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetNoloButtonDown(NoloButtonID.DoubleClickSystem))
        {
            NOLO_Events.Send(NOLO_Events.EventsType.RecenterRight);
        }
#elif NOLO_3DOF
        if (NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloButtonUp(NoloButtonID.SystemLongPress))
        {
            UnityEngine.VR.InputTracking.Recenter();
        }
#endif
    }
    */
    void OnApplicationQuit()
    {
        //close connect from device
        Debug.Log("Nolo debug:Application quit");
        NoloVR_Playform.GetInstance().DisconnectDevice();
    }
}
