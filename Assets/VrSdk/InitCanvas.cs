using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitCanvas : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        BindCanvas();
    }

    public void BindCanvas()
    {
        CanvasManager.GetInstance().BindSdkCameraForCanvas(gameObject);
    }
}
