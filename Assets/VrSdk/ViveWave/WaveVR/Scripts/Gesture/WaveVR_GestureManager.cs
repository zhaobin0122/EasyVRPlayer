using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using wvr;
using WVR_Log;
using System.Threading;
using System;

public class WaveVR_GestureManager : MonoBehaviour {
	private const string LOG_TAG = "WaveVR_GestureManager";
	private void DEBUG(string msg)
	{
		if (Log.EnableDebugLog)
			Log.d (LOG_TAG, msg, true);
	}

	private static WaveVR_GestureManager instance = null;
	public static WaveVR_GestureManager Instance {
		get {
			return instance;
		}
	}


	// ------------------- Pointer Related begins -------------------
	#region Pointer variables.
	public enum EGestureHand
	{
		RIGHT = 0,
		LEFT = 1
	};

	[HideInInspector]
	public static EGestureHand GestureFocusHand = EGestureHand.RIGHT;
	#endregion
	// ------------------- Pointer Related ends -------------------


	public enum EStaticGestures
	{
		FIST = WVR_HandGestureType.WVR_HandGestureType_Fist,
		FIVE = WVR_HandGestureType.WVR_HandGestureType_Five,
		OK = WVR_HandGestureType.WVR_HandGestureType_OK,
		THUMBUP = WVR_HandGestureType.WVR_HandGestureType_ThumbUp,
		INDEXUP = WVR_HandGestureType.WVR_HandGestureType_IndexUp,
		//PINCH = WVR_HandGestureType.WVR_HandGestureType_Pinch
	}

	public bool EnableHandGesture = true;
	private bool preEnableHandGesture = true;
	private WVR_HandGestureData_t handGestureData = new WVR_HandGestureData_t();

	public bool EnableHandTracking = true;
	private bool preEnableHandTracking = true;

	private bool hasHandGesture = false;
	private WVR_HandGestureType prevStaticGestureLeft = WVR_HandGestureType.WVR_HandGestureType_Invalid;
	private WVR_HandGestureType currStaticGestureLeft = WVR_HandGestureType.WVR_HandGestureType_Invalid;
	private WVR_HandGestureType prevStaticGestureRight = WVR_HandGestureType.WVR_HandGestureType_Invalid;
	private WVR_HandGestureType currStaticGestureRight = WVR_HandGestureType.WVR_HandGestureType_Invalid;

	#region MonoBehaviour Overrides
	void Awake()
	{
		if (instance == null)
			instance = this;
	}

	void Start () {
		preEnableHandGesture = this.EnableHandGesture;
		if (this.EnableHandGesture)
		{
			DEBUG ("Start() Start hand gesture.");
			StartHandGesture ();
		}

		preEnableHandTracking = this.EnableHandTracking;
		if (this.EnableHandTracking)
		{
			DEBUG ("Start() Start hand tracking.");
			StartHandTracking ();
		}
	}

	void Update () {
		if (preEnableHandGesture != this.EnableHandGesture)
		{
			preEnableHandGesture = this.EnableHandGesture;
			if (this.EnableHandGesture)
			{
				DEBUG ("Update() Start hand gesture.");
				StartHandGesture ();
			}
			if (!this.EnableHandGesture)
			{
				DEBUG ("Update() Stop hand gesture.");
				StopHandGesture ();
			}
		}

		if (preEnableHandTracking != this.EnableHandTracking)
		{
			preEnableHandTracking = this.EnableHandTracking;
			if (this.EnableHandTracking)
			{
				DEBUG ("Update() Start hand tracking.");
				StartHandTracking ();
			}
			if (!this.EnableHandTracking)
			{
				DEBUG ("Update() Stop hand tracking.");
				StopHandTracking ();
			}
		}

		// Sending an event when gesture changes.
		hasHandGesture = GetHandGestureData (ref handGestureData);
		if (hasHandGesture)
		{
			UpdateLeftHandGestureData (handGestureData);
			UpdateRightHandGestureData (handGestureData);
		}
	}

	void OnEnable()
	{
		WaveVR_Utils.Event.Listen (WaveVR_Utils.Event.ALL_VREVENT, OnEvent);
	}

	void OnDisable()
	{
		WaveVR_Utils.Event.Remove (WaveVR_Utils.Event.ALL_VREVENT, OnEvent);
	}
	#endregion

