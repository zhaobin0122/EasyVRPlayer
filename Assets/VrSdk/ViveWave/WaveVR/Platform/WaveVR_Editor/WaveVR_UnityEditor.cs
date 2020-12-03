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
using System.Runtime.InteropServices;
using System;

#if UNITY_EDITOR
public class WaveVR_UnityEditor : MonoBehaviour
{
	private const string LOG_TAG = "WaveVR_UnityEditor";
	private void DEBUG(string msg)
	{
		if (Log.EnableDebugLog)
			Log.d (LOG_TAG, msg, true);
	}
	private void ERROR(string msg)
	{
		Log.e (LOG_TAG, msg, true);
	}

	public static WaveVR_UnityEditor Instance
	{
		get {
			if (instance == null)
			{
				if (instance == null)
				{
					var gameObject = new GameObject ("WaveVRUnityEditor");
					instance = gameObject.AddComponent<WaveVR_UnityEditor> ();
					// This object should survive all scene transitions.
					GameObject.DontDestroyOnLoad (instance);
				}
			}
			return instance;
		}
	}
	private static WaveVR_UnityEditor instance = null;

	#region Variables
	private WVR_Event_t mEvent = new WVR_Event_t();
	private bool hasEvent = false;

	private const string MOUSE_X = "Mouse X";
	private const string MOUSE_Y = "Mouse Y";
	private const string MOUSE_SCROLLWHEEL = "Mouse ScrollWheel";

	// =================== Button Events ===============================
	static WVR_InputId[] pressIds = new WVR_InputId[] {
		WVR_InputId.WVR_InputId_Alias1_System,
		WVR_InputId.WVR_InputId_Alias1_Menu,
		WVR_InputId.WVR_InputId_Alias1_Grip,
		WVR_InputId.WVR_InputId_Alias1_DPad_Left,
		WVR_InputId.WVR_InputId_Alias1_DPad_Up,
		WVR_InputId.WVR_InputId_Alias1_DPad_Right,
		WVR_InputId.WVR_InputId_Alias1_DPad_Down,
		WVR_InputId.WVR_InputId_Alias1_Volume_Up,
		WVR_InputId.WVR_InputId_Alias1_Volume_Down,
		WVR_InputId.WVR_InputId_Alias1_Digital_Trigger,
		WVR_InputId.WVR_InputId_Alias1_Back,
		WVR_InputId.WVR_InputId_Alias1_Enter,
		WVR_InputId.WVR_InputId_Alias1_Touchpad,
		WVR_InputId.WVR_InputId_Alias1_Trigger,
		WVR_InputId.WVR_InputId_Alias1_Thumbstick
	};

	bool[] state_press_right = new bool[pressIds.Length];
	bool[] state_press_left = new bool[pressIds.Length];

	static WVR_InputId[] touchIds = new WVR_InputId[] {
		WVR_InputId.WVR_InputId_Alias1_Touchpad,
		WVR_InputId.WVR_InputId_Alias1_Trigger,
		WVR_InputId.WVR_InputId_Alias1_Thumbstick
	};

	bool[] state_touch_right = new bool[touchIds.Length];
	private WVR_Axis_t rightAxis;

	bool[] state_touch_left = new bool[touchIds.Length];
	private WVR_Axis_t leftAxis;
	private const float leftAxisX = 0, leftAxisY = 1;

	private bool mFocusIsCapturedBySystem = false;
	private float mFPS = 60.0f;
	#endregion

	#region Monobehaviour overrides
	void OnEnable()
	{
		DEBUG ("OnEnable()");

		rightAxis.x = 0;
		rightAxis.y = 0;

		InitializeBonesAndHandTrackingData ();
		InitHandGesture ();
	}

	void Start()
	{
		DEBUG ("Start()");
		Cursor.visible = false;
	}

	private float xAxis = 0, yAxis = 0, zAxis = 0;
	private float xOffset = 0, yOffset = 0, zOffset = 0;
	void Update ()
	{
		mFPS = 1.0f / Time.deltaTime;

		ButtonPressed (WVR_DeviceType.WVR_DeviceType_Controller_Right);
		ButtonPressed (WVR_DeviceType.WVR_DeviceType_Controller_Left);
		ButtonUnpressed (WVR_DeviceType.WVR_DeviceType_Controller_Right);
		ButtonUnpressed (WVR_DeviceType.WVR_DeviceType_Controller_Left);
		TouchTapped (WVR_DeviceType.WVR_DeviceType_Controller_Left);
		TouchUntapped (WVR_DeviceType.WVR_DeviceType_Controller_Left);
		SideToSideSwipe ();

		xAxis = Input.GetAxis (MOUSE_X);
		yAxis = Input.GetAxis (MOUSE_Y);
		zAxis = Input.GetAxis (MOUSE_SCROLLWHEEL);
		float axis_x = xAxis + xOffset;
		float axis_y = yAxis + yOffset;
		float axis_z = zAxis + zOffset;
		UpdateHeadPose (axis_x, axis_y, axis_z);
		UpdateRightPose (axis_x, axis_y, axis_z);
		UpdateLefHandPose (axis_x, axis_y, axis_z);
		SetDevicePosePairHead ();
		SetDevicePosePairRight ();
		SetDevicePosePairLeft ();

		// Gesture
		UpdateBonesAndHandTrackingData();
		UpdateHandGesture ();
	}
	#endregion

	#region [Event] Polling
	private void clearEventQueue()
	{
		hasEvent = false;
		mEvent.device.type = WVR_DeviceType.WVR_DeviceType_Invalid;
		mEvent.input.inputId = WVR_InputId.WVR_InputId_Max;
	}

	public bool PollEventQueue(ref WVR_Event_t e)
	{
		// Get current state
		bool ret = hasEvent;
		e = mEvent;
		// Clear current state after poll queue.
		clearEventQueue();

		return ret;
	}
	#endregion

	#region [Event] Buttons
	private bool IsButtonAvailable(WVR_DeviceType device, WVR_InputId button)
	{
		if (device == WVR_DeviceType.WVR_DeviceType_HMD && inputTable_Hmd != null)
		{
			for (int i = 0; i < inputTable_Hmd.Length; i++)
			{
				if (inputTable_Hmd [i].destination.id == button)
					return true;
			}
		}
		if (device == WVR_DeviceType.WVR_DeviceType_Controller_Right && inputTable_Right != null)
		{
			for (int i = 0; i < inputTable_Right.Length; i++)
			{
				if (inputTable_Right [i].destination.id == button)
					return true;
			}
		}
		if (device == WVR_DeviceType.WVR_DeviceType_Controller_Left && inputTable_Left != null)
		{
			for (int i = 0; i < inputTable_Left.Length; i++)
			{
				if (inputTable_Left [i].destination.id == button)
					return true;
			}
		}
		return false;
	}

	private void ButtonPressed(WVR_DeviceType type)
	{
		switch (type)
		{
		case WVR_DeviceType.WVR_DeviceType_Controller_Right:
			if (IsButtonAvailable (type, WVR_InputId.WVR_InputId_Alias1_Touchpad))
			{
				if (Input.GetMouseButtonDown (1))   // right mouse key
				{
					DEBUG ("ButtonPressed() " + type + ", touchpad.");
					mEvent.common.type = WVR_EventType.WVR_EventType_ButtonPressed;
					mEvent.device.type = type;
					mEvent.input.inputId = WVR_InputId.WVR_InputId_Alias1_Touchpad;
					hasEvent = true;
				}
			}

			if (IsButtonAvailable (type, WVR_InputId.WVR_InputId_Alias1_Trigger))
			{
				if (Input.GetKeyDown (KeyCode.T))
				{
					DEBUG ("ButtonPressed() " + type + ", trigger.");
					mEvent.common.type = WVR_EventType.WVR_EventType_ButtonPressed;
					mEvent.device.type = type;
					mEvent.input.inputId = WVR_InputId.WVR_InputId_Alias1_Trigger;
					hasEvent = true;
				}
			}

			for (int _p = 0; _p < pressIds.Length; _p++)
			{
				if (pressIds [_p] == mEvent.input.inputId)
				{
					state_press_right [_p] = true;
					break;
				}
			}
			break;
		case WVR_DeviceType.WVR_DeviceType_Controller_Left:
			if (IsButtonAvailable (type, WVR_InputId.WVR_InputId_Alias1_Trigger))
			{
				if (Input.GetKeyDown (KeyCode.R))
				{
					DEBUG ("ButtonPressed() " + type + ", trigger.");
					mEvent.common.type = WVR_EventType.WVR_EventType_ButtonPressed;
					mEvent.device.type = type;
					mEvent.input.inputId = WVR_InputId.WVR_InputId_Alias1_Trigger;
					hasEvent = true;
				}
			}

			for (int _p = 0; _p < pressIds.Length; _p++)
			{
				if (pressIds [_p] == mEvent.input.inputId)
				{
					state_press_left [_p] = true;
					break;
				}
			}
			break;
		default:
			break;
		}
	}

	private void ButtonUnpressed(WVR_DeviceType type)
	{
		switch (type)
		{
		case WVR_DeviceType.WVR_DeviceType_Controller_Right:
			if (IsButtonAvailable (type, WVR_InputId.WVR_InputId_Alias1_Touchpad))
			{
				if (Input.GetMouseButtonUp (1))	 // right mouse key
				{
					DEBUG ("ButtonUnpressed() " + type + ", touchpad.");
					mEvent.common.type = WVR_EventType.WVR_EventType_ButtonUnpressed;
					mEvent.device.type = type;
					mEvent.input.inputId = WVR_InputId.WVR_InputId_Alias1_Touchpad;
					hasEvent = true;
				}
			}

			if (IsButtonAvailable (type, WVR_InputId.WVR_InputId_Alias1_Trigger))
			{
				if (Input.GetKeyUp (KeyCode.T))
				{
					DEBUG ("ButtonUnpressed() " + type + ", trigger.");
					mEvent.common.type = WVR_EventType.WVR_EventType_ButtonUnpressed;
					mEvent.device.type = type;
					mEvent.input.inputId = WVR_InputId.WVR_InputId_Alias1_Trigger;
					hasEvent = true;
				}
			}

			for (int _p = 0; _p < pressIds.Length; _p++)
			{
				if (pressIds [_p] == mEvent.input.inputId)
				{
					state_press_right [_p] = false;
					break;
				}
			}
			break;
		case WVR_DeviceType.WVR_DeviceType_Controller_Left:
			if (IsButtonAvailable (type, WVR_InputId.WVR_InputId_Alias1_Trigger))
			{
				if (Input.GetKeyUp (KeyCode.R))
				{
					DEBUG ("ButtonUnpressed() " + type + ", trigger.");
					mEvent.common.type = WVR_EventType.WVR_EventType_ButtonUnpressed;
					mEvent.device.type = type;
					mEvent.input.inputId = WVR_InputId.WVR_InputId_Alias1_Trigger;
					hasEvent = true;
				}
			}

			for (int _p = 0; _p < pressIds.Length; _p++)
			{
				if (pressIds [_p] == mEvent.input.inputId)
				{
					state_press_left [_p] = false;
					break;
				}
			}
			break;
		default:
			break;
		}
	}

	public bool GetInputButtonState(WVR_DeviceType type, WVR_InputId id)
	{
		bool pressed = false;
		switch (type)
		{
		case WVR_DeviceType.WVR_DeviceType_Controller_Right:
			for (int _p = 0; _p < pressIds.Length; _p++)
			{
				if (id == pressIds [_p])
				{
					pressed = state_press_right [_p];
					break;
				}
			}
			break;
		case WVR_DeviceType.WVR_DeviceType_Controller_Left:
			for (int _p = 0; _p < pressIds.Length; _p++)
			{
				if (id == pressIds [_p])
				{
					pressed = state_press_left [_p];
					break;
				}
			}
			break;
		default:
			break;
		}
		return pressed;
	}

	private void TouchTapped(WVR_DeviceType type)
	{
		switch (type)
		{
		case WVR_DeviceType.WVR_DeviceType_Controller_Left:
			if (IsButtonAvailable (type, WVR_InputId.WVR_InputId_Alias1_Touchpad))
			{
				if (Input.GetMouseButtonDown (0))   // left  mouse key
				{
					mEvent.common.type = WVR_EventType.WVR_EventType_TouchTapped;
					mEvent.device.type = type;
					mEvent.input.inputId = WVR_InputId.WVR_InputId_Alias1_Touchpad;
					hasEvent = true;

					for (int _t = 0; _t < touchIds.Length; _t++)
					{
						if (touchIds [_t] == mEvent.input.inputId)
						{
							state_touch_left [_t] = true;
							break;
						}
					}
					leftAxis.x = leftAxisX;
					leftAxis.y = leftAxisY;
					DEBUG ("TouchTapped() " + type + ", touchpad" + ", axis (" + leftAxis.x + ", " + leftAxis.y + ")");
				}
			}
			break;
		default:
			break;
		}
	}

