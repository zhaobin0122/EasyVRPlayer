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
using UnityEngine.EventSystems;
using System;

public class WaveVR_ControllerInstanceManager : MonoBehaviour {
	private static string LOG_TAG = "WaveVR_ControllerInstanceManager";
	private void PrintDebugLog(string msg)
	{
		if (Log.EnableDebugLog)
			Log.d (LOG_TAG, msg, true);
	}

	[System.Serializable]
	private class ControllerInstance
	{
		public WVR_DeviceType type;
		public GameObject instance;
		public int index;
		public bool eventEnabled;
		public bool showBeam;
		public bool showPointer;
	}

	private enum CComponent
	{
		Beam,
		ControllerPointer
	};

	private static WaveVR_ControllerInstanceManager instance = null;
	public static WaveVR_ControllerInstanceManager Instance
	{
		get
		{
			if (instance == null)
			{
				Log.i(LOG_TAG, "Instance, create WaveVR_ControllerInstanceManager GameObject", true);
				var gameObject = new GameObject("WaveVR_ControllerInstanceManager");
				instance = gameObject.AddComponent<WaveVR_ControllerInstanceManager>();
				// This object should survive all scene transitions.
				GameObject.DontDestroyOnLoad(instance);
			}
			return instance;
		}
	}

	private int ControllerIdx = 0;
	private GameObject eventSystem = null;
	private List<ControllerInstance> ctrInstanceList = new List<ControllerInstance>();
	public WVR_DeviceType ControllerFocus = WVR_DeviceType.WVR_DeviceType_Controller_Right;
	private WVR_DeviceType lastControllerFocus = WVR_DeviceType.WVR_DeviceType_Invalid;
	private bool mFocusCapturedBySystem = false;
	public bool EnableSingleBeam = true;

	private bool rConnected = false, lConnected = false;

	private bool getEventSystemParameter(WVR_DeviceType type)
	{
		bool ret = false;
		if (EventSystem.current == null)
		{
			EventSystem _es = FindObjectOfType<EventSystem>();
			if (_es != null)
			{
				eventSystem = _es.gameObject;
			}
		}
		else
		{
			eventSystem = EventSystem.current.gameObject;
		}

		if (eventSystem != null)
		{
			WaveVR_ControllerInputModule wcim = eventSystem.GetComponent<WaveVR_ControllerInputModule>();

			if (wcim != null)
			{
				if (type == WVR_DeviceType.WVR_DeviceType_Controller_Right)
				{
					ret = wcim.DomintEventEnabled;
					PrintDebugLog("getEventSystemParameter() DomintEventEnabled is " + ret);
				}
				else if (type == WVR_DeviceType.WVR_DeviceType_Controller_Left)
				{
					ret = wcim.NoDomtEventEnabled;
					PrintDebugLog("getEventSystemParameter() NoDomtEventEnabled is " + ret);
				}

			}
		}

		return ret;
	}

	private bool getComponentParameter(GameObject controller, CComponent comp)
	{
		bool ret = false;

		var ch = controller.transform.childCount;

		for (int i = 0; i < ch; i++)
		{
			GameObject child = controller.transform.GetChild(i).gameObject;

			if (comp == CComponent.Beam)
			{
				WaveVR_Beam wb = child.GetComponentInChildren<WaveVR_Beam>();
				if (wb != null)
				{
					ret = wb.ShowBeam;
					PrintDebugLog("getComponentParameter() wb.ShowBeam is " + ret);
					break;
				}
			}
			else if (comp == CComponent.ControllerPointer)
			{
				WaveVR_ControllerPointer wcp = child.GetComponentInChildren<WaveVR_ControllerPointer>();
				if (wcp != null)
				{
					ret = wcp.ShowPointer;
					PrintDebugLog("getComponentParameter() wcp.ShowPointer is " + ret);
					break;
				}
			}
		}

		return ret;
	}

