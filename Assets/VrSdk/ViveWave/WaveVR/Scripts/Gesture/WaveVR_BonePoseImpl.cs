using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using wvr;
using WVR_Log;
using System;

public class WaveVR_BonePoseImpl {
	private const string LOG_TAG = "WaveVR_BonePoseImpl";
	private void DEBUG(string msg)
	{
		if (Log.EnableDebugLog)
			Log.d (LOG_TAG, msg, true);
	}
	public enum Bones
	{
		ROOT = 0,

		LEFT_WRIST,
		LEFT_THUMB_JOINT1,
		LEFT_THUMB_JOINT2,
		LEFT_THUMB_JOINT3,
		LEFT_THUMB_TIP,
		LEFT_INDEX_JOINT1,
		LEFT_INDEX_JOINT2,
		LEFT_INDEX_JOINT3,
		LEFT_INDEX_TIP,
		LEFT_MIDDLE_JOINT1,
		LEFT_MIDDLE_JOINT2,
		LEFT_MIDDLE_JOINT3,
		LEFT_MIDDLE_TIP,
		LEFT_RING_JOINT1,
		LEFT_RING_JOINT2,
		LEFT_RING_JOINT3,
		LEFT_RING_TIP,
		LEFT_PINKY_JOINT1,
		LEFT_PINKY_JOINT2,
		LEFT_PINKY_JOINT3,
		LEFT_PINKY_TIP,

		RIGHT_WRIST,
		RIGHT_THUMB_JOINT1,
		RIGHT_THUMB_JOINT2,
		RIGHT_THUMB_JOINT3,
		RIGHT_THUMB_TIP,
		RIGHT_INDEX_JOINT1,
		RIGHT_INDEX_JOINT2,
		RIGHT_INDEX_JOINT3,
		RIGHT_INDEX_TIP,
		RIGHT_MIDDLE_JOINT1,
		RIGHT_MIDDLE_JOINT2,
		RIGHT_MIDDLE_JOINT3,
		RIGHT_MIDDLE_TIP,
		RIGHT_RING_JOINT1,
		RIGHT_RING_JOINT2,
		RIGHT_RING_JOINT3,
		RIGHT_RING_TIP,
		RIGHT_PINKY_JOINT1,
		RIGHT_PINKY_JOINT2,
		RIGHT_PINKY_JOINT3,
		RIGHT_PINKY_TIP,
	};

	private static Bones[] leftBones = new Bones[] {
		Bones.LEFT_WRIST,
		Bones.LEFT_THUMB_JOINT1,
		Bones.LEFT_THUMB_JOINT2,
		Bones.LEFT_THUMB_JOINT3,
		Bones.LEFT_THUMB_TIP,
		Bones.LEFT_INDEX_JOINT1,
		Bones.LEFT_INDEX_JOINT2,
		Bones.LEFT_INDEX_JOINT3,
		Bones.LEFT_INDEX_TIP,
		Bones.LEFT_MIDDLE_JOINT1,
		Bones.LEFT_MIDDLE_JOINT2,
		Bones.LEFT_MIDDLE_JOINT3,
		Bones.LEFT_MIDDLE_TIP,
		Bones.LEFT_RING_JOINT1,
		Bones.LEFT_RING_JOINT2,
		Bones.LEFT_RING_JOINT3,
		Bones.LEFT_RING_TIP,
		Bones.LEFT_PINKY_JOINT1,
		Bones.LEFT_PINKY_JOINT2,
		Bones.LEFT_PINKY_JOINT3,
		Bones.LEFT_PINKY_TIP
	};

	private static Bones[] rightBones = new Bones[] {
		Bones.RIGHT_WRIST,
		Bones.RIGHT_THUMB_JOINT1,
		Bones.RIGHT_THUMB_JOINT2,
		Bones.RIGHT_THUMB_JOINT3,
		Bones.RIGHT_THUMB_TIP,
		Bones.RIGHT_INDEX_JOINT1,
		Bones.RIGHT_INDEX_JOINT2,
		Bones.RIGHT_INDEX_JOINT3,
		Bones.RIGHT_INDEX_TIP,
		Bones.RIGHT_MIDDLE_JOINT1,
		Bones.RIGHT_MIDDLE_JOINT2,
		Bones.RIGHT_MIDDLE_JOINT3,
		Bones.RIGHT_MIDDLE_TIP,
		Bones.RIGHT_RING_JOINT1,
		Bones.RIGHT_RING_JOINT2,
		Bones.RIGHT_RING_JOINT3,
		Bones.RIGHT_RING_TIP,
		Bones.RIGHT_PINKY_JOINT1,
		Bones.RIGHT_PINKY_JOINT2,
		Bones.RIGHT_PINKY_JOINT3,
		Bones.RIGHT_PINKY_TIP
	};

	private class BoneData
	{
		private WaveVR_Utils.RigidTransform rigidTransform = WaveVR_Utils.RigidTransform.identity;
		private bool valid = false;

		public BoneData(){
			rigidTransform = WaveVR_Utils.RigidTransform.identity;
			valid = false;
		}

		public WaveVR_Utils.RigidTransform GetTransform() { return rigidTransform; }
		public Vector3 GetPosition() { return rigidTransform.pos; }
		public void SetPosition(Vector3 in_pos) { rigidTransform.pos = in_pos; }
		public Quaternion GetRotation() { return rigidTransform.rot; }
		public void SetRotation(Quaternion in_rot) { rigidTransform.rot = in_rot; }
		public bool IsValidPose() { return valid; }
		public void SetValidPose(bool in_valid) { valid = in_valid; }
	};