	private void TouchUntapped(WVR_DeviceType type)
	{
		switch (type)
		{
		case WVR_DeviceType.WVR_DeviceType_Controller_Left:
			if (IsButtonAvailable (type, WVR_InputId.WVR_InputId_Alias1_Touchpad))
			{
				if (Input.GetMouseButtonUp (0))	 // left  mouse key
				{
					DEBUG ("TouchUntapped() " + type + ", touchpad.");
					mEvent.common.type = WVR_EventType.WVR_EventType_TouchUntapped;
					mEvent.device.type = type;
					mEvent.input.inputId = WVR_InputId.WVR_InputId_Alias1_Touchpad;
					hasEvent = true;

					for (int _t = 0; _t < touchIds.Length; _t++)
					{
						if (touchIds [_t] == mEvent.input.inputId)
						{
							state_touch_left [_t] = false;
							break;
						}
					}
					leftAxis.x = 0;
					leftAxis.y = 0;
				}
			}
			break;
		default:
			break;
		}
	}

	public bool GetInputTouchState(WVR_DeviceType type, WVR_InputId id)
	{
		bool touched = false;
		switch (type)
		{
		case WVR_DeviceType.WVR_DeviceType_Controller_Right:
			for (int _t = 0; _t < touchIds.Length; _t++)
			{
				if (id == touchIds [_t])
				{
					touched = state_touch_right [_t];
					break;
				}
			}
			break;
		case WVR_DeviceType.WVR_DeviceType_Controller_Left:
			for (int _t = 0; _t < touchIds.Length; _t++)
			{
				if (id == touchIds [_t])
				{
					touched = state_touch_left [_t];
					break;
				}
			}
			break;
		default:
			break;
		}
		return touched;
	}

	public WVR_Axis_t GetInputAnalogAxis(WVR_DeviceType type, WVR_InputId button)
	{
		WVR_Axis_t axis2d;
		axis2d.x = 0;
		axis2d.y = 0;

		switch (type)
		{
		case WVR_DeviceType.WVR_DeviceType_Controller_Left:
			axis2d = leftAxis;
			break;
		case WVR_DeviceType.WVR_DeviceType_Controller_Right:
			axis2d = rightAxis;
			break;
		default:
			break;
		}

		return axis2d;
	}
	#endregion

	#region [Event] Swipe
	private void SideToSideSwipe ()
	{
		if (Input.GetKeyUp (KeyCode.W))	 // Down To Up Swipe
		{
			DEBUG ("SideToSideSwipe() WVR_EventType_DownToUpSwipe.");
			mEvent.common.type = WVR_EventType.WVR_EventType_DownToUpSwipe;
			mEvent.device.type = WVR_DeviceType.WVR_DeviceType_Controller_Right;
			mEvent.input.inputId = WVR_InputId.WVR_InputId_Alias1_Touchpad;
			hasEvent = true;
		}
		if (Input.GetKeyUp (KeyCode.A))	 // Right To Left Swipe
		{
			DEBUG ("SideToSideSwipe() WVR_EventType_RightToLeftSwipe.");
			mEvent.common.type = WVR_EventType.WVR_EventType_RightToLeftSwipe;
			mEvent.device.type = WVR_DeviceType.WVR_DeviceType_Controller_Right;
			mEvent.input.inputId = WVR_InputId.WVR_InputId_Alias1_Touchpad;
			hasEvent = true;
		}
		if (Input.GetKeyUp (KeyCode.S))	 // Up To Down Swipe
		{
			DEBUG ("SideToSideSwipe() WVR_EventType_UpToDownSwipe.");
			mEvent.common.type = WVR_EventType.WVR_EventType_UpToDownSwipe;
			mEvent.device.type = WVR_DeviceType.WVR_DeviceType_Controller_Right;
			mEvent.input.inputId = WVR_InputId.WVR_InputId_Alias1_Touchpad;
			hasEvent = true;
		}
		if (Input.GetKeyUp (KeyCode.D))	 // Left To Right Swipe
		{
			DEBUG ("SideToSideSwipe() WVR_EventType_LeftToRightSwipe.");
			mEvent.common.type = WVR_EventType.WVR_EventType_LeftToRightSwipe;
			mEvent.device.type = WVR_DeviceType.WVR_DeviceType_Controller_Right;
			mEvent.input.inputId = WVR_InputId.WVR_InputId_Alias1_Touchpad;
			hasEvent = true;
		}
	}
	#endregion

	#region Device Pose
	// =================== Pose ===============================
	// Head position variables.
	private WVR_DevicePosePair_t posePairHead = new WVR_DevicePosePair_t();
	private WVR_DevicePosePair_t posePairRight = new WVR_DevicePosePair_t();
	private WVR_DevicePosePair_t posePairLeft = new WVR_DevicePosePair_t();

	private bool is6DoFPose = false;

	private Vector3 defaultHeadPosition = Vector3.zero;
	private Vector3 headPosition = Vector3.zero;
	private float headPosX = 0, headPosY = 0, headPosZ = 0;
	private readonly Vector3 CENTER_EYE_POSITION = new Vector3 (0, 0.15f, 0.12f);
	private Vector3 NECK_OFFSET = new Vector3(0, 0.15f, -0.08f);
	// Head rotation variables.
	private Quaternion headRotation = Quaternion.identity;
	private float headAngleX = 0, headAngleY = 0, headAngleZ = 0;
	// Head RigidTransform and Pose Matirx
	private WaveVR_Utils.RigidTransform headRigidTransform = WaveVR_Utils.RigidTransform.identity;
	private WVR_Matrix4f_t headPoseMatrix;

	// Right position variables.
	private const float shiftSpeed_Right = 10.0f;
	private Vector3 rightPosition = Vector3.zero;
	private float rightPosX = 0, rightPosY = 0, rightPosZ = 0;
	// Right rotation variables.
	private Quaternion rightRotation = Quaternion.identity;
	private float rightAngleX = 0, rightAngleY = 0, rightAngleZ = 0;
	// Right RigidTransform and Pose Matirx
	private WaveVR_Utils.RigidTransform rightRigidTransform = WaveVR_Utils.RigidTransform.identity;
	private WVR_Matrix4f_t rightPoseMatrix;

	// Left position variables.
	private Vector3 leftPosition = Vector3.zero;
	private float leftPosX = 0, leftPosY = 0, leftPosZ = 0;
	// Left rotation variables.
	private Quaternion leftRotation = Quaternion.identity;
	private float leftAngleX = 0, leftAngleY = 0, leftAngleZ = 0;
	// Right RigidTransform and Pose Matirx
	private WaveVR_Utils.RigidTransform leftRigidTransform = WaveVR_Utils.RigidTransform.identity;
	private WVR_Matrix4f_t leftPoseMatrix;

	// Position simulation variables.
	private Quaternion bodyRotation = Quaternion.identity;
	private Vector3 bodyDirection = Vector3.zero;
	private const float BodyAngleBound = 0.01f;
	private const float BodyAngleLimitation = 0.3f; // bound of controller angle in SPEC provided to provider.
	private uint framesOfFreeze = 0;				// if framesOfFreeze >= mFPS, means controller freezed.
	private Vector3 v3ChangeArmXAxis = new Vector3(0, 1, 1);
	private readonly Vector3 HEADTOELBOW_OFFSET = new Vector3(0.2f, -0.7f, 0f);
	private readonly Vector3 ELBOW_PITCH_OFFSET = new Vector3(-0.2f, 0.55f, 0.08f);
	private readonly Vector3 ELBOW_RAISE_OFFSET = new Vector3 (0, 0, 0);
	private const float ELBOW_PITCH_ANGLE_MIN = 0;
	private const float ELBOW_PITCH_ANGLE_MAX = 60;
	private const float ELBOW_TO_XYPLANE_LERP_MIN = 0.45f;
	private const float ELBOW_TO_XYPLANE_LERP_MAX = 0.65f;
	private readonly Vector3 ELBOWTOWRIST_OFFSET = new Vector3(0.0f, 0.0f, 0.15f);
	private readonly Vector3 WRISTTOCONTROLLER_OFFSET = new Vector3(0.0f, 0.0f, 0.05f);
	/// controller lerp speed for smooth movement between with head position case and without head position case
	private float smoothMoveSpeed = 0.3f;
	private Vector3 controllerSimulatedPosition = Vector3.zero;
	//private Quaternion controllerSimulatedRotation = Quaternion.identity;

	private WVR_PoseOriginModel hmdOriginModel = WVR_PoseOriginModel.WVR_PoseOriginModel_OriginOnHead;
	public void GetSyncPose(WVR_PoseOriginModel origin, WVR_DevicePosePair_t[] poseArray, uint pairArrayCount)
	{
		hmdOriginModel = origin;
		for (uint i = 0; i < pairArrayCount; i++)
		{
			WVR_DeviceType _type = (WVR_DeviceType)(i + 1);
			switch (_type)
			{
			case WVR_DeviceType.WVR_DeviceType_HMD:
				poseArray [i] = posePairHead;
				break;
			case WVR_DeviceType.WVR_DeviceType_Controller_Right:
				poseArray [i] = posePairRight;
				break;
			case WVR_DeviceType.WVR_DeviceType_Controller_Left:
				poseArray [i] = posePairLeft;
				break;
			default:
				break;
			}
		}
	}

	private void UpdateHeadPose(float axis_x, float axis_y, float axis_z)
	{
		if (Input.GetKey (KeyCode.LeftAlt))
		{
			headAngleX -= axis_y * 2.4f;
			headAngleX = Mathf.Clamp (headAngleX, -89, 89);
			headAngleY += axis_x * 5;
			if (headAngleY <= -180)
			{
				headAngleY += 360;
			} else if (headAngleY > 180)
			{
				headAngleY -= 360;
			}
		}
		if (Input.GetKey (KeyCode.LeftControl))
		{
			headAngleZ += axis_x * 5;
			headAngleZ = Mathf.Clamp (headAngleZ, -89, 89);
		}

		if (Input.GetKey (KeyCode.LeftShift))
		{
			headPosX += axis_x / 5;
			headPosY += axis_y / 5;
			headPosZ += axis_z;
		}

		headPosition.x = headPosX;
		headPosition.y = headPosY;
		headPosition.z = headPosZ;
		if (hmdOriginModel == WVR_PoseOriginModel.WVR_PoseOriginModel_OriginOnHead_3DoF && enableNeckModel)
		{
			headPosition = ApplyNeckToHead (headPosition);
		}
		headPosition.y = hmdOriginModel == WVR_PoseOriginModel.WVR_PoseOriginModel_OriginOnGround ? headPosition.y + 1.75f : headPosition.y;

		headRotation = Quaternion.Euler (headAngleX, headAngleY, headAngleZ);
		headRigidTransform.update (headPosition, headRotation);
		headPoseMatrix = GetOpenGLMatrix44 (headPosition, headRotation);
	}

	private void UpdateRightPose(float axis_x, float axis_y, float axis_z)
	{
		// Right-Alt + mouse for x & y angle.
		if (Input.GetKey (KeyCode.RightAlt))
		{
			rightAngleY += axis_x / 2;
			rightAngleX -= axis_y * 1.5f;
		}
		// Right-Ctrl + mouse for z angle.
		if (Input.GetKey (KeyCode.RightControl))
		{
			rightAngleZ += axis_z * 5;
		}
		rightRotation = Quaternion.Euler (rightAngleX, rightAngleY, rightAngleZ);

		// Right-Shift + mouse for position.
		if (Input.GetKey (KeyCode.RightShift))
		{
			rightPosX += axis_x / 5;
			rightPosY += axis_y / 5;
			rightPosZ += axis_z;
		}

		//-------- keyboard control ---------
		if (Input.GetKey (KeyCode.RightArrow)){ rightPosX += shiftSpeed_Right * Time.deltaTime; }
		if (Input.GetKey (KeyCode.LeftArrow)) { rightPosX -= shiftSpeed_Right * Time.deltaTime; }
		if (Input.GetKey (KeyCode.UpArrow))   { rightPosY += shiftSpeed_Right * Time.deltaTime; }
		if (Input.GetKey (KeyCode.DownArrow)) { rightPosY -= shiftSpeed_Right * Time.deltaTime; }

		if (simulationType == WVR_SimulationType.WVR_SimulationType_ForceOff
			|| (simulationType == WVR_SimulationType.WVR_SimulationType_Auto && is6DoFPose == true)
		)
		{
			rightPosition.x = rightPosX;
			rightPosition.y = rightPosY;
			rightPosition.z = rightPosZ;
		}
		if (simulationType == WVR_SimulationType.WVR_SimulationType_ForceOn
			|| (simulationType == WVR_SimulationType.WVR_SimulationType_Auto && is6DoFPose == false)
		)
		{
			updateControllerPose (WVR_DeviceType.WVR_DeviceType_Controller_Right, rightRigidTransform);
			rightPosition = Vector3.Lerp (rightPosition, controllerSimulatedPosition, smoothMoveSpeed);
		}

		rightRigidTransform.update (rightPosition, rightRotation);
		rightPoseMatrix = GetOpenGLMatrix44 (rightPosition, rightRotation);
	}

