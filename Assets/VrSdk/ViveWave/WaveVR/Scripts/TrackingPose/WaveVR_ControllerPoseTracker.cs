// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//	 http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using UnityEngine;
using wvr;
//using UnityEngine.VR;
using System.Collections;
using WVR_Log;
using System.Collections.Generic;
using System;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(WaveVR_ControllerPoseTracker))]
public class WaveVR_ControllerPoseTrackerEditor : Editor
{
	public override void OnInspectorGUI()
	{
		WaveVR_ControllerPoseTracker myScript = target as WaveVR_ControllerPoseTracker;

		myScript.Type = (WaveVR_Controller.EDeviceType)EditorGUILayout.EnumPopup ("Type", myScript.Type);
		myScript.TrackPosition = EditorGUILayout.Toggle ("Track Position", myScript.TrackPosition);
		if (true == myScript.TrackPosition)
		{
			myScript.SimulationOption = (WVR_SimulationOption)EditorGUILayout.EnumPopup ("    Simulate Position", myScript.SimulationOption);
			if (myScript.SimulationOption == WVR_SimulationOption.ForceSimulation || myScript.SimulationOption == WVR_SimulationOption.WhenNoPosition)
			{
				myScript.FollowHead = (bool)EditorGUILayout.Toggle ("        Follow Head", myScript.FollowHead);
			}
		}

		myScript.TrackRotation = EditorGUILayout.Toggle ("Track Rotation", myScript.TrackRotation);
		myScript.TrackTiming = (WVR_TrackTiming)EditorGUILayout.EnumPopup ("Track Timing", myScript.TrackTiming);

		if (GUI.changed)
			EditorUtility.SetDirty ((WaveVR_ControllerPoseTracker)target);
	}
}
#endif

public class WaveVR_ControllerPoseTracker : MonoBehaviour
{
	private static string LOG_TAG = "WaveVR_ControllerPoseTracker";
	private void PrintDebugLog(string msg)
	{
		if (Log.EnableDebugLog)
			Log.d (LOG_TAG, msg);
	}
	private void PrintErrorLog(string msg)
	{
		Log.e (LOG_TAG, this.Type + ", " + msg);
	}

	#region Developer variables
	public WaveVR_Controller.EDeviceType Type;
	public bool InversePosition = false;
	public bool TrackPosition = true;
	public WVR_SimulationOption SimulationOption = WVR_SimulationOption.WhenNoPosition;
	private WVR_SimulationOption currSimulationOption = WVR_SimulationOption.WhenNoPosition;
	private bool bSetupSimulation = false;
	public bool FollowHead = false;
	private bool currFollowHead = false;
	private bool bSetupFollowHead = false;
	public bool InverseRotation = false;
	public bool TrackRotation = true;
	public WVR_TrackTiming TrackTiming = WVR_TrackTiming.WhenNewPoses;
	[HideInInspector]
	public bool ApplyNeckOffset = false;

	/// Height of the elbow  (m).
	[Range(0.0f, 0.2f)]
	public float ElbowRaiseYaxis = 0.0f;

	/// Depth of the elbow  (m).
	[Range(0.0f, 0.4f)]
	public float ElbowRaiseZaxis = 0.0f;
	#endregion

	private GameObject Head = null;
	[HideInInspector]
	private bool SetCustomHand = false;

	#region Monobehaviour
	private bool cptEnabled = false;
	void OnEnable()
	{
		if (!cptEnabled)
		{
			if (Head == null)
			{
				if (WaveVR_Render.Instance != null)
					Head = WaveVR_Render.Instance.gameObject;
			}

			if (this.TrackTiming == WVR_TrackTiming.WhenNewPoses)
				WaveVR_Utils.Event.Listen (WaveVR_Utils.Event.NEW_POSES, OnNewPoses);
			//WaveVR_Utils.Event.Listen (WaveVR_Utils.Event.ALL_VREVENT, OnEvent);

			SetupPoseSimulation();

			cptEnabled = true;

			PrintDebugLog ("OnEnable() " + this.Type
				+ ", TrackPosition: " + this.TrackPosition
				+ ", SimulationOption: " + this.SimulationOption
				+ ", FollowHead: " + this.FollowHead
				+ ", TrackRotation: " + this.TrackRotation
				+ ", TrackTiming: " + this.TrackTiming);
		}
	}

	void OnDisable()
	{
		PrintDebugLog ("OnDisable()" + this.Type +
			"\nLocal Pos: " + transform.localPosition.x + ", "  + transform.localPosition.y + ", "  + transform.localPosition.z +
			"\nPos: "	   + transform.position.x + ", "	   + transform.position.y + ", "	   + transform.position.z +
			"\nLocal rot: " + transform.localRotation.x + ", "  + transform.localRotation.y + ", "  + transform.localRotation.z);

		if (this.TrackTiming == WVR_TrackTiming.WhenNewPoses)
			WaveVR_Utils.Event.Remove(WaveVR_Utils.Event.NEW_POSES, OnNewPoses);
		//WaveVR_Utils.Event.Remove (WaveVR_Utils.Event.ALL_VREVENT, OnEvent);
		this.bSetupSimulation = false;
		this.bSetupFollowHead = false;
		cptEnabled = false;
	}

	private WVR_DevicePosePair_t wvr_pose = new WVR_DevicePosePair_t ();
	private WaveVR_Utils.RigidTransform rigid_pose = WaveVR_Utils.RigidTransform.identity;

