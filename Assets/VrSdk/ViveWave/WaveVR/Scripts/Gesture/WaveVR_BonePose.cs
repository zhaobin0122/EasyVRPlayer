using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WVR_Log;

public class WaveVR_BonePose : IWaveVR_BonePose {
	private const string LOG_TAG = "WaveVR_BonePose";
	private void DEBUG(string msg)
	{
		if (Log.EnableDebugLog)
			Log.d (LOG_TAG, this.BoneType + ", " + msg, true);
	}

	[Tooltip("The bone type.")]
	public WaveVR_BonePoseImpl.Bones BoneType = WaveVR_BonePoseImpl.Bones.ROOT;
	[Tooltip("Hide the bone with invalid poses.")]
	public bool HideInvalidBone = true;

	#region Export APIs
	public bool IsBonePoseValid()
	{
		return IsBonePoseValid (this.BoneType);
	}

	public bool Valid {
		get {
			return IsBonePoseValid ();
		}
	}

	public WaveVR_Utils.RigidTransform GetBoneTransform()
	{
		return GetBoneTransform (this.BoneType);
	}

	public Vector3 Position {
		get {
			return GetBoneTransform ().pos;
		}
	}

	public Quaternion Rotation {
		get {
			return GetBoneTransform ().rot;
		}
	}
	#endregion

	#region MonoBehaviour Overrides
	private bool mEnabled = false;
	private MeshRenderer meshRend = null;
	private List<GameObject> childrenObjects = new List<GameObject> ();
	private List<bool> childrenObjectsState = new List<bool> ();
	private bool objectsShown = true;
	void OnEnable()
	{
		if (!mEnabled)
		{
			meshRend = GetComponent<MeshRenderer> ();

			childrenObjects.Clear ();
			childrenObjectsState.Clear ();
			for (int i = 0; i < transform.childCount; i++)
			{
				childrenObjects.Add (transform.GetChild (i).gameObject);
				childrenObjectsState.Add (transform.GetChild (i).gameObject.activeSelf);
				DEBUG ("OnEnable() " + gameObject.name + " has child: " + childrenObjects [i].name + ", active? " + childrenObjectsState [i]);
			}

			mEnabled = true;
		}
	}

	void OnDisable()
	{
		if (mEnabled)
		{
			DEBUG ("OnDisable() Restore children objects.");
			ForceActivateObjects (true);
			mEnabled = false;
		}
	}

	private WaveVR_Utils.RigidTransform boneTransform = WaveVR_Utils.RigidTransform.identity;
	void Update () {
		if (!WaveVR_GestureManager.Instance.EnableHandTracking)
			return;

		// 1. Get Hand Tracking data first.
		boneTransform = GetBoneTransform (this.BoneType);

		// 2. After getting Hand Tracking data, check whether the Hand Tracking data is valid or not.
		if (this.Valid)
		{
			gameObject.transform.localPosition = boneTransform.pos;
			if (this.BoneType == WaveVR_BonePoseImpl.Bones.LEFT_WRIST ||
			   this.BoneType == WaveVR_BonePoseImpl.Bones.RIGHT_WRIST)
			{
				gameObject.transform.localRotation = boneTransform.rot;
			}

			if (Log.gpl.Print)
				DEBUG ("Position (" + boneTransform.pos.x + ", " + boneTransform.pos.y + ", " + boneTransform.pos.z + ")");
		}

		// 3. Check if hiding the tracked object.
		ActivateObjects ();
	}
	#endregion

	private void ActivateObjects()
	{
		bool active = true;

		if (this.HideInvalidBone)
			active &= this.Valid;

		if (active == objectsShown)
			return;

		DEBUG ("ActivateObjects() valid pose: " + this.Valid);

		ForceActivateObjects (active);
	}

	private void ForceActivateObjects(bool active)
	{
		DEBUG ("ForceActivateObjects() " + active);
		if (meshRend != null)
			meshRend.enabled = active;

		for (int i = 0; i < childrenObjects.Count; i++)
		{
			if (childrenObjects [i] == null)
				continue;

			if (childrenObjectsState [i])
			{
				DEBUG ("ForceActivateTargetObjects() " + (active ? "activate" : "deactivate") + " " + childrenObjects [i].name);
				childrenObjects [i].SetActive (active);
			}
		}

		objectsShown = active;
	}
}
