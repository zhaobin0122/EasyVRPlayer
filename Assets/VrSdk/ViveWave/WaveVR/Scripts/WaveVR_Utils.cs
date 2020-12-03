// "WaveVR SDK
// © 2017 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the WaveVR SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

#define SYSTRACE_NATIVE  // Systrace in native support multi-thread rendering.
using UnityEngine;
using System.Collections;
using wvr;
using WVR_Log;
using System.Runtime.InteropServices;
using AOT;
using System.Text;
using System;
using UnityEngine.Rendering;
using System.Collections.Generic;
using UnityEngine.Profiling;

/// This class is mainly for common handling:
/// including event handling and pose data handling.
public static class WaveVR_Utils
{
	public static string LOG_TAG = "WVR_Utils";

	public enum DegreeField
	{
		DOF3,
		DOF6
	}

	public enum WVR_PerfLevel
	{
		System = 0,			//!< System defined performance level (default)
		Minimum = 1,		   //!< Minimum performance level
		Medium = 2,			//!< Medium performance level
		Maximum = 3			//!< Maximum performance level
	};

	public struct WVR_ButtonState_t
	{
		public ulong BtnPressed;
		public ulong BtnTouched;
	}

	public class OEMConfig
	{
		private static bool isSetCallback = false;

		public static void OEMConfig_Changed()
		{
			Log.i("OEMConfig", "onConfigChanged callback");
			WaveVR_Utils.Event.Send(WaveVR_Utils.Event.OEM_CONFIG_CHANGED);
		}

		public static void initOEMConfig()
		{
			if (!isSetCallback)
			{
				Log.i("OEMConfig", "initOEMConfig");
				Interop.WVR_SetOEMConfigChangedCallback(OEMConfig_Changed);
				isSetCallback = true;
			}
		}
		public static string getControllerConfig()
		{
			initOEMConfig();
			return Interop.WVR_GetOEMConfigByKey("controller_property");
		}

		public static string getBatteryConfig()
		{
			initOEMConfig();
			return Interop.WVR_GetOEMConfigByKey("battery_indicator");
		}

		public static string getSingleBeamEnableConfig()
		{
			initOEMConfig();
			return Interop.WVR_GetOEMConfigByKey("controller_singleBeam");
		}
	}

	public class Event
	{
		public static string DEVICE_CONNECTED = "device_connected";
		public static string NEW_POSES = "new_poses";
		public static string AFTER_NEW_POSES = "after_new_poses";
		public static string ALL_VREVENT = "all_vrevent";  // Called when had event from WVR_PollEventQueue()
		public static string BATTERY_STATUS_UPDATE = "BatteryStatusUpdate";
		public static string CONTROLLER_MODEL_LOADED = "controller_model_loaded";
		public static string CONTROLLER_MODEL_UNLOADED = "controller_model_unloaded";
		public static string PRE_CULL_LEFT = "PreCull_left";
		public static string PRE_CULL_RIGHT = "PreCull_right";
		public static string SWIPE_EVENT = "SWIPE_EVENT";
		public static string SYSTEMFOCUS_CHANGED = "SYSTEMFOCUS_CHANGED";
		public static string INTERACTION_MODE_CHANGED = "INTERACTION_MODE_CHANGED";
		public static string ADAPTIVE_CONTROLLER_READY = "adaptive_controller_ready";
		public static string RENDER_CONFIGURATION_CHANGED = "RenderConfigChanged";
		[Obsolete]
		public static string DEVICE_ROLE_CHANGED = "device_role_changed";
		public static string DS_ASSETS_NOT_FOUND = "FBXorPNG_Not_Found";
		public static string OEM_CONFIG_CHANGED = "OEM_CONFIG_CHANGED";
		public static string DEVICE_STATUS_UPDATE = "TrackedDeviceUpdated";
		public static string IPD_CHANGED = "IpdChanged";
		public static string HAND_STATIC_GESTURE_LEFT = "HAND_STATIC_GESTURE_LEFT";
		public static string HAND_STATIC_GESTURE_RIGHT = "HAND_STATIC_GESTURE_RIGHT";
		public static string HAND_DYNAMIC_GESTURE_LEFT = "HAND_DYNAMIC_GESTURE_LEFT";
		public static string HAND_DYNAMIC_GESTURE_RIGHT = "HAND_DYNAMIC_GESTURE_RIGHT";
		public static string HAND_GESTURE_STATUS = "HAND_GESTURE_STATUS";
		public static string HAND_TRACKING_STATUS = "HAND_TRACKING_STATUS";

		public delegate void Handler(params object[] args);

		public static void Listen(string message, Handler action)
		{
			List<Handler> handlerList = null;
			listeners.TryGetValue(message, out handlerList);
			if (handlerList == null)
			{
				handlerList = new List<Handler>();
				listeners[message] = handlerList;
			}
			else if (handlerList.Contains(action))
			{
				Log.w(LOG_TAG,
					Log.CSB
					.AppendLine("Skip a duplicated listener from here:")
					.Append(new System.Diagnostics.StackTrace(false).ToString())
					.ToString());
				return;
			}

			handlerList.Add(action);
		}

