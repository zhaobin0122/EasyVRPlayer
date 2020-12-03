using UnityEngine;
using System.Collections;

public class BoundaryRegion_Manager : MonoBehaviour
{

    public GameObject baseStation;

    private bool isOutOfRange = false;

    void OnEnable()
    {
        NOLO_Events.Listen(NOLO_Events.EventsType.TrackingOutofRange, OutOfRange);
        NOLO_Events.Listen(NOLO_Events.EventsType.TrackingInRange, InRange);
    }
    void OnDisable()
    {
        NOLO_Events.Remove(NOLO_Events.EventsType.TrackingOutofRange, OutOfRange);
        NOLO_Events.Remove(NOLO_Events.EventsType.TrackingInRange, InRange);
    }

    void OutOfRange(params object[] args)
    {
        //do out of range
        baseStation.SetActive(true);
        transform.GetChild(0).gameObject.SetActive(true);
        isOutOfRange = true;
    }
    void InRange(params object[] args)
    {
        if (isOutOfRange)
        {
            //do in range
            baseStation.SetActive(false);
            transform.GetChild(0).gameObject.SetActive(false);
            isOutOfRange = false;
        }
    }
}
