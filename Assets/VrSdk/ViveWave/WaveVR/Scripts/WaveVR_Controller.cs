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
using System;
using wvr.TypeExtensions;

public class WaveVR_Controller
{
	private static string LOG_TAG = "WaveVR_Controller";
	private static void DEBUG(string msg)
	{
		if (Log.EnableDebugLog)
		{
			Log.d (LOG_TAG, msg, true);
		}
	}

	public static bool IsLeftHanded
	{
		get
		{
			return isLeftHanded;
		}
	}
	private static bool isLeftHanded = false;

	public static void SetLeftHandedMode(bool lefthanded)
	{
		isLeftHanded = lefthanded;
		DEBUG ("SetLeftHandedMode() left handed? " + isLeftHanded);
	}

	public enum EDeviceType
	{
		Head = 1,
		Dominant = 2,
		NonDominant = 3
	};

	public static EDeviceType[] DeviceTypes = new EDeviceType[]{
		EDeviceType.Head,
		EDeviceType.Dominant,
		EDeviceType.NonDominant
	};

	public static EDeviceType GetEDeviceByWVRType(WVR_DeviceType type)
	{
		if (type == Input(EDeviceType.Dominant).DeviceType)
			return EDeviceType.Dominant;
		if (type == Input(EDeviceType.NonDominant).DeviceType)
			return EDeviceType.NonDominant;
		return EDeviceType.Head;
	}

	private static Device[] devices;

	/// <summary>
	/// Get the controller by device index.
	/// </summary>
	/// <param name="deviceIndex">The index of the controller.</param>
	/// <returns></returns>
	public static Device Input(WVR_DeviceType deviceIndex)
	{
		if (isLeftHanded)
		{
			switch (deviceIndex)
			{
			case WVR_DeviceType.WVR_DeviceType_Controller_Right:
				deviceIndex = WVR_DeviceType.WVR_DeviceType_Controller_Left;
				break;
			case WVR_DeviceType.WVR_DeviceType_Controller_Left:
				deviceIndex = WVR_DeviceType.WVR_DeviceType_Controller_Right;
				break;
			default:
				break;
			}
		}

		return ChangeRole (deviceIndex);
	}

	public static Device Input(EDeviceType type)
	{
		WVR_DeviceType _type = WVR_DeviceType.WVR_DeviceType_Invalid;
		switch (type)
		{
		case EDeviceType.Head:
			_type = WVR_DeviceType.WVR_DeviceType_HMD;
			break;
		case EDeviceType.Dominant:
			_type = isLeftHanded ? WVR_DeviceType.WVR_DeviceType_Controller_Left : WVR_DeviceType.WVR_DeviceType_Controller_Right;
			break;
		case EDeviceType.NonDominant:
			_type = isLeftHanded ? WVR_DeviceType.WVR_DeviceType_Controller_Right : WVR_DeviceType.WVR_DeviceType_Controller_Left;
			break;
		default:
			break;
		}

		return ChangeRole (_type);
	}

	private static Device ChangeRole(WVR_DeviceType deviceIndex)
	{
		if (devices == null)
		{
			devices = new Device[Enum.GetNames (typeof(WVR_DeviceType)).Length];
			uint i = 0;
			devices [i++] = new Device (WVR_DeviceType.WVR_DeviceType_Invalid);
			devices [i++] = new Device (WVR_DeviceType.WVR_DeviceType_HMD);
			devices [i++] = new Device (WVR_DeviceType.WVR_DeviceType_Controller_Right);
			devices [i++] = new Device (WVR_DeviceType.WVR_DeviceType_Controller_Left);
		}

		for (uint i = 0; i < devices.Length; i++)
		{
			if (deviceIndex == devices [i].DeviceType)
			{
				return devices [i];
			}
		}

		return null;
	}

	public class Device
	{
		#region button definition
		// ============================== press ==============================
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

		// Timer of each button (has press state) should be seperated.
		int[] prevFrameCount_press = new int[pressIds.Length];
		bool[] state_press = new bool[pressIds.Length];
		bool[] prevState_press = new bool[pressIds.Length];
		bool[] event_state_press = new bool[pressIds.Length];


		// ============================== touch ==============================
		static WVR_InputId[] touchIds = new WVR_InputId[] {
			WVR_InputId.WVR_InputId_Alias1_Touchpad,
			WVR_InputId.WVR_InputId_Alias1_Trigger,
			WVR_InputId.WVR_InputId_Alias1_Thumbstick
		};