		public static void Remove(string message, Handler action)
		{
			List<Handler> handlerList = null;
			listeners.TryGetValue(message, out handlerList);
			if (handlerList == null)
				return;
			if (!handlerList.Contains(action))
				return;

			handlerList.Remove(action);
		}

		public static void Send(string message, params object[] args)
		{
			List<Handler> handlerList = null;
			listeners.TryGetValue(message, out handlerList);
			if (handlerList != null) {
				int N = handlerList.Count;
				for (int i = N - 1; i >= 0; i--)
				{
					Handler single = handlerList[i];
					try
					{
						single(args);
					}
					catch (Exception e)
					{
						Log.e(LOG_TAG, e.ToString(), true);
						handlerList.Remove(single);
						Log.e(LOG_TAG, "A listener is removed due to exception.", true);
					}
				}
			}
		}

		private static Dictionary<string, List<Handler>> listeners = new Dictionary<string, List<Handler>>();
	}

	// Example:  SafeExecuteAllDelegate<OnConfigurationChanged>(onConfigurationChanged, a => a(this));
	// Example2:  SafeExecuteAllDelegate<OnConfigurationChanged>(onConfigurationChanged, a => { using (var ee = Log.ee("call custom", true)) { a(this); } });
	public static void SafeExecuteAllDelegate<T>(Delegate multi, Action<T> invoker, bool throws = false)
	{
		if (multi == null)
			return;
#if NET_4_6
		List<Exception> exceptions = null;
		if (throws)
			exceptions = new List<Exception>();
#endif
		// TODO This line will have GC.Alloc()
		var list = multi.GetInvocationList();

		int N = list.Length;
		for (int i = N - 1; i >= 0; i--)
		{
			T single = (T)(object)list[i];
			try
			{
				if (single != null)
					invoker(single);
			}
			catch (Exception e)
			{
				Log.e(LOG_TAG, e.ToString(), true);
#if NET_4_6
				if (throws)
					exceptions.Add(e);
#endif
			}
		}
#if NET_4_6
		if (throws && exceptions.Count > 0)
			throw new AggregateException(exceptions);
#endif
	}

	private static float _copysign(float sizeval, float signval)
	{
		return Mathf.Sign(signval) == 1 ? Mathf.Abs(sizeval) : -Mathf.Abs(sizeval);
	}

	public static Quaternion GetRotation(Matrix4x4 matrix)
	{
		float tr = matrix.m00 + matrix.m11 + matrix.m22;
		float qw, qx, qy, qz;
		if (tr > 0) {
			float S = Mathf.Sqrt(tr + 1.0f) * 2; // S=4*qw
			qw = 0.25f * S;
			qx = (matrix.m21 - matrix.m12) / S;
			qy = (matrix.m02 - matrix.m20) / S;
			qz = (matrix.m10 - matrix.m01) / S;
		} else if ((matrix.m00 > matrix.m11) & (matrix.m00 > matrix.m22)) {
			float S = Mathf.Sqrt(1.0f + matrix.m00 - matrix.m11 - matrix.m22) * 2; // S=4*qx
			qw = (matrix.m21 - matrix.m12) / S;
			qx = 0.25f * S;
			qy = (matrix.m01 + matrix.m10) / S;
			qz = (matrix.m02 + matrix.m20) / S;
		} else if (matrix.m11 > matrix.m22) {
			float S = Mathf.Sqrt(1.0f + matrix.m11 - matrix.m00 - matrix.m22) * 2; // S=4*qy
			qw = (matrix.m02 - matrix.m20) / S;
			qx = (matrix.m01 + matrix.m10) / S;
			qy = 0.25f * S;
			qz = (matrix.m12 + matrix.m21) / S;
		} else {
			float S = Mathf.Sqrt(1.0f + matrix.m22 - matrix.m00 - matrix.m11) * 2; // S=4*qz
			qw = (matrix.m10 - matrix.m01) / S;
			qx = (matrix.m02 + matrix.m20) / S;
			qy = (matrix.m12 + matrix.m21) / S;
			qz = 0.25f * S;
		}
#if UNITY_2018_1_OR_NEWER
		return new Quaternion(qx, qy, qz, qw).normalized;
#else
		Vector4 un = new Vector4(qx, qy, qz, qw);
		un.Normalize();
		return new Quaternion(un.x, un.y, un.z, un.w);
#endif
	}

	public static Quaternion GetRotation(WVR_Quatf_t glQuat)
	{
		return new Quaternion (glQuat.x, glQuat.y, -glQuat.z, -glQuat.w);
	}

	public static Vector3 GetPosition(this Matrix4x4 matrix)
	{
		var x = matrix.m03;
		var y = matrix.m13;
		var z = matrix.m23;

		return new Vector3(x, y, z);
	}

	public static Vector3 GetPosition(WVR_Vector3f_t glVector)
	{
		return new Vector3 (glVector.v0, glVector.v1, -glVector.v2);
	}

	public static void GetVectorFromGL(WVR_Vector3f_t gl_vec, out Vector3 unity_vec)
	{
		unity_vec.x = gl_vec.v0;
		unity_vec.y = gl_vec.v1;
		unity_vec.z = -gl_vec.v2;
	}

