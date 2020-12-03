// "WaveVR SDK
// © 2017 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the WaveVR SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

#pragma warning disable 0162

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WVR_Log;
using System;
using wvr;
using System.Runtime.InteropServices;

public class WaveVR_CameraTexture
{
	private static string LOG_TAG = "WVR_CameraTexture";

	private WVR_CameraInfo_t camerainfo;
	private bool mStarted = false;
	private IntPtr nativeTextureId = IntPtr.Zero;
	private IntPtr mframeBuffer = IntPtr.Zero;
	private bool syncPose = false;
	private WVR_PoseState_t mPoseState;
	private WVR_PoseOriginModel origin = WVR_PoseOriginModel.WVR_PoseOriginModel_OriginOnGround;

	public bool isStarted
	{
		get
		{
			return mStarted;
		}
	}

	public delegate void UpdateCameraCompleted(System.IntPtr nativeTextureId);
	public static event UpdateCameraCompleted UpdateCameraCompletedDelegate = null;

	[Obsolete("StartCameraCompleted delegate is not used in the future")]
	public delegate void StartCameraCompleted(bool result);
	[Obsolete("StartCameraCompletedDelegate is not used in the future")]
	public static event StartCameraCompleted StartCameraCompletedDelegate = null;

	private static WaveVR_CameraTexture mInstance = null;
	private const bool DEBUG = false;

	private void PrintDebugLog(string msg)
	{
		if (DEBUG)
		{
			Log.d(LOG_TAG, msg);
		}
	}

	public static WaveVR_CameraTexture instance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = new WaveVR_CameraTexture();
			}

			return mInstance;
		}
	}
	/*
	private void OnStartCameraCompleted(params object[] args)
	{
		mStarted = (bool)args[0];
		if (StartCameraCompletedDelegate != null) StartCameraCompletedDelegate(mStarted);
		if (!mStarted) return ;
		camerainfo = (WVR_CameraInfo_t)args[1];

		Log.d(LOG_TAG, "OnStartCameraCompleted, result = " + mStarted + " type = " + camerainfo.imgType + " width = " + camerainfo.width + " height = " + camerainfo.height);
	}
	*/

	private void OnUpdateCameraCompleted(params object[] args)
	{
		//bool texUpdated = (bool)args[0];

		if (UpdateCameraCompletedDelegate != null)  UpdateCameraCompletedDelegate(nativeTextureId);
	}

	public IntPtr getNativeTextureId()
	{
		if (!mStarted) return IntPtr.Zero;
		return nativeTextureId;
	}

	public bool startCamera()
	{
		if (mStarted) return false;
		WaveVR_Utils.Event.Listen("DrawCameraCompleted", OnUpdateCameraCompleted);

		mStarted = Interop.WVR_StartCamera(ref camerainfo);

		Log.i(LOG_TAG, "startCamera, result = " + mStarted + " format: " + camerainfo.imgFormat + " size: " + camerainfo.size
			+ " width: " + camerainfo.width + " height: " + camerainfo.height);

		if (mStarted)
		{
			PrintDebugLog("allocate frame buffer");
			mframeBuffer = Marshal.AllocHGlobal((int)camerainfo.size);

			//zero out buffer
			for (int i = 0; i < camerainfo.size; i++)
			{
				Marshal.WriteByte(mframeBuffer, i, 0);
			}

			if (syncPose)
			{
				mPoseState = new WVR_PoseState_t();
			}
		}
		if (StartCameraCompletedDelegate != null) StartCameraCompletedDelegate(mStarted);

		return mStarted;
	}

	public void enableSyncPose(bool enable)
	{
		Log.i(LOG_TAG, "enableSyncPose: " + enable);

		syncPose = enable;
	}

	[Obsolete("Please use getImageType instead")]
	public WVR_CameraImageType GetCameraImageType()
	{
		return camerainfo.imgType;
	}

	public WVR_CameraImageType getImageType()
	{
		if (!mStarted) return WVR_CameraImageType.WVR_CameraImageType_Invalid;
		return camerainfo.imgType;
	}

	[Obsolete("Please use getImageFormat instead")]
	public WVR_CameraImageFormat GetCameraImageFormat()
	{
		if (!mStarted) return 0;
		return camerainfo.imgFormat;
	}

	public WVR_CameraImageFormat getImageFormat()
	{
		if (!mStarted) return WVR_CameraImageFormat.WVR_CameraImageFormat_Invalid;
		return camerainfo.imgFormat;
	}

	[Obsolete("Please use getImageWidth instead")]
	public uint GetCameraImageWidth()
	{
		if (!mStarted) return 0;
		return camerainfo.width;
	}

	public uint getImageWidth()
	{
		if (!mStarted) return 0;
		return camerainfo.width;
	}

	[Obsolete("Please use getImageHeight instead")]
	public uint GetCameraImageHeight()
	{
		if (!mStarted) return 0;
		return camerainfo.height;
	}

	public uint getImageHeight()
	{
		if (!mStarted) return 0;
		return camerainfo.height;
	}

	public uint getImageSize()
	{
		if (!mStarted) return 0;
		return camerainfo.size;
	}

	public bool isEnableSyncPose()
	{
		if (!mStarted) return false;
		return syncPose;
	}

	public IntPtr getNativeFrameBuffer()
	{
		if (!mStarted) return IntPtr.Zero;
		return mframeBuffer;
	}

	public void stopCamera()
	{
		if (!mStarted) return ;
		WaveVR_Utils.Event.Remove("DrawCameraCompleted", OnUpdateCameraCompleted);

		if (syncPose)
		{
			Log.i(LOG_TAG, "Reset WaveVR_Render submit pose");
			WaveVR_Render.ResetPoseUsedOnSubmit();
		}

		Interop.WVR_StopCamera();
		if (mframeBuffer != IntPtr.Zero)
		{
			Marshal.FreeHGlobal(mframeBuffer);
			mframeBuffer = IntPtr.Zero;
		}
		Log.i(LOG_TAG, "Release native texture resources");
		WaveVR_Utils.SendRenderEvent(WaveVR_Utils.RENDEREVENTID_ReleaseTexture);
		mStarted = false;
	}

	public WVR_PoseState_t getFramePose()
	{
		return mPoseState;
	}

	public void updateTexture(IntPtr textureId)
	{
		if (!mStarted)
		{
			Log.w(LOG_TAG, "Camera not start yet");
			return;
		}

		PrintDebugLog("updateTexture start");

		nativeTextureId = textureId;
		if (WaveVR_Render.Instance != null)
			origin = WaveVR_Render.Instance.origin;

		if (mframeBuffer != IntPtr.Zero)
		{
			uint predictInMs = 0;
			PrintDebugLog("updateTexture frameBuffer and PoseState, predict time:" + predictInMs);

			Interop.WVR_GetFrameBufferWithPoseState(mframeBuffer, camerainfo.size, origin, predictInMs, ref mPoseState);

			if (syncPose)
			{
				PrintDebugLog("Sync camera frame buffer with poseState, timeStamp: " + mPoseState.PoseTimestamp_ns);
				WaveVR_Render.SetPoseUsedOnSubmit(mPoseState);
			}

			PrintDebugLog("send event to draw OpenGL");
			WaveVR_Utils.SendRenderEvent(WaveVR_Utils.RENDEREVENTID_DrawTextureWithBuffer);
		}
	}
}
