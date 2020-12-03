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
using UnityEngine.Profiling;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(WaveVR_PoseTrackerManager))]
public class WaveVR_PoseTrackerManagerEditor : Editor
{
	public override void OnInspectorGUI()
	{
		WaveVR_PoseTrackerManager myScript = target as WaveVR_PoseTrackerManager;

		myScript.Type = (WaveVR_Controller.EDeviceType)EditorGUILayout.EnumPopup ("Type", myScript.Type);
		myScript.TrackPosition = EditorGUILayout.Toggle ("Track Position", myScript.TrackPosition);
		if (true == myScript.TrackPosition)
		{
			if (myScript.Type == WaveVR_Controller.EDeviceType.Head)
			{
				myScript.EnableNeckModel = (bool)EditorGUILayout.Toggle ("    Enable Neck Model", myScript.EnableNeckModel);
			} else
			{
				myScript.SimulationOption = (WVR_SimulationOption)EditorGUILayout.EnumPopup ("    Simulate Position", myScript.SimulationOption);
				if (myScript.SimulationOption == WVR_SimulationOption.ForceSimulation || myScript.SimulationOption == WVR_SimulationOption.WhenNoPosition)
				{
					myScript.FollowHead = (bool)EditorGUILayout.Toggle ("        Follow Head", myScript.FollowHead);
				}
			}
		}

		myScript.TrackRotation = EditorGUILayout.Toggle ("Track Rotation", myScript.TrackRotation);
		myScript.TrackTiming = (WVR_TrackTiming)EditorGUILayout.EnumPopup ("Track Timing", myScript.TrackTiming);

		if (GUI.changed)
			EditorUtility.SetDirty ((WaveVR_PoseTrackerManager)target);
	}
}
#endif

public enum WVR_TrackTiming {
	WhenUpdate,  // Pose will delay one frame.
	WhenNewPoses
};

public enum WVR_SimulationOption
{
	WhenNoPosition = 0,
	ForceSimulation = 1,
	NoSimulation = 2
};

public class WaveVR_PoseTrackerManager : MonoBehaviour
{
	private const string LOG_TAG = "WaveVR_PoseTrackerManager";
	private void PrintDebugLog(string msg)
	{
		if (Log.EnableDebugLog)
			Log.d (LOG_TAG, msg, true);
	}

	public WaveVR_Controller.EDeviceType Type = WaveVR_Controller.EDeviceType.Dominant;
	public bool TrackPosition = true;
	public bool EnableNeckModel = true;
	public WVR_SimulationOption SimulationOption = WVR_SimulationOption.WhenNoPosition;
	public bool FollowHead = false;
	public bool TrackRotation = true;
	public WVR_TrackTiming TrackTiming = WVR_TrackTiming.WhenNewPoses;

	private List<GameObject> IncludedObjects = new List<GameObject>();
	private List<bool> IncludedStates = new List<bool>();

	/// <summary>
	/// We use 4 variables to determine whether to hide object with pose tracker or not.
	/// There are 2 kinds of pose tracked object:
	/// 1. Normal object
	/// 2. Controller
	/// 1 is shown when connected && has system focus && pose updated.
	/// 2 is shown when connected && has system focus && pose updated && not Gaze mode.
	/// And for 2. Controller, if it is NOT focused controller, only model will be shown, beam & pointer will be hidden.
	/// </summary>
	private bool showTrackedObject = true;
	private bool connected = false;
	private bool mFocusCapturedBySystem = false;
	public bool poseUpdated = false;
	private bool hasNewPose = false;
	private bool gazeOnly = false;

	private WaveVR_DevicePoseTracker devicePoseTracker = null;
	private WaveVR_ControllerPoseTracker ctrlerPoseTracker = null;

	private bool ptmEnabled = false;

	#region Monobehaviour overrides
	void OnEnable()
	{
		if (!ptmEnabled)
		{
			IncludedObjects.Clear ();
			IncludedStates.Clear ();
			int _children_count = transform.childCount;
			for (int i = 0; i < _children_count; i++)
			{
				IncludedObjects.Add (transform.GetChild (i).gameObject);
				IncludedStates.Add (transform.GetChild (i).gameObject.activeSelf);
				PrintDebugLog ("OnEnable() " + this.Type + ", " + gameObject.name + " has child: " + IncludedObjects [i].name + ", active? " + IncludedStates [i]);
			}

			// If pose is invalid, considering as disconnected and not show controller.
			WaveVR.Device _device = WaveVR.Instance.getDeviceByType (Type);
			this.connected = _device.connected;

			// Always hide Pose Tracker objects when enabled.
			// Check whether to show object when:
			// 1. device connected
			// 2. pose updated
			// 3. system focus changed
			// 4. interaction mode changed
			ForceActivateTargetObjects (false);
			this.poseUpdated = false;
			this.hasNewPose = false;

			WaveVR_Utils.Event.Listen (WaveVR_Utils.Event.DEVICE_CONNECTED, onDeviceConnected);
			WaveVR_Utils.Event.Listen (WaveVR_Utils.Event.NEW_POSES, OnNewPoses);
			WaveVR_Utils.Event.Listen (WaveVR_Utils.Event.SYSTEMFOCUS_CHANGED, onSystemFocusChanged);

			ptmEnabled = true;
		}
	}