	public static Vector3 GetScale(this Matrix4x4 matrix)
	{
		Vector3 scale;
		scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
		scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
		scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
		return scale;
	}

	public static string GetControllerName(WaveVR_Controller.EDeviceType type)
	{
		string retString = "";
		if (type == WaveVR_Controller.EDeviceType.Head)
		{
			Log.w(LOG_TAG, "EDeviceType is Head");
			return retString;
		}

		WVR_DeviceType deviceType = WaveVR_Controller.Input(type).DeviceType;
		if (WaveVR_Controller.Input(type).connected)
		{
			int bufferSize = 128;
			uint resultVertLength = 128;
			string parameterName = "GetRenderModelName";
			IntPtr ptrParameterName = Marshal.StringToHGlobalAnsi(parameterName);
			IntPtr ptrResult = Marshal.AllocHGlobal(bufferSize);
			uint ret = Interop.WVR_GetParameters(deviceType, ptrParameterName, ptrResult, resultVertLength);
			if (ret > 0)
			{
				retString = Marshal.PtrToStringAnsi(ptrResult);
			} else
			{
				Log.w(LOG_TAG, "WVR_GetParameters returns empty");
			}
		} else
		{
			Log.w(LOG_TAG, type + " controller is disconnect");
		}
		Log.i(LOG_TAG, "GetControllerName returns " + retString);
		return retString;
	}

	// get new position and rotation from new pose
	[System.Serializable]
	public struct RigidTransform
	{
		public Vector3 pos;
		public Quaternion rot;

		public static RigidTransform identity
		{
			get { return new RigidTransform(Vector3.zero, Quaternion.identity); }
		}

		public RigidTransform(Vector3 pos, Quaternion rot)
		{
			this.pos = pos;
			this.rot = rot;
		}

		public RigidTransform(Transform t)
		{
			this.pos = t.position;
			this.rot = t.rotation;
		}

		public RigidTransform(WVR_Matrix4f_t pose)
		{
			var m = toMatrix44(pose);
			this.pos = GetPosition(m);
			this.rot = GetRotation(m);
		}

		public static Matrix4x4 toMatrix44(WVR_Matrix4f_t pose, bool glToUnity = true)
		{
			var m = Matrix4x4.identity;
			int sign = glToUnity ? -1 : 1;

			m[0, 0] = pose.m0;
			m[0, 1] = pose.m1;
			m[0, 2] = pose.m2 * sign;
			m[0, 3] = pose.m3;

			m[1, 0] = pose.m4;
			m[1, 1] = pose.m5;
			m[1, 2] = pose.m6 * sign;
			m[1, 3] = pose.m7;

			m[2, 0] = pose.m8 * sign;
			m[2, 1] = pose.m9 * sign;
			m[2, 2] = pose.m10;
			m[2, 3] = pose.m11 * sign;

			m[3, 0] =  pose.m12;
			m[3, 1] =  pose.m13;
			m[3, 2] =  pose.m14;
			m[3, 3] =  pose.m15;

			return m;
		}

		public static WVR_Matrix4f_t ToWVRMatrix(Matrix4x4 m, bool unityToGL = true)
		{
			WVR_Matrix4f_t pose;
			int sign = unityToGL ? -1 : 1;

			pose.m0 =  m[0, 0];
			pose.m1 =  m[0, 1];
			pose.m2 =  m[0, 2] * sign;
			pose.m3 =  m[0, 3];

			pose.m4 =  m[1, 0];
			pose.m5 =  m[1, 1];
			pose.m6 =  m[1, 2] * sign;
			pose.m7 =  m[1, 3];

			pose.m8 =  m[2, 0] * sign;
			pose.m9 =  m[2, 1] * sign;
			pose.m10 = m[2, 2];
			pose.m11 = m[2, 3] * sign;

			pose.m12 =  m[3, 0];
			pose.m13 =  m[3, 1];
			pose.m14 =  m[3, 2];
			pose.m15 =  m[3, 3];

			return pose;
		}

		public static Vector3 ToUnityPos(Vector3 glPos)
		{
			glPos.z *= -1;
			return glPos;
		}

		public void update(WVR_Matrix4f_t pose)
		{
			var m = toMatrix44(pose);
			this.pos = GetPosition(m);
			this.rot = GetRotation(m);
		}

		public void update(Vector3 position, Quaternion orientation)
		{
			this.pos = position;
			this.rot = orientation;
		}

		public override bool Equals(object o)
		{
			if (o is RigidTransform)
			{
				RigidTransform t = (RigidTransform)o;
				return pos == t.pos && rot == t.rot;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return pos.GetHashCode() ^ rot.GetHashCode();
		}

		public static bool operator ==(RigidTransform a, RigidTransform b)
		{
			return a.pos == b.pos && a.rot == b.rot;
		}

		public static bool operator !=(RigidTransform a, RigidTransform b)
		{
			return a.pos != b.pos || a.rot != b.rot;
		}

		public static RigidTransform operator *(RigidTransform a, RigidTransform b)
		{
			return new RigidTransform
			{
				rot = a.rot * b.rot,
				pos = a.pos + a.rot * b.pos
			};
		}

		public void Inverse()
		{
			rot = Quaternion.Inverse(rot);
			pos = -(rot * pos);
		}

		public RigidTransform GetInverse()
		{
			var t = new RigidTransform(pos, rot);
			t.Inverse();
			return t;
		}

		public Vector3 TransformPoint(Vector3 point)
		{
			return pos + (rot * point);
		}

		public static Vector3 operator *(RigidTransform t, Vector3 v)
		{
			return t.TransformPoint(v);
		}

	}

#if SYSTRACE_NATIVE
	public static Queue TraceSessionNameQueue = new Queue(5);
#else
	public static AndroidJavaObject trace = new AndroidJavaObject("android.os.Trace");
#endif