	private void UpdateLefHandPose(float axis_x, float axis_y, float axis_z)
	{
		//-------- mouse control ---------
		if (Input.GetKey (KeyCode.C))
		{
			leftAngleY += axis_x / 2;
			leftAngleX -= (float)(axis_y * 1.5f);
		}
		if (Input.GetKey (KeyCode.X))
		{
			leftAngleZ += axis_z * 5;
		}
		leftRotation = Quaternion.Euler (leftAngleX, leftAngleY, leftAngleZ);

		if (Input.GetKey (KeyCode.Z))
		{
			leftPosX += axis_x / 5;
			leftPosY += axis_y / 5;
			leftPosZ += axis_z;
		}
		if (simulationType == WVR_SimulationType.WVR_SimulationType_ForceOff
			|| (simulationType == WVR_SimulationType.WVR_SimulationType_Auto && is6DoFPose == true)
		)
		{
			leftPosition.x = leftPosX;
			leftPosition.y = leftPosY;
			leftPosition.z = leftPosZ;
		}
		if (simulationType == WVR_SimulationType.WVR_SimulationType_ForceOn
			|| (simulationType == WVR_SimulationType.WVR_SimulationType_Auto && is6DoFPose == false)
		)
		{
			updateControllerPose (WVR_DeviceType.WVR_DeviceType_Controller_Left, leftRigidTransform);
			leftPosition = Vector3.Lerp (leftPosition, controllerSimulatedPosition, smoothMoveSpeed);
		}

		leftRigidTransform.update (leftPosition, leftRotation);
		leftPoseMatrix = GetOpenGLMatrix44 (leftPosition, leftRotation);
	}

	private void SetDevicePosePairHead()
	{
		posePairHead.type = WVR_DeviceType.WVR_DeviceType_HMD;
		posePairHead.pose.IsValidPose = true;
		posePairHead.pose.PoseMatrix = headPoseMatrix;
		posePairHead.pose.Velocity.v0 = 0.1f;
		posePairHead.pose.Velocity.v1 = 0.0f;
		posePairHead.pose.Velocity.v2 = 0.0f;
		posePairHead.pose.AngularVelocity.v0 = 0.1f;
		posePairHead.pose.AngularVelocity.v1 = 0.1f;
		posePairHead.pose.AngularVelocity.v2 = 0.1f;
		posePairHead.pose.Is6DoFPose = is6DoFPose;
		posePairHead.pose.OriginModel = hmdOriginModel;
	}

	private void SetDevicePosePairRight()
	{
		posePairRight.type = WVR_DeviceType.WVR_DeviceType_Controller_Right;
		posePairRight.pose.IsValidPose = true;
		posePairRight.pose.PoseMatrix = rightPoseMatrix;
		posePairRight.pose.Velocity.v0 = 0.1f;
		posePairRight.pose.Velocity.v1 = 0.0f;
		posePairRight.pose.Velocity.v2 = 0.0f;
		posePairRight.pose.AngularVelocity.v0 = 0.1f;
		posePairRight.pose.AngularVelocity.v1 = 0.1f;
		posePairRight.pose.AngularVelocity.v2 = 0.1f;
		posePairRight.pose.Is6DoFPose = is6DoFPose;
		posePairRight.pose.OriginModel = hmdOriginModel;
	}

	private void SetDevicePosePairLeft()
	{
		posePairLeft.type = WVR_DeviceType.WVR_DeviceType_Controller_Left;
		posePairLeft.pose.IsValidPose = true;
		posePairLeft.pose.PoseMatrix = leftPoseMatrix;
		posePairLeft.pose.Velocity.v0 = 0.1f;
		posePairLeft.pose.Velocity.v1 = 0.0f;
		posePairLeft.pose.Velocity.v2 = 0.0f;
		posePairLeft.pose.AngularVelocity.v0 = 0.1f;
		posePairLeft.pose.AngularVelocity.v1 = 0.1f;
		posePairLeft.pose.AngularVelocity.v2 = 0.1f;
		posePairLeft.pose.Is6DoFPose = is6DoFPose;
		posePairLeft.pose.OriginModel = hmdOriginModel;
	}

	private Vector3 GetHeadPosition()
	{
		return followHead ? headPosition : defaultHeadPosition;
	}

	private Vector3 GetHeadForward()
	{
		return headRotation * Vector3.forward;
	}

	private void updateControllerPose(WVR_DeviceType device, WaveVR_Utils.RigidTransform rtPose)
	{
		bodyRotation = Quaternion.identity;
		UpdateHeadAndBodyPose (device, rtPose);

		if (device == WVR_DeviceType.WVR_DeviceType_Controller_Right)
			v3ChangeArmXAxis.x = 1.0f;
		if (device == WVR_DeviceType.WVR_DeviceType_Controller_Left)
			v3ChangeArmXAxis.x = -1.0f;

		ComputeControllerPose (rtPose);
		controllerSimulatedPosition += CENTER_EYE_POSITION;
	}

	private void UpdateHeadAndBodyPose(WVR_DeviceType device, WaveVR_Utils.RigidTransform rtPose)
	{
		// Determine the gaze direction horizontally.
		Vector3 gazeDirection = GetHeadForward();
		gazeDirection.y = 0;
		gazeDirection.Normalize();

		float _bodyLerpFilter = BodyRotationFilter (device, rtPose);
		if (_bodyLerpFilter > 0)
		{
			if (!followHead)
			{
				defaultHeadPosition = headPosition;
			}
		}

		bodyDirection = Vector3.Slerp (bodyDirection, gazeDirection, _bodyLerpFilter);
		bodyRotation = Quaternion.FromToRotation(Vector3.forward, bodyDirection);
	}

	private bool quaternionEqual(Quaternion qua1, Quaternion qua2)
	{
		if (qua1.x == qua2.x &&
		    qua1.y == qua2.y &&
		    qua1.z == qua2.z &&
		    qua1.w == qua2.w)
			return true;
		return false;
	}

	private float BodyRotationFilter(WVR_DeviceType device, WaveVR_Utils.RigidTransform rtPose)
	{
		float _bodyLerpFilter = 0;

		try {
			Quaternion _rot_old = Quaternion.identity;
			if (device == WVR_DeviceType.WVR_DeviceType_Controller_Right)
				_rot_old = rightRotation;
			if (device == WVR_DeviceType.WVR_DeviceType_Controller_Left)
				_rot_old = leftRotation;
			Quaternion _rot_new = rtPose.rot;
			float _rot_XY_angle_old = 0, _rot_XY_angle_new = 0;

			Vector3 _rot_forward = Vector3.zero;
			Quaternion _rot_XY_rotation = Quaternion.identity;

			_rot_forward = _rot_old * Vector3.forward;
			_rot_XY_rotation = Quaternion.FromToRotation (Vector3.forward, _rot_forward);
			_rot_XY_angle_old = Quaternion.Angle (_rot_XY_rotation, Quaternion.identity);

			_rot_forward = _rot_new * Vector3.forward;
			_rot_XY_rotation = Quaternion.FromToRotation (Vector3.forward, _rot_forward);
			_rot_XY_angle_new = Quaternion.Angle (_rot_XY_rotation, Quaternion.identity);

			float _diff_angle = _rot_XY_angle_new - _rot_XY_angle_old;
			_diff_angle = _diff_angle > 0 ? _diff_angle : -_diff_angle;

			_bodyLerpFilter = Mathf.Clamp ((_diff_angle - BodyAngleBound) / BodyAngleLimitation, 0, 1.0f);

			framesOfFreeze = _bodyLerpFilter < 1.0f ? framesOfFreeze + 1 : 0;

			if (framesOfFreeze > mFPS)
				_bodyLerpFilter = 0;
		} catch (NullReferenceException e) {
			ERROR ("BodyRotationFilter() NullReferenceException " + e.Message);
		} catch (MissingReferenceException e) {
			ERROR ("BodyRotationFilter() MissingReferenceException " + e.Message);
		} catch (MissingComponentException e) {
			ERROR ("BodyRotationFilter() MissingComponentException " + e.Message);
		} catch (IndexOutOfRangeException e) {
			ERROR ("BodyRotationFilter() IndexOutOfRangeException " + e.Message);
		}
		return _bodyLerpFilter;
	}

	private Vector3 ApplyNeckToHead(Vector3 head_position)
	{
		Vector3 _neckOffset = headRotation * NECK_OFFSET;
		_neckOffset.y -= NECK_OFFSET.y;  // add neck length
		head_position += _neckOffset;

		return head_position;
	}

	/// <summary>
	/// Get the position of controller in Arm Model
	/// 
	/// Consider the parts construct controller position:
	/// Parts contain elbow, wrist and controller and each part has default offset from head.
	/// <br>
	/// 1. simulated elbow offset = default elbow offset apply body rotation = body rotation (Quaternion) * elbow offset (Vector3)
	/// <br>
	/// 2. simulated wrist offset = default wrist offset apply elbow rotation = elbow rotation (Quaternion) * wrist offset (Vector3)
	/// <br>
	/// 3. simulated controller offset = default controller offset apply wrist rotation = wrist rotation (Quat) * controller offset (V3)
	/// <br>
	/// head + 1 + 2 + 3 = controller position.
	/// </summary>
	/// <param name="rtPose">WaveVR_Utils.RigidTransform</param>
	private void ComputeControllerPose(WaveVR_Utils.RigidTransform rtPose)
	{
		// if bodyRotation angle is θ, _inverseBodyRation is -θ
		// the operator * of Quaternion in Unity means concatenation, not multipler.
		// If quaternion qA has angle θ, quaternion qB has angle ε,
		// qA * qB will plus θ and ε which means rotating angle θ then rotating angle ε.
		// (_inverseBodyRotation * rotation of controller in world space) means angle ε subtracts angle θ.
		Quaternion _controllerRotation = Quaternion.Inverse(bodyRotation) * rtPose.rot;
		Vector3 _headPosition = GetHeadPosition ();

		/// 1. simulated elbow offset = default elbow offset apply body rotation = body rotation (Quaternion) * elbow offset (Vector3)
		// Default left / right elbow offset.
		Vector3 _elbowOffset = Vector3.Scale (HEADTOELBOW_OFFSET, v3ChangeArmXAxis);
		// Default left / right elbow pitch offset.
		Vector3 _elbowPitchOffset = Vector3.Scale (ELBOW_PITCH_OFFSET, v3ChangeArmXAxis) + ELBOW_RAISE_OFFSET;

		// Use controller pitch to simulate elbow pitch.
		// Range from ELBOW_PITCH_ANGLE_MIN ~ ELBOW_PITCH_ANGLE_MAX.
		// The percent of pitch angle will be used to calculate the position offset.
		Vector3 _controllerForward = _controllerRotation * Vector3.forward;
		float _controllerPitch = 90.0f - Vector3.Angle (_controllerForward, Vector3.up);	// 0~90
		float _controllerPitchRadio = (_controllerPitch - ELBOW_PITCH_ANGLE_MIN) / (ELBOW_PITCH_ANGLE_MAX - ELBOW_PITCH_ANGLE_MIN);
		_controllerPitchRadio = Mathf.Clamp (_controllerPitchRadio, 0.0f, 1.0f);

		// According to pitch angle percent, plus offset to elbow position.
		_elbowOffset += _elbowPitchOffset * _controllerPitchRadio;
		// Apply body rotation and head position to calculate final elbow position.
		_elbowOffset = _headPosition + bodyRotation * _elbowOffset;

		// Rotation from Z-axis to XY-plane used to simulated elbow & wrist rotation.
		Quaternion _controllerXYRotation = Quaternion.FromToRotation (Vector3.forward, _controllerForward);
		float _controllerXYRotationRadio = (Quaternion.Angle (_controllerXYRotation, Quaternion.identity)) / 180;
		// Simulate the elbow raising curve.
		float _elbowCurveLerpValue = ELBOW_TO_XYPLANE_LERP_MIN + (_controllerXYRotationRadio * (ELBOW_TO_XYPLANE_LERP_MAX - ELBOW_TO_XYPLANE_LERP_MIN));
		Quaternion _controllerXYLerpRotation = Quaternion.Lerp (Quaternion.identity, _controllerXYRotation, _elbowCurveLerpValue);


		/// 2. simulated wrist offset = default wrist offset apply elbow rotation = elbow rotation (Quaternion) * wrist offset (Vector3)
		// Default left / right wrist offset
		Vector3 _wristOffset = Vector3.Scale (ELBOWTOWRIST_OFFSET, v3ChangeArmXAxis);
		// elbow rotation + curve = wrist rotation
		// wrist rotation = controller XY rotation
		// => elbow rotation + curve = controller XY rotation
		// => elbow rotation = controller XY rotation - curve
		Quaternion _elbowRotation = bodyRotation * Quaternion.Inverse(_controllerXYLerpRotation) * _controllerXYRotation;
		// Apply elbow offset and elbow rotation to calculate final wrist position.
		_wristOffset = _elbowOffset + _elbowRotation * _wristOffset;


		/// 3. simulated controller offset = default controller offset apply wrist rotation = wrist rotation (Quat) * controller offset (V3)
		// Default left / right controller offset.
		Vector3 _controllerOffset = Vector3.Scale (WRISTTOCONTROLLER_OFFSET, v3ChangeArmXAxis);
		Quaternion _wristRotation = _controllerXYRotation;
		// Apply wrist offset and wrist rotation to calculate final controller position.
		_controllerOffset = _wristOffset + _wristRotation * _controllerOffset;

		controllerSimulatedPosition = /*bodyRotation */ _controllerOffset;
		//controllerSimulatedRotation = bodyRotation * _controllerRotation;
	}

