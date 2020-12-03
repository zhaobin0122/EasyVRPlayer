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

public class WaveVR_AddEventSystemGUI : MonoBehaviour
{
	private bool added = false;
	void OnEnable()
	{
		Canvas _canvas = (Canvas)gameObject.GetComponent (typeof(Canvas));
		if (_canvas != null)
		{
			WaveVR_EventSystemGUIProvider.AddEventGUI (gameObject);
			added = true;
		}
	}

	void OnDisable()
	{
		if (added)
		{
			WaveVR_EventSystemGUIProvider.RemoveEventGUI (gameObject);
			added = false;
		}
	}
}