	public class Trace {
		public static void BeginSection(string sectionName, bool inRenderThread = true)
		{
#if !UNITY_EDITOR && UNITY_ANDROID
#if SYSTRACE_NATIVE
			if (inRenderThread) {
				lock (TraceSessionNameQueue)
				{
					TraceSessionNameQueue.Enqueue(sectionName);
				}
				SendRenderEvent(RENDEREVENTID_Systrace_BeginSession);
			} else {
				TraceBeginSection(sectionName);
			}
#else
			trace.CallStatic("beginSection", sectionName);
#endif
#endif
		}

		public static void EndSection(bool inRenderThread = true)
		{
#if !UNITY_EDITOR && UNITY_ANDROID
#if SYSTRACE_NATIVE
			if (inRenderThread) {
				SendRenderEvent(RENDEREVENTID_Systrace_EndSession);
			} else {
				TraceEndSection();
			}
#else
			trace.CallStatic("endSection");
#endif
#endif
		}
	}

	public static void notifyActivityUnityStarted()
	{
#if !UNITY_EDITOR && UNITY_ANDROID
		AndroidJavaClass clazz = new AndroidJavaClass("com.htc.vr.unity.WVRUnityVRActivity");
		AndroidJavaObject activity = clazz.CallStatic<AndroidJavaObject>("getInstance");
		activity.Call("onUnityStarted");
#endif
	}

	public const int k_nRenderEventID_SubmitL = 1;
	public const int k_nRenderEventID_SubmitR = 2;
	public const int k_nRenderEventID_SubmitBoth = 3;
	public const int k_nRenderEventID_GraphicInitial = 8;
	public const int k_nRenderEventID_GraphicShutdown = 9;
	public const int k_nRenderEventID_RenderEyeL = 0x100;
	public const int k_nRenderEventID_RenderEyeR = 0x101;
	public const int k_nRenderEventID_RenderEyeEndL = 0x102;
	public const int k_nRenderEventID_RenderEyeEndR = 0x103;

	public const int k_nRenderEventID_RenderEyeBoth = 0x111;
	public const int k_nRenderEventID_RenderEyeEndBoth = 0x112;

#if UNITY_STANDALONE
	[DllImportAttribute("wave_api", EntryPoint = "GetRenderEventFunc", CallingConvention = CallingConvention.Cdecl)]
	public static extern System.IntPtr GetRenderEventFuncHVR();
	public static System.IntPtr GetRenderEventFunc() {
		Log.i("WVR_HVR", "GetRenderEventFunc()");
		return GetRenderEventFuncHVR();
	}

