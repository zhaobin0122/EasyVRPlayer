// "WaveVR SDK 
// © 2017 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the WaveVR SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using wvr;

#if UNITY_EDITOR
public class WVR_Editor : wvr.Interop.WVR_Base
{
	private static WaveVR_UnityEditor system = null;
	public WVR_Editor()
	{
		Debug.Log ("wvr_editor()");
		system = WaveVR_UnityEditor.Instance;
	}

	~WVR_Editor()
	{
	}

	#region [Event]
	public override bool PollEventQueue(ref WVR_Event_t e)
	{
		return system.PollEventQueue (ref e);
	}

	public override bool GetInputButtonState(WVR_DeviceType type, WVR_InputId id)
	{
		return system.GetInputButtonState (type, id);
	}

	public override bool GetInputTouchState(WVR_DeviceType type, WVR_InputId id)
	{
		return system.GetInputTouchState (type, id);
	}

	public override WVR_Axis_t GetInputAnalogAxis(WVR_DeviceType type, WVR_InputId id)
	{
		return system.GetInputAnalogAxis (type, id);
	}
	#endregion

	#region Device Pose
	public override void GetSyncPose(WVR_PoseOriginModel originModel, [In, Out] WVR_DevicePosePair_t[] poseArray, uint pairArrayCount)
	{
		system.GetSyncPose (originModel, poseArray, pairArrayCount);
	}
	#endregion

	#region Simulation Pose
	public override void SetArmModel(WVR_SimulationType simulationType)
	{
		system.SetArmModel (simulationType);
	}

	public override void SetArmSticky(bool stickyArm)
	{
		system.SetArmSticky (stickyArm);
	}

	public override void SetNeckModelEnabled(bool enabled)
	{
		system.SetNeckModelEnabled (enabled);
	}
	#endregion

	#region Key Mapping
	public override bool SetInputRequest(WVR_DeviceType type, WVR_InputAttribute_t[] request, uint size)
	{
		return system.SetInputRequest (type, request, size);
	}

	public override uint GetInputMappingTable(WVR_DeviceType type, [In, Out] WVR_InputMappingPair_t[] table, uint size)
	{
		return system.GetInputMappingTable (type, table, size);
	}

	public override bool GetInputMappingPair(WVR_DeviceType type, WVR_InputId destination, ref WVR_InputMappingPair_t pair)
	{
		return system.GetInputMappingPair (type, destination, ref pair);
	}
	#endregion

	#region Interaction Mode
	public override bool SetInteractionMode(WVR_InteractionMode mode)
	{
		return system.SetInteractionMode (mode);
	}

	public override WVR_InteractionMode GetInteractionMode()
	{
		return system.GetInteractionMode ();
	}

	public override bool SetGazeTriggerType(WVR_GazeTriggerType type)
	{
		return system.SetGazeTriggerType (type);
	}
	public override WVR_GazeTriggerType GetGazeTriggerType()
	{
		return system.GetGazeTriggerType ();
	}
	#endregion

	#region Arena
	public override bool SetArena(ref WVR_Arena_t arena)
	{
		return system.SetArena (ref arena);
	}

	public override WVR_Arena_t GetArena()
	{
		return system.GetArena ();
	}

	public override bool IsOverArenaRange()
	{
		return system.IsOverArenaRange ();
	}

	public override void SetArenaVisible(WVR_ArenaVisible config)
	{
		system.SetArenaVisible (config);
	}

	public override WVR_ArenaVisible GetArenaVisible()
	{
		return system.GetArenaVisible ();
	}
	#endregion

	#region Focused Controller
	public override WVR_DeviceType GetFocusedController()
	{
		return system.GetFocusedController ();
	}

	public override void SetFocusedController(WVR_DeviceType focusController)
	{
		system.SetFocusedController (focusController);
	}
	#endregion

	#region Gesture
	public override WVR_Result StartHandGesture()
	{
		return system.StartHandGesture ();
	}

	public override void StopHandGesture()
	{
		system.StopHandGesture ();
	}

	public override WVR_Result GetHandGestureData(ref WVR_HandGestureData_t data)
	{
		return system.GetHandGestureData (ref data);
	}

	public override WVR_Result StartHandTracking()
	{
		return system.StartHandTracking ();
	}

	public override void StopHandTracking()
	{
		system.StopHandTracking ();
	}

	public override WVR_Result GetHandTrackingData(ref WVR_HandTrackingData_t data, WVR_PoseOriginModel originModel, uint predictedMilliSec)
	{
		return system.GetHandTrackingData (ref data, originModel, predictedMilliSec);
	}
	#endregion

	public override bool IsDeviceConnected(WVR_DeviceType type)
	{
		return system.IsDeviceConnected (type);
	}

	public override bool IsInputFocusCapturedBySystem()
	{
		return system.IsInputFocusCapturedBySystem ();
	}

	public override void InAppRecenter(WVR_RecenterType recenterType)
	{
		system.InAppRecenter (recenterType);
	}
}
#endif