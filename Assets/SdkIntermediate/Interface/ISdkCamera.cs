using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISdkCamera
{
    //获取sdk的Camera
     GameObject GetSdkCamera();
    //获取sdk的左眼camera
    Camera GetLeftSdkCamera();
    //获取sdk的右眼camera
    Camera GetRightSdkCamera();
    //设置左眼cullingMask
    void SetLeftSdkCameraCullingMask(int cullingMask);
    //设置右眼cullingMask
    void SetRightSdkCameraCullingMask(int cullingMask);
    //切换场景是否不销毁摄像头
    bool IsDontDestroyCamera();
}