		// Timer of each button (has touch state) should be seperated.
		int[] prevFrameCount_touch = new int[touchIds.Length];
		bool[] state_touch = new bool[touchIds.Length];
		bool[] prevState_touch = new bool[touchIds.Length];
		bool[] event_state_touch = new bool[touchIds.Length];
		#endregion

		public Device(WVR_DeviceType dt)
		{
			DEBUG ("Initialize WaveVR_Controller Device: " + dt);
			DeviceType = dt;

			ResetAllButtonStates();
		}

		public void SetEventState_Press(WVR_InputId btn, bool down)
		{
			for (int _p = 0; _p < pressIds.Length; _p++)
			{
				if (pressIds [_p] == btn)
				{
					event_state_press [_p] = down;
					DEBUG ("SetEventState_Press() " + this.DeviceType + ", " + pressIds [_p] + ": " + event_state_press [_p]);
					break;
				}
			}
		}

		public bool GetEventState_Press(WVR_InputId btn)
		{
			bool _press = false;
			for (int _p = 0; _p < pressIds.Length; _p++)
			{
				if (pressIds [_p] == btn)
				{
					_press = event_state_press [_p];
					break;
				}
			}
			return _press;
		}

		public void SetEventState_Touch(WVR_InputId btn, bool down)
		{
			for (int _t = 0; _t < touchIds.Length; _t++)
			{
				if (touchIds [_t] == btn)
				{
					event_state_touch [_t] = down;
					DEBUG ("SetEventState_Touch() " + this.DeviceType + ", " + touchIds [_t] + ": " + event_state_touch [_t]);
					break;
				}
			}
		}

		public bool GetEventState_Touch(WVR_InputId btn)
		{
			bool _touch = false;
			for (int _t = 0; _t < touchIds.Length; _t++)
			{
				if (touchIds [_t] == btn)
				{
					_touch = event_state_touch [_t];
					break;
				}
			}
			return _touch;
		}

		public WVR_DeviceType DeviceType
		{
			get;
			private set;
		}

		int prevFrame_connected = -1;
		private bool AllowGetConnectionState()
		{
			if (Time.frameCount != prevFrame_connected)
			{
				prevFrame_connected = Time.frameCount;
				return true;
			}

			return false;
		}

		private bool _connected = false;
		/// Whether is the device connected.
		public bool connected
		{
			get {
				return _connected;
			}
			set {
				_connected = value;
				DEBUG ("Device " + DeviceType + " is " + (_connected ? "connected." : "disconnected."));
			}
		}

		int prevFrame_pose = -1;
		private bool AllowGetPoseState()
		{
			if (!this._connected)
				return false;

			if (Time.frameCount != prevFrame_pose)
			{
				prevFrame_pose = Time.frameCount;
				return true;
			}

			return false;
		}

		private WVR_PoseState_t pose;
		private WaveVR_Utils.RigidTransform rtPose = WaveVR_Utils.RigidTransform.identity;
		/// Gets the RigidTransform {pos=Vector3, rot=Rotation}
		public WaveVR_Utils.RigidTransform transform
		{
			get
			{
				if (!WaveVR.Instance.Initialized)
					return this.rtPose;

				switch (this.DeviceType)
				{
				case WVR_DeviceType.WVR_DeviceType_HMD:
					this.rtPose = WaveVR.Instance.hmd.rigidTransform;
					break;
				case WVR_DeviceType.WVR_DeviceType_Controller_Right:
					this.rtPose = WaveVR.Instance.controllerRight.rigidTransform;
					break;
				case WVR_DeviceType.WVR_DeviceType_Controller_Left:
					this.rtPose = WaveVR.Instance.controllerLeft.rigidTransform;
					break;
				default:
					break;
				}
				return this.rtPose;
			}
		}

		private WVR_Vector3f_t vel = new WVR_Vector3f_t ();
		[Obsolete("This variable will be obsoleted in next release, please use V3Velocity instead.")]
		public WVR_Vector3f_t velocity
		{
			get {
				if (!WaveVR.Instance.Initialized)
					return this.vel;

				switch (this.DeviceType)
				{
				case WVR_DeviceType.WVR_DeviceType_HMD:
					this.vel = WaveVR.Instance.hmd.pose.pose.Velocity;
					break;
				case WVR_DeviceType.WVR_DeviceType_Controller_Right:
					this.vel = WaveVR.Instance.controllerRight.pose.pose.Velocity;
					break;
				case WVR_DeviceType.WVR_DeviceType_Controller_Left:
					this.vel = WaveVR.Instance.controllerLeft.pose.pose.Velocity;
					break;
				default:
					break;
				}
				return this.vel;
			}
		}