	void Awake()
	{
		if (TrackPosition == false)
		{
			SimulationOption = WVR_SimulationOption.NoSimulation;
			FollowHead = false;
		}

		gameObject.SetActive (false);
		PrintDebugLog ("Awake() " + this.Type
			+ ", TrackPosition: " + TrackPosition
			+ ", SimulationOption: " + SimulationOption
			+ ", FollowHead: " + FollowHead
			+ ", TrackRotation: " + TrackRotation
			+ ", TrackTiming: " + TrackTiming);

		WaveVR_PointerCameraTracker pcTracker = gameObject.GetComponent<WaveVR_PointerCameraTracker>();

		if (pcTracker == null)
		{
			if (this.Type == WaveVR_Controller.EDeviceType.Head)
			{
				PrintDebugLog ("Awake() " + this.Type + ", load WaveVR_DevicePoseTracker.");
				devicePoseTracker = (WaveVR_DevicePoseTracker)gameObject.AddComponent<WaveVR_DevicePoseTracker> ();
				if (null != devicePoseTracker)
				{
					devicePoseTracker.type = Type;
					devicePoseTracker.trackPosition = TrackPosition;
					devicePoseTracker.EnableNeckModel = this.EnableNeckModel;
					devicePoseTracker.trackRotation = TrackRotation;
					devicePoseTracker.timing = TrackTiming;
				}
			} else
			{
				PrintDebugLog ("Awake() " + this.Type + ", load WaveVR_ControllerPoseTracker.");
				ctrlerPoseTracker = (WaveVR_ControllerPoseTracker)gameObject.AddComponent<WaveVR_ControllerPoseTracker> ();
				if (null != ctrlerPoseTracker)
				{
					ctrlerPoseTracker.Type = Type;
					ctrlerPoseTracker.TrackPosition = TrackPosition;
					ctrlerPoseTracker.SimulationOption = SimulationOption;
					ctrlerPoseTracker.FollowHead = FollowHead;
					ctrlerPoseTracker.TrackRotation = TrackRotation;
					ctrlerPoseTracker.TrackTiming = TrackTiming;
				}
			}
		}
		gameObject.SetActive (true);
	}

	void Update()
	{
		if (!Application.isEditor)
		{
			if (Log.gpl.Print)
			{
				if (this.Type == WaveVR_Controller.EDeviceType.Head)
				{
					PrintDebugLog (showTrackedObject ? "Update() Head, showTrackedObject is true." : "Update() Head, showTrackedObject is false.");
				}
				if (this.Type == WaveVR_Controller.EDeviceType.Dominant)
				{
					PrintDebugLog (showTrackedObject ? "Update() Dominant, showTrackedObject is true." : "Update() Dominant, showTrackedObject is false.");
				}
				if (this.Type == WaveVR_Controller.EDeviceType.NonDominant)
				{
					PrintDebugLog (showTrackedObject ? "Update() NonDominant, showTrackedObject is true." : "Update() NonDominant, showTrackedObject is false.");
				}

				for (int i = 0; i < IncludedObjects.Count; i++)
				{
					if (IncludedObjects [i] != null)
						PrintDebugLog ("Update() " + this.Type + ", Update() GameObject " + IncludedObjects [i].name + " is " + (IncludedObjects [i].activeSelf ? "active." : "inactive."));
				}
			}
		}

		if (!this.connected)
			return;

		ActivateTargetObjects ();

		// Update after the frame receiving 1st new pose.
		this.poseUpdated = this.hasNewPose;
	}

	void OnDisable()
	{
		if (ptmEnabled)
		{
			// Consider a situation: no pose is updated and WaveVR_PoseTrackerManager is enabled <-> disabled multiple times.
			// At this situation, IncludedStates will be set to false forever since thay are deactivated at 1st time OnEnable()
			// and the deactivated state will be updated to IncludedStates in 2nd time OnEnable().
			// To prevent this situation, activate IncludedObjects in OnDisable to restore the state Children GameObjects.
			PrintDebugLog ("OnDisable() " + this.Type + ", restore children objects.");
			ForceActivateTargetObjects (true);

			WaveVR_Utils.Event.Remove (WaveVR_Utils.Event.DEVICE_CONNECTED, onDeviceConnected);
			WaveVR_Utils.Event.Remove (WaveVR_Utils.Event.NEW_POSES, OnNewPoses);
			WaveVR_Utils.Event.Remove (WaveVR_Utils.Event.SYSTEMFOCUS_CHANGED, onSystemFocusChanged);

			ptmEnabled = false;
		}
	}