	private BoneData[] boneDatas;

	public WaveVR_BonePoseImpl()
	{
		DEBUG ("WaveVR_BonePoseImpl()");
		boneDatas = new BoneData[Enum.GetNames (typeof(Bones)).Length];
		for (int i = 0; i < Enum.GetNames (typeof(Bones)).Length; i++)
		{
			boneDatas [i] = new BoneData ();
			boneDatas [i].SetValidPose (false);
		}
	}

	int prevFrame_tracking = -1;
	private bool AllowGetTrackingData()
	{
		if (Time.frameCount != prevFrame_tracking)
		{
			prevFrame_tracking = Time.frameCount;
			return true;
		}

		return false;
	}

	private bool hasHandTrackingData = false;
	private WVR_HandTrackingData_t handTrackingData = new WVR_HandTrackingData_t();
	public WaveVR_Utils.RigidTransform GetBoneTransform(Bones bone_type)
	{
		if (AllowGetTrackingData ())
		{
			hasHandTrackingData = WaveVR_GestureManager.Instance.GetHandTrackingData (ref handTrackingData, WaveVR_Render.Instance.origin, 0);
			if (hasHandTrackingData)
			{
				if (handTrackingData.left.IsValidPose)
					UpdateLeftHandTrackingData ();

				if (handTrackingData.right.IsValidPose)
					UpdateRightHandTrackingData ();
			}
		}

		return boneDatas[(int)bone_type].GetTransform();
	}

	public bool IsBonePoseValid(Bones bone_type)
	{
		for (int i = 0; i < leftBones.Length; i++)
		{
			if (leftBones [i] == bone_type)
				return IsHandPoseValid (WaveVR_GestureManager.EGestureHand.LEFT);
		}

		for (int i = 0; i < rightBones.Length; i++)
		{
			if (rightBones [i] == bone_type)
				return IsHandPoseValid (WaveVR_GestureManager.EGestureHand.RIGHT);
		}

		return false;
	}

	public bool IsHandPoseValid(WaveVR_GestureManager.EGestureHand hand)
	{
		if (hasHandTrackingData)
		{
			if (hand == WaveVR_GestureManager.EGestureHand.LEFT)
				return handTrackingData.left.IsValidPose;
			if (hand == WaveVR_GestureManager.EGestureHand.RIGHT)
				return handTrackingData.right.IsValidPose;
		}

		return false;
	}