	[DllImportAttribute("wave_api", EntryPoint = "NativeRenderEvent", CallingConvention = CallingConvention.Cdecl)]
	public static extern void NativeRenderEventHVR(int eventID);
	public static void NativeRenderEvent(int eventID) {
		Log.i("WVR_HVR", "NativeRenderEvent()+");
		NativeRenderEventHVR(eventID);
	    Log.i("WVR_HVR", "NativeRenderEvent()-");
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_SetColorSpace", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetColorSpaceHVR(int colorspace);
	public static void SetColorSpace(int colorspace) {
		Log.i("WVR_HVR", "Fake SetColorSpace()");
	}

	// This api does not guarantee these argument are used by current frame.  Use it if you know what will happen.
	//[DllImportAttribute("wave_api", EntryPoint = "SetSubmitOptionalArgument", CallingConvention = CallingConvention.Cdecl)]
	//public static extern void SetSubmitOptionalArgumentHVR([Out] WVR_PoseState_t [] poses, int submit_extend_flag);
	public static void SetSubmitOptionalArgument([Out] WVR_PoseState_t [] poses, int submit_extend_flag) {
		Log.i("WVR_HVR", "Fake SetSubmitOptionalArgument()");
		//SetSubmitOptionalArgument(poses, submit_extend_flag);
	}

	//[DllImportAttribute("wave_api", EntryPoint = "nativeProcessEngineEvent", CallingConvention = CallingConvention.Cdecl)]
	//public static extern void nativeProcessEngineEventHVR(uint tID, uint eventID);
	public static void NativeProcessEngineEvent(uint tID, uint eventID) {
		Log.i("WVR_HVR", "Fake NativeProcessEngineEvent()+");
		//nativeProcessEngineEventHVR(tID, eventID);
		Log.i("WVR_HVR", "Fake NativeProcessEngineEvent()-");
	}

	[DllImportAttribute("wave_api", EntryPoint = "IsSinglePassSupported", CallingConvention = CallingConvention.Cdecl)]
	public static extern int IsSinglePassSupportedHVR();
	public static int IsSinglePassSupported() {
		Log.i("WVR_HVR", "IsSinglePassSupported()");
		return 1;
	}

	//[DllImportAttribute("wave_api", EntryPoint = "PrepareSinglePassTexture", CallingConvention = CallingConvention.Cdecl)]
	//public static extern System.IntPtr PrepareSinglePassTextureHVR(int antiAliasing, int width, int height);
	public static System.IntPtr PrepareSinglePassTexture(int antiAliasing, int width, int height) {
		Log.i("WVR_HVR", "Fake PrepareSinglePassTexture()");
		//return PrepareSinglePassTextureHVR(antiAliasing, width, height);
		return System.IntPtr.Zero;
	}

	//[DllImportAttribute("wave_api", EntryPoint = "SinglePassBeforeForwardOpaque", CallingConvention = CallingConvention.Cdecl)]
	//public static extern void SinglePassBeforeForwardOpaqueHVR();
	public static void SinglePassBeforeForwardOpaque() {
		Log.i("WVR_HVR", "Fake SinglePassBeforeForwardOpaque()");
		//SinglePassBeforeForwardOpaqueHVR();
	}

	//[DllImportAttribute("wave_api", EntryPoint = "SinglePassPostRender", CallingConvention = CallingConvention.Cdecl)]
	//public static extern void SinglePassPostRenderHVR();
	public static void SinglePassPostRender() {
		Log.i("WVR_HVR", "Fake SinglePassPostRender()");
		//SinglePassPostRenderHVR();
	}

#if SYSTRACE_NATIVE
	private static void TraceBeginSection(string name) {}

	private static void TraceEndSection() {}
#endif

	public static bool WVR_IsATWActive() {
		Log.i("WVR_HVR", "Fake WVR_IsATWActive()");
		return true;
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_GetNumberOfTextures", CallingConvention = CallingConvention.Cdecl)]
	public static extern int WVR_GetNumberOfTexturesHVR();
	public static int WVR_GetNumberOfTextures() {
	    int num = 1;
		Log.i("WVR_HVR", "WVR_GetNumberOfTextures()");
		num = WVR_GetNumberOfTexturesHVR();
		Log.i("WVR_HVR", "num = " + num);
		return num;
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_StoreRenderTextures", CallingConvention = CallingConvention.Cdecl)]
	public static extern System.IntPtr WVR_StoreRenderTexturesHVR(System.IntPtr[] texturesIDs, int size, bool eEye, WVR_TextureTarget target);
	public static System.IntPtr WVR_StoreRenderTextures(System.IntPtr[] texturesIDs, int size, bool eEye, WVR_TextureTarget target) {
		Log.i("WVR_HVR", "WVR_StoreRenderTextures()");
		return WVR_StoreRenderTexturesHVR(texturesIDs, size, eEye, target);
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_ReplaceCurrentTextureID", CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr WVR_ReplaceCurrentTextureIDHVR(IntPtr queue, IntPtr imageHandle);
	public static IntPtr WVR_ReplaceCurrentTextureID(IntPtr queue, IntPtr imageHandle)
	{
		Log.i("WVR_HVR", "WVR_ReplaceCurrentTextureID()");
		return WVR_ReplaceCurrentTextureIDHVR(queue, imageHandle);
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_GetAvailableTextureID", CallingConvention = CallingConvention.Cdecl)]
	public static extern System.IntPtr WVR_GetAvailableTextureIDHVR(System.IntPtr queue);
	public static System.IntPtr WVR_GetAvailableTextureID(System.IntPtr queue) {
		Log.i("WVR_HVR", "WVR_GetAvailableTextureID()");
		return WVR_GetAvailableTextureIDHVR(queue);
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_IsPresentedOnExternalD", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool WVR_IsPresentedOnExternalHVR();
	public static bool WVR_IsPresentedOnExternal() {
		Log.i("WVR_HVR", "WVR_IsPresentedOnExternal()");
		return WVR_IsPresentedOnExternalHVR();
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_LoadCriteriaData", CallingConvention = CallingConvention.Cdecl)]
	public static extern int WVR_LoadCriteriaDataHVR(string criteriaData);
	public static int WVR_LoadCriteriaData(string criteriaData) {
		Log.i("WVR_HVR", "WVR_LoadCriteriaData()");
		return WVR_LoadCriteriaDataHVR(criteriaData);
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_StartPerformanceTest", CallingConvention = CallingConvention.Cdecl)]
	public static extern int WVR_StartPerformanceTestHVR(string sceneID);
	public static int WVR_StartPerformanceTest(string sceneID) {
		Log.i("WVR_HVR", "WVR_StartPerformanceTest()");
		return WVR_StartPerformanceTestHVR(sceneID);
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_EndPerformanceTest", CallingConvention = CallingConvention.Cdecl)]
	public static extern int WVR_EndPerformanceTestHVR(StringBuilder pReport, uint ReportSize);
	public static int WVR_EndPerformanceTest(StringBuilder pReport, uint ReportSize) {
		Log.i("WVR_HVR", "WVR_EndPerformanceTest()");
		return WVR_EndPerformanceTestHVR(pReport, ReportSize);
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_GetPerformanceReportSize", CallingConvention = CallingConvention.Cdecl)]
	public static extern uint WVR_GetPerformanceReportSizeHVR();
	public static uint WVR_GetPerformanceReportSize() {
		Log.i("WVR_HVR", "WVR_GetPerformanceReportSize()");
		return WVR_GetPerformanceReportSizeHVR();
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_SetPerformanceLevels", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool WVR_SetPerformanceLevelsHVR(WVR_PerfLevel cpuLevel, WVR_PerfLevel gpuLevel);
	public static bool WVR_SetPerformanceLevels(WVR_PerfLevel cpuLevel, WVR_PerfLevel gpuLevel) {
		Log.i("WVR_HVR", "WVR_SetPerformanceLevels()");
		return WVR_SetPerformanceLevelsHVR(cpuLevel, gpuLevel);
	}

	[DllImportAttribute("wave_api", EntryPoint = "WVR_GetStencilMesh", CallingConvention = CallingConvention.Cdecl)]
	public static extern void WVR_GetStencilMeshHVR(WVR_Eye eEye, ref uint vertexCount, ref uint triangleCount, uint floatArrayCount, [In, Out] float[] vertexData, uint intArrayCount, [In, Out] int[] indexData);
	public static void WVR_GetStencilMesh(WVR_Eye eEye, ref uint vertexCount, ref uint triangleCount, uint floatArrayCount, [In, Out] float[] vertexData, uint intArrayCount, [In, Out] int[] indexData) {
		Log.i("WVR_HVR", "WVR_GetStencilMesh()");
		WVR_GetStencilMeshHVR(eEye, ref vertexCount, ref triangleCount, floatArrayCount, vertexData, intArrayCount, indexData);
	}
#else
	[DllImport("wvrunity", CallingConvention = CallingConvention.Cdecl)]
	public static extern void NativeRenderEvent(int eventID);

	[DllImport("wvrunity", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetColorSpace(int colorspace);

	// This api does not guarantee these argument are used by current frame.  Use it if you know what will happen.
	[DllImport("wvrunity", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetSubmitOptionalArgument([Out] WVR_PoseState_t [] poses, int submit_extend_flag);

	[DllImport("wvrunity", EntryPoint = "nativeProcessEngineEvent", CallingConvention = CallingConvention.Cdecl)]
	public static extern void NativeProcessEngineEvent(uint tID, uint eventID);

	[DllImport("wvrunity", CallingConvention = CallingConvention.Cdecl)]
	public static extern int IsSinglePassSupported();
	
	[DllImport("wvrunity", CallingConvention = CallingConvention.Cdecl)]
	public static extern System.IntPtr PrepareSinglePassTexture(int antiAliasing, int width, int height);

	[DllImport("wvrunity", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SinglePassBeforeForwardOpaque();

	[DllImport("wvrunity", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SinglePassPostRender();

	//[DllImport("wvrunity", CallingConvention = CallingConvention.Cdecl)]
	//public static extern void SinglePassSubmit();

#if SYSTRACE_NATIVE
	[DllImport("wvrunity", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	private static extern void TraceBeginSection(string name);

	[DllImport("wvrunity", CallingConvention = CallingConvention.Cdecl)]
	private static extern void TraceEndSection();
#endif

	[DllImportAttribute("wvr_api", EntryPoint = "WVR_IsATWActive", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool WVR_IsATWActive();

	[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetNumberOfTextures", CallingConvention = CallingConvention.Cdecl)]
	public static extern int WVR_GetNumberOfTextures();

	[DllImportAttribute("wvr_api", EntryPoint = "WVR_StoreRenderTextures", CallingConvention = CallingConvention.Cdecl)]
	public static extern System.IntPtr WVR_StoreRenderTextures(System.Int32[] texturesIDs, int size, bool eEye, WVR_TextureTarget target);

	[DllImportAttribute("wvr_api", EntryPoint = "WVR_ReplaceCurrentTextureID", CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr WVR_ReplaceCurrentTextureID(IntPtr queue, IntPtr imageHandle);

	[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetAvailableTextureID", CallingConvention = CallingConvention.Cdecl)]
	public static extern uint WVR_GetAvailableTextureID(System.IntPtr queue);

	[DllImportAttribute("wvr_api", EntryPoint = "WVR_IsPresentedOnExternalD", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool WVR_IsPresentedOnExternal();

	[DllImportAttribute("wvr_api", EntryPoint = "WVR_LoadCriteriaData", CallingConvention = CallingConvention.Cdecl)]
	public static extern int WVR_LoadCriteriaData(string criteriaData);

	[DllImportAttribute("wvr_api", EntryPoint = "WVR_StartPerformanceTest", CallingConvention = CallingConvention.Cdecl)]
	public static extern int WVR_StartPerformanceTest(string sceneID);

	[DllImportAttribute("wvr_api", EntryPoint = "WVR_EndPerformanceTest", CallingConvention = CallingConvention.Cdecl)]
	public static extern int WVR_EndPerformanceTest(StringBuilder pReport, uint ReportSize);

	[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetPerformanceReportSize", CallingConvention = CallingConvention.Cdecl)]
	public static extern uint WVR_GetPerformanceReportSize();

	[DllImportAttribute("wvr_api", EntryPoint = "WVR_SetPerformanceLevels", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool WVR_SetPerformanceLevels(WVR_PerfLevel cpuLevel, WVR_PerfLevel gpuLevel);

	[DllImportAttribute("wvr_api", EntryPoint = "WVR_GetStencilMesh", CallingConvention = CallingConvention.Cdecl)]
	public static extern void WVR_GetStencilMesh(WVR_Eye eEye, ref uint vertexCount, ref uint triangleCount, uint floatArrayCount, [In, Out] float[] vertexData, uint intArrayCount, [In, Out] int[] indexData);
#endif

	public const int RENDEREVENTID_INIT_GRAPHIC = 0;
	public const int RENDEREVENTID_SHUTDOWN_GRAPHIC = 1;
	public const int RENDEREVENTID_Systrace_BeginSession = 4;
	public const int RENDEREVENTID_Systrace_EndSession = 5;
	public const int RENDEREVENTID_StartCamera = 21;
	public const int RENDEREVENTID_StopCamera = 22;
	public const int RENDEREVENTID_UpdateCamera = 23;
	public const int RENDEREVENTID_DrawTextureWithBuffer = 24;
	public const int RENDEREVENTID_ReleaseTexture = 25;
	public const int RENDEREVENTID_RenderMaskLeft = 30;
	public const int RENDEREVENTID_RenderMaskRight = 31;

	public const int RENDEREVENTID_SinglePassPrepare = 90;
	public const int RENDEREVENTID_SinglePassPrepareWithAntiAliasing2x = 92;
	public const int RENDEREVENTID_SinglePassPrepareWithAntiAliasing4x = 94;
	public const int RENDEREVENTID_SinglePassPrepareWithAntiAliasing8x = 98;
	public const int RENDEREVENTID_SinglePassBeforeForwardOpaque = 86;
	public const int RENDEREVENTID_SinglePassPostRender = 87;

	public const int RENDEREVENTID_ExecuteCustomFunction = 45;

	public const int RENDEREVENTID_EditorEmptyOperation = 65536;

	public const int RENDEREVENTID_SubmitL = 1001;
	public const int RENDEREVENTID_SubmitR = 1002;

	public const uint RENDEREVENTID_SubmitL_Index_Min = 1100;
	public const uint RENDEREVENTID_SubmitR_Index_Min = 1200;

	public const uint RENDEREVENTID_Wait_Get_Poses = 2000;

	[MonoPInvokeCallback(typeof(RenderEventDelegate))]
	private static void RenderEvent(int eventId)
	{
		if ((eventId & (int)EngineEventID.ENGINE_EVENT_ID_BEGIN) == (int) EngineEventID.ENGINE_EVENT_ID_BEGIN)
		{
			NativeProcessEngineEvent((uint) EngineThreadID.RENDER_THREAD, (uint)eventId);
			return;
		}

		switch (eventId)
		{
			case RENDEREVENTID_EditorEmptyOperation:
				break;
			case RENDEREVENTID_INIT_GRAPHIC:
				break;
			case RENDEREVENTID_SHUTDOWN_GRAPHIC:
				// Use native code to shutdown compositor.
				NativeRenderEvent(k_nRenderEventID_GraphicShutdown);
				break;
			case RENDEREVENTID_Systrace_BeginSession:
				string sectionName;
				lock (TraceSessionNameQueue)
				{
					try
					{
						sectionName = (string)TraceSessionNameQueue.Dequeue();
					}
					catch (System.InvalidOperationException)
					{
						sectionName = "Empty";
					}
				}
				TraceBeginSection(sectionName);
				break;
			case RENDEREVENTID_Systrace_EndSession:
				TraceEndSection();
				break;
			case RENDEREVENTID_StartCamera:
				{
					WVR_CameraInfo_t camerainfo = new WVR_CameraInfo_t();
					var result = Interop.WVR_StartCamera(ref camerainfo);

					Event.Send("StartCameraCompleted", result, camerainfo);
				}

				break;
			case RENDEREVENTID_StopCamera:
				{
					Interop.WVR_StopCamera();
				}

				break;
			case RENDEREVENTID_UpdateCamera:
				{
					var updated = Interop.WVR_UpdateTexture(WaveVR_CameraTexture.instance.getNativeTextureId());
					Event.Send("UpdateCameraCompleted", updated);
				}
				break;

			case RENDEREVENTID_DrawTextureWithBuffer:
				{
					IntPtr nativeTexId = WaveVR_CameraTexture.instance.getNativeTextureId();
					uint bufferSize = WaveVR_CameraTexture.instance.getImageSize();
					IntPtr framebuffer = WaveVR_CameraTexture.instance.getNativeFrameBuffer();
					uint width = WaveVR_CameraTexture.instance.getImageWidth();
					uint height = WaveVR_CameraTexture.instance.getImageHeight();
					WVR_CameraImageFormat imgFormat = WaveVR_CameraTexture.instance.getImageFormat();

					bool updated = Interop.WVR_DrawTextureWithBuffer(nativeTexId, imgFormat, framebuffer, bufferSize, width, height);

					Event.Send("DrawCameraCompleted", updated);
				}
				break;

			case RENDEREVENTID_ReleaseTexture:
				{
					Interop.WVR_ReleaseCameraTexture();
				}
				break;

			case RENDEREVENTID_RenderMaskLeft:
				{
					Interop.WVR_RenderMask(WVR_Eye.WVR_Eye_Left);
				}
				break;
			case RENDEREVENTID_RenderMaskRight:
				{
					Interop.WVR_RenderMask(WVR_Eye.WVR_Eye_Right);
				}
				break;

			case RENDEREVENTID_SinglePassPrepare:
			case RENDEREVENTID_SinglePassPrepareWithAntiAliasing2x:
			case RENDEREVENTID_SinglePassPrepareWithAntiAliasing4x:
			case RENDEREVENTID_SinglePassPrepareWithAntiAliasing8x:
				{
					var render = WaveVR_Render.Instance;
					if (render == null)
						return;
					int aa = eventId - RENDEREVENTID_SinglePassPrepare;
					PrepareSinglePassTexture(aa, (int) render.sceneWidth, (int) render.sceneHeight);
				}
				break;
			case RENDEREVENTID_SinglePassBeforeForwardOpaque:
				{
					SinglePassBeforeForwardOpaque();
				}
				break;
			case RENDEREVENTID_SinglePassPostRender:
				{
					SinglePassPostRender();
				}
				break;
			case RENDEREVENTID_ExecuteCustomFunction:
				{
					if (mCustomRenderThreadFunc != null)
					{
						mCustomRenderThreadFunc();
					}
					mCustomRenderThreadFunc = null;
				}
				break;
		}
	}

	private static IntPtr GetFunctionPointerForDelegate(Delegate del)
	{
#if UNITY_EDITOR && UNITY_ANDROID
		if (Application.isEditor)
			return IntPtr.Zero;
#endif

#if UNITY_ANDROID
		return Marshal.GetFunctionPointerForDelegate(del);
#else
		return IntPtr.Zero;
#endif
	}

	private delegate void RenderEventDelegate(int e);
	private static RenderEventDelegate RenderEventHandle = new RenderEventDelegate(RenderEvent);
	private static IntPtr RenderEventHandlePtr = GetFunctionPointerForDelegate(RenderEventHandle);

	public delegate void CustomRenderThreadFunc();
	public static CustomRenderThreadFunc mCustomRenderThreadFunc = null;

	public static void SendRenderEvent(int eventId)
	{
#if UNITY_EDITOR && UNITY_ANDROID
		if (Application.isEditor)
		{
			RenderEvent(eventId);
			return;
		}
#endif

#if UNITY_ANDROID
		GL.IssuePluginEvent(RenderEventHandlePtr, eventId);
		return;
#else
		RenderEvent(eventId);
		return;
#endif
	}

	public enum EngineThreadID {
		JAVA_THREAD,
		GAME_THREAD,
		RENDER_THREAD,
		WORKER1_THREAD,
		WORKER2_THREAD,
	}

	public enum EngineEventID
	{
		ENGINE_EVENT_ID_BEGIN = 0xA000,

		HMD_CREATE,
		HMD_INITIAILZED,
		HMD_RESUME,
		HMD_PAUSE,
		HMD_TERMINATED,

		FIRST_FRAME,
		FRAME_START,
		FRAME_END,

		UNITY_AWAKE,
		UNITY_ENABLE,
		UNITY_DISABLE,
		UNITY_START,
		UNITY_DESTROY,
		UNITY_APPLICATION_RESUME,
		UNITY_APPLICATION_PAUSE,
		UNITY_APPLICATION_QUIT,

		ENGINE_EVENT_ID_END
	}

	public static void IssueEngineEvent(EngineEventID eventID)
	{
		IssueEngineEvent(EngineThreadID.GAME_THREAD, eventID);
		IssueEngineEvent(EngineThreadID.RENDER_THREAD, eventID);
	}

	public static void IssueEngineEvent(EngineThreadID tID, EngineEventID eventID)
	{
#if UNITY_EDITOR && UNITY_ANDROID
		if (Application.isEditor)
			return;
#endif
		if (tID == EngineThreadID.RENDER_THREAD)
		{
			SendRenderEvent((int) eventID);
		}
		else
		{
			NativeProcessEngineEvent((uint) tID, (uint)eventID);
		}
	}

	#region Gesture
	public delegate void HandGestureResultDelegate(object sender, bool result);
	public enum HandGestureStatus
	{
		// Initial, can call Start API in this state.
		NOT_START,
		START_FAILURE,

		// Processing, should NOT call API in this state.
		STARTING,
		STOPING,

		// Running, can call Stop API in this state.
		AVAILABLE,

		// Do nothing.
		UNSUPPORT
	}

	public delegate void HandTrackingResultDelegate(object sender, bool result);
	public enum HandTrackingStatus
	{
		// Initial, can call Start API in this state.
		NOT_START,
		START_FAILURE,

		// Processing, should NOT call API in this state.
		STARTING,
		STOPING,

		// Running, can call Stop API in this state.
		AVAILABLE,

		// Do nothing.
		UNSUPPORT
	}
	#endregion
}
