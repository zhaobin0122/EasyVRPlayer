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
using UnityEngine.Assertions;
using wvr;
using WVR_Log;
using System;
using System.Runtime.InteropServices;
using UnityEngine.Profiling;
using wvr.TypeExtensions;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class WaveVR : System.IDisposable
{
	[SerializeField]
	public bool editor = false;
#if UNITY_EDITOR && UNITY_ANDROID
	public static bool EnableSimulator = false;
#endif

	private static string LOG_TAG = "WVR_WaveVR";
	private void PrintDebugLog(string msg)
	{
		if (Log.EnableDebugLog)
			Log.d (LOG_TAG, msg, true);
	}

	private void PrintInfoLog(string msg)
	{
		Log.i (LOG_TAG, msg, true);
	}

	private void PrintErrorLog(string msg)
	{
		Log.e (LOG_TAG, msg, true);
	}

	public static WaveVR Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new WaveVR ();
			}
			return instance;
		}
	}
	private static WaveVR instance = null;
	public bool Initialized = false;

	private ulong supportedFeatures = 0;
	public bool FocusCapturedBySystem = false;
	private bool handTrackingEnabled = false;
	private bool handGestureEnabled = false;
	public WVR_InteractionMode InteractionMode = WVR_InteractionMode.WVR_InteractionMode_SystemDefault;
	public WVR_GazeTriggerType GazeTriggerType = WVR_GazeTriggerType.WVR_GazeTriggerType_Timeout;

	public class Device
	{
		public Device(WVR_DeviceType type)
		{
			this.type = type;
			for (int i = 0; i < DeviceTypes.Length; i++)
			{
				if (DeviceTypes[i] == type)
				{
					index = i;
					break;
				}
			}
		}
		public WVR_DeviceType type { get; private set; }
		public int index { get; private set; }
		public bool connected { get { return instance.isValidPose[index]; } }
		public WVR_DevicePosePair_t pose { get { return instance.poses[instance.deviceIndexMap[index]]; } }
		public WaveVR_Utils.RigidTransform rigidTransform { get { return instance.rtPoses[instance.deviceIndexMap[index]]; } }
	}

	public Device hmd { get; private set; }
	public Device controllerLeft { get; private set; }
	public Device controllerRight { get; private set; }

	public Device getDeviceByType(WVR_DeviceType type)
	{
		switch (type)
		{
		case WVR_DeviceType.WVR_DeviceType_HMD:
			return hmd;
		case WVR_DeviceType.WVR_DeviceType_Controller_Right:
			return WaveVR_Controller.IsLeftHanded ? controllerLeft : controllerRight;
		case WVR_DeviceType.WVR_DeviceType_Controller_Left:
			return WaveVR_Controller.IsLeftHanded ? controllerRight : controllerLeft;
		default:
			Assert.raiseExceptions = true;
			return hmd;  // Should not happen
		}
	}

	public Device getDeviceByType(WaveVR_Controller.EDeviceType type)
	{
		if (type == WaveVR_Controller.EDeviceType.Head)
			return hmd;
		if (type == WaveVR_Controller.EDeviceType.Dominant)
			return WaveVR_Controller.IsLeftHanded ? controllerLeft : controllerRight;
		if (type == WaveVR_Controller.EDeviceType.NonDominant)
			return WaveVR_Controller.IsLeftHanded ? controllerRight : controllerLeft;
		return null;
	}

	private void ReportError(WVR_InitError error)
	{
		switch (error)
		{
		case WVR_InitError.WVR_InitError_None:
			break;
		case WVR_InitError.WVR_InitError_NotInitialized:
			PrintErrorLog ("WaveVR: Not initialized");
			Application.Quit ();
			break;
		case WVR_InitError.WVR_InitError_Unknown:
			PrintErrorLog ("WaveVR: Unknown error during initializing");
			break;
		default:
			//TODO PrintErrorLog (Interop.WVR_GetErrorString(error));
			break;
		}
	}

	[System.Obsolete("Please check WaveVR.Instance directly")]
	public static bool Hmd
	{
		get
		{
			return Instance != null;
		}
	}

	public static WVR_DeviceType[] DeviceTypes = new WVR_DeviceType[]{
		WVR_DeviceType.WVR_DeviceType_HMD,
		WVR_DeviceType.WVR_DeviceType_Controller_Right,
		WVR_DeviceType.WVR_DeviceType_Controller_Left
	};

	private bool[] isValidPose = new bool[DeviceTypes.Length];
	private uint[] deviceIndexMap = new uint[DeviceTypes.Length];  // Mapping from DeviceTypes's index to poses's index
	public uint frameInx = 0;
	private WVR_DevicePosePair_t[] poses = new WVR_DevicePosePair_t[DeviceTypes.Length];  // HMD, R, L controllers.
	private WaveVR_Utils.RigidTransform[] rtPoses = new WaveVR_Utils.RigidTransform[DeviceTypes.Length];

	public static WaveVR_Render.StereoRenderingPath UnityPlayerSettingsStereoRenderingPath = WaveVR_Render.StereoRenderingPath.SinglePass;

	private WaveVR()
	{
		PrintInfoLog ("WaveVR()+ commit: " + WaveVR_COMMITINFO.wavevr_version);
		{
			WVR_InitError error = Interop.WVR_Init(WVR_AppType.WVR_AppType_VRContent);
			if (error != WVR_InitError.WVR_InitError_None)
			{
				ReportError(error);
				Interop.WVR_Quit();
				this.Initialized = false;
				PrintErrorLog ("WaveVR() initialize simulator failed, WVR_Quit()");
				return;
			}
			WaveVR_Utils.notifyActivityUnityStarted();
		}

		this.Initialized = true;
		PrintInfoLog ("WaveVR() initialization succeeded.");
#if !UNITY_EDITOR
		UnityPlayerSettingsStereoRenderingPath = WaveVR_Render.IsVRSinglePassBuildTimeSupported()? WaveVR_Render.StereoRenderingPath.SinglePass: WaveVR_Render.StereoRenderingPath.MultiPass;
#else
		UnityPlayerSettingsStereoRenderingPath = (WaveVR_Render.StereoRenderingPath)PlayerSettings.stereoRenderingPath;
#endif
		PrintInfoLog ("UnityPlayerSettingsStereoRenderingPath = " + UnityPlayerSettingsStereoRenderingPath);


		for (int i = 0; i < 3; i++)
		{
			poses[i] = new WVR_DevicePosePair_t();
			isValidPose[i] = false; // force update connection status to all listener.
			deviceIndexMap[i] = 0;  // use hmd's id as default.
		}

		hmd = new Device(WVR_DeviceType.WVR_DeviceType_HMD);
		controllerLeft = new Device(WVR_DeviceType.WVR_DeviceType_Controller_Left);
		controllerRight = new Device(WVR_DeviceType.WVR_DeviceType_Controller_Right);

		// Check left-handed mode first, then set connection status according to left-handed mode.
		SetLeftHandedMode ();
		UpdateAllConnection ();
		SetDefaultButtons ();

		supportedFeatures = Interop.WVR_GetSupportedFeatures ();
		PrintInfoLog ("WaveVR() supportedFeatures: " + supportedFeatures);

		PrintInfoLog ("WaveVR()-");
	}

	~WaveVR()
	{
		Dispose();
	}

	public void onLoadLevel()
	{
		if (!this.Initialized)
			return;

		PrintInfoLog ("onLoadLevel() reset all connection");
		for (int i = 0; i < DeviceTypes.Length; i++)
		{
			poses[i] = new WVR_DevicePosePair_t();
			isValidPose [i] = false;	// force update connection status to all listener.
		}
	}

	public void Dispose()
	{
		if (!this.Initialized)
			return;


		// ---------------------------------- Gesture begins ----------------------------------
		PrintInfoLog("Stop Hand Gesture before WVR_Quit.");
		StopHandGesture ();
		PrintInfoLog("Stop Hand Tracking before WVR_Quit.");
		StopHandTracking ();
		// ---------------------------------- Gesture ends ----------------------------------


		Interop.WVR_Quit();

		PrintInfoLog ("WVR_Quit");
		instance = null;
		Initialized = false;
		System.GC.SuppressFinalize(this);
	}

	// Use this interface to avoid accidentally creating the instance 
	// in the process of attempting to dispose of it.
	public static void SafeDispose()
	{
		if (instance != null)
			instance.Dispose();
	}

	// Use this interface to check what kind of dof is running
	public int is6DoFTracking()
	{
		if (!this.Initialized)
			return 0;

		WVR_NumDoF dof = Interop.WVR_GetDegreeOfFreedom(WVR_DeviceType.WVR_DeviceType_HMD);

		if (dof == WVR_NumDoF.WVR_NumDoF_6DoF)
			return 6;  // 6 DoF
		else if (dof == WVR_NumDoF.WVR_NumDoF_3DoF)
			return 3;  // 3 DoF
		else
			return 0;  // abnormal case
	}

	public void UpdateEachFrame(WVR_PoseOriginModel origin)
	{
		if (!this.Initialized)
			return;

		UpdateEachFrame (origin, false);
	}

	public void UpdateEachFrame(WVR_PoseOriginModel origin, bool isSimulator)
	{
		if (!this.Initialized)
			return;

		if (Log.gpl.Print)
			PrintDebugLog ("UpdateEachFrame()");

		bool _focusCapturedBySystem = Interop.WVR_IsInputFocusCapturedBySystem ();
		if (this.FocusCapturedBySystem != _focusCapturedBySystem)
		{
			this.FocusCapturedBySystem = _focusCapturedBySystem;
			WaveVR_Utils.Event.Send (WaveVR_Utils.Event.SYSTEMFOCUS_CHANGED, this.FocusCapturedBySystem);

			// When getting system focus again, reset button events.
			if (!this.FocusCapturedBySystem)
			{
				PrintInfoLog ("UpdateEachFrame() get system focus, update button events.");
				UpdateButtonEvents ();
			} else
			{
				PrintInfoLog ("UpdateEachFrame() lost system focus.");
			}
		}

		InteractionMode = Interop.WVR_GetInteractionMode ();
		GazeTriggerType = Interop.WVR_GetGazeTriggerType ();

		Profiler.BeginSample("GetSyncPose");
#if UNITY_STANDALONE
		Interop.WVR_GetLastPoseIndex(origin, poses, (uint)poses.Length, ref frameInx);
#else
		Interop.WVR_GetSyncPose (origin, poses, (uint)poses.Length);
#endif
		Profiler.EndSample();

		for (uint i = 0; i < DeviceTypes.Length; i++)
		{
			bool _hasType = false;

			for (uint j = 0; j < poses.Length; j++)
			{
				WVR_DevicePosePair_t _pose = poses[j];

				if (_pose.type == DeviceTypes [i])
				{
					_hasType = true;

					// Check connection first
					bool _valid_pose = GetConnectionStatus (DeviceTypes [i]);
					// Check whether pose is valid with connection.
					if (_valid_pose)
						_valid_pose = _pose.pose.IsValidPose;

					deviceIndexMap[i] = j;

					if (isValidPose [i] != _valid_pose)
					{
						isValidPose [i] = _valid_pose;
						PrintInfoLog (Log.CSB.Append("UpdateEachFrame() device ").Append(DeviceTypes [i].Name()).Append(" pose is ").Append(isValidPose [i] ? "valid." : "invalid.").ToString());
						WaveVR_Utils.Event.Send (WaveVR_Utils.Event.DEVICE_CONNECTED, DeviceTypes [i], isValidPose [i]);
					}

					if (isValidPose [i])
					{
						rtPoses[j].update(_pose.pose.PoseMatrix);
					}

					break;
				}
			}

			// no such type
			if (!_hasType)
			{
				if (isValidPose [i] == true)
				{
					isValidPose [i] = false;
					PrintInfoLog (Log.CSB.Append("UpdateEachFrame() device ").Append(DeviceTypes [i].Name()).Append(" pose is invalid.").ToString());
					WaveVR_Utils.Event.Send (WaveVR_Utils.Event.DEVICE_CONNECTED, DeviceTypes [i], isValidPose [i]);
				}
			}
		}

		for (int i = 0; i < poses.Length; i++)
		{
			WVR_DeviceType _type = poses [i].type;
			bool _connected = GetConnectionStatus (_type);
			bool _poseValid = poses [i].pose.IsValidPose;

			if (Log.gpl.Print)
			{
				PrintDebugLog (
					Log.CSB
					.Append("UpdateEachFrame() device ").Append(_type.Name()).Append(" is ").Append(_connected ? "connected" : "disconnected")
					.Append(", pose is ").Append(_poseValid ? "valid" : "invalid")
					.Append(", pos: {").Append(rtPoses [i].pos.x).Append(", ").Append(rtPoses [i].pos.y).Append(", ").Append(rtPoses [i].pos.z).Append("}")
					.Append(", rot: {").Append(rtPoses [i].rot.x).Append(", ").Append(rtPoses [i].rot.y).Append(", ").Append(rtPoses [i].rot.z).Append(", ").Append(rtPoses [i].rot.w).Append("}")
					.ToString());
			}
		}

		Profiler.BeginSample("SendNewPose");
		try
		{
			WaveVR_Utils.Event.Send(WaveVR_Utils.Event.NEW_POSES, poses, rtPoses);
		}
		catch (Exception ex)
		{
			PrintErrorLog ("Send NEW_POSES Event Exception : " + ex);
		}

		if (Log.gpl.Print)
			PrintDebugLog ("UpdateEachFrame() after new pose.");

		try
		{
			WaveVR_Utils.Event.Send(WaveVR_Utils.Event.AFTER_NEW_POSES);
		}
		catch (Exception ex)
		{
			PrintErrorLog ("Send AFTER_NEW_POSES Event Exception : " + ex);
		}
		Profiler.EndSample();
	}

	public int SetQualityLevel(int level, bool applyExpensiveChanges = true)
	{
		if (!this.Initialized)
			return -1;

		return WaveVR_Render.Instance.SetQualityLevel(level, applyExpensiveChanges);
	}

	#region WaveVR_Controller status
	public bool SetLeftHandedMode(bool leftHandedInEditor = false)
	{
		if (!this.Initialized)
			return false;

		bool _changed = false, _lefthanded = false;
#if UNITY_EDITOR && UNITY_ANDROID
		if (Application.isEditor)
		{
			_lefthanded = leftHandedInEditor;
		} else
		#endif
		{
			_lefthanded = Interop.WVR_GetDefaultControllerRole () == WVR_DeviceType.WVR_DeviceType_Controller_Left ? true : false;
		}

		if (WaveVR_Controller.IsLeftHanded != _lefthanded)
		{
			_changed = true;

			Log.i(LOG_TAG, Log.CSB.Append("SetLeftHandedMode() Set left-handed mode to ").Append(_lefthanded).ToString());
			WaveVR_Controller.SetLeftHandedMode (_lefthanded);
		}
		else
		{
			Log.i(LOG_TAG, Log.CSB.Append("SetLeftHandedMode() not change default role: ").Append(_lefthanded ? "LEFT." : "RIGHT.").ToString());
		}

		return _changed;
	}

	private bool GetConnectionStatus(WVR_DeviceType type)
	{
		if (type == WVR_DeviceType.WVR_DeviceType_Invalid)
			return false;

		WVR_DeviceType _type = WaveVR_Controller.Input (type).DeviceType;
		return WaveVR_Controller.Input (_type).connected;
	}

	public void SetConnectionStatus(WVR_DeviceType type, bool conn)
	{
		if (!this.Initialized)
			return;

		if (type == WVR_DeviceType.WVR_DeviceType_Invalid)
			return;

		PrintInfoLog ("SetConnectionStatus() " + type + " is " + (conn ? "connected." : "disconnected."));
		WVR_DeviceType _type = WaveVR_Controller.Input (type).DeviceType;
		WaveVR_Controller.Input (_type).connected = conn;
	}

	public void UpdateAllConnection()
	{
		if (!this.Initialized)
			return;

		for (int i = 0; i < DeviceTypes.Length; i++)
			SetConnectionStatus (DeviceTypes [i], IsDeviceConnected (DeviceTypes [i]));

		UpdateAllPoseState ();
	}

	private void UpdateAllPoseState()
	{
		for (int i = 0; i < DeviceTypes.Length; i++)
		{
			if (GetConnectionStatus (DeviceTypes [i]) == false)
			{
				isValidPose [i] = false;
				PrintInfoLog ("UpdateAllPoseState() " + DeviceTypes [i] + " pose is " + (isValidPose [i] ? "valid." : "invalid."));
				WaveVR_Utils.Event.Send (WaveVR_Utils.Event.DEVICE_CONNECTED, DeviceTypes [i], isValidPose [i]);
			}
		}
	}

	public void UpdateButtonEvents()
	{
		if (!this.Initialized)
			return;

		PrintInfoLog ("ResetButtonEvents() Reset button events.");
		for (int i = 0; i < WaveVR.DeviceTypes.Length; i++)
		{
			WaveVR_Controller.Input (WaveVR.DeviceTypes [i]).UpdateButtonEvents ();
		}
	}

	public void ResetAllButtonStates()
	{
		PrintInfoLog ("ResetAllButtonStates() Reset button states.");
		for (int i = 0; i < WaveVR.DeviceTypes.Length; i++)
		{
			WaveVR_Controller.Input (WaveVR.DeviceTypes [i]).ResetAllButtonStates ();
		}
	}

	public void SetDefaultButtons()
	{
		if (!this.Initialized)
			return;

		PrintInfoLog ("SetDefaultButtons()");

		WVR_InputAttribute_t[] inputAttribtues_hmd = new WVR_InputAttribute_t[1];
		inputAttribtues_hmd [0].id = WVR_InputId.WVR_InputId_Alias1_Enter;
		inputAttribtues_hmd [0].capability = (uint)WVR_InputType.WVR_InputType_Button;
		inputAttribtues_hmd [0].axis_type = WVR_AnalogType.WVR_AnalogType_None;

		WVR_DeviceType _type = WaveVR_Controller.Input (WaveVR_Controller.EDeviceType.Head).DeviceType;
		bool _ret = Interop.WVR_SetInputRequest (_type, inputAttribtues_hmd, (uint)inputAttribtues_hmd.Length);
		if (_ret)
		{
			uint inputTableSize = (uint)WVR_InputId.WVR_InputId_Max;
			WVR_InputMappingPair_t[] inputTable = new WVR_InputMappingPair_t[inputTableSize];
			uint _size = Interop.WVR_GetInputMappingTable (_type, inputTable, inputTableSize);
			if (_size > 0)
			{
				for (int _i = 0; _i < (int)_size; _i++)
				{
					Log.d(LOG_TAG, Log.CSB
						.Append("SetDefaultButtons() ")
						.Append(_type.Name())
						.Append(" button: ")
						.Append(inputTable[_i].source.id)
						.Append(" is mapping to HMD input ID: ")
						.Append(inputTable[_i].destination.id)
						.ToString());
				}
			}
		}
	}
	#endregion

	public void SetPoseSimulation(WVR_SimulationType type)
	{
		if (!this.Initialized)
			return;

		PrintInfoLog ("SetPoseSimulation() " + type);
		Interop.WVR_SetArmModel (type);
	}

	public void SetPoseSimulationFollowHead(bool follow)
	{
		if (!this.Initialized)
			return;

		PrintInfoLog ("SetPoseSimulationFollowHead ()" + follow);
		Interop.WVR_SetArmSticky (follow);
	}

	public void SetNeckModelEnabled(bool enabled)
	{
		if (!this.Initialized)
			return;

		PrintInfoLog ("SetNeckModelEnabled() " + enabled);
		Interop.WVR_SetNeckModelEnabled (enabled);
	}

	#region Gesture
	public bool StartHandGesture()
	{
		if (this.Initialized && !handGestureEnabled)
		{
			ulong feature = GetSupportedFeatures ();
			PrintInfoLog ("StartHandGesture() supported feature: " + feature);
			if ((feature & (ulong)WVR_SupportedFeature.WVR_SupportedFeature_HandGesture) == 0)
				return false;

			WVR_Result result = Interop.WVR_StartHandGesture ();
			handGestureEnabled = result == WVR_Result.WVR_Success ? true : false;
			PrintInfoLog ("StartHandGesture() " + result);
		}

		return handGestureEnabled;
	}

	public void StopHandGesture()
	{
		if (this.Initialized && handGestureEnabled)
		{
			PrintInfoLog ("StopHandGesture() Starts.");
			Interop.WVR_StopHandGesture ();
			PrintInfoLog ("StopHandGesture() Ends.");
			handGestureEnabled = false;
		}
	}

	public bool IsHandGestureEnabled()
	{
		return handGestureEnabled;
	}

	public bool GetHandGestureData(ref WVR_HandGestureData_t data)
	{
		bool hasHandGestureData = false;

		if (handGestureEnabled)
			hasHandGestureData = Interop.WVR_GetHandGestureData (ref data) == WVR_Result.WVR_Success ? true : false;

		return hasHandGestureData;
	}

	public bool StartHandTracking()
	{
		if (this.Initialized && !handTrackingEnabled)
		{
			ulong feature = GetSupportedFeatures ();
			PrintInfoLog ("StartHandTracking() supported feature: " + feature);
			if ((feature & (ulong)WVR_SupportedFeature.WVR_SupportedFeature_HandTracking) == 0)
				return false;

			WVR_Result result = Interop.WVR_StartHandTracking ();
			handTrackingEnabled = result == WVR_Result.WVR_Success ? true : false;
			PrintInfoLog ("StartHandTracking() " + result);
		}

		return handTrackingEnabled;
	}

	public void StopHandTracking()
	{
		if (this.Initialized && handTrackingEnabled)
		{
			PrintInfoLog ("StopHandTracking() Starts.");
			Interop.WVR_StopHandTracking ();
			PrintInfoLog ("StopHandTracking() Ends.");
			handTrackingEnabled = false;
		}
	}

	public bool IsHandTrackingEnabled()
	{
		return handTrackingEnabled;
	}

	public bool GetHandTrackingData(ref WVR_HandTrackingData_t data, WVR_PoseOriginModel originModel, uint predictedMilliSec)
	{
		bool hasHandTrackingData = false;

		if (handTrackingEnabled)
			hasHandTrackingData = Interop.WVR_GetHandTrackingData (ref data, originModel, predictedMilliSec) == WVR_Result.WVR_Success ? true : false;

		return hasHandTrackingData;
	}
	#endregion

	public ulong GetSupportedFeatures()
	{
		return supportedFeatures;
	}

	private bool IsDeviceConnected(WVR_DeviceType device)
	{
		if (!this.Initialized)
			return false;

		return Interop.WVR_IsDeviceConnected (device);
	}
}
