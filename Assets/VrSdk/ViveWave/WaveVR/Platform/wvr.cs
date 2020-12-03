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
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace wvr
{
	public enum WVR_AppType
	{
		WVR_AppType_VRContent = 1,
		WVR_AppType_NonVRContent = 2,
	}

	public enum WVR_InitError
	{
		WVR_InitError_None = 0,
		WVR_InitError_Unknown = 1,
		WVR_InitError_NotInitialized = 2,
	}

	public enum WVR_EventType
	{
		/** common event region */
		WVR_EventType_Quit                               = 1000,    /**< Application Quit. */
		WVR_EventType_SystemInteractionModeChanged       = 1001,    /**< @ref WVR_InteractionMode changed; using @ref WVR_GetInteractionMode to get interaction mode. */
		WVR_EventType_SystemGazeTriggerTypeChanged       = 1002,    /**< @ref WVR_GazeTriggerType changed; using @ref WVR_GetGazeTriggerType to get gaze trigger type. */
		WVR_EventType_TrackingModeChanged                = 1003,    /**< Notification of changing tracking mode (3 Dof/6 Dof); using @ref WVR_GetDegreeOfFreedom can get current tracking mode.*/
		WVR_EventType_RecommendedQuality_Lower           = 1004,    /**< Notification recommended quality to Lower from runtime. */
		WVR_EventType_RecommendedQuality_Higher          = 1005,    /**< Notification recommended quality to Higher from runtime. */
		WVR_EventType_HandGesture_Changed                = 1006,    /**< Notification gesture changed. */
		WVR_EventType_HandGesture_Abnormal               = 1007,    /**< Notification gesture abnormal. */
		WVR_EventType_HandTracking_Abnormal              = 1008,    /**< Notification hand tracking abnormal. */

		/** Device events region */
		WVR_EventType_DeviceConnected                    = 2000,    /**< @ref WVR_DeviceType connected. */
		WVR_EventType_DeviceDisconnected                 = 2001,    /**< @ref WVR_DeviceType disconnected. */
		WVR_EventType_DeviceStatusUpdate                 = 2002,    /**< @ref WVR_DeviceType configure changed. */
		WVR_EventType_DeviceSuspend                      = 2003,    /**< When user takes off HMD*/
		WVR_EventType_DeviceResume                       = 2004,    /**< When user puts on HMD*/
		WVR_EventType_IpdChanged                         = 2005,    /**< The interpupillary distance has been changed; using @ref WVR_GetRenderProps can get current ipd. */
		WVR_EventType_DeviceRoleChanged                  = 2006,    /**< @ref WVR_DeviceType controller roles are switched. */
		WVR_EventType_BatteryStatusUpdate                = 2007,    /**< @ref WVR_DeviceType the battery status of device has changed; using @ref WVR_GetBatteryStatus to check the current status of the battery. */
		WVR_EventType_ChargeStatusUpdate                 = 2008,    /**< @ref WVR_DeviceType the charged status of device has changed; using @ref WVR_GetChargeStatus to check the current status of the battery in use. */
		WVR_EventType_DeviceErrorStatusUpdate            = 2009,    /**< @ref WVR_DeviceType device occurs some warning; using @ref WVR_GetDeviceErrorState to get the current error status from device service. */
		WVR_EventType_BatteryTemperatureStatusUpdate     = 2010,    /**< @ref WVR_DevcieType battery temperature of device has changed; using @ref WVR_GetBatteryTemperatureStatus to get the current battery temperature. */
		WVR_EventType_RecenterSuccess                    = 2011,    /**< Notification of recenter success for 6 DoF device*/
		WVR_EventType_RecenterFail                       = 2012,    /**< Notification of recenter fail for 6 DoF device*/
		WVR_EventType_RecenterSuccess3DoF                = 2013,    /**< Notification of recenter success for 3 DoF device*/
		WVR_EventType_RecenterFail3DoF                   = 2014,    /**< Notification of recenter fail for 3 DoF device*/

		WVR_EventType_PassThroughOverlayShownBySystem    = 2100,
		WVR_EventType_PassThroughOverlayHiddenBySystem   = 2101,

		/** Input Event region */
		WVR_EventType_ButtonPressed                      = 3000,     /**< @ref WVR_InputId status change to pressed. */
		WVR_EventType_ButtonUnpressed                    = 3001,     /**< @ref WVR_InputId status change to unpressed */
		WVR_EventType_TouchTapped                        = 3002,     /**< @ref WVR_InputId status change to touched. */
		WVR_EventType_TouchUntapped                      = 3003,     /**< @ref WVR_InputId status change to untouched. */
		WVR_EventType_LeftToRightSwipe                   = 3004,     /**< Notification of swipe motion (move Left to Right) on touchpad */
		WVR_EventType_RightToLeftSwipe                   = 3005,     /**< Notification of swipe motion (move Right to Left) on touchpad */
		WVR_EventType_DownToUpSwipe                      = 3006,     /**< Notification of swipe motion (move Down to Up) on touchpad */
		WVR_EventType_UpToDownSwipe                      = 3007,     /**< Notification of swipe motion (move Up to Down) on touchpad */
	}

	public enum WVR_PeripheralQuality
	{
		Low,
		Middle,
		High,
	}

	public enum WVR_DeviceType
	{
		WVR_DeviceType_Invalid = 0,
		WVR_DeviceType_HMD = 1,
		WVR_DeviceType_Controller_Right = 2,
		WVR_DeviceType_Controller_Left = 3,
	};

	public enum WVR_RecenterType
	{
		WVR_RecenterType_Disabled = 0,
		WVR_RecenterType_YawOnly = 1,
		WVR_RecenterType_YawAndPosition = 2,
		WVR_RecenterType_RotationAndPosition = 3,
	};

	public enum WVR_InputType
	{
		WVR_InputType_Button = 1 << 0,
		WVR_InputType_Touch = 1 << 1,
		WVR_InputType_Analog = 1 << 2,
	};

	public enum WVR_BatteryStatus
	{
		WVR_BatteryStatus_Unknown = 0,
		WVR_BatteryStatus_Normal = 1,
		WVR_BatteryStatus_Low = 2, //  5% <= Battery  < 15%
		WVR_BatteryStatus_UltraLow = 3, //  Battery < 5%
	}

	public enum WVR_ChargeStatus
	{
		WVR_ChargeStatus_Unknown = 0,
		WVR_ChargeStatus_Discharging = 1,
		WVR_ChargeStatus_Charging = 2,
		WVR_ChargeStatus_Full = 3,
	}

	public enum WVR_BatteryTemperatureStatus
	{
		WVR_BatteryTemperature_Unknown = 0,
		WVR_BatteryTemperature_Normal = 1,
		WVR_BatteryTemperature_Overheat = 2,
		WVR_BatteryTemperature_UltraOverheat = 3,
	}

	public enum WVR_DeviceErrorStatus
	{
		WVR_DeviceErrorStatus_None = 0,
		WVR_DeviceErrorStatus_BatteryOverheat = 1,
		WVR_DeviceErrorStatus_BatteryOverheatRestore = 1 << 1,
		WVR_DeviceErrorStatus_BatteryOvervoltage = 1 << 2,
		WVR_DeviceErrorStatus_BatteryOvervoltageRestore = 1 << 3,
		WVR_DeviceErrorStatus_DeviceConnectFail = 1 << 4,
		WVR_DeviceErrorStatus_DeviceConnectRestore = 1 << 5,
		WVR_DeviceErrorStatus_DeviceLostTracking = 1 << 6,
		WVR_DeviceErrorStatus_DeviceLostTrackingRestore = 1 << 7,
		WVR_DeviceErrorStatus_ChargeFail = 1 << 8,
		WVR_DeviceErrorStatus_ChargeRestore = 1 << 9,
	}

	public enum WVR_DeviceErrorState
	{
		WVR_DeviceErrorState_None = 0,
		WVR_DeviceErrorState_BatteryOverheat = 1,
		WVR_DeviceErrorState_BatteryOvervoltage = 2,
		WVR_DeviceErrorState_DeviceConnectFail = 3,
		WVR_DeviceErrorState_DeviceLostTracking = 4,
		WVR_DeviceErrorState_ChargeFail = 5,
	}

	public enum WVR_InputId
	{
		WVR_InputId_0 = 0,
		WVR_InputId_1 = 1,
		WVR_InputId_2 = 2,
		WVR_InputId_3 = 3,
		WVR_InputId_4 = 4,
		WVR_InputId_5 = 5,
		WVR_InputId_6 = 6,
		WVR_InputId_7 = 7,
		WVR_InputId_8 = 8,
		WVR_InputId_9 = 9,
		WVR_InputId_14 = 14,
		WVR_InputId_15 = 15,
		WVR_InputId_16 = 16,
		WVR_InputId_17 = 17,
		WVR_InputId_18 = 18,

		//alias group mapping
		WVR_InputId_Alias1_System	   = WVR_InputId_0,
		WVR_InputId_Alias1_Menu		 = WVR_InputId_1,
		WVR_InputId_Alias1_Grip		 = WVR_InputId_2,
		WVR_InputId_Alias1_DPad_Left	= WVR_InputId_3,
		WVR_InputId_Alias1_DPad_Up	  = WVR_InputId_4,
		WVR_InputId_Alias1_DPad_Right   = WVR_InputId_5,
		WVR_InputId_Alias1_DPad_Down	= WVR_InputId_6,
		WVR_InputId_Alias1_Volume_Up	= WVR_InputId_7,
		WVR_InputId_Alias1_Volume_Down  = WVR_InputId_8,
		WVR_InputId_Alias1_Digital_Trigger = WVR_InputId_9,
		WVR_InputId_Alias1_Back		 = WVR_InputId_14,   // HMD Back Button
		WVR_InputId_Alias1_Enter		= WVR_InputId_15,   // HMD Enter Button
		WVR_InputId_Alias1_Touchpad	 = WVR_InputId_16,
		WVR_InputId_Alias1_Trigger	  = WVR_InputId_17,
		WVR_InputId_Alias1_Thumbstick   = WVR_InputId_18,

		WVR_InputId_Max = 32,
	}

	public enum WVR_AnalogType
	{
		WVR_AnalogType_None = 0,
		WVR_AnalogType_2D = 1,
		WVR_AnalogType_1D = 2,
	}

	public enum WVR_Intensity
	{
		WVR_Intensity_Weak = 1,   /**< The Intensity of vibrate is Weak. */
		WVR_Intensity_Light = 2,   /**< The Intensity of vibrate is Light. */
		WVR_Intensity_Normal = 3,   /**< The Intensity of vibrate is Normal. */
		WVR_Intensity_Strong = 4,   /**< The Intensity of vibrate is Strong. */
		WVR_Intensity_Severe = 5,   /**< The Intensity of vibrate is Severe. */
	}

	public enum WVR_PoseOriginModel
	{
		WVR_PoseOriginModel_OriginOnHead = 0,
		WVR_PoseOriginModel_OriginOnGround = 1,
		WVR_PoseOriginModel_OriginOnTrackingObserver = 2,
		WVR_PoseOriginModel_OriginOnHead_3DoF = 3,
	}

	public enum WVR_ArenaVisible
	{
		WVR_ArenaVisible_Auto = 0,  // show Arena while HMD out off bounds
		WVR_ArenaVisible_ForceOn = 1,  // always show Arena
		WVR_ArenaVisible_ForceOff = 2,  // never show Arena
	}

	public enum WVR_GraphicsApiType
	{
		WVR_GraphicsApiType_OpenGL = 1,
	}

	public enum WVR_ScreenshotMode
	{
		WVR_ScreenshotMode_Default,	  /**< Screenshot image is stereo. Just as show on screen*/
		WVR_ScreenshotMode_Raw,		  /**< Screenshot image has only single eye, and without distortion correction*/
		WVR_ScreenshotMode_Distorted
	}

	public enum WVR_SubmitError
	{
		WVR_SubmitError_None = 0,
		WVR_SubmitError_InvalidTexture = 400,
		WVR_SubmitError_ThreadStop = 401,
		WVR_SubmitError_BufferSubmitFailed = 402,
		WVR_SubmitError_Max = 65535
	}

	public enum WVR_SubmitExtend
	{
		WVR_SubmitExtend_Default = 0x0000,
		WVR_SubmitExtend_DisableDistortion = 0x0001,
		WVR_SubmitExtend_PartialTexture = 0x0010,
	}

	public enum WVR_Eye
	{
		WVR_Eye_Left = 0,
		WVR_Eye_Right = 1,
		WVR_Eye_Both = 2,
		WVR_Eye_None,
	}

	public enum WVR_TextureTarget
	{
		WVR_TextureTarget_2D,
		WVR_TextureTarget_2D_ARRAY
	}

	public enum WVR_TextureFormat
	{
		WVR_TextureFormat_RGBA
	}

	public enum WVR_TextureType
	{
		WVR_TextureType_UnsignedByte
	}

	public enum WVR_RenderError
	{
		WVR_RenderError_None = 0,
		WVR_RenderError_RuntimeInitFailed = 410,
		WVR_RenderError_ContextSetupFailed = 411,
		WVR_RenderError_DisplaySetupFailed = 412,
		WVR_RenderError_LibNotSupported = 413,
		WVR_RenderError_NullPtr = 414,
		WVR_RenderError_Max = 65535
	}

	public enum WVR_RenderConfig
	{
		WVR_RenderConfig_Default                    = 0,             /**< **WVR_RenderConfig_Default**: Runtime initialization reflects certain properties in device service. Such as single buffer mode and reprojection mechanism, the default settings are determined by device service or runtime config file on specific platform. The default color space is set as linear domain. */
		WVR_RenderConfig_Disable_SingleBuffer       = ( 1 << 0 ),    /**< **WVR_RenderConfig_Disable_SingleBuffer**: Disable single buffer mode in runtime. */
		WVR_RenderConfig_Disable_Reprojection       = ( 1 << 1 ),    /**< **WVR_RenderConfig_Disable_Reprojection**: Disable reprojection mechanism in runtime. */
		WVR_RenderConfig_sRGB                       = ( 1 << 2 ),    /**< **WVR_RenderConfig_sRGB**: Determine whether the color space is set as sRGB domain. */
	}

	public enum WVR_CameraImageType
	{
		WVR_CameraImageType_Invalid = 0,
		WVR_CameraImageType_SingleEye = 1,	 // the image is comprised of one camera
		WVR_CameraImageType_DualEye = 2,	 // the image is comprised of dual cameras
	}

	public enum WVR_CameraImageFormat
	{
		WVR_CameraImageFormat_Invalid = 0,
		WVR_CameraImageFormat_YUV_420 = 1, // the image format is YUV420
		WVR_CameraImageFormat_Grayscale = 2, // the image format is 8-bit gray-scale
	}

	public enum WVR_CameraPosition
	{
		WVR_CameraPosition_Invalid = 0,
		WVR_CameraPosition_left = 1,
		WVR_CameraPosition_Right = 2,
	}

	public enum WVR_OverlayError
	{
		WVR_OverlayError_None = 0,
		WVR_OverlayError_UnknownOverlay = 10,
		WVR_OverlayError_OverlayUnavailable = 11,
		WVR_OverlayError_InvalidParameter = 20,
	}

	public enum WVR_OverlayTransformType
	{
		WVR_OverlayTransformType_None,
		WVR_OverlayTransformType_Absolute,
		WVR_OverlayTransformType_Fixed,
	}

	public enum WVR_NumDoF
	{
		WVR_NumDoF_3DoF = 0,
		WVR_NumDoF_6DoF = 1,
	}

	public enum WVR_ArenaShape
	{
		WVR_ArenaShape_None = 0,
		WVR_ArenaShape_Rectangle = 1,
		WVR_ArenaShape_Round = 2,
	}

	public enum WVR_InteractionMode
	{
		WVR_InteractionMode_SystemDefault = 1,
		WVR_InteractionMode_Gaze = 2,
		WVR_InteractionMode_Controller = 3,
	}

	public enum WVR_GazeTriggerType
	{
		WVR_GazeTriggerType_Timeout = 1,
		WVR_GazeTriggerType_Button = 2,
		WVR_GazeTriggerType_TimeoutButton = 3,
	}

	public enum WVR_PerfLevel
	{
		WVR_PerfLevel_System = 0,			//!< System defined performance level (default)
		WVR_PerfLevel_Minimum = 1,			//!< Minimum performance level
		WVR_PerfLevel_Medium = 2,			//!< Medium performance level
		WVR_PerfLevel_Maximum = 3,			//!< Maximum performance level
		WVR_PerfLevel_NumPerfLevels
	}

	public enum WVR_RenderQuality
	{
		WVR_RenderQuality_Low = 1,		   /**< Low recommended render quality */
		WVR_RenderQuality_Medium = 2,		   /**< Medium recommended render quality */
		WVR_RenderQuality_High = 3,		   /**< High recommended render quality */
		WVR_RenderQuality_NumRenderQuality
	}

	public enum WVR_SimulationType
	{
		WVR_SimulationType_Auto = 0,
		WVR_SimulationType_ForceOn = 1,
		WVR_SimulationType_ForceOff = 2,
	}

	/**
	 * Enum containing flags indicating data valididty of an eye pose
	 */
	public enum WVR_EyePoseStatus
	{
		WVR_GazePointValid         = 1 << 0,    /**< Button input type */
		WVR_GazeVectorValid        = 1 << 1,    /**< Touch input type */
		WVR_EyeOpennessValid       = 1 << 2,    /**< Analog input type */
		WVR_EyePupilDilationValid  = 1 << 3,
		WVR_EyePositionGuideValid  = 1 << 4,
	}


	[StructLayout(LayoutKind.Sequential)]
	public struct WVR_RenderInitParams_t
	{
		public WVR_GraphicsApiType graphicsApi;
		public UInt64 renderConfig;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct WVR_Matrix4f_t
	{
		public float m0; //float[4][4]
		public float m1;
		public float m2;
		public float m3;
		public float m4;
		public float m5;
		public float m6;
		public float m7;
		public float m8;
		public float m9;
		public float m10;
		public float m11;
		public float m12;
		public float m13;
		public float m14;
		public float m15;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct WVR_Vector2f_t
	{
		public float v0;
		public float v1;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct WVR_Vector3f_t
	{
		public float v0;  // float[3]
		public float v1;
		public float v2;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct WVR_CameraIntrinsic_t
	{
		public WVR_Vector2f_t focalLength;
		public WVR_Vector2f_t principalPoint;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct WVR_CameraInfo_t
	{
		public WVR_CameraImageType imgType;	// SINGLE OR STEREO image
		public WVR_CameraImageFormat imgFormat;
		public uint width;
		public uint height;
		public uint size;	   // The buffer size for raw image data
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct WVR_Quatf_t
	{
		public float w;
		public float x;
		public float y;
		public float z;
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct WVR_PoseState_t	// [FieldOffset(164)]
	{
		[FieldOffset(0)] public bool IsValidPose;
		[FieldOffset(4)] public WVR_Matrix4f_t PoseMatrix;
		[FieldOffset(68)] public WVR_Vector3f_t Velocity;
		[FieldOffset(80)] public WVR_Vector3f_t AngularVelocity;
		[FieldOffset(92)] public bool Is6DoFPose;
		[FieldOffset(96)] public long PoseTimestamp_ns;
		[FieldOffset(104)] public WVR_Vector3f_t Acceleration;
		[FieldOffset(116)] public WVR_Vector3f_t AngularAcceleration;
		[FieldOffset(128)] public float PredictedMilliSec;
		[FieldOffset(132)] public WVR_PoseOriginModel OriginModel;
		[FieldOffset(136)] public WVR_Pose_t RawPose;
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct WVR_DevicePosePair_t
	{
		[FieldOffset(0)] public WVR_DeviceType type;
		[FieldOffset(8)] public WVR_PoseState_t pose;
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct WVR_TextureLayout_t
	{
		[FieldOffset(0)] public WVR_Vector2f_t leftLowUVs;
		[FieldOffset(8)] public WVR_Vector2f_t rightUpUVs;
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct WVR_TextureBound_t
	{
		[FieldOffset(0)] public float uMin;
		[FieldOffset(4)] public float vMin;
		[FieldOffset(8)] public float uMax;
		[FieldOffset(12)] public float vMax;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct WVR_TextureParams_t
	{
		public IntPtr id;
		public WVR_TextureTarget target;
		public WVR_TextureLayout_t layout;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct WVR_RenderProps_t
	{
		public float refreshRate;
		public bool hasExternal;
		public float ipdMeter;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct WVR_CommonEvent_t
	{
		public WVR_EventType type;
		public long timestamp;		 // Delivered time in nanoseconds
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct WVR_DeviceEvent_t
	{
		public WVR_CommonEvent_t common;
		public WVR_DeviceType type;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct WVR_InputEvent_t
	{
		public WVR_DeviceEvent_t device;
		public WVR_InputId inputId;
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct WVR_Event_t
	{
		[FieldOffset(0)] public WVR_CommonEvent_t common;
		[FieldOffset(0)] public WVR_DeviceEvent_t device;
		[FieldOffset(0)] public WVR_InputEvent_t input;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct WVR_Axis_t
	{
		public float x;
		public float y;
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct WVR_AnalogState_t
	{
		[FieldOffset(0)] public WVR_InputId id;
		[FieldOffset(4)] public WVR_AnalogType type;
		[FieldOffset(8)] public WVR_Axis_t axis;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct WVR_InputAttribute_t
	{
		public WVR_InputId id;
		public uint capability;
		public WVR_AnalogType axis_type;

	}

	[StructLayout(LayoutKind.Sequential)]
	public struct WVR_InputMappingPair_t
	{
		public WVR_InputAttribute_t destination;
		public WVR_InputAttribute_t source;
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct WVR_Pose_t	// [FieldOffset(28)]
	{
		[FieldOffset(0)] public WVR_Vector3f_t position;
		[FieldOffset(12)] public WVR_Quatf_t rotation;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct WVR_OverlayPosition_t
	{
		public float x;
		public float y;
		public float z;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct WVR_OverlayBlendColor_t
	{
		public float r;
		public float g;
		public float b;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct WVR_OverlayTexture_t
	{
		public uint textureId;
		public uint width;
		public uint height;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct WVR_ArenaRectangle_t
	{
		public float width;
		public float length;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct WVR_ArenaRound_t
	{
		public float diameter;
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct WVR_ArenaArea_t
	{
		[FieldOffset(0)] public WVR_ArenaRectangle_t rectangle;
		[FieldOffset(0)] public WVR_ArenaRound_t round;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct WVR_Arena_t
	{
		public WVR_ArenaShape shape;
		public WVR_ArenaArea_t area;
	}

	public delegate void WVR_OverlayInputEventCallback(int overlayId, WVR_EventType type, WVR_InputId inputId);
	[StructLayout(LayoutKind.Sequential)]
	public struct WVR_OverlayInputEvent_t
	{
		public int overlayId;
		public IntPtr callback;
	}

	public struct WVR_RenderFoveationParams
	{
		public float focalX;
		public float focalY;
		public float fovealFov;
		public WVR_PeripheralQuality periQuality;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct FBXInfo_t
	{
		//public char* name;
		public WVR_Matrix4f_t matrix;
		public uint verticeCount;
		public uint normalCount;
		public uint uvCount;
		public uint indiceCount;
		public IntPtr meshName;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct MeshInfo_t
	{
		public Vector3[] _vectice;
		public Vector3[] _normal;
		public Vector2[] _uv;
		public int[] _indice;
		public bool _active;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct StencilMesh_t
	{
		public uint vertCount;	  // uint32_t
		public IntPtr vertData;	 // float*
		public uint triCount;	   // uint32_t
		public IntPtr indexData;	// uint16_t*
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct WVR_RequestResult
	{
		public string mPermission;
		public bool mGranted;
	}

	/**
	 * The eye pose data
	 */
	[StructLayout(LayoutKind.Sequential)]
	public struct WVR_EyeData_t
	{
		public int     leftEyeDataStatus;
		public int     rightEyeDataStatus;
		public int     combinedEyeDataStatus;
		public WVR_Vector3f_t leftEyeGazePoint;
		public WVR_Vector3f_t rightEyeGazePoint;
		public WVR_Vector3f_t combinedEyeGazePoint;
		public WVR_Vector3f_t leftEyeGazeVector;
		public WVR_Vector3f_t rightEyeGazeVector;
		public WVR_Vector3f_t combinedEyeGazeVector;
		public float   leftEyeOpenness;
		public float   rightEyeOpenness;
		public float   leftEyePupilDilation;
		public float   rightEyePupilDilation;
		public WVR_Vector3f_t leftEyePositionGuide;
		public WVR_Vector3f_t rightEyePositionGuide;
		public long    timestamp;
	}

	public enum WVR_EyeDataStatus
	{
		WVR_GazePointValid         = 1<<0,    /**< Button input type */
		WVR_GazeVectorValid        = 1<<1,    /**< Touch input type */
		WVR_EyeOpennessValid       = 1<<2,    /**< Analog input type */
		WVR_EyePupilDilationValid  = 1<<3,
		WVR_EyePositionGuideValid  = 1<<4
	}

	public enum WVR_QualityStrategy
	{
		WVR_QualityStrategy_Default = 1,                    /**< Auto adjust CPU/GPU performane level if need. */
		WVR_QualityStrategy_SendQualityEvent = 1 << 1,      /**< Send recommended quality changed event if need. */
		WVR_QualityStrategy_AutoFoveation = 1 << 2,         /**< Auto adjust foveation rendering intensity if need. */
		WVR_QualityStrategy_Reserved = 1 << 30,             /**< System reserved. */
	}

	/**
	 * @brief the returned result of a function call for providing the information of failure.
	 */
	public enum WVR_Result
	{
		WVR_Success                              = 0,    /**< The result of the function call was successful. */
		WVR_Error_SystemInvalid                  = 1,    /**< The initialization was not finished or the feature was not started yet. */
		WVR_Error_InvalidArgument                = 2,    /**< One of the arguments was not appropriate for the function call. */
		WVR_Error_OutOfMemory                    = 3,    /**< A memory allocation has failed. */
		WVR_Error_FeatureNotSupport              = 4,    /**< The feature was not supported; either lack of some services or service does not support this feature. */
		WVR_Error_RuntimeVersionNotSupport       = 5,    /**< The runtime version is too old to support the function call. */
	}


	#region Gesture
	public enum WVR_HandGestureType
	{
		WVR_HandGestureType_Invalid         = 0,    /**< The gesture is invalid. */
		WVR_HandGestureType_Unknown         = 1,    /**< Unknow gesture type. */
		WVR_HandGestureType_Fist            = 2,    /**< Represent fist gesture. */
		WVR_HandGestureType_Five            = 3,    /**< Represent five gesture. */
		WVR_HandGestureType_OK              = 4,    /**< Represent ok gesture. */
		WVR_HandGestureType_ThumbUp         = 5,    /**< Represent thumb up gesture. */
		WVR_HandGestureType_IndexUp         = 6,    /**< Represent index up gesture. */
		WVR_HandGestureType_Pinch           = 7,    /**< Represent pinch gesture. */
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct WVR_HandGestureData_t
	{
		public long timestamp;     
		public WVR_HandGestureType right;
		public WVR_HandGestureType left;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct WVR_SingleFinger_t
	{
		public WVR_Vector3f_t joint1;
		public WVR_Vector3f_t joint2;
		public WVR_Vector3f_t joint3;
		public WVR_Vector3f_t tip;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct WVR_Fingers_t
	{
		public WVR_SingleFinger_t    thumb;
		public WVR_SingleFinger_t    index;
		public WVR_SingleFinger_t    middle;
		public WVR_SingleFinger_t    ring;
		public WVR_SingleFinger_t    pinky;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct WVR_HandTrackingData_t
	{
		public WVR_PoseState_t         right;                      /**< tracking data of right hand */
		public WVR_Fingers_t           rightFinger;
		public WVR_PoseState_t         left;                       /**< tracking data of left hand */
		public WVR_Fingers_t           leftFinger;
	}
	#endregion

	public enum WVR_SupportedFeature {
		WVR_SupportedFeature_PassthroughImage   = 1 << 0,
		WVR_SupportedFeature_PassthroughOverlay = 1 << 1,
		WVR_SupportedFeature_HandTracking       = 1 << 4,
		WVR_SupportedFeature_HandGesture        = 1 << 5,
	}

	public delegate void WVR_RequestCompleteCallback(List<WVR_RequestResult> results);
	public delegate void WVR_RequestUsbCompleteCallback(bool result);
	public delegate void WVR_OnOEMConfigChanged();

	public class Interop
	{
		#region Interaction
		public static bool WVR_PollEventQueue(ref WVR_Event_t e)
		{
			return WVR_Base.Instance.PollEventQueue(ref e);
		}

		public static int WVR_GetInputDeviceCapability(WVR_DeviceType type, WVR_InputType inputType)
		{
			return WVR_Base.Instance.GetInputDeviceCapability(type, inputType);
		}

		public static WVR_AnalogType WVR_GetInputDeviceAnalogType(WVR_DeviceType type, WVR_InputId id)
		{
			return WVR_Base.Instance.GetInputDeviceAnalogType(type, id);
		}

		public static bool WVR_GetInputDeviceState(WVR_DeviceType type, uint inputMask, ref uint buttons, ref uint touches,
			[In, Out] WVR_AnalogState_t[] analogArray, uint analogArrayCount)
		{
			return WVR_Base.Instance.GetInputDeviceState(type, inputMask, ref buttons, ref touches, analogArray, analogArrayCount);
		}

		public static int WVR_GetInputTypeCount(WVR_DeviceType type, WVR_InputType inputType)
		{
			return WVR_Base.Instance.GetInputTypeCount(type, inputType);
		}

		public static bool WVR_GetInputButtonState(WVR_DeviceType type, WVR_InputId id)
		{
			return WVR_Base.Instance.GetInputButtonState(type, id);
		}

		public static bool WVR_GetInputTouchState(WVR_DeviceType type, WVR_InputId id)
		{
			return WVR_Base.Instance.GetInputTouchState(type, id);
		}

		public static WVR_Axis_t WVR_GetInputAnalogAxis(WVR_DeviceType type, WVR_InputId id)
		{
			return WVR_Base.Instance.GetInputAnalogAxis(type, id);
		}

		public static void WVR_GetPoseState(WVR_DeviceType type, WVR_PoseOriginModel originModel, uint predictedMilliSec, ref WVR_PoseState_t poseState)
		{
			WVR_Base.Instance.GetPoseState(type, originModel, predictedMilliSec, ref poseState);
		}

		public static void WVR_SetTextureBounds([In, Out] WVR_TextureBound_t[] textureBounds)
		{
			WVR_Base.Instance.SetTextureBounds(textureBounds);
		}

		public static void WVR_GetLastPoseIndex(WVR_PoseOriginModel originModel, [In, Out] WVR_DevicePosePair_t[] poseArray, uint pairArrayCount, ref uint frameIndex)
		{
			WVR_Base.Instance.GetLastPoseIndex(originModel, poseArray, pairArrayCount, ref frameIndex);
		}
		public static void WVR_WaitGetPoseIndex(WVR_PoseOriginModel originModel, [In, Out] WVR_DevicePosePair_t[] poseArray, uint pairArrayCount, ref uint frameIndex)
		{
			WVR_Base.Instance.WaitGetPoseIndex(originModel, poseArray, pairArrayCount, ref frameIndex);
		}
		public static System.IntPtr WVR_StoreRenderTextures(System.IntPtr[] texturesIDs, int size, bool eEye, WVR_TextureTarget target)
		{
			return WVR_Base.Instance.StoreRenderTextures(texturesIDs, size, eEye, target);
		}

		public static void WVR_GetSyncPose(WVR_PoseOriginModel originModel, [In, Out] WVR_DevicePosePair_t[] poseArray, uint pairArrayCount)
		{
			WVR_Base.Instance.GetSyncPose(originModel, poseArray, pairArrayCount);
		}

		public static bool WVR_IsDeviceConnected(WVR_DeviceType type)
		{
			return WVR_Base.Instance.IsDeviceConnected(type);
		}

		public static void WVR_TriggerVibration(WVR_DeviceType type, WVR_InputId id, uint durationMicroSec, uint frequency, WVR_Intensity intensity)
		{
			WVR_Base.Instance.TriggerVibration(type, id, durationMicroSec, frequency, intensity);
		}

		public static void WVR_InAppRecenter(WVR_RecenterType recenterType)
		{
			WVR_Base.Instance.InAppRecenter(recenterType);
		}

		public static void WVR_SetNeckModelEnabled(bool enabled)
		{
			WVR_Base.Instance.SetNeckModelEnabled(enabled);
		}

		public static void WVR_SetNeckModel(WVR_SimulationType simulationType)
		{
			WVR_Base.Instance.SetNeckModel(simulationType);
		}

		public static void WVR_SetArmModel(WVR_SimulationType simulationType)
		{
			WVR_Base.Instance.SetArmModel(simulationType);
		}

		public static void WVR_SetArmSticky(bool stickyArm)
		{
			WVR_Base.Instance.SetArmSticky(stickyArm);
		}

		public static bool WVR_SetInputRequest(WVR_DeviceType type, WVR_InputAttribute_t[] request, uint size)
		{
			return WVR_Base.Instance.SetInputRequest(type, request, size);
		}

		public static bool WVR_GetInputMappingPair(WVR_DeviceType type, WVR_InputId destination, ref WVR_InputMappingPair_t pair)
		{
			return WVR_Base.Instance.GetInputMappingPair(type, destination, ref pair);
		}

		public static uint WVR_GetInputMappingTable(WVR_DeviceType type, [In, Out] WVR_InputMappingPair_t[] table, uint size)
		{
			return WVR_Base.Instance.GetInputMappingTable(type, table, size);
		}

		public static WVR_Arena_t WVR_GetArena()
		{
			return WVR_Base.Instance.GetArena();
		}

		public static bool WVR_SetArena(ref WVR_Arena_t arena)
		{
			return WVR_Base.Instance.SetArena(ref arena);
		}

		public static WVR_ArenaVisible WVR_GetArenaVisible()
		{
			return WVR_Base.Instance.GetArenaVisible();
		}

		public static void WVR_SetArenaVisible(WVR_ArenaVisible config)
		{
			WVR_Base.Instance.SetArenaVisible(config);
		}

		public static bool WVR_IsOverArenaRange()
		{
			return WVR_Base.Instance.IsOverArenaRange();
		}

		public static float WVR_GetDeviceBatteryPercentage(WVR_DeviceType type)
		{
			return WVR_Base.Instance.GetDeviceBatteryPercentage(type);
		}

		public static WVR_BatteryStatus WVR_GetBatteryStatus(WVR_DeviceType type)
		{
			return WVR_Base.Instance.GetBatteryStatus(type);
		}

		public static WVR_ChargeStatus WVR_GetChargeStatus(WVR_DeviceType type)
		{
			return WVR_Base.Instance.GetChargeStatus(type);
		}

		public static WVR_BatteryTemperatureStatus WVR_GetBatteryTemperatureStatus(WVR_DeviceType type)
		{
			return WVR_Base.Instance.GetBatteryTemperatureStatus(type);
		}

		public static float WVR_GetBatteryTemperature(WVR_DeviceType type)
		{
			return WVR_Base.Instance.GetBatteryTemperature(type);
		}
		#endregion

		#region Gesture
		public static WVR_Result WVR_StartHandGesture()
		{
			return WVR_Base.Instance.StartHandGesture ();
		}

		public static void WVR_StopHandGesture()
		{
			WVR_Base.Instance.StopHandGesture ();
		}

		public static WVR_Result WVR_GetHandGestureData(ref WVR_HandGestureData_t data)
		{
			return WVR_Base.Instance.GetHandGestureData (ref data);
		}

		public static WVR_Result WVR_StartHandTracking()
		{
			return WVR_Base.Instance.StartHandTracking ();
		}

		public static void WVR_StopHandTracking()
		{
			WVR_Base.Instance.StopHandTracking ();
		}

		public static WVR_Result WVR_GetHandTrackingData(ref WVR_HandTrackingData_t data, WVR_PoseOriginModel originModel, uint predictedMilliSec)
		{
			return WVR_Base.Instance.GetHandTrackingData (ref data, originModel, predictedMilliSec);
		}
		#endregion

		public static ulong WVR_GetSupportedFeatures()
		{
			return WVR_Base.Instance.GetSupportedFeatures ();
		}

		public static WVR_InitError WVR_Init(WVR_AppType eType)
		{
			return WVR_Base.Instance.Init(eType);
		}

		public static void WVR_PostInit()
		{
			WVR_Base.Instance.PostInit();
		}

		public static void WVR_Quit()
		{
			WVR_Base.Instance.Quit();
		}

		public static IntPtr WVR_GetInitErrorString(WVR_InitError error)
		{
			return WVR_Base.Instance.GetInitErrorString(error);
		}

		public static uint WVR_GetWaveRuntimeVersion()
		{
			return WVR_Base.Instance.GetWaveRuntimeVersion();
		}

		public static uint WVR_GetWaveSDKVersion()
		{
			return WVR_Base.Instance.GetWaveSDKVersion();
		}

		public static bool WVR_IsInputFocusCapturedBySystem()
		{
			return WVR_Base.Instance.IsInputFocusCapturedBySystem();
		}

		internal static WVR_RenderError WVR_RenderInit(ref WVR_RenderInitParams_t param)
		{
			return WVR_Base.Instance.RenderInit(ref param);
		}

		public static bool WVR_SetPerformanceLevels(WVR_PerfLevel cpuLevel, WVR_PerfLevel gpuLevel)
		{
			return WVR_Base.Instance.SetPerformanceLevels(cpuLevel, gpuLevel);
		}

		public static bool WVR_EnableAdaptiveQuality(bool enable, uint flags)
		{
			return WVR_Base.Instance.EnableAdaptiveQuality(enable, flags);
		}

		public static bool WVR_IsAdaptiveQualityEnabled()
		{
			return WVR_Base.Instance.IsAdaptiveQualityEnabled();
		}

		public static bool WVR_StartCamera(ref WVR_CameraInfo_t info)
		{
			return WVR_Base.Instance.StartCamera(ref info);
		}

		public static void WVR_StopCamera()
		{
			WVR_Base.Instance.StopCamera();
		}

		public static bool WVR_UpdateTexture(IntPtr textureid)
		{
			return WVR_Base.Instance.UpdateTexture(textureid);
		}

		public static bool WVR_GetCameraIntrinsic(WVR_CameraPosition position, ref WVR_CameraIntrinsic_t intrinsic)
		{
			return WVR_Base.Instance.GetCameraIntrinsic(position, ref intrinsic);
		}

		public static bool WVR_GetCameraFrameBuffer(IntPtr pFramebuffer, uint frameBufferSize)
		{
			return WVR_Base.Instance.GetCameraFrameBuffer(pFramebuffer, frameBufferSize);
		}

		public static bool WVR_GetFrameBufferWithPoseState(IntPtr frameBuffer, uint frameBufferSize, WVR_PoseOriginModel origin, uint predictInMs, ref WVR_PoseState_t poseState)
		{
			return WVR_Base.Instance.GetFrameBufferWithPoseState(frameBuffer, frameBufferSize, origin, predictInMs, ref poseState);
		}

		public static bool WVR_DrawTextureWithBuffer(IntPtr textureId, WVR_CameraImageFormat imgFormat, IntPtr frameBuffer, uint size, uint width, uint height)
		{
			return WVR_Base.Instance.DrawTextureWithBuffer(textureId, imgFormat, frameBuffer, size, width, height);
		}

		public static void WVR_ReleaseCameraTexture()
		{
			WVR_Base.Instance.ReleaseCameraTexture();
		}

		public static bool WVR_IsDeviceSuspend(WVR_DeviceType type)
		{
			return WVR_Base.Instance.IsDeviceSuspend(type);
		}

		public static void WVR_ConvertMatrixQuaternion(ref WVR_Matrix4f_t mat, ref WVR_Quatf_t quat, bool m2q)
		{
			WVR_Base.Instance.ConvertMatrixQuaternion(ref mat, ref quat, m2q);
		}

		public static WVR_NumDoF WVR_GetDegreeOfFreedom(WVR_DeviceType type)
		{
			return WVR_Base.Instance.GetDegreeOfFreedom(type);
		}

		public static void WVR_SetParameters(WVR_DeviceType type, IntPtr pchValue)
		{
			WVR_Base.Instance.SetParameters(type, pchValue);
		}

		public static uint WVR_GetParameters(WVR_DeviceType type, IntPtr pchValue, IntPtr retValue, uint unBufferSize)
		{
			return WVR_Base.Instance.GetParameters(type, pchValue, retValue, unBufferSize);
		}

		public static WVR_DeviceType WVR_GetDefaultControllerRole()
		{
			return WVR_Base.Instance.GetDefaultControllerRole();
		}

		public static bool WVR_SetInteractionMode(WVR_InteractionMode mode)
		{
			return WVR_Base.Instance.SetInteractionMode(mode);
		}

		public static WVR_InteractionMode WVR_GetInteractionMode()
		{
			return WVR_Base.Instance.GetInteractionMode();
		}

		public static bool WVR_SetGazeTriggerType(WVR_GazeTriggerType type)
		{
			return WVR_Base.Instance.SetGazeTriggerType(type);
		}

		public static WVR_GazeTriggerType WVR_GetGazeTriggerType()
		{
			return WVR_Base.Instance.GetGazeTriggerType();
		}

		public static bool WVR_GetDeviceErrorState(WVR_DeviceType dev_type, WVR_DeviceErrorState error_state)
		{
			return WVR_Base.Instance.GetDeviceErrorState(dev_type, error_state);
		}

		public static void WVR_GetRenderTargetSize(ref uint width, ref uint height)
		{
			WVR_Base.Instance.GetRenderTargetSize(ref width, ref height);
		}

		public static WVR_Matrix4f_t WVR_GetProjection(WVR_Eye eye, float near, float far)
		{
			return WVR_Base.Instance.GetProjection(eye, near, far);
		}

		public static void WVR_GetClippingPlaneBoundary(WVR_Eye eye, ref float left, ref float right, ref float top, ref float bottom)
		{
			WVR_Base.Instance.GetClippingPlaneBoundary(eye, ref left, ref right, ref top, ref bottom);
		}

		public static void WVR_SetOverfillRatio(float ratioX, float ratioY)
		{
			WVR_Base.Instance.SetOverfillRatio(ratioX, ratioY);
		}

		public static WVR_Matrix4f_t WVR_GetTransformFromEyeToHead(WVR_Eye eye, WVR_NumDoF dof)
		{
			return WVR_Base.Instance.GetTransformFromEyeToHead(eye, dof);
		}

		public static WVR_SubmitError WVR_SubmitFrame(WVR_Eye eye, [In, Out] WVR_TextureParams_t[] param, [In, Out] WVR_PoseState_t[] pose, WVR_SubmitExtend extendMethod)
		{
			return WVR_Base.Instance.SubmitFrame(eye, param, pose, extendMethod);
		}

		public static void WVR_SetSubmitParams(WVR_Eye eye, [In, Out] WVR_TextureParams_t[] param, [In, Out] WVR_PoseState_t[] pose, WVR_SubmitExtend extendMethod)
		{
			WVR_Base.Instance.SetSubmitParams(eye, param, pose, extendMethod);
		}

		public static void WVR_PreRenderEye(WVR_Eye eye, [In, Out] WVR_TextureParams_t[] param, [In, Out] WVR_RenderFoveationParams[] foveationParams)
		{
			WVR_Base.Instance.PreRenderEye(eye, param, foveationParams);
		}


		public static bool WVR_RequestScreenshot(uint width, uint height, WVR_ScreenshotMode mode, IntPtr filename)
		{
			return WVR_Base.Instance.RequestScreenshot(width, height, mode, filename);
		}

		public static void WVR_RenderMask(WVR_Eye eye)
		{
			WVR_Base.Instance.RenderMask(eye);
		}

		public static bool WVR_GetRenderProps(ref WVR_RenderProps_t props)
		{
			return WVR_Base.Instance.GetRenderProps(ref props);
		}

		public static IntPtr WVR_ObtainTextureQueue(WVR_TextureTarget target, WVR_TextureFormat format, WVR_TextureType type, uint width, uint height, int level)
		{
			return WVR_Base.Instance.ObtainTextureQueue(target, format, type, width, height, level);
		}

		public static uint WVR_GetTextureQueueLength(IntPtr handle)
		{
			return WVR_Base.Instance.GetTextureQueueLength(handle);
		}

		public static WVR_TextureParams_t WVR_GetTexture(IntPtr handle, int index)
		{
			return WVR_Base.Instance.GetTexture(handle, index);
		}

		public static int WVR_GetAvailableTextureIndex(IntPtr handle)
		{
			return WVR_Base.Instance.GetAvailableTextureIndex(handle);
		}

		public static void WVR_ReleaseTextureQueue(IntPtr handle)
		{
			WVR_Base.Instance.ReleaseTextureQueue(handle);
		}

		public static bool WVR_IsRenderFoveationSupport()
		{
			return WVR_Base.Instance.IsRenderFoveationSupport();
		}

		public static void WVR_RenderFoveation(bool enable)
		{
			WVR_Base.Instance.RenderFoveation(enable);
		}

		public static bool WVR_IsPermissionInitialed()
		{
			return WVR_Base.Instance.IsPermissionInitialed();
		}

		public static bool WVR_ShowDialogOnScene()
		{
			return WVR_Base.Instance.ShowDialogOnScene();
		}

		public static bool WVR_IsPermissionGranted(string permission)
		{
			return WVR_Base.Instance.IsPermissionGranted(permission);
		}

		public static bool WVR_ShouldGrantPermission(string permission)
		{
			return WVR_Base.Instance.ShouldGrantPermission(permission);
		}

		public static void WVR_RequestPermissions(string[] permissions, WVR_RequestCompleteCallback cb)
		{
			WVR_Base.Instance.RequestPermissions(permissions, cb);
		}

		public static void WVR_RequestUsbPermission(WVR_RequestUsbCompleteCallback cb)
		{
			WVR_Base.Instance.RequestUsbPermission(cb);
		}



		public static string WVR_GetStringBySystemLanguage(string stringName)
		{
			return WVR_Base.Instance.GetStringBySystemLanguage(stringName);
		}

		public static string WVR_GetStringByLanguage(string stringName, string lang, string country)
		{
			return WVR_Base.Instance.GetStringByLanguage(stringName, lang, country);
		}

		public static string WVR_GetSystemLanguage()
		{
			return WVR_Base.Instance.GetSystemLanguage();
		}

		public static string WVR_GetSystemCountry()
		{
			return WVR_Base.Instance.GetSystemCountry();
		}

		public static void WVR_SetPosePredictEnabled(WVR_DeviceType type, bool enabled_position_predict, bool enable_rotation_predict)
		{
			WVR_Base.Instance.SetPosePredictEnabled(type, enabled_position_predict, enable_rotation_predict);
		}

		public static bool WVR_ShowPassthroughOverlay(bool show)
		{
			return WVR_Base.Instance.ShowPassthroughOverlay(show);
		}

		public static void WVR_EnableAutoPassthrough(bool enable)
		{
			WVR_Base.Instance.EnableAutoPassthrough(enable);
		}

		public static bool WVR_IsPassthroughOverlayVisible()
		{
			return WVR_Base.Instance.IsPassthroughOverlayVisible();
		}

		#region Internal
		public static string WVR_DeployRenderModelAssets(int deviceIndex, string renderModelName)
		{
			return WVR_Base.Instance.DeployRenderModelAssets(deviceIndex, renderModelName);
		}

		public static void WVR_SetFocusedController(WVR_DeviceType focusController)
		{
			WVR_Base.Instance.SetFocusedController(focusController);
		}

		public static WVR_DeviceType WVR_GetFocusedController()
		{
			return WVR_Base.Instance.GetFocusedController();
		}

		public static bool WVR_OpenMesh(string filename, ref uint sessionid, IntPtr errorCode, bool merge)
		{
			return WVR_Base.Instance.OpenMesh(filename, ref sessionid, errorCode, merge);
		}

		public static bool WVR_GetSectionCount(uint sessionid, ref uint sectionCount)
		{
			return WVR_Base.Instance.GetSectionCount(sessionid, ref sectionCount);
		}

		public static bool WVR_GetMeshData(uint sessionid, [In, Out] FBXInfo_t[] infoArray)
		{
			return WVR_Base.Instance.GetMeshData(sessionid, infoArray);
		}

		public static bool WVR_GetSectionData(uint sessionid, uint sectionIndiceIndex, [In, Out] Vector3[] vecticeArray, [In, Out] Vector3[] normalArray, [In, Out] Vector2[] uvArray, [In, Out] int[] indiceArray, ref bool active)
		{
			return WVR_Base.Instance.GetSectionData(sessionid, sectionIndiceIndex, vecticeArray, normalArray, uvArray, indiceArray, ref active);
		}

		public static void WVR_ReleaseMesh(uint sessiionid)
		{
			WVR_Base.Instance.ReleaseMesh(sessiionid);
		}

		public static string WVR_GetOEMConfigByKey(string key)
		{
			return WVR_Base.Instance.GetOEMConfigByKey(key);
		}

		public static void WVR_SetOEMConfigChangedCallback(WVR_OnOEMConfigChanged cb)
		{
			WVR_Base.Instance.SetOEMConfigChangedCallback(cb);
		}
		#endregion
		public class WVR_Base
		{
			private static WVR_Base instance = null;
			public static WVR_Base Instance
			{
				get
				{
					if (instance == null)
					{
#if !UNITY_EDITOR && UNITY_ANDROID
						instance = new WVR_Android();
#elif UNITY_STANDALONE
						instance = new WVR_HVR();
#elif UNITY_EDITOR && UNITY_ANDROID
						WaveVR.EnableSimulator = EditorPrefs.GetBool("WaveVR/DirectPreview/Enable Direct Preview", false);
						if (WaveVR.EnableSimulator)
							instance = new WVR_DirectPreview();
						else
						instance = new WVR_Editor();
#else
						instance = new WVR_Base();
#endif
					}
					return instance;
				}
			}

			#region Interaction
			// ------------- wvr_events.h -------------
			// Events: swipe, battery status.
			public virtual bool PollEventQueue(ref WVR_Event_t e)
			{
				return false;
			}

			// ------------- wvr_device.h -------------
			// Button types for which device is capable.
			public virtual int GetInputDeviceCapability(WVR_DeviceType type, WVR_InputType inputType)
			{
				return 0;
			}

			// Get analog type for which device.
			public virtual WVR_AnalogType GetInputDeviceAnalogType(WVR_DeviceType type, WVR_InputId id)
			{
				return WVR_AnalogType.WVR_AnalogType_None;
			}

			// Button press and touch state.
			public virtual bool GetInputDeviceState(WVR_DeviceType type, uint inputMask, ref uint buttons, ref uint touches,
				[In, Out] WVR_AnalogState_t[] analogArray, uint analogArrayCount)
			{
				return false;
			}

			// Count of specified button type.
			public virtual int GetInputTypeCount(WVR_DeviceType type, WVR_InputType inputType)
			{
				return 0;
			}

			// Button press state.
			public virtual bool GetInputButtonState(WVR_DeviceType type, WVR_InputId id)
			{
				return false;
			}

			// Button touch state.
			public virtual bool GetInputTouchState(WVR_DeviceType type, WVR_InputId id)
			{
				return false;
			}

			// Axis of analog button: touchpad (x, y), trigger (x only)
			public virtual WVR_Axis_t GetInputAnalogAxis(WVR_DeviceType type, WVR_InputId id)
			{
				WVR_Axis_t _T = new WVR_Axis_t();
				_T.x = 0.0f;
				_T.y = 0.0f;
				return _T;
			}

			public virtual void SetTextureBounds([In, Out] WVR_TextureBound_t[] textureBounds)
			{

			}

			// Get transform of specified device.
			public virtual void GetPoseState(WVR_DeviceType type, WVR_PoseOriginModel originModel, uint predictedMilliSec, ref WVR_PoseState_t poseState)
			{
			}
			public virtual void GetLastPoseIndex(WVR_PoseOriginModel originModel, [In, Out] WVR_DevicePosePair_t[] poseArray, uint pairArrayCount, ref uint frameIndex)
			{

			}
			public virtual void WaitGetPoseIndex(WVR_PoseOriginModel originModel, [In, Out] WVR_DevicePosePair_t[] poseArray, uint pairArrayCount, ref uint frameIndex)
			{

			}
			public virtual System.IntPtr StoreRenderTextures(System.IntPtr[] texturesIDs, int size, bool eEye, WVR_TextureTarget target)
			{
				return System.IntPtr.Zero;
			}

			// Get all attributes of pose of all devices.
			public virtual void GetSyncPose(WVR_PoseOriginModel originModel, [In, Out] WVR_DevicePosePair_t[] poseArray, uint pairArrayCount)
			{
			}

			// Device connection state.
			public virtual bool IsDeviceConnected(WVR_DeviceType type)
			{
				return false;
			}

			// Make device vibration.
			public virtual void TriggerVibration(WVR_DeviceType type, WVR_InputId id, uint durationMicroSec, uint frequency, WVR_Intensity intensity)
			{
			}

			// Recenter the "Virtual World" in current App.
			public virtual void InAppRecenter(WVR_RecenterType recenterType)
			{
			}

			// Enables or disables use of the neck model for 3-DOF head tracking
			public virtual void SetNeckModelEnabled(bool enabled)
			{
			}

			// Decide Neck Model on/off/3dofOn
			public virtual void SetNeckModel(WVR_SimulationType simulationType)
			{
			}

			// Decide Arm Model on/off/3dofOn
			public virtual void SetArmModel(WVR_SimulationType simulationType)
			{
			}

			// Decide Arm Model behaviors
			public virtual void SetArmSticky(bool stickyArm)
			{
			}

			// bool WVR_SetInputRequest(WVR_DeviceType type, const WVR_InputAttribute* request, uint32_t size);
			public virtual bool SetInputRequest(WVR_DeviceType type, WVR_InputAttribute_t[] request, uint size)
			{
				return false;
			}

			// bool WVR_GetInputMappingPair(WVR_DeviceType type, WVR_InputId destination, WVR_InputMappingPair* pair);
			public virtual bool GetInputMappingPair(WVR_DeviceType type, WVR_InputId destination, ref WVR_InputMappingPair_t pair)
			{
				return false;
			}

			// uint32_t WVR_GetInputMappingTable(WVR_DeviceType type, WVR_InputMappingPair* table, uint32_t size);
			public virtual uint GetInputMappingTable(WVR_DeviceType type, [In, Out] WVR_InputMappingPair_t[] table, uint size)
			{
				return 0;
			}

			// ------------- wvr_arena.h -------------
			// Get current attributes of arena.
			public virtual WVR_Arena_t GetArena()
			{
				WVR_Arena_t _T = new WVR_Arena_t();
				return _T;
			}

			// Set up arena.
			public virtual bool SetArena(ref WVR_Arena_t arena)
			{
				return false;
			}

			// Get visibility type of arena.
			public virtual WVR_ArenaVisible GetArenaVisible()
			{
				return WVR_ArenaVisible.WVR_ArenaVisible_Auto;
			}

			// Set visibility type of arena.
			public virtual void SetArenaVisible(WVR_ArenaVisible config)
			{
			}

			// Check if player is over range of arena.
			public virtual bool IsOverArenaRange()
			{
				return false;
			}

			// ------------- wvr_status.h -------------
			// Battery electricity (%).
			public virtual float GetDeviceBatteryPercentage(WVR_DeviceType type)
			{
				return 1.0f;
			}

			// Battery life status.
			public virtual WVR_BatteryStatus GetBatteryStatus(WVR_DeviceType type)
			{
				return WVR_BatteryStatus.WVR_BatteryStatus_Normal;
			}

			// Battery is charging or not.
			public virtual WVR_ChargeStatus GetChargeStatus(WVR_DeviceType type)
			{
				return WVR_ChargeStatus.WVR_ChargeStatus_Full;
			}

			// Whether battery is overheat.
			public virtual WVR_BatteryTemperatureStatus GetBatteryTemperatureStatus(WVR_DeviceType type)
			{
				return WVR_BatteryTemperatureStatus.WVR_BatteryTemperature_Normal;
			}

			// Battery temperature.
			public virtual float GetBatteryTemperature(WVR_DeviceType type)
			{
				return 0.0f;
			}
			#endregion

			#region Gesture
			public virtual WVR_Result StartHandGesture()
			{
				return WVR_Result.WVR_Error_FeatureNotSupport;
			}

			public virtual void StopHandGesture()
			{
			}

			public virtual WVR_Result GetHandGestureData(ref WVR_HandGestureData_t data)
			{
				data.timestamp = 0;
				data.right = WVR_HandGestureType.WVR_HandGestureType_Invalid;
				data.left = WVR_HandGestureType.WVR_HandGestureType_Invalid;
				return WVR_Result.WVR_Error_FeatureNotSupport;
			}

			public virtual WVR_Result StartHandTracking()
			{
				return WVR_Result.WVR_Error_FeatureNotSupport;
			}

			public virtual void StopHandTracking()
			{
			}

			public virtual WVR_Result GetHandTrackingData(ref WVR_HandTrackingData_t data, WVR_PoseOriginModel originModel, uint predictedMilliSec)
			{
				return WVR_Result.WVR_Error_FeatureNotSupport;
			}
			#endregion

			public virtual ulong GetSupportedFeatures()
			{
				return (
					(ulong)WVR_SupportedFeature.WVR_SupportedFeature_PassthroughImage |
					(ulong)WVR_SupportedFeature.WVR_SupportedFeature_PassthroughOverlay|
					(ulong)WVR_SupportedFeature.WVR_SupportedFeature_HandTracking |
					(ulong)WVR_SupportedFeature.WVR_SupportedFeature_HandGesture
				);
			}

			// wvr.h
			public virtual WVR_InitError Init(WVR_AppType eType)
			{
				return WVR_InitError.WVR_InitError_None;
			}

			public virtual void PostInit()
			{

			}

			public virtual void Quit()
			{
			}

			public virtual IntPtr GetInitErrorString(WVR_InitError error)
			{
				IntPtr t  = new IntPtr();
				return t;
			}

			public virtual uint GetWaveRuntimeVersion()
			{
				return 1;
			}

			public virtual uint GetWaveSDKVersion()
			{
				return 1;
			}

			// wvr_system.h
			public virtual bool IsInputFocusCapturedBySystem()
			{
				return false;
			}

			internal virtual WVR_RenderError RenderInit(ref WVR_RenderInitParams_t param)
			{
				return WVR_RenderError.WVR_RenderError_None;
			}

			// Set CPU and GPU performance level.
			internal virtual bool SetPerformanceLevels(WVR_PerfLevel cpuLevel, WVR_PerfLevel gpuLevel)
			{
				return true;
			}

			// Allow WaveVR SDK runtime to adjust render quality and CPU/GPU perforamnce level automatically.
			internal virtual bool EnableAdaptiveQuality(bool enable, uint flags)
			{
				return true;
			}

			// Check if adaptive quailty enabled.
			internal virtual bool IsAdaptiveQualityEnabled()
			{
				return false;
			}

			// wvr_camera.h
			public virtual bool StartCamera(ref WVR_CameraInfo_t info)
			{
				return false;
			}

			public virtual void StopCamera()
			{
			}

			public virtual bool UpdateTexture(IntPtr textureid)
			{
				return false;
			}

			public virtual bool GetCameraIntrinsic(WVR_CameraPosition position, ref WVR_CameraIntrinsic_t intrinsic)
			{
				return true;
			}

			public virtual bool GetCameraFrameBuffer(IntPtr pFramebuffer, uint frameBufferSize)
			{
				return false;
			}

			public virtual bool GetFrameBufferWithPoseState(IntPtr frameBuffer, uint frameBufferSize, WVR_PoseOriginModel origin, uint predictInMs, ref WVR_PoseState_t poseState)
			{
				return false;
			}

			public virtual bool DrawTextureWithBuffer(IntPtr textureId, WVR_CameraImageFormat imgFormat, IntPtr frameBuffer, uint size, uint width, uint height)
			{
				return false;
			}

			public virtual void ReleaseCameraTexture()
			{
			}

			// wvr_device.h
			public virtual bool IsDeviceSuspend(WVR_DeviceType type)
			{
				return false;
			}

			public virtual void ConvertMatrixQuaternion(ref WVR_Matrix4f_t mat, ref WVR_Quatf_t quat, bool m2q)
			{
			}

			public virtual WVR_NumDoF GetDegreeOfFreedom(WVR_DeviceType type)
			{
				return WVR_NumDoF.WVR_NumDoF_3DoF;
			}

			public virtual void SetParameters(WVR_DeviceType type, IntPtr pchValue)
			{
			}

			public virtual uint GetParameters(WVR_DeviceType type, IntPtr pchValue, IntPtr retValue, uint unBufferSize)
			{
				return 0;
			}

			public virtual WVR_DeviceType GetDefaultControllerRole()
			{
				return WVR_DeviceType.WVR_DeviceType_Invalid;
			}

			public virtual bool SetInteractionMode(WVR_InteractionMode mode)
			{
				return true;
			}

			public virtual WVR_InteractionMode GetInteractionMode()
			{
				return WVR_InteractionMode.WVR_InteractionMode_Controller;
			}

			public virtual bool SetGazeTriggerType(WVR_GazeTriggerType type)
			{
				return true;
			}

			public virtual WVR_GazeTriggerType GetGazeTriggerType()
			{
				return WVR_GazeTriggerType.WVR_GazeTriggerType_TimeoutButton;
			}

			public virtual bool GetDeviceErrorState(WVR_DeviceType dev_type, WVR_DeviceErrorState error_state)
			{
				return false; ;
			}

			// TODO
			public virtual void GetRenderTargetSize(ref uint width, ref uint height)
			{
			}

			public virtual WVR_Matrix4f_t GetProjection(WVR_Eye eye, float near, float far)
			{
				WVR_Matrix4f_t _T = new WVR_Matrix4f_t();
				return _T;
			}

			public virtual void GetClippingPlaneBoundary(WVR_Eye eye, ref float left, ref float right, ref float top, ref float bottom)
			{
			}

			public virtual void SetOverfillRatio(float ratioX, float ratioY)
			{
			}

			public virtual WVR_Matrix4f_t GetTransformFromEyeToHead(WVR_Eye eye, WVR_NumDoF dof)
			{
				WVR_Matrix4f_t _T = new WVR_Matrix4f_t();
				return _T;
			}

			public virtual WVR_SubmitError SubmitFrame(WVR_Eye eye, [In, Out] WVR_TextureParams_t[] param, [In, Out] WVR_PoseState_t[] pose, WVR_SubmitExtend extendMethod)
			{
				return WVR_SubmitError.WVR_SubmitError_None;
			}

			public virtual void SetSubmitParams(WVR_Eye eye, [In, Out] WVR_TextureParams_t[] param, [In, Out] WVR_PoseState_t[] pose, WVR_SubmitExtend extendMethod)
			{
			}

			public virtual void PreRenderEye(WVR_Eye eye, [Out] WVR_TextureParams_t[] param, [Out] WVR_RenderFoveationParams[] foveationParams)
			{
			}

			public virtual bool RequestScreenshot(uint width, uint height, WVR_ScreenshotMode mode, IntPtr filename)
			{
				return true;
			}

			public virtual void RenderMask(WVR_Eye eye)
			{
			}

			public virtual bool GetRenderProps(ref WVR_RenderProps_t props)
			{
				return true;
			}

			public virtual IntPtr ObtainTextureQueue(WVR_TextureTarget target, WVR_TextureFormat format, WVR_TextureType type, uint width, uint height, int level)
			{
				return new IntPtr();
			}

			public virtual uint GetTextureQueueLength(IntPtr handle)
			{
				return 0;
			}

			public virtual WVR_TextureParams_t GetTexture(IntPtr handle, int index)
			{
				return new WVR_TextureParams_t();
			}

			public virtual int GetAvailableTextureIndex(IntPtr handle)
			{
				return -1;
			}

			public virtual void ReleaseTextureQueue(IntPtr handle)
			{
			}

			public virtual bool IsRenderFoveationSupport()
			{
				return false;
			}

			public virtual void RenderFoveation(bool enable)
			{
			}

			public virtual void SetPosePredictEnabled(WVR_DeviceType type, bool enabled_position_predict, bool enable_rotation_predict)
			{
			}

			public virtual bool ShowPassthroughOverlay(bool show)
			{
				return false;
			}

			public virtual void EnableAutoPassthrough(bool enable)
			{

			}

			public virtual bool IsPassthroughOverlayVisible()
			{
				return false;
			}

			#region Internal
			public virtual string DeployRenderModelAssets(int deviceIndex, string renderModelName)
			{
				return "";
			}

			public virtual void SetFocusedController(WVR_DeviceType focusController)
			{

			}

			public virtual WVR_DeviceType GetFocusedController()
			{
				return WVR_DeviceType.WVR_DeviceType_Controller_Right;
			}

			public virtual bool OpenMesh(string filename, ref uint sessiionid, IntPtr errorCode, bool merge)
			{
				return false;
			}

			public virtual bool GetSectionCount(uint sessionid, ref uint sectionCount)
			{
				return false;
			}

			public virtual bool GetMeshData(uint sessionid, [In, Out] FBXInfo_t[] infoArray)
			{
				return false;
			}

			public virtual bool GetSectionData(uint sessionid, uint sectionIndiceIndex, [In, Out] Vector3[] vecticeArray, [In, Out] Vector3[] normalArray, [In, Out] Vector2[] uvArray, [In, Out] int[] indiceArray, ref bool active)
			{
				return false;
			}

			public virtual void ReleaseMesh(uint sessionid)
			{

			}

			public virtual bool IsPermissionInitialed()
			{
				return true;
			}

			public virtual bool ShowDialogOnScene()
			{
				return true;
			}

			public virtual bool IsPermissionGranted(string permission)
			{
				return true;
			}

			public virtual bool ShouldGrantPermission(string permission)
			{
				return false;
			}

			public virtual void RequestPermissions(string[] permissions, WVR_RequestCompleteCallback cb)
			{
				List<WVR_RequestResult> permissionResults = new List<WVR_RequestResult>();

				if (permissions != null)
				{
					for (int i = 0; i < permissions.Length; i++)
					{
						WVR_RequestResult rr;
						rr.mPermission = permissions[i];
						rr.mGranted = true;
						permissionResults.Add(rr);
					}
				}

				cb(permissionResults);
			}

			public virtual void RequestUsbPermission(WVR_RequestUsbCompleteCallback cb)
			{
				cb(true);
			}



			public virtual string GetStringBySystemLanguage(string stringName)
			{
				return stringName;
			}

			public virtual string GetStringByLanguage(string stringName, string lang, string country)
			{
				return stringName;
			}

			public virtual string GetSystemLanguage()
			{
				return "";
			}

			public virtual string GetSystemCountry()
			{
				return "";
			}

			public virtual string GetOEMConfigByKey(string key)
			{
				return "";
			}

			public virtual void SetOEMConfigChangedCallback(WVR_OnOEMConfigChanged cb)
			{

			}
			#endregion
		}
	}
} // namespace wvr
