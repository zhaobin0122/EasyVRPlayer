using UnityEngine;
using System.Collections;

public class NoloVR_BaseStation : MonoBehaviour
{
    public bool showTrackingBoundary = false;
    void Start()
    {
        enabled = showTrackingBoundary;
    }
    void Update()
    {
        if (NoloVR_Playform.GetInstance().GetPlayformError() != NoloError.None)
        {
            return;
        }
        if(showTrackingBoundary == false)
        {
            return;
        }
        var pose = NoloVR_Controller.GetDevice(NoloDeviceType.BaseStation).GetPose();
        transform.localPosition = pose.pos;
        transform.localRotation = pose.rot;

        for (int i = 0; i < NoloVR_System.GetInstance().objects.Length; i++)
        {
            if (Mathf.Abs(NoloVR_System.GetInstance().objects[i].transform.localPosition.x) > Mathf.Abs(transform.position.z - NoloVR_System.GetInstance().objects[i].transform.localPosition.z) ||
                Mathf.Abs(NoloVR_System.GetInstance().objects[i].transform.localPosition.y - transform.position.y) > Mathf.Abs(transform.position.z - NoloVR_System.GetInstance().objects[i].transform.localPosition.z + 0.1f))
            {
                NOLO_Events.Send(NOLO_Events.EventsType.TrackingOutofRange);
                break;
            }
            else
            {
                NOLO_Events.Send(NOLO_Events.EventsType.TrackingInRange);
            }
        }

    }
}