	void OnDestroy()
	{
		IncludedObjects.Clear ();
		IncludedStates.Clear ();
	}
	#endregion

	private bool hideEventController()
	{
		bool _hide = false;

		WVR_InteractionMode _imode = WaveVR.Instance.InteractionMode;
		if (WaveVR_InputModuleManager.Instance != null)
			_imode = WaveVR_InputModuleManager.Instance.GetInteractionMode ();

		this.gazeOnly = _imode == WVR_InteractionMode.WVR_InteractionMode_Gaze ? true : false;

		GameObject _model = WaveVR_EventSystemControllerProvider.Instance.GetControllerModel (this.Type);
		if (GameObject.ReferenceEquals (gameObject, _model))
		{
			_hide = this.gazeOnly;
		}

		return _hide;
	}

	public void ActivateTargetObjects()
	{
		bool _hide = hideEventController ();
		bool _has_input_module_enabled = true;
		if (WaveVR_InputModuleManager.Instance != null)
			_has_input_module_enabled = WaveVR_InputModuleManager.Instance.EnableInputModule;

		bool _active = (
			(this.connected == true)								// controller is connected (pose is valid).
			&& (!this.mFocusCapturedBySystem)						// scene has system focus.
			&& this.poseUpdated										// already has pose.
			&& (!_hide)												// not event controller or controller in Gaze
			&& _has_input_module_enabled							// has InputModuleManager and enabled
		);

		if (this.showTrackedObject == _active)
			return;

		if (this.Type == WaveVR_Controller.EDeviceType.Head)
		{
			PrintDebugLog (connected ? "ActivateTargetObjects() Head, connected is true." : "ActivateTargetObjects() Head, connected is false.");
			PrintDebugLog (mFocusCapturedBySystem ? "ActivateTargetObjects() Head, mFocusCapturedBySystem is true." : "ActivateTargetObjects() Head, mFocusCapturedBySystem is false.");
			PrintDebugLog (gazeOnly ? "ActivateTargetObjects() Head, gazeOnly is true." : "ActivateTargetObjects() Head, gazeOnly is false.");
			PrintDebugLog (_has_input_module_enabled ? "ActivateTargetObjects() Head, _has_input_module_enabled is true." : "ActivateTargetObjects() Head, _has_input_module_enabled is false.");
		}
		if (this.Type == WaveVR_Controller.EDeviceType.Dominant)
		{
			PrintDebugLog (connected ? "ActivateTargetObjects() Dominant, connected is true." : "ActivateTargetObjects() Dominant, connected is false.");
			PrintDebugLog (mFocusCapturedBySystem ? "ActivateTargetObjects() Dominant, mFocusCapturedBySystem is true." : "ActivateTargetObjects() Dominant, mFocusCapturedBySystem is false.");
			PrintDebugLog (gazeOnly ? "ActivateTargetObjects() Dominant, gazeOnly is true." : "ActivateTargetObjects() Dominant, gazeOnly is false.");
			PrintDebugLog (_has_input_module_enabled ? "ActivateTargetObjects() Dominant, _has_input_module_enabled is true." : "ActivateTargetObjects() Dominant, _has_input_module_enabled is false.");
		}
		if (this.Type == WaveVR_Controller.EDeviceType.NonDominant)
		{
			PrintDebugLog (connected ? "ActivateTargetObjects() NonDominant, connected is true." : "ActivateTargetObjects() NonDominant, connected is false.");
			PrintDebugLog (mFocusCapturedBySystem ? "ActivateTargetObjects() NonDominant, mFocusCapturedBySystem is true." : "ActivateTargetObjects() NonDominant, mFocusCapturedBySystem is false.");
			PrintDebugLog (gazeOnly ? "ActivateTargetObjects() NonDominant, gazeOnly is true." : "ActivateTargetObjects() NonDominant, gazeOnly is false.");
			PrintDebugLog (_has_input_module_enabled ? "ActivateTargetObjects() NonDominant, _has_input_module_enabled is true." : "ActivateTargetObjects() NonDominant, _has_input_module_enabled is false.");
		}

		ForceActivateTargetObjects (_active);
	}