		private Vector3 v3velocity = Vector3.zero;
		public Vector3 V3Velocity
		{
			get {
				if (!WaveVR.Instance.Initialized)
					return v3velocity;

				switch (this.DeviceType)
				{
				case WVR_DeviceType.WVR_DeviceType_HMD:
					WaveVR_Utils.GetVectorFromGL (WaveVR.Instance.hmd.pose.pose.Velocity, out v3velocity);
					break;
				case WVR_DeviceType.WVR_DeviceType_Controller_Right:
					WaveVR_Utils.GetVectorFromGL (WaveVR.Instance.controllerRight.pose.pose.Velocity, out v3velocity);
					break;
				case WVR_DeviceType.WVR_DeviceType_Controller_Left:
					WaveVR_Utils.GetVectorFromGL (WaveVR.Instance.controllerLeft.pose.pose.Velocity, out v3velocity);
					break;
				default:
					break;
				}

				return v3velocity;
			}
		}

		private WVR_Vector3f_t aVel = new WVR_Vector3f_t ();
		[Obsolete("This variable will be obsoleted in next release, please use V3AngularVelocity instead.")]
		public WVR_Vector3f_t AngularVelocity
		{
			get {
				if (!WaveVR.Instance.Initialized)
					return this.aVel;

				switch (this.DeviceType)
				{
				case WVR_DeviceType.WVR_DeviceType_HMD:
					this.aVel = WaveVR.Instance.hmd.pose.pose.AngularVelocity;
					break;
				case WVR_DeviceType.WVR_DeviceType_Controller_Right:
					this.aVel = WaveVR.Instance.controllerRight.pose.pose.AngularVelocity;
					break;
				case WVR_DeviceType.WVR_DeviceType_Controller_Left:
					this.aVel = WaveVR.Instance.controllerLeft.pose.pose.AngularVelocity;
					break;
				default:
					break;
				}
				return this.aVel;
			}
		}

		private Vector3 v3AngularVelocity = Vector3.zero;
		public Vector3 V3AngularVelocity
		{
			get {
				if (!WaveVR.Instance.Initialized)
					return v3AngularVelocity;

				switch (this.DeviceType)
				{
				case WVR_DeviceType.WVR_DeviceType_HMD:
					WaveVR_Utils.GetVectorFromGL (WaveVR.Instance.hmd.pose.pose.AngularVelocity, out v3AngularVelocity);
					break;
				case WVR_DeviceType.WVR_DeviceType_Controller_Right:
					WaveVR_Utils.GetVectorFromGL (WaveVR.Instance.controllerRight.pose.pose.AngularVelocity, out v3AngularVelocity);
					break;
				case WVR_DeviceType.WVR_DeviceType_Controller_Left:
					WaveVR_Utils.GetVectorFromGL (WaveVR.Instance.controllerLeft.pose.pose.AngularVelocity, out v3AngularVelocity);
					break;
				default:
					break;
				}

				return v3AngularVelocity;
			}
		}

		//private WaveVR_Utils.WVR_ButtonState_t state, pre_state;

		#region Timer
		private bool AllowPressActionInAFrame(WVR_InputId _id)
		{
			for (uint i = 0; i < pressIds.Length; i++)
			{
				if (_id == pressIds [i])
				{
					if (Time.frameCount != prevFrameCount_press [i])
					{
						prevFrameCount_press [i] = Time.frameCount;
						return true;
					}
				}
			}

			return false;
		}

		private bool AllowTouchActionInAFrame(WVR_InputId _id)
		{
			for (uint i = 0; i < touchIds.Length; i++)
			{
				if (_id == touchIds [i])
				{
					if (Time.frameCount != prevFrameCount_touch [i])
					{
						prevFrameCount_touch [i] = Time.frameCount;
						return true;
					}
				}
			}

			return false;
		}
		#endregion

		private void Update_PressState(WVR_InputId _id)
		{
			if (AllowPressActionInAFrame (_id))
			{
				bool _pressed = GetEventState_Press(_id);

				for (int _p = 0; _p < pressIds.Length; _p++)
				{
					if (_id == pressIds [_p])
					{
						prevState_press [_p] = state_press [_p];
						state_press [_p] = _pressed;
					}
				}
			}
		}

		private void Update_TouchState(WVR_InputId _id)
		{
			if (AllowTouchActionInAFrame (_id))
			{
				bool _touched = GetEventState_Touch(_id);

				for (int _t = 0; _t < touchIds.Length; _t++)
				{
					if (_id == touchIds [_t])
					{
						prevState_touch [_t] = state_touch [_t];
						state_touch [_t] = _touched;
					}
				}
			}
		}

