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
using System;
using System.Collections;
using UnityEngine.EventSystems;
using WVR_Log;
using System.Collections.Generic;

public class WaveVR_EventSystemControllerProvider
{
	private const string LOG_TAG = "WaveVR_EventSystemControllerProvider";

	private void PrintDebugLog(string msg)
	{
		if (Log.EnableDebugLog)
			Log.d (LOG_TAG, msg);
	}

	public static WaveVR_EventSystemControllerProvider Instance
	{
		get
		{
			if (instance == null)
				instance = new WaveVR_EventSystemControllerProvider();
			return instance;
		}
	}
	private static WaveVR_EventSystemControllerProvider instance = null;

	public class ControllerModel
	{
		public WaveVR_Controller.EDeviceType DeviceType { get; set; }
		public GameObject Model { get; set; }
		public bool HasLoader { get; set; }

		public ControllerModel(WaveVR_Controller.EDeviceType type, GameObject model)
		{
			DeviceType = type;
			Model = model;
			HasLoader = false;
		}
	}


	private List<ControllerModel> ControllerModels = new List<ControllerModel>();

	private WaveVR_EventSystemControllerProvider()
	{
	}

	public void SetControllerModel (WaveVR_Controller.EDeviceType type, GameObject model)
	{
		PrintDebugLog ("SetControllerModel() type: " + type + ", Model: " + (model != null ? model.name : "null"));
		bool found = false;
		for (int i = 0; i < ControllerModels.Count; i++)
		{
			if (ControllerModels [i].DeviceType == type)
			{
				if (ControllerModels [i].Model != null)
					ControllerModels [i].Model.SetActive (false);
				
				ControllerModels [i].Model = model;
				ControllerModels [i].Model.SetActive (true);
				found = true;
				break;
			}
		}
		if (!found)
			ControllerModels.Add (new ControllerModel (type, model));
	}

	public GameObject GetControllerModel(WaveVR_Controller.EDeviceType type)
	{
		for (int i = 0; i < ControllerModels.Count; i++)
		{
			if (ControllerModels [i].DeviceType == type)
			{
				return ControllerModels [i].Model;
			}
		}
		return null;
	}

	public void MarkControllerLoader(WaveVR_Controller.EDeviceType type, bool value)
	{
		PrintDebugLog (type + " " + (value ? "has" : "doesn't have") + " ControllerLoader.");
		for (int i = 0; i < ControllerModels.Count; i++)
		{
			if (ControllerModels [i].DeviceType == type)
			{
				ControllerModels [i].HasLoader = value;
				return;
			}
		}
	}

	public bool HasControllerLoader(WaveVR_Controller.EDeviceType type)
	{
		for (int i = 0; i < ControllerModels.Count; i++)
		{
			if (ControllerModels [i].DeviceType == type)
			{
				return ControllerModels [i].HasLoader;
			}
		}
		return false;
	}
}
