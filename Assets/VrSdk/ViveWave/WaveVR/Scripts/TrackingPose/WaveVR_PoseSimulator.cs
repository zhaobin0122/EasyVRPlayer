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

[System.Obsolete("WaveVR_PoseSimulator is deprecated.")]
public class WaveVR_PoseSimulator : MonoBehaviour
{
	public static WaveVR_PoseSimulator Instance
	{
		get
		{
			if (instance == null)
			{
				var gameObject = new GameObject("WaveVRPoseSimulator");
				instance = gameObject.AddComponent<WaveVR_PoseSimulator>();
				// This object should survive all scene transitions.
				GameObject.DontDestroyOnLoad(instance);
			}
			return instance;
		}
	}
	private static WaveVR_PoseSimulator instance = null;

	private WaveVR_Utils.RigidTransform rtPose_head = WaveVR_Utils.RigidTransform.identity;
	private WaveVR_Utils.RigidTransform rtPose_head_onGround = WaveVR_Utils.RigidTransform.identity;
	private WaveVR_Utils.RigidTransform rtPose_right = WaveVR_Utils.RigidTransform.identity;
	private WaveVR_Utils.RigidTransform rtPose_left = WaveVR_Utils.RigidTransform.identity;
	private WaveVR_Utils.WVR_ButtonState_t btn_right, btn_left;
	private WVR_Axis_t axis_right, axis_left;

	public void GetRigidTransform(WVR_DeviceType type, ref WaveVR_Utils.RigidTransform rtPose, WVR_PoseOriginModel origin)
	{
		switch (type)
		{
		case WVR_DeviceType.WVR_DeviceType_HMD:
			if (origin == WVR_PoseOriginModel.WVR_PoseOriginModel_OriginOnGround)
				rtPose = rtPose_head_onGround;
			else
				rtPose = rtPose_head;
			break;
		case WVR_DeviceType.WVR_DeviceType_Controller_Right:
			rtPose = rtPose_right;
			break;
		case WVR_DeviceType.WVR_DeviceType_Controller_Left:
			rtPose = rtPose_left;
			break;
		default:
			break;
		}
	}

	private const string MOUSE_X = "Mouse X";
	private const string MOUSE_Y = "Mouse Y";
	private const string MOUSE_SCROLLWHEEL = "Mouse ScrollWheel";

	// position of head
	private float shiftX_head = 0, shiftY_head = 0, shiftZ_head = 0;
	// rotation of head
	private float angleX_head = 0, angleY_head = 0, angleZ_head = 0;
	// position of right controller
	private float shiftX_right = 0, shiftY_right = 0, shiftZ_right = 0;
	// rotation of right controller
	private float angleX_right = 0, angleY_right = 0, angleZ_right = 0;
	private float shiftX_left = 0, shiftY_left = 0, shiftZ_left = 0;
	private float angleX_left = 0, angleY_left = 0, angleZ_left = 0;

	private static readonly KeyCode[] KeyCode_Head = { KeyCode.LeftAlt, KeyCode.LeftControl, KeyCode.LeftShift };
	private static readonly KeyCode[] KeyCode_Right = {
		KeyCode.RightAlt, KeyCode.RightControl, KeyCode.RightShift,
		KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow
	};
	private static readonly KeyCode[] KeyCode_Left = { KeyCode.LeftAlt, KeyCode.LeftControl, KeyCode.LeftShift };

