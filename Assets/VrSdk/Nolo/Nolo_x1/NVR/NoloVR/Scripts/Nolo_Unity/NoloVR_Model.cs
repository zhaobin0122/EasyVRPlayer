using UnityEngine;
using System.Collections;

public class NoloVR_Model : MonoBehaviour
{
    NoloVR_TrackedDevice trackedDevice;
    Transform touchpad;
    Transform menu;
    Transform system;
    Transform grip_left;
    Transform grip_right;
    Transform trigger;


    // Use this for initialization
    void OnEnable()
    {
        trackedDevice = GetComponentInParent<NoloVR_TrackedDevice>();
        touchpad = transform.Find("buttons/button_touchpad");
        menu = transform.Find("buttons/button_menu");
        system = transform.Find("buttons/button_system");
        grip_left = transform.Find("buttons/button_grip_left");
        grip_right = transform.Find("buttons/button_grip_right");
        trigger = transform.Find("buttons/button_trigger");

    }
    void Update()
    {
        if (NoloVR_Controller.GetDevice(trackedDevice).GetNoloButtonPressed(NoloButtonID.TouchPad))
        {
            TouchPad_Down();
        }
        else
        {
            TouchPad_Up();
        }

        if (NoloVR_Controller.GetDevice(trackedDevice).GetNoloButtonPressed(NoloButtonID.Menu))
        {
            Menu_Down();
        }
        else
        {
            Menu_Up();
        }

        if (NoloVR_Controller.GetDevice(trackedDevice).GetNoloButtonPressed(NoloButtonID.System))
        {
            System_Down();
        }
        else
        {
            System_Up();
        }

        if (NoloVR_Controller.GetDevice(trackedDevice).GetNoloButtonPressed(NoloButtonID.Grip))
        {
            Grip_Down();
        }
        else
        {
            Grip_Up();
        }

        if (NoloVR_Controller.GetDevice(trackedDevice).GetNoloButtonPressed(NoloButtonID.Trigger))
        {
            Trigger_Down();
        }
        else
        {
            Trigger_Up();
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

    //menu
    void Menu_Down()
    {
        menu.transform.localPosition = new Vector3(0, -1, 0);
    }
    void Menu_Up()
    {
        menu.transform.localPosition = Vector3.zero;
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
        trigger.transform.localPosition = new Vector3(0, 12, -5);
        trigger.transform.localRotation = Quaternion.Euler(-20, 0, 0);
    }
    void Trigger_Up()
    {
        trigger.transform.localPosition = Vector3.zero;
        trigger.transform.localRotation = Quaternion.identity;
    }

    //grip
    void Grip_Down()
    {
        grip_left.transform.localPosition = new Vector3(1, 0, 0);
        grip_right.transform.localPosition = new Vector3(-1, 0, 0);
    }
    void Grip_Up()
    {
        grip_left.transform.localPosition = Vector3.zero;
        grip_right.transform.localPosition = Vector3.zero;
    }

}