	private WVR_Matrix4f_t GetOpenGLMatrix44(Vector3 pos, Quaternion rot)
	{
		WVR_Matrix4f_t matrix44;
		// m0 = 1 - 2 * y^2 - 2 * z^2
		matrix44.m0 = 1 - (2 * (rot.y * rot.y)) - (2 * (rot.z * rot.z));
		// m1 = 2xy - 2zw
		matrix44.m1 = (2 * rot.x * rot.y) - (2 * rot.z * rot.w);
		// m2 = -(2xz + 2yw)
		matrix44.m2 = -((2 * rot.x * rot.z) + (2 * rot.y * rot.w));
		// m3 = X
		matrix44.m3 = pos.x;
		// m4 = 2xy + 2zw
		matrix44.m4 = (2 * rot.x * rot.y) + (2 * rot.z * rot.w);
		// m5 = 1 - 2 * x^2 - 2 * z^2
		matrix44.m5 = 1 - (2 * (rot.x * rot.x)) - (2 * (rot.z * rot.z));
		// m6 = 2xw - 2yz
		matrix44.m6 = (2 * rot.x * rot.w) - (2 * rot.y * rot.z);
		// m7 = Y
		matrix44.m7 = pos.y;
		// m8 = 2yw - 2xz
		matrix44.m8 = (2 * rot.y * rot.w) - (2 * rot.x * rot.z);
		// m9 = -(2yz + 2xw)
		matrix44.m9 = -((2 * rot.y * rot.z) + (2 * rot.x * rot.w));
		// m10 = 1 - 2 * x^2 - 2 * y^2
		matrix44.m10 = 1 - (2 * rot.x * rot.x) - (2 * rot.y * rot.y);
		// m11 = -Z
		matrix44.m11 = -pos.z;
		// m12 = 0
		matrix44.m12 = 0;
		// m13 = 0
		matrix44.m13 = 0;
		// m14 = 0
		matrix44.m14 = 0;
		// m15 = 1
		matrix44.m15 = 1;

		return matrix44;
	}

	private WVR_Quatf_t GetOpenGLQuaternion(Quaternion rot)
	{
		WVR_Quatf_t qua;
		qua.x = rot.x;
		qua.y = rot.y;
		qua.z = -rot.z;
		qua.w = -rot.w;
		return qua;
	}

	private WVR_Vector3f_t GetOpenGLVector(Vector3 pos)
	{
		WVR_Vector3f_t vec;
		vec.v0 = pos.x;
		vec.v1 = pos.y;
		vec.v2 = -pos.z;
		return vec;
	}
	#endregion

	#region Simulation Pose
	private WVR_SimulationType simulationType = WVR_SimulationType.WVR_SimulationType_ForceOn;
	public void SetArmModel(WVR_SimulationType type)
	{
		DEBUG ("SetArmModel() " + type);
		simulationType = type;
	}

	private bool followHead = false;
	public void SetArmSticky(bool stickyArm)
	{
		DEBUG ("Follow head = " + stickyArm);
		followHead = stickyArm;
	}

	private bool enableNeckModel = true;
	public void SetNeckModelEnabled(bool enabled)
	{
		DEBUG ("SetNeckModelEnabled() " + enabled);
		enableNeckModel = enabled;
	}
	#endregion

	#region Key Mapping
	private WVR_InputMappingPair_t[] inputTable_Hmd, inputTable_Right, inputTable_Left;

	public bool SetInputRequest(WVR_DeviceType type, WVR_InputAttribute_t[] request, uint size)
	{
		if (type == WVR_DeviceType.WVR_DeviceType_HMD)
		{
			inputTable_Hmd = new WVR_InputMappingPair_t[size];
			WVR_InputId[] inputId_Hmd = new WVR_InputId[size];
			for (int i = 0; i < (int)size; i++)
			{
				inputId_Hmd [i] = request [i].id;
				inputTable_Hmd [i].destination = request [i];
			}
			UpdateInputTable (inputId_Hmd, inputTable_Hmd);
			for (int i = 0; i < inputTable_Hmd.Length; i++)
				DEBUG ("SetInputRequest() " + type + ", src: " + inputTable_Hmd[i].source.id + ", dst: " + inputTable_Hmd[i].destination.id);
		}

		if (type == WVR_DeviceType.WVR_DeviceType_Controller_Right)
		{
			inputTable_Right = new WVR_InputMappingPair_t[size];
			WVR_InputId[] inputId_Right = new WVR_InputId[size];
			for (int i = 0; i < (int)size; i++)
			{
				inputId_Right [i] = request [i].id;
				inputTable_Right [i].destination = request [i];
			}
			UpdateInputTable (inputId_Right, inputTable_Right);
			for (int i = 0; i < inputTable_Right.Length; i++)
				DEBUG ("SetInputRequest() " + type + ", src: " + inputTable_Right[i].source.id + ", dst: " + inputTable_Right[i].destination.id);
		}
		if (type == WVR_DeviceType.WVR_DeviceType_Controller_Left)
		{
			inputTable_Left = new WVR_InputMappingPair_t[size];
			WVR_InputId[] inputId_Left = new WVR_InputId[size];
			for (int i = 0; i < (int)size; i++)
			{
				inputId_Left [i] = request [i].id;
				inputTable_Left [i].destination = request [i];
			}
			UpdateInputTable (inputId_Left, inputTable_Left);
			for (int i = 0; i < inputTable_Left.Length; i++)
				DEBUG ("SetInputRequest() " + type + ", src: " + inputTable_Left[i].source.id + ", dst: " + inputTable_Left[i].destination.id);
		}
		return true;
	}

	void UpdateInputTable(WVR_InputId[] buttons, WVR_InputMappingPair_t[] inputTable)
	{
		for (int i = 0; i < buttons.Length; i++)
		{
			switch (buttons [i])
			{
			case WVR_InputId.WVR_InputId_Alias1_System:
			case WVR_InputId.WVR_InputId_Alias1_Menu:
			case WVR_InputId.WVR_InputId_Alias1_Grip:
			case WVR_InputId.WVR_InputId_Alias1_DPad_Left:
			case WVR_InputId.WVR_InputId_Alias1_DPad_Up:
			case WVR_InputId.WVR_InputId_Alias1_DPad_Right:
			case WVR_InputId.WVR_InputId_Alias1_DPad_Down:
			case WVR_InputId.WVR_InputId_Alias1_Volume_Up:
			case WVR_InputId.WVR_InputId_Alias1_Volume_Down:
			case WVR_InputId.WVR_InputId_Alias1_Digital_Trigger:
			case WVR_InputId.WVR_InputId_Alias1_Enter:
				inputTable [i].source.id = buttons [i];
				inputTable [i].source.capability = (uint)WVR_InputType.WVR_InputType_Button;
				inputTable [i].source.axis_type = WVR_AnalogType.WVR_AnalogType_None;
				break;
			case WVR_InputId.WVR_InputId_Alias1_Touchpad:
			case WVR_InputId.WVR_InputId_Alias1_Thumbstick:
				inputTable [i].source.id = buttons [i];
				inputTable [i].source.capability = (uint)(WVR_InputType.WVR_InputType_Button | WVR_InputType.WVR_InputType_Touch | WVR_InputType.WVR_InputType_Analog);
				inputTable [i].source.axis_type = WVR_AnalogType.WVR_AnalogType_2D;
				break;
			case WVR_InputId.WVR_InputId_Alias1_Trigger:
				inputTable [i].source.id = buttons [i];
				inputTable [i].source.capability = (uint)(WVR_InputType.WVR_InputType_Button | WVR_InputType.WVR_InputType_Touch | WVR_InputType.WVR_InputType_Analog);
				inputTable [i].source.axis_type = WVR_AnalogType.WVR_AnalogType_1D;
				break;
			default:
				break;
			}
		}
	}

	public uint GetInputMappingTable(WVR_DeviceType type, [In, Out] WVR_InputMappingPair_t[] table, uint size)
	{
		uint count = 0;
		switch (type)
		{
		case WVR_DeviceType.WVR_DeviceType_HMD:
			for (int i = 0; i < inputTable_Hmd.Length; i++)
				table [i] = inputTable_Hmd [i];
			count = (uint)inputTable_Hmd.Length;
			break;
		case WVR_DeviceType.WVR_DeviceType_Controller_Right:
			for (int i = 0; i < inputTable_Right.Length; i++)
				table [i] = inputTable_Right [i];
			count = (uint)inputTable_Right.Length;
			break;
		case WVR_DeviceType.WVR_DeviceType_Controller_Left:
			for (int i = 0; i < inputTable_Left.Length; i++)
				table [i] = inputTable_Left [i];
			count = (uint)inputTable_Left.Length;
			break;
		default:
			break;
		}

		return count;
	}

	public bool GetInputMappingPair(WVR_DeviceType type, WVR_InputId destination, ref WVR_InputMappingPair_t pair)
	{
		WVR_InputMappingPair_t[] inputTable = new WVR_InputMappingPair_t[10];
		switch (type)
		{
		case WVR_DeviceType.WVR_DeviceType_HMD:
			inputTable = inputTable_Hmd;
			break;
		case WVR_DeviceType.WVR_DeviceType_Controller_Right:
			inputTable = inputTable_Right;
			break;
		case WVR_DeviceType.WVR_DeviceType_Controller_Left:
			inputTable = inputTable_Left;
			break;
		default:
			break;
		}

		for (int i = 0; i < inputTable.Length; i++)
		{
			if (destination == inputTable [i].destination.id)
			{
				pair.source = inputTable [i].source;
				pair.destination = inputTable [i].destination;
				return true;
			}
		}

		return false;
	}
	#endregion

	#region Interaction Mode
	private WVR_InteractionMode interactionMode = WVR_InteractionMode.WVR_InteractionMode_Controller;
	public bool SetInteractionMode(WVR_InteractionMode mode)
	{
		if (mode == WVR_InteractionMode.WVR_InteractionMode_Gaze)
			interactionMode = WVR_InteractionMode.WVR_InteractionMode_Gaze;
		else
			interactionMode = WVR_InteractionMode.WVR_InteractionMode_Controller;
		return true;
	}
	public WVR_InteractionMode GetInteractionMode()
	{
		return interactionMode;
	}

