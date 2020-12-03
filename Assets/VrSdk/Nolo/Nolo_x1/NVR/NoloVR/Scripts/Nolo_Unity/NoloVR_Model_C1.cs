using UnityEngine;
using System.Collections;

public class NoloVR_Model_C1 : MonoBehaviour
{
    NoloVR_TrackedDevice trackedDevice;
    Transform touchpad;
    Transform back;
    Transform system;
    Transform trigger;
    Transform volume;

    // Use this for initialization
    void OnEnable()
    {
        trackedDevice = GetComponentInParent<NoloVR_TrackedDevice>();
        touchpad = transform.Find("buttons/button_touchpad");
        back = transform.Find("buttons/button_back");
        system = transform.Find("buttons/button_system");
        trigger = transform.Find("buttons/button_trigger");
        volume = transform.Find("buttons/button_volume");

    }
    void Update()
    {
        if (NoloVR_System.GetInstance().realTrackDevices == 3)
        {
            if (NoloVR_Controller.GetDevice(trackedDevice).GetNoloButtonPressed((NoloButtonID)NoloC1ButtonID.TouchPad))
            {
                TouchPad_Down();
            }
            else
            {
                TouchPad_Up();
            }

            if (NoloVR_Controller.GetDevice(trackedDevice).GetNoloButtonPressed((NoloButtonID)NoloC1ButtonID.Back))
            {
                Back_Down();
            }
            else
            {
                Back_Up();
            }

            if (NoloVR_Controller.GetDevice(trackedDevice).GetNoloButtonPressed((NoloButtonID)NoloC1ButtonID.System))
            {
                System_Down();
            }
            else
            {
                System_Up();
            }
            if (NoloVR_Controller.GetDevice(trackedDevice).GetNoloButtonPressed((NoloButtonID)NoloC1ButtonID.Trigger))
            {
                Trigger_Down();
            }
            else
            {
                Trigger_Up();
            }
            if (NoloVR_Controller.GetDevice(trackedDevice).GetNoloButtonPressed((NoloButtonID)NoloC1ButtonID.VolumeDown))
            {
                VolumeDown_Down();
            }
            else if (NoloVR_Controller.GetDevice(trackedDevice).GetNoloButtonPressed((NoloButtonID)NoloC1ButtonID.VolumeUp))
            {
                VolumeUp_Down();
            }
            else
            {
                Volume_Up();
            }
        }
    }

    //touchpad
    void TouchPad_Down()
    {
        touchpad.transform.localPosition = new Vector3(0, -1, 0);
    }
    void TouchPad_Up()
    {
        touchpad.transform.localPosition = Vector3.zero;
    }

    //back
    void Back_Down()
    {
        back.transform.localPosition = new Vector3(0, -1, 0);
    }
    void Back_Up()
    {
        back.transform.localPosition = Vector3.zero;
    }

    //system
    void System_Down()
    {
        system.transform.localPosition = new Vector3(0, -1, 0);
    }
    void System_Up()
    {
        system.transform.localPosition = Vector3.zero;
    }

    //trigger
    void Trigger_Down()
    {
        trigger.transform.localPosition = new Vector3(0.2f, 2.6f, 0.2f);
        trigger.transform.localRotation = Quaternion.Euler(0, 0, -5);
    }
    void Trigger_Up()
    {
        trigger.transform.localPosition = Vector3.zero;
        trigger.transform.localRotation = Quaternion.identity;
    }

    //volume
    void VolumeDown_Down()
    {
        volume.transform.localPosition = Vector3.zero;
        volume.transform.localRotation = Quaternion.Euler(0, -2, 0);
    }
    void VolumeUp_Down()
    {
        volume.transform.localPosition = Vector3.zero;
        volume.transform.localRotation = Quaternion.Euler(0, 2, 0);
    }
    void Volume_Up()
    {
        volume.transform.localPosition = Vector3.zero;
        volume.transform.localRotation = Quaternion.identity;
    }
}