	public int registerControllerInstance(WVR_DeviceType type, GameObject controller)
	{
		PrintDebugLog ("registerControllerInstance() " + type + ", controller: " + (controller != null ? controller.name : "null"));
		if (type != WVR_DeviceType.WVR_DeviceType_Controller_Left && type != WVR_DeviceType.WVR_DeviceType_Controller_Right)
		{
			PrintDebugLog("registerControllerInstance, type is not allowed");
			return 0;
		}

		if (controller == null)
		{
			PrintDebugLog("registerControllerInstance, controller is null");
			return 0;
		}

		ControllerIdx++;

		ControllerInstance t = new ControllerInstance();
		t.type = type;
		t.instance = controller;
		t.index = ControllerIdx;

		t.eventEnabled = getEventSystemParameter(type);
		t.showBeam = getComponentParameter(controller, CComponent.Beam);
		t.showPointer = getComponentParameter(controller, CComponent.ControllerPointer);

		ctrInstanceList.Add(t);
		PrintDebugLog("registerControllerInstance, add controller index: " + t.index + ", type: " + t.type + ", name: " + t.instance.name
			+ ", event able: " + t.eventEnabled + ", ShowBeam: " + t.showBeam + ", showPointer: " + t.showPointer);

		return ControllerIdx;
	}

	public void removeControllerInstance(int index)
	{
		ControllerInstance waitforRemove = null;
		foreach (ControllerInstance t in ctrInstanceList)
		{
			if (t.index == index)
			{
				PrintDebugLog("removeControllerInstance, remove controller index: " + t.index + ", type: " + t.type);
				waitforRemove = t;
			}
		}

		if (waitforRemove != null)
		{
			ctrInstanceList.Remove (waitforRemove);
		}
	}

	private void onDeviceConnected(params object[] args)
	{
		WVR_DeviceType _type = (WVR_DeviceType)args [0];
		bool _connected = (bool)args [1];
		PrintDebugLog ("onDeviceConnected() device " + _type + " is " + (_connected ? "connected." : "disconnected.") + ", left-handed? " + WaveVR_Controller.IsLeftHanded);

		WaveVR.Device _rdev = WaveVR.Instance.getDeviceByType (WVR_DeviceType.WVR_DeviceType_Controller_Right);
		WaveVR.Device _ldev = WaveVR.Instance.getDeviceByType (WVR_DeviceType.WVR_DeviceType_Controller_Left);

		if (_type == _rdev.type)
		{
			this.rConnected = _rdev.connected;
			PrintDebugLog ("onDeviceConnected() rConnected: " + this.rConnected);
		}
		if (_type == _ldev.type)
		{
			this.lConnected = _ldev.connected;
			PrintDebugLog ("onDeviceConnected() lConnected: " + this.lConnected);
		}
	}

	private void onSystemFocusChanged(params object[] args)
	{
		bool _focusCapturedBySystem = (bool)args [0];
		if (this.mFocusCapturedBySystem != _focusCapturedBySystem)
		{
			this.mFocusCapturedBySystem = _focusCapturedBySystem;
			if (!this.mFocusCapturedBySystem)
			{
				this.ControllerFocus = Interop.WVR_GetFocusedController ();
				PrintDebugLog ("onSystemFocusChanged() get focus controller " + this.ControllerFocus);
			}
		}
	}

	#region Monobehaviour overrides
	void OnEnable()
	{
		WaveVR_Utils.Event.Listen (WaveVR_Utils.Event.DEVICE_CONNECTED, onDeviceConnected);
		WaveVR_Utils.Event.Listen (WaveVR_Utils.Event.SYSTEMFOCUS_CHANGED, onSystemFocusChanged);
		WaveVR_Utils.Event.Listen (WaveVR_Utils.Event.OEM_CONFIG_CHANGED, onOEMConfigChanged);
		checkControllerConnected ();
	}

	void OnDisable()
	{
		WaveVR_Utils.Event.Remove (WaveVR_Utils.Event.DEVICE_CONNECTED, onDeviceConnected);
		WaveVR_Utils.Event.Remove (WaveVR_Utils.Event.SYSTEMFOCUS_CHANGED, onSystemFocusChanged);
		WaveVR_Utils.Event.Remove (WaveVR_Utils.Event.OEM_CONFIG_CHANGED, onOEMConfigChanged);
	}

