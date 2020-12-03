using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using wvr;
using WVR_Log;

public abstract class IWaveVR_BonePose : MonoBehaviour {
	private static WaveVR_BonePoseImpl instance = null;
	public static WaveVR_BonePoseImpl Instance {
		get {
			if (instance == null)
				instance = new WaveVR_BonePoseImpl ();
			return instance;
		}
	}

	//private static WVR_HandTrackingData_t handTrackingData = new WVR_HandTrackingData_t();
	void OnEnable()
	{
		if (instance == null)
			instance = new WaveVR_BonePoseImpl ();
	}

	public WaveVR_Utils.RigidTransform GetBoneTransform(WaveVR_BonePoseImpl.Bones bone_type)
	{
		return Instance.GetBoneTransform (bone_type);
	}

	public bool IsBonePoseValid(WaveVR_BonePoseImpl.Bones bone_type)
	{
		return Instance.IsBonePoseValid (bone_type);
	}

	public bool IsHandPoseValid(WaveVR_GestureManager.EGestureHand hand)
	{
		return Instance.IsHandPoseValid (hand);
	}
}
