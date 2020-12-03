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
using WVR_Log;
using System.Collections;

public class WaveVR_Init : MonoBehaviour
{
	private const string LOG_TAG = "WaveVR_Init";
	private void PrintDebugLog(string msg)
	{
		if (Log.EnableDebugLog)
			Log.d (LOG_TAG, msg, true);
	}

	/// <summary>
	/// The singleton instance of the <see cref="WaveVR_Init"/> class, there only be one instance in a scene.
	/// </summary>
	public static WaveVR_Init Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FindObjectOfType<WaveVR_Init>();
				if (_instance == null)
				{
					Log.i (LOG_TAG, "WaveVR_Init create an instance");
					_instance = new GameObject("[WaveVR]").AddComponent<WaveVR_Init>();
				}
			}
			return _instance;
		}
	}
	private static WaveVR_Init _instance;

	void signalSurfaceState(string msg) {
		WaveVR_Render.signalSurfaceState(msg);
	}

	void Start()
	{
		if (WaveVR.Instance.Initialized)
		{
			Log.i (LOG_TAG, "Start()", true);
			WaveVR.Instance.onLoadLevel ();
			WaveVR.Instance.UpdateAllConnection ();
			WaveVR.Instance.SetLeftHandedMode ();
		}
	}

	void Awake()
	{
		if (_instance == null)
		{
			_instance = this;
		}
		if (_instance != this)
		{
			Log.w(LOG_TAG, "Has another [WaveVR] object in a scene. Destory this.");
			Destroy(this);
			return;
		}

		if (WaveVR.Instance.Initialized)
		{
			Log.i(LOG_TAG, "Initialized");
		}
	}

	private bool toCheckStatesWhenNoEvent = false;
	private void CheckStatesWhenNoEvent()
	{
		if (!toCheckStatesWhenNoEvent)
			return;

		// Check connection states.
		Log.d(LOG_TAG, "CheckStatesWhenNoEvent() UpdateAllConnection.");
		WaveVR.Instance.UpdateAllConnection();

		toCheckStatesWhenNoEvent = false;
	}

	WVR_Event_t vrevent = new WVR_Event_t ();
	void Update()
	{
		bool ret = Interop.WVR_PollEventQueue (ref vrevent);
		if (ret)
			processVREvent (vrevent);
		else
			CheckStatesWhenNoEvent ();
	}

	void OnApplicationPause(bool pauseStatus)
	{
		PrintDebugLog ("OnApplicationPause() pauseStatus: " + pauseStatus);
		if (!pauseStatus)
		{
			// Application resume.
			if (WaveVR.Instance.Initialized)
			{
				WaveVR.Instance.UpdateAllConnection ();
				// If system resumes and role changes, reset all button state.
				if (WaveVR.Instance.SetLeftHandedMode ())
				{
					WaveVR_Utils.Event.Send (WaveVR_Utils.Event.DEVICE_ROLE_CHANGED);
					WaveVR.Instance.ResetAllButtonStates ();
				} else
				{
					WaveVR.Instance.UpdateButtonEvents ();
				}
			}
		}
	}

	private void processVREvent(WVR_Event_t vrEvent)
	{
		WVR_DeviceType _type = vrEvent.device.type;
		WVR_InputId _btn = vrEvent.input.inputId;
		// Process events used by plugin
		switch ((WVR_EventType)vrEvent.common.type)
		{
		case WVR_EventType.WVR_EventType_IpdChanged:
			WaveVR_Utils.Event.Send (WaveVR_Utils.Event.IPD_CHANGED);
			if (WaveVR_Render.Instance != null)
				WaveVR_Render.Expand (WaveVR_Render.Instance);
			break;
		case WVR_EventType.WVR_EventType_DeviceStatusUpdate:
			WaveVR_Utils.Event.Send (WaveVR_Utils.Event.DEVICE_STATUS_UPDATE, vrEvent.device.common.type);
			break;
		case WVR_EventType.WVR_EventType_BatteryStatusUpdate:
			WaveVR_Utils.Event.Send (WaveVR_Utils.Event.BATTERY_STATUS_UPDATE);
			break;
		case WVR_EventType.WVR_EventType_LeftToRightSwipe:
		case WVR_EventType.WVR_EventType_RightToLeftSwipe:
		case WVR_EventType.WVR_EventType_DownToUpSwipe:
		case WVR_EventType.WVR_EventType_UpToDownSwipe:
			PrintDebugLog ("processVREvent() Swipe event: " + (WVR_EventType)vrEvent.common.type);
			WaveVR_Utils.Event.Send (WaveVR_Utils.Event.SWIPE_EVENT, vrEvent.common.type, _type);
			break;
		case WVR_EventType.WVR_EventType_DeviceRoleChanged:
			if (WaveVR.Instance.SetLeftHandedMode ())
			{
				PrintDebugLog ("processVREvent() WVR_EventType_DeviceRoleChanged: " + _type + ", " + _btn + ", Resend connection notification after switching hand.");
				// Due to connected, valid pose and role change are not synchronized, DEVICE_ROLE_CHANGED will be obseleted.
				WaveVR_Utils.Event.Send (WaveVR_Utils.Event.DEVICE_ROLE_CHANGED);
				WaveVR.Instance.ResetAllButtonStates ();
				WaveVR.Instance.UpdateAllConnection ();
			}
			break;
		case WVR_EventType.WVR_EventType_ButtonPressed:
			PrintDebugLog ("processVREvent() WVR_EventType_ButtonPressed: " + _type + ", " + _btn + ", left-handed? " + WaveVR_Controller.IsLeftHanded);
			if (_type != WVR_DeviceType.WVR_DeviceType_Invalid && WaveVR.Instance.Initialized)
				_type = WaveVR.Instance.getDeviceByType (_type).type;
			WaveVR_Controller.Input (_type).SetEventState_Press (_btn, true);
			break;
		case WVR_EventType.WVR_EventType_ButtonUnpressed:
			PrintDebugLog ("processVREvent() WVR_EventType_ButtonUnpressed: " + _type + ", " + _btn + ", left-handed? " + WaveVR_Controller.IsLeftHanded);
			if (_type != WVR_DeviceType.WVR_DeviceType_Invalid && WaveVR.Instance.Initialized)
				_type = WaveVR.Instance.getDeviceByType (_type).type;
			WaveVR_Controller.Input (_type).SetEventState_Press (_btn, false);
			break;
		case WVR_EventType.WVR_EventType_TouchTapped:
			PrintDebugLog ("processVREvent() WVR_EventType_TouchTapped: " + _type + ", " + _btn + ", left-handed? " + WaveVR_Controller.IsLeftHanded);
			if (_type != WVR_DeviceType.WVR_DeviceType_Invalid && WaveVR.Instance.Initialized)
				_type = WaveVR.Instance.getDeviceByType (_type).type;
			WaveVR_Controller.Input (_type).SetEventState_Touch (_btn, true);
			break;
		case WVR_EventType.WVR_EventType_TouchUntapped:
			PrintDebugLog ("processVREvent() WVR_EventType_TouchUntapped: " + _type + ", " + _btn + ", left-handed? " + WaveVR_Controller.IsLeftHanded);
			if (_type != WVR_DeviceType.WVR_DeviceType_Invalid && WaveVR.Instance.Initialized)
				_type = WaveVR.Instance.getDeviceByType (_type).type;
			WaveVR_Controller.Input (_type).SetEventState_Touch (_btn, false);
			break;
		case WVR_EventType.WVR_EventType_DeviceConnected:
			PrintDebugLog ("processVREvent() WVR_EventType_DeviceConnected: " + _type + ", left-handed? " + WaveVR_Controller.IsLeftHanded);
			toCheckStatesWhenNoEvent = true;
			break;
		case WVR_EventType.WVR_EventType_DeviceDisconnected:
			PrintDebugLog ("processVREvent() WVR_EventType_DeviceDisconnected: " + _type + ", left-handed? " + WaveVR_Controller.IsLeftHanded);
			toCheckStatesWhenNoEvent = true;

			if (_type != WVR_DeviceType.WVR_DeviceType_Invalid)
			{
				_type = WaveVR_Controller.Input (_type).DeviceType;
				WaveVR_Controller.Input (_type).ResetButtonEvents ();
			}
			break;
		default:
			break;
		}

		// Send event to developer for all kind of event if developer don't want to add callbacks for every event.
		WaveVR_Utils.Event.Send(WaveVR_Utils.Event.ALL_VREVENT, vrEvent);

		// Send event to developer by name.
		WaveVR_Utils.Event.Send(vrEvent.common.type.ToString(), vrEvent);
	}

	void OnDestroy()
	{
		if (_instance == this)
		{
			_instance = null;
		}
	}
}