	private void UpdateLeftHandGestureData(WVR_HandGestureData_t data)
	{
		prevStaticGestureLeft = currStaticGestureLeft;
		currStaticGestureLeft = data.left;

		if (currStaticGestureLeft != prevStaticGestureLeft)
		{
			DEBUG ("UpdateLeftHandGestureData() Receives " + currStaticGestureLeft);
			WaveVR_Utils.Event.Send (WaveVR_Utils.Event.HAND_STATIC_GESTURE_LEFT, currStaticGestureLeft);
		}
	}

	public WVR_HandGestureType GetCurrentLeftHandStaticGesture()
	{
		return currStaticGestureLeft;
	}

	private void UpdateRightHandGestureData(WVR_HandGestureData_t data)
	{
		prevStaticGestureRight = currStaticGestureRight;
		currStaticGestureRight = data.right;

		if (currStaticGestureRight != prevStaticGestureRight)
		{
			DEBUG ("UpdateLeftHandGestureData() Receives " + currStaticGestureRight);
			WaveVR_Utils.Event.Send (WaveVR_Utils.Event.HAND_STATIC_GESTURE_RIGHT, currStaticGestureRight);
		}
	}

	public WVR_HandGestureType GetCurrentRightHandStaticGesture()
	{
		return currStaticGestureRight;
	}

	void OnEvent(params object[] args)
	{
		WVR_Event_t event_t = (WVR_Event_t)args [0];
		switch (event_t.common.type)
		{
		case WVR_EventType.WVR_EventType_HandGesture_Abnormal:
			DEBUG ("OnEvent() WVR_EventType_HandGesture_Abnormal, restart the hand gesture component.");
			RestartHandGesture ();
			break;
		case WVR_EventType.WVR_EventType_HandTracking_Abnormal:
			DEBUG ("OnEvent() WVR_EventType_HandTracking_Abnormal, restart the hand tracking component.");
			RestartHandTracking ();
			break;
		default:
			break;
		}
	}

	#region Hand Gesture Lifecycle
	private WaveVR_Utils.HandGestureStatus handGestureStatus = WaveVR_Utils.HandGestureStatus.NOT_START;
	private static ReaderWriterLockSlim handGestureStatusRWLock = new ReaderWriterLockSlim ();
	private void SetHandGestureStatus(WaveVR_Utils.HandGestureStatus status)
	{
		try {
			handGestureStatusRWLock.TryEnterWriteLock(2000);
			handGestureStatus = status;
		} catch (Exception e) {
			Log.e (LOG_TAG, "SetHandGestureStatus() " + e.Message, true);
			throw;
		} finally {
			handGestureStatusRWLock.ExitWriteLock ();
		}
	}

	public WaveVR_Utils.HandGestureStatus GetHandGestureStatus()
	{
		ulong feature = WaveVR.Instance.GetSupportedFeatures ();
		if ((feature & (ulong)WVR_SupportedFeature.WVR_SupportedFeature_HandGesture) == 0)
			return WaveVR_Utils.HandGestureStatus.UNSUPPORT;

		try {
			handGestureStatusRWLock.TryEnterReadLock(2000);
			return handGestureStatus;
		} catch (Exception e) {
			Log.e (LOG_TAG, "GetHandGestureStatus() " + e.Message, true);
			throw;
		} finally {
			handGestureStatusRWLock.ExitReadLock ();
		}
	}

	private System.Object handGestureThreadLock = new System.Object ();
	private event WaveVR_Utils.HandGestureResultDelegate handGestureResultCB = null;
	private void StartHandGestureLock()
	{
		bool result = false;

		if (WaveVR.Instance.IsHandGestureEnabled ())
		{
			result = true;
			SetHandGestureStatus (WaveVR_Utils.HandGestureStatus.AVAILABLE);
		}

		WaveVR_Utils.HandGestureStatus status = GetHandGestureStatus ();
		if (this.EnableHandGesture &&
			(
				status == WaveVR_Utils.HandGestureStatus.NOT_START ||
				status == WaveVR_Utils.HandGestureStatus.START_FAILURE
			)
		)
		{
			SetHandGestureStatus (WaveVR_Utils.HandGestureStatus.STARTING);
			result = WaveVR.Instance.StartHandGesture ();
			SetHandGestureStatus (result ? WaveVR_Utils.HandGestureStatus.AVAILABLE : WaveVR_Utils.HandGestureStatus.START_FAILURE);
		}

		status = GetHandGestureStatus ();
		DEBUG ("StartHandGestureLock() " + result + ", status: " + status);
		WaveVR_Utils.Event.Send (WaveVR_Utils.Event.HAND_GESTURE_STATUS, status);

		if (handGestureResultCB != null)
		{
			handGestureResultCB (this, result);
			handGestureResultCB = null;
		}
	}

