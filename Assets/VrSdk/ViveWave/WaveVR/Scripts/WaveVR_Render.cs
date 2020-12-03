// "WaveVR SDK 
// © 2017 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the WaveVR SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using wvr;
using WVR_Log;
using wvr.render;
using wvr.render.thread;
using System;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;
using AOT;
using System.Collections.Generic;
using UnityEngine.Profiling;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

#if UNITY_2017_2_OR_NEWER
using UnityEngine.XR;
#else
using UnityEngine.VR;
#endif

using wvr.render.gl;

//[RequireComponent(typeof(Camera))]
public class WaveVR_Render : MonoBehaviour
{
	private static readonly string TAG = "WVR_Render";
	private static WaveVR_Render instance = null;
	public static WaveVR_Render Instance {
		get
		{
			return instance;
		}
	}

#if UNITY_STANDALONE
	[SerializeField] private Vector4[] textureBounds = new Vector4[2];

	private WVR_TextureBound_t[] texBounds = null;
#endif

	[Tooltip("Only used on editor.")]
	public float ipd = 0.063f;
	private int targetFPS = -1;
	private static bool surfaceChanged = false;
	private static bool isNeedTimeout = false;
#if UNITY_STANDALONE
	private static bool isGraphicInitialized = true;
#else
	private static bool isGraphicInitialized = false;
#endif
	private static bool isDuringFirstFrame = false;
	private static bool isSetActiveSceneChangedCB = false;
	public bool IsGraphicReady { get { return isGraphicInitialized; } }

	public uint recommendedWidth { get; private set; }
	public uint recommendedHeight { get; private set; }
	public float sceneWidth { get; private set; }
	public float sceneHeight { get; private set; }
	//public float[] projRawL { get; private set; }
	//public float[] projRawR { get; private set; }
	public float[] projRawL = new float[4] { -1, 1, 1, -1 };
	public float[] projRawR = new float[4] { -1, 1, 1, -1 };
	private WaveVR_Utils.RigidTransform[] _eyes = new WaveVR_Utils.RigidTransform[] {
			new WaveVR_Utils.RigidTransform(new Vector3(-0.063f / 2, 0.15f, 0.12f), Quaternion.identity),
			new WaveVR_Utils.RigidTransform(new Vector3(0.063f / 2, 0.15f, 0.12f), Quaternion.identity)
		};
	public WaveVR_Utils.RigidTransform[] eyes { get { return _eyes; } private set { _eyes = value; } }

	[Tooltip("You can trigger a configuration change on editor " +
		"by checking this.  Help to test related delegate.")]
	public bool configurationChanged = false;

	public enum StereoRenderingPath
	{
		MultiPass,
		SinglePass,
		//SinglePassInstanced  // not supported now
		Auto = SinglePass,
		Instancing
	}

	[Tooltip("SinglePass is an experimental feature.  Use it at your own risk.\n\n" +
		"Choose a preferred stereo rendering path setting according to your scene.  " +
		"The actural rendering path will still depend on your project PlayerSettings and VR device.  " +
		"It will fallback to multi-pass if not qualified.  Changing in runtime will take no effect.  " +
		"Default is Auto (SinglePass)."), SerializeField]
	private StereoRenderingPath PreferredStereoRenderingPath = StereoRenderingPath.Auto;
	public StereoRenderingPath acturalStereoRenderingPath {
		get
		{
			return IsSinglePass ? StereoRenderingPath.SinglePass : StereoRenderingPath.MultiPass;
		}
	}

	public bool IsSinglePass { get; private set; }

	#region delegate
	public delegate void RenderCallback(WaveVR_Render render);
	public delegate void RenderCallbackWithEye(WaveVR_Render render, WVR_Eye eye);
	public delegate void RenderCallbackWithEyeAndCamera(WaveVR_Render render, WVR_Eye eye, WaveVR_Camera wvrCamera);

	// Expand will be happened in Start().  Register these delegate in OnEnable().
	public RenderCallback beforeRenderExpand;
	public RenderCallbackWithEye beforeEyeExpand;
	public RenderCallbackWithEyeAndCamera afterEyeExpand;
	public RenderCallback afterRenderExpand;

	// Configuration changed
	public RenderCallback onConfigurationChanged;

	public RenderCallback onSDKGraphicReady;
	public RenderCallback onFirstFrame;

	// Render eye
	public RenderCallbackWithEyeAndCamera beforeRenderEye;
	public RenderCallbackWithEyeAndCamera afterRenderEye;
	#endregion  // delegate

	public class RenderThreadSynchronizer
	{
		RenderTexture mutable = new RenderTexture(1,1,0);
		public RenderThreadSynchronizer()
		{
			mutable.useMipMap = false;
			mutable.Create();
		}

		// May call eglMakeCurrent inside.
		public void sync()
		{
			// It will always get an error internally due to our EGL hacking.  Close the callstack dump for speed.
			var origin = Application.GetStackTraceLogType(LogType.Error);
			Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.None);

			// Sync
			mutable.GetNativeTexturePtr();

			// The libEGL and Unity error will show because WaveVR change the egl surface for necessary.  Every game using WaveVR Unity plugin will have these logs.
			Log.e(TAG, "If the libEGL and Unity errors appeared above, don't panic or report a bug.  They are safe and will not crash your game.");
			Application.SetStackTraceLogType(LogType.Error, origin);
		}
	}
	private RenderThreadSynchronizer synchronizer;

	public T GetComponentFromChildren<T>(string name)
	{
		var children = transform.Find(name);
		if (children != null)
		{
			var component = children.GetComponent<T>();
			return component;
		}
		return default(T);
	}
	const string OBJ_NAME_EYE_CENTER = "Eye Center";
	const string OBJ_NAME_LEFT_EYE = "Eye Left";
	const string OBJ_NAME_RIGHT_EYE = "Eye Right";
	const string OBJ_NAME_BOTH_EYES = "Eye Both";
	const string OBJ_NAME_EAR = "Ear";
	const string OBJ_NAME_DISTORTION = "Distortion";
	const string OBJ_NAME_RETICLE = "Reticle";
	const string OBJ_NAME_LOADING = "Loading";

	// Checked by custom editor
	public bool isExpanded
	{
		get
		{
			if (centerWVRCamera == null)
				centerWVRCamera = GetComponentFromChildren<WaveVR_Camera>(OBJ_NAME_EYE_CENTER);
			if (lefteye == null)
				lefteye = GetComponentFromChildren<WaveVR_Camera>(OBJ_NAME_LEFT_EYE);
			if (righteye == null)
				righteye = GetComponentFromChildren<WaveVR_Camera>(OBJ_NAME_RIGHT_EYE);
			if (botheyes == null)
				botheyes = GetComponentFromChildren<WaveVR_Camera>(OBJ_NAME_BOTH_EYES);
#if UNITY_EDITOR && UNITY_ANDROID
			if (distortion == null)
				distortion = GetComponentFromChildren<WaveVR_Distortion>(OBJ_NAME_DISTORTION);
			if (Application.isEditor)
				return !(centerWVRCamera == null || lefteye == null || righteye == null || distortion == null || botheyes == null);
#endif
			return !(centerWVRCamera == null || lefteye == null || righteye == null || botheyes == null);
		}
	}

	public Camera centerCamera { get { return centerWVRCamera == null ? null : centerWVRCamera.GetCamera(); } }
	public WaveVR_Camera centerWVRCamera = null;
	public WaveVR_Camera lefteye = null;
	public WaveVR_Camera righteye = null;
	public WaveVR_Camera botheyes = null;
	public WaveVR_Distortion distortion = null;
	public GameObject loadingCanvas = null;  // Loading canvas will force clean black to avoid any thing draw on screen before Wave's Graphic's ready.
	public GameObject ear = null;

	[SerializeField]
	[Tooltip("Function of loading canvas will be replaced by GL.Clear(). " +
		"You can still use loading canvas by this check. " +
		"However if using it, you will get a warning from Unity " +
		"about screen space canvas should not be used in VR support.\n" +
		"For LoadingCanvas already expanded/existed, Render will choose to " +
		"apply it unless developer delete it from hierarchy.")]
	private bool useLoadingCanvas = false;
	[SerializeField]
	[Tooltip("The VR render system can not take action while game is loading or resuming. " +
		"For a short period, the VR system is not ready, and a clean color will be applied" +
		"to help clean the snowy screen.\n" +
		"This color will also effect the LoadingCanvas if useLoadingCanvas is checked.")]
	private Color loadingBlockerColor = Color.black;

    private static TextureManager _textureManager = null;
    public TextureManager textureManager { get { return _textureManager; } private set { _textureManager = value; } }

	public static int globalOrigin = -1;
	public static int globalPreferredStereoRenderingPath = -1;

	[HideInInspector]
	public ColorSpace QSColorSpace { get; private set; }
	public WVR_PoseOriginModel _origin = WVR_PoseOriginModel.WVR_PoseOriginModel_OriginOnGround;
	public WVR_PoseOriginModel origin { get { return _origin; } set { _origin = value; OnIpdChanged(null); } }

	public static void InitializeGraphic(RenderThreadSynchronizer synchronizer = null)
	{
		Log.i(TAG, "Color space is " + QualitySettings.activeColorSpace);
		WVR_RenderConfig config = WVR_RenderConfig.WVR_RenderConfig_Default;
		if (QualitySettings.activeColorSpace == ColorSpace.Linear)
			config |= WVR_RenderConfig.WVR_RenderConfig_sRGB;
		var param = new WVR_RenderInitParams_t()
		{
			graphicsApi = WVR_GraphicsApiType.WVR_GraphicsApiType_OpenGL,
			renderConfig = (ulong) config
		};

		RenderThreadContext.IssueRenderEvent(RenderCommandRenderEvent, WaveVR_Utils.RENDEREVENTID_INIT_GRAPHIC, param);
		if (synchronizer != null)
			synchronizer.sync();
	}

	public void OnIpdChanged(params object[] args)
	{
		Log.d(TAG, "OnIpdChanged");

#if UNITY_EDITOR && UNITY_ANDROID
		if (!WaveVR.EnableSimulator) return;
#endif

		WVR_NumDoF dof;
		if (WaveVR.Instance.is6DoFTracking() == 3)
		{
			dof = WVR_NumDoF.WVR_NumDoF_3DoF;
		}
		else
		{
			if (origin == WVR_PoseOriginModel.WVR_PoseOriginModel_OriginOnHead_3DoF)
				dof = WVR_NumDoF.WVR_NumDoF_3DoF;
			else
				dof = WVR_NumDoF.WVR_NumDoF_6DoF;
		}

		//for update EyeToHead transform
		WVR_Matrix4f_t eyeToHeadL = Interop.WVR_GetTransformFromEyeToHead(WVR_Eye.WVR_Eye_Left, dof);
		WVR_Matrix4f_t eyeToHeadR = Interop.WVR_GetTransformFromEyeToHead(WVR_Eye.WVR_Eye_Right, dof);

		eyes = new WaveVR_Utils.RigidTransform[] {
				new WaveVR_Utils.RigidTransform(eyeToHeadL),
				new WaveVR_Utils.RigidTransform(eyeToHeadR)
		};

		ipd = Vector3.Distance(eyes[1].pos, eyes[0].pos);

		//for update projection matrix
		Interop.WVR_GetClippingPlaneBoundary(WVR_Eye.WVR_Eye_Left, ref projRawL[0], ref projRawL[1], ref projRawL[2], ref projRawL[3]);
		Interop.WVR_GetClippingPlaneBoundary(WVR_Eye.WVR_Eye_Right, ref projRawR[0], ref projRawR[1], ref projRawR[2], ref projRawR[3]);

		// Do it again for resume
		UpdateViewports();

		Log.d(TAG, "targetFPS=" + targetFPS + " sceneWidth=" + sceneWidth + " sceneHeight=" + sceneHeight +
			"\nprojRawL[0]=" + projRawL[0] + " projRawL[1]=" + projRawL[1] + " projRawL[2]=" + projRawL[2] + " projRawL[3]=" + projRawL[3] +
			"\nprojRawR[0]=" + projRawR[0] + " projRawR[1]=" + projRawR[1] + " projRawR[2]=" + projRawR[2] + " projRawR[3]=" + projRawR[3] +
			"\neyes[L]=" + eyes[0].pos.x + "," + eyes[0].pos.y + "," + eyes[0].pos.z + 
			"\neyes[R]=" + eyes[1].pos.x + "," + eyes[1].pos.y + "," + eyes[1].pos.z +
			"\nPixelDensity=" + pixelDensity + ", OverDraw=" + overDraw);
		configurationChanged = true;
	}

	public static bool IsVRSinglePassBuildTimeSupported()
	{
#if WAVEVR_SINGLEPASS_ENABLED || (UNITY_EDITOR && UNITY_ANDROID)
		return true;
#else
		return false;
#endif
	}

	private const string WVRSinglePassDeviceName =
