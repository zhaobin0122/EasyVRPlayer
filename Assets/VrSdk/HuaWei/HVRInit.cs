using System.Collections;
using System.Collections.Generic;
using HVRCORE;
using UnityEngine;

public class HVRInit : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        float mNearClipPlane = 0.02f;
        float mFarClipPlane = 1000.0f;
        HVRPluginCore.HVR_SetClipPlaneParams(mNearClipPlane,mFarClipPlane);
    }
}