	private void StartHandGestureThread()
	{
		lock(handGestureThreadLock)
		{
			DEBUG ("StartHandGestureThread()");
			StartHandGestureLock ();
		}
	}

	private void StartHandGesture()
	{
		Thread hand_gesture_t = new Thread (StartHandGestureThread);
		hand_gesture_t.Start ();
	}

	private void StopHandGestureLock()
	{
		if (!WaveVR.Instance.IsHandGestureEnabled ())
			SetHandGestureStatus (WaveVR_Utils.HandGestureStatus.NOT_START);

		WaveVR_Utils.HandGestureStatus status = GetHandGestureStatus ();
		if (status == WaveVR_Utils.HandGestureStatus.AVAILABLE)
		{
			DEBUG ("StopHandGestureLock()");
			SetHandGestureStatus (WaveVR_Utils.HandGestureStatus.STOPING);
			WaveVR.Instance.StopHandGesture ();
			SetHandGestureStatus (WaveVR_Utils.HandGestureStatus.NOT_START);
		}

		status = GetHandGestureStatus ();
		WaveVR_Utils.Event.Send (WaveVR_Utils.Event.HAND_GESTURE_STATUS, status);
	}

	private void StopHandGestureThread()
	{
		lock(handGestureThreadLock)
		{
			DEBUG ("StopHandGestureThread()");
			StopHandGestureLock ();
		}
	}

	private void StopHandGesture()
	{
		Thread hand_gesture_t = new Thread (StopHandGestureThread);
		hand_gesture_t.Start ();
	}

	private void RestartHandGestureThread()
	{
		lock (handGestureThreadLock)
		{
			DEBUG ("RestartHandGestureThread()");
			StopHandGestureLock ();
			StartHandGestureLock ();
		}
	}

	public void RestartHandGesture()
	{
		Thread hand_gesture_t = new Thread (RestartHandGestureThread);
		hand_gesture_t.Start ();
	}

	public void RestartHandGesture(WaveVR_Utils.HandGestureResultDelegate callback)
	{
		if (handGestureResultCB == null)
			handGestureResultCB = callback;
		else
			handGestureResultCB += callback;

		RestartHandGesture ();
	}

	private bool GetHandGestureData(ref WVR_HandGestureData_t data)
	{
		bool hasHandGestureData = false;

		WaveVR_Utils.HandGestureStatus status = GetHandGestureStatus ();
		if (status == WaveVR_Utils.HandGestureStatus.AVAILABLE)
			hasHandGestureData = WaveVR.Instance.GetHandGestureData (ref data);

		return hasHandGestureData;
	}
	#endregion

	#region Hand Tracking Lifecycle
	private WaveVR_Utils.HandTrackingStatus handTrackingStatus = WaveVR_Utils.HandTrackingStatus.NOT_START;
	private static ReaderWriterLockSlim handTrackingStatusRWLock = new ReaderWriterLockSlim ();
	private void SetHandTrackingStatus(WaveVR_Utils.HandTrackingStatus status)
	{
		try {
			handTrackingStatusRWLock.TryEnterWriteLock(2000);
			handTrackingStatus = status;
		} catch (Exception e) {
			Log.e (LOG_TAG, "SetHandTrackingStatus() " + e.Message, true);
			throw;
		} finally {
			handTrackingStatusRWLock.ExitWriteLock ();
		}
	}

	public WaveVR_Utils.HandTrackingStatus GetHandTrackingStatus()
	{
		ulong feature = WaveVR.Instance.GetSupportedFeatures ();
		if ((feature & (ulong)WVR_SupportedFeature.WVR_SupportedFeature_HandTracking) == 0)
			return WaveVR_Utils.HandTrackingStatus.UNSUPPORT;

		try {
			handTrackingStatusRWLock.TryEnterReadLock(2000);
			return handTrackingStatus;
		} catch (Exception e) {
			Log.e (LOG_TAG, "GetHandTrackingStatus() " + e.Message, true);
			throw;
		} finally {
			handTrackingStatusRWLock.ExitReadLock ();
		}
	}