#if UNITY_2018_2_OR_NEWER
			"mockhmd";  // cast to lower
#else
			"split";
#endif

	// This check combine runtime check and buildtime check.
	bool checkVRSinglePassSupport()
	{

#if UNITY_EDITOR && UNITY_ANDROID
		if (isMirroDeviceState == true) {
			return false;
		}
#endif

#if UNITY_2017_2_OR_NEWER
		var devices = XRSettings.supportedDevices;
#else
		var devices = VRSettings.supportedDevices;
#endif
		string deviceName = "";
		foreach (var dev in devices)
		{
			var lower = dev.ToLower();
			if (lower.Contains(WVRSinglePassDeviceName))
			{
				deviceName = dev;
				break;
			}
		}

		bool active = false;
		if (!String.IsNullOrEmpty(deviceName))
		{
#if UNITY_2017_2_OR_NEWER
			active = XRSettings.isDeviceActive;
#else
			//VRSettings.LoadDeviceByName(deviceName);
			//VRSettings.enabled = true;
			active = VRSettings.isDeviceActive;
#endif
		}

		int sdkNativeSupport = 0;
#if UNITY_EDITOR && UNITY_ANDROID
		if (Application.isEditor)
			sdkNativeSupport = 1;
		else
#endif
			sdkNativeSupport = WaveVR_Utils.IsSinglePassSupported();

		Log.d(TAG, "sdkNativeSupport = " + sdkNativeSupport);


		bool globalIsMultiPass = false;
		if (globalPreferredStereoRenderingPath > -1)
		{
			// We won't let a scene which doesn't support singlepass to enable singlepass.
			if (PreferredStereoRenderingPath == StereoRenderingPath.SinglePass)
				globalIsMultiPass = globalPreferredStereoRenderingPath == 0;
		}

		bool result;
		if (PreferredStereoRenderingPath != StereoRenderingPath.SinglePass || globalIsMultiPass)
			result = false;
		else
			result = sdkNativeSupport > 0 && active && IsVRSinglePassBuildTimeSupported();

		var msg = "VRSupport: deviceName " + deviceName + ", Graphic support " + sdkNativeSupport +
			", XRSettings.isDeviceActive " + active + ", BuildTimeSupport " + IsVRSinglePassBuildTimeSupported() +
			", preferred " + PreferredStereoRenderingPath + ", global " + globalPreferredStereoRenderingPath +
			", IsSinglePass " + result;
		Log.d(TAG, msg, true);
		return result;
	}

	private void SwitchDeviceView(bool enableSinglepass)
	{
		if (enableSinglepass)
		{
			// This can avoid the centerCamera be rendered

#if UNITY_2018_2_OR_NEWER
			bool showDeviceView = false;
#if UNITY_EDITOR && UNITY_ANDROID
			showDeviceView = true;
#endif
			if (showDeviceView)
				XRSettings.gameViewRenderMode = GameViewRenderMode.BothEyes;
			XRSettings.showDeviceView = showDeviceView;
#elif UNITY_2017_2_OR_NEWER
#else
			UnityEngine.VR.VRSettings.showDeviceView = showDeviceView;
#endif
		}
		else
		{
#if UNITY_2018_2_OR_NEWER
			XRSettings.gameViewRenderMode = GameViewRenderMode.BothEyes;
#elif UNITY_2017_2_OR_NEWER
			XRSettings.showDeviceView = true;
#else
			UnityEngine.VR.VRSettings.showDeviceView = true;
#endif
		}
	}

	private void SwitchKeyword(bool enable)
	{
		if (enable)
		{
			//Enable these keywords to let the unity shaders works for single pass stereo rendering
			Shader.EnableKeyword("STEREO_MULTIVIEW_ON");
			Shader.EnableKeyword("UNITY_SINGLE_PASS_STEREO");
		}
		else
		{
			Shader.DisableKeyword("STEREO_MULTIVIEW_ON");
			Shader.DisableKeyword("UNITY_SINGLE_PASS_STEREO");
		}
	}

	void Awake()
	{
		Log.d(TAG, "Awake()+");
		Log.d(TAG, "Version of the runtime: " + Application.unityVersion);
		if (instance == null)
			instance = this;
		else
			Log.w(TAG, "Render already Awaked");

		QualitySettings.SetQualityLevel(QualitySettings.GetQualityLevel(), true);
		synchronizer = new RenderThreadSynchronizer();

		if (globalOrigin >= 0 && globalOrigin <= 3)
		{
			origin = (WVR_PoseOriginModel) globalOrigin;
			Log.d(TAG, "Has global tracking space " + origin);
		}

		if (WaveVR_Init.Instance == null || !WaveVR.Instance.Initialized)
			Log.e(TAG, "Fail to initialize");


#if UNITY_EDITOR && UNITY_ANDROID
		if (EditorPrefs.GetBool("isMirrorToDevice") == true)
		{
			isMirroDeviceState = true;
		}
		else
		{
			isMirroDeviceState = false;
		}
		if (!WaveVR.EnableSimulator || !WaveVR.Instance.Initialized)
		{
			recommendedWidth = (uint)Mathf.Max(Screen.width / 2, Screen.height);
			recommendedHeight = recommendedWidth;

			sceneWidth = recommendedWidth * pixelDensity;
			sceneHeight = recommendedHeight * pixelDensity;

			//projRawL = new float[4] { -1, 1, 1, -1 };
			//projRawR = new float[4] { -1, 1, 1, -1 };

			UpdateViewports();

			IsSinglePass = checkVRSinglePassSupport();
		}
		else
#endif
		{
			IsSinglePass = checkVRSinglePassSupport();

			// This command can make sure native's render code are initialized in render thread.
			// InitializeGraphic(synchronizer);

			// Setup render values
			uint w = 0, h = 0;
			Interop.WVR_GetRenderTargetSize(ref w, ref h);
			recommendedWidth = w;
			recommendedHeight = h;

			UpdateViewports();

			// Only do array initialization.  The OnIpdChanged will get correct value.
			//projRawL = new float[4] { -1, 1, 1, -1 };
			//projRawR = new float[4] { -1, 1, 1, -1 };

			WVR_RenderProps_t props = new WVR_RenderProps_t();
			Interop.WVR_GetRenderProps(ref props);
			targetFPS = (int)props.refreshRate;

			OnIpdChanged(null);
		}

		Log.d(TAG, "Actural StereoRenderingPath is " + acturalStereoRenderingPath);

		WaveVR_Utils.IssueEngineEvent(WaveVR_Utils.EngineEventID.HMD_INITIAILZED);

		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		Application.targetFrameRate = targetFPS;
		Log.d(TAG, "Awake()-");
	}

	private Coroutine renderLooperCoroutine = null;
	private void enableRenderLoop(bool start)
	{
#if UNITY_STANDALONE
		return;
#else
		if (start && enabled)
		{
			if (renderLooperCoroutine != null)
				return;
			var renderLoop = RenderLoop();
			renderLooperCoroutine = StartCoroutine(renderLoop);
		}
		else
		{
			if (renderLooperCoroutine != null)
				StopCoroutine(renderLooperCoroutine);
			renderLooperCoroutine = null;
		}
#endif
	}

	void OnEnable()
	{
		Log.d(TAG, "OnEnable()+");
		WaveVR_Utils.Event.Listen (WaveVR_Utils.Event.IPD_CHANGED, OnIpdChanged);
		enableRenderLoop(true);
		setLoadingCanvas(true);
		WaveVR_Utils.IssueEngineEvent(WaveVR_Utils.EngineEventID.UNITY_ENABLE);
		if (!isSetActiveSceneChangedCB)
		{
			Log.d(TAG, "Added scene loaded callback");
			SceneManager.sceneLoaded += OnSceneLoaded;
			isSetActiveSceneChangedCB = true;
		}
		Log.d(TAG, "OnEnable()-");
	}

	void Start()
	{
		Log.d(TAG, "Start()+");

		WaveVR_Render.Expand(this);

		// Not to modify developer's design.
		if (Camera.main == null)
			centerCamera.tag = "MainCamera";

		// if you need the Camera.main workable you can enable the centerCamera when OnConfigurationChanged 
		centerCamera.enabled = false;

		// these camera will be enabled in RenderLoop
#if UNITY_STANDALONE
		botheyes.GetCamera().enabled = true;
#else
		botheyes.GetCamera().enabled = false;
#endif
		lefteye.GetCamera().enabled = false;
		righteye.GetCamera().enabled = false;

#if UNITY_STANDALONE

		InitTextureBound();

		XRSettings.renderViewportScale = 1;
		XRSettings.eyeTextureResolutionScale = 1.0f;
#endif


		Log.d(TAG, "onConfigurationChanged+");
		WaveVR_Utils.Event.Send(WaveVR_Utils.Event.RENDER_CONFIGURATION_CHANGED);
		WaveVR_Utils.SafeExecuteAllDelegate<RenderCallback>(onConfigurationChanged, a => a(this));
		configurationChanged = false;
		Log.d(TAG, "onConfigurationChanged-");
		Log.d(TAG, "Start()-");
	}

	static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		Log.d(TAG, "OnSceneLoaded Scene name: " + scene.name + ", mode: " + mode);