	private WaveVR_Utils.RigidTransform rtWristLeft = WaveVR_Utils.RigidTransform.identity;
	private void UpdateLeftHandTrackingData()
	{
		// Left wrist - LEFT_WRIST
		rtWristLeft.update (handTrackingData.left.PoseMatrix);
		Vector3 LEFT_WRIST_Pos = rtWristLeft.pos;
		Quaternion LEFT_WRIST_Rot = rtWristLeft.rot;

		boneDatas [(int)Bones.LEFT_WRIST].SetPosition (LEFT_WRIST_Pos);
		boneDatas [(int)Bones.LEFT_WRIST].SetRotation (LEFT_WRIST_Rot);
		boneDatas [(int)Bones.LEFT_WRIST].SetValidPose (handTrackingData.left.IsValidPose);

		// Left thumb joint1 - LEFT_THUMB_JOINT1
		Vector3 LEFT_THUMB_JOINT1_Pos = WaveVR_Utils.GetPosition (handTrackingData.leftFinger.thumb.joint1);
		Quaternion LEFT_THUMB_JOINT1_Rot = Quaternion.LookRotation (LEFT_THUMB_JOINT1_Pos - LEFT_WRIST_Pos);

		boneDatas [(int)Bones.LEFT_THUMB_JOINT1].SetPosition (LEFT_THUMB_JOINT1_Pos);
		boneDatas [(int)Bones.LEFT_THUMB_JOINT1].SetRotation (LEFT_THUMB_JOINT1_Rot);
		boneDatas [(int)Bones.LEFT_THUMB_JOINT1].SetValidPose (handTrackingData.left.IsValidPose);

		// Left thumb joint2 - LEFT_THUMB_JOINT2
		Vector3 LEFT_THUMB_JOINT2_Pos = WaveVR_Utils.GetPosition (handTrackingData.leftFinger.thumb.joint2);
		Quaternion LEFT_THUMB_JOINT2_Rot = Quaternion.LookRotation (LEFT_THUMB_JOINT2_Pos - LEFT_THUMB_JOINT1_Pos);

		boneDatas [(int)Bones.LEFT_THUMB_JOINT2].SetPosition (LEFT_THUMB_JOINT2_Pos);
		boneDatas [(int)Bones.LEFT_THUMB_JOINT2].SetRotation (LEFT_THUMB_JOINT2_Rot);
		boneDatas [(int)Bones.LEFT_THUMB_JOINT2].SetValidPose (handTrackingData.left.IsValidPose);

		// Left thumb joint3 - LEFT_THUMB_JOINT3
		Vector3 LEFT_THUMB_JOINT3_Pos = WaveVR_Utils.GetPosition (handTrackingData.leftFinger.thumb.joint3);
		Quaternion LEFT_THUMB_JOINT3_Rot = Quaternion.LookRotation (LEFT_THUMB_JOINT3_Pos - LEFT_THUMB_JOINT2_Pos);

		boneDatas [(int)Bones.LEFT_THUMB_JOINT3].SetPosition (LEFT_THUMB_JOINT3_Pos);
		boneDatas [(int)Bones.LEFT_THUMB_JOINT3].SetRotation (LEFT_THUMB_JOINT3_Rot);
		boneDatas [(int)Bones.LEFT_THUMB_JOINT3].SetValidPose (handTrackingData.left.IsValidPose);

		// Left thumb tip - LEFT_THUMB_TIP
		Vector3 LEFT_THUMB_TIP_Pos = WaveVR_Utils.GetPosition (handTrackingData.leftFinger.thumb.tip);
		Quaternion LEFT_THUMB_TIP_Rot = Quaternion.LookRotation (LEFT_THUMB_TIP_Pos - LEFT_THUMB_JOINT3_Pos);

		boneDatas [(int)Bones.LEFT_THUMB_TIP].SetPosition (LEFT_THUMB_TIP_Pos);
		boneDatas [(int)Bones.LEFT_THUMB_TIP].SetRotation (LEFT_THUMB_TIP_Rot);
		boneDatas [(int)Bones.LEFT_THUMB_TIP].SetValidPose (handTrackingData.left.IsValidPose);

		// Left index joint1 - LEFT_INDEX_JOINT1
		Vector3 LEFT_INDEX_JOINT1_Pos = WaveVR_Utils.GetPosition (handTrackingData.leftFinger.index.joint1);
		Quaternion LEFT_INDEX_JOINT1_Rot = Quaternion.LookRotation (LEFT_INDEX_JOINT1_Pos - LEFT_WRIST_Pos);

		boneDatas [(int)Bones.LEFT_INDEX_JOINT1].SetPosition (LEFT_INDEX_JOINT1_Pos);
		boneDatas [(int)Bones.LEFT_INDEX_JOINT1].SetRotation (LEFT_INDEX_JOINT1_Rot);
		boneDatas [(int)Bones.LEFT_INDEX_JOINT1].SetValidPose (handTrackingData.left.IsValidPose);

		// Left index joint2 - LEFT_INDEX_JOINT2
		Vector3 LEFT_INDEX_JOINT2_Pos = WaveVR_Utils.GetPosition (handTrackingData.leftFinger.index.joint2);
		Quaternion LEFT_INDEX_JOINT2_Rot = Quaternion.LookRotation (LEFT_INDEX_JOINT2_Pos - LEFT_INDEX_JOINT1_Pos);

		boneDatas [(int)Bones.LEFT_INDEX_JOINT2].SetPosition (LEFT_INDEX_JOINT2_Pos);
		boneDatas [(int)Bones.LEFT_INDEX_JOINT2].SetRotation (LEFT_INDEX_JOINT2_Rot);
		boneDatas [(int)Bones.LEFT_INDEX_JOINT2].SetValidPose (handTrackingData.left.IsValidPose);

		// Left index joint3 - LEFT_INDEX_JOINT3
		Vector3 LEFT_INDEX_JOINT3_Pos = WaveVR_Utils.GetPosition (handTrackingData.leftFinger.index.joint3);
		Quaternion LEFT_INDEX_JOINT3_Rot = Quaternion.LookRotation (LEFT_INDEX_JOINT3_Pos, LEFT_INDEX_JOINT2_Pos);

		boneDatas [(int)Bones.LEFT_INDEX_JOINT3].SetPosition (LEFT_INDEX_JOINT3_Pos);
		boneDatas [(int)Bones.LEFT_INDEX_JOINT3].SetRotation (LEFT_INDEX_JOINT3_Rot);
		boneDatas [(int)Bones.LEFT_INDEX_JOINT3].SetValidPose (handTrackingData.left.IsValidPose);

		// Left index tip - LEFT_INDEX_TIP
		Vector3 LEFT_INDEX_TIP_Pos = WaveVR_Utils.GetPosition (handTrackingData.leftFinger.index.tip);
		Quaternion LEFT_INDEX_TIP_Rot = Quaternion.LookRotation (LEFT_INDEX_TIP_Pos - LEFT_INDEX_JOINT3_Pos);

		boneDatas [(int)Bones.LEFT_INDEX_TIP].SetPosition (LEFT_INDEX_TIP_Pos);
		boneDatas [(int)Bones.LEFT_INDEX_TIP].SetRotation (LEFT_INDEX_TIP_Rot);
		boneDatas [(int)Bones.LEFT_INDEX_TIP].SetValidPose (handTrackingData.left.IsValidPose);

		// Left middle joint1 - LEFT_MIDDLE_JOINT1
		Vector3 LEFT_MIDDLE_JOINT1_Pos = WaveVR_Utils.GetPosition (handTrackingData.leftFinger.middle.joint1);
		Quaternion LEFT_MIDDLE_JOINT1_Rot = Quaternion.LookRotation (LEFT_MIDDLE_JOINT1_Pos - LEFT_WRIST_Pos);

		boneDatas [(int)Bones.LEFT_MIDDLE_JOINT1].SetPosition (LEFT_MIDDLE_JOINT1_Pos);
		boneDatas [(int)Bones.LEFT_MIDDLE_JOINT1].SetRotation (LEFT_MIDDLE_JOINT1_Rot);
		boneDatas [(int)Bones.LEFT_MIDDLE_JOINT1].SetValidPose (handTrackingData.left.IsValidPose);

		// Left middle joint2 - LEFT_MIDDLE_JOINT2
		Vector3 LEFT_MIDDLE_JOINT2_Pos = WaveVR_Utils.GetPosition (handTrackingData.leftFinger.middle.joint2);
		Quaternion LEFT_MIDDLE_JOINT2_Rot = Quaternion.LookRotation (LEFT_MIDDLE_JOINT2_Pos - LEFT_MIDDLE_JOINT1_Pos);

		boneDatas [(int)Bones.LEFT_MIDDLE_JOINT2].SetPosition (LEFT_MIDDLE_JOINT2_Pos);
		boneDatas [(int)Bones.LEFT_MIDDLE_JOINT2].SetRotation (LEFT_MIDDLE_JOINT2_Rot);
		boneDatas [(int)Bones.LEFT_MIDDLE_JOINT2].SetValidPose (handTrackingData.left.IsValidPose);

		// Left middle joint3 - LEFT_MIDDLE_JOINT3
		Vector3 LEFT_MIDDLE_JOINT3_Pos = WaveVR_Utils.GetPosition (handTrackingData.leftFinger.middle.joint3);
		Quaternion LEFT_MIDDLE_JOINT3_Rot = Quaternion.LookRotation (LEFT_MIDDLE_JOINT3_Pos - LEFT_MIDDLE_JOINT2_Pos);

		boneDatas [(int)Bones.LEFT_MIDDLE_JOINT3].SetPosition (LEFT_MIDDLE_JOINT3_Pos);
		boneDatas [(int)Bones.LEFT_MIDDLE_JOINT3].SetRotation (LEFT_MIDDLE_JOINT3_Rot);
		boneDatas [(int)Bones.LEFT_MIDDLE_JOINT3].SetValidPose (handTrackingData.left.IsValidPose);

		// Left middle tip - LEFT_MIDDLE_TIP
		Vector3 LEFT_MIDDLE_TIP_Pos = WaveVR_Utils.GetPosition (handTrackingData.leftFinger.middle.tip);
		Quaternion LEFT_MIDDLE_TIP_Rot = Quaternion.LookRotation (LEFT_MIDDLE_TIP_Pos - LEFT_MIDDLE_JOINT3_Pos);

		boneDatas [(int)Bones.LEFT_MIDDLE_TIP].SetPosition (LEFT_MIDDLE_TIP_Pos);
		boneDatas [(int)Bones.LEFT_MIDDLE_TIP].SetRotation (LEFT_MIDDLE_TIP_Rot);
		boneDatas [(int)Bones.LEFT_MIDDLE_TIP].SetValidPose (handTrackingData.left.IsValidPose);

		// Left ring joint1 - LEFT_RING_JOINT1
		Vector3 LEFT_RING_JOINT1_Pos = WaveVR_Utils.GetPosition (handTrackingData.leftFinger.ring.joint1);
		Quaternion LEFT_RING_JOINT1_Rot = Quaternion.LookRotation (LEFT_RING_JOINT1_Pos - LEFT_WRIST_Pos);

		boneDatas [(int)Bones.LEFT_RING_JOINT1].SetPosition (LEFT_RING_JOINT1_Pos);
		boneDatas [(int)Bones.LEFT_RING_JOINT1].SetRotation (LEFT_RING_JOINT1_Rot);
		boneDatas [(int)Bones.LEFT_RING_JOINT1].SetValidPose (handTrackingData.left.IsValidPose);

		// Left ring joint2 - LEFT_RING_JOINT2
		Vector3 LEFT_RING_JOINT2_Pos = WaveVR_Utils.GetPosition (handTrackingData.leftFinger.ring.joint2);
		Quaternion LEFT_RING_JOINT2_Rot = Quaternion.LookRotation (LEFT_RING_JOINT2_Pos - LEFT_RING_JOINT1_Pos);

		boneDatas [(int)Bones.LEFT_RING_JOINT2].SetPosition (LEFT_RING_JOINT2_Pos);
		boneDatas [(int)Bones.LEFT_RING_JOINT2].SetRotation (LEFT_RING_JOINT2_Rot);
		boneDatas [(int)Bones.LEFT_RING_JOINT2].SetValidPose (handTrackingData.left.IsValidPose);

		// Left ring joint3 - LEFT_RING_JOINT3
		Vector3 LEFT_RING_JOINT3_Pos = WaveVR_Utils.GetPosition (handTrackingData.leftFinger.ring.joint3);
		Quaternion LEFT_RING_JOINT3_Rot = Quaternion.LookRotation (LEFT_RING_JOINT3_Pos - LEFT_RING_JOINT2_Pos);

		boneDatas [(int)Bones.LEFT_RING_JOINT3].SetPosition (LEFT_RING_JOINT3_Pos);
		boneDatas [(int)Bones.LEFT_RING_JOINT3].SetRotation (LEFT_RING_JOINT3_Rot);
		boneDatas [(int)Bones.LEFT_RING_JOINT3].SetValidPose (handTrackingData.left.IsValidPose);

		// Left ring tip - LEFT_RING_TIP
		Vector3 LEFT_RING_TIP_Pos = WaveVR_Utils.GetPosition (handTrackingData.leftFinger.ring.tip);
		Quaternion LEFT_RING_TIP_Rot = Quaternion.LookRotation (LEFT_RING_TIP_Pos - LEFT_RING_JOINT3_Pos);

		boneDatas [(int)Bones.LEFT_RING_TIP].SetPosition (LEFT_RING_TIP_Pos);
		boneDatas [(int)Bones.LEFT_RING_TIP].SetRotation (LEFT_RING_TIP_Rot);
		boneDatas [(int)Bones.LEFT_RING_TIP].SetValidPose (handTrackingData.left.IsValidPose);

		// Left pinky joint1 - LEFT_PINKY_JOINT1
		Vector3 LEFT_PINKY_JOINT1_Pos = WaveVR_Utils.GetPosition (handTrackingData.leftFinger.pinky.joint1);
		Quaternion LEFT_PINKY_JOINT1_Rot = Quaternion.LookRotation (LEFT_PINKY_JOINT1_Pos - LEFT_WRIST_Pos);

		boneDatas [(int)Bones.LEFT_PINKY_JOINT1].SetPosition (LEFT_PINKY_JOINT1_Pos);
		boneDatas [(int)Bones.LEFT_PINKY_JOINT1].SetRotation (LEFT_PINKY_JOINT1_Rot);
		boneDatas [(int)Bones.LEFT_PINKY_JOINT1].SetValidPose (handTrackingData.left.IsValidPose);

		// Left pinky joint2 - LEFT_PINKY_JOINT2
		Vector3 LEFT_PINKY_JOINT2_Pos = WaveVR_Utils.GetPosition (handTrackingData.leftFinger.pinky.joint2);
		Quaternion LEFT_PINKY_JOINT2_Rot = Quaternion.LookRotation (LEFT_PINKY_JOINT2_Pos - LEFT_PINKY_JOINT1_Pos);

		boneDatas [(int)Bones.LEFT_PINKY_JOINT2].SetPosition (LEFT_PINKY_JOINT2_Pos);
		boneDatas [(int)Bones.LEFT_PINKY_JOINT2].SetRotation (LEFT_PINKY_JOINT2_Rot);
		boneDatas [(int)Bones.LEFT_PINKY_JOINT2].SetValidPose (handTrackingData.left.IsValidPose);

		// Left pinky joint3 - LEFT_PINKY_JOINT3
		Vector3 LEFT_PINKY_JOINT3_Pos = WaveVR_Utils.GetPosition (handTrackingData.leftFinger.pinky.joint3);
		Quaternion LEFT_PINKY_JOINT3_Rot = Quaternion.LookRotation (LEFT_PINKY_JOINT3_Pos - LEFT_PINKY_JOINT2_Pos);

		boneDatas [(int)Bones.LEFT_PINKY_JOINT3].SetPosition (LEFT_PINKY_JOINT3_Pos);
		boneDatas [(int)Bones.LEFT_PINKY_JOINT3].SetRotation (LEFT_PINKY_JOINT3_Rot);
		boneDatas [(int)Bones.LEFT_PINKY_JOINT3].SetValidPose (handTrackingData.left.IsValidPose);

		// Left pinky tip - LEFT_PINKY_TIP
		Vector3 LEFT_PINKY_TIP_Pos = WaveVR_Utils.GetPosition (handTrackingData.leftFinger.pinky.tip);
		Quaternion LEFT_PINKY_TIP_Rot = Quaternion.LookRotation (LEFT_PINKY_TIP_Pos - LEFT_PINKY_JOINT3_Pos);

		boneDatas [(int)Bones.LEFT_PINKY_TIP].SetPosition (LEFT_PINKY_TIP_Pos);
		boneDatas [(int)Bones.LEFT_PINKY_TIP].SetRotation (LEFT_PINKY_TIP_Rot);
		boneDatas [(int)Bones.LEFT_PINKY_TIP].SetValidPose (handTrackingData.left.IsValidPose);
	}

