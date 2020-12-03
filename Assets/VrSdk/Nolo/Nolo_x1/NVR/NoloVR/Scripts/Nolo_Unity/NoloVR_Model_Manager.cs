using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoloVR_Model_Manager : MonoBehaviour {

    void Start()
    {
        //事件监听激活3/6 dof手柄
        NOLO_Events.Listen(NOLO_Events.EventsType.GetTrackModel, GetTrackModel);
        //默认激活6 dof手柄
        {
            if (transform.Find("NOLO_Controller") != null)
            {
                transform.Find("NOLO_Controller").gameObject.SetActive(true);
            }
        }
    }

    void OnDestroy()
    {
        NOLO_Events.Remove(NOLO_Events.EventsType.GetTrackModel, GetTrackModel);
    }

    public void GetTrackModel(params object[] args)
    {
        if (NoloVR_System.GetInstance().realTrackDevices == 3) //3dof  隐藏6dof 显示3dof
        {
            if (transform.Find("NOLO_Controller_C1") != null)
            {
                transform.Find("NOLO_Controller_C1").gameObject.SetActive(true);
            }
            if (transform.Find("NOLO_Controller") != null)
            {
                transform.Find("NOLO_Controller").gameObject.SetActive(false);
            }
        }
        else //6dof  隐藏3dof 显示6dof
        {
            if (transform.Find("NOLO_Controller_C1") != null)
            {
                transform.Find("NOLO_Controller_C1").gameObject.SetActive(false);
            }
            if (transform.Find("NOLO_Controller") != null)
            {
                transform.Find("NOLO_Controller").gameObject.SetActive(true);
            }
        }
    }
}