#if UNITY_EDITOR && UNITY_ANDROID
		if (WaveVR.EnableSimulator)
		{
			Log.d(TAG, "OnSceneLoaded() call WVR_PostInit()");
			wvr.Interop.WVR_PostInit();
		}
		if (Application.isPlaying)
			return;
#endif

		try
		{
			Log.d(TAG, "OnSceneLoaded Set TrackingSpaceOrigin: " + WaveVR_Render.instance.origin);
			Log.d(TAG, "OnSceneLoaded WVR_GetDegreeOfFreedom(HMD): " + Interop.WVR_GetDegreeOfFreedom(WVR_DeviceType.WVR_DeviceType_HMD));
			Log.d(TAG, "OnSceneLoaded HMD Pose DOF: " + (WaveVR.Instance.hmd.pose.pose.Is6DoFPose ? "6DoF" : "3DoF"));
			Log.d(TAG, "OnSceneLoaded Left-hand mode: " + WaveVR_Controller.IsLeftHanded);
			WaveVR.Device rightController = WaveVR.Instance.getDeviceByType(WVR_DeviceType.WVR_DeviceType_Controller_Right);
			if (rightController != null && rightController.connected)
			{
				Log.d(TAG, "OnSceneLoaded WVR_GetDegreeOfFreedom(Controller_Right): " + Interop.WVR_GetDegreeOfFreedom(WVR_DeviceType.WVR_DeviceType_Controller_Right));
				Log.d(TAG, "OnSceneLoaded Right Controller Pose DOF: " + (rightController.pose.pose.Is6DoFPose ? "6DoF" : "3DoF"));
			}
			WaveVR.Device leftController = WaveVR.Instance.getDeviceByType(WVR_DeviceType.WVR_DeviceType_Controller_Left);
			if (rightController != null && leftController.connected)
			{
				Log.d(TAG, "OnSceneLoaded WVR_GetDegreeOfFreedom(Controller_Left): " + Interop.WVR_GetDegreeOfFreedom(WVR_DeviceType.WVR_DeviceType_Controller_Left));
				Log.d(TAG, "OnSceneLoaded Left Controller Pose DOF: " + (leftController.pose.pose.Is6DoFPose ? "6DoF" : "3DoF"));
			}
			if (WaveVR_InputModuleManager.Instance != null)
			{
				Log.d(TAG, "OnSceneLoaded enable Input module: " + WaveVR_InputModuleManager.Instance.EnableInputModule + ", Interaction mode: " + WaveVR_InputModuleManager.Instance.GetInteractionMode());
				Log.d(TAG, "OnSceneLoaded override system settings: " + WaveVR_InputModuleManager.Instance.OverrideSystemSettings + ", custom input module: " + WaveVR_InputModuleManager.Instance.CustomInputModule);
				Log.d(TAG, "OnSceneLoaded TimeToGaze: " + WaveVR_InputModuleManager.Instance.Gaze.TimeToGaze + ", Gaze trigger type: " + WaveVR_InputModuleManager.Instance.GetUserGazeTriggerType());
				Log.d(TAG, "OnSceneLoaded Controller Raycast Mode: " + WaveVR_InputModuleManager.Instance.Controller.RaycastMode);
			}
		} catch (Exception e) {
			Log.e(TAG, "Error during OnSceneLoaded\n" + e.ToString());
		}
	}

	public static void signalSurfaceState(string msg) {
		Log.d(TAG, "signalSurfaceState[ " + msg + " ]");
		if (String.Equals(msg, "CHANGED")) {
			surfaceChanged = false;
		} else if (String.Equals(msg, "CHANGED_WRONG")) {
			surfaceChanged = false;
			isNeedTimeout = true;
		} else if (String.Equals(msg, "CHANGED_RIGHT")) {
			surfaceChanged = true;
		} else if (String.Equals(msg, "DESTROYED")) {
			surfaceChanged = false;
			Log.d(TAG, "surfaceDestroyed");
		}
	}

	private static bool checkSurfaceChanged()
	{
		bool tmp = false;
		try
		{
			AndroidJavaClass jc = new AndroidJavaClass("com.htc.vr.unity.WVRUnityVRActivity");
			tmp = jc.GetStatic<bool>("mSurfaceChanged");
		}
		catch (Exception e)
		{
			WVR_Log.Log.e(TAG, e.Message, true);
		}
		return tmp;
	}

	void OnApplicationPause(bool pauseStatus)
	{
		Log.d(TAG, "Pause(" + pauseStatus + ")");

		if (pauseStatus)
		{
			WaveVR_Utils.IssueEngineEvent(WaveVR_Utils.EngineEventID.UNITY_APPLICATION_PAUSE);
			if (synchronizer != null)
				synchronizer.sync();
			if (lefteye != null)
				lefteye.GetCamera().targetTexture = null;
			if (righteye != null)
				righteye.GetCamera().targetTexture = null;
			if (botheyes != null)
				botheyes.GetCamera().targetTexture = null;
			if (textureManager != null)
				textureManager.ReleaseTexturePools();
		}
		else
		{
			WaveVR_Utils.IssueEngineEvent(WaveVR_Utils.EngineEventID.UNITY_APPLICATION_RESUME);
		}

#if !UNITY_STANDALONE
		if (IsSinglePass)
			// Let the loading canvas can draw on display
			SwitchDeviceView(false);
#endif

		setLoadingCanvas(true);
		enableRenderLoop(!pauseStatus);
	}

	public int SetQualityLevel(int level, bool applyExpensiveChanges = true)
	{
		if (level < 0) return -1;
		string[] names = QualitySettings.names;
		if (level >= names.Length) return -1;
		int qualityLevel = QualitySettings.GetQualityLevel();
		if (qualityLevel != level)
		{
			QualitySettings.SetQualityLevel(level, false);
			if (applyExpensiveChanges)
			{
				Scene s = SceneManager.GetActiveScene();
				SceneManager.LoadScene(s.name);
			}
			qualityLevel = QualitySettings.GetQualityLevel();
		}
		return qualityLevel;
	}

	void LateUpdate()
	{
		Log.gpl.check();
	}

	void OnApplicationQuit()
	{
		WaveVR_Utils.IssueEngineEvent(WaveVR_Utils.EngineEventID.UNITY_APPLICATION_QUIT);
		if (synchronizer != null)
			synchronizer.sync();
		WaveVR.SafeDispose();
	}

	void OnDisable()
	{
		using (var ee = Log.ee(TAG, "OnDisable()+", "OnDisable()-"))
		{
			enableRenderLoop(false);
#if UNITY_EDITOR && UNITY_ANDROID
			if (!Application.isEditor)
#endif
			{
				WaveVR_Utils.IssueEngineEvent(WaveVR_Utils.EngineEventID.UNITY_DISABLE);
				if (synchronizer != null)
					synchronizer.sync();
			}
			WaveVR_Utils.Event.Remove (WaveVR_Utils.Event.IPD_CHANGED, OnIpdChanged);
			setLoadingCanvas(false);

			if (lefteye != null)
				lefteye.GetCamera().targetTexture = null;
			if (righteye != null)
				righteye.GetCamera().targetTexture = null;
			if (botheyes != null)
				botheyes.GetCamera().targetTexture = null;
			// TODO If the scene didn't have WaveVR_Render any more, memory is occupied.
			//if (textureManager != null)
			//	textureManager.ReleaseTexturePools();

			if (isSetActiveSceneChangedCB)
			{
				Log.d(TAG, "Removed scene loaded callback");
				SceneManager.sceneLoaded -= OnSceneLoaded;
				isSetActiveSceneChangedCB = false;
			}
		}
	}

	void OnDestroy()
	{
		using (var ee = Log.ee(TAG, "OnDestroy()+", "OnDestroy()-"))
		{
			//textureManager = null;
			instance = null;
			WaveVR_Utils.IssueEngineEvent(WaveVR_Utils.EngineEventID.UNITY_DESTROY);
		}
	}

	private WaitForEndOfFrame cachedWaitForEndOfFrame;

	private IEnumerator RenderLoop()
	{
		Log.d(TAG, "RenderLoop() is started");
		if (cachedWaitForEndOfFrame == null)
			cachedWaitForEndOfFrame = new WaitForEndOfFrame();
		yield return cachedWaitForEndOfFrame;
		yield return cachedWaitForEndOfFrame;

		if (isGraphicInitialized == false) {
			InitializeGraphic(synchronizer);
			isGraphicInitialized = true;
		}

		while (!isExpanded)
			yield return cachedWaitForEndOfFrame;

        bool allowMSAA = false;
        if (IsSinglePass)
            allowMSAA = botheyes.GetCamera().allowMSAA;
        else
            allowMSAA = lefteye.GetCamera().allowMSAA && lefteye.GetCamera().allowMSAA;
		textureManager = new TextureManager(textureManager, IsSinglePass, allowMSAA, pixelDensity);

		WaveVR_Utils.SafeExecuteAllDelegate<RenderCallback>(onSDKGraphicReady, a => a(this));

#if UNITY_EDITOR && UNITY_ANDROID
		if (!Application.isEditor)
#endif
		{
			// Time Control
			var tim = Time.realtimeSinceStartup;

			// Restart ATW thread before rendering.
			while (!WaveVR_Utils.WVR_IsATWActive()) {
				yield return cachedWaitForEndOfFrame;
				if (surfaceChanged && isNeedTimeout == false)
					break;
				if (checkSurfaceChanged() && isNeedTimeout == false)
					break;
				if (Time.realtimeSinceStartup - tim > 1.0f)
				{
					Log.w(TAG, "Waiting for surface change is timeout.");
					break;
				}
			}
			// Reset isNeedTimeout flag
			isNeedTimeout = false;

			if (textureManager != null)
			{
				if (!textureManager.validate())
					textureManager.reset();
			}
		}

		setLoadingCanvas(false);
		if (IsSinglePass)
			SwitchDeviceView(IsSinglePass);

		Log.d(TAG, "RenderLoop() is running");

		Log.d(TAG, "First frame");
		isDuringFirstFrame = true;
		WaveVR_Utils.IssueEngineEvent(WaveVR_Utils.EngineEventID.FIRST_FRAME);
		WaveVR_Utils.SafeExecuteAllDelegate<RenderCallback>(onFirstFrame, a => a(this));
		RenderCycle();
		yield return cachedWaitForEndOfFrame;
		isDuringFirstFrame = false;

		while (true)
		{
			Log.gpl.d(TAG, "RenderLoop() is still running");

			//Profiler.BeginSample("Synchronizer");
			//synchronizer.sync();
			//configurationChanged = true;
			//Profiler.EndSample();

			RenderCycle();

			yield return cachedWaitForEndOfFrame;
		}
	}

	private void RenderCycle()
	{
		WaveVR_Utils.Trace.BeginSection("RenderLoop", false);

		Profiler.BeginSample("UpdateEachFrame");
		WaveVR.Instance.UpdateEachFrame(origin);
		Profiler.EndSample();

		// Set next texture before running any graphic command.
		if (textureManager != null)
			textureManager.Next();

		if (configurationChanged)
		{
			WaveVR_Render.Expand(this);
			Log.d(TAG, "onConfigurationChanged+");
			WaveVR_Utils.Event.Send(WaveVR_Utils.Event.RENDER_CONFIGURATION_CHANGED);
			WaveVR_Utils.SafeExecuteAllDelegate<RenderCallback>(onConfigurationChanged, a => a(this));
			configurationChanged = false;
			Log.d(TAG, "onConfigurationChanged-");
		}

		Profiler.BeginSample("RenderEye");
		if (IsSinglePass)
		{
			RenderEyeBoth(botheyes);
		}
		else
		{
			botheyes.GetCamera().enabled = false;
			RenderEye(lefteye, WVR_Eye.WVR_Eye_Left);
			RenderEye(righteye, WVR_Eye.WVR_Eye_Right);

#if UNITY_STANDALONE
			UpdateGameView(false);
#endif
#if UNITY_EDITOR && UNITY_ANDROID
			// Because the latest unity will mess up the render order if submit after each eye is rendered.
			// Move the distortion here to have better framedebug resoult.
			if (Application.isEditor)
			{
				distortion.RenderEye(WVR_Eye.WVR_Eye_Left, textureManager.left.currentRt);
				distortion.RenderEye(WVR_Eye.WVR_Eye_Right, textureManager.right.currentRt);
			}
#endif
		}
		Profiler.EndSample();
		WaveVR_Utils.Trace.EndSection(false);

		// Put here to control the time of next frame.
		TimeControl();

		Log.gpl.d(TAG, "End of frame");
	}

