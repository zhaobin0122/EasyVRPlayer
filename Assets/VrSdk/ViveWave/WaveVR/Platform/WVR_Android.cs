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

public class WVR_Android : wvr.Interop.WVR_Base
{
	#region Interaction
	// ------------- wvr_events.h -------------
	// Events: swipe, battery status.
	[DllImportAttribute("wvr_api", EntryPoint = "WVR_PollEventQueue", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool WVR_PollEventQueue_Android (ref WVR_Event_t e);
	public override bool PollEventQueue(ref WVR_Event_t e)
	{
		return WVR_PollEventQueue_Android(ref e);
	}

	// ------------- wvr_device.h -------------
	// Button types for which device is capable.
	[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetInputDeviceCapability", CallingConvention = CallingConvention.Cdecl)]
	public static extern int WVR_GetInputDeviceCapability_Android(WVR_DeviceType type, WVR_InputType inputType);
	public override int GetInputDeviceCapability(WVR_DeviceType type, WVR_InputType inputType)
	{
		return WVR_GetInputDeviceCapability_Android(type, inputType);
	}

	// Get analog type for which device.
	[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetInputDeviceAnalogType", CallingConvention = CallingConvention.Cdecl)]
	public static extern WVR_AnalogType WVR_GetInputDeviceAnalogType_Android(WVR_DeviceType type, WVR_InputId id);
	public override WVR_AnalogType GetInputDeviceAnalogType(WVR_DeviceType type, WVR_InputId id)
	{
		return WVR_GetInputDeviceAnalogType_Android(type, id);
	}

	// Button press and touch state.
	[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetInputDeviceState", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool WVR_GetInputDeviceState_Android(WVR_DeviceType type, uint inputMask, ref uint buttons, ref uint touches,
		[In, Out] WVR_AnalogState_t[] analogArray, uint analogArrayCount);
	public override bool GetInputDeviceState(WVR_DeviceType type, uint inputMask, ref uint buttons, ref uint touches,
		[In, Out] WVR_AnalogState_t[] analogArray, uint analogArrayCount)
	{
		return WVR_GetInputDeviceState_Android(type, inputMask, ref buttons, ref touches, analogArray, analogArrayCount);
	}

	// Count of specified button type.
	[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetInputTypeCount", CallingConvention = CallingConvention.Cdecl)]
	public static extern int WVR_GetInputTypeCount_Android(WVR_DeviceType type, WVR_InputType inputType);
	public override int GetInputTypeCount(WVR_DeviceType type, WVR_InputType inputType)
	{
		return WVR_GetInputTypeCount_Android(type, inputType);
	}

	// Button press state.
	[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetInputButtonState", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool WVR_GetInputButtonState_Android(WVR_DeviceType type, WVR_InputId id);
	public override bool GetInputButtonState(WVR_DeviceType type, WVR_InputId id)
	{
		return WVR_GetInputButtonState_Android(type, id);
	}

	// Button touch state.
	[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetInputTouchState", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool WVR_GetInputTouchState_Android(WVR_DeviceType type, WVR_InputId id);
	public override bool GetInputTouchState(WVR_DeviceType type, WVR_InputId id)
	{
		return WVR_GetInputTouchState_Android(type, id);
	}

	// Axis of analog button: touchpad (x, y), trigger (x only)
	[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetInputAnalogAxis", CallingConvention = CallingConvention.Cdecl)]
	public static extern WVR_Axis_t WVR_GetInputAnalogAxis_Android(WVR_DeviceType type, WVR_InputId id);
	public override WVR_Axis_t GetInputAnalogAxis(WVR_DeviceType type, WVR_InputId id)
	{
		return WVR_GetInputAnalogAxis_Android(type, id);
	}

	// Get transform of specified device.
	[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetPoseState", CallingConvention = CallingConvention.Cdecl)]
	public static extern void WVR_GetPoseState_Android(WVR_DeviceType type, WVR_PoseOriginModel originModel, uint predictedMilliSec, ref WVR_PoseState_t poseState);
	public override void GetPoseState(WVR_DeviceType type, WVR_PoseOriginModel originModel, uint predictedMilliSec, ref WVR_PoseState_t poseState)
	{
		WVR_GetPoseState_Android(type, originModel, predictedMilliSec, ref poseState);
	}

	// Get all attributes of pose of all devices.
	[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetSyncPose", CallingConvention = CallingConvention.Cdecl)]
	public static extern void WVR_GetSyncPose_Android(WVR_PoseOriginModel originModel, [In, Out] WVR_DevicePosePair_t[] poseArray, uint pairArrayCount);
	public override void GetSyncPose(WVR_PoseOriginModel originModel, [In, Out] WVR_DevicePosePair_t[] poseArray, uint pairArrayCount)
	{
		WVR_GetSyncPose_Android(originModel, poseArray, pairArrayCount);
	}

	// Device connection state.
	[DllImportAttribute("wvr_api", EntryPoint = "WVR_IsDeviceConnected", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool WVR_IsDeviceConnected_Android(WVR_DeviceType type);
	public override bool IsDeviceConnected(WVR_DeviceType type)
	{
		return WVR_IsDeviceConnected_Android(type);
	}

	// Make device vibration.
	[DllImportAttribute("wvr_api", EntryPoint = "WVR_TriggerVibration", CallingConvention = CallingConvention.Cdecl)]
	public static extern void WVR_TriggerVibration_Android(WVR_DeviceType type, WVR_InputId id, uint durationMicroSec, uint frequency, WVR_Intensity intensity);
	public override void TriggerVibration(WVR_DeviceType type, WVR_InputId id, uint durationMicroSec, uint frequency, WVR_Intensity intensity)
	{
		WVR_TriggerVibration_Android(type, id, durationMicroSec, frequency, intensity);
	}

	// Recenter the "Virtual World" in current App.
	[DllImportAttribute("wvr_api", EntryPoint = "WVR_InAppRecenter", CallingConvention = CallingConvention.Cdecl)]
	public static extern void WVR_InAppRecenter_Android(WVR_RecenterType recenterType);
	public override void InAppRecenter(WVR_RecenterType recenterType)
	{
		WVR_InAppRecenter_Android(recenterType);
	}

	// Enables or disables use of the neck model for 3-DOF head tracking
	[DllImportAttribute("wvr_api", EntryPoint = "WVR_SetNeckModelEnabled", CallingConvention = CallingConvention.Cdecl)]
	public static extern void WVR_SetNeckModelEnabled_Android(bool enabled);
	public override void SetNeckModelEnabled(bool enabled)
	{
		WVR_SetNeckModelEnabled_Android(enabled);
	}

	// Decide Neck Model on/off/3dofOn
	[DllImportAttribute("wvr_api", EntryPoint = "WVR_SetNeckModel", CallingConvention = CallingConvention.Cdecl)]
	public static extern void WVR_SetNeckModel_Android(WVR_SimulationType simulationType);
	public override void SetNeckModel(WVR_SimulationType simulationType)
	{
		WVR_SetNeckModel_Android(simulationType);
	}

	// Decide Arm Model on/off/3dofOn
	[DllImportAttribute("wvr_api", EntryPoint = "WVR_SetArmModel", CallingConvention = CallingConvention.Cdecl)]
	public static extern void WVR_SetArmModel_Android(WVR_SimulationType simulationType);
	public override void SetArmModel(WVR_SimulationType simulationType)
	{
		WVR_SetArmModel_Android(simulationType);
	}

	// Decide Arm Model behaviors
	[DllImportAttribute("wvr_api", EntryPoint = "WVR_SetArmSticky", CallingConvention = CallingConvention.Cdecl)]
	public static extern void WVR_SetArmSticky_Android(bool stickyArm);
	public override void SetArmSticky(bool stickyArm)
	{
		WVR_SetArmSticky_Android(stickyArm);
	}

	// bool WVR_SetInputRequest(WVR_DeviceType type, const WVR_InputAttribute* request, uint32_t size);
	[DllImportAttribute("wvr_api", EntryPoint = "WVR_SetInputRequest", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool WVR_SetInputRequest_Android(WVR_DeviceType type, WVR_InputAttribute_t[] request, uint size);
	public override bool SetInputRequest(WVR_DeviceType type, WVR_InputAttribute_t[] request, uint size)
	{
		return WVR_SetInputRequest_Android(type, request, size);
	}

	// bool WVR_GetInputMappingPair(WVR_DeviceType type, WVR_InputId destination, WVR_InputMappingPair* pair);
	[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetInputMappingPair", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool WVR_GetInputMappingPair_Android(WVR_DeviceType type, WVR_InputId destination, ref WVR_InputMappingPair_t pair);
	public override bool GetInputMappingPair(WVR_DeviceType type, WVR_InputId destination, ref WVR_InputMappingPair_t pair)
	{
		return WVR_GetInputMappingPair_Android(type, destination, ref pair);
	}

	// uint32_t WVR_GetInputMappingTable(WVR_DeviceType type, WVR_InputMappingPair* table, uint32_t size);
	[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetInputMappingTable", CallingConvention = CallingConvention.Cdecl)]
	public static extern uint WVR_GetInputMappingTable_Android(WVR_DeviceType type, [In, Out] WVR_InputMappingPair_t[] table, uint size);
	public override uint GetInputMappingTable(WVR_DeviceType type, [In, Out] WVR_InputMappingPair_t[] table, uint size)
	{
		return WVR_GetInputMappingTable_Android(type, table, size);
	}

	// ------------- wvr_arena.h -------------
	// Get current attributes of arena.
	[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetArena", CallingConvention = CallingConvention.Cdecl)]
	public static extern WVR_Arena_t WVR_GetArena_Android();
	public override WVR_Arena_t GetArena()
	{
		return WVR_GetArena_Android();
	}

	// Set up arena.
	[DllImportAttribute("wvr_api", EntryPoint = "WVR_SetArena", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool WVR_SetArena_Android(ref WVR_Arena_t arena);
	public override bool SetArena(ref WVR_Arena_t arena)
	{
		return WVR_SetArena_Android(ref arena);
	}

	// Get visibility type of arena.
	[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetArenaVisible", CallingConvention = CallingConvention.Cdecl)]
	public static extern WVR_ArenaVisible WVR_GetArenaVisible_Android();
	public override WVR_ArenaVisible GetArenaVisible()
	{
		return WVR_GetArenaVisible_Android();
	}

	// Set visibility type of arena.
	[DllImportAttribute("wvr_api", EntryPoint = "WVR_SetArenaVisible", CallingConvention = CallingConvention.Cdecl)]
	public static extern void WVR_SetArenaVisible_Android(WVR_ArenaVisible config);
	public override void SetArenaVisible(WVR_ArenaVisible config)
	{
		WVR_SetArenaVisible_Android(config);
	}

	// Check if player is over range of arena.
	[DllImportAttribute("wvr_api", EntryPoint = "WVR_IsOverArenaRange", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool WVR_IsOverArenaRange_Android();
	public override bool IsOverArenaRange()
	{
		return WVR_IsOverArenaRange_Android();
	}

	// ------------- wvr_status.h -------------
	// Battery electricity (%).
	[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetDeviceBatteryPercentage", CallingConvention = CallingConvention.Cdecl)]
	public static extern float WVR_GetDeviceBatteryPercentage_Android(WVR_DeviceType type);
	public override float GetDeviceBatteryPercentage(WVR_DeviceType type)
	{
		return WVR_GetDeviceBatteryPercentage_Android(type);
	}

	// Battery life status.
	[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetBatteryStatus", CallingConvention = CallingConvention.Cdecl)]
	public static extern WVR_BatteryStatus WVR_GetBatteryStatus_Android(WVR_DeviceType type);
	public override WVR_BatteryStatus GetBatteryStatus(WVR_DeviceType type)
	{
		return WVR_GetBatteryStatus_Android(type);
	}

	// Battery is charging or not.
	[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetChargeStatus", CallingConvention = CallingConvention.Cdecl)]
	public static extern WVR_ChargeStatus WVR_GetChargeStatus_Android(WVR_DeviceType type);
	public override WVR_ChargeStatus GetChargeStatus(WVR_DeviceType type)
	{
		return WVR_GetChargeStatus_Android(type);
	}

	// Whether battery is overheat.
	[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetBatteryTemperatureStatus", CallingConvention = CallingConvention.Cdecl)]
	public static extern WVR_BatteryTemperatureStatus WVR_GetBatteryTemperatureStatus_Android(WVR_DeviceType type);
	public override WVR_BatteryTemperatureStatus GetBatteryTemperatureStatus(WVR_DeviceType type)
	{
		return WVR_GetBatteryTemperatureStatus_Android(type);
	}

	// Battery temperature.
	[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetBatteryTemperature", CallingConvention = CallingConvention.Cdecl)]
	public static extern float WVR_GetBatteryTemperature_Android(WVR_DeviceType type);
	public override float GetBatteryTemperature(WVR_DeviceType type)
	{
		return WVR_GetBatteryTemperature_Android(type);
	}

	// ------------- wvr_hand.h -------------
	[DllImportAttribute("wvr_api", EntryPoint = "WVR_StartHandGesture", CallingConvention = CallingConvention.Cdecl)]
	public static extern WVR_Result WVR_StartHandGesture ();
	public override WVR_Result StartHandGesture()
	{
		return WVR_StartHandGesture ();
	}

	[DllImportAttribute("wvr_api", EntryPoint = "WVR_StopHandGesture", CallingConvention = CallingConvention.Cdecl)]
	public static extern void WVR_StopHandGesture ();
	public override void StopHandGesture()
	{
		WVR_StopHandGesture ();
	}

	[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetHandGestureData", CallingConvention = CallingConvention.Cdecl)]
	public static extern WVR_Result WVR_GetHandGestureData (ref WVR_HandGestureData_t data);
	public override WVR_Result GetHandGestureData(ref WVR_HandGestureData_t data)
	{
		return WVR_GetHandGestureData (ref data);
	}

	[DllImportAttribute("wvr_api", EntryPoint = "WVR_StartHandTracking", CallingConvention = CallingConvention.Cdecl)]
	public static extern WVR_Result WVR_StartHandTracking ();
	public override WVR_Result StartHandTracking()
	{
		return WVR_StartHandTracking ();
	}

	[DllImportAttribute("wvr_api", EntryPoint = "WVR_StopHandTracking", CallingConvention = CallingConvention.Cdecl)]
	public static extern void WVR_StopHandTracking ();
	public override void StopHandTracking()
	{
		WVR_StopHandTracking ();
	}

	[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetHandTrackingData", CallingConvention = CallingConvention.Cdecl)]
	public static extern WVR_Result WVR_GetHandTrackingData (ref WVR_HandTrackingData_t data, WVR_PoseOriginModel originModel, uint predictedMilliSec);
	public override WVR_Result GetHandTrackingData(ref WVR_HandTrackingData_t data, WVR_PoseOriginModel originModel, uint predictedMilliSec)
	{
		return WVR_GetHandTrackingData (ref data, originModel, predictedMilliSec);
	}
	#endregion

	[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetSupportedFeatures", CallingConvention = CallingConvention.Cdecl)]
	public static extern ulong WVR_GetSupportedFeatures();
	public override ulong GetSupportedFeatures()
	{
		return WVR_GetSupportedFeatures ();
	}

		// wvr.h
		[DllImportAttribute("wvr_api", EntryPoint = "WVR_Init", CallingConvention = CallingConvention.Cdecl)]
		public static extern WVR_InitError WVR_Init_Android(WVR_AppType eType);
		public override WVR_InitError Init(WVR_AppType eType)
		{
			return WVR_Init_Android(eType);
		}

		[DllImportAttribute("wvr_api", EntryPoint = "WVR_Quit", CallingConvention = CallingConvention.Cdecl)]
		public static extern void WVR_Quit_Android();
		public override void Quit()
		{
			WVR_Quit_Android();
		}

		[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetInitErrorString", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr WVR_GetInitErrorString_Android(WVR_InitError error);
		public override IntPtr GetInitErrorString(WVR_InitError error)
		{
			return WVR_GetInitErrorString_Android(error);
		}

		[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetWaveRuntimeVersion", CallingConvention = CallingConvention.Cdecl)]
		public static extern uint WVR_GetWaveRuntimeVersion_Android();
		public override uint GetWaveRuntimeVersion()
		{
			return WVR_GetWaveRuntimeVersion_Android();
		}

		[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetWaveSDKVersion", CallingConvention = CallingConvention.Cdecl)]
		public static extern uint WVR_GetWaveSDKVersion_Android();
		public override uint GetWaveSDKVersion()
		{
			return WVR_GetWaveSDKVersion_Android();
		}

		// wvr_system.h
		[DllImportAttribute("wvr_api", EntryPoint = "WVR_IsInputFocusCapturedBySystem", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool WVR_IsInputFocusCapturedBySystem_Android();
		public override bool IsInputFocusCapturedBySystem()
		{
			return WVR_IsInputFocusCapturedBySystem_Android();
		}

		[DllImportAttribute("wvr_api", EntryPoint = "WVR_RenderInit", CallingConvention = CallingConvention.Cdecl)]
		internal static extern WVR_RenderError WVR_RenderInit_Android(ref WVR_RenderInitParams_t param);
		internal override WVR_RenderError RenderInit(ref WVR_RenderInitParams_t param)
		{
			return WVR_RenderInit_Android(ref param);
		}

		// Set CPU and GPU performance level.
		[DllImportAttribute("wvr_api", EntryPoint = "WVR_SetPerformanceLevels", CallingConvention = CallingConvention.Cdecl)]
		internal static extern bool WVR_SetPerformanceLevels_Android(WVR_PerfLevel cpuLevel, WVR_PerfLevel gpuLevel);
		internal override bool SetPerformanceLevels(WVR_PerfLevel cpuLevel, WVR_PerfLevel gpuLevel)
		{
			return WVR_SetPerformanceLevels_Android(cpuLevel, gpuLevel);
		}

		// Allow WaveVR SDK runtime to adjust render quality and CPU/GPU perforamnce level automatically.
		[DllImportAttribute("wvr_api", EntryPoint = "WVR_EnableAdaptiveQuality", CallingConvention = CallingConvention.Cdecl)]
		internal static extern bool WVR_EnableAdaptiveQuality_Android(bool enable, uint flags);
		internal override bool EnableAdaptiveQuality(bool enable, uint flags)
		{
			return WVR_EnableAdaptiveQuality_Android(enable, flags);
		}

		// Check if adaptive quailty enabled.
		[DllImportAttribute("wvr_api", EntryPoint = "WVR_IsAdaptiveQualityEnabled", CallingConvention = CallingConvention.Cdecl)]
		internal static extern bool WVR_IsAdaptiveQualityEnabled_Android();
		internal override bool IsAdaptiveQualityEnabled()
		{
			return WVR_IsAdaptiveQualityEnabled_Android();
		}

		// wvr_camera.h
		[DllImportAttribute("wvr_api", EntryPoint = "WVR_StartCamera", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool WVR_StartCamera_Android(ref WVR_CameraInfo_t info);
		public override bool StartCamera(ref WVR_CameraInfo_t info)
		{
			return WVR_StartCamera_Android(ref info);
		}

		[DllImportAttribute("wvr_api", EntryPoint = "WVR_StopCamera", CallingConvention = CallingConvention.Cdecl)]
		public static extern void WVR_StopCamera_Android();
		public override void StopCamera()
		{
			WVR_StopCamera_Android();
		}

		[DllImportAttribute("wvr_api", EntryPoint = "WVR_UpdateTexture", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool WVR_UpdateTexture_Android(uint textureid);
		public override bool UpdateTexture(IntPtr textureid)
		{
			return WVR_UpdateTexture_Android((uint)textureid);
		}

		[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetCameraIntrinsic", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool WVR_GetCameraIntrinsic_Android(WVR_CameraPosition position, ref WVR_CameraIntrinsic_t intrinsic);
		public override bool GetCameraIntrinsic(WVR_CameraPosition position, ref WVR_CameraIntrinsic_t intrinsic)
		{
			return WVR_GetCameraIntrinsic_Android(position, ref intrinsic);
		}

		[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetCameraFrameBuffer", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool WVR_GetCameraFrameBuffer_Android(IntPtr pFramebuffer, uint frameBufferSize);
		public override bool GetCameraFrameBuffer(IntPtr pFramebuffer, uint frameBufferSize)
		{
			return WVR_GetCameraFrameBuffer_Android(pFramebuffer, frameBufferSize);
		}

		[DllImportAttribute("wvrutility", EntryPoint = "GetFrameBufferWithPoseState", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool GetFrameBufferWithPoseState_Android(IntPtr pFramebuffer, uint frameBufferSize, WVR_PoseOriginModel origin, uint predictInMs, ref WVR_PoseState_t poseState);
		public override bool GetFrameBufferWithPoseState(IntPtr pFramebuffer, uint frameBufferSize, WVR_PoseOriginModel origin, uint predictInMs, ref WVR_PoseState_t poseState)
		{
			return GetFrameBufferWithPoseState_Android(pFramebuffer, frameBufferSize, origin, predictInMs, ref poseState);
		}

		[DllImportAttribute("wvrutility", EntryPoint = "ReleaseAll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ReleaseCameraTexture_Android();
		public override void ReleaseCameraTexture()
		{
			ReleaseCameraTexture_Android();
		}

		[DllImportAttribute("wvrutility", EntryPoint = "DrawTextureWithBuffer", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool DrawTextureWithBuffer_Android(uint textureId, WVR_CameraImageFormat imgFormat, IntPtr frameBuffer, uint size, uint width, uint height);
		public override bool DrawTextureWithBuffer(IntPtr textureId, WVR_CameraImageFormat imgFormat, IntPtr frameBuffer, uint size, uint width, uint height)
		{
			return DrawTextureWithBuffer_Android((uint)textureId, imgFormat, frameBuffer, size, width, height);
		}

		// wvr_device.h
		[DllImportAttribute("wvr_api", EntryPoint = "WVR_IsDeviceSuspend", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool WVR_IsDeviceSuspend_Android(WVR_DeviceType type);
		public override bool IsDeviceSuspend(WVR_DeviceType type)
		{
			return WVR_IsDeviceSuspend_Android(type);
		}

		[DllImportAttribute("wvr_api", EntryPoint = "WVR_ConvertMatrixQuaternion", CallingConvention = CallingConvention.Cdecl)]
		public static extern void WVR_ConvertMatrixQuaternion_Android(ref WVR_Matrix4f_t mat, ref WVR_Quatf_t quat, bool m2q);
		public override void ConvertMatrixQuaternion(ref WVR_Matrix4f_t mat, ref WVR_Quatf_t quat, bool m2q)
		{
			WVR_ConvertMatrixQuaternion_Android(ref mat, ref quat, m2q);
		}

		[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetDegreeOfFreedom", CallingConvention = CallingConvention.Cdecl)]
		public static extern WVR_NumDoF WVR_GetDegreeOfFreedom_Android(WVR_DeviceType type);
		public override WVR_NumDoF GetDegreeOfFreedom(WVR_DeviceType type)
		{
			return WVR_GetDegreeOfFreedom_Android(type);
		}

		[DllImportAttribute("wvr_api", EntryPoint = "WVR_SetParameters", CallingConvention = CallingConvention.Cdecl)]
		public static extern void WVR_SetParameters_Android(WVR_DeviceType type, IntPtr pchValue);
		public override void SetParameters(WVR_DeviceType type, IntPtr pchValue)
		{
			WVR_SetParameters_Android(type, pchValue);
		}

		[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetParameters", CallingConvention = CallingConvention.Cdecl)]
		public static extern uint WVR_GetParameters_Android(WVR_DeviceType type, IntPtr pchValue, IntPtr retValue, uint unBufferSize);
		public override uint GetParameters(WVR_DeviceType type, IntPtr pchValue, IntPtr retValue, uint unBufferSize)
		{
			return WVR_GetParameters_Android(type, pchValue, retValue, unBufferSize);
		}

		[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetDefaultControllerRole", CallingConvention = CallingConvention.Cdecl)]
		public static extern WVR_DeviceType WVR_GetDefaultControllerRole_Android();
		public override WVR_DeviceType GetDefaultControllerRole()
		{
			return WVR_GetDefaultControllerRole_Android();
		}

		[DllImportAttribute("wvr_api", EntryPoint = "WVR_SetInteractionMode", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool WVR_SetInteractionMode_Android(WVR_InteractionMode mode);
		public override bool SetInteractionMode(WVR_InteractionMode mode)
		{
			return WVR_SetInteractionMode_Android(mode);
		}

		[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetInteractionMode", CallingConvention = CallingConvention.Cdecl)]
		public static extern WVR_InteractionMode WVR_GetInteractionMode_Android();
		public override WVR_InteractionMode GetInteractionMode()
		{
			return WVR_GetInteractionMode_Android();
		}

		[DllImportAttribute("wvr_api", EntryPoint = "WVR_SetGazeTriggerType", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool WVR_SetGazeTriggerType_Android(WVR_GazeTriggerType type);
		public override bool SetGazeTriggerType(WVR_GazeTriggerType type)
		{
			return WVR_SetGazeTriggerType_Android(type);
		}

		[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetGazeTriggerType", CallingConvention = CallingConvention.Cdecl)]
		public static extern WVR_GazeTriggerType WVR_GetGazeTriggerType_Android();
		public override WVR_GazeTriggerType GetGazeTriggerType()
		{
			return WVR_GetGazeTriggerType_Android();
		}

		[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetDeviceErrorState", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool WVR_GetDeviceErrorState_Android(WVR_DeviceType dev_type, WVR_DeviceErrorState error_state);
		public override bool GetDeviceErrorState(WVR_DeviceType dev_type, WVR_DeviceErrorState error_state)
		{
			return WVR_GetDeviceErrorState_Android(dev_type, error_state);
		}

		// TODO
		[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetRenderTargetSize", CallingConvention = CallingConvention.Cdecl)]
		public static extern void WVR_GetRenderTargetSize_Android(ref uint width, ref uint height);
		public override void GetRenderTargetSize(ref uint width, ref uint height)
		{
			WVR_GetRenderTargetSize_Android(ref width, ref height);
		}

		[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetProjection", CallingConvention = CallingConvention.Cdecl)]
		public static extern WVR_Matrix4f_t WVR_GetProjection_Android(WVR_Eye eye, float near, float far);
		public override WVR_Matrix4f_t GetProjection(WVR_Eye eye, float near, float far)
		{
			return WVR_GetProjection_Android(eye, near, far);
		}

		[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetClippingPlaneBoundary", CallingConvention = CallingConvention.Cdecl)]
		public static extern void WVR_GetClippingPlaneBoundary_Android(WVR_Eye eye, ref float left, ref float right, ref float top, ref float bottom);
		public override void GetClippingPlaneBoundary(WVR_Eye eye, ref float left, ref float right, ref float top, ref float bottom)
		{
			WVR_GetClippingPlaneBoundary_Android(eye, ref left, ref right, ref top, ref bottom);
		}

		[DllImportAttribute("wvr_api", EntryPoint = "WVR_SetOverfillRatio", CallingConvention = CallingConvention.Cdecl)]
		public static extern void WVR_SetOverfillRatio_Android(float ratioX, float ratioY);
		public override void SetOverfillRatio(float ratioX, float ratioY)
		{
			WVR_SetOverfillRatio_Android(ratioX, ratioY);
		}

		[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetTransformFromEyeToHead", CallingConvention = CallingConvention.Cdecl)]
		public static extern WVR_Matrix4f_t WVR_GetTransformFromEyeToHead_Android(WVR_Eye eye, WVR_NumDoF dof);
		public override WVR_Matrix4f_t GetTransformFromEyeToHead(WVR_Eye eye, WVR_NumDoF dof)
		{
			return WVR_GetTransformFromEyeToHead_Android(eye, dof);
		}

		[DllImportAttribute("wvr_api", EntryPoint = "WVR_SubmitFrame", CallingConvention = CallingConvention.Cdecl)]
		public static extern WVR_SubmitError WVR_SubmitFrame_Android(WVR_Eye eye, [In, Out] WVR_TextureParams_t[] param, [In, Out] WVR_PoseState_t[] pose, WVR_SubmitExtend extendMethod);
		public override WVR_SubmitError SubmitFrame(WVR_Eye eye, [In, Out] WVR_TextureParams_t[] param, [In, Out] WVR_PoseState_t[] pose, WVR_SubmitExtend extendMethod)
		{
			return WVR_SubmitFrame_Android(eye, param, pose, extendMethod);
		}

		[DllImportAttribute("wvr_api", EntryPoint = "WVR_PreRenderEye", CallingConvention = CallingConvention.Cdecl)]
		public static extern void WVR_PreRenderEye_Android(WVR_Eye eye, [In, Out] WVR_TextureParams_t[] param, [In, Out] WVR_RenderFoveationParams[] foveationParams);
		public override void PreRenderEye(WVR_Eye eye, [In, Out] WVR_TextureParams_t[] param, [In, Out] WVR_RenderFoveationParams[] foveationParams)
		{
			WVR_PreRenderEye_Android(eye, param, foveationParams);
		}

		[DllImportAttribute("wvr_api", EntryPoint = "WVR_RequestScreenshot", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool WVR_RequestScreenshot_Android(uint width, uint height, WVR_ScreenshotMode mode, IntPtr filename);
		public override bool RequestScreenshot(uint width, uint height, WVR_ScreenshotMode mode, IntPtr filename)
		{
			return WVR_RequestScreenshot_Android(width, height, mode, filename);
		}

		[DllImportAttribute("wvr_api", EntryPoint = "WVR_RenderMask", CallingConvention = CallingConvention.Cdecl)]
		public static extern void WVR_RenderMask_Android(WVR_Eye eye);
		public override void RenderMask(WVR_Eye eye)
		{
			WVR_RenderMask_Android(eye);
		}

		[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetRenderProps", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool WVR_GetRenderProps_Android(ref WVR_RenderProps_t props);
		public override bool GetRenderProps(ref WVR_RenderProps_t props)
		{
			return WVR_GetRenderProps_Android(ref props);
		}

		[DllImportAttribute("wvr_api", EntryPoint = "WVR_ObtainTextureQueue", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr WVR_ObtainTextureQueue_Android(WVR_TextureTarget target, WVR_TextureFormat format, WVR_TextureType type, uint width, uint height, int level);
		public override IntPtr ObtainTextureQueue(WVR_TextureTarget target, WVR_TextureFormat format, WVR_TextureType type, uint width, uint height, int level)
		{
			return WVR_ObtainTextureQueue_Android(target, format, type, width, height, level);
		}

		[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetTextureQueueLength", CallingConvention = CallingConvention.Cdecl)]
		public static extern uint WVR_GetTextureQueueLength_Android(IntPtr handle);
		public override uint GetTextureQueueLength(IntPtr handle)
		{
			return WVR_GetTextureQueueLength_Android(handle);
		}

		[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetTexture", CallingConvention = CallingConvention.Cdecl)]
		public static extern WVR_TextureParams_t WVR_GetTexture_Android(IntPtr handle, int index);
		public override WVR_TextureParams_t GetTexture(IntPtr handle, int index)
		{
			return WVR_GetTexture_Android(handle, index);
		}

		[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetAvailableTextureIndex", CallingConvention = CallingConvention.Cdecl)]
		public static extern int WVR_GetAvailableTextureIndex_Android(IntPtr handle);
		public override int GetAvailableTextureIndex(IntPtr handle)
		{
			return WVR_GetAvailableTextureIndex_Android(handle);
		}

		[DllImportAttribute("wvr_api", EntryPoint = "WVR_ReleaseTextureQueue", CallingConvention = CallingConvention.Cdecl)]
		public static extern void WVR_ReleaseTextureQueue_Android(IntPtr handle);
		public override void ReleaseTextureQueue(IntPtr handle)
		{
			WVR_ReleaseTextureQueue_Android(handle);
		}

		[DllImportAttribute("wvr_api", EntryPoint = "WVR_IsRenderFoveationSupport", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool WVR_IsRenderFoveationSupport_Android();
		public override bool IsRenderFoveationSupport()
		{
			return WVR_IsRenderFoveationSupport_Android();
		}

		[DllImportAttribute("wvr_api", EntryPoint = "WVR_RenderFoveation", CallingConvention = CallingConvention.Cdecl)]
		public static extern void WVR_RenderFoveation_Android(bool enable);
		public override void RenderFoveation(bool enable)
		{
			WVR_RenderFoveation_Android(enable);
		}

		[DllImportAttribute("wvr_api", EntryPoint = "WVR_SetPosePredictEnabled", CallingConvention = CallingConvention.Cdecl)]
		public static extern void WVR_SetPosePredictEnabled_Android(WVR_DeviceType type, bool enabled_position_predict, bool enable_rotation_predict);
		public override void SetPosePredictEnabled(WVR_DeviceType type, bool enabled_position_predict, bool enable_rotation_predict)
		{
			WVR_SetPosePredictEnabled_Android(type, enabled_position_predict, enable_rotation_predict);
		}

		[DllImportAttribute("wvr_api", EntryPoint = "WVR_ShowPassthroughOverlay", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool WVR_ShowPassthroughOverlay_Android(bool show);
		public override bool ShowPassthroughOverlay(bool show)
		{
			return WVR_ShowPassthroughOverlay_Android(show);
		}

		[DllImportAttribute("wvr_api", EntryPoint = "WVR_EnableAutoPassthrough", CallingConvention = CallingConvention.Cdecl)]
		public static extern void WVR_EnableAutoPassthrough_Android(bool enable);
		public override void EnableAutoPassthrough(bool enable)
		{
			WVR_EnableAutoPassthrough_Android(enable);
		}

		[DllImportAttribute("wvr_api", EntryPoint = "WVR_IsPassthroughOverlayVisible", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool WVR_IsPassthroughOverlayVisible_Android();
		public override bool IsPassthroughOverlayVisible()
		{
			return WVR_IsPassthroughOverlayVisible_Android();

		}

	#region Internal
	public override string DeployRenderModelAssets(int deviceIndex, string renderModelName)
		{
			const string VRACTIVITY_CLASSNAME = "com.htc.vr.unity.WVRUnityVRActivity";
			const string FILEUTILS_CLASSNAME = "com.htc.vr.unity.FileUtils";

			AndroidJavaClass ajc = new AndroidJavaClass(VRACTIVITY_CLASSNAME);

			if (ajc == null || deviceIndex == -1)
			{
				//PrintWarningLog("AndroidJavaClass vractivity is null, deviceIndex" + deviceIndex);
				return "";
			}
			else
			{
				AndroidJavaObject activity = ajc.CallStatic<AndroidJavaObject>("getInstance");
				if (activity != null)
				{
					AndroidJavaObject afd = activity.Call<AndroidJavaObject>("getControllerModelFileDescriptor", deviceIndex);
					if (afd != null)
					{
						AndroidJavaObject fileUtisObject = new AndroidJavaObject(FILEUTILS_CLASSNAME, activity, afd);

						if (fileUtisObject != null)
						{
							string retUnzip = fileUtisObject.Call<string>("deployRenderModelAssets", renderModelName);

							if (retUnzip == "")
							{
								//PrintWarningLog("doUnZIPAndDeploy failed");
							}
							else
							{
								//PrintInfoLog("doUnZIPAndDeploy success");
								ajc = null;
								return retUnzip;
							}
						}
						else
						{
							//PrintWarningLog("fileUtisObject is null");
						}
					}
					else
					{
						//PrintWarningLog("get fd failed");
					}
				}
				else
				{
					//PrintWarningLog("getInstance failed");
				}
			}
			ajc = null;
			return "";
		}

		[DllImportAttribute("wvr_api", EntryPoint = "WVR_SetFocusedController", CallingConvention = CallingConvention.Cdecl)]
		public static extern void WVR_SetFocusedController_Android(WVR_DeviceType focusController);
		public override void SetFocusedController(WVR_DeviceType focusController)
		{
			WVR_SetFocusedController_Android(focusController);
		}

		[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetFocusedController", CallingConvention = CallingConvention.Cdecl)]
		public static extern WVR_DeviceType WVR_GetFocusedController_Android();
		public override WVR_DeviceType GetFocusedController()
		{
			return WVR_GetFocusedController_Android();
		}

		[DllImportAttribute("wvrassimp", EntryPoint = "OpenMesh", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool WVR_OpenMesh_Android(string filename, ref uint sessiionid, IntPtr errorCode, bool merge);
		public override bool OpenMesh(string filename, ref uint sessionid, IntPtr errorCode, bool merge)
		{
			return WVR_OpenMesh_Android(filename, ref sessionid, errorCode, merge);
		}

		[DllImportAttribute("wvrassimp", EntryPoint = "getSectionCount", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool WVR_getSectionCount_Android(uint sessionid, ref uint sectionCount);
		public override bool GetSectionCount(uint sessionid, ref uint sectionCount)
		{
			return WVR_getSectionCount_Android(sessionid, ref sectionCount);
		}

		[DllImportAttribute("wvrassimp", EntryPoint = "getMeshData", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool WVR_getMeshData_Android(uint sessionid, [In, Out] FBXInfo_t[] infoArray);
		public override bool GetMeshData(uint sessionid, [In, Out] FBXInfo_t[] infoArray)
		{
			return WVR_getMeshData_Android(sessionid, infoArray);
		}

		[DllImportAttribute("wvrassimp", EntryPoint = "getSectionData", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool WVR_getSectionData_Android(uint sessionid, uint sectionIndiceIndex, [In, Out] Vector3[] vecticeArray, [In, Out] Vector3[] normalArray, [In, Out] Vector2[] uvArray, [In, Out] int[] indiceArray, ref bool active);
		public override bool GetSectionData(uint sessionid, uint sectionIndiceIndex, [In, Out] Vector3[] vecticeArray, [In, Out] Vector3[] normalArray, [In, Out] Vector2[] uvArray, [In, Out] int[] indiceArray, ref bool active)
		{
			return WVR_getSectionData_Android(sessionid, sectionIndiceIndex, vecticeArray, normalArray, uvArray, indiceArray, ref active);
		}

		[DllImportAttribute("wvrassimp", EntryPoint = "releaseMesh", CallingConvention = CallingConvention.Cdecl)]
		public static extern void WVR_releaseMesh_Android(uint sessionid);
		public override void ReleaseMesh(uint sessionid)
		{
			WVR_releaseMesh_Android(sessionid);
		}

		private const string PERMISSION_MANAGER_CLASSNAME = "com.htc.vr.permission.client.PermissionManager";
		private static WVR_RequestCompleteCallback mCallback = null;
		private static WVR_RequestUsbCompleteCallback mUsbCallback = null;
		private AndroidJavaObject permissionsManager = null;

		private AndroidJavaObject javaArrayFromCS(string[] values)
		{
			AndroidJavaClass arrayClass = new AndroidJavaClass("java.lang.reflect.Array");
			AndroidJavaObject arrayObject = arrayClass.CallStatic<AndroidJavaObject>("newInstance", new AndroidJavaClass("java.lang.String"), values.Length);
			for (int i = 0; i < values.Length; ++i)
			{
				arrayClass.CallStatic("set", arrayObject, i, new AndroidJavaObject("java.lang.String", values[i]));
			}

			return arrayObject;
		}

		public override bool IsPermissionInitialed()
		{
			bool ret = false;
			if (permissionsManager == null)
			{
				AndroidJavaClass ajc = new AndroidJavaClass(PERMISSION_MANAGER_CLASSNAME);

				if (ajc != null)
				{
					permissionsManager = ajc.CallStatic<AndroidJavaObject>("getInstance");
				}
			}

			if (permissionsManager != null)
			{
				ret = permissionsManager.Call<bool>("isInitialized");
			}

			return ret;
		}

		public override bool ShowDialogOnScene()
		{
			if (!IsPermissionInitialed())
			{
				return false;
			}

			return permissionsManager.Call<bool>("showDialogOnVRScene");
		}

		public override bool IsPermissionGranted(string permission)
		{
			if (!IsPermissionInitialed())
			{
				return false;
			}

			return permissionsManager.Call<bool>("isPermissionGranted", permission);
		}

		public override bool ShouldGrantPermission(string permission)
		{
			if (!IsPermissionInitialed())
			{
				return false;
			}

			return permissionsManager.Call<bool>("shouldGrantPermission", permission);
		}

		public override void RequestPermissions(string[] permissions, WVR_RequestCompleteCallback cb)
		{
			//Log.d(LOG_TAG, "requestPermission");

			if (!IsPermissionInitialed())
			{
				//Log.e(LOG_TAG, "requestPermissions failed because permissionsManager doesn't initialize");
				return;
			}

			mCallback = cb;
			if (!permissionsManager.Call<bool>("isShow2D"))
			{
				permissionsManager.Call("requestPermissions", javaArrayFromCS(permissions), new RequestCompleteHandler());
			}
			else
			{
				using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
				{
					using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
					{
						jo.Call("setRequestPermissionCallback", new RequestCompleteHandler());
					}
				}
				permissionsManager.Call("requestPermissions", javaArrayFromCS(permissions), new RequestCompleteHandler());
			}
		}

		public override void RequestUsbPermission(WVR_RequestUsbCompleteCallback cb)
		{

			if (!IsPermissionInitialed())
			{
				return;
			}

			mUsbCallback = cb;
		    permissionsManager.Call("requestUsbPermission", new RequestUsbCompleteHandler());
		}



		class RequestCompleteHandler : AndroidJavaProxy
		{
			internal RequestCompleteHandler() : base(new AndroidJavaClass("com.htc.vr.permission.client.PermissionCallback"))
			{
			}

			public void onRequestCompletedwithObject(AndroidJavaObject resultObject)
			{
				//Log.i(LOG_TAG, "unity callback with result object");
				if (mCallback == null)
				{
				   // Log.w(LOG_TAG, "unity callback but user callback is null ");
				}

				List<WVR_RequestResult> permissionResults = new List<WVR_RequestResult>();
				bool[] resultArray = null;
				AndroidJavaObject boolbuffer = resultObject.Get<AndroidJavaObject>("result");
				if ((boolbuffer) != null && (boolbuffer.GetRawObject() != IntPtr.Zero))
				{
					try
					{
	#if UNITY_2018
						resultArray = AndroidJNI.FromBooleanArray(boolbuffer.GetRawObject());
	#else
						resultArray = AndroidJNIHelper.ConvertFromJNIArray<bool[]>(boolbuffer.GetRawObject());
	#endif
					  //  Log.i(LOG_TAG, "ConvertFromJNIArray to bool array : " + resultArray.Length);
					}
					catch (Exception)
					{
					  //  Log.e(LOG_TAG, "ConvertFromJNIArray failed: " + e.ToString());
					}
				}

				string[] permissionArray = null;

				AndroidJavaObject stringbuffer = resultObject.Get<AndroidJavaObject>("requestPermissions");
				if ((stringbuffer) != null && (stringbuffer.GetRawObject() != IntPtr.Zero))
				{
					permissionArray = AndroidJNIHelper.ConvertFromJNIArray<string[]>(stringbuffer.GetRawObject());
				   // Log.i(LOG_TAG, "ConvertFromJNIArray to string array : " + permissionArray.Length);
				}

				if (permissionArray != null && resultArray != null)
				{
					for (int i = 0; i < permissionArray.Length; i++)
					{
						WVR_RequestResult rr;
						rr.mPermission = permissionArray[i];
						rr.mGranted = resultArray[i];
						permissionResults.Add(rr);
					}
				}

				mCallback(permissionResults);
			}
		}

        class RequestUsbCompleteHandler : AndroidJavaProxy
		{
			internal RequestUsbCompleteHandler() : base(new AndroidJavaClass("com.htc.vr.permission.client.UsbPermissionCallback"))
			{
			}
	     	public void onRequestCompletedwithObject(AndroidJavaObject resultObject)
			{
				//Log.i(LOG_TAG, "unity callback with result object");
				if (mUsbCallback == null)
				{
				   // Log.w(LOG_TAG, "unity callback but user callback is null ");
				}
   			    bool resut = resultObject.Get<bool>("result");
				mUsbCallback(resut);
			}
		}

		private const string RESOURCE_WRAPPER_CLASSNAME = "com.htc.vr.unity.ResourceWrapper";
		private AndroidJavaObject ResourceWrapper = null;

		private bool initializeResourceObject()
		{
			if (ResourceWrapper == null)
			{
				AndroidJavaClass ajc = new AndroidJavaClass(RESOURCE_WRAPPER_CLASSNAME);

				if (ajc != null)
				{
					// Get the PermissionManager object
					ResourceWrapper = ajc.CallStatic<AndroidJavaObject>("getInstance");
				}
			}
			return (ResourceWrapper == null) ? false : true;
		}

		public override string GetStringBySystemLanguage(string stringName)
		{
			string retString = "";
			//Log.d(LOG_TAG, "getString, string " + stringName);
			if (initializeResourceObject())
			{
				retString = ResourceWrapper.Call<string>("getStringByName", stringName);
			}
			//Log.d(LOG_TAG, "getString, return string " + retString);
			return retString;
		}

		public override string GetStringByLanguage(string stringName, string lang, string country)
		{
			string retString = "";
			//Log.d(LOG_TAG, "getPreferredStringByName, string = " + stringName + ", lang = " + lang + ", country = " + country);
			if (initializeResourceObject())
			{
				retString = ResourceWrapper.Call<string>("getPreferredStringByName", stringName, lang, country);
			}
			//Log.d(LOG_TAG, "getPreferredStringByName, return string " + retString);
			return retString;
		}

		public override string GetSystemLanguage()
		{
			string retString = "";
			if (initializeResourceObject())
			{
				retString = ResourceWrapper.Call<string>("getSystemLanguage");
			}
			//Log.d(LOG_TAG, "getSystemLanguage, return string " + retString);
			return retString;
		}

		public override string GetSystemCountry()
		{
			string retString = "";
			if (initializeResourceObject())
			{
				retString = ResourceWrapper.Call<string>("getSystemCountry");
			}
			//Log.d(LOG_TAG, "getSystemCountry, return string " + retString);
			return retString;
		}

		private const string OEM_CONFIG_CLASSNAME = "com.htc.vr.unity.WVRUnityVRActivity";
		private static WVR_OnOEMConfigChanged OEMChangedCallback = null;
		private static AndroidJavaObject mOEMConfig = null;

		public static OEMConfigCallback mOEMCallback = new OEMConfigCallback();

		public class OEMConfigCallback : AndroidJavaProxy
		{
			internal OEMConfigCallback() : base(new AndroidJavaClass("com.htc.vr.unity.WVRUnityVRActivity$OEMConfigCallback"))
			{
			}

			public void onConfigChanged()
			{
				if (OEMChangedCallback != null)
				{
					OEMChangedCallback();
				}
			}
		}

		private static void initAJC()
		{
			if (mOEMConfig == null)
			{
				AndroidJavaClass ajc = new AndroidJavaClass(OEM_CONFIG_CLASSNAME);

				if (ajc == null)
				{
					// Log.e(LOG_TAG, "AndroidJavaClass is null");
					return;
				}
				// Get the OEMConfig object
				mOEMConfig = ajc.CallStatic<AndroidJavaObject>("getInstance");

				mOEMConfig.Call("setOEMChangedCB", mOEMCallback);
			}
		}

		public override string GetOEMConfigByKey(string key)
		{
			string getString = "";
			initAJC();

			if (mOEMConfig != null)
			{
				getString = mOEMConfig.Call<string>("getJsonRawData", key);
#if false
				const int charPerLine = 200;
				int len = (getString.Length / charPerLine);

				Log.d(LOG_TAG, "len = " + len + ", length of string = " + getString.Length);
				Log.d(LOG_TAG, key + ": raw data = ");

				for (int i = 0; i < len; i++)
				{
					string substr = getString.Substring(i * charPerLine, charPerLine);
					Log.d(LOG_TAG, substr);
				}

				int remainLen = getString.Length - (len * charPerLine);
				string remainstr = getString.Substring(len * charPerLine, remainLen);
				Log.d(LOG_TAG, remainstr);
#endif
			}

			getString.Trim(' ');
			if (getString.Length == 0 || getString[0] != '{' || getString[getString.Length - 1] != '}')
				return "";
			return getString;
		}

		public override void SetOEMConfigChangedCallback(WVR_OnOEMConfigChanged cb)
		{
			OEMChangedCallback = cb;
		}
	#endregion
}