	private System.Object handTrackingThreadLocker = new System.Object ();
	private event WaveVR_Utils.HandTrackingResultDelegate handTrackingResultCB = null;
	private void StartHandTrackingLock()
	{
		bool result = false;

		if (WaveVR.Instance.IsHandTrackingEnabled ())
		{
			result = true;
			SetHandTrackingStatus (WaveVR_Utils.HandTrackingStatus.AVAILABLE);
		}

		WaveVR_Utils.HandTrackingStatus status = GetHandTrackingStatus ();
		if (this.EnableHandTracking &&
			(
				status == WaveVR_Utils.HandTrackingStatus.NOT_START ||
				status == WaveVR_Utils.HandTrackingStatus.START_FAILURE
			)
		)
		{
			SetHandTrackingStatus (WaveVR_Utils.HandTrackingStatus.STARTING);
			result = WaveVR.Instance.StartHandTracking ();
			SetHandTrackingStatus (result ? WaveVR_Utils.HandTrackingStatus.AVAILABLE : WaveVR_Utils.HandTrackingStatus.START_FAILURE);
		}

		status = GetHandTrackingStatus ();
		DEBUG ("StartHandTrackingLock() " + result + ", status: " + status);
		WaveVR_Utils.Event.Send (WaveVR_Utils.Event.HAND_TRACKING_STATUS, status);

		if (handTrackingResultCB != null)
		{
			handTrackingResultCB (this, result);
			handTrackingResultCB = null;
		}
	}

	private void StartHandTrackingThread()
	{
		lock(handTrackingThreadLocker)
		{
			DEBUG ("StartHandTrackingThread()");
			StartHandTrackingLock ();
		}
	}

	private void StartHandTracking()
	{
		Thread hand_tracking_t = new Thread (StartHandTrackingThread);
		hand_tracking_t.Start ();
	}

	private void StopHandTrackingLock()
	{
		if (!WaveVR.Instance.IsHandTrackingEnabled ())
			SetHandTrackingStatus (WaveVR_Utils.HandTrackingStatus.NOT_START);

		WaveVR_Utils.HandTrackingStatus status = GetHandTrackingStatus ();
		if (status == WaveVR_Utils.HandTrackingStatus.AVAILABLE)
		{
			DEBUG ("StopHandTrackingLock()");
			SetHandTrackingStatus (WaveVR_Utils.HandTrackingStatus.STOPING);
			WaveVR.Instance.StopHandTracking ();
			SetHandTrackingStatus (WaveVR_Utils.HandTrackingStatus.NOT_START);
		}

		status = GetHandTrackingStatus ();
		WaveVR_Utils.Event.Send (WaveVR_Utils.Event.HAND_TRACKING_STATUS, status);
	}

	private void StopHandTrackingThread()
	{
		lock(handTrackingThreadLocker)
		{
			DEBUG ("StopHandTrackingThread()");
			StopHandTrackingLock ();
		}
	}

	private void StopHandTracking()
	{
		Thread hand_tracking_t = new Thread (StopHandTrackingThread);
		hand_tracking_t.Start ();
	}

	private void RestartHandTrackingThread()
	{
		lock (handTrackingThreadLocker)
		{
			DEBUG ("RestartHandTrackingThread()");
			StopHandTrackingLock ();
			StartHandTrackingLock ();
		}
	}

	public void RestartHandTracking()
	{
		Thread hand_tracking_t = new Thread (RestartHandTrackingThread);
		hand_tracking_t.Start ();
	}

	public void RestartHandTracking(WaveVR_Utils.HandTrackingResultDelegate callback)
	{
		if (handTrackingResultCB == null)
			handTrackingResultCB = callback;
		else
			handTrackingResultCB += callback;

		RestartHandTracking ();
	}

	public bool GetHandTrackingData(ref WVR_HandTrackingData_t data, WVR_PoseOriginModel originModel, uint predictedMilliSec)
	{
		bool hasHandTrackingData = false;

		WaveVR_Utils.HandTrackingStatus status = GetHandTrackingStatus ();
		if (status == WaveVR_Utils.HandTrackingStatus.AVAILABLE)
			hasHandTrackingData = WaveVR.Instance.GetHandTrackingData (ref data, originModel, predictedMilliSec);

		return hasHandTrackingData;
	}
	#endregion
}