#if UNITY_STANDALONE
	private Material linearToGammaMat = null;
	public void UpdateGameView(bool linearToGamma = true)
	{
		// Reset screen space.
		GL.PushMatrix();
		GL.LoadPixelMatrix(0, Screen.width, 0, Screen.height);

		// Workaround for Unity render textures draw in linear space. 
		if (linearToGamma)
		{
			if (linearToGammaMat == null)
			{
				linearToGammaMat = new Material(Shader.Find("Hidden/GUITexture_LinearToGamma"));
			}

			Graphics.DrawTexture(new Rect(Screen.width / 2, Screen.height, Screen.width / 2, -Screen.height), textureManager.left.currentRt, linearToGammaMat);
			Graphics.DrawTexture(new Rect(0.0f, Screen.height, Screen.width / 2, -Screen.height), textureManager.right.currentRt, linearToGammaMat);
		}
		else
		{
			Graphics.DrawTexture(new Rect(Screen.width / 2, Screen.height, Screen.width / 2, -Screen.height), textureManager.left.currentRt);
			Graphics.DrawTexture(new Rect(0.0f, Screen.height, Screen.width / 2, -Screen.height), textureManager.right.currentRt);
		}

		GL.PopMatrix();
	}
#endif

	private void GetSubmitTextureParams(WVR_Eye eye, IntPtr texture, [Out] WVR_TextureParams_t []textureParams, ref WVR_SubmitExtend flag)
	{
		if (isPartialTexture)
			flag = WVR_SubmitExtend.WVR_SubmitExtend_PartialTexture;
		else
			flag = WVR_SubmitExtend.WVR_SubmitExtend_Default;

		if (textureParams == null)
			return;

		textureParams[0].id = texture;
		textureParams[0].target = eye == WVR_Eye.WVR_Eye_Both ? WVR_TextureTarget.WVR_TextureTarget_2D_ARRAY : WVR_TextureTarget.WVR_TextureTarget_2D;

		textureParams[0].layout.leftLowUVs.v0 = drawViewport.xMin;
		textureParams[0].layout.leftLowUVs.v1 = drawViewport.yMin;
		textureParams[0].layout.rightUpUVs.v0 = drawViewport.xMax;
		textureParams[0].layout.rightUpUVs.v1 = drawViewport.yMax;
	}

#region adjustable_resolution
	private bool isPartialTexture = false;

	[SerializeField]
	[Range(0.1f, 2)]
	[Tooltip("The value is a scale to the recommended RenderTarget width and height. " +
		"It will change the render texture size.  " +
		"Use a smaller value to improve performance. " +
		"Or use a larger value to improve quality.")]
	private float pixelDensity = 1.0f;
	public float PixelDensity { get { return pixelDensity; } }

	//[SerializeField]
	[Range(1.0f, 2.0f)]
	[Tooltip("Let the rendering FOV larger than original size," +
		" but only show the original size.  Help to reduce the showing " +
		"of black peripheral area when ATW triggered.  " +
		"It will scale up the render texture size.  Therefore," +
		"the pixel quality is keeped.\n" +
		"WARNING: This setting will impact the performance.")]
	private float overDraw = 1.0f;
	public float OverDraw { get { return overDraw; } }

	private float resolutionScale = 1.0f;
	public float ResolutionScale { get { return resolutionScale; } }

	private readonly Rect fullViewport = Rect.MinMaxRect(0, 0, 1, 1);
	private Rect drawViewport = Rect.MinMaxRect(0, 0, 1, 1);

	private void UpdateViewports()
	{
		overDraw = Mathf.Clamp(overDraw, 1.0f, 2.0f);
		//Overfill can only set once before RenderInit.
		//Interop.WVR_SetOverfillRatio(overDraw, overDraw);

		pixelDensity = Mathf.Clamp(pixelDensity, 0.1f, 2.0f);
		sceneWidth = recommendedWidth * pixelDensity;
		sceneHeight = recommendedHeight * pixelDensity;

		if (textureManager != null)
			textureManager.Resize(resolutionScale);

		// The PartialTexture method is abandond now.
#if false
		// Let the margin larger than 1 pixel.
		float epsilon = 2 / (float)Mathf.Min(recommendedWidth, recommendedHeight);

		float margin = Mathf.Abs(1 - resolutionScale) / 2;
		if (margin > epsilon)
		{
			drawViewport = Rect.MinMaxRect(margin, margin, 1 - margin, 1 - margin);
			isPartialTexture = true;
		}
		else
#endif
		{
			drawViewport = fullViewport;
			isPartialTexture = false;
		}
	}

	/**
	 * Resolution Scale is use to dynamic resize the render size.
	 * This operation will cost about 1.5 msec to create one new texture.
	 * And double the cost at multiview.
	 * 
	 * XXX In Focus device, process may be killed by driver for the error, 
	 * "Resource deadlock would occur" when SetResolutionScale is invoked.
	 * However This feature has been tested on the latest Unity version or
	 * the other device.  And the same problem has not been reproduced. We
	 * thought it was Unity2017 & GPU Adreno's problem.
	**/
	public void SetResolutionScale(float scale)
	{
#if !UNITY_2018_1_OR_NEWER
		bool newVersion = false;
		if (!newVersion) return;
#endif
		resolutionScale = Mathf.Clamp(scale, 0.1f, 1.0f);
		UpdateViewports();
	}
#endregion  // adjustable_resolution

#region render
	private WVR_TextureParams_t[] textureParams = new WVR_TextureParams_t[2] {
		new WVR_TextureParams_t
		{
			layout = new WVR_TextureLayout_t
			{
				leftLowUVs = new WVR_Vector2f_t() { v0 = 0, v1 = 0 },
				rightUpUVs = new WVR_Vector2f_t() { v0 = 1, v1 = 1 }
			}
		},
		new WVR_TextureParams_t
		{
			layout = new WVR_TextureLayout_t
			{
				leftLowUVs = new WVR_Vector2f_t() { v0 = 0, v1 = 0 },
				rightUpUVs = new WVR_Vector2f_t() { v0 = 1, v1 = 1 }
			}
		},
	};

	private void RenderEyeBoth(WaveVR_Camera wvrCamera)
	{
		var camera = wvrCamera.GetCamera();
		var rt = textureManager.both.currentRt;

		var eye = WVR_Eye.WVR_Eye_Both;

#if UNITY_EDITOR && UNITY_ANDROID
		if (Application.isEditor)
		{
			// It was disabled in Start()
			camera.enabled = true;
			//camera.rect = drawViewport;  // Not to use it when preview
			SafeExecuteRenderEyeCallback(beforeRenderEye, eye, wvrCamera);
			SafeExecuteRenderEyeCallback(afterRenderEye, eye, wvrCamera);
			return;
		}
#endif

		WaveVR_Utils.Trace.BeginSection("Render_WVR_Eye_Both");
		Log.gpl.d(TAG, "Render_WVR_Eye_Both");
		if (isDuringFirstFrame)
			Log.d(TAG, "FirstFrame : " + eye);

		WVR_SubmitExtend flag = 0;
		GetSubmitTextureParams(eye, (IntPtr)textureManager.both.currentPtr, textureParams, ref flag);

		// PreRenderEye
		if (isDuringFirstFrame)
			Log.d(TAG, "FirstFrame : PreRenderEye +++");
		RenderThreadContext.IssueBefore(RenderCommandBeforeEye[(int)eye], eye, 1, textureParams[0], textureParams[1], 2, foveationParams[0], foveationParams[1]);
		if (isDuringFirstFrame)
			Log.d(TAG, "FirstFrame : PreRenderEye ---");

		camera.enabled = true;

		camera.targetTexture = rt;
		camera.forceIntoRenderTexture = true;
		camera.rect = drawViewport;
		//camera.cullingMatrix = MakeProjection(-1f, 1f, 1f, -1f, 0.001f, 1000, true) * camera.worldToCameraMatrix;
		SwitchKeyword(true);

		if (isDuringFirstFrame)
			Log.d(TAG, "FirstFrame : beforeRenderEye Callback +++");
		SafeExecuteRenderEyeCallback(beforeRenderEye, eye, wvrCamera);
		if (isDuringFirstFrame)
			Log.d(TAG, "FirstFrame : beforeRenderEye Callback ---");
		camera.Render();
		if (isDuringFirstFrame)
			Log.d(TAG, "FirstFrame : afterRenderEye Callback +++");
		SafeExecuteRenderEyeCallback(afterRenderEye, eye, wvrCamera);
		if (isDuringFirstFrame)
			Log.d(TAG, "FirstFrame : afterRenderEye Callback ---");

		SwitchKeyword(false);
		//camera.ResetCullingMatrix();
		camera.enabled = false;

		// Just discard it from GPU cache.  The texture is still usable.
		rt.DiscardContents(false, true);

		// SubmitFrame
		if (isDuringFirstFrame)
			Log.d(TAG, "FirstFrame : SubmitFrame +++");
		RenderThreadContext.IssueAfter(RenderCommandAfterEye[(int)eye], eye, 1, textureParams[0], textureParams[1], poseUsedOnSubmit.IsValidPose ? 1 : 0, poseUsedOnSubmit, flag | submitExtendFlag);
		if (isDuringFirstFrame)
			Log.d(TAG, "FirstFrame : SubmitFrame ---");

		WaveVR_Utils.Trace.EndSection();
	}

	private void RenderEye(WaveVR_Camera wvrCamera, WVR_Eye eye)
	{
		var camera = wvrCamera.GetCamera();
		WaveVR_Utils.Trace.BeginSection((eye == WVR_Eye.WVR_Eye_Left) ? "Render_WVR_Eye_Left" : "Render_WVR_Eye_Right");
		Log.gpl.d(TAG, (eye == WVR_Eye.WVR_Eye_Left) ? "Render_WVR_Eye_Left" : "Render_WVR_Eye_Right");
		if (isDuringFirstFrame)
			Log.d(TAG, "FirstFrame : " + eye);

		bool isleft = eye == WVR_Eye.WVR_Eye_Left;
		RenderTexture rt = textureManager.GetRenderTextureLR(isleft);

		WVR_SubmitExtend flag = 0;
		GetSubmitTextureParams(eye, (IntPtr)textureManager.GetNativePtrLR(isleft), textureParams, ref flag);

		var foveationParamEye = isleft ? foveationParams[0] : foveationParams[1];
		// Yes, we need two the same foveation param
		// PreRenderEye
		if (isDuringFirstFrame)
			Log.d(TAG, "FirstFrame : PreRenderEye +++");
		RenderThreadContext.IssueBefore(RenderCommandBeforeEye[(int)eye], eye, 1, textureParams[0], textureParams[1], 2, foveationParamEye, foveationParamEye);
		if (isDuringFirstFrame)
			Log.d(TAG, "FirstFrame : PreRenderEye ---");

		camera.enabled = true;
		camera.targetTexture = rt;
		camera.rect = drawViewport;

		if (isDuringFirstFrame)
			Log.d(TAG, "FirstFrame : beforeRenderEye Callback +++");
		SafeExecuteRenderEyeCallback(beforeRenderEye, eye, wvrCamera);
		if (isDuringFirstFrame)
			Log.d(TAG, "FirstFrame : beforeRenderEye Callback ---");
		camera.Render();
		if (isDuringFirstFrame)
			Log.d(TAG, "FirstFrame : afterRenderEye Callback +++");
		SafeExecuteRenderEyeCallback(afterRenderEye, eye, wvrCamera);
		if (isDuringFirstFrame)
			Log.d(TAG, "FirstFrame : afterRenderEye Callback ---");

		camera.rect = Rect.MinMaxRect(0, 0, 1, 1);
		camera.enabled = false;

		// Just discard it from GPU cache.  The texture is still usable.
		rt.DiscardContents(false, true);

		// SubmitFrame
		if (isDuringFirstFrame)
			Log.d(TAG, "FirstFrame : SubmitFrame +++");
		RenderThreadContext.IssueAfter(RenderCommandAfterEye[(int)eye], eye, 1, textureParams[0], textureParams[1], poseUsedOnSubmit.IsValidPose ? 1 : 0, poseUsedOnSubmit, flag | submitExtendFlag);
		if (isDuringFirstFrame)
			Log.d(TAG, "FirstFrame : SubmitFrame ---");

		WaveVR_Utils.Trace.EndSection();
	}