	// Use this for initialization
	void Awake()
	{
		// should get current system focus device type
		this.ControllerFocus = Interop.WVR_GetFocusedController ();
		PrintDebugLog ("Start() Focus controller: " + this.ControllerFocus);
	}

	private void onOEMConfigChanged(params object[] args)
	{
		PrintDebugLog("onOEMConfigChanged");
		ReadJsonValues();
	}

	private void ReadJsonValues()
	{
		//EnableSingleBeam = true;
		string json_values = WaveVR_Utils.OEMConfig.getSingleBeamEnableConfig();

		if (!json_values.Equals(""))
		{
			try
			{
				SimpleJSON.JSONNode jsNodes = SimpleJSON.JSONNode.Parse(json_values);

				string node_value = "";
				node_value = jsNodes["enable"].Value;
				if (!node_value.Equals(""))
				{
					if (!node_value.Equals("true", System.StringComparison.OrdinalIgnoreCase)) // not true
					{
						EnableSingleBeam = false;
					}
				}
			}
			catch (Exception e)
			{
				Log.e(LOG_TAG, "JsonParse failed: " + e.ToString());
			}
		}

		PrintDebugLog("enable Single Beam: " + EnableSingleBeam);
	}

	void Start()
	{
		ReadJsonValues();
	}

	void OnDestroy()
	{
		PrintDebugLog("OnDestroy");
	}

	void OnApplicationPause(bool pauseStatus)
	{
		if (!pauseStatus) // resume
		{
			this.ControllerFocus = Interop.WVR_GetFocusedController();
			PrintDebugLog("Application resume, Focus controller: " + this.ControllerFocus);
			ReadJsonValues();
		}
	}

	// Update is called once per frame
	void Update () {
		if (this.mFocusCapturedBySystem)
			return;

		if (this.ctrInstanceList.Count < 1)
		{
			this.lastControllerFocus = WVR_DeviceType.WVR_DeviceType_Invalid;
			return;
		}

		if (Log.gpl.Print)
			PrintDebugLog ("Controller instance: " + ctrInstanceList.Count + ", Focus controller: " + this.ControllerFocus + ", enable single beam: " + EnableSingleBeam);

		if (this.ctrInstanceList.Count == 1)
		{
			ForceSetActiveOfEmitter (ctrInstanceList [0], true);
			ActivateEventSystem (ctrInstanceList [0].type, true);
		} else // count > 1
		{
			if (this.ControllerFocus == WVR_DeviceType.WVR_DeviceType_Controller_Right)
			{
				if (WaveVR_Controller.Input(WVR_DeviceType.WVR_DeviceType_Controller_Left).GetPressUp(WVR_InputId.WVR_InputId_Alias1_Digital_Trigger) ||
					WaveVR_Controller.Input(WVR_DeviceType.WVR_DeviceType_Controller_Left).GetPressUp(WVR_InputId.WVR_InputId_Alias1_Trigger))
				{
					this.ControllerFocus = WVR_DeviceType.WVR_DeviceType_Controller_Left;
					PrintDebugLog("Update() Controller focus changes from Right to Left, set to runtime.");
				}
			}
			if (this.ControllerFocus == WVR_DeviceType.WVR_DeviceType_Controller_Left)
			{
				// Listen to right
				if (WaveVR_Controller.Input(WVR_DeviceType.WVR_DeviceType_Controller_Right).GetPressUp(WVR_InputId.WVR_InputId_Alias1_Digital_Trigger) ||
					WaveVR_Controller.Input(WVR_DeviceType.WVR_DeviceType_Controller_Right).GetPressUp(WVR_InputId.WVR_InputId_Alias1_Trigger))
				{
					this.ControllerFocus = WVR_DeviceType.WVR_DeviceType_Controller_Right;
					PrintDebugLog("Update() Controller focus changes from Left to Right, set to runtime.");
				}
			}

			if (this.lastControllerFocus != this.ControllerFocus)
			{
				this.lastControllerFocus = this.ControllerFocus;
				PrintDebugLog("Update() focus set to: " + this.ControllerFocus);

				Interop.WVR_SetFocusedController(this.ControllerFocus);
			}

			SetActiveOfEmitter();
		}
	}
	#endregion

