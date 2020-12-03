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
using WVR_Log;
using wvr;

public class WVR_HVR : wvr.Interop.WVR_Base {
	#region Interaction
	// ------------- wvr_events.h -------------
	// Events: swipe, battery status.
	[DllImportAttribute("wave_api", EntryPoint = "WVR_PollEventQueue", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool WVR_PollEventQueue_HVR(ref WVR_Event_t e);
	public override bool PollEventQueue(ref WVR_Event_t e)
	{
		return WVR_PollEventQueue_HVR(ref e);
	}

	// ------------- wvr_device.h -------------
	// Button types for which device is capable.
	[DllImportAttribute("wave_api", EntryPoint = "WVR_GetInputDeviceCapability", CallingConvention = CallingConvention.Cdecl)]
	public static extern int WVR_GetInputDeviceCapability_HVR(WVR_DeviceType type, WVR_InputType inputType);
	public override int GetInputDeviceCapability(WVR_DeviceType type, WVR_InputType inputType)
	{
		return WVR_GetInputDeviceCapability_HVR(type, inputType);
	}

	// Get analog type for which device.
	[DllImportAttribute("wave_api", EntryPoint = "WVR_GetInputDeviceAnalogType", CallingConvention = CallingConvention.Cdecl)]
	public static extern WVR_AnalogType WVR_GetInputDeviceAnalogType_HVR(WVR_DeviceType type, WVR_InputId id);
	public override WVR_AnalogType GetInputDeviceAnalogType(WVR_DeviceType type, WVR_InputId id)
	{
		return WVR_GetInputDeviceAnalogType_HVR(type, id);
	}

	// Button press and touch state.
	[DllImportAttribute("wave_api", EntryPoint = "WVR_GetInputDeviceState", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool WVR_GetInputDeviceState_HVR(WVR_DeviceType type, uint inputMask, ref uint buttons, ref uint touches,
		[In, Out] WVR_AnalogState_t[] analogArray, uint analogArrayCount);
	public override bool GetInputDeviceState(WVR_DeviceType type, uint inputMask, ref uint buttons, ref uint touches,
		[In, Out] WVR_AnalogState_t[] analogArray, uint analogArrayCount)
	{
		return WVR_GetInputDeviceState_HVR(type, inputMask, ref buttons, ref touches, analogArray, analogArrayCount);
	}

	// Count of specified button type.
	[DllImportAttribute("wave_api", EntryPoint = "WVR_GetInputTypeCount", CallingConvention = CallingConvention.Cdecl)]
	public static extern int WVR_GetInputTypeCount_HVR(WVR_DeviceType type, WVR_InputType inputType);
	public override int GetInputTypeCount(WVR_DeviceType type, WVR_InputType inputType)
	{
		return WVR_GetInputTypeCount_HVR(type, inputType);
	}

	// Button press state.
	[DllImportAttribute("wave_api", EntryPoint = "WVR_GetInputButtonState", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool WVR_GetInputButtonState_HVR(WVR_DeviceType type, WVR_InputId id);
	public override bool GetInputButtonState(WVR_DeviceType type, WVR_InputId id)
	{
		return WVR_GetInputButtonState_HVR(type, id);
	}

	// Button touch state.
	[DllImportAttribute("wave_api", EntryPoint = "WVR_GetInputTouchState", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool WVR_GetInputTouchState_HVR(WVR_DeviceType type, WVR_InputId id);
	public override bool GetInputTouchState(WVR_DeviceType type, WVR_InputId id)
	{
		return WVR_GetInputTouchState_HVR(type, id);
	}

	// Axis of analog button: touchpad (x, y), trigger (x only)
	[DllImportAttribute("wave_api", EntryPoint = "WVR_GetInputAnalogAxis", CallingConvention = CallingConvention.Cdecl)]
	public static extern WVR_Axis_t WVR_GetInputAnalogAxis_HVR(WVR_DeviceType type, WVR_InputId id);
	public override WVR_Axis_t GetInputAnalogAxis(WVR_DeviceType type, WVR_InputId id)
	{
		return WVR_GetInputAnalogAxis_HVR(type, id);
	}

	// Get transform of specified device.
	[DllImportAttribute("wave_api", EntryPoint = "WVR_GetPoseState", CallingConvention = CallingConvention.Cdecl)]
	public static extern void WVR_GetPoseState_HVR(WVR_DeviceType type, WVR_PoseOriginModel originModel, uint predictedMilliSec, ref WVR_PoseState_t poseState);
	public override void GetPoseState(WVR_DeviceType type, WVR_PoseOriginModel originModel, uint predictedMilliSec, ref WVR_PoseState_t poseState)
	{
		WVR_GetPoseState_HVR(type, originModel, predictedMilliSec, ref poseState);
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_SetTextureBounds", CallingConvention = CallingConvention.Cdecl)]
	public static extern void WVR_SetTextureBounds_HVR([In, Out] WVR_TextureBound_t[] textureBound);
	public override void SetTextureBounds([In, Out] WVR_TextureBound_t[] textureBound)
	{
		Log.i("WVR_HVR", "WVR_SetTextureBounds()");
		WVR_SetTextureBounds_HVR(textureBound);
	}

	// Get all attributes of pose of all devices.
	[DllImportAttribute("wave_api", EntryPoint = "WVR_WaitGetPoseIndex", CallingConvention = CallingConvention.Cdecl)]
	public static extern void WVR_WaitGetPoseIndex_HVR(WVR_PoseOriginModel originModel, [In, Out] WVR_DevicePosePair_t[] poseArray, uint pairArrayCount, ref uint frameIndex);
	public override void WaitGetPoseIndex(WVR_PoseOriginModel originModel, [In, Out] WVR_DevicePosePair_t[] poseArray, uint pairArrayCount, ref uint frameIndex)
	{
		Log.i("WVR_HVR", "WVR_WaitGetPoseIndex()");
		WVR_WaitGetPoseIndex_HVR(originModel, poseArray, pairArrayCount, ref frameIndex);
	}

	// Get all attributes of pose of all devices.
	[DllImportAttribute("wave_api", EntryPoint = "WVR_GetLastPoseIndex", CallingConvention = CallingConvention.Cdecl)]
	public static extern void WVR_GetLastPoseIndex_HVR(WVR_PoseOriginModel originModel, [In, Out] WVR_DevicePosePair_t[] poseArray, uint pairArrayCount, ref uint frameIndex);
	public override void GetLastPoseIndex(WVR_PoseOriginModel originModel, [In, Out] WVR_DevicePosePair_t[] poseArray, uint pairArrayCount, ref uint frameIndex)
	{
		Log.i("WVR_HVR", "GetLastPoseIndex()");
		WVR_GetLastPoseIndex_HVR(originModel, poseArray, pairArrayCount, ref frameIndex);
	}

	// Get all attributes of pose of all devices.
	[DllImportAttribute("wave_api", EntryPoint = "WVR_GetSyncPose", CallingConvention = CallingConvention.Cdecl)]
	public static extern void WVR_GetSyncPose_HVR(WVR_PoseOriginModel originModel, [In, Out] WVR_DevicePosePair_t[] poseArray, uint pairArrayCount);
	public override void GetSyncPose(WVR_PoseOriginModel originModel, [In, Out] WVR_DevicePosePair_t[] poseArray, uint pairArrayCount)
	{
		Log.i("WVR_HVR", "GetSyncPose()");
		WVR_GetSyncPose_HVR(originModel, poseArray, pairArrayCount);
	}

	// Device connection state.
	[DllImportAttribute("wave_api", EntryPoint = "WVR_IsDeviceConnected", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool WVR_IsDeviceConnected_HVR(WVR_DeviceType type);
	public override bool IsDeviceConnected(WVR_DeviceType type)
	{
		return WVR_IsDeviceConnected_HVR(type);
	}

	// Make device vibrate.
	[DllImportAttribute("wave_api", EntryPoint = "WVR_TriggerVibration", CallingConvention = CallingConvention.Cdecl)]
	public static extern void WVR_TriggerVibration_HVR(WVR_DeviceType type, WVR_InputId id, uint durationMicroSec, uint frequency, WVR_Intensity intensity);
	public override void TriggerVibration(WVR_DeviceType type, WVR_InputId id, uint durationMicroSec, uint frequency, WVR_Intensity intensity)
	{
		WVR_TriggerVibration_HVR(type, id, durationMicroSec, frequency, intensity);
	}

	// Recenter the "Virtual World" in current App.
	[DllImportAttribute("wave_api", EntryPoint = "WVR_InAppRecenter", CallingConvention = CallingConvention.Cdecl)]
	public static extern void WVR_InAppRecenter_HVR(WVR_RecenterType recenterType);
	public override void InAppRecenter(WVR_RecenterType recenterType)
	{
		WVR_InAppRecenter_HVR(recenterType);
	}

	// Enables or disables use of the neck model for 3-DOF head tracking
	[DllImportAttribute("wave_api", EntryPoint = "WVR_SetNeckModelEnabled", CallingConvention = CallingConvention.Cdecl)]
	public static extern void WVR_SetNeckModelEnabled_HVR(bool enabled);
	public override void SetNeckModelEnabled(bool enabled)
	{
		//WVR_SetNeckModelEnabled_HVR(enabled);
	}

	// Decide Neck Model on/off/3dofOn
	[DllImportAttribute("wave_api", EntryPoint = "WVR_SetNeckModel", CallingConvention = CallingConvention.Cdecl)]
	public static extern void WVR_SetNeckModel_HVR(WVR_SimulationType simulationType);
	public override void SetNeckModel(WVR_SimulationType simulationType)
	{
		WVR_SetNeckModel_HVR(simulationType);
	}

	// Decide Arm Model on/off/3dofOn
	[DllImportAttribute("wave_api", EntryPoint = "WVR_SetArmModel", CallingConvention = CallingConvention.Cdecl)]
	public static extern void WVR_SetArmModel_HVR(WVR_SimulationType simulationType);
	public override void SetArmModel(WVR_SimulationType simulationType)
	{
		//WVR_SetArmModel_HVR(simulationType);
	}

	// Decide Arm Model behaviors
	[DllImportAttribute("wave_api", EntryPoint = "WVR_SetArmSticky", CallingConvention = CallingConvention.Cdecl)]
	public static extern void WVR_SetArmSticky_HVR(bool stickyArm);
	public override void SetArmSticky(bool stickyArm)
	{
		//WVR_SetArmSticky_HVR(stickyArm);
	}

	// bool WVR_SetInputRequest(WVR_DeviceType type, const WVR_InputAttribute* request, uint32_t size);
	[DllImportAttribute("wave_api", EntryPoint = "WVR_SetInputRequest", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool WVR_SetInputRequest_HVR(WVR_DeviceType type, WVR_InputAttribute_t[] request, uint size);
	public override bool SetInputRequest(WVR_DeviceType type, WVR_InputAttribute_t[] request, uint size)
	{
		return WVR_SetInputRequest_HVR(type, request, size);
	}

	// bool WVR_GetInputMappingPair(WVR_DeviceType type, WVR_InputId destination, WVR_InputMappingPair* pair);
	[DllImportAttribute("wave_api", EntryPoint = "WVR_GetInputMappingPair", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool WVR_GetInputMappingPair_HVR(WVR_DeviceType type, WVR_InputId destination, ref WVR_InputMappingPair_t pair);
	public override bool GetInputMappingPair(WVR_DeviceType type, WVR_InputId destination, ref WVR_InputMappingPair_t pair)
	{
		return WVR_GetInputMappingPair_HVR(type, destination, ref pair);
	}

	// uint32_t WVR_GetInputMappingTable(WVR_DeviceType type, WVR_InputMappingPair* table, uint32_t size);
	[DllImportAttribute("wave_api", EntryPoint = "WVR_GetInputMappingTable", CallingConvention = CallingConvention.Cdecl)]
	public static extern uint WVR_GetInputMappingTable_HVR(WVR_DeviceType type, [In, Out] WVR_InputMappingPair_t[] table, uint size);
	public override uint GetInputMappingTable(WVR_DeviceType type, [In, Out] WVR_InputMappingPair_t[] table, uint size)
	{
		return WVR_GetInputMappingTable_HVR(type, table, size);
	}

	// ------------- wvr_arena.h -------------
	// Get current attributes of arena.
	[DllImportAttribute("wave_api", EntryPoint = "WVR_GetArena", CallingConvention = CallingConvention.Cdecl)]
	public static extern WVR_Arena_t WVR_GetArena_HVR();
	public override WVR_Arena_t GetArena()
	{
		return WVR_GetArena_HVR();
	}

	// Set up arena.
	[DllImportAttribute("wave_api", EntryPoint = "WVR_SetArena", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool WVR_SetArena_HVR(ref WVR_Arena_t arena);
	public override bool SetArena(ref WVR_Arena_t arena)
	{
		return WVR_SetArena_HVR(ref arena);
	}

	// Get visibility type of arena.
	[DllImportAttribute("wave_api", EntryPoint = "WVR_GetArenaVisible", CallingConvention = CallingConvention.Cdecl)]
	public static extern WVR_ArenaVisible WVR_GetArenaVisible_HVR();
	public override WVR_ArenaVisible GetArenaVisible()
	{
		return WVR_GetArenaVisible_HVR();
	}

	// Set visibility type of arena.
	[DllImportAttribute("wave_api", EntryPoint = "WVR_SetArenaVisible", CallingConvention = CallingConvention.Cdecl)]
	public static extern void WVR_SetArenaVisible_HVR(WVR_ArenaVisible config);
	public override void SetArenaVisible(WVR_ArenaVisible config)
	{
		WVR_SetArenaVisible_HVR(config);
	}

	// Check if player is over range of arena.
	[DllImportAttribute("wave_api", EntryPoint = "WVR_IsOverArenaRange", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool WVR_IsOverArenaRange_HVR();
	public override bool IsOverArenaRange()
	{
		return WVR_IsOverArenaRange_HVR();
	}

	// ------------- wvr_status.h -------------
	// Battery electricity (%).
	[DllImportAttribute("wave_api", EntryPoint = "WVR_GetDeviceBatteryPercentage", CallingConvention = CallingConvention.Cdecl)]
	public static extern float WVR_GetDeviceBatteryPercentage_HVR(WVR_DeviceType type);
	public override float GetDeviceBatteryPercentage(WVR_DeviceType type)
	{
		return WVR_GetDeviceBatteryPercentage_HVR(type);
	}

	// Battery life status.
	[DllImportAttribute("wave_api", EntryPoint = "WVR_GetBatteryStatus", CallingConvention = CallingConvention.Cdecl)]
	public static extern WVR_BatteryStatus WVR_GetBatteryStatus_HVR(WVR_DeviceType type);
	public override WVR_BatteryStatus GetBatteryStatus(WVR_DeviceType type)
	{
		return WVR_GetBatteryStatus_HVR(type);
	}

	// Battery is charging or not.
	[DllImportAttribute("wave_api", EntryPoint = "WVR_GetChargeStatus", CallingConvention = CallingConvention.Cdecl)]
	public static extern WVR_ChargeStatus WVR_GetChargeStatus_HVR(WVR_DeviceType type);
	public override WVR_ChargeStatus GetChargeStatus(WVR_DeviceType type)
	{
		return WVR_GetChargeStatus_HVR(type);
	}

	// Whether battery is overheat.
	[DllImportAttribute("wave_api", EntryPoint = "WVR_GetBatteryTemperatureStatus", CallingConvention = CallingConvention.Cdecl)]
	public static extern WVR_BatteryTemperatureStatus WVR_GetBatteryTemperatureStatus_HVR(WVR_DeviceType type);
	public override WVR_BatteryTemperatureStatus GetBatteryTemperatureStatus(WVR_DeviceType type)
	{
		return WVR_GetBatteryTemperatureStatus_HVR(type);
	}

	// Battery temperature.
	[DllImportAttribute("wave_api", EntryPoint = "WVR_GetBatteryTemperature", CallingConvention = CallingConvention.Cdecl)]
	public static extern float WVR_GetBatteryTemperature_HVR(WVR_DeviceType type);
	public override float GetBatteryTemperature(WVR_DeviceType type)
	{
		return WVR_GetBatteryTemperature_HVR(type);
	}
	#endregion

	// wvr.h
	[DllImportAttribute("wave_api", EntryPoint = "WVR_Init", CallingConvention = CallingConvention.Cdecl)]
	public static extern WVR_InitError WVR_Init_HVR(WVR_AppType eType);
	public override WVR_InitError Init(WVR_AppType eType)
	{
		Log.i("WVR_HVR", "Init()");
		return WVR_Init_HVR(eType);
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_Quit", CallingConvention = CallingConvention.Cdecl)]
	public static extern void WVR_Quit_HVR();
	public override void Quit()
	{
		Log.i("WVR_HVR", "Quit()");
		WVR_Quit_HVR();
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_GetInitErrorString", CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr WVR_GetInitErrorString_HVR(WVR_InitError error);
	public override IntPtr GetInitErrorString(WVR_InitError error)
	{
		Log.i("WVR_HVR", "GetInitErrorString()");
		return WVR_GetInitErrorString_HVR(error);
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_GetWaveRuntimeVersion", CallingConvention = CallingConvention.Cdecl)]
	public static extern uint WVR_GetWaveRuntimeVersion_HVR();
	public override uint GetWaveRuntimeVersion()
	{
		Log.i("WVR_HVR", "GetWaveRuntimeVersion()");
		return WVR_GetWaveRuntimeVersion_HVR();
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_GetWaveSDKVersion", CallingConvention = CallingConvention.Cdecl)]
	public static extern uint WVR_GetWaveSDKVersion_HVR();
	public override uint GetWaveSDKVersion()
	{
		Log.i("WVR_HVR", "GetWaveSDKVersion()");
		//return WVR_GetWaveSDKVersion_HVR();
		return 1;
	}

	// wvr_system.h
	[DllImportAttribute("wave_api", EntryPoint = "WVR_IsInputFocusCapturedBySystem", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool WVR_IsInputFocusCapturedBySystem_HVR();
	public override bool IsInputFocusCapturedBySystem()
	{
		Log.i("WVR_HVR", "IsInputFocusCapturedBySystem()");
		return WVR_IsInputFocusCapturedBySystem_HVR();
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_RenderInit", CallingConvention = CallingConvention.Cdecl)]
	internal static extern WVR_RenderError WVR_RenderInit_HVR(ref WVR_RenderInitParams_t param);
	internal override WVR_RenderError RenderInit(ref WVR_RenderInitParams_t param)
	{
		return WVR_RenderInit_HVR(ref param);
	}

	// Set CPU and GPU performance level.
	[DllImportAttribute("wave_api", EntryPoint = "WVR_SetPerformanceLevels", CallingConvention = CallingConvention.Cdecl)]
	internal static extern bool WVR_SetPerformanceLevels_HVR(WVR_PerfLevel cpuLevel, WVR_PerfLevel gpuLevel);
	internal override bool SetPerformanceLevels(WVR_PerfLevel cpuLevel, WVR_PerfLevel gpuLevel)
	{
		Log.i("WVR_HVR", "SetPerformanceLevels()");
		//return WVR_SetPerformanceLevels_HVR(cpuLevel, gpuLevel);
		return false;
	}

	// Allow WaveVR SDK runtime to adjust render quality and CPU/GPU perforamnce level automatically.
	[DllImportAttribute("wave_api", EntryPoint = "WVR_EnableAdaptiveQuality", CallingConvention = CallingConvention.Cdecl)]
	internal static extern bool WVR_EnableAdaptiveQuality_HVR(bool enable, uint flags);
	internal override bool EnableAdaptiveQuality(bool enable, uint flags)
	{
		Log.i("WVR_HVR", "EnableAdaptiveQuality()");
		//return WVR_EnableAdaptiveQuality_HVR(enable, flags);
		return false;
	}

	// Check if adaptive quailty enabled.
	[DllImportAttribute("wave_api", EntryPoint = "WVR_IsAdaptiveQualityEnabled", CallingConvention = CallingConvention.Cdecl)]
	internal static extern bool WVR_IsAdaptiveQualityEnabled_HVR();
	internal override bool IsAdaptiveQualityEnabled()
	{
		Log.i("WVR_HVR", "IsAdaptiveQualityEnabled()");
		return WVR_IsAdaptiveQualityEnabled_HVR();
	}

	// wvr_camera.h
	[DllImportAttribute("wave_api", EntryPoint = "WVR_StartCamera", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool WVR_StartCamera_HVR(ref WVR_CameraInfo_t info);
	public override bool StartCamera(ref WVR_CameraInfo_t info)
	{
		Log.i("WVR_HVR", "StartCamera()");
		return WVR_StartCamera_HVR(ref info);
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_StopCamera", CallingConvention = CallingConvention.Cdecl)]
	public static extern void WVR_StopCamera_HVR();
	public override void StopCamera()
	{
		Log.i("WVR_HVR", "StopCamera()");
		WVR_StopCamera_HVR();
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_UpdateTexture", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool WVR_UpdateTexture_HVR(IntPtr textureid);
	public override bool UpdateTexture(IntPtr textureid)
	{
		Log.i("WVR_HVR", "UpdateTexture()");
		return WVR_UpdateTexture_HVR(textureid);
	}

	[DllImportAttribute("wave_api", EntryPoint = "DrawTextureWithBuffer", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool DrawTextureWithBuffer_HVR(IntPtr textureId, WVR_CameraImageFormat imgFormat, IntPtr frameBuffer, uint size, uint width, uint height);
	public override bool DrawTextureWithBuffer(IntPtr textureId, WVR_CameraImageFormat imgFormat, IntPtr frameBuffer, uint size, uint width, uint height)
	{
		return DrawTextureWithBuffer_HVR(textureId, imgFormat, frameBuffer, size, width, height);
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_GetCameraIntrinsic", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool WVR_GetCameraIntrinsic_HVR(WVR_CameraPosition position, ref WVR_CameraIntrinsic_t intrinsic);
	public override bool GetCameraIntrinsic(WVR_CameraPosition position, ref WVR_CameraIntrinsic_t intrinsic)
	{
		Log.i("WVR_HVR", "GetCameraIntrinsic()");
		return WVR_GetCameraIntrinsic_HVR(position, ref intrinsic);
	}

	// wvr_device.h
	[DllImportAttribute("wave_api", EntryPoint = "WVR_IsDeviceSuspend", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool WVR_IsDeviceSuspend_HVR(WVR_DeviceType type);
	public override bool IsDeviceSuspend(WVR_DeviceType type)
	{
		Log.i("WVR_HVR", "IsDeviceSuspend()");
		return WVR_IsDeviceSuspend_HVR(type);
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_ConvertMatrixQuaternion", CallingConvention = CallingConvention.Cdecl)]
	public static extern void WVR_ConvertMatrixQuaternion_HVR(ref WVR_Matrix4f_t mat, ref WVR_Quatf_t quat, bool m2q);
	public override void ConvertMatrixQuaternion(ref WVR_Matrix4f_t mat, ref WVR_Quatf_t quat, bool m2q)
	{
		Log.i("WVR_HVR", "ConvertMatrixQuaternion()");
		WVR_ConvertMatrixQuaternion_HVR(ref mat, ref quat, m2q);
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_GetDegreeOfFreedom", CallingConvention = CallingConvention.Cdecl)]
	public static extern WVR_NumDoF WVR_GetDegreeOfFreedom_HVR(WVR_DeviceType type);
	public override WVR_NumDoF GetDegreeOfFreedom(WVR_DeviceType type)
	{
		Log.i("WVR_HVR", "GetDegreeOfFreedom()");
		return WVR_GetDegreeOfFreedom_HVR(type);
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_SetParameters", CallingConvention = CallingConvention.Cdecl)]
	public static extern void WVR_SetParameters_HVR(WVR_DeviceType type, IntPtr pchValue);
	public override void SetParameters(WVR_DeviceType type, IntPtr pchValue)
	{
		Log.i("WVR_HVR", "SetParameters()");
		WVR_SetParameters_HVR(type, pchValue);
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_GetParameters", CallingConvention = CallingConvention.Cdecl)]
	public static extern uint WVR_GetParameters_HVR(WVR_DeviceType type, IntPtr pchValue, IntPtr retValue, uint unBufferSize);
	public override uint GetParameters(WVR_DeviceType type, IntPtr pchValue, IntPtr retValue, uint unBufferSize)
	{
		Log.i("WVR_HVR", "GetParameters()");
		return WVR_GetParameters_HVR(type, pchValue, retValue, unBufferSize);
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_GetDefaultControllerRole", CallingConvention = CallingConvention.Cdecl)]
	public static extern WVR_DeviceType WVR_GetDefaultControllerRole_HVR();
	public override WVR_DeviceType GetDefaultControllerRole()
	{
		return WVR_GetDefaultControllerRole_HVR();
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_SetInteractionMode", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool WVR_SetInteractionMode_HVR(WVR_InteractionMode mode);
	public override bool SetInteractionMode(WVR_InteractionMode mode)
	{
		return WVR_SetInteractionMode_HVR(mode);
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_GetInteractionMode", CallingConvention = CallingConvention.Cdecl)]
	public static extern WVR_InteractionMode WVR_GetInteractionMode_HVR();
	public override WVR_InteractionMode GetInteractionMode()
	{
		return WVR_GetInteractionMode_HVR();
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_SetGazeTriggerType", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool WVR_SetGazeTriggerType_HVR(WVR_GazeTriggerType type);
	public override bool SetGazeTriggerType(WVR_GazeTriggerType type)
	{
		return WVR_SetGazeTriggerType_HVR(type);
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_GetGazeTriggerType", CallingConvention = CallingConvention.Cdecl)]
	public static extern WVR_GazeTriggerType WVR_GetGazeTriggerType_HVR();
	public override WVR_GazeTriggerType GetGazeTriggerType()
	{
		return WVR_GetGazeTriggerType_HVR();
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_GetDeviceErrorState", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool WVR_GetDeviceErrorState_HVR(WVR_DeviceType dev_type, WVR_DeviceErrorState error_state);
	public override bool GetDeviceErrorState(WVR_DeviceType dev_type, WVR_DeviceErrorState error_state)
	{
		Log.i("WVR_HVR", "GetDeviceErrorState()");
		return WVR_GetDeviceErrorState_HVR(dev_type, error_state);
	}

	// TODO
	[DllImportAttribute("wave_api", EntryPoint = "WVR_GetRenderTargetSize", CallingConvention = CallingConvention.Cdecl)]
	public static extern void WVR_GetRenderTargetSize_HVR(ref uint width, ref uint height);
	public override void GetRenderTargetSize(ref uint width, ref uint height)
	{
		Log.i("WVR_HVR", "GetRenderTargetSize()");
		WVR_GetRenderTargetSize_HVR(ref width, ref height);
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_GetProjection", CallingConvention = CallingConvention.Cdecl)]
	public static extern WVR_Matrix4f_t WVR_GetProjection_HVR(WVR_Eye eye, float near, float far);
	public override WVR_Matrix4f_t GetProjection(WVR_Eye eye, float near, float far)
	{
		Log.i("WVR_HVR", "GetProjection()");
		return WVR_GetProjection_HVR(eye, near, far);
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_GetClippingPlaneBoundary", CallingConvention = CallingConvention.Cdecl)]
	public static extern void WVR_GetClippingPlaneBoundary_HVR(WVR_Eye eye, ref float left, ref float right, ref float top, ref float bottom);
	public override void GetClippingPlaneBoundary(WVR_Eye eye, ref float left, ref float right, ref float top, ref float bottom)
	{
		Log.i("WVR_HVR", "GetClippingPlaneBoundary()");
		WVR_GetClippingPlaneBoundary_HVR(eye, ref left, ref right, ref top, ref bottom);
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_SetOverfillRatio", CallingConvention = CallingConvention.Cdecl)]
	public static extern void WVR_SetOverfillRatio_HVR(float ratioX, float ratioY);
	public override void SetOverfillRatio(float ratioX, float ratioY)
	{
		Log.i("WVR_HVR", "SetOverfillRatio()");
		WVR_SetOverfillRatio_HVR(ratioX, ratioY);
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_GetTransformFromEyeToHead", CallingConvention = CallingConvention.Cdecl)]
	public static extern WVR_Matrix4f_t WVR_GetTransformFromEyeToHead_HVR(WVR_Eye eye, WVR_NumDoF dof);
	public override WVR_Matrix4f_t GetTransformFromEyeToHead(WVR_Eye eye, WVR_NumDoF dof)
	{
		Log.i("WVR_HVR", "GetTransformFromEyeToHead()");
		return WVR_GetTransformFromEyeToHead_HVR(eye, dof);
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_SubmitFrame", CallingConvention = CallingConvention.Cdecl)]
	public static extern WVR_SubmitError WVR_SubmitFrame_HVR(WVR_Eye eye, [In, Out] WVR_TextureParams_t[] param, [In, Out] WVR_PoseState_t[] pose, WVR_SubmitExtend extendMethod);
	public override WVR_SubmitError SubmitFrame(WVR_Eye eye, [In, Out] WVR_TextureParams_t[] param, [In, Out] WVR_PoseState_t[] pose, WVR_SubmitExtend extendMethod)
	{
		Log.i("WVR_HVR", "SubmitFrame()");
		return WVR_SubmitFrame_HVR(eye, param, pose, extendMethod);
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_SetSubmitParams", CallingConvention = CallingConvention.Cdecl)]
	public static extern void WVR_SetSubmitParams_HVR(WVR_Eye eye, [Out] WVR_TextureParams_t[] param, [Out] WVR_PoseState_t[] pose, WVR_SubmitExtend extendMethod);
	public override void SetSubmitParams(WVR_Eye eye, [In, Out] WVR_TextureParams_t[] param, [In, Out] WVR_PoseState_t[] pose, WVR_SubmitExtend extendMethod)
	{
		Log.i("WVR_HVR", "SetSubmitParams()");
		WVR_SetSubmitParams_HVR(eye, param, pose, extendMethod);
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_RequestScreenshot", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool WVR_RequestScreenshot_HVR(uint width, uint height, WVR_ScreenshotMode mode, IntPtr filename);
	public override bool RequestScreenshot(uint width, uint height, WVR_ScreenshotMode mode, IntPtr filename)
	{
		Log.i("WVR_HVR", "RequestScreenshot()");
		return WVR_RequestScreenshot_HVR(width, height, mode, filename);
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_RenderMask", CallingConvention = CallingConvention.Cdecl)]
	public static extern void WVR_RenderMask_HVR(WVR_Eye eye);
	public override void RenderMask(WVR_Eye eye)
	{
		Log.i("WVR_HVR", "RenderMask()");
		WVR_RenderMask_HVR(eye);
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_GetRenderProps", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool WVR_GetRenderProps_HVR(ref WVR_RenderProps_t props);
	public override bool GetRenderProps(ref WVR_RenderProps_t props)
	{
		Log.i("WVR_HVR", "GetRenderProps()");
		return WVR_GetRenderProps_HVR(ref props);
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_ObtainTextureQueue", CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr WVR_ObtainTextureQueue_HVR(WVR_TextureTarget target, WVR_TextureFormat format, WVR_TextureType type, uint width, uint height, int level);
	public override IntPtr ObtainTextureQueue(WVR_TextureTarget target, WVR_TextureFormat format, WVR_TextureType type, uint width, uint height, int level)
	{
		Log.i("WVR_HVR", "ObtainTextureQueue()");
		return WVR_ObtainTextureQueue_HVR(target, format, type, width, height, level);
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_GetTextureQueueLength", CallingConvention = CallingConvention.Cdecl)]
	public static extern uint WVR_GetTextureQueueLength_HVR(IntPtr handle);
	public override uint GetTextureQueueLength(IntPtr handle)
	{
		Log.i("WVR_HVR", "GetTextureQueueLength()");
		return WVR_GetTextureQueueLength_HVR(handle);
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_GetTexture", CallingConvention = CallingConvention.Cdecl)]
	public static extern WVR_TextureParams_t WVR_GetTexture_HVR(IntPtr handle, int index);
	public override WVR_TextureParams_t GetTexture(IntPtr handle, int index)
	{
		Log.i("WVR_HVR", "GetTexture()");
		return WVR_GetTexture_HVR(handle, index);
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_GetAvailableTextureIndex", CallingConvention = CallingConvention.Cdecl)]
	public static extern int WVR_GetAvailableTextureIndex_HVR(IntPtr handle);
	public override int GetAvailableTextureIndex(IntPtr handle)
	{
		Log.i("WVR_HVR", "GetAvailableTextureIndex()");
		return WVR_GetAvailableTextureIndex_HVR(handle);
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_ReleaseTextureQueue", CallingConvention = CallingConvention.Cdecl)]
	public static extern void WVR_ReleaseTextureQueue_HVR(IntPtr handle);
	public override void ReleaseTextureQueue(IntPtr handle)
	{
		Log.i("WVR_HVR", "ReleaseTextureQueue()");
		WVR_ReleaseTextureQueue_HVR(handle);
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_IsRenderFoveationSupport", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool WVR_IsRenderFoveationSupport_HVR();
	public override bool IsRenderFoveationSupport()
	{
		return WVR_IsRenderFoveationSupport_HVR();
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_RenderFoveation", CallingConvention = CallingConvention.Cdecl)]
	public static extern void WVR_RenderFoveation_HVR(bool enable);
	public override void RenderFoveation(bool enable)
	{
		Log.i("WVR_HVR", "RenderFoveation()");
		WVR_RenderFoveation_HVR(enable);
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_SetFocusedController", CallingConvention = CallingConvention.Cdecl)]
	public static extern void WVR_SetFocusedController_HVR(WVR_DeviceType focusController);
	public override void SetFocusedController(WVR_DeviceType focusController)
	{
		Log.i("WVR_HVR", "SetFocusedController()");
		WVR_SetFocusedController_HVR(focusController);
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_GetFocusedController", CallingConvention = CallingConvention.Cdecl)]
	public static extern WVR_DeviceType WVR_GetFocusedController_HVR();
	public override WVR_DeviceType GetFocusedController()
	{
		Log.i("WVR_HVR", "GetFocusedController()");
		return WVR_GetFocusedController_HVR();
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_StoreRenderTextures", CallingConvention = CallingConvention.Cdecl)]
	public static extern System.IntPtr WVR_StoreRenderTexturesHVR(System.IntPtr[] texturesIDs, int size, bool eEye, WVR_TextureTarget target);
	public override System.IntPtr StoreRenderTextures(System.IntPtr[] texturesIDs, int size, bool eEye, WVR_TextureTarget target)
	{
		Log.i("WVR_HVR", "StoreRenderTextures()");
		return WVR_StoreRenderTexturesHVR(texturesIDs, size, eEye, target);
	}
}