#endregion  // render

#region expand
	private static void AddRaycaster(GameObject obj)
	{
		PhysicsRaycaster ray = obj.GetComponent<PhysicsRaycaster>();
		if (ray == null)
			ray = obj.AddComponent<PhysicsRaycaster>();
		LayerMask mask = -1;
		mask.value = LayerMask.GetMask("Default", "TransparentFX", "Water");
		ray.eventMask = mask;
	}

	private WaveVR_Camera CreateCenterCamera()
	{
		Log.d(TAG, "CreateEye(None)+");
		if (beforeEyeExpand != null)
		{
			WaveVR_Utils.SafeExecuteAllDelegate<RenderCallbackWithEye>(beforeEyeExpand, a => a(this, WVR_Eye.WVR_Eye_None));
			Log.d(TAG, "CreateEye(None)+custom");
		}

		WaveVR_Camera vrcamera = centerWVRCamera;
		Camera camera;

		// If WaveVR_Render attached to an camera, recreate a copy to center camera game object.
		if (vrcamera == null)
		{
			var obj = new GameObject(OBJ_NAME_EYE_CENTER, typeof(Camera), typeof(WaveVR_Camera));
			obj.transform.SetParent(transform, false);
			obj.transform.localPosition = (eyes[0].pos + eyes[1].pos) / 2.0f;

			camera = obj.GetComponent<Camera>();
			vrcamera = obj.GetComponent<WaveVR_Camera>();

			Camera attachedCamera = GetComponent<Camera>();
			if (attachedCamera != null)
			{
				camera.CopyFrom(attachedCamera);
				attachedCamera.enabled = false;
			}
			else
			{
				// The stereoTargetEye will modify fov.  Disable it first
				camera.stereoConvergence = 0;
				camera.stereoTargetEye = StereoTargetEyeMask.None;

				camera.nearClipPlane = 0.01f;
				camera.farClipPlane = 1000f;
				camera.renderingPath = RenderingPath.Forward;
				camera.allowMSAA = false;
				camera.fieldOfView = 100;
			}
		}
		camera = vrcamera.GetCamera();

		camera.allowHDR = false;
#if UNITY_2017_3_OR_NEWER
		camera.allowDynamicResolution = false;
#endif
		camera.stereoConvergence = 0;
		camera.stereoTargetEye = StereoTargetEyeMask.None;

		// The stereo settings will reset the localPosition. That is an Unity's bug.  Set the pos after stereo settings.
		vrcamera.transform.localPosition = (eyes[0].pos + eyes[1].pos) / 2.0f;

#if UNITY_EDITOR && UNITY_ANDROID
		// After main center camera is ready, use it's fov to set projection raw.
		projRawL = projRawR = GetEditorProjectionRaw(camera.fieldOfView, sceneWidth, sceneHeight);
#endif

		if (afterEyeExpand != null)
		{
			Log.d(TAG, "CreateEye(None)-custom");
			WaveVR_Utils.SafeExecuteAllDelegate<RenderCallbackWithEyeAndCamera>(afterEyeExpand, a => a(this, WVR_Eye.WVR_Eye_None, vrcamera));
		}
		Log.d(TAG, "CreateEye(None)-");
		return vrcamera;
	}

	private WaveVR_Camera CreateEyeBoth()
	{
		Log.d(TAG, "CreateEye(Both)+");
		if (beforeEyeExpand != null)
		{
			WaveVR_Utils.SafeExecuteAllDelegate<RenderCallbackWithEye>(beforeEyeExpand, a => a(this, WVR_Eye.WVR_Eye_Both));
			Log.d(TAG, "CreateEye(Both)+custom");
		}

		Camera camera;
		var vrcamera = botheyes;
		if (vrcamera == null)
		{
			GameObject go = new GameObject(OBJ_NAME_BOTH_EYES, typeof(Camera), typeof(FlareLayer), typeof(WaveVR_Camera));
			go.transform.SetParent(transform, false);
			camera = go.GetComponent<Camera>();
			camera.CopyFrom(centerCamera);

			vrcamera = go.GetComponent<WaveVR_Camera>();

#if UNITY_2017_1_OR_NEWER
            if (false) {
#endif
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0162 // Unreachable code detected
#if !UNITY_2019_3_OR_NEWER
				vrcamera.gameObject.AddComponent<GUILayer>();
#endif
        }
#pragma warning restore CS0162 // Unreachable code detected
#pragma warning restore CS0618 // Type or member is obsolete
		}
		else
		{
			camera = vrcamera.GetCamera();
		}

		vrcamera.eye = WVR_Eye.WVR_Eye_Both;

		if (Camera.main == null)
			vrcamera.tag = "MainCamera";
		else if (Camera.main == centerCamera)
		{
			centerCamera.tag = "Untagged";
			vrcamera.tag = "MainCamera";
		}

		//// When render to texture, the rect is no function.  We still reset it to full size.
		//camera.rect = new Rect(0, 0, 1, 1);

		// Settings here doesn't matter the result.  Just set it.
		camera.stereoTargetEye = StereoTargetEyeMask.Both;
		camera.stereoSeparation = ipd;
		camera.stereoConvergence = 0;  // Not support convergence because the projection is created by SDK

		// We don't create HDR rendertexture.  And we may have our own 'dynamic resolution' resolution.
		camera.allowHDR = false;
#if UNITY_2017_3_OR_NEWER
		camera.allowDynamicResolution = false;
#endif
		Matrix4x4 projL, projR, projCull;
		projL = GetProjection(projRawL, camera.nearClipPlane, camera.farClipPlane);
		projR = GetProjection(projRawR, camera.nearClipPlane, camera.farClipPlane);

		// In some device the head center is not at the eye center.
		var l = eyes[0].pos;
		var r = eyes[1].pos;
		var center = (l + r) / 2.0f;
		vrcamera.transform.localPosition = center;
		vrcamera.transform.localRotation = Quaternion.identity;

		// Because the BothEye camera is already at center pos, the eyes pos should not have y, and z parts.
		vrcamera.SetEyesPosition(l - center, r - center);

		projCull = MakeCullingProjectionMatrix(projRawL, projRawR, camera.nearClipPlane, camera.farClipPlane, l, r);

		vrcamera.SetStereoProjectionMatrix(projL, projR, projCull);

		if (afterEyeExpand != null)
		{
			Log.d(TAG, "CreateEye(Both)-custom");
			WaveVR_Utils.SafeExecuteAllDelegate<RenderCallbackWithEyeAndCamera>(afterEyeExpand, a => a(this, WVR_Eye.WVR_Eye_Both, vrcamera));
		}
		Log.d(TAG, "CreateEye(Both)-");
		return vrcamera;
	}

	private WaveVR_Camera CreateEye(WVR_Eye eye)
	{
		Log.d(TAG, "CreateEye(" + eye + ")+");
		if (beforeEyeExpand != null)
		{
			WaveVR_Utils.SafeExecuteAllDelegate<RenderCallbackWithEye>(beforeEyeExpand, a => a(this, eye));
			Log.d(TAG, "CreateEye(" + eye + ")+custom");
		}

		bool isleft = eye == WVR_Eye.WVR_Eye_Left;
		WaveVR_Camera vrcamera = isleft ? lefteye : righteye;
		Camera camera;
		if (vrcamera == null)
		{
			string eyename = isleft ? OBJ_NAME_LEFT_EYE : OBJ_NAME_RIGHT_EYE;
			GameObject go = new GameObject(eyename, typeof(Camera), typeof(FlareLayer), typeof(WaveVR_Camera));
			go.transform.SetParent(transform, false);
			camera = go.GetComponent<Camera>();
			camera.CopyFrom(centerCamera);

#if !UNITY_2017_1_OR_NEWER
			go.AddComponent<GUILayer>();
#endif
			vrcamera = go.GetComponent<WaveVR_Camera>();
		}
		else
		{
			camera = vrcamera.GetComponent<Camera>();
		}

		vrcamera.eye = eye;
		// Settings here doesn't matter the result.  Just set it.
		camera.stereoTargetEye = StereoTargetEyeMask.None;
		camera.stereoSeparation = ipd;
		camera.stereoConvergence = 0;  // Not support convergence because the projection is created by SDK

		camera.enabled = false;

		camera.transform.localPosition = eyes[isleft ? 0 : 1].pos;

		// We don't create HDR rendertexture.  And we may have our own 'dynamic resolution' resolution.
		camera.allowHDR = false;
#if UNITY_2017_3_OR_NEWER
		camera.allowDynamicResolution = false;
#endif

		var projRaw = isleft ? projRawL : projRawR;
		camera.projectionMatrix = GetProjection(projRaw, camera.nearClipPlane, camera.farClipPlane);
		camera.fieldOfView = GetFieldOfView(projRaw);

		if (afterEyeExpand != null)
		{
			Log.d(TAG, "CreateEye(" + eye + ")-custom");
			WaveVR_Utils.SafeExecuteAllDelegate<RenderCallbackWithEyeAndCamera>(afterEyeExpand, a => a(this, eye, vrcamera));
		}
		Log.d(TAG, "CreateEye(" + eye + ")-");
		return vrcamera;
	}

