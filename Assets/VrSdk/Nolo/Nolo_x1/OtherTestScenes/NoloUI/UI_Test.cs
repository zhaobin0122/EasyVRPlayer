using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using NoloClientCSharp;


public class UI_Test : MonoBehaviour
{
    private Text UIText;
    void Start()
    {
        UIText = GetComponent<Text>();
    }

    void Update()
    {
        //Debug.Log(NoloVR_Plugins.GetElectricity(3));
        try
        {

            if (NoloVR_System.GetInstance().realTrackDevices == 3) {
                UIText.text = "DATA:" + "\n"
               + "LEFT POS      :" + NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetPose().pos + "       RealPos:" + NoloClientSo.GetPoseByDeviceType(1).pos.x + "\n"
                + "LEFT ROT      :" + NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetPose().rot + "      RealRot:" + NoloClientSo.GetPoseByDeviceType(1).rot.x + "\n"
                + "LEFT Touchpad  :" + NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloButtonPressed((NoloButtonID)NoloC1ButtonID.TouchPad) + "\n"
                + "LEFT Trigger  :" + NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloButtonPressed((NoloButtonID)NoloC1ButtonID.Trigger) + "\n"
                + "LEFT system  :" + NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloButtonPressed((NoloButtonID)NoloC1ButtonID.System) + "\n"
                + "LEFT systemLongPress  :" + NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloButtonPressed((NoloButtonID)NoloC1ButtonID.SystemLongPress) + "\n"
                + "LEFT back  :" + NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloButtonPressed((NoloButtonID)NoloC1ButtonID.Back) + "\n"
                + "LEFT volimedown  :" + NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloButtonPressed((NoloButtonID)NoloC1ButtonID.VolumeDown) + "\n"
                + "LEFT volimeup  :" + NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloButtonPressed((NoloButtonID)NoloC1ButtonID.VolumeUp) + "\n"
                + "LEFT Axis  :" + NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetAxis(NoloTouchID.TouchPad) + "\n";
            }
            else {
                UIText.text = "DATA:" + "\n"
            + "HMD POS       :" + NoloVR_Controller.GetDevice(NoloDeviceType.Hmd).GetPose().pos + "     HMD ROT       :" + NoloVR_Plugins.GetPose(0).rot + "\n"
            + "HMD VEC     :" + NoloVR_Controller.GetDevice(NoloDeviceType.Hmd).GetPose().vecVelocity + "       HMD ANGULAR     :" + NoloVR_Controller.GetDevice(NoloDeviceType.Hmd).GetPose().vecAngularVelocity + "\n"
            + "LEFT Touch AXIS     :" + NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetAxis(NoloTouchID.TouchPad) + "        RIGHT Touch AXIS    :" + NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetAxis(NoloTouchID.TouchPad) + "\n"
            + "LEFT Trigger AXIS     :" + NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetAxis(NoloTouchID.Trigger) + "       RIGHT Trigger AXIS    :" + NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetAxis(NoloTouchID.Trigger) + "\n"
            + "LEFT POS      :" + NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetPose().pos + "      RIGHT POS     :" + NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetPose().pos + "\n"
            + "LEFT ROT      :" + NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetPose().rot + "      RIGHT ROT     :" + NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetPose().rot + "\n"
            + "LEFT VEC      :" + NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetPose().vecVelocity + "      RIGHT VEC      :" + NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetPose().vecVelocity + "\n"
            + "LEFT ANGULAR      :" + NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetPose().vecAngularVelocity + "       RIGHT ANGULAR      :" + NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetPose().vecAngularVelocity + "\n"
            + "LEFT TRIGGER  :" + NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloButtonPressed(NoloButtonID.Trigger) + "     RIGHT TRIGGER :" + NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetNoloButtonPressed(NoloButtonID.Trigger) + "\n"
            + "LEFT MENU     :" + NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloButtonPressed(NoloButtonID.Menu) + "        RIGHT MENU    :" + NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetNoloButtonPressed(NoloButtonID.Menu) + "\n"
            + "LEFT TOUCHPAD :" + NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloButtonPressed(NoloButtonID.TouchPad) + "        RIGHT TOUCHPAD:" + NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetNoloButtonPressed(NoloButtonID.TouchPad) + "\n"
            + "LEFT SYSTEM   :" + NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloButtonPressed(NoloButtonID.System) + "      RIGHT SYSTEM  :" + NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetNoloButtonPressed(NoloButtonID.System) + "\n"
            + "LEFT GRIP     :" + NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloButtonPressed(NoloButtonID.Grip) + "        RIGHT GRIP    :" + NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetNoloButtonPressed(NoloButtonID.Grip) + "\n"
            //+ "LEFT up     :" + NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloButtonPressed(NoloButtonID.TouchPadUp) + "RIGHT up     :" + NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetNoloButtonPressed(NoloButtonID.TouchPadUp) + "\n"
            //+ "LEFT down     :" + NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloButtonPressed(NoloButtonID.TouchPadDown) + "RIGHT down     :" + NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetNoloButtonPressed(NoloButtonID.TouchPadDown) + "\n"
            //+ "LEFT left     :" + NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloButtonPressed(NoloButtonID.TouchPadLeft) + "RIGHT left     :" + NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetNoloButtonPressed(NoloButtonID.TouchPadLeft) + "\n"
            //+ "LEFT right     :" + NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloButtonPressed(NoloButtonID.TouchPadRight) + "RIGHT right     :" + NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetNoloButtonPressed(NoloButtonID.TouchPadRight) + "\n"
            + "HMD ELE      :" + NoloVR_Plugins.GetElectricity(0) + "       BASE ELE     :" + NoloVR_Plugins.GetElectricity(3) + "\n"
            + "LEFT ELE      :" + NoloVR_Plugins.GetElectricity(1) + "      RIGHT ELE     :" + NoloVR_Plugins.GetElectricity(2) + "\n"
            + " HmdIsconnect:" + NoloVR_Controller.GetDevice(NoloDeviceType.Hmd).GetNoloDeviceConnectStatus() + "\n LeftIsconnect:" + NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloDeviceConnectStatus() + " \nrightIsconnect:" + NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetNoloDeviceConnectStatus()
            ;
            }
        }
        catch (System.Exception e)
        {
            Debug.Log("Catch" + e.Message);
            throw;
        }


    }
}
