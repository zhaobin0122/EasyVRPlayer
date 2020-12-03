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
using UnityEngine.EventSystems;

public class WaveVR_RaycastResult
{
	public GameObject gameObject
	{
		get;
		set;
	}
	public Vector3 worldPosition
	{
		get;
		set;
	}

	public WaveVR_RaycastResult()
	{
		this.gameObject = null;
		this.worldPosition = Vector3.zero;
	}
}
