// "WaveVR SDK 
// © 2017 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the WaveVR SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using wvr;
using WVR_Log;

public class WaveVR_PointerCameraTracker : MonoBehaviour
{
	private const string LOG_TAG = "WaveVR_PointerCameraTracker";

	public GameObject reticleObject = null;
	public WaveVR_Controller.EDeviceType type;
	private WaveVR_ControllerPointer reticle;
	private Vector3 pointer_location;

	private void PrintDebugLog(string msg)
	{
		Log.d (LOG_TAG, msg);
	}

	// Use this for initialization
	void Start()
	{
		if (reticleObject != null)
		{
			reticle = reticleObject.GetComponent<WaveVR_ControllerPointer>();
		}
	}

	// Update is called once per frame
	void Update()
	{
		if (reticleObject != null && reticle == null) {
			reticle = reticleObject.GetComponent<WaveVR_ControllerPointer>();
		}

		if (reticleObject != null && reticle != null)
		{
			pointer_location = reticleObject.transform.position + reticleObject.transform.forward * reticle.PointerDistanceInMeters;
			transform.rotation = Quaternion.LookRotation(pointer_location - transform.position);
		}
	}

	public void setDeviceType(WaveVR_Controller.EDeviceType value)
	{
		type =  value;
	}
}