		public void ResetAllButtonStates()
		{
			DEBUG ("ResetAllButtonStates() " + DeviceType);
			for (int _p = 0; _p < pressIds.Length; _p++)
			{
				prevFrameCount_press[_p] = -1;
				state_press[_p] = false;
				prevState_press[_p] = false;
				event_state_press [_p] = false;
			}
			for (int _t = 0; _t < touchIds.Length; _t++)
			{
				prevFrameCount_touch[_t] = -1;
				state_touch[_t] = false;
				prevState_touch[_t] = false;
				event_state_touch [_t] = false;
			}
		}

		/// <summary>
		/// Reset the button event only.
		/// The flow in Update() of end of frame will be:
		/// 1. prevState = state
		/// 2. state = event_state
		/// If preState = true, state will change from true -> false which means button up.
		/// </summary>
		public void ResetButtonEvents()
		{
			DEBUG ("ResetButtonEvents() " + DeviceType);
			for (int _p = 0; _p < pressIds.Length; _p++)
				event_state_press [_p] = false;

			for (int _t = 0; _t < touchIds.Length; _t++)
				event_state_touch [_t] = false;
		}

		/// <summary>
		/// Update the button event only.
		/// The flow in Update() of end of frame will be:
		/// 1. prevState = state
		/// 2. state = event_state
		///
		/// Note: This API is called only when
		/// 1. Application resume (WaveVR_Init)
		/// 2. The first frame of getting system focus. (WaveVR)
		/// </summary>
		public void UpdateButtonEvents()
		{
			if (!WaveVR.Instance.Initialized)
				return;

			for (int _p = 0; _p < pressIds.Length; _p++)
			{
				bool _pressed = Interop.WVR_GetInputButtonState (DeviceType, pressIds [_p]);
				event_state_press [_p] = _pressed;
				DEBUG (Log.CSB
					.Append("UpdateButtonEvents() ").Append(DeviceType.Name()).Append(" ")
					.Append(pressIds [_p].Name()).Append(" is ")
					.Append(event_state_press [_p] ? "pressed." : "not pressed.")
					.ToString());
			}
			for (int _t = 0; _t < touchIds.Length; _t++)
			{
				bool _touched = Interop.WVR_GetInputTouchState (DeviceType, touchIds [_t]);
				event_state_touch [_t] = _touched;
				DEBUG (Log.CSB
					.Append("UpdateButtonEvents() ").Append(DeviceType.Name()).Append(" ")
					.Append(touchIds [_t].Name()).Append(" is ")
					.Append(event_state_touch [_t] ? "touched." : "not touched.")
					.ToString());
			}
		}

		#region Button Press state
		public bool GetPress(WaveVR_ButtonList.EButtons btn) { return GetPress ((WVR_InputId)btn); }
		/// <summary>
		/// Check if button state is equivallent to specified state.
		/// </summary>
		/// <returns><c>true</c>, equal, <c>false</c> otherwise.</returns>
		/// <param name="_id">input button</param>
		public bool GetPress(WVR_InputId _id)
		{
			bool _state = false;
			Update_PressState (_id);

			for (int _p = 0; _p < pressIds.Length; _p++)
			{
				if (_id == pressIds [_p])
				{
					_state = state_press [_p];
					break;
				}
			}

			return _state;
		}

		public bool GetPressDown(WaveVR_ButtonList.EButtons btn) { return GetPressDown ((WVR_InputId)btn); }
		/// <summary>
		/// If true, button with _id is pressed, else unpressed.
		/// </summary>
		/// <returns><c>true</c>, if press down was gotten, <c>false</c> otherwise.</returns>
		/// <param name="_id">WVR_InputId, id of button</param>
		public bool GetPressDown(WVR_InputId _id)
		{
			bool _state = false;
			Update_PressState (_id);

			for (int _p = 0; _p < pressIds.Length; _p++)
			{
				if (_id == pressIds [_p])
				{
					_state = (
						(prevState_press [_p] == false) &&
						(state_press [_p] == true)
					);
					break;
				}
			}

			return _state;
		}