#if UNITY_EDITOR && UNITY_ANDROID
	private void createDistortion()
	{
		Log.d(TAG, "createDistortion()+");
		if (distortion == null)
		{
			GameObject distortionobj = new GameObject(OBJ_NAME_DISTORTION, typeof(Camera), typeof(WaveVR_Distortion));
			distortionobj.transform.SetParent(transform, false);
			distortion = distortionobj.GetComponent<WaveVR_Distortion>();
			if (WaveVR.EnableSimulator)
			{
				bool enablePreview = UnityEditor.EditorPrefs.GetBool("EnablePreviewImage");
				if (enablePreview)
				{
					distortion.NoDistortion = true;
				}
			}
		}
		var cam = distortion.GetComponent<Camera>();
		cam.allowHDR = false;
		cam.allowMSAA = false;
#if UNITY_2017_3_OR_NEWER
		cam.allowDynamicResolution = false;
#endif
		cam.stereoTargetEye = StereoTargetEyeMask.None;


		distortion.init();
		Log.d(TAG, "createDistortion()-");
	}
#endif

	/**
	 * The loading black is used to block the other camera or UI drawing on the display.
	 * The native render will use the screen after WaitForEndOfFrame.  And the
	 * native render need time to be ready for sync with Android's flow.  Therefore, the
	 * Screen or HMD may show othehr camera or UI's drawing.  For example, the graphic
	 * raycast need the camera has real output on screen.  We draw it, and cover it by
	 * binocular vision.  It let the gaze or the controller work well.  If we don't
	 * have a black canvas and the native render is delayed, the screen may show a BG
	 * color or the raycast image on the screen for a while.
	**/
	private void createLoadingBlack()
	{
		var found = GetComponentFromChildren<Canvas>(OBJ_NAME_LOADING);
		if (found == null && useLoadingCanvas)
		{
			loadingCanvas = new GameObject(OBJ_NAME_LOADING);
			var canvas = loadingCanvas.AddComponent<Canvas>();
			loadingCanvas.AddComponent<UnityEngine.UI.CanvasScaler>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			GameObject loadingImage = new GameObject("Loading Image");
			loadingImage.transform.SetParent(loadingCanvas.transform, false);
			loadingImage.AddComponent<CanvasRenderer>();
			UnityEngine.UI.Image loading = loadingImage.AddComponent<UnityEngine.UI.Image>();
			loading.material = null;
			loading.color = loadingBlockerColor;
			loading.raycastTarget = false;
			loading.rectTransform.anchoredPosition = new Vector2(0.5f, 0.5f);
			loading.rectTransform.anchorMin = new Vector2(0, 0);
			loading.rectTransform.anchorMax = new Vector2(1, 1);
			loading.rectTransform.offsetMin = new Vector2(0, 0);
			loading.rectTransform.offsetMax = new Vector2(0, 0);

			canvas.enabled = true;  // Avoid black in Editor GameView preview or configuraiton change.
			loadingCanvas.transform.SetParent(transform, true);
		}
	}

	private void setLoadingCanvas(Boolean enabled)
	{
#if UNITY_STANDALONE
		return;
#else
		// If developer has a loadingCanvas, follow developer's current setting. (Backward compatible)
		if (loadingCanvas)
			loadingCanvas.SetActive(enabled);
		else 
			GL.Clear(true, true, loadingBlockerColor);
#endif
	}

#if UNITY_EDITOR && UNITY_ANDROID
	public static void EditorInitial(WaveVR_Render head)
	{
		// Because the variables in runtime need be initialized in Awake, when user click the
		// inspector expand button, these variables will be used without initialized.
		if (!Application.isPlaying)
		{
			head.eyes = new WaveVR_Utils.RigidTransform[] {
				new WaveVR_Utils.RigidTransform(new Vector3(-head.ipd / 2, 0.15f, 0.12f), Quaternion.identity),
				new WaveVR_Utils.RigidTransform(new Vector3(head.ipd / 2, 0.15f, 0.12f), Quaternion.identity)
			};

			head.sceneWidth = Mathf.Max(Screen.width / 2, Screen.height);
			head.sceneHeight = head.sceneWidth;
			Debug.Log("WaveVR_Render internal variables initialized in editor mode.");
		}
	}
#endif

	public static void Expand(WaveVR_Render head)
	{
		Log.d(TAG, "Expand()+");
		if (head.beforeRenderExpand != null)
		{
			WaveVR_Utils.SafeExecuteAllDelegate<RenderCallback>(head.beforeRenderExpand, a => a(head));
			Log.d(TAG, "Expand()+custom");
		}

#if UNITY_EDITOR && UNITY_ANDROID
		EditorInitial(head);
#endif

		if (head.isExpanded) {
			//Debug.Log("Expanded");
		}

		head.centerWVRCamera = head.CreateCenterCamera();
		head.botheyes = head.CreateEyeBoth();
		head.righteye = head.CreateEye(WVR_Eye.WVR_Eye_Right);
		head.lefteye = head.CreateEye(WVR_Eye.WVR_Eye_Left);

#if UNITY_EDITOR && UNITY_ANDROID
		head.createDistortion();
#endif

		var found = head.GetComponentFromChildren<AudioListener>(OBJ_NAME_EAR);
		if (found == null) {
			var earObj = new GameObject(OBJ_NAME_EAR);
			earObj.transform.SetParent(head.transform, false);
			earObj.transform.localPosition = new Vector3(0, 0, -0.01f);  // TODO if 6DOF should be around -0.025f
			earObj.AddComponent<AudioListener>();
			head.ear = earObj;
		}

		AddRaycaster(head.centerCamera.gameObject);

		head.createLoadingBlack();

		if (head.afterRenderExpand != null)
		{
			Log.d(TAG, "Expand()-custom");
			WaveVR_Utils.SafeExecuteAllDelegate<RenderCallback>(head.afterRenderExpand, a => a(head));
		}
		Log.d(TAG, "Expand()-");
	}

	public static void Collapse(WaveVR_Render head)
	{
		if (head.lefteye != null)
			DestroyImmediate(head.lefteye.gameObject);
		head.lefteye = null;

		if (head.righteye != null)
			DestroyImmediate(head.righteye.gameObject);
		head.righteye = null;

		if (head.distortion != null)
			DestroyImmediate(head.distortion.gameObject);
		head.distortion = null;

		if (head.botheyes != null)
			DestroyImmediate(head.botheyes.gameObject);
		head.botheyes = null;

		if (head.centerWVRCamera != null)
			DestroyImmediate(head.centerWVRCamera.gameObject);
		head.centerWVRCamera = null;

		//Transform ear = head.transform.Find(OBJ_NAME_EAR);
		//if (ear != null)
		//	DestroyImmediate(ear.gameObject);
		//head.ear = null;

		var raycast = head.GetComponent<PhysicsRaycaster>();
		if (raycast != null)
			DestroyImmediate(raycast);
		raycast = null;

		if (head.loadingCanvas != null)
		{
			var loading = head.loadingCanvas.gameObject;
			head.loadingCanvas = null;
			DestroyImmediate(loading);
		}
	}
#endregion  // expand

