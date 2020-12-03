using UnityEngine;
using System.Collections;
using System.Threading;
using NoloClientCSharp;


public class Input_Test : MonoBehaviour {
    //AndroidJavaClass unityPlayer;
    //AndroidJavaObject currentActivity;
    //AndroidJavaObject context;
    //AndroidJavaObject jc, jo;

    void Update () {
        if (NoloVR_System.GetInstance().realTrackDevices == 6)
        {
            //右手所有按键事件
            //触摸板按下、长按、抬起事件
            if (NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetNoloButtonDown(NoloButtonID.TouchPad))
            {
                Debug.Log("RightController TouchPad Down");
            }
            if (NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetNoloButtonPressed(NoloButtonID.TouchPad))
            {
                Debug.Log("RightController TouchPad Pressed");
            }
            if (NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetNoloButtonUp(NoloButtonID.TouchPad))
            {
                Debug.Log("RightController TouchPad Up");
            }

            //扳机按键按下、长按、抬起事件
            if (NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetNoloButtonDown(NoloButtonID.Trigger))
            {
                Debug.Log("RightController Trigger Down");
            }
            if (NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetNoloButtonPressed(NoloButtonID.Trigger))
            {
                Debug.Log("RightController Trigger Pressed");
                NoloVR_Controller.GetDevice(NoloDeviceType.RightController).TriggerHapticPulse(100);
            }
            if (NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetNoloButtonUp(NoloButtonID.Trigger))
            {
                Debug.Log("RightController Trigger Up");
            }


            //home按键按下、长按、抬起事件
            if (NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetNoloButtonDown(NoloButtonID.System))
            {
                Debug.Log("RightController System Down");
            }
            if (NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetNoloButtonPressed(NoloButtonID.System))
            {
                Debug.Log("RightController System Pressed");
            }
            if (NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetNoloButtonUp(NoloButtonID.System))
            {
                Debug.Log("RightController System Up");
            }


            //菜单按键按下、长按、抬起事件
            if (NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetNoloButtonDown(NoloButtonID.Menu))
            {
                Debug.Log("RightController Menu Down");
            }
            if (NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetNoloButtonPressed(NoloButtonID.Menu))
            {
                Debug.Log("RightController Menu Pressed");
            }
            if (NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetNoloButtonUp(NoloButtonID.Menu))
            {
                Debug.Log("RightController Menu Up");
            }


            //抓握按键按下、长按、抬起事件
            if (NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetNoloButtonDown(NoloButtonID.Grip))
            {
                Debug.Log("RightController Grip Down");
            }
            if (NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetNoloButtonPressed(NoloButtonID.Grip))
            {
                Debug.Log("RightController Grip Pressed");
            }
            if (NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetNoloButtonUp(NoloButtonID.Grip))
            {
                Debug.Log("RightController Grip Up");
            }
            if (NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetNoloTouchPressed(NoloTouchID.TouchPad))
            {
                Debug.Log(NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetAxis());
            }

            //左手所有按键事件
            //触摸板按下、长按、抬起事件
            if (NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloButtonDown(NoloButtonID.TouchPad))
            {
                Debug.Log("LeftController TouchPad Down");
            }
            if (NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloButtonPressed(NoloButtonID.TouchPad))
            {
                Debug.Log("LeftController TouchPad Pressed");
            }
            if (NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloButtonUp(NoloButtonID.TouchPad))
            {
                Debug.Log("LeftController TouchPad Up");
            }

            //左手扳机按键按下、长按、抬起事件
            if (NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloButtonDown(NoloButtonID.Trigger))
            {
                Debug.Log("LeftController Trigger Down");
            }
            if (NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloButtonPressed(NoloButtonID.Trigger))
            {
                NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).TriggerHapticPulse(100);
                Debug.Log("LeftController Trigger Pressed");
            }
            if (NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloButtonUp(NoloButtonID.Trigger))
            {
                Debug.Log("LeftController Trigger Up");
            }


            //左手home按下、长按、抬起事件
            if (NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloButtonDown(NoloButtonID.System))
            {
                Debug.Log("LeftController System Down");
            }
            if (NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloButtonPressed(NoloButtonID.System))
            {
                Debug.Log("LeftController System Pressed");
            }
            if (NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloButtonUp(NoloButtonID.System))
            {
                Debug.Log("LeftController System Up");
            }


            //左手菜单按下、长按、抬起事件
            if (NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloButtonDown(NoloButtonID.Menu))
            {
                Debug.Log("LeftController Menu Down");
            }
            if (NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloButtonPressed(NoloButtonID.Menu))
            {
                Debug.Log("LeftController Menu Pressed");
            }
            if (NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloButtonUp(NoloButtonID.Menu))
            {
                Debug.Log("LeftController Menu Up");
            }


            //左手抓握按下、长按、抬起事件
            if (NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloButtonDown(NoloButtonID.Grip))
            {
                Debug.Log("LeftController Grip Down");
            }
            if (NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloButtonPressed(NoloButtonID.Grip))
            {
                Debug.Log("LeftController Grip Pressed");
            }
            if (NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloButtonUp(NoloButtonID.Grip))
            {
                Debug.Log("LeftController Grip Up");
            }
            //左手触摸板轴数据
            if (NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloTouchPressed(NoloTouchID.TouchPad))
            {
                Debug.Log(NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetAxis());
            }
        }
    }
}