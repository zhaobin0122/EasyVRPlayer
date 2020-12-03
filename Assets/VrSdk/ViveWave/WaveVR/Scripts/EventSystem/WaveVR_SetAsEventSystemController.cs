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

public class WaveVR_SetAsEventSystemController : MonoBehaviour
{
	private bool added = false;
	public WaveVR_Controller.EDeviceType Type = WaveVR_Controller.EDeviceType.Dominant;

	void OnEnable()
	{
		WaveVR_EventSystemControllerProvider.Instance.SetControllerModel (Type, gameObject);
		added = true;
	}

	void OnDisable()
	{
		if (added)
		{
			WaveVR_EventSystemControllerProvider.Instance.SetControllerModel (Type, null);
			added = false;
		}
	}
}