	private void ForceActivateTargetObjects(bool active)
	{
		for (int i = 0; i < IncludedObjects.Count; i++)
		{
			if (IncludedObjects [i] == null)
				continue;

			if (IncludedStates [i])
			{
				PrintDebugLog ("ForceActivateTargetObjects() " + this.Type + ", " + (active ? "activate" : "deactivate") + " " + IncludedObjects [i].name);
				IncludedObjects [i].SetActive (active);
			}
		}

		this.showTrackedObject = active;
	}

	#region Broadcast Handling
	private void onDeviceConnected(params object[] args)
	{
		if (!ptmEnabled)
		{
			if (this.Type == WaveVR_Controller.EDeviceType.Head)
				PrintDebugLog ("onDeviceConnected() Head, do NOTHING when disabled.");
			if (this.Type == WaveVR_Controller.EDeviceType.Dominant)
				PrintDebugLog ("onDeviceConnected() Dominant, do NOTHING when disabled.");
			if (this.Type == WaveVR_Controller.EDeviceType.NonDominant)
				PrintDebugLog ("onDeviceConnected() NonDominant, do NOTHING when disabled.");
			return;
		}

		WVR_DeviceType _type = (WVR_DeviceType)args [0];
		bool _connected = (bool)args [1];

		if (this.Type == WaveVR_Controller.EDeviceType.Head)
			PrintDebugLog (_connected ? "onDeviceConnected() Head is connected. " : "onDeviceConnected() Head is disconnected. ");
		if (this.Type == WaveVR_Controller.EDeviceType.Dominant)
			PrintDebugLog (_connected ? "onDeviceConnected() Dominant is connected. " : "onDeviceConnected() Dominant is disconnected. ");
		if (this.Type == WaveVR_Controller.EDeviceType.NonDominant)
			PrintDebugLog (_connected ? "onDeviceConnected() NonDominant is connected. " : "onDeviceConnected() NonDominant is disconnected. ");

		if (_type != WaveVR_Controller.Input (this.Type).DeviceType)
			return;

		this.connected = _connected;
		ActivateTargetObjects ();
	}

	private void OnNewPoses(params object[] args)
	{
		if (!ptmEnabled)
		{
			if (this.Type == WaveVR_Controller.EDeviceType.Head)
				PrintDebugLog ("OnNewPoses() Head, do NOTHING when disabled.");
			if (this.Type == WaveVR_Controller.EDeviceType.Dominant)
				PrintDebugLog ("OnNewPoses() Dominant, do NOTHING when disabled.");
			if (this.Type == WaveVR_Controller.EDeviceType.NonDominant)
				PrintDebugLog ("OnNewPoses() NonDominant, do NOTHING when disabled.");
			return;
		}

		if (!this.hasNewPose)
		{
			// After 1st frame, pose has been updated.
			if (this.Type == WaveVR_Controller.EDeviceType.Head)
				PrintDebugLog ("OnNewPoses() Head, pose updated.");
			if (this.Type == WaveVR_Controller.EDeviceType.Dominant)
				PrintDebugLog ("OnNewPoses() Dominant, pose updated.");
			if (this.Type == WaveVR_Controller.EDeviceType.NonDominant)
				PrintDebugLog ("OnNewPoses() NonDominant, pose updated.");

			this.hasNewPose = true;
		}
	}

	private void onSystemFocusChanged(params object[] args)
	{
		if (!ptmEnabled)
		{
			if (this.Type == WaveVR_Controller.EDeviceType.Head)
				PrintDebugLog ("onSystemFocusChanged() Head, do NOTHING when disabled.");
			if (this.Type == WaveVR_Controller.EDeviceType.Dominant)
				PrintDebugLog ("onSystemFocusChanged() Dominant, do NOTHING when disabled.");
			if (this.Type == WaveVR_Controller.EDeviceType.NonDominant)
				PrintDebugLog ("onSystemFocusChanged() NonDominant, do NOTHING when disabled.");
			return;
		}

		this.mFocusCapturedBySystem = (bool)args [0];
		if (this.Type == WaveVR_Controller.EDeviceType.Head)
			PrintDebugLog (mFocusCapturedBySystem ? "onSystemFocusChanged() Head, focus is captured by system." : "onSystemFocusChanged() Head, focus is NOT captured by system.");
		if (this.Type == WaveVR_Controller.EDeviceType.Dominant)
			PrintDebugLog (mFocusCapturedBySystem ? "onSystemFocusChanged() Dominant, focus is captured by system." : "onSystemFocusChanged() Dominant, focus is NOT captured by system.");
		if (this.Type == WaveVR_Controller.EDeviceType.NonDominant)
			PrintDebugLog (mFocusCapturedBySystem ? "onSystemFocusChanged() NonDominant, focus is captured by system." : "onSystemFocusChanged() NonDominant, focus is NOT captured by system.");

		ActivateTargetObjects ();
	}
	#endregion
}