	void Update ()
	{
		// FollowHead changes in runtime.
		if (this.FollowHead != this.currFollowHead)
			this.bSetupFollowHead = false;
		// SimulationOption changes in runtime.
		if (this.SimulationOption != this.currSimulationOption)
			this.bSetupSimulation = false;
		SetupPoseSimulation ();

		if (TrackTiming == WVR_TrackTiming.WhenNewPoses)
			return;
		if (!WaveVR.Instance.Initialized)
			return;

		WaveVR.Device device = WaveVR.Instance.getDeviceByType (this.Type);
		if (device.connected)
		{
			wvr_pose = device.pose;
			rigid_pose = device.rigidTransform;
		}

		updateDevicePose (wvr_pose, rigid_pose);
	}
	#endregion
	/*
	void OnEvent(params object[] args)
	{
		WVR_Event_t _event = (WVR_Event_t)args[0];
		switch (_event.common.type)
		{
		case WVR_EventType.WVR_EventType_ButtonPressed:
			// Get system key
			if (_event.input.inputId == WVR_InputId.WVR_InputId_Alias1_System)
			{
				PrintDebugLog (OnEvent() WVR_InputId_Alias1_System is pressed.);
			}
			break;
		case WVR_EventType.WVR_EventType_RecenterSuccess:
		case WVR_EventType.WVR_EventType_RecenterSuccess3DoF:
			PrintDebugLog (OnEvent() recentered.);
			break;
		}
	}
	*/
	private void SetupPoseSimulation()
	{
		if (WaveVR.Instance.Initialized)
		{
			if (!this.bSetupSimulation)
			{
				this.currSimulationOption = this.SimulationOption;
				if (this.currSimulationOption == WVR_SimulationOption.WhenNoPosition)
					WaveVR.Instance.SetPoseSimulation (WVR_SimulationType.WVR_SimulationType_Auto);
				else if (this.currSimulationOption == WVR_SimulationOption.ForceSimulation)
					WaveVR.Instance.SetPoseSimulation (WVR_SimulationType.WVR_SimulationType_ForceOn);
				else
					WaveVR.Instance.SetPoseSimulation (WVR_SimulationType.WVR_SimulationType_ForceOff);

				this.bSetupSimulation = true;
				PrintDebugLog ("SetupPoseSimulation() " + this.Type + ", simulation option: " + this.currSimulationOption);
			}
			if (!this.bSetupFollowHead)
			{
				this.currFollowHead = this.FollowHead;
				WaveVR.Instance.SetPoseSimulationFollowHead (this.currFollowHead);
				this.bSetupFollowHead = true;
				PrintDebugLog ("SetupPoseSimulation() " + this.Type + ", follow head: " + this.currFollowHead);
			}
		}
	}

	#region Pose Update
	private void OnNewPoses(params object[] args)
	{
		WVR_DevicePosePair_t[] _poses = (WVR_DevicePosePair_t[])args [0];
		WaveVR_Utils.RigidTransform[] _rtPoses = (WaveVR_Utils.RigidTransform[])args [1];

		WVR_DeviceType _type = WaveVR_Controller.Input (this.Type).DeviceType;
		for (int i = 0; i < _poses.Length; i++)
		{
			if (_type == _poses [i].type)
			{
				wvr_pose = _poses [i];
				rigid_pose = _rtPoses [i];
			}
		}

		updateDevicePose (wvr_pose, rigid_pose);
	}

	private Vector3 v3ChangeAxisX = new Vector3 (1, 1, 1);
	private void updateDevicePose(WVR_DevicePosePair_t pose, WaveVR_Utils.RigidTransform rtPose)
	{
		if (TrackPosition)
		{
			if (InversePosition)
				transform.localPosition = -rtPose.pos;
			else
				transform.localPosition = rtPose.pos;

			if (SetCustomHand && pose.pose.Is6DoFPose == false)
			{
				v3ChangeAxisX.x =
					WaveVR_Controller.Input (this.Type).DeviceType == WVR_DeviceType.WVR_DeviceType_Controller_Right ?
					1 : -1;
				transform.localPosition = Vector3.Scale (transform.localPosition, v3ChangeAxisX);
			}
		}
		if (TrackRotation)
		{
			if (InverseRotation)
				transform.localRotation = Quaternion.Inverse(rtPose.rot);
			else
				transform.localRotation = rtPose.rot;
		}
	}
	#endregion

	private const float BodyAngularVelocityUpperBound = 0.2f;
	private const float ControllerAngularVelocityUpperBound = 30.0f;
	private float BodyRotationFilter1(WVR_DevicePosePair_t pose)
	{
		Vector3 _v3AngularVelocity =
			new Vector3 (pose.pose.AngularVelocity.v0, pose.pose.AngularVelocity.v1, pose.pose.AngularVelocity.v2);
		float _v3magnitude = _v3AngularVelocity.magnitude;

		// If magnitude < body angular velocity upper bound, it means body rotation.
		// Thus the controller lerp filter will be 0 then controller will not move in scene.
		// If magnitude > body angular velocity upper bound, it means controller movement instead of body rotation.
		// In order to move controller smoothly, let the lerp max value to 0.2f means controller will move to correct position in 0.5s.
		// If controller angular velocity reaches upper bound, it means user wants the controller to move fast!
		float _bodyLerpFilter = Mathf.Clamp ((_v3magnitude - BodyAngularVelocityUpperBound) / ControllerAngularVelocityUpperBound, 0, 0.2f);

		return _bodyLerpFilter;
	}
}