	private void printAllChildren(GameObject go)
	{
		var ch = go.transform.childCount;

		for (int i = 0; i < ch; i++)
		{
			GameObject child = go.transform.GetChild(i).gameObject;
			PrintDebugLog("-- " + child.name + " " + child.activeInHierarchy);

			printAllChildren(child);
		}
	}

	private void checkControllerConnected()
	{
		this.rConnected = WaveVR.Instance.getDeviceByType (WVR_DeviceType.WVR_DeviceType_Controller_Right).connected;
		this.lConnected = WaveVR.Instance.getDeviceByType (WVR_DeviceType.WVR_DeviceType_Controller_Left).connected;
		PrintDebugLog ("checkControllerConnected() rConnected: " + this.rConnected + ", lConnected: " + this.lConnected);
	}

	private void SetActiveOfEmitter()
	{
		//printAllChildren(ci.instance);
		foreach (ControllerInstance t in ctrInstanceList)
		{
			ForceSetActiveOfEmitter (t, ((t.type == this.ControllerFocus) || !EnableSingleBeam));
			ActivateEventSystem (t.type, ((t.type == this.ControllerFocus) || !EnableSingleBeam));
		}
	}

	private void ForceSetActiveOfEmitter(ControllerInstance ci, bool enabled)
	{
		GameObject _controller = ci.instance;
		if (_controller != null)
		{
			if (ci.showBeam != enabled)
			{
				WaveVR_Beam _beam = _controller.GetComponentInChildren<WaveVR_Beam> ();
				if (_beam != null)
				{
					ci.showBeam = enabled;
					_beam.ShowBeam = enabled;
					PrintDebugLog ("ForceSetActiveOfEmitter() Set " + ci.type + " controller " + _controller.name
					+ ", index: " + ci.index
					+ ", beam: " + _beam.ShowBeam);
				}
			}

			if (ci.showPointer != enabled)
			{
				WaveVR_ControllerPointer _pointer = _controller.GetComponentInChildren<WaveVR_ControllerPointer> ();
				if (_pointer != null)
				{
					ci.showPointer = enabled;
					_pointer.ShowPointer = enabled;
					PrintDebugLog ("ForceSetActiveOfEmitter() Set " + ci.type + " controller " + _controller.name
					+ ", index: " + ci.index
					+ ", pointer: " + _pointer.ShowPointer);
				}
			}
		} else
		{
			if (Log.gpl.Print)
				PrintDebugLog("ForceSetActiveOfEmitter() controller " + ci.type + " , index: " + ci.index + " controller is null, remove it from list.");
			removeControllerInstance(ci.index);
		}
	}

	private void ActivateEventSystem(WVR_DeviceType type, bool enabled)
	{
		if (EventSystem.current == null)
		{
			EventSystem _es = FindObjectOfType<EventSystem>();
			if (_es != null)
			{
				eventSystem = _es.gameObject;
			}
		}
		else
		{
			eventSystem = EventSystem.current.gameObject;
		}

		if (eventSystem != null)
		{
			WaveVR_ControllerInputModule wcim = eventSystem.GetComponent<WaveVR_ControllerInputModule>();

			if (wcim != null)
			{
				if (type == WVR_DeviceType.WVR_DeviceType_Controller_Right)
				{
					if (wcim.DomintEventEnabled != enabled)
					{
						wcim.DomintEventEnabled = enabled;
						PrintDebugLog ("Forced set DomintEventEnabled to " + wcim.DomintEventEnabled);
					}
				}
				else if (type == WVR_DeviceType.WVR_DeviceType_Controller_Left)
				{
					if (wcim.NoDomtEventEnabled != enabled)
					{
						wcim.NoDomtEventEnabled = enabled;
						PrintDebugLog ("Forced set NoDomtEventEnabled to " + wcim.NoDomtEventEnabled);
					}
				}
			}
		}
	}
}