	private WVR_GazeTriggerType gazeType = WVR_GazeTriggerType.WVR_GazeTriggerType_TimeoutButton;
	public bool SetGazeTriggerType(WVR_GazeTriggerType type)
	{
		gazeType = type;
		return true;
	}
	public WVR_GazeTriggerType GetGazeTriggerType()
	{
		return gazeType;
	}
	#endregion

	#region Arena
	private WVR_Arena_t mArena;
	private WVR_ArenaVisible mArenaVisible;
	public bool SetArena(ref WVR_Arena_t arena)
	{
		mArena = arena;
		return true;
	}

	public WVR_Arena_t GetArena()
	{
		return mArena;
	}

	public bool IsOverArenaRange()
	{
		return false;
	}

	public void SetArenaVisible(WVR_ArenaVisible config)
	{
		mArenaVisible = config;
	}

	public WVR_ArenaVisible GetArenaVisible()
	{
		return mArenaVisible;
	}
	#endregion

	#region Focused Controller
	private WVR_DeviceType focusedType = WVR_DeviceType.WVR_DeviceType_Controller_Right;
	public WVR_DeviceType GetFocusedController()
	{
		return focusedType;
	}

	public void SetFocusedController(WVR_DeviceType focusController)
	{
		focusedType = focusController;
	}
	#endregion

	#region Gesture
	private WVR_HandGestureType[] staticGestures = new WVR_HandGestureType[] {
		WVR_HandGestureType.WVR_HandGestureType_Invalid,
		WVR_HandGestureType.WVR_HandGestureType_Unknown,
		WVR_HandGestureType.WVR_HandGestureType_Fist,
		WVR_HandGestureType.WVR_HandGestureType_Five,
		WVR_HandGestureType.WVR_HandGestureType_OK,
		WVR_HandGestureType.WVR_HandGestureType_ThumbUp,
		WVR_HandGestureType.WVR_HandGestureType_IndexUp,
		WVR_HandGestureType.WVR_HandGestureType_Pinch
	};
	private int staticGesturesIndex = 0;
	private float staticGestureTime = 0;
	private int gestureDuration_Invalid = 2;	// Invalid gesture keeps 2s.
	private int gestureDuration_Fist = 5;		// Fist gesture keeps 5s.

	private void InitHandGesture()
	{
		staticGesturesIndex = 0;
		staticGestureTime = Time.unscaledTime;
	}
	private void UpdateHandGesture()
	{
		// Invalid -> Fist
		if (staticGesturesIndex == (int)WVR_HandGestureType.WVR_HandGestureType_Invalid)
		{
			if (Time.unscaledTime - staticGestureTime > gestureDuration_Invalid)
			{
				staticGestureTime = Time.unscaledTime;
				staticGesturesIndex = (int)WVR_HandGestureType.WVR_HandGestureType_Fist;
			}
		}

		// Fist -> Invalid
		if (staticGesturesIndex == (int)WVR_HandGestureType.WVR_HandGestureType_Fist)
		{
			if (Time.unscaledTime - staticGestureTime > gestureDuration_Fist)
			{
				staticGestureTime = Time.unscaledTime;
				staticGesturesIndex = (int)WVR_HandGestureType.WVR_HandGestureType_Invalid;
			}
		}
	}

	private bool isHandGestureEnabled = false;
	public WVR_Result StartHandGesture()
	{
		isHandGestureEnabled = true;
		return WVR_Result.WVR_Success;
	}

	public void StopHandGesture()
	{
		isHandGestureEnabled = false;
	}

	public WVR_Result GetHandGestureData(ref WVR_HandGestureData_t data)
	{
		if (isHandGestureEnabled)
		{
			data.timestamp = Time.frameCount;
			data.right = staticGestures [staticGesturesIndex];
			data.left = WVR_HandGestureType.WVR_HandGestureType_OK;
		} else
		{
			data.timestamp = 0;
			data.right = WVR_HandGestureType.WVR_HandGestureType_Invalid;
			data.left = WVR_HandGestureType.WVR_HandGestureType_Invalid;
		}
		return WVR_Result.WVR_Success;
	}

	private bool isHandTrackingEnabled = false;
	public WVR_Result StartHandTracking()
	{
		isHandTrackingEnabled = true;
		return WVR_Result.WVR_Success;
	}

	public void StopHandTracking()
	{
		isHandTrackingEnabled = false;
	}

	private enum EWaveVRGestureBoneType
	{
		// Left Arm, 19 finger bones + wrist / fore arm / upper arm.
		BONE_UPPERARM_L = 0,
		BONE_FOREARM_L,
		BONE_HAND_WRIST_L,
		BONE_HAND_PALM_L,
		BONE_THUMB_JOIN1_L,
		BONE_THUMB_JOIN2_L,
		BONE_THUMB_JOIN3_L,
		BONE_THUMB_TIP_L,
		BONE_INDEX_JOINT1_L,
		BONE_INDEX_JOINT2_L,
		BONE_INDEX_JOINT3_L,
		BONE_INDEX_TIP_L,
		BONE_MIDDLE_JOINT1_L,
		BONE_MIDDLE_JOINT2_L,
		BONE_MIDDLE_JOINT3_L,
		BONE_MIDDLE_TIP_L,
		BONE_RING_JOINT1_L,
		BONE_RING_JOINT2_L,
		BONE_RING_JOINT3_L,
		BONE_RING_TIP_L,
		BONE_PINKY_JOINT1_L,
		BONE_PINKY_JOINT2_L,
		BONE_PINKY_JOINT3_L,
		BONE_PINKY_TIP_L,

		// Right Arm, 19 finger bones + wrist / fore arm / upper arm.
		BONE_UPPERARM_R,
		BONE_FOREARM_R,
		BONE_HAND_WRIST_R,
		BONE_HAND_PALM_R,
		BONE_THUMB_JOINT1_R,
		BONE_THUMB_JOINT2_R,
		BONE_THUMB_JOINT3_R,
		BONE_THUMB_TIP_R,
		BONE_INDEX_JOINT1_R,
		BONE_INDEX_JOINT2_R,
		BONE_INDEX_JOINT3_R,
		BONE_INDEX_TIP_R,
		BONE_MIDDLE_JOINT1_R,
		BONE_MIDDLE_JOINT2_R,
		BONE_MIDDLE_JOINT3_R,
		BONE_MIDDLE_TIP_R,
		BONE_RING_JOINT1_R,
		BONE_RING_JOINT2_R,
		BONE_RING_JOINT3_R,
		BONE_RING_TIP_R,
		BONE_PINKY_JOINT1_R,
		BONE_PINKY_JOINT2_R,
		BONE_PINKY_JOINT3_R,
		BONE_PINKY_TIP_R,

		BONES_COUNT
	};

	// Left wrist.
	private readonly Vector3 BONE_HAND_WRIST_L_POS = new Vector3(-0.09f, 0, 0.2f);
	private readonly Vector3 BONE_HAND_WRIST_L_ROT = new Vector3 (7, 0, -15);
	// Left thumb.
	private readonly Vector3 BONE_THUMB_JOIN2_L_POS = new Vector3 (-0.05f, 0.02f, 0.2f);
	private readonly Vector3 BONE_THUMB_JOIN2_L_ROT = new Vector3 (0, 0, -42.54f);
	private readonly Vector3 BONE_THUMB_JOIN3_L_POS = new Vector3 (-0.04f, 0.03f, 0.2f);
	private readonly Vector3 BONE_THUMB_JOIN3_L_ROT = new Vector3(0, 0, -42.54f);
	private readonly Vector3 BONE_THUMB_TIP_L_POS = new Vector3 (-0.03f, 0.04f, 0.2f);
	private readonly Vector3 BONE_THUMB_TIP_L_ROT = new Vector3 (0, 0, -42.54f);
	// Left index.
	private readonly Vector3 BONE_INDEX_JOINT1_L_POS = new Vector3 (-0.06f, 0.04f, 0.2f);
	private readonly Vector3 BONE_INDEX_JOINT1_L_ROT = new Vector3 (0, 0, -16);
	private readonly Vector3 BONE_INDEX_JOINT2_L_POS = new Vector3 (-0.056f, 0.05f, 0.2f);
	private readonly Vector3 BONE_INDEX_JOINT2_L_ROT = new Vector3 (0, 0, -16);
	private readonly Vector3 BONE_INDEX_JOINT3_L_POS = new Vector3 (-0.052f, 0.06f, 0.2f);
	private readonly Vector3 BONE_INDEX_JOINT3_L_ROT = new Vector3 (0, 0, -16);
	private readonly Vector3 BONE_INDEX_TIP_L_POS = new Vector3 (-0.048f, 0.07f, 0.2f);
	private readonly Vector3 BONE_INDEX_TIP_L_ROT = new Vector3 (0, 0, -16);
	// Left middle.
	private readonly Vector3 BONE_MIDDLE_JOINT1_L_POS = new Vector3 (-0.075f, 0.045f, 0.2f);
	private readonly Vector3 BONE_MIDDLE_JOINT1_L_ROT = new Vector3 (0, 0, -0.87f);
	private readonly Vector3 BONE_MIDDLE_JOINT2_L_POS = new Vector3 (-0.074f, 0.055f, 0.2f);
	private readonly Vector3 BONE_MIDDLE_JOINT2_L_ROT = new Vector3 (0, 0, -0.87f);
	private readonly Vector3 BONE_MIDDLE_JOINT3_L_POS = new Vector3 (-0.073f, 0.065f, 0.2f);
	private readonly Vector3 BONE_MIDDLE_JOINT3_L_ROT = new Vector3 (0, 0, -0.87f);
	private readonly Vector3 BONE_MIDDLE_TIP_L_POS = new Vector3 (-0.072f, 0.075f, 0.2f);
	private readonly Vector3 BONE_MIDDLE_TIP_L_ROT = new Vector3 (0, 0, -0.87f);
	// Left ring.
	private readonly Vector3 BONE_RING_JOINT1_L_POS = new Vector3 (-0.087f, 0.04f, 0.2f);
	private readonly Vector3 BONE_RING_JOINT1_L_ROT = new Vector3 (0, 0, 12.48f);
	private readonly Vector3 BONE_RING_JOINT2_L_POS = new Vector3 (-0.089f, 0.05f, 0.2f);
	private readonly Vector3 BONE_RING_JOINT2_L_ROT = new Vector3 (0, 0, 12.48f);
	private readonly Vector3 BONE_RING_JOINT3_L_POS = new Vector3 (-0.091f, 0.06f, 0.2f);
	private readonly Vector3 BONE_RING_JOINT3_L_ROT = new Vector3 (0, 0, 12.48f);
	private readonly Vector3 BONE_RING_TIP_L_POS = new Vector3 (-0.093f, 0.07f, 0.2f);
	private readonly Vector3 BONE_RING_TIP_L_ROT = new Vector3 (0, 0, 12.48f);
	// Left pinky.
	private readonly Vector3 BONE_PINKY_JOINT1_L_POS = new Vector3 (-0.099f, 0.03f, 0.2f);
	private readonly Vector3 BONE_PINKY_JOINT1_L_ROT = new Vector3 (0, 0, 28);
	private readonly Vector3 BONE_PINKY_JOINT2_L_POS = new Vector3 (-0.103f, 0.04f, 0.2f);
	private readonly Vector3 BONE_PINKY_JOINT2_L_ROT = new Vector3 (0, 0, 28);
	private readonly Vector3 BONE_PINKY_JOINT3_L_POS = new Vector3 (-0.106f, 0.05f, 0.2f);
	private readonly Vector3 BONE_PINKY_JOINT3_L_ROT = new Vector3 (0, 0, 28);
	private readonly Vector3 BONE_PINKY_TIP_L_POS = new Vector3 (-0.109f, 0.06f, 0.2f);
	private readonly Vector3 BONE_PINKY_TIP_L_ROT = new Vector3 (0, 0, 28);