#region FOV_Projection_and_Matrix
	private float GetFieldOfView(float[] projRaw)
	{
		float max = 0;
		// assume near is 1. find max tangent angle.
		foreach (var v in projRaw)
		{
			max = Mathf.Max(Mathf.Abs(v), max);
		}
		return Mathf.Atan2(max, 1) * Mathf.Rad2Deg * 2;
	}

	private static float[] GetEditorProjectionRaw(float fov, float width, float height)
	{
		if (fov < 1)
			fov = 1;
		if (fov > 179)
			fov = 179;

		Matrix4x4 proj = Matrix4x4.identity;
		float w, h;
		if (height > width)
		{
			h = Mathf.Tan(fov / 2 * Mathf.Deg2Rad);
			w = h / height * width;
		}
		else
		{
			w = Mathf.Tan(fov / 2 * Mathf.Deg2Rad);
			h = w / width * height;
		}
		float l = -w, r = w, t = h, b = -h;
		return new float[] { l, r, t, b };

		// This log can help debug projection problem.  Keep it.
		//float[] lrtb = { l, r, t, b };
		//Debug.LogError(" reversed fov " + GetFieldOfView(lrtb) + " real fov " + fov);
	}

	public static void debugLogMatrix(Matrix4x4 m, string name)
	{
		Log.d(TAG, name + ":\n" +
			"/ " + m.m00 + " " + m.m01 + " " + m.m02 + " " + m.m03 + " \\\n" +
			"| " + m.m10 + " " + m.m11 + " " + m.m12 + " " + m.m13 + " |\n" +
			"| " + m.m20 + " " + m.m21 + " " + m.m22 + " " + m.m23 + " |\n" +
			"\\ " + m.m30 + " " + m.m31 + " " + m.m32 + " " + m.m33 + " /");
	}

	private static Matrix4x4 MakeCullingProjectionMatrix(float[] projRawL, float[] projRawR, float near, float far, Vector3 leftEyePosition, Vector3 rightEyePosition, bool lrtbWithNear1 = false)
	{
#if false
		// for debug
		near = 3f;
		far = 10;
		projRawL = new float[] { -0.5f, 0.5f, 0.5f, -0.5f };
		projRawR = new float[] { -0.5f, 0.5f, 0.5f, -0.5f };
		leftEyePosition = new Vector3(-0.03f, 0, 0);
		rightEyePosition = new Vector3(0.03f, 0, 0);
		lrtbWithNear1 = true;
#endif

		if (near < 0.01f)
			near = 0.01f;
		if (far < 0.02f)
			far = 0.02f;

		if (lrtbWithNear1)
		{
			//	LRTBWithNear1 : 1 = LRTB : near
			// => LRTB = LRTBWithNear1 * near
			for (int i = 0; i < 4; i++)
			{
				projRawL[i] = projRawL[i] * near;
				projRawR[i] = projRawR[i] * near;
			}
		}

		Matrix4x4 proj = Matrix4x4.identity;

		// In Unity axis.  lrtb points in eye space.
		Vector3[] lrtbLeftEye = {
			new Vector3(projRawL[0], 0, near),
			new Vector3(projRawL[1], 0, near),
			new Vector3(0, projRawL[2], near),
			new Vector3(0, projRawL[3], near)
		};

		Vector3[] lrtbRightEye = {
			new Vector3(projRawR[0], 0, near),
			new Vector3(projRawR[1], 0, near),
			new Vector3(0, projRawR[2], near),
			new Vector3(0, projRawR[3], near)
		};

		Vector3[] normals = {
			new Vector3(-1, 0, 0),
			new Vector3( 1, 0, 0),
			new Vector3(0,  1, 0),
			new Vector3(0, -1, 0),
		};

		/*
		 *   Different offset case
		 *         +--+-------+ lrtb point in head space.
		 * +-------+   \  |θr/
		 *  \θl|  /|    \ | /
		 *   \ | / |     \|/
		 *    \|/  |      * Right Eye center
		 *     *   |     / <---Left Eye center
		 *      \  h    / <----Head origin
		 *       \ |   /
		 *        \|  /
		 *         + /
		 *         |/
		 *         + new head point
		 */

		float [] largestTangent = new float [4] { 1, 1, 1, 1 };
		float [] largestDistance = new float[4] { near, near, near, near };
		float [] largestZOffset = new float[4] { 0, 0, 0, 0 };

		for (int i = 0; i < 4; i++)
		{
			// In the (x + y), one of them will be zero
			var tanLeft = Mathf.Abs(Vector3.Dot(lrtbLeftEye[i], normals[i])) / lrtbLeftEye[i].z;
			var pointLeft = lrtbLeftEye[i] + Vector3.Dot(leftEyePosition, normals[i]) * normals[i];
			float distanceLeft = Vector3.Distance(pointLeft, new Vector3(0, 0, pointLeft.z));
			//Log.d(TAG, "[" + i + "] l point=(" + pointLeft.x + ", " + pointLeft.y + ", " + pointLeft.z + ")");
			//Log.d(TAG, "[" + i + "] l tangent=" + tanLeft + ", distance=" + distanceLeft);

			var tanRight = Mathf.Abs(Vector3.Dot(lrtbRightEye[i], normals[i])) / lrtbRightEye[i].z;
			var pointRight = lrtbRightEye[i] + Vector3.Dot(rightEyePosition, normals[i]) * normals[i];
			float distanceRight = Vector3.Distance(pointRight, new Vector3(0, 0, pointRight.z));
			//Log.d(TAG, "[" + i + "] r point=(" + pointRight.x + ", " + pointRight.y + ", " + pointRight.z + ")");
			//Log.d(TAG, "[" + i + "] r tangent=" + tanRight + ", distance=" + distanceRight);

			largestTangent[i] = Mathf.Max(tanLeft, tanRight);
			largestDistance[i] = Mathf.Max(distanceLeft, distanceRight);
			var largestDistancePoint = distanceLeft > distanceRight ? pointLeft : pointRight;
			largestZOffset[i] = largestDistancePoint.z - largestDistance[i] / largestTangent[i];

			//Log.d(TAG, "[" + i + "] largestPoint=(" + largestDistancePoint.x + ", " + largestDistancePoint.y + ", " + largestDistancePoint.z + ")");
			//Log.d(TAG, "[" + i + "] tangent=" + largestTangent[i] + ", distance=" + largestDistance[i] + ", offset=" + largestZOffset[i]);
		}

		// Find the new head point
		float ZOffset = 0;
		for (int i = 0; i < 4; i++)
			ZOffset = Mathf.Min(largestZOffset[i], ZOffset);

		//Log.d(TAG, "final ZOffset is " + ZOffset);

		proj = MakeProjection(-largestTangent[0] / 2, largestTangent[1] / 2, largestTangent[2], -largestTangent[3], -ZOffset, far, true);
		//debugLogMatrix(proj, "Culling Proj");

		var WorldToCullingCameraTranslation = Matrix4x4.TRS(new Vector3(0, 0, -3), Quaternion.identity, Vector3.one);
		//debugLogMatrix(proj, "Culling Camera Translation");

		// (P * T) * Vcentercamera = P * (T * Vcentercamera)
		return proj * WorldToCullingCameraTranslation;
	}

	private static Matrix4x4 GetProjection(float[] projRaw, float near, float far)
	{
		Log.d(TAG, "GetProjection()");
		if (near < 0.01f)
			near = 0.01f;
		if (far < 0.02f)
			far = 0.02f;

		Matrix4x4 proj = Matrix4x4.identity;

		// The values in ProjectionRaw are made by assuming the near value is 1.
		proj = MakeProjection(projRaw[0], projRaw[1], projRaw[2], projRaw[3], near, far, true);
		return proj;
	}

#if UNITY_STANDALONE

	public static Matrix4x4 MakeProjection(float l, float r, float t, float b, float n, float f, bool lrtbWithNear1 = false)
	{
		float idx = 1.0f / (r - l);
		float idy = 1.0f / (b - t);
		float idz = 1.0f / (f - n);
		float sx = r + l;
		float sy = b + t;

		Matrix4x4 m = Matrix4x4.zero;
		m[0, 0] = 2 * idx; m[0, 1] = 0; m[0, 2] = sx * idx; m[0, 3] = 0;
		m[1, 0] = 0; m[1, 1] = 2 * idy; m[1, 2] = sy * idy; m[1, 3] = 0;
		m[2, 0] = 0; m[2, 1] = 0; m[2, 2] = -f * idz; m[2, 3] = -f * n * idz;
		m[3, 0] = 0; m[3, 1] = 0; m[3, 2] = -1.0f; m[3, 3] = 0;
		return m;
	}
#else
	// When lrtbWithNear1 is true, lrtb should be the tangent value.
	public static Matrix4x4 MakeProjection(float l, float r, float t, float b, float n, float f, bool lrtbWithNear1 = false)
	{
		//	LRTBWithNear1 : 1 = LRTB : near
		// => LRTB = LRTBWithNear1 * near
		// => m[0, 0] = 2 * near / (r * near - l * near) => 2 / (r - l)
		float near = lrtbWithNear1 ? 1 : n;
		Matrix4x4 m = Matrix4x4.zero;
		m[0, 0] = 2 * near / (r - l);
		m[1, 1] = 2 * near / (t - b);
		m[0, 2] = (r + l) / (r - l);
		m[1, 2] = (t + b) / (t - b);
		m[2, 2] = -(f + n) / (f - n);
		m[2, 3] = -2 * f * n / (f - n);
		m[3, 2] = -1;
		return m;
	}
#endif
#endregion  // fov_projection_and_matrix

#region TimeControl
	// TimeControl: Set Time.timeScale = 0 if input focus in gone.
	private bool previousInputFocus = true;

	[Tooltip("Allow render to set Time.timeScale = 0 if input focus in gone.")]
	public bool needTimeControl = false;

	private void TimeControl()
	{
		if (needTimeControl)
		{
#if UNITY_EDITOR && UNITY_ANDROID
			// Nothing can simulate the focus lost in editor.  Just leave.
			if (Application.isEditor)
				return;
#endif
			bool hasInputFocus = !WaveVR.Instance.FocusCapturedBySystem;

			if (!previousInputFocus || !hasInputFocus)
			{
				previousInputFocus = hasInputFocus;
				Time.timeScale = hasInputFocus ? 1 : 0;
				Log.d(TAG, "InputFocus " + hasInputFocus + "Time.timeScale " + Time.timeScale);
			}
		}
	}
#endregion

	public void SafeExecuteRenderEyeCallback(RenderCallbackWithEyeAndCamera multi, WVR_Eye eye, WaveVR_Camera wvrCamera)
	{
		if (multi == null)
			return;

		try
		{
			multi(this, eye, wvrCamera);
		}
		catch (Exception e)
		{
			Log.e(TAG, e.ToString(), true);
		}
	}

#region FoveatedRendering
	private static readonly WVR_RenderFoveationParams[] foveationParams = new WVR_RenderFoveationParams[2];

	// Use WaveVR_FoveatedRednering.cs for your convenience.
	public static void SetFoveatedRenderingParameter(WVR_Eye eye, float ndcFocalPointX, float ndcFocalPointY, float clearVisionFOV, WVR_PeripheralQuality peripheralQuality)
	{
		int e = eye == WVR_Eye.WVR_Eye_Left ? 0 : 1;
		foveationParams[e].focalX = ndcFocalPointX;
		foveationParams[e].focalY = ndcFocalPointY;
		foveationParams[e].fovealFov = clearVisionFOV;
		foveationParams[e].periQuality = peripheralQuality;
	}
#endregion