		public bool GetPressUp(WaveVR_ButtonList.EButtons btn) { return GetPressUp ((WVR_InputId)btn); }
		/// <summary>
		/// If true, button with _id is unpressed, else pressed.
		/// </summary>
		/// <returns><c>true</c>, if unpress up was gotten, <c>false</c> otherwise.</returns>
		/// <param name="_id">WVR_ButtonId, id of button</param>
		public bool GetPressUp(WVR_InputId _id)
		{
			bool _state = false;
			Update_PressState (_id);

			for (int _p = 0; _p < pressIds.Length; _p++)
			{
				if (_id == pressIds [_p])
				{
					_state = (
						(prevState_press [_p] == true) &&
						(state_press [_p] == false)
					);
					break;
				}
			}

			return _state;
		}
		#endregion

		#region Button Touch state
		public bool GetTouch(WaveVR_ButtonList.EButtons btn) { return GetTouch ((WVR_InputId)btn); }
		/// <summary>
		/// If true, button with _id is touched, else untouched..
		/// </summary>
		/// <returns><c>true</c>, if touch was gotten, <c>false</c> otherwise.</returns>
		/// <param name="_id">WVR_ButtonId, id of button</param>
		public bool GetTouch(WVR_InputId _id)
		{
			bool _state = false;
			Update_TouchState (_id);

			for (int _t = 0; _t < touchIds.Length; _t++)
			{
				if (_id == touchIds [_t])
				{
					_state = state_touch [_t];
					break;
				}
			}

			return _state;
		}

		public bool GetTouchDown(WaveVR_ButtonList.EButtons btn) { return GetTouchDown ((WVR_InputId)btn); }
		/// <summary>
		/// If true, button with _id is touched, else untouched..
		/// </summary>
		/// <returns><c>true</c>, if touch was gotten, <c>false</c> otherwise.</returns>
		/// <param name="_id">WVR_ButtonId, id of button</param>
		public bool GetTouchDown(WVR_InputId _id)
		{
			bool _state = false;
			Update_TouchState (_id);

			for (int _t = 0; _t < touchIds.Length; _t++)
			{
				if (_id == touchIds [_t])
				{
					_state = (
						(prevState_touch [_t] == false) &&
						(state_touch [_t] == true)
					);
					break;
				}
			}

			return _state;
		}

		public bool GetTouchUp(WaveVR_ButtonList.EButtons btn) { return GetTouchUp ((WVR_InputId)btn); }
		/// <summary>
		/// If true, button with _id is touched, else untouched..
		/// </summary>
		/// <returns><c>true</c>, if touch was gotten, <c>false</c> otherwise.</returns>
		/// <param name="_id">WVR_ButtonId, id of button</param>
		public bool GetTouchUp(WVR_InputId _id)
		{
			bool _state = false;
			Update_TouchState (_id);

			for (int _t = 0; _t < touchIds.Length; _t++)
			{
				if (_id == touchIds [_t])
				{
					_state = (
						(prevState_touch [_t] == true) &&
						(state_touch [_t] == false)
					);
					break;
				}
			}

			return _state;
		}
		#endregion

		private WVR_Axis_t axis;
		public Vector2 GetAxis(WaveVR_ButtonList.EButtons btn) { return GetAxis ((WVR_InputId)btn); }
		public Vector2 GetAxis(WVR_InputId btn)
		{
			// No axis if disconnected or untouched.
			if (!_connected || !GetTouch (btn))
				return Vector2.zero;

			axis = Interop.WVR_GetInputAnalogAxis (DeviceType, btn);
			return new Vector2 (axis.x, axis.y);
		}

		/// <summary>
		/// Triggers the haptic pulse.
		/// This function is for vibrating 1 time during the "durationMicroSec" micro seconds with normal strength.
		/// For changing the frequency or the strength, please use "TriggerVibration" instead.
		/// </summary>
		/// <param name="_durationMicroSec">Duration micro sec.</param>
		/// <param name="_id">Identifier.</param>
		public void TriggerHapticPulse(
			uint durationMicroSec = 1000000,
			WVR_InputId id = WVR_InputId.WVR_InputId_Alias1_Touchpad
		)
		{
			TriggerVibration (id, durationMicroSec, 1, WVR_Intensity.WVR_Intensity_Normal);
		}

		/// <summary>
		/// Triggers the vibration.
		/// </summary>
		/// <param name="id">Button ID.</param>
		/// <param name="durationMicroSec">Duration micro sec.</param>
		/// <param name="frequency">Frequency.</param>
		/// <param name="intensity">Vibration strength.</param>
		public void TriggerVibration(
			WVR_InputId id,
			uint durationMicroSec,
			uint frequency,
			WVR_Intensity intensity)
		{
			if (!this.connected)
				return;

			Interop.WVR_TriggerVibration (DeviceType, id, durationMicroSec, frequency, intensity);
		}

	} // public class Device
}
