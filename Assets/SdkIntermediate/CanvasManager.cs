using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasManager : ICanvas
{
    private static CanvasManager canvasManager = new CanvasManager();

    public static CanvasManager GetInstance()
    {
        return canvasManager;
    }

    public void BindSdkCameraForCanvas(GameObject canvas)
    {
       ICanvas iCanvas = (ICanvas)TargetSdkManager.GetTargetSdkHelperInstance();
       iCanvas.BindSdkCameraForCanvas(canvas);
    }


}