#region RenderThreadAndSubmit
	// Unsafe / Unmanagered
	private class RenderThreadContext : wvr.render.utils.Message
	{
		public WVR_Eye eye;

		public int textureCount;
		public WVR_TextureParams_t[] textureParam = new WVR_TextureParams_t[2];

		public int foveationCount;
		public WVR_RenderFoveationParams[] foveationParams = new WVR_RenderFoveationParams[2];

		public int poseCount;
		public WVR_PoseState_t[] pose = new WVR_PoseState_t[1];

		public WVR_SubmitExtend flag;

		public int renderEvent = -1;
		public WVR_RenderInitParams_t renderInitParams = new WVR_RenderInitParams_t();

		public RenderThreadContext() { }

		public static void IssueRenderEvent(RenderThreadSyncObject syncObj, int renderEvent, WVR_RenderInitParams_t renderInitParams)
		{
			var queue = syncObj.Queue;
			lock (queue)
			{
				var msg = queue.Obtain<RenderThreadContext>();
				msg.renderEvent = renderEvent;
				msg.renderInitParams = renderInitParams;
				queue.Enqueue(msg);
			}
			syncObj.IssueEvent();
		}

		public static void ReceiveRenderEvent(wvr.render.utils.PreAllocatedQueue queue)
		{
			// Run in render thread
			lock (queue)
			{
				var msg = (RenderThreadContext)queue.Dequeue();
				msg.CopyTo(contextRTOnly);
				queue.Release(msg);
			}

			switch (contextRTOnly.renderEvent)
			{
				case WaveVR_Utils.RENDEREVENTID_INIT_GRAPHIC:
					{
						Interop.WVR_RenderInit(ref contextRTOnly.renderInitParams);
					}
					break;
				default:
					break;
			}
			contextRTOnly.renderEvent = -1;
		}

		public static void IssueBefore(RenderThreadSyncObject syncObj, WVR_Eye eye, int textureCount, WVR_TextureParams_t textureParam0, WVR_TextureParams_t textureParam1, int foveationCount, WVR_RenderFoveationParams foveationParams0, WVR_RenderFoveationParams foveationParams1)
		{
			var queue = syncObj.Queue;
			lock (queue)
			{
				var msg = queue.Obtain<RenderThreadContext>();
				msg.eye = eye;
				msg.textureCount = textureCount;
				msg.textureParam[0] = textureParam0;
				msg.textureParam[1] = textureParam1;
				msg.foveationCount = foveationCount;
				msg.foveationParams[0] = foveationParams0;
				msg.foveationParams[1] = foveationParams1;
				queue.Enqueue(msg);
			}
			syncObj.IssueEvent();
		}

		public static void IssueAfter(RenderThreadSyncObject syncObj, WVR_Eye eye, int textureCount, WVR_TextureParams_t textureParam0, WVR_TextureParams_t textureParam1, int poseCount, WVR_PoseState_t pose0, WVR_SubmitExtend flag)
		{
			var queue = syncObj.Queue;
			lock (queue)
			{
				var msg = queue.Obtain<RenderThreadContext>();
				msg.eye = eye;
				msg.textureCount = textureCount;
				msg.textureParam[0] = textureParam0;
				msg.textureParam[1] = textureParam1;
				msg.poseCount = poseCount;
				msg.pose[0] = pose0;
				msg.flag = flag;
				queue.Enqueue(msg);
			}
			syncObj.IssueEvent();
		}

		public static void ReceiveBefore(wvr.render.utils.PreAllocatedQueue queue)
		{
			// Run in render thread
			lock (queue)
			{
				// Please avoid crash here!  The Unity's RenderThread didn't allow any exception and UnityEditor will hang.
				var msg = (RenderThreadContext)queue.Dequeue();
				msg.CopyTo(contextRTOnly);
				queue.Release(msg);
			}

			if (contextRTOnly.eye == WVR_Eye.WVR_Eye_Both)
				contextRTOnly.eye = WVR_Eye.WVR_Eye_Left;

#if UNITY_EDITOR && UNITY_ANDROID
			bool isEditor = true;
			if (isEditor) return;
#endif
			if (isDuringFirstFrame)
				Log.d(TAG, "FirstFrame : call WVR_PreRenderEye +++");
			Interop.WVR_PreRenderEye(
				contextRTOnly.eye,
				contextRTOnly.textureCount == 0 ? null : contextRTOnly.textureParam,
				contextRTOnly.foveationCount == 0 ? null : contextRTOnly.foveationParams);
			if (isDuringFirstFrame)
				Log.d(TAG, "FirstFrame : call WVR_PreRenderEye ---");
		}

		public static void ReceiveAfter(wvr.render.utils.PreAllocatedQueue queue)
		{
			lock (queue)
			{
				var msg = (RenderThreadContext)queue.Dequeue();
				msg.CopyTo(contextRTOnly);
				queue.Release(msg);
			}
			// Hack here.  Hope we can remove it later.
			if (contextRTOnly.eye == WVR_Eye.WVR_Eye_Both)
				contextRTOnly.eye = WVR_Eye.WVR_Eye_Left;

#if UNITY_EDITOR && UNITY_ANDROID
			bool isEditor = true;
			if (isEditor) return;
#endif

			if (isDuringFirstFrame)
				Log.d(TAG, "FirstFrame : call WVR_SubmitFrame +++");
			Interop.WVR_SubmitFrame(
				contextRTOnly.eye,
				contextRTOnly.textureCount == 0 ? null : contextRTOnly.textureParam,
				contextRTOnly.poseCount == 0 ? null : contextRTOnly.pose,
				contextRTOnly.flag);
			if (isDuringFirstFrame)
				Log.d(TAG, "FirstFrame : call WVR_SubmitFrame ---");

			//UGL.TexParameteri((uint) contextRTOnly.textureParam[0].id.ToInt32(), 0x8BFB, 0);
		}

		public void CopyTo(RenderThreadContext dest)
		{
			dest.eye = eye;
			dest.textureCount = textureCount;
			dest.textureParam[0] = textureParam[0];
			dest.textureParam[1] = textureParam[1];
			dest.foveationCount = foveationCount;
			dest.foveationParams[0] = foveationParams[0];
			dest.foveationParams[1] = foveationParams[1];
			dest.poseCount = poseCount;
			dest.pose[0] = pose[0];
			dest.flag = flag;
			dest.renderEvent = renderEvent;
			dest.renderInitParams = renderInitParams;
		}
	}

	// static... no need to manager.  It works like a global variable in C++ code, and only render thread will modify it.
	private static RenderThreadContext contextRTOnly = new RenderThreadContext();
	private static readonly RenderThreadSyncObject RenderCommandRenderEvent = new RenderThreadSyncObject(RenderThreadContext.ReceiveRenderEvent);
	private static readonly RenderThreadSyncObject RenderCommandBeforeEyeTemplate = new RenderThreadSyncObject(RenderThreadContext.ReceiveBefore);
	private static readonly RenderThreadSyncObject RenderCommandAfterEyeTemplate = new RenderThreadSyncObject(RenderThreadContext.ReceiveAfter);

	// { 0, 1, 2 } maps { left, right, both }
	// TODO left, right, both share the same process now.  Can we let the developer to override them?
	private static readonly RenderThreadSyncObject[] RenderCommandBeforeEye = { RenderCommandBeforeEyeTemplate, RenderCommandBeforeEyeTemplate, RenderCommandBeforeEyeTemplate };
	private static readonly RenderThreadSyncObject[] RenderCommandAfterEye = { RenderCommandAfterEyeTemplate, RenderCommandAfterEyeTemplate, RenderCommandAfterEyeTemplate };

	private static WVR_PoseState_t poseUsedOnSubmit = new WVR_PoseState_t() { IsValidPose = false };
	private static WVR_SubmitExtend submitExtendFlag = WVR_SubmitExtend.WVR_SubmitExtend_Default;

	public static void SetPoseUsedOnSubmit(WVR_PoseState_t pose)
	{
		poseUsedOnSubmit = pose;
	}

	public static void ResetPoseUsedOnSubmit()
	{
		poseUsedOnSubmit.IsValidPose = false;
	}

	public static void SetSubmitExtendedFlag(WVR_SubmitExtend flag)
	{
		submitExtendFlag = flag;
	}
#endregion  // RenderThread and Submit

#if UNITY_EDITOR && UNITY_ANDROID
	private bool isMirroDeviceState = false;

	void OnValidate()
	{
		// Add tag of isMirrorToDevice state for simulator
		{
			if (EditorPrefs.GetBool("isMirrorToDevice") == true )
			{
				isMirroDeviceState = true;
			}
			else
			{
				isMirroDeviceState = false;
			}
		}
	}
#endif

#if UNITY_STANDALONE
	public void OnUpdateFrame()
	{
		WaveVR.Instance.UpdateEachFrame(origin);

		UpdateViewMatrix();
	}

	public Matrix4x4[] uView = new Matrix4x4[2];
	private void UpdateViewMatrix()
	{
		uView[0] = lefteye.GetCamera().worldToCameraMatrix;
		uView[1] = righteye.GetCamera().worldToCameraMatrix;
	}

    public uint frameInx = 0;
    public WVR_DevicePosePair_t[] devicePoses = new WVR_DevicePosePair_t[3];
	public bool submitUnityNativeRenderTexture = false;
	public System.IntPtr[] leftTexPtr = null;
	public System.IntPtr[] rightTexPtr = null;

	private void InitTextureBound()
	{
		textureBounds[0] = MakeTextureBound(
			botheyes.GetCamera().GetStereoProjectionMatrix(Camera.StereoscopicEye.Left),
			MakeProjection(projRawL[0], projRawL[1], projRawL[2], projRawL[3], botheyes.GetCamera().nearClipPlane, botheyes.GetCamera().farClipPlane));

		textureBounds[1] = MakeTextureBound(
			botheyes.GetCamera().GetStereoProjectionMatrix(Camera.StereoscopicEye.Right),
			MakeProjection(projRawR[0], projRawR[1], projRawR[2], projRawR[3], botheyes.GetCamera().nearClipPlane, botheyes.GetCamera().farClipPlane));

		texBounds = new WVR_TextureBound_t[2];

		texBounds[0] = new WVR_TextureBound_t { uMin = textureBounds[0].x, uMax = textureBounds[0].y, vMin = textureBounds[0].w, vMax = textureBounds[0].z };
		texBounds[1] = new WVR_TextureBound_t { uMin = textureBounds[1].x, uMax = textureBounds[1].y, vMin = textureBounds[1].w, vMax = textureBounds[1].z };

		Interop.WVR_SetTextureBounds(texBounds);
	}

	public Vector4 MakeTextureBound(Matrix4x4 uProj, Matrix4x4 tProj)
	{
		Vector4 v_RT, v_LB, s_RT, s_LB;

		v_RT = MatrixMulVector(tProj.inverse, new Vector4(1, 1, 0, 1));
		v_LB = MatrixMulVector(tProj.inverse, new Vector4(-1, -1, 0, 1));

		s_RT = MatrixMulVector(uProj, new Vector4(v_RT.x, v_RT.y, -1, 1));
		s_LB = MatrixMulVector(uProj, new Vector4(v_LB.x, v_LB.y, -1, 1));

		s_RT.x = ((s_RT.x / s_RT.w) + 1.0f) / 2.0f;
		s_RT.y = ((s_RT.y / s_RT.w) + 1.0f) / 2.0f;
		s_LB.x = ((s_LB.x / s_LB.w) + 1.0f) / 2.0f;
		s_LB.y = ((s_LB.y / s_LB.w) + 1.0f) / 2.0f;

		return new Vector4(s_LB.x, s_RT.x, s_LB.y, s_RT.y);
	}

	public Vector4 MatrixMulVector(Matrix4x4 m, Vector4 v)
	{
		Vector4 row0 = m.GetRow(0);
		Vector4 row1 = m.GetRow(1);
		Vector4 row2 = m.GetRow(2);
		Vector4 row3 = m.GetRow(3);

		float v0 = row0.x * v.x + row0.y * v.y + row0.z * v.z + row0.w * v.w;
		float v1 = row1.x * v.x + row1.y * v.y + row1.z * v.z + row1.w * v.w;
		float v2 = row2.x * v.x + row2.y * v.y + row2.z * v.z + row2.w * v.w;
		float v3 = row3.x * v.x + row3.y * v.y + row3.z * v.z + row3.w * v.w;

		return new Vector4(v0, v1, v2, v3);
	}

#endif
}