	// Right wrist.
	private readonly Vector3 BONE_HAND_WRIST_R_POS = new Vector3(0.09f, 0, 0.2f);
	private readonly Vector3 BONE_HAND_WRIST_R_ROT = new Vector3 (7, 0, 15);
	// Right thumb.
	private readonly Vector3 BONE_THUMB_JOINT2_R_POS = new Vector3 (0.05f, 0.02f, 0.2f);
	private readonly Vector3 BONE_THUMB_JOINT2_R_ROT = new Vector3 (0, 0, 42.54f);
	private readonly Vector3 BONE_THUMB_JOINT3_R_POS = new Vector3 (0.04f, 0.03f, 0.2f);
	private readonly Vector3 BONE_THUMB_JOINT3_R_ROT = new Vector3(0, 0, 42.54f);
	private readonly Vector3 BONE_THUMB_TIP_R_POS = new Vector3 (0.03f, 0.04f, 0.2f);
	private readonly Vector3 BONE_THUMB_TIP_R_ROT = new Vector3 (0, 0, 42.54f);
	// Right index.
	private readonly Vector3 BONE_INDEX_JOINT1_R_POS = new Vector3 (0.06f, 0.04f, 0.2f);
	private readonly Vector3 BONE_INDEX_JOINT1_R_ROT = new Vector3 (0, 0, 16);
	private readonly Vector3 BONE_INDEX_JOINT2_R_POS = new Vector3 (0.056f, 0.05f, 0.2f);
	private readonly Vector3 BONE_INDEX_JOINT2_R_ROT = new Vector3 (0, 0, 16);
	private readonly Vector3 BONE_INDEX_JOINT3_R_POS = new Vector3 (0.052f, 0.06f, 0.2f);
	private readonly Vector3 BONE_INDEX_JOINT3_R_ROT = new Vector3 (0, 0, 16);
	private readonly Vector3 BONE_INDEX_TIP_R_POS = new Vector3 (0.048f, 0.07f, 0.2f);
	private readonly Vector3 BONE_INDEX_TIP_R_ROT = new Vector3 (0, 0, 16);
	// Right middle.
	private readonly Vector3 BONE_MIDDLE_JOINT1_R_POS = new Vector3 (0.075f, 0.045f, 0.2f);
	private readonly Vector3 BONE_MIDDLE_JOINT1_R_ROT = new Vector3 (0, 0, 0.87f);
	private readonly Vector3 BONE_MIDDLE_JOINT2_R_POS = new Vector3 (0.074f, 0.055f, 0.2f);
	private readonly Vector3 BONE_MIDDLE_JOINT2_R_ROT = new Vector3 (0, 0, 0.87f);
	private readonly Vector3 BONE_MIDDLE_JOINT3_R_POS = new Vector3 (0.073f, 0.065f, 0.2f);
	private readonly Vector3 BONE_MIDDLE_JOINT3_R_ROT = new Vector3 (0, 0, 0.87f);
	private readonly Vector3 BONE_MIDDLE_TIP_R_POS = new Vector3 (0.072f, 0.075f, 0.2f);
	private readonly Vector3 BONE_MIDDLE_TIP_R_ROT = new Vector3 (0, 0, 0.87f);
	// Right ring.
	private readonly Vector3 BONE_RING_JOINT1_R_POS = new Vector3 (0.087f, 0.04f, 0.2f);
	private readonly Vector3 BONE_RING_JOINT1_R_ROT = new Vector3 (0, 0, -12.48f);
	private readonly Vector3 BONE_RING_JOINT2_R_POS = new Vector3 (0.089f, 0.05f, 0.2f);
	private readonly Vector3 BONE_RING_JOINT2_R_ROT = new Vector3 (0, 0, -12.48f);
	private readonly Vector3 BONE_RING_JOINT3_R_POS = new Vector3 (0.091f, 0.06f, 0.2f);
	private readonly Vector3 BONE_RING_JOINT3_R_ROT = new Vector3 (0, 0, -12.48f);
	private readonly Vector3 BONE_RING_TIP_R_POS = new Vector3 (0.093f, 0.07f, 0.2f);
	private readonly Vector3 BONE_RING_TIP_R_ROT = new Vector3 (0, 0, -12.48f);
	// Right pinky.
	private readonly Vector3 BONE_PINKY_JOINT1_R_POS = new Vector3 (0.099f, 0.03f, 0.2f);
	private readonly Vector3 BONE_PINKY_JOINT1_R_ROT = new Vector3 (0, 0, -28);
	private readonly Vector3 BONE_PINKY_JOINT2_R_POS = new Vector3 (0.103f, 0.04f, 0.2f);
	private readonly Vector3 BONE_PINKY_JOINT2_R_ROT = new Vector3 (0, 0, -28);
	private readonly Vector3 BONE_PINKY_JOINT3_R_POS = new Vector3 (0.106f, 0.05f, 0.2f);
	private readonly Vector3 BONE_PINKY_JOINT3_R_ROT = new Vector3 (0, 0, -28);
	private readonly Vector3 BONE_PINKY_TIP_R_POS = new Vector3 (0.109f, 0.06f, 0.2f);
	private readonly Vector3 BONE_PINKY_TIP_R_ROT = new Vector3 (0, 0, -28);


	// Left bones.
	private List<WVR_Vector3f_t> leftBonesPosition = new List<WVR_Vector3f_t>();
	private List<WVR_Quatf_t> leftBonesOrientation = new List<WVR_Quatf_t> ();
	private WVR_Matrix4f_t leftWristMatrix;
	// Right bones.
	private List<WVR_Vector3f_t> rightBonesPosition = new List<WVR_Vector3f_t>();
	private List<WVR_Quatf_t> rightBonesOrientation = new List<WVR_Quatf_t> ();
	private WVR_Matrix4f_t rightWristMatrix;

	private void InitializeBonesAndHandTrackingData()
	{
		for (int i = 0; i < (int)EWaveVRGestureBoneType.BONES_COUNT; i++)
		{
			WVR_Vector3f_t pos;
			pos.v0 = 0;
			pos.v1 = 0;
			pos.v2 = 0;
			leftBonesPosition.Add (pos);
			rightBonesPosition.Add (pos);

			WVR_Quatf_t rot;
			rot.w = 0;
			rot.x = 0;
			rot.y = 0;
			rot.z = 0;
			leftBonesOrientation.Add (rot);
			rightBonesOrientation.Add (rot);
		}
	}