	private WaveVR_Utils.RigidTransform rtWristRight = WaveVR_Utils.RigidTransform.identity;
	private void UpdateRightHandTrackingData()
	{
		// Right wrist - RIGHT_WRIST
		rtWristRight.update(handTrackingData.right.PoseMatrix);
		Vector3 RIGHT_WRIST_Pos = rtWristRight.pos;
		Quaternion RIGHT_WRIST_Rot = rtWristRight.rot;

		boneDatas [(int)Bones.RIGHT_WRIST].SetPosition (RIGHT_WRIST_Pos);
		boneDatas [(int)Bones.RIGHT_WRIST].SetRotation (RIGHT_WRIST_Rot);
		boneDatas [(int)Bones.RIGHT_WRIST].SetValidPose (handTrackingData.right.IsValidPose);

		// Right thumb joint1 - RIGHT_THUMB_JOINT1
		Vector3 RIGHT_THUMB_JOINT1_Pos = WaveVR_Utils.GetPosition (handTrackingData.rightFinger.thumb.joint1);
		Quaternion RIGHT_THUMB_JOINT1_Rot = Quaternion.LookRotation (RIGHT_THUMB_JOINT1_Pos - RIGHT_WRIST_Pos);

		boneDatas [(int)Bones.RIGHT_THUMB_JOINT1].SetPosition (RIGHT_THUMB_JOINT1_Pos);
		boneDatas [(int)Bones.RIGHT_THUMB_JOINT1].SetRotation (RIGHT_THUMB_JOINT1_Rot);
		boneDatas [(int)Bones.RIGHT_THUMB_JOINT1].SetValidPose (handTrackingData.right.IsValidPose);

		// Right thumb joint2 - RIGHT_THUMB_JOINT2
		Vector3 RIGHT_THUMB_JOINT2_Pos = WaveVR_Utils.GetPosition (handTrackingData.rightFinger.thumb.joint2);
		Quaternion RIGHT_THUMB_JOINT2_Rot = Quaternion.LookRotation (RIGHT_THUMB_JOINT2_Pos - RIGHT_THUMB_JOINT1_Pos);

		boneDatas [(int)Bones.RIGHT_THUMB_JOINT2].SetPosition (RIGHT_THUMB_JOINT2_Pos);
		boneDatas [(int)Bones.RIGHT_THUMB_JOINT2].SetRotation (RIGHT_THUMB_JOINT2_Rot);
		boneDatas [(int)Bones.RIGHT_THUMB_JOINT2].SetValidPose (handTrackingData.right.IsValidPose);

		// Right thumb joint3 - RIGHT_THUMB_JOINT3
		Vector3 RIGHT_THUMB_JOINT3_Pos = WaveVR_Utils.GetPosition (handTrackingData.rightFinger.thumb.joint3);
		Quaternion RIGHT_THUMB_JOINT3_Rot = Quaternion.LookRotation (RIGHT_THUMB_JOINT3_Pos - RIGHT_THUMB_JOINT2_Pos);

		boneDatas [(int)Bones.RIGHT_THUMB_JOINT3].SetPosition (RIGHT_THUMB_JOINT3_Pos);
		boneDatas [(int)Bones.RIGHT_THUMB_JOINT3].SetRotation (RIGHT_THUMB_JOINT3_Rot);
		boneDatas [(int)Bones.RIGHT_THUMB_JOINT3].SetValidPose (handTrackingData.right.IsValidPose);

		// Right thumb tip - RIGHT_THUMB_TIP
		Vector3 RIGHT_THUMB_TIP_Pos = WaveVR_Utils.GetPosition (handTrackingData.rightFinger.thumb.tip);
		Quaternion RIGHT_THUMB_TIP_Rot = Quaternion.LookRotation (RIGHT_THUMB_TIP_Pos - RIGHT_THUMB_JOINT3_Pos);

		boneDatas [(int)Bones.RIGHT_THUMB_TIP].SetPosition (RIGHT_THUMB_TIP_Pos);
		boneDatas [(int)Bones.RIGHT_THUMB_TIP].SetRotation (RIGHT_THUMB_TIP_Rot);
		boneDatas [(int)Bones.RIGHT_THUMB_TIP].SetValidPose (handTrackingData.right.IsValidPose);

		// Right index joint1 - RIGHT_INDEX_JOINT1
		Vector3 RIGHT_INDEX_JOINT1_Pos = WaveVR_Utils.GetPosition (handTrackingData.rightFinger.index.joint1);
		Quaternion RIGHT_INDEX_JOINT1_Rot = Quaternion.LookRotation (RIGHT_INDEX_JOINT1_Pos - RIGHT_WRIST_Pos);

		boneDatas [(int)Bones.RIGHT_INDEX_JOINT1].SetPosition (RIGHT_INDEX_JOINT1_Pos);
		boneDatas [(int)Bones.RIGHT_INDEX_JOINT1].SetRotation (RIGHT_INDEX_JOINT1_Rot);
		boneDatas [(int)Bones.RIGHT_INDEX_JOINT1].SetValidPose (handTrackingData.right.IsValidPose);

		// Right index joint2 - RIGHT_INDEX_JOINT2
		Vector3 RIGHT_INDEX_JOINT2_Pos = WaveVR_Utils.GetPosition (handTrackingData.rightFinger.index.joint2);
		Quaternion RIGHT_INDEX_JOINT2_Rot = Quaternion.LookRotation (RIGHT_INDEX_JOINT2_Pos - RIGHT_INDEX_JOINT1_Pos);

		boneDatas [(int)Bones.RIGHT_INDEX_JOINT2].SetPosition (RIGHT_INDEX_JOINT2_Pos);
		boneDatas [(int)Bones.RIGHT_INDEX_JOINT2].SetRotation (RIGHT_INDEX_JOINT2_Rot);
		boneDatas [(int)Bones.RIGHT_INDEX_JOINT2].SetValidPose (handTrackingData.right.IsValidPose);

		// Right index joint3 - RIGHT_INDEX_JOINT3
		Vector3 RIGHT_INDEX_JOINT3_Pos = WaveVR_Utils.GetPosition (handTrackingData.rightFinger.index.joint3);
		Quaternion RIGHT_INDEX_JOINT3_Rot = Quaternion.LookRotation (RIGHT_INDEX_JOINT3_Pos, RIGHT_INDEX_JOINT2_Pos);

		boneDatas [(int)Bones.RIGHT_INDEX_JOINT3].SetPosition (RIGHT_INDEX_JOINT3_Pos);
		boneDatas [(int)Bones.RIGHT_INDEX_JOINT3].SetRotation (RIGHT_INDEX_JOINT3_Rot);
		boneDatas [(int)Bones.RIGHT_INDEX_JOINT3].SetValidPose (handTrackingData.right.IsValidPose);

		// Right index tip - RIGHT_INDEX_TIP
		Vector3 RIGHT_INDEX_TIP_Pos = WaveVR_Utils.GetPosition (handTrackingData.rightFinger.index.tip);
		Quaternion RIGHT_INDEX_TIP_Rot = Quaternion.LookRotation (RIGHT_INDEX_TIP_Pos - RIGHT_INDEX_JOINT3_Pos);

		boneDatas [(int)Bones.RIGHT_INDEX_TIP].SetPosition (RIGHT_INDEX_TIP_Pos);
		boneDatas [(int)Bones.RIGHT_INDEX_TIP].SetRotation (RIGHT_INDEX_TIP_Rot);
		boneDatas [(int)Bones.RIGHT_INDEX_TIP].SetValidPose (handTrackingData.right.IsValidPose);

		// Right middle joint1 - RIGHT_MIDDLE_JOINT1
		Vector3 RIGHT_MIDDLE_JOINT1_Pos = WaveVR_Utils.GetPosition (handTrackingData.rightFinger.middle.joint1);
		Quaternion RIGHT_MIDDLE_JOINT1_Rot = Quaternion.LookRotation (RIGHT_MIDDLE_JOINT1_Pos - RIGHT_WRIST_Pos);

		boneDatas [(int)Bones.RIGHT_MIDDLE_JOINT1].SetPosition (RIGHT_MIDDLE_JOINT1_Pos);
		boneDatas [(int)Bones.RIGHT_MIDDLE_JOINT1].SetRotation (RIGHT_MIDDLE_JOINT1_Rot);
		boneDatas [(int)Bones.RIGHT_MIDDLE_JOINT1].SetValidPose (handTrackingData.right.IsValidPose);

		// Right middle joint2 - RIGHT_MIDDLE_JOINT2
		Vector3 RIGHT_MIDDLE_JOINT2_Pos = WaveVR_Utils.GetPosition (handTrackingData.rightFinger.middle.joint2);
		Quaternion RIGHT_MIDDLE_JOINT2_Rot = Quaternion.LookRotation (RIGHT_MIDDLE_JOINT2_Pos - RIGHT_MIDDLE_JOINT1_Pos);

		boneDatas [(int)Bones.RIGHT_MIDDLE_JOINT2].SetPosition (RIGHT_MIDDLE_JOINT2_Pos);
		boneDatas [(int)Bones.RIGHT_MIDDLE_JOINT2].SetRotation (RIGHT_MIDDLE_JOINT2_Rot);
		boneDatas [(int)Bones.RIGHT_MIDDLE_JOINT2].SetValidPose (handTrackingData.right.IsValidPose);

		// Right middle joint3 - RIGHT_MIDDLE_JOINT3
		Vector3 RIGHT_MIDDLE_JOINT3_Pos = WaveVR_Utils.GetPosition (handTrackingData.rightFinger.middle.joint3);
		Quaternion RIGHT_MIDDLE_JOINT3_Rot = Quaternion.LookRotation (RIGHT_MIDDLE_JOINT3_Pos - RIGHT_MIDDLE_JOINT2_Pos);

		boneDatas [(int)Bones.RIGHT_MIDDLE_JOINT3].SetPosition (RIGHT_MIDDLE_JOINT3_Pos);
		boneDatas [(int)Bones.RIGHT_MIDDLE_JOINT3].SetRotation (RIGHT_MIDDLE_JOINT3_Rot);
		boneDatas [(int)Bones.RIGHT_MIDDLE_JOINT3].SetValidPose (handTrackingData.right.IsValidPose);

		// Right middle tip - RIGHT_MIDDLE_TIP
		Vector3 RIGHT_MIDDLE_TIP_Pos = WaveVR_Utils.GetPosition (handTrackingData.rightFinger.middle.tip);
		Quaternion RIGHT_MIDDLE_TIP_Rot = Quaternion.LookRotation (RIGHT_MIDDLE_TIP_Pos - RIGHT_MIDDLE_JOINT3_Pos);

		boneDatas [(int)Bones.RIGHT_MIDDLE_TIP].SetPosition (RIGHT_MIDDLE_TIP_Pos);
		boneDatas [(int)Bones.RIGHT_MIDDLE_TIP].SetRotation (RIGHT_MIDDLE_TIP_Rot);
		boneDatas [(int)Bones.RIGHT_MIDDLE_TIP].SetValidPose (handTrackingData.right.IsValidPose);

		// Right ring joint1 - RIGHT_RING_JOINT1
		Vector3 RIGHT_RING_JOINT1_Pos = WaveVR_Utils.GetPosition (handTrackingData.rightFinger.ring.joint1);
		Quaternion RIGHT_RING_JOINT1_Rot = Quaternion.LookRotation (RIGHT_RING_JOINT1_Pos - RIGHT_WRIST_Pos);

		boneDatas [(int)Bones.RIGHT_RING_JOINT1].SetPosition (RIGHT_RING_JOINT1_Pos);
		boneDatas [(int)Bones.RIGHT_RING_JOINT1].SetRotation (RIGHT_RING_JOINT1_Rot);
		boneDatas [(int)Bones.RIGHT_RING_JOINT1].SetValidPose (handTrackingData.right.IsValidPose);

		// Right ring joint2 - RIGHT_RING_JOINT2
		Vector3 RIGHT_RING_JOINT2_Pos = WaveVR_Utils.GetPosition (handTrackingData.rightFinger.ring.joint2);
		Quaternion RIGHT_RING_JOINT2_Rot = Quaternion.LookRotation (RIGHT_RING_JOINT2_Pos - RIGHT_RING_JOINT1_Pos);

		boneDatas [(int)Bones.RIGHT_RING_JOINT2].SetPosition (RIGHT_RING_JOINT2_Pos);
		boneDatas [(int)Bones.RIGHT_RING_JOINT2].SetRotation (RIGHT_RING_JOINT2_Rot);
		boneDatas [(int)Bones.RIGHT_RING_JOINT2].SetValidPose (handTrackingData.right.IsValidPose);

		// Right ring joint3 - RIGHT_RING_JOINT3
		Vector3 RIGHT_RING_JOINT3_Pos = WaveVR_Utils.GetPosition (handTrackingData.rightFinger.ring.joint3);
		Quaternion RIGHT_RING_JOINT3_Rot = Quaternion.LookRotation (RIGHT_RING_JOINT3_Pos - RIGHT_RING_JOINT2_Pos);

		boneDatas [(int)Bones.RIGHT_RING_JOINT3].SetPosition (RIGHT_RING_JOINT3_Pos);
		boneDatas [(int)Bones.RIGHT_RING_JOINT3].SetRotation (RIGHT_RING_JOINT3_Rot);
		boneDatas [(int)Bones.RIGHT_RING_JOINT3].SetValidPose (handTrackingData.right.IsValidPose);

		// Right ring tip - RIGHT_RING_TIP
		Vector3 RIGHT_RING_TIP_Pos = WaveVR_Utils.GetPosition (handTrackingData.rightFinger.ring.tip);
		Quaternion RIGHT_RING_TIP_Rot = Quaternion.LookRotation (RIGHT_RING_TIP_Pos - RIGHT_RING_JOINT3_Pos);

		boneDatas [(int)Bones.RIGHT_RING_TIP].SetPosition (RIGHT_RING_TIP_Pos);
		boneDatas [(int)Bones.RIGHT_RING_TIP].SetRotation (RIGHT_RING_TIP_Rot);
		boneDatas [(int)Bones.RIGHT_RING_TIP].SetValidPose (handTrackingData.right.IsValidPose);

		// Right pinky joint1 - RIGHT_PINKY_JOINT1
		Vector3 RIGHT_PINKY_JOINT1_Pos = WaveVR_Utils.GetPosition (handTrackingData.rightFinger.pinky.joint1);
		Quaternion RIGHT_PINKY_JOINT1_Rot = Quaternion.LookRotation (RIGHT_PINKY_JOINT1_Pos - RIGHT_WRIST_Pos);

		boneDatas [(int)Bones.RIGHT_PINKY_JOINT1].SetPosition (RIGHT_PINKY_JOINT1_Pos);
		boneDatas [(int)Bones.RIGHT_PINKY_JOINT1].SetRotation (RIGHT_PINKY_JOINT1_Rot);
		boneDatas [(int)Bones.RIGHT_PINKY_JOINT1].SetValidPose (handTrackingData.right.IsValidPose);

		// Right pinky joint2 - RIGHT_PINKY_JOINT2
		Vector3 RIGHT_PINKY_JOINT2_Pos = WaveVR_Utils.GetPosition (handTrackingData.rightFinger.pinky.joint2);
		Quaternion RIGHT_PINKY_JOINT2_Rot = Quaternion.LookRotation (RIGHT_PINKY_JOINT2_Pos - RIGHT_PINKY_JOINT1_Pos);

		boneDatas [(int)Bones.RIGHT_PINKY_JOINT2].SetPosition (RIGHT_PINKY_JOINT2_Pos);
		boneDatas [(int)Bones.RIGHT_PINKY_JOINT2].SetRotation (RIGHT_PINKY_JOINT2_Rot);
		boneDatas [(int)Bones.RIGHT_PINKY_JOINT2].SetValidPose (handTrackingData.right.IsValidPose);

		// Right pinky joint3 - RIGHT_PINKY_JOINT3
		Vector3 RIGHT_PINKY_JOINT3_Pos = WaveVR_Utils.GetPosition (handTrackingData.rightFinger.pinky.joint3);
		Quaternion RIGHT_PINKY_JOINT3_Rot = Quaternion.LookRotation (RIGHT_PINKY_JOINT3_Pos - RIGHT_PINKY_JOINT2_Pos);

		boneDatas [(int)Bones.RIGHT_PINKY_JOINT3].SetPosition (RIGHT_PINKY_JOINT3_Pos);
		boneDatas [(int)Bones.RIGHT_PINKY_JOINT3].SetRotation (RIGHT_PINKY_JOINT3_Rot);
		boneDatas [(int)Bones.RIGHT_PINKY_JOINT3].SetValidPose (handTrackingData.right.IsValidPose);

		// Right pinky tip - RIGHT_PINKY_TIP
		Vector3 RIGHT_PINKY_TIP_Pos = WaveVR_Utils.GetPosition (handTrackingData.rightFinger.pinky.tip);
		Quaternion RIGHT_PINKY_TIP_Rot = Quaternion.LookRotation (RIGHT_PINKY_TIP_Pos - RIGHT_PINKY_JOINT3_Pos);

		boneDatas [(int)Bones.RIGHT_PINKY_TIP].SetPosition (RIGHT_PINKY_TIP_Pos);
		boneDatas [(int)Bones.RIGHT_PINKY_TIP].SetRotation (RIGHT_PINKY_TIP_Rot);
		boneDatas [(int)Bones.RIGHT_PINKY_TIP].SetValidPose (handTrackingData.right.IsValidPose);
	}
}
