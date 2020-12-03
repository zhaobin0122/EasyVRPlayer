// "WaveVR SDK 
// © 2017 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the WaveVR SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System.Collections.Generic;
using wvr;
using WVR_Log;

public class WaveVR_PermissionManager {
	private static string LOG_TAG = "WaveVR_PermissionManager";

	public class RequestResult
	{
		private string mPermission;
		private bool mGranted;

		public RequestResult(string name, bool granted)
		{
			mPermission = name;
			mGranted = granted;
		}
		public string PermissionName
		{
			get { return mPermission; }
		}

		public bool Granted
		{
			get { return mGranted; }
		}
	}

	private static WaveVR_PermissionManager mInstance = null;
	public delegate void requestCompleteCallback(List<RequestResult> results);
	public delegate void requestUsbCompleteCallback(bool result);
	private static requestCompleteCallback mCallback = null;
	private static requestUsbCompleteCallback mUsbCallback = null;

	public static WaveVR_PermissionManager instance {
		get
		{
			if (mInstance == null)
			{
				mInstance = new WaveVR_PermissionManager();
			}

			return mInstance;
		}
	}

	public static void requestDoneCallback(List<WVR_RequestResult> results)
	{
		Log.d(LOG_TAG, "requestDoneCallback, result count = " + results.Count);
		List<RequestResult> listResult = new List<RequestResult>();

		for (int i = 0; i < results.Count; i++)
		{
			listResult.Add(new RequestResult(results[i].mPermission, results[i].mGranted));
		}

		mCallback(listResult);
	}

	public static void requestUsbDoneCallback(bool result)
	{
		Log.d(LOG_TAG, "requestUsbDoneCallback, result=" + result);
		mUsbCallback(result);
	}

	public bool isInitialized()
	{
		bool ret = Interop.WVR_IsPermissionInitialed();
		Log.d(LOG_TAG, "isInitialized: " + ret);
		return ret;
	}

	public void requestPermissions(string[] permissions, requestCompleteCallback cb)
	{
		Log.d(LOG_TAG, "requestPermission");

		mCallback = cb;

		Interop.WVR_RequestPermissions(permissions, requestDoneCallback);
	}

	public void requestUsbPermission(requestUsbCompleteCallback cb)
	{
		Log.d(LOG_TAG, "requestUsbPermission");
		mUsbCallback = cb;
		Interop.WVR_RequestUsbPermission(requestUsbDoneCallback);
	}

	public bool isPermissionGranted(string permission)
	{
		bool ret = Interop.WVR_IsPermissionGranted(permission);
		Log.d(LOG_TAG, "isPermissionGranted: permission = " + permission + " : " + ret);

		return ret;
	}

	public bool shouldGrantPermission(string permission)
	{
		bool ret = Interop.WVR_ShouldGrantPermission(permission);
		Log.d(LOG_TAG, "shouldGrantPermission: permission = " + permission + " : " + ret);

		return ret;
	}

	public bool showDialogOnScene()
	{
		bool ret = Interop.WVR_ShowDialogOnScene();
		Log.d(LOG_TAG, "showDialogOnScene: " + ret);

		return ret;
	}
}
