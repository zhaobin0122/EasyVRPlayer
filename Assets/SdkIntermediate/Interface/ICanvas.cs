using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICanvas
{
    //为canvas绑定sdk camera
    void BindSdkCameraForCanvas(GameObject canvas);
}
