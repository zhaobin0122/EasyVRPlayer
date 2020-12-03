using UnityEngine;
using UnityEngine.EventSystems;

public abstract class HVRRaycasterBase : BaseRaycaster
{

    private Ray lastRay;
    public GameObject rayObject;
    public float maxPointerDistance;

    public Ray GetLastRay()
    {
        return lastRay;
    }
    protected override void Start()
    {
       // rayObject = HvrController.m_LineObj;
    }
    public Ray GetRay()
    {
        maxPointerDistance = HVRController.m_DefaultDistance;
        if(Application.platform == RuntimePlatform.Android)
        {
            if (rayObject == null)
            {
                return new Ray();
            }
            lastRay = new Ray(rayObject.transform.position + HVRController.m_ObjForwardDir * rayObject.transform.forward , rayObject.transform.forward);
        }
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            lastRay = Camera.allCameras[0].ScreenPointToRay(Input.mousePosition);
        }
        return lastRay;
    }
}