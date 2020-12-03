using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitSdkCamera : MonoBehaviour
{
    private static GameObject sdkCamera;

    private void Awake()
    {
        if (SdkCameraManager.GetInstance().IsDontDestroyCamera())
        {
            if (sdkCamera == null)
            {
                sdkCamera = SdkCameraManager.GetInstance().GetSdkCamera();
                sdkCamera.AddComponent<Singleton>();
                Instantiate(sdkCamera);
            }
        }
        else {
            GameObject sdkCamera = SdkCameraManager.GetInstance().GetSdkCamera();
            Instantiate(sdkCamera);
        }

        SdkCameraManager.GetInstance().SetLeftSdkCameraCullingMask(~(1 << LayerMask.NameToLayer("rightSphere")));
        SdkCameraManager.GetInstance().SetRightSdkCameraCullingMask(~(1 << LayerMask.NameToLayer("leftSphere")));
    }

}