	private readonly Vector3 HAND_L_POS_OFFSET = new Vector3 (0, 0, 0.1f);
	private readonly Vector3 HAND_R_POS_OFFSET = new Vector3 (0, 0, 0.1f);
	private Vector3 leftYawRotation = new Vector3(0, 0.1f, 0), rightYawRotation = new Vector3(0, -0.1f, 0);
	private Quaternion leftYawOrientation = Quaternion.identity, rightYawOrientation = Quaternion.identity;
	private int boneCount = 0, boneCountAdder = 1;
	private void UpdateBonesAndHandTrackingData()
	{
		// Move the bone position continuously.
		if (boneCount == 100 || boneCount == -100)
			boneCountAdder *= -1;
		boneCount += boneCountAdder;
		leftYawRotation.y += 0.1f * boneCountAdder;
		leftYawOrientation = Quaternion.Euler (leftYawRotation);
		rightYawRotation.y += -0.1f * boneCountAdder;
		rightYawOrientation = Quaternion.Euler (rightYawRotation);

		Vector3 vec_raw = Vector3.zero;	// Raw data position.
		Vector3 vec = Vector3.zero;
		Quaternion qua = Quaternion.identity;

		// Calculate the left bone offset according to the origin.
		Vector3 BONE_HAND_L_POS_OFFSET = HAND_L_POS_OFFSET;
		BONE_HAND_L_POS_OFFSET.y = gestureOriginModel == WVR_PoseOriginModel.WVR_PoseOriginModel_OriginOnGround ? BONE_HAND_L_POS_OFFSET.y + 1.75f : BONE_HAND_L_POS_OFFSET.y;

		// Left wrist.
		vec_raw = leftYawOrientation * (BONE_HAND_WRIST_L_POS + BONE_HAND_L_POS_OFFSET);	// Assume raw data with the offset of the origin.
		vec = leftYawOrientation * (BONE_HAND_WRIST_L_POS + BONE_HAND_L_POS_OFFSET);
		qua = Quaternion.Euler (BONE_HAND_WRIST_L_ROT);
		leftBonesPosition [(int)EWaveVRGestureBoneType.BONE_HAND_WRIST_L] = GetOpenGLVector (vec_raw);
		leftBonesOrientation [(int)EWaveVRGestureBoneType.BONE_HAND_WRIST_L] = GetOpenGLQuaternion (qua);
		leftWristMatrix = GetOpenGLMatrix44 (vec, qua);

		// Left thumb.
		vec = leftYawOrientation * (BONE_THUMB_JOIN2_L_POS + BONE_HAND_L_POS_OFFSET);
		qua = Quaternion.Euler (BONE_THUMB_JOIN2_L_ROT);
		leftBonesPosition [(int)EWaveVRGestureBoneType.BONE_THUMB_JOIN2_L] = GetOpenGLVector (vec);
		leftBonesOrientation[(int)EWaveVRGestureBoneType.BONE_THUMB_JOIN2_L] = GetOpenGLQuaternion (qua);

		vec = leftYawOrientation * (BONE_THUMB_JOIN3_L_POS + BONE_HAND_L_POS_OFFSET);
		qua = Quaternion.Euler (BONE_THUMB_JOIN3_L_ROT);
		leftBonesPosition [(int)EWaveVRGestureBoneType.BONE_THUMB_JOIN3_L] = GetOpenGLVector (vec);
		leftBonesOrientation [(int)EWaveVRGestureBoneType.BONE_THUMB_JOIN3_L] = GetOpenGLQuaternion (qua);

		vec = leftYawOrientation * (BONE_THUMB_TIP_L_POS + BONE_HAND_L_POS_OFFSET);
		qua = Quaternion.Euler (BONE_THUMB_TIP_L_ROT);
		leftBonesPosition [(int)EWaveVRGestureBoneType.BONE_THUMB_TIP_L] = GetOpenGLVector (vec);
		leftBonesOrientation [(int)EWaveVRGestureBoneType.BONE_THUMB_TIP_L] = GetOpenGLQuaternion (qua);

		// Left index.
		vec = leftYawOrientation * (BONE_INDEX_JOINT1_L_POS + BONE_HAND_L_POS_OFFSET);
		qua = Quaternion.Euler (BONE_INDEX_JOINT1_L_ROT);
		leftBonesPosition [(int)EWaveVRGestureBoneType.BONE_INDEX_JOINT1_L] = GetOpenGLVector (vec);
		leftBonesOrientation [(int)EWaveVRGestureBoneType.BONE_INDEX_JOINT1_L] = GetOpenGLQuaternion (qua);

		vec = leftYawOrientation * (BONE_INDEX_JOINT2_L_POS + BONE_HAND_L_POS_OFFSET);
		qua = Quaternion.Euler (BONE_INDEX_JOINT2_L_ROT);
		leftBonesPosition [(int)EWaveVRGestureBoneType.BONE_INDEX_JOINT2_L] = GetOpenGLVector (vec);
		leftBonesOrientation [(int)EWaveVRGestureBoneType.BONE_INDEX_JOINT2_L] = GetOpenGLQuaternion (qua);

		vec = leftYawOrientation * (BONE_INDEX_JOINT3_L_POS + BONE_HAND_L_POS_OFFSET);
		qua = Quaternion.Euler (BONE_INDEX_JOINT3_L_ROT);
		leftBonesPosition [(int)EWaveVRGestureBoneType.BONE_INDEX_JOINT3_L] = GetOpenGLVector (vec);
		leftBonesOrientation [(int)EWaveVRGestureBoneType.BONE_INDEX_JOINT3_L] = GetOpenGLQuaternion (qua);

		vec = leftYawOrientation * (BONE_INDEX_TIP_L_POS + BONE_HAND_L_POS_OFFSET);
		qua = Quaternion.Euler (BONE_INDEX_TIP_L_ROT);
		leftBonesPosition [(int)EWaveVRGestureBoneType.BONE_INDEX_TIP_L] = GetOpenGLVector (vec);
		leftBonesOrientation [(int)EWaveVRGestureBoneType.BONE_INDEX_TIP_L] = GetOpenGLQuaternion (qua);

		// Left middle.
		vec = leftYawOrientation * (BONE_MIDDLE_JOINT1_L_POS + BONE_HAND_L_POS_OFFSET);
		qua = Quaternion.Euler (BONE_MIDDLE_JOINT1_L_ROT);
		leftBonesPosition [(int)EWaveVRGestureBoneType.BONE_MIDDLE_JOINT1_L] = GetOpenGLVector (vec);
		leftBonesOrientation [(int)EWaveVRGestureBoneType.BONE_MIDDLE_JOINT1_L] = GetOpenGLQuaternion (qua);

		vec = leftYawOrientation * (BONE_MIDDLE_JOINT2_L_POS + BONE_HAND_L_POS_OFFSET);
		qua = Quaternion.Euler (BONE_MIDDLE_JOINT2_L_ROT);
		leftBonesPosition [(int)EWaveVRGestureBoneType.BONE_MIDDLE_JOINT2_L] = GetOpenGLVector (vec);
		leftBonesOrientation [(int)EWaveVRGestureBoneType.BONE_MIDDLE_JOINT2_L] = GetOpenGLQuaternion (qua);

		vec = leftYawOrientation * (BONE_MIDDLE_JOINT3_L_POS + BONE_HAND_L_POS_OFFSET);
		qua = Quaternion.Euler (BONE_MIDDLE_JOINT3_L_ROT);
		leftBonesPosition [(int)EWaveVRGestureBoneType.BONE_MIDDLE_JOINT3_L] = GetOpenGLVector (vec);
		leftBonesOrientation [(int)EWaveVRGestureBoneType.BONE_MIDDLE_JOINT3_L] = GetOpenGLQuaternion (qua);

		vec = leftYawOrientation * (BONE_MIDDLE_TIP_L_POS + BONE_HAND_L_POS_OFFSET);
		qua = Quaternion.Euler (BONE_MIDDLE_TIP_L_ROT);
		leftBonesPosition [(int)EWaveVRGestureBoneType.BONE_MIDDLE_TIP_L] = GetOpenGLVector (vec);
		leftBonesOrientation [(int)EWaveVRGestureBoneType.BONE_MIDDLE_TIP_L] = GetOpenGLQuaternion (qua);

		// Left ring.
		vec = leftYawOrientation * (BONE_RING_JOINT1_L_POS + BONE_HAND_L_POS_OFFSET);
		qua = Quaternion.Euler (BONE_RING_JOINT1_L_ROT);
		leftBonesPosition [(int)EWaveVRGestureBoneType.BONE_RING_JOINT1_L] = GetOpenGLVector (vec);
		leftBonesOrientation [(int)EWaveVRGestureBoneType.BONE_RING_JOINT1_L] = GetOpenGLQuaternion (qua);

		vec = leftYawOrientation * (BONE_RING_JOINT2_L_POS + BONE_HAND_L_POS_OFFSET);
		qua = Quaternion.Euler (BONE_RING_JOINT2_L_ROT);
		leftBonesPosition [(int)EWaveVRGestureBoneType.BONE_RING_JOINT2_L] = GetOpenGLVector (vec);
		leftBonesOrientation [(int)EWaveVRGestureBoneType.BONE_RING_JOINT2_L] = GetOpenGLQuaternion (qua);

		vec = leftYawOrientation * (BONE_RING_JOINT3_L_POS + BONE_HAND_L_POS_OFFSET);
		qua = Quaternion.Euler (BONE_RING_JOINT3_L_ROT);
		leftBonesPosition [(int)EWaveVRGestureBoneType.BONE_RING_JOINT3_L] = GetOpenGLVector (vec);
		leftBonesOrientation [(int)EWaveVRGestureBoneType.BONE_RING_JOINT3_L] = GetOpenGLQuaternion (qua);

		vec = leftYawOrientation * (BONE_RING_TIP_L_POS + BONE_HAND_L_POS_OFFSET);
		qua = Quaternion.Euler (BONE_RING_TIP_L_ROT);
		leftBonesPosition [(int)EWaveVRGestureBoneType.BONE_RING_TIP_L] = GetOpenGLVector (vec);
		leftBonesOrientation [(int)EWaveVRGestureBoneType.BONE_RING_TIP_L] = GetOpenGLQuaternion (qua);

		// Left pinky.
		vec = leftYawOrientation * (BONE_PINKY_JOINT1_L_POS + BONE_HAND_L_POS_OFFSET);
		qua = Quaternion.Euler (BONE_PINKY_JOINT1_L_ROT);
		leftBonesPosition [(int)EWaveVRGestureBoneType.BONE_PINKY_JOINT1_L] = GetOpenGLVector (vec);
		leftBonesOrientation [(int)EWaveVRGestureBoneType.BONE_PINKY_JOINT1_L] = GetOpenGLQuaternion (qua);

		vec = leftYawOrientation * (BONE_PINKY_JOINT2_L_POS + BONE_HAND_L_POS_OFFSET);
		qua = Quaternion.Euler (BONE_PINKY_JOINT2_L_ROT);
		leftBonesPosition [(int)EWaveVRGestureBoneType.BONE_PINKY_JOINT2_L] = GetOpenGLVector (vec);
		leftBonesOrientation [(int)EWaveVRGestureBoneType.BONE_PINKY_JOINT2_L] = GetOpenGLQuaternion (qua);

		vec = leftYawOrientation * (BONE_PINKY_JOINT3_L_POS + BONE_HAND_L_POS_OFFSET);
		qua = Quaternion.Euler (BONE_PINKY_JOINT3_L_ROT);
		leftBonesPosition [(int)EWaveVRGestureBoneType.BONE_PINKY_JOINT3_L] = GetOpenGLVector (vec);
		leftBonesOrientation [(int)EWaveVRGestureBoneType.BONE_PINKY_JOINT3_L] = GetOpenGLQuaternion (qua);

		vec = leftYawOrientation * (BONE_PINKY_TIP_L_POS + BONE_HAND_L_POS_OFFSET);
		qua = Quaternion.Euler (BONE_PINKY_TIP_L_ROT);
		leftBonesPosition [(int)EWaveVRGestureBoneType.BONE_PINKY_TIP_L] = GetOpenGLVector (vec);
		leftBonesOrientation [(int)EWaveVRGestureBoneType.BONE_PINKY_TIP_L] = GetOpenGLQuaternion (qua);

		// ----------------------------------
		Vector3 BONE_HAND_R_POS_OFFSET = HAND_R_POS_OFFSET;
		BONE_HAND_R_POS_OFFSET.y = gestureOriginModel == WVR_PoseOriginModel.WVR_PoseOriginModel_OriginOnGround ? BONE_HAND_R_POS_OFFSET.y + 1.75f : BONE_HAND_R_POS_OFFSET.y;

		// Right wrist.
		vec_raw = rightYawOrientation * (BONE_HAND_WRIST_R_POS + BONE_HAND_R_POS_OFFSET);	// Assume raw data with the offset of the origin.
		vec = rightYawOrientation * (BONE_HAND_WRIST_R_POS + BONE_HAND_R_POS_OFFSET);
		qua = Quaternion.Euler (BONE_HAND_WRIST_R_ROT);
		rightBonesPosition [(int)EWaveVRGestureBoneType.BONE_HAND_WRIST_R] = GetOpenGLVector (vec_raw);
		rightBonesOrientation [(int)EWaveVRGestureBoneType.BONE_HAND_WRIST_R] = GetOpenGLQuaternion (qua);
		rightWristMatrix = GetOpenGLMatrix44 (vec, qua);

		// Right thumb.
		vec = rightYawOrientation * (BONE_THUMB_JOINT2_R_POS + BONE_HAND_R_POS_OFFSET);
		qua = Quaternion.Euler (BONE_THUMB_JOINT2_R_ROT);
		rightBonesPosition [(int)EWaveVRGestureBoneType.BONE_THUMB_JOINT2_R] = GetOpenGLVector (vec);
		rightBonesOrientation[(int)EWaveVRGestureBoneType.BONE_THUMB_JOINT2_R] = GetOpenGLQuaternion (qua);

		vec = rightYawOrientation * (BONE_THUMB_JOINT3_R_POS + BONE_HAND_R_POS_OFFSET);
		qua = Quaternion.Euler (BONE_THUMB_JOINT3_R_ROT);
		rightBonesPosition [(int)EWaveVRGestureBoneType.BONE_THUMB_JOINT3_R] = GetOpenGLVector (vec);
		rightBonesOrientation [(int)EWaveVRGestureBoneType.BONE_THUMB_JOINT3_R] = GetOpenGLQuaternion (qua);

		vec = rightYawOrientation * (BONE_THUMB_TIP_R_POS + BONE_HAND_R_POS_OFFSET);
		qua = Quaternion.Euler (BONE_THUMB_TIP_R_ROT);
		rightBonesPosition [(int)EWaveVRGestureBoneType.BONE_THUMB_TIP_R] = GetOpenGLVector (vec);
		rightBonesOrientation [(int)EWaveVRGestureBoneType.BONE_THUMB_TIP_R] = GetOpenGLQuaternion (qua);

		// Right index.
		vec = rightYawOrientation * (BONE_INDEX_JOINT1_R_POS + BONE_HAND_R_POS_OFFSET);
		qua = Quaternion.Euler (BONE_INDEX_JOINT1_R_ROT);
		rightBonesPosition [(int)EWaveVRGestureBoneType.BONE_INDEX_JOINT1_R] = GetOpenGLVector (vec);
		rightBonesOrientation [(int)EWaveVRGestureBoneType.BONE_INDEX_JOINT1_R] = GetOpenGLQuaternion (qua);

		vec = rightYawOrientation * (BONE_INDEX_JOINT2_R_POS + BONE_HAND_R_POS_OFFSET);
		qua = Quaternion.Euler (BONE_INDEX_JOINT2_R_ROT);
		rightBonesPosition [(int)EWaveVRGestureBoneType.BONE_INDEX_JOINT2_R] = GetOpenGLVector (vec);
		rightBonesOrientation [(int)EWaveVRGestureBoneType.BONE_INDEX_JOINT2_R] = GetOpenGLQuaternion (qua);

		vec = rightYawOrientation * (BONE_INDEX_JOINT3_R_POS + BONE_HAND_R_POS_OFFSET);
		qua = Quaternion.Euler (BONE_INDEX_JOINT3_R_ROT);
		rightBonesPosition [(int)EWaveVRGestureBoneType.BONE_INDEX_JOINT3_R] = GetOpenGLVector (vec);
		rightBonesOrientation [(int)EWaveVRGestureBoneType.BONE_INDEX_JOINT3_R] = GetOpenGLQuaternion (qua);

		vec = rightYawOrientation * (BONE_INDEX_TIP_R_POS + BONE_HAND_R_POS_OFFSET);
		qua = Quaternion.Euler (BONE_INDEX_TIP_R_ROT);
		rightBonesPosition [(int)EWaveVRGestureBoneType.BONE_INDEX_TIP_R] = GetOpenGLVector (vec);
		rightBonesOrientation [(int)EWaveVRGestureBoneType.BONE_INDEX_TIP_R] = GetOpenGLQuaternion (qua);

		// Right middle.
		vec = rightYawOrientation * (BONE_MIDDLE_JOINT1_R_POS + BONE_HAND_R_POS_OFFSET);
		qua = Quaternion.Euler (BONE_MIDDLE_JOINT1_R_ROT);
		rightBonesPosition [(int)EWaveVRGestureBoneType.BONE_MIDDLE_JOINT1_R] = GetOpenGLVector (vec);
		rightBonesOrientation [(int)EWaveVRGestureBoneType.BONE_MIDDLE_JOINT1_R] = GetOpenGLQuaternion (qua);

		vec = rightYawOrientation * (BONE_MIDDLE_JOINT2_R_POS + BONE_HAND_R_POS_OFFSET);
		qua = Quaternion.Euler (BONE_MIDDLE_JOINT2_R_ROT);
		rightBonesPosition [(int)EWaveVRGestureBoneType.BONE_MIDDLE_JOINT2_R] = GetOpenGLVector (vec);
		rightBonesOrientation [(int)EWaveVRGestureBoneType.BONE_MIDDLE_JOINT2_R] = GetOpenGLQuaternion (qua);

		vec = rightYawOrientation * (BONE_MIDDLE_JOINT3_R_POS + BONE_HAND_R_POS_OFFSET);
		qua = Quaternion.Euler (BONE_MIDDLE_JOINT3_R_ROT);
		rightBonesPosition [(int)EWaveVRGestureBoneType.BONE_MIDDLE_JOINT3_R] = GetOpenGLVector (vec);
		rightBonesOrientation [(int)EWaveVRGestureBoneType.BONE_MIDDLE_JOINT3_R] = GetOpenGLQuaternion (qua);

		vec = rightYawOrientation * (BONE_MIDDLE_TIP_R_POS + BONE_HAND_R_POS_OFFSET);
		qua = Quaternion.Euler (BONE_MIDDLE_TIP_R_ROT);
		rightBonesPosition [(int)EWaveVRGestureBoneType.BONE_MIDDLE_TIP_R] = GetOpenGLVector (vec);
		rightBonesOrientation [(int)EWaveVRGestureBoneType.BONE_MIDDLE_TIP_R] = GetOpenGLQuaternion (qua);

		// Right ring.
		vec = rightYawOrientation * (BONE_RING_JOINT1_R_POS + BONE_HAND_R_POS_OFFSET);
		qua = Quaternion.Euler (BONE_RING_JOINT1_R_ROT);
		rightBonesPosition [(int)EWaveVRGestureBoneType.BONE_RING_JOINT1_R] = GetOpenGLVector (vec);
		rightBonesOrientation [(int)EWaveVRGestureBoneType.BONE_RING_JOINT1_R] = GetOpenGLQuaternion (qua);

		vec = rightYawOrientation * (BONE_RING_JOINT2_R_POS + BONE_HAND_R_POS_OFFSET);
		qua = Quaternion.Euler (BONE_RING_JOINT2_R_ROT);
		rightBonesPosition [(int)EWaveVRGestureBoneType.BONE_RING_JOINT2_R] = GetOpenGLVector (vec);
		rightBonesOrientation [(int)EWaveVRGestureBoneType.BONE_RING_JOINT2_R] = GetOpenGLQuaternion (qua);

		vec = rightYawOrientation * (BONE_RING_JOINT3_R_POS + BONE_HAND_R_POS_OFFSET);
		qua = Quaternion.Euler (BONE_RING_JOINT3_R_ROT);
		rightBonesPosition [(int)EWaveVRGestureBoneType.BONE_RING_JOINT3_R] = GetOpenGLVector (vec);
		rightBonesOrientation [(int)EWaveVRGestureBoneType.BONE_RING_JOINT3_R] = GetOpenGLQuaternion (qua);

		vec = rightYawOrientation * (BONE_RING_TIP_R_POS + BONE_HAND_R_POS_OFFSET);
		qua = Quaternion.Euler (BONE_RING_TIP_R_ROT);
		rightBonesPosition [(int)EWaveVRGestureBoneType.BONE_RING_TIP_R] = GetOpenGLVector (vec);
		rightBonesOrientation [(int)EWaveVRGestureBoneType.BONE_RING_TIP_R] = GetOpenGLQuaternion (qua);

		// Right pinky.
		vec = rightYawOrientation * (BONE_PINKY_JOINT1_R_POS + BONE_HAND_R_POS_OFFSET);
		qua = Quaternion.Euler (BONE_PINKY_JOINT1_R_ROT);
		rightBonesPosition [(int)EWaveVRGestureBoneType.BONE_PINKY_JOINT1_R] = GetOpenGLVector (vec);
		rightBonesOrientation [(int)EWaveVRGestureBoneType.BONE_PINKY_JOINT1_R] = GetOpenGLQuaternion (qua);

		vec = rightYawOrientation * (BONE_PINKY_JOINT2_R_POS + BONE_HAND_R_POS_OFFSET);
		qua = Quaternion.Euler (BONE_PINKY_JOINT2_R_ROT);
		rightBonesPosition [(int)EWaveVRGestureBoneType.BONE_PINKY_JOINT2_R] = GetOpenGLVector (vec);
		rightBonesOrientation [(int)EWaveVRGestureBoneType.BONE_PINKY_JOINT2_R] = GetOpenGLQuaternion (qua);

		vec = rightYawOrientation * (BONE_PINKY_JOINT3_R_POS + BONE_HAND_R_POS_OFFSET);
		qua = Quaternion.Euler (BONE_PINKY_JOINT3_R_ROT);
		rightBonesPosition [(int)EWaveVRGestureBoneType.BONE_PINKY_JOINT3_R] = GetOpenGLVector (vec);
		rightBonesOrientation [(int)EWaveVRGestureBoneType.BONE_PINKY_JOINT3_R] = GetOpenGLQuaternion (qua);

		vec = rightYawOrientation * (BONE_PINKY_TIP_R_POS + BONE_HAND_R_POS_OFFSET);
		qua = Quaternion.Euler (BONE_PINKY_TIP_R_ROT);
		rightBonesPosition [(int)EWaveVRGestureBoneType.BONE_PINKY_TIP_R] = GetOpenGLVector (vec);
		rightBonesOrientation [(int)EWaveVRGestureBoneType.BONE_PINKY_TIP_R] = GetOpenGLQuaternion (qua);

		// ----------------------------------
	}

