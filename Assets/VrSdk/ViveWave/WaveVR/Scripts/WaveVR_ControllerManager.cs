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

public class WaveVR_ControllerManager : MonoBehaviour
{
	private static string LOG_TAG = "WaveVR_ControllerManager";
	private void PrintDebugLog(string msg)
	{
		Log.d (LOG_TAG, msg);
	}

	public GameObject Dominant, NonDominant;

	public enum CIndex
	{
		invalid = -1,
		Dominant = 0,
		NonDominant = 1
	}
	private GameObject[] ControllerObjects; // populate with objects you want to assign to additional controllers
	private bool[] ControllerConnected = new bool[2]{false, false};

	#region Override functions
	void Awake()
	{
		var objects = new GameObject[2];
		objects [(uint)CIndex.Dominant] = Dominant;
		objects [(uint)CIndex.NonDominant] = NonDominant;

		this.ControllerObjects = objects;
	}

	void OnEnable()
	{
		for (int i = 0; i < ControllerObjects.Length; i++)
		{
			var obj = ControllerObjects[i];
			if (obj != null)
			{
				PrintDebugLog ("OnEnable() disable controller " + i);
				obj.SetActive (false);
			}
		}

		checkConnection ();

		WaveVR_Utils.Event.Listen(WaveVR_Utils.Event.DEVICE_CONNECTED, onDeviceConnected);
	}

	void OnDisable()
	{
		WaveVR_Utils.Event.Remove(WaveVR_Utils.Event.DEVICE_CONNECTED, onDeviceConnected);
	}
	#endregion

	private void BroadcastToObjects(CIndex index)
	{
		var obj = ControllerObjects [(uint)index];
		if (obj != null)
		{
			if (ControllerConnected [(uint)index] == false)
			{
				PrintDebugLog ("BroadcastToObjects() disable controller " + index);
				obj.SetActive (false);
			} else
			{
				PrintDebugLog ("BroadcastToObjects() enable controller " + index);
				// means object with index is not null and connected.
				obj.SetActive(true);
				WaveVR_Controller.EDeviceType _device = index == CIndex.Dominant ?
					WaveVR_Controller.EDeviceType.Dominant :
					WaveVR_Controller.EDeviceType.NonDominant;

				obj.BroadcastMessage("SetDeviceIndex", _device, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	private void checkConnection()
	{
		bool _connected_D = false, _connected_ND = false;

		WaveVR.Device _dev_D = WaveVR.Instance.getDeviceByType (WaveVR_Controller.EDeviceType.Dominant);
		if (_dev_D != null)
			_connected_D = _dev_D.connected;

		WaveVR.Device _dev_ND = WaveVR.Instance.getDeviceByType (WaveVR_Controller.EDeviceType.NonDominant);
		if (_dev_ND != null)
			_connected_ND = _dev_ND.connected;

		if (ControllerConnected [(uint)CIndex.Dominant] != _connected_D)
		{
			PrintDebugLog ("checkConnection() dominant device  is " + (_connected_D == true ? "connected" : "disconnected")
			+ ", left-handed? " + WaveVR_Controller.IsLeftHanded);
			ControllerConnected [(uint)CIndex.Dominant] = _connected_D;
			BroadcastToObjects (CIndex.Dominant);
		}

		if (ControllerConnected [(uint)CIndex.NonDominant] != _connected_ND)
		{
			PrintDebugLog ("checkConnection() non-dominant device  is " + (_connected_ND == true ? "connected" : "disconnected")
			+ ", left-handed? " + WaveVR_Controller.IsLeftHanded);
			ControllerConnected [(uint)CIndex.NonDominant] = _connected_ND;
			BroadcastToObjects (CIndex.NonDominant);
		}
	}

	private void onDeviceConnected(params object[] args)
	{
		WVR_DeviceType _type = (WVR_DeviceType)args [0];
		bool _connected = (bool)args [1];
		PrintDebugLog ("onDeviceConnected() device " + _type + " is " + (_connected ? "connected." : "disconnected."));

		checkConnection ();
	}
}