	private void UpdateHeadPose(float axis_x, float axis_y, float axis_z)
	{
		Vector3 _headPos = Vector3.zero, _headPos_onGround = Vector3.zero;
		Quaternion _headRot = Quaternion.identity;

		if (Input.GetKey (KeyCode_Head [0]))
		{
			angleX_head -= axis_y * 2.4f;
			angleX_head = Mathf.Clamp (angleX_head, -89, 89);
			angleY_head += axis_x * 5;
			if (angleY_head <= -180)
			{
				angleY_head += 360;
			} else if (angleY_head > 180)
			{
				angleY_head -= 360;
			}
		}
		if (Input.GetKey (KeyCode_Head [1]))
		{
			angleZ_head += axis_x * 5;
			angleZ_head = Mathf.Clamp (angleZ_head, -89, 89);
		}
		_headRot = Quaternion.Euler (angleX_head, angleY_head, angleZ_head);

		if (Input.GetKey (KeyCode_Head [2]))
		{
			// Assume mouse can move move 150 pixels in a-half second. 
			// So we map from 150 pixels to 0.3 meter.

			Vector3 shift = _headRot * new Vector3 (axis_x / 5, axis_y / 5, axis_z);
			shiftX_head += shift.x;
			shiftY_head += shift.y;
			shiftZ_head += shift.z;
		}
		_headPos = new Vector3 (shiftX_head, shiftY_head, shiftZ_head);
		_headPos_onGround = new Vector3 (shiftX_head, shiftY_head + 1.75f, shiftZ_head);
		rtPose_head.update (_headPos, _headRot);
		rtPose_head_onGround.update (_headPos_onGround, _headRot);
	}

	private void UpdateRightHandPose(float axis_x, float axis_y, float axis_z)
	{
		Vector3 _rightPos = Vector3.zero;
		Quaternion _rightRot = Quaternion.identity;

		//-------- mouse control ---------
		if (Input.GetKey (KeyCode_Right [0]))
		{
			angleY_right += axis_x / 2;
			angleX_right -= (float)(axis_y * 1.5f);
		}
		if (Input.GetKey (KeyCode_Right [1]))
		{
			angleZ_right += axis_z * 5;
		}
		if (Input.GetKey (KeyCode_Right [2]))
		{
			shiftX_right += axis_x / 5;
			shiftY_right += axis_y / 5;
			shiftZ_right += axis_z;
		}

		//-------- keyboard control ---------
		const float _speed = 10.0f;
		if (Input.GetKey (KeyCode_Right [3]))
		{
			shiftY_right += _speed * Time.deltaTime;
		}

		if (Input.GetKey (KeyCode_Right [4]))
		{
			shiftY_right -= _speed * Time.deltaTime;
		}

		if (Input.GetKey (KeyCode_Right [5]))
		{
			shiftX_right -= _speed * Time.deltaTime;
		}

		if (Input.GetKey (KeyCode_Right [6]))
		{
			shiftX_right += _speed * Time.deltaTime;
		}

		_rightPos = new Vector3 (shiftX_right, shiftY_right, shiftZ_right);
		_rightRot = Quaternion.Euler (angleX_right, angleY_right, angleZ_right);
		rtPose_right.update (_rightPos, _rightRot);
	}

	private void UpdateLefHandPose(float axis_x, float axis_y, float axis_z)
	{
		Vector3 _leftPos = Vector3.zero;
		Quaternion _leftRot = Quaternion.identity;

		//-------- mouse control ---------
		if (Input.GetKey (KeyCode_Left [0]))
		{
			angleY_left += axis_x / 2;
			angleX_left -= (float)(axis_y * 1.5f);
		}
		if (Input.GetKey (KeyCode_Left [1]))
		{
			angleZ_left += axis_z * 5;
		}
		if (Input.GetKey (KeyCode_Left [2]))
		{
			shiftX_left += axis_x / 5;
			shiftY_left += axis_y / 5;
			shiftZ_left += axis_z;
		}

		_leftPos = new Vector3 (shiftX_left, shiftY_left, shiftZ_left);
		_leftRot = Quaternion.Euler (angleX_left, angleY_left, angleZ_left);
		rtPose_left.update (_leftPos, _leftRot);
	}

	public void Update()
	{
		float axis_x = Input.GetAxis (MOUSE_X);
		float axis_y = Input.GetAxis (MOUSE_Y);
		float axis_z = Input.GetAxis (MOUSE_SCROLLWHEEL);

		UpdateHeadPose (axis_x, axis_y, axis_z);
		UpdateRightHandPose (axis_x, axis_y, axis_z);
		//UpdateLefHandPose(axis_x, axis_y, axis_z);
	}
}