	private WVR_PoseOriginModel gestureOriginModel = WVR_PoseOriginModel.WVR_PoseOriginModel_OriginOnHead;
	public WVR_Result GetHandTrackingData(ref WVR_HandTrackingData_t data, WVR_PoseOriginModel originModel, uint predictedMilliSec)
	{
		if (!isHandTrackingEnabled)
			return WVR_Result.WVR_Error_FeatureNotSupport;

		gestureOriginModel = originModel;

		data.left.IsValidPose = true;
		data.left.PoseTimestamp_ns = Time.frameCount;
		// Left wrist.
		data.left.PoseMatrix = leftWristMatrix;
		data.left.RawPose.position = leftBonesPosition[(int)EWaveVRGestureBoneType.BONE_HAND_WRIST_L];
		data.left.RawPose.rotation = leftBonesOrientation[(int)EWaveVRGestureBoneType.BONE_HAND_WRIST_L];

		// Left thumb.
		data.leftFinger.thumb.joint1 = leftBonesPosition [(int)EWaveVRGestureBoneType.BONE_THUMB_JOIN1_L];
		data.leftFinger.thumb.joint2 = leftBonesPosition [(int)EWaveVRGestureBoneType.BONE_THUMB_JOIN2_L];
		data.leftFinger.thumb.joint3 = leftBonesPosition [(int)EWaveVRGestureBoneType.BONE_THUMB_JOIN3_L];
		data.leftFinger.thumb.tip = leftBonesPosition [(int)EWaveVRGestureBoneType.BONE_THUMB_TIP_L];

		// Left index.
		data.leftFinger.index.joint1 = leftBonesPosition [(int)EWaveVRGestureBoneType.BONE_INDEX_JOINT1_L];
		data.leftFinger.index.joint2 = leftBonesPosition [(int)EWaveVRGestureBoneType.BONE_INDEX_JOINT2_L];
		data.leftFinger.index.joint3 = leftBonesPosition [(int)EWaveVRGestureBoneType.BONE_INDEX_JOINT3_L];
		data.leftFinger.index.tip = leftBonesPosition [(int)EWaveVRGestureBoneType.BONE_INDEX_TIP_L];

		// Left middle.
		data.leftFinger.middle.joint1 = leftBonesPosition [(int)EWaveVRGestureBoneType.BONE_MIDDLE_JOINT1_L];
		data.leftFinger.middle.joint2 = leftBonesPosition [(int)EWaveVRGestureBoneType.BONE_MIDDLE_JOINT2_L];
		data.leftFinger.middle.joint3 = leftBonesPosition [(int)EWaveVRGestureBoneType.BONE_MIDDLE_JOINT3_L];
		data.leftFinger.middle.tip = leftBonesPosition [(int)EWaveVRGestureBoneType.BONE_MIDDLE_TIP_L];

		// Left ring.
		data.leftFinger.ring.joint1 = leftBonesPosition [(int)EWaveVRGestureBoneType.BONE_RING_JOINT1_L];
		data.leftFinger.ring.joint2 = leftBonesPosition [(int)EWaveVRGestureBoneType.BONE_RING_JOINT2_L];
		data.leftFinger.ring.joint3 = leftBonesPosition [(int)EWaveVRGestureBoneType.BONE_RING_JOINT3_L];
		data.leftFinger.ring.tip = leftBonesPosition [(int)EWaveVRGestureBoneType.BONE_RING_TIP_L];

		// Left pinky.
		data.leftFinger.pinky.joint1 = leftBonesPosition [(int)EWaveVRGestureBoneType.BONE_PINKY_JOINT1_L];
		data.leftFinger.pinky.joint2 = leftBonesPosition [(int)EWaveVRGestureBoneType.BONE_PINKY_JOINT2_L];
		data.leftFinger.pinky.joint3 = leftBonesPosition [(int)EWaveVRGestureBoneType.BONE_PINKY_JOINT3_L];
		data.leftFinger.pinky.tip = leftBonesPosition [(int)EWaveVRGestureBoneType.BONE_PINKY_TIP_L];

		// ---------------------------------------------------------------------------------------

		data.right.IsValidPose = true;
		data.right.PoseTimestamp_ns = Time.frameCount;
		// Right wrist.
		data.right.PoseMatrix = rightWristMatrix;
		data.right.RawPose.position = rightBonesPosition[(int)EWaveVRGestureBoneType.BONE_HAND_WRIST_R];
		data.right.RawPose.rotation = rightBonesOrientation[(int)EWaveVRGestureBoneType.BONE_HAND_WRIST_R];

		// Right thumb.
		data.rightFinger.thumb.joint1 = rightBonesPosition [(int)EWaveVRGestureBoneType.BONE_THUMB_JOINT1_R];
		data.rightFinger.thumb.joint2 = rightBonesPosition [(int)EWaveVRGestureBoneType.BONE_THUMB_JOINT2_R];
		data.rightFinger.thumb.joint3 = rightBonesPosition [(int)EWaveVRGestureBoneType.BONE_THUMB_JOINT3_R];
		data.rightFinger.thumb.tip = rightBonesPosition [(int)EWaveVRGestureBoneType.BONE_THUMB_TIP_R];

		// Right index.
		data.rightFinger.index.joint1 = rightBonesPosition [(int)EWaveVRGestureBoneType.BONE_INDEX_JOINT1_R];
		data.rightFinger.index.joint2 = rightBonesPosition [(int)EWaveVRGestureBoneType.BONE_INDEX_JOINT2_R];
		data.rightFinger.index.joint3 = rightBonesPosition [(int)EWaveVRGestureBoneType.BONE_INDEX_JOINT3_R];
		data.rightFinger.index.tip = rightBonesPosition [(int)EWaveVRGestureBoneType.BONE_INDEX_TIP_R];

		// Right middle.
		data.rightFinger.middle.joint1 = rightBonesPosition [(int)EWaveVRGestureBoneType.BONE_MIDDLE_JOINT1_R];
		data.rightFinger.middle.joint2 = rightBonesPosition [(int)EWaveVRGestureBoneType.BONE_MIDDLE_JOINT2_R];
		data.rightFinger.middle.joint3 = rightBonesPosition [(int)EWaveVRGestureBoneType.BONE_MIDDLE_JOINT3_R];
		data.rightFinger.middle.tip = rightBonesPosition [(int)EWaveVRGestureBoneType.BONE_MIDDLE_TIP_R];

		// Right ring.
		data.rightFinger.ring.joint1 = rightBonesPosition [(int)EWaveVRGestureBoneType.BONE_RING_JOINT1_R];
		data.rightFinger.ring.joint2 = rightBonesPosition [(int)EWaveVRGestureBoneType.BONE_RING_JOINT2_R];
		data.rightFinger.ring.joint3 = rightBonesPosition [(int)EWaveVRGestureBoneType.BONE_RING_JOINT3_R];
		data.rightFinger.ring.tip = rightBonesPosition [(int)EWaveVRGestureBoneType.BONE_RING_TIP_R];

		// Right pinky.
		data.rightFinger.pinky.joint1 = rightBonesPosition [(int)EWaveVRGestureBoneType.BONE_PINKY_JOINT1_R];
		data.rightFinger.pinky.joint2 = rightBonesPosition [(int)EWaveVRGestureBoneType.BONE_PINKY_JOINT2_R];
		data.rightFinger.pinky.joint3 = rightBonesPosition [(int)EWaveVRGestureBoneType.BONE_PINKY_JOINT3_R];
		data.rightFinger.pinky.tip = rightBonesPosition [(int)EWaveVRGestureBoneType.BONE_PINKY_TIP_R];

		return WVR_Result.WVR_Success;
	}
	#endregion

	public bool IsDeviceConnected(WVR_DeviceType type)
	{
		return true;
	}

	public bool IsInputFocusCapturedBySystem()
	{
		mFocusIsCapturedBySystem = Input.GetKey (KeyCode.Escape);
		return mFocusIsCapturedBySystem;
	}

	public void InAppRecenter(WVR_RecenterType recenterType)
	{
		xOffset = -xAxis;
		yOffset = -yAxis;
		zOffset = -zAxis;
		DEBUG (xOffset + ", " + yOffset + ", " + zOffset);
	}
}
#endif
