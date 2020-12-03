// "WaveVR SDK 
// © 2017 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the WaveVR SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using UnityEngine;
using wvr;

[RequireComponent(typeof(Canvas)), System.Obsolete("The Canvas ScreenSpaceCamera is not workable in VR mode. (Unity design)")]
public class WaveVR_CanvasEye : MonoBehaviour
{
	private Canvas canvas;

	// Use this for initialization
	void Start()
	{
		canvas = GetComponent<Canvas>();
		canvas.worldCamera = null;
	}

	void OnEnable()
	{
		if (WaveVR_Render.Instance)
			WaveVR_Render.Instance.beforeRenderEye += MyRenderEye;
	}

	void OnDisable()
	{
		if (WaveVR_Render.Instance)
			WaveVR_Render.Instance.beforeRenderEye -= MyRenderEye;
	}

	void MyRenderEye(WaveVR_Render render, WVR_Eye eye, WaveVR_Camera wvrCamera)
	{
		if (eye == WVR_Eye.WVR_Eye_Both)
			return;
		var camera = wvrCamera.GetCamera();
		canvas.worldCamera = camera;
		canvas.renderMode = RenderMode.ScreenSpaceCamera;
	}
}
