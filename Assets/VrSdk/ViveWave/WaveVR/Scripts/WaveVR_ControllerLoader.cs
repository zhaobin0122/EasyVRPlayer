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
using System.Runtime.InteropServices;
using UnityEngine.EventSystems;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(WaveVR_ControllerLoader))]
public class WaveVR_ControllerLoaderEditor : Editor
{
	public override void OnInspectorGUI()
	{
		WaveVR_ControllerLoader myScript = target as WaveVR_ControllerLoader;

		myScript.WhichHand = (WaveVR_ControllerLoader.ControllerHand)EditorGUILayout.EnumPopup ("Type", myScript.WhichHand);
		myScript.ControllerComponents = (WaveVR_ControllerLoader.CComponent)EditorGUILayout.EnumPopup ("Controller Components", myScript.ControllerComponents);

		myScript.TrackPosition = EditorGUILayout.Toggle ("Track Position", myScript.TrackPosition);
		if (true == myScript.TrackPosition)
		{
			myScript.SimulationOption = (WVR_SimulationOption)EditorGUILayout.EnumPopup ("  Simulate Position", myScript.SimulationOption);
			if (myScript.SimulationOption == WVR_SimulationOption.ForceSimulation || myScript.SimulationOption == WVR_SimulationOption.WhenNoPosition)
			{
				myScript.FollowHead = (bool)EditorGUILayout.Toggle ("    Follow Head", myScript.FollowHead);
			}
		}

		myScript.TrackRotation = EditorGUILayout.Toggle ("Track Rotation", myScript.TrackRotation);
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Controller model");
		myScript.adaptiveLoading = EditorGUILayout.Toggle("  Adaptive loading", myScript.adaptiveLoading);
		if (true == myScript.adaptiveLoading)
		{

			EditorGUILayout.LabelField("    Emitter");
			myScript.enableEmitter = EditorGUILayout.Toggle("      Enable emitter", myScript.enableEmitter);
			if (true == myScript.enableEmitter)
			{
				EditorGUILayout.LabelField("    Event");
				myScript.sendEvent = EditorGUILayout.Toggle("      Send event", myScript.sendEvent);

				EditorGUILayout.LabelField("    Beam");
				myScript.ShowBeam = EditorGUILayout.Toggle("      Show beam", myScript.ShowBeam);
				if (true == myScript.ShowBeam)
				{
					myScript.useBeamSystemConfig = EditorGUILayout.Toggle("      Apply system config", myScript.useBeamSystemConfig);
					if (true != myScript.useBeamSystemConfig)
					{
						EditorGUILayout.LabelField("      Custom settings");
						myScript.updateEveryFrame = EditorGUILayout.Toggle("        Need to update every frame", myScript.updateEveryFrame);
						myScript.StartOffset = EditorGUILayout.FloatField("        Start offset ", myScript.StartOffset);
						myScript.StartWidth = EditorGUILayout.FloatField("        Start width ", myScript.StartWidth);
						myScript.EndOffset = EditorGUILayout.FloatField("        End offset ", myScript.EndOffset);
						myScript.EndWidth = EditorGUILayout.FloatField("        End width ", myScript.EndWidth);

						EditorGUILayout.Space();
						myScript.useDefaultMaterial = EditorGUILayout.Toggle("        Use default material", myScript.useDefaultMaterial);

						if (false == myScript.useDefaultMaterial)
						{
							myScript.customMat = (Material)EditorGUILayout.ObjectField("        Custom material", myScript.customMat, typeof(Material), false);
						}
						else
						{
							myScript.StartColor = EditorGUILayout.ColorField("          Start color", myScript.StartColor);
							myScript.EndColor = EditorGUILayout.ColorField("          End color", myScript.EndColor);
						}
					}
				}

				EditorGUILayout.LabelField("    Controller pointer");
				myScript.showPointer = EditorGUILayout.Toggle("      Show controller pointer", myScript.showPointer);
				if (true == myScript.showPointer)
				{
					myScript.usePointerSystemConfig = EditorGUILayout.Toggle("      Apply system config", myScript.usePointerSystemConfig);
					if (true != myScript.usePointerSystemConfig)
					{
						EditorGUILayout.LabelField("      Custom settings");
						myScript.PointerOuterDiameterMin = EditorGUILayout.FloatField("        Min. pointer diameter ", myScript.PointerOuterDiameterMin);
						myScript.useTexture = EditorGUILayout.Toggle("        Use Teuxture", myScript.useTexture);

						// useTexture only support mesh of CtrlQuadPointer
						if (true == myScript.useTexture) {
							myScript.UseDefaultTexture = EditorGUILayout.Toggle ("        Use default pointer texture ", myScript.UseDefaultTexture);

							if (false == myScript.UseDefaultTexture) {
								myScript.customTexture = (Texture2D)EditorGUILayout.ObjectField ("        Custom pointer texture", myScript.customTexture, typeof(Texture2D), false);
							}
						} else {
							// Blink only support mesh of WaveVR_Mesh_Q
							myScript.Blink = EditorGUILayout.Toggle("        Controller pointer will Blink", myScript.Blink);
						}
					}
				}
			}

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("  Button effect");
			myScript.enableButtonEffect = EditorGUILayout.Toggle("    Enable button effect", myScript.enableButtonEffect);
			if (true == myScript.enableButtonEffect)
			{
				myScript.useEffectSystemConfig = EditorGUILayout.Toggle("    Apply system config", myScript.useEffectSystemConfig);
				if (true != myScript.useEffectSystemConfig)
				{
					myScript.buttonEffectColor = EditorGUILayout.ColorField("    Button effect color", myScript.buttonEffectColor);
				}
			}

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("  Indication feature");
			myScript.overwriteIndicatorSettings = true;
			myScript.showIndicator = EditorGUILayout.Toggle("    Show Indicator", myScript.showIndicator);
			if (true == myScript.showIndicator)
			{
				myScript.useIndicatorSystemConfig = EditorGUILayout.Toggle("    Use system config", myScript.useIndicatorSystemConfig);
				if (false == myScript.useIndicatorSystemConfig)
				{
					myScript.basedOnEmitter = EditorGUILayout.Toggle("      Indicator based on emitter ", myScript.basedOnEmitter);
					myScript.hideIndicatorByRoll = EditorGUILayout.Toggle("      Hide Indicator when roll angle > 90 ", myScript.hideIndicatorByRoll);
					myScript.showIndicatorAngle = EditorGUILayout.FloatField("      Show When Angle > ", myScript.showIndicatorAngle);
					EditorGUILayout.Space();

					EditorGUILayout.LabelField("      Line customization");
					myScript.lineLength = EditorGUILayout.FloatField("        Line Length", myScript.lineLength);
					myScript.lineStartWidth = EditorGUILayout.FloatField("        Line Start Width", myScript.lineStartWidth);
					myScript.lineEndWidth = EditorGUILayout.FloatField("        Line End Width", myScript.lineEndWidth);
					myScript.lineColor = EditorGUILayout.ColorField("        Line Color", myScript.lineColor);
					EditorGUILayout.Space();

					EditorGUILayout.LabelField("      Text customization");
					myScript.textCharacterSize = EditorGUILayout.FloatField("        Text Character Size", myScript.textCharacterSize);
					myScript.zhCharactarSize = EditorGUILayout.FloatField("        Chinese Character Size", myScript.zhCharactarSize);
					myScript.textFontSize = EditorGUILayout.IntField("        Text Font Size", myScript.textFontSize);
					myScript.textColor = EditorGUILayout.ColorField("        Text Color", myScript.textColor);
					EditorGUILayout.Space();

					EditorGUILayout.LabelField("      Key indication");
					var list = myScript.buttonIndicationList;

					int newCount = Mathf.Max(0, EditorGUILayout.IntField("         Button indicator size", list.Count));

					while (newCount < list.Count)
						list.RemoveAt(list.Count - 1);
					while (newCount > list.Count)
						list.Add(new ButtonIndication());

					for (int i = 0; i < list.Count; i++)
					{
						EditorGUILayout.LabelField("         Button indication " + i);
						myScript.buttonIndicationList[i].keyType = (ButtonIndication.KeyIndicator)EditorGUILayout.EnumPopup("         Key Type", myScript.buttonIndicationList[i].keyType);
						myScript.buttonIndicationList[i].alignment = (ButtonIndication.Alignment)EditorGUILayout.EnumPopup("         Alignment", myScript.buttonIndicationList[i].alignment);
						myScript.buttonIndicationList[i].indicationOffset = EditorGUILayout.Vector3Field("         Indication offset", myScript.buttonIndicationList[i].indicationOffset);
						myScript.buttonIndicationList[i].useMultiLanguage = EditorGUILayout.Toggle("         Use multi-language", myScript.buttonIndicationList[i].useMultiLanguage);
						if (myScript.buttonIndicationList[i].useMultiLanguage)
							myScript.buttonIndicationList[i].indicationText = EditorGUILayout.TextField("         Indication key", myScript.buttonIndicationList[i].indicationText);
						else
							myScript.buttonIndicationList[i].indicationText = EditorGUILayout.TextField("         Indication text", myScript.buttonIndicationList[i].indicationText);
						myScript.buttonIndicationList[i].followButtonRotation = EditorGUILayout.Toggle("         Follow button rotation", myScript.buttonIndicationList[i].followButtonRotation);
						EditorGUILayout.Space();
					}
				}
			}
		}
		else
		{
			EditorGUILayout.LabelField("Indication feature");

			myScript.overwriteIndicatorSettings = EditorGUILayout.Toggle("  Overwrite Indicator Settings", myScript.overwriteIndicatorSettings);
			if (true == myScript.overwriteIndicatorSettings)
			{
				myScript.showIndicator = EditorGUILayout.Toggle("	Show Indicator", myScript.showIndicator);
				if (true == myScript.showIndicator)
				{
					myScript.useIndicatorSystemConfig = EditorGUILayout.Toggle("	Use system config", myScript.useIndicatorSystemConfig);
					if (false == myScript.useIndicatorSystemConfig)
					{
						myScript.basedOnEmitter = EditorGUILayout.Toggle("	Indicator based on emitter ", myScript.basedOnEmitter);
						myScript.hideIndicatorByRoll = EditorGUILayout.Toggle("	Hide Indicator when roll angle > 90 ", myScript.hideIndicatorByRoll);
						myScript.showIndicatorAngle = EditorGUILayout.FloatField("	Show When Angle > ", myScript.showIndicatorAngle);
						EditorGUILayout.Space();

						EditorGUILayout.LabelField("	Line customization");
						myScript.lineLength = EditorGUILayout.FloatField("	  Line Length", myScript.lineLength);
						myScript.lineStartWidth = EditorGUILayout.FloatField("	  Line Start Width", myScript.lineStartWidth);
						myScript.lineEndWidth = EditorGUILayout.FloatField("	  Line End Width", myScript.lineEndWidth);
						myScript.lineColor = EditorGUILayout.ColorField("	  Line Color", myScript.lineColor);
						EditorGUILayout.Space();

						EditorGUILayout.LabelField("	Text customization");
						myScript.textCharacterSize = EditorGUILayout.FloatField("	  Text Character Size", myScript.textCharacterSize);
						myScript.zhCharactarSize = EditorGUILayout.FloatField("	  Chinese Character Size", myScript.zhCharactarSize);
						myScript.textFontSize = EditorGUILayout.IntField("	  Text Font Size", myScript.textFontSize);
						myScript.textColor = EditorGUILayout.ColorField("	  Text Color", myScript.textColor);
						EditorGUILayout.Space();

						EditorGUILayout.LabelField("	Key indication");
						var list = myScript.buttonIndicationList;

						int newCount = Mathf.Max(0, EditorGUILayout.IntField("	  Button indicator size", list.Count));

						while (newCount < list.Count)
							list.RemoveAt(list.Count - 1);
						while (newCount > list.Count)
							list.Add(new ButtonIndication());

						for (int i = 0; i < list.Count; i++)
						{
							EditorGUILayout.LabelField("	  Button indication " + i);
							myScript.buttonIndicationList[i].keyType = (ButtonIndication.KeyIndicator)EditorGUILayout.EnumPopup("		Key Type", myScript.buttonIndicationList[i].keyType);
							myScript.buttonIndicationList[i].alignment = (ButtonIndication.Alignment)EditorGUILayout.EnumPopup("		Alignment", myScript.buttonIndicationList[i].alignment);
							myScript.buttonIndicationList[i].indicationOffset = EditorGUILayout.Vector3Field("		Indication offset", myScript.buttonIndicationList[i].indicationOffset);
							myScript.buttonIndicationList[i].useMultiLanguage = EditorGUILayout.Toggle("		Use multi-language", myScript.buttonIndicationList[i].useMultiLanguage);
							if (myScript.buttonIndicationList[i].useMultiLanguage)
								myScript.buttonIndicationList[i].indicationText = EditorGUILayout.TextField("		Indication key", myScript.buttonIndicationList[i].indicationText);
							else
								myScript.buttonIndicationList[i].indicationText = EditorGUILayout.TextField("		Indication text", myScript.buttonIndicationList[i].indicationText);
							myScript.buttonIndicationList[i].followButtonRotation = EditorGUILayout.Toggle("		Follow button rotation", myScript.buttonIndicationList[i].followButtonRotation);
							EditorGUILayout.Space();
						}
					}
				}
			}
		}

		if (GUI.changed)
			EditorUtility.SetDirty ((WaveVR_ControllerLoader)target);
	}
}
#endif

public class WaveVR_ControllerLoader : MonoBehaviour {
	private static string LOG_TAG = "WaveVR_ControllerLoader";
	private void PrintDebugLog(string msg)
	{
		Log.d (LOG_TAG, "Hand: " + WhichHand + ", " + msg, true);
	}

	private void PrintInfoLog(string msg)
	{
		Log.i (LOG_TAG, "Hand: " + WhichHand + ", " + msg, true);
	}

	private void PrintWarningLog(string msg)
	{
		Log.w(LOG_TAG, "Hand: " + WhichHand + ", " + msg, true);
	}

	public enum ControllerHand
	{
		Dominant,
		Non_Dominant
	};

	public enum CComponent
	{
		One_Bone,
		Multi_Component
	};

	public enum CTrackingSpace
	{
		REAL_POSITION_ONLY,
		FAKE_POSITION_ONLY,
		AUTO_POSITION_ONLY,
		ROTATION_ONLY,
		ROTATION_AND_REAL_POSITION,
		ROTATION_AND_FAKE_POSITION,
		ROTATION_AND_AUTO_POSITION,
		CTS_SYSTEM
	};

	public enum ControllerType
	{
		ControllerType_None,
		ControllerType_Generic,
		ControllerType_Resources,
		ControllerType_AssetBundles,
		ControllerType_AdaptiveController
	}

	public enum CLoadingState
	{
		LoadingState_NOT_LOADED,
		LoadingState_LOADING,
		LoadingState_LOADED
	}

	[Header("Loading options")]
	public ControllerHand WhichHand = ControllerHand.Dominant;
	public CComponent ControllerComponents = CComponent.Multi_Component;
	public bool TrackPosition = true;
	public WVR_SimulationOption SimulationOption = WVR_SimulationOption.WhenNoPosition;
	public bool FollowHead = false;
	public bool TrackRotation = true;

	[Header("Indication feature")]
	public bool overwriteIndicatorSettings = true;
	public bool showIndicator = false;
	public bool hideIndicatorByRoll = true;
	public bool basedOnEmitter = true;

	[Range(0, 90.0f)]
	public float showIndicatorAngle = 30.0f;

	[Header("Line customization")]
	[Range(0.01f, 0.1f)]
	public float lineLength = 0.03f;
	[Range(0.0001f, 0.1f)]
	public float lineStartWidth = 0.0004f;
	[Range(0.0001f, 0.1f)]
	public float lineEndWidth = 0.0004f;
	public Color lineColor = Color.white;

	[Header("Text customization")]
	[Range(0.01f, 0.2f)]
	public float textCharacterSize = 0.08f;
	[Range(0.01f, 0.2f)]
	public float zhCharactarSize = 0.07f;
	[Range(50, 200)]
	public int textFontSize = 100;
	public Color textColor = Color.white;

	[Header("Indications")]
	public bool useIndicatorSystemConfig = true;
	public List<ButtonIndication> buttonIndicationList = new List<ButtonIndication>();

	[Header("AdaptiveLoading")]
	public bool adaptiveLoading = true;  // flag to describe if enable adaptive controller loading feature
	public bool enableEmitter = true;
	public bool sendEvent = true;

	[Header("ButtonEffect")]
	public bool enableButtonEffect = true;
	public bool useEffectSystemConfig = true;
	public Color32 buttonEffectColor = new Color32(0, 179, 227, 255);

	[Header("Beam")]
	public bool ShowBeam = true;
	public bool useBeamSystemConfig = true;
	public bool updateEveryFrame = false;
	public float StartWidth = 0.000625f;	// in x,y axis
	public float EndWidth = 0.00125f;	   // let the bean seems the same width in far distance.
	public float StartOffset = 0.015f;
	public float EndOffset = 0.8f;
	public Color32 StartColor = new Color32(255, 255, 255, 255);
	public Color32 EndColor = new Color32(255, 255, 255, 77);
	public bool useDefaultMaterial = true;
	public Material customMat = null;

	[Header("Controller pointer")]
	public bool showPointer = true;
	public bool usePointerSystemConfig = true;
	public bool Blink = false;
	public bool UseDefaultTexture = true;
	public Texture2D customTexture = null;
	public float PointerOuterDiameterMin = 0.01f;
	private float PointerDistanceInMeters = 1.3f;   // Current distance of the pointer (in meters) = beam.endOffset (0.8) + 0.5
	public bool useTexture = true;
	public Color PointerColor = Color.white;						// #FFFFFFFF
	private Color borderColor = new Color(119, 119, 119, 255);	  // #777777FF
	private Color focusColor = new Color(255, 255, 255, 255);	   // #FFFFFFFF
	private Color focusBorderColor = new Color(119, 119, 119, 255); // #777777FF
	private string TextureName = null;

	private ControllerType controllerType = ControllerType.ControllerType_None;
	private GameObject controllerPrefab = null;
	private GameObject originalControllerPrefab = null;
	private string controllerFileName = "";
	private string controllerModelFoler = "Controller/";
	private string genericControllerFileName = "Generic_";
	private List<AssetBundle> loadedAssetBundle = new List<AssetBundle>();
	private string renderModelNamePath = "";
	private WaveVR_Controller.EDeviceType focusController = WaveVR_Controller.EDeviceType.Dominant;
	private WaveVR_Controller.EDeviceType deviceType = WaveVR_Controller.EDeviceType.Dominant;
	private string renderModelName = "";
	private bool connected = false;
	private CLoadingState mLoadingState = CLoadingState.LoadingState_NOT_LOADED;

	// Variables for getting render model
	private string parameterName = "GetRenderModelName";
	private IntPtr ptrParameterName = IntPtr.Zero;
	private IntPtr ptrResult = IntPtr.Zero;
	private uint resultVertLength = 64;
	private int bufferSize = 64;
	private WaveVR_ControllerInstanceManager CtrInstanceMgr;
	private int ControllerIdx = 0;
	private ModelSpecify modelSpecify;
#if UNITY_EDITOR
	public delegate void ControllerModelLoaded(GameObject go);
	public static event ControllerModelLoaded onControllerModelLoaded = null;
#endif
	private bool forceCheckRenderModelName = false;
	private bool lastFrameConnection = false;

	private void checkAndCreateCIM()
	{
		GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
		foreach (GameObject go in allObjects) {
			if (go.name.Equals("WaveVR_ControllerInstanceManager"))
			{
				PrintDebugLog("WaveVR_ControllerInstanceManager is found in scene!");
				CtrInstanceMgr = go.GetComponent<WaveVR_ControllerInstanceManager>();
				break;
			}
		}

		if (CtrInstanceMgr == null)
		{
			PrintDebugLog("controllerInstanceManager is NOT found in scene! create it.");

			CtrInstanceMgr = WaveVR_ControllerInstanceManager.Instance;
		}
	}

	void OnEnable()
	{
		resetControllerState();

		if (WhichHand == ControllerHand.Dominant)
		{
			this.deviceType = WaveVR_Controller.EDeviceType.Dominant;
			this.modelSpecify = ModelSpecify.MS_Dominant;
}
		else
		{
			this.deviceType = WaveVR_Controller.EDeviceType.NonDominant;
			this.modelSpecify = ModelSpecify.MS_NonDominant;
		}

		WaveVR_Utils.Event.Listen(WaveVR_Utils.Event.DEVICE_CONNECTED, onDeviceConnected);
	}

	void resetControllerState()
	{
		controllerPrefab = null;
		controllerFileName = "";
		mLoadingState = CLoadingState.LoadingState_NOT_LOADED;
		genericControllerFileName = "Generic_";
		renderModelName = "";
		ControllerIdx = 0;
		this.connected = false;
	}

	void OnDisable()
	{
		WaveVR_Utils.Event.Remove(WaveVR_Utils.Event.DEVICE_CONNECTED, onDeviceConnected);
	}

	void OnDestroy()
	{
		PrintDebugLog("OnDestroy");
		removeControllerFromMgr("OnDestroy()");
	}

	// Use this for initialization
	void Start()
	{
		loadedAssetBundle.Clear();
		if (checkConnection () != connected)
			connected = !connected;

		if (connected)
		{
			if (WaveVR.Instance.Initialized)
			{
				WaveVR.Device _device = WaveVR.Instance.getDeviceByType(this.deviceType);
				onLoadController(_device.type);
			}
		}

		WaveVR_EventSystemControllerProvider.Instance.MarkControllerLoader (deviceType, true);
		forceCheckRenderModelName = true;
	}

	private void onDeviceConnected(params object[] args)
	{
		WVR_DeviceType _type = (WVR_DeviceType)args [0];
		bool _connected = (bool)args [1];
		PrintDebugLog ("onDeviceConnected() device " + _type + " is " + (_connected ? "connected." : "disconnected."));

		if (_type != WaveVR_Controller.Input (this.deviceType).DeviceType)
			return;

		this.connected = _connected;

		if (this.connected)
		{
			if (controllerPrefab == null)
				onLoadController (_type);
		}
	}

	private void removeControllerFromMgr(string funcName)
	{
		if (CtrInstanceMgr != null)
		{
			if (ControllerIdx != 0)
			{
				CtrInstanceMgr.removeControllerInstance(ControllerIdx);
				PrintDebugLog(funcName + " remove controller: " + ControllerIdx);
				ControllerIdx = 0;
			}
		}
	}

	private void UpdateFocusController()
	{
		WVR_DeviceType _dev = Interop.WVR_GetFocusedController ();
		if (_dev == WVR_DeviceType.WVR_DeviceType_Controller_Right)
			focusController = WaveVR_Controller.EDeviceType.Dominant;
		if (_dev == WVR_DeviceType.WVR_DeviceType_Controller_Left)
			focusController = WaveVR_Controller.EDeviceType.NonDominant;
	}

	private void onLoadController(WVR_DeviceType type)
	{
		if (mLoadingState != CLoadingState.LoadingState_NOT_LOADED)
		{
			PrintDebugLog("Controller model is loading or already loaded! state: " + mLoadingState);
			return;
		}

		mLoadingState = CLoadingState.LoadingState_LOADING;
		controllerFileName = "";
		controllerModelFoler = "Controller/";
		genericControllerFileName = "Generic_";

		// Make up file name
		// Rule =
		// ControllerModel_TrackingMethod_CComponent_Hand

		checkAndCreateCIM();
		if (renderModelName == "")
		{
			ptrParameterName = Marshal.StringToHGlobalAnsi(parameterName);
			ptrResult = Marshal.AllocHGlobal(bufferSize);
			//zero out buffer
			for (int i = 0; i < bufferSize; i++)
			{
				Marshal.WriteByte(ptrResult, i, 0);
			}
			uint ret = Interop.WVR_GetParameters(type, ptrParameterName, ptrResult, resultVertLength);
			string tmpName = Marshal.PtrToStringAnsi(ptrResult);
			Marshal.FreeHGlobal(ptrParameterName);
			Marshal.FreeHGlobal(ptrResult);

			if (ret > 0)
			{
				renderModelName = tmpName;
			}
		}
		int deviceIndex = -1;
		string sparameterName = "backdoor_get_device_index";
		ptrParameterName = Marshal.StringToHGlobalAnsi(sparameterName);
		IntPtr ptrResultDeviceIndex = Marshal.AllocHGlobal(2);
		Marshal.WriteByte(ptrResultDeviceIndex, 0, 0);
		Interop.WVR_GetParameters(type, ptrParameterName, ptrResultDeviceIndex, 2);

		int _out = 0;
		bool _ret = int.TryParse (Marshal.PtrToStringAnsi (ptrResultDeviceIndex), out _out);
		if (_ret)
			deviceIndex = _out;
		Marshal.FreeHGlobal(ptrParameterName);
		Marshal.FreeHGlobal(ptrResultDeviceIndex);

		PrintInfoLog("get controller id from runtime is " + renderModelName);

		controllerFileName += renderModelName;
		controllerFileName += "_";

		if (ControllerComponents == CComponent.Multi_Component)
		{
			controllerFileName += "MC_";
		}
		else
		{
			controllerFileName += "OB_";
		}

		if (WhichHand == ControllerHand.Dominant)
		{
			controllerFileName += "R";
		}
		else
		{
			controllerFileName += "L";
		}

		PrintInfoLog("controller file name is " + controllerFileName);
		var found = false;
		controllerType = ControllerType.ControllerType_None;

		if (adaptiveLoading)
		{
			if (Interop.WVR_GetWaveRuntimeVersion() >= 2)
			{
				PrintInfoLog("Start adaptive loading");
				// 1. check if there are assets in private folder
				string renderModelUnzipFolder = Interop.WVR_DeployRenderModelAssets(deviceIndex, renderModelName);

				// load model from runtime
				if (renderModelUnzipFolder != "") {
					// try emitter folder
					string modelPath = renderModelUnzipFolder + "Model";
					found = loadMeshAndImageByDevice(modelPath);

					if (found)
					{
						PrintInfoLog("Model FBX is found!");
					}

					if (!found)
					{
						string UnityVersion = Application.unityVersion;
						PrintInfoLog("Application built by Unity version : " + UnityVersion);
						renderModelNamePath = renderModelUnzipFolder + "Unity";

						int assetVersion = checkAssetBundlesVersion(UnityVersion);

						if (assetVersion == 1)
						{
							renderModelNamePath += "/5.6";
						}
						else if (assetVersion == 2)
						{
							renderModelNamePath += "/2017.3";
						}

						// try root path
						found = tryLoadModelFromRuntime(renderModelNamePath, controllerFileName);

						// try to load generic from runtime
						if (!found)
						{
							PrintInfoLog("Try to load generic controller model from runtime");
							string tmpGeneric = genericControllerFileName;
							if (WhichHand == ControllerHand.Dominant)
							{
								tmpGeneric += "MC_R";
							}
							else
							{
								tmpGeneric += "MC_L";
							}
							found = tryLoadModelFromRuntime(renderModelNamePath, tmpGeneric);
						}
					}
				}
			} else
			{
				PrintInfoLog("API Level(2) is larger than Runtime Version (" + Interop.WVR_GetWaveRuntimeVersion() + ")");
			}
		} else
		{
			PrintInfoLog("Disable adaptive loading, use package resource loading");
		}

		// load model from package
		if (!found)
		{
			PrintWarningLog("Start package resource loading");
			originalControllerPrefab = Resources.Load(controllerModelFoler + controllerFileName) as GameObject;
			if (originalControllerPrefab == null)
			{
				Log.e(LOG_TAG, "Can't load preferred controller model from package: " + controllerFileName);
			}
			else
			{
				PrintInfoLog(controllerFileName + " controller model is found!");
				controllerType = ControllerType.ControllerType_Resources;
				found = true;
			}
		}

		// Nothing exist, load generic
		if (!found)
		{
			PrintInfoLog(controllerFileName + " controller model is not found from runtime and package!");

			originalControllerPrefab = loadGenericControllerModelFromPackage(genericControllerFileName);
			if (originalControllerPrefab == null)
			{
				Log.e(LOG_TAG, "Can't load generic controller model, Please check file under Resources/" + controllerModelFoler + genericControllerFileName + ".prefab is exist!");
			}
			else
			{
				PrintInfoLog(genericControllerFileName + " controller model is found!");
				controllerType = ControllerType.ControllerType_Generic;
				found = true;
			}
		}

		if (found && (originalControllerPrefab != null))
		{
			UpdateFocusController ();
			PrintInfoLog("Instantiate controller model, controller type: " + controllerType + ", focus: " + focusController);
			SetControllerOptions(originalControllerPrefab);
			if (controllerType == ControllerType.ControllerType_AdaptiveController) PresetAdaptiveControllerParameters(originalControllerPrefab);
			SetControllerBeamParameters (originalControllerPrefab);
			SetControllerPointerParameters (originalControllerPrefab);
			controllerPrefab = Instantiate(originalControllerPrefab);
			controllerPrefab.transform.parent = this.transform.parent;
			ApplyIndicatorParameters();

			WaveVR_EventSystemControllerProvider.Instance.SetControllerModel(deviceType, controllerPrefab);

			if (controllerType == ControllerType.ControllerType_AdaptiveController) setEventSystemParameter();

			if (CtrInstanceMgr != null)
			{
				// To sync with overlay, the Dominant is always right, NonDominant is always left.
				WVR_DeviceType _type = this.deviceType == WaveVR_Controller.EDeviceType.Dominant ?
					WVR_DeviceType.WVR_DeviceType_Controller_Right : WVR_DeviceType.WVR_DeviceType_Controller_Left;
				ControllerIdx = CtrInstanceMgr.registerControllerInstance(_type, controllerPrefab);
				PrintDebugLog("onLoadController() controller index: " + ControllerIdx);
			}
			PrintDebugLog("onLoadController() broadcast " + this.deviceType + " CONTROLLER_MODEL_LOADED");
			WaveVR_Utils.Event.Send(WaveVR_Utils.Event.CONTROLLER_MODEL_LOADED, this.deviceType, controllerPrefab);
#if UNITY_EDITOR
			if (onControllerModelLoaded != null)
			{
				PrintDebugLog("ControllerModelLoaded trigger delegate");
				onControllerModelLoaded(controllerPrefab);
			}
#endif
			mLoadingState = CLoadingState.LoadingState_LOADED;
		}

		if (adaptiveLoading && controllerType == ControllerType.ControllerType_AssetBundles)
		{
			PrintInfoLog("loadedAssetBundle length: " + loadedAssetBundle.Count);
			foreach (AssetBundle tmpAB in loadedAssetBundle)
			{
				tmpAB.Unload(false);
			}
			loadedAssetBundle.Clear();
		}

		Resources.UnloadUnusedAssets();
		System.GC.Collect();
	}

	private bool loadMeshAndImageByDevice(string renderModelNamePath)
	{
		string FBXFile = renderModelNamePath + "/";
		string imageFile = renderModelNamePath + "/";

		FBXFile += "controller00.fbx";
		imageFile += "controller00.png";

		bool ret = false;

		ret = File.Exists(FBXFile);
		PrintInfoLog("FBX exist = " + ret);

		if (ret)
		{
			bool fileExist = File.Exists(imageFile);
			PrintInfoLog("PNG exist: " + fileExist);
			ret = fileExist;

			if (ret)
			{
				originalControllerPrefab = Resources.Load("AdaptiveController") as GameObject;
				ret = (originalControllerPrefab != null) ? true : false;
			} else
			{
				WaveVR_Utils.Event.Send(WaveVR_Utils.Event.DS_ASSETS_NOT_FOUND, this.deviceType);
			}
		} else
		{
			WaveVR_Utils.Event.Send(WaveVR_Utils.Event.DS_ASSETS_NOT_FOUND, this.deviceType);
		}
		PrintInfoLog("Files and prefab are ready: " + ret);

		if (ret)
		{
			controllerType = ControllerType.ControllerType_AdaptiveController;
			bool modelRet = false;

			modelRet = WaveVR_ControllerResourceHolder.Instance.addRenderModel(renderModelName, renderModelNamePath, modelSpecify, true);
			if (modelRet)
			{
				PrintInfoLog("Added " + modelSpecify + " render model, name: " + renderModelName);
			} else
			{
				PrintInfoLog(renderModelName + " adding " + modelSpecify + " model failure!");

				modelRet = WaveVR_ControllerResourceHolder.Instance.addRenderModel(renderModelName, renderModelNamePath, ModelSpecify.MS_Dominant, true);
				if (modelRet)
				{
					PrintInfoLog("Added MS_Dominant render model for name: " + renderModelName + " cant find " + modelSpecify + " model");
				} else
				{
					PrintInfoLog(renderModelName + ", " + modelSpecify + " will use MS_Dominant model!");
				}
			}
		}
		return ret;
	}

	// used for asset bundles
	private bool tryLoadModelFromRuntime(string renderModelNamePath, string modelName)
	{
		string renderModelAssetBundle = renderModelNamePath + "/" + "Unity";
		PrintInfoLog("tryLoadModelFromRuntime, path is " + renderModelAssetBundle);
		// clear unused asset bundles
		foreach (AssetBundle tmpAB in loadedAssetBundle)
		{
			tmpAB.Unload(false);
		}
		loadedAssetBundle.Clear();
		// check root folder
		AssetBundle ab = AssetBundle.LoadFromFile(renderModelAssetBundle);
		if (ab != null)
		{
			loadedAssetBundle.Add(ab);
			AssetBundleManifest abm = ab.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

			if (abm != null)
			{
				PrintDebugLog(renderModelAssetBundle + " loaded");
				string[] assetsName = abm.GetAllAssetBundles();

				for (int i = 0; i < assetsName.Length; i++)
				{
					string subRMAsset = renderModelNamePath + "/" + assetsName[i];
					ab = AssetBundle.LoadFromFile(subRMAsset);

					loadedAssetBundle.Add(ab);
					PrintDebugLog(subRMAsset + " loaded");
				}
				PrintInfoLog("All asset Bundles loaded, start loading asset");
				originalControllerPrefab = ab.LoadAsset<GameObject>(modelName);

				if (originalControllerPrefab != null)
				{
					if (verifyControllerPrefab(originalControllerPrefab))
					{
						PrintInfoLog("adaptive load controller model " + modelName + " success");
						controllerType = ControllerType.ControllerType_AssetBundles;
						return true;
					}
				}
			}
			else
			{
				PrintWarningLog("Can't find AssetBundleManifest!!");
			}
		}
		else
		{
			PrintWarningLog("Load " + renderModelAssetBundle + " failed");
		}
		PrintInfoLog("adaptive load controller model " + modelName + " from " + renderModelNamePath + " fail!");
		return false;
	}

	private bool verifyControllerPrefab(GameObject go)
	{
		bool ret = true;

		if (renderModelName.StartsWith("WVR_CONTROLLER_ASPEN") || renderModelName.StartsWith("WVR_CONTROLLER_FINCH3DOF"))
			ret = false;

		/*PrintInfoLog(go.name + " active: " + go.activeInHierarchy);

		WaveVR_Beam wb = go.GetComponent<WaveVR_Beam>();

		if (wb != null)
			return true;

		WaveVR_ControllerPointer wcp = go.GetComponent<WaveVR_ControllerPointer>();

		if (wcp != null)
			return true;

		MeshRenderer mr = go.GetComponent<MeshRenderer>();

		if (mr != null)
		{
			foreach (Material mat in mr.materials)
			{
				if (mat == null)
				{
					PrintWarningLog(go.name + " material is null");
					ret = false;
				} else
				{
					if (mat.shader == null)
					{
						PrintWarningLog(go.name + " shader is null");
						ret = false;
					} else if (mat.mainTexture == null)
					{
						PrintWarningLog(go.name + " texture is null");
						ret = false;
					}
				}
			}
		}

		if (ret)
		{
			var ch = go.transform.childCount;

			for (int i = 0; i < ch; i++)
			{
				ret = verifyControllerPrefab(go.transform.GetChild(i).gameObject);
				if (!ret) break;
			}
		}*/

		return ret;
	}

	// used for asset bundles
	private int checkAssetBundlesVersion(string version)
	{
		if (version.StartsWith("5.6.3") || version.StartsWith("5.6.4") || version.StartsWith("5.6.5") || version.StartsWith("5.6.6") || version.StartsWith("2017.1") || version.StartsWith("2017.2"))
		{
			return 1;
		}

		if (version.StartsWith("2017.3") || version.StartsWith("2017.4") || version.StartsWith("2018.1"))
		{
			return 2;
		}

		return 0;
	}

	private GameObject loadGenericControllerModelFromPackage(string tmpGeneric)
	{
		if (WhichHand == ControllerHand.Dominant)
		{
			tmpGeneric += "MC_R";
		}
		else
		{
			tmpGeneric += "MC_L";
		}
		Log.w(LOG_TAG, "Can't find preferred controller model, load generic controller : " + tmpGeneric);
		if (adaptiveLoading) PrintInfoLog("Please update controller models from device service to have better experience!");
		return Resources.Load(controllerModelFoler + tmpGeneric) as GameObject;
	}

	private void SetControllerOptions(GameObject controller_prefab)
	{
		WaveVR_PoseTrackerManager _ptm = controller_prefab.GetComponent<WaveVR_PoseTrackerManager> ();
		if (_ptm != null)
		{
			_ptm.TrackPosition = TrackPosition;
			_ptm.SimulationOption = SimulationOption;
			_ptm.FollowHead = FollowHead;
			_ptm.TrackRotation = TrackRotation;
			_ptm.Type = this.deviceType;
			PrintInfoLog("set " + this.deviceType + " to WaveVR_PoseTrackerManager");
		}
	}

	void OnApplicationPause(bool pauseStatus)
	{
		if (!pauseStatus) // resume
		{
			PrintInfoLog("App resume and forceCheckRenderModelName");
			forceCheckRenderModelName = true;
		}
	}

	// Update is called once per frame
	void Update () {
		if (Interop.WVR_IsInputFocusCapturedBySystem()) {
			if (mLoadingState == CLoadingState.LoadingState_NOT_LOADED)
			{
				WVR_DeviceType type = WaveVR_Controller.Input(this.deviceType).DeviceType;
				if (WaveVR_Controller.Input(type).connected)
				{
					ptrParameterName = Marshal.StringToHGlobalAnsi(parameterName);
					ptrResult = Marshal.AllocHGlobal(bufferSize);
					//zero out buffer
					for (int i = 0; i < bufferSize; i++)
					{
						Marshal.WriteByte(ptrResult, i, 0);
					}
					uint ret = Interop.WVR_GetParameters(type, ptrParameterName, ptrResult, resultVertLength);
					string tmpName = Marshal.PtrToStringAnsi(ptrResult);
					Marshal.FreeHGlobal(ptrParameterName);
					Marshal.FreeHGlobal(ptrResult);

					if (ret > 0)
					{
						renderModelName = tmpName;
						PrintInfoLog("Load controller in background");
						onLoadController(type);
					}
				}
			}
		}

		if (mLoadingState == CLoadingState.LoadingState_LOADED)
		{
			checkConnectionAndModelName();
		}
	}

	private void checkConnectionAndModelName()
	{
		if (controllerPrefab == null) return;

		bool _connected = WaveVR_Controller.Input(this.deviceType).connected;
		WVR_DeviceType _type = WaveVR_Controller.Input(this.deviceType).DeviceType;

		if (_connected != lastFrameConnection || forceCheckRenderModelName) {
			PrintInfoLog("connection state changed or force check!");
			if (_connected)
			{
				PrintInfoLog("Check render model name when controller is connected");
				ptrParameterName = Marshal.StringToHGlobalAnsi(parameterName);
				ptrResult = Marshal.AllocHGlobal(bufferSize);
				//zero out buffer
				for (int i = 0; i < bufferSize; i++)
				{
					Marshal.WriteByte(ptrResult, i, 0);
				}

				uint ret = Interop.WVR_GetParameters(_type, ptrParameterName, ptrResult, resultVertLength);
				string tmprenderModelName = Marshal.PtrToStringAnsi(ptrResult);
				PrintInfoLog("previous render model name:" + renderModelName);
				PrintInfoLog("render model name from runtime:" + tmprenderModelName);
				Marshal.FreeHGlobal(ptrParameterName);
				Marshal.FreeHGlobal(ptrResult);

				if (ret > 0 && (!tmprenderModelName.Equals(renderModelName)))
				{
					renderModelName = tmprenderModelName;
					PrintDebugLog("Render model change device: " + _type + ", new render model name: " + renderModelName);
					PrintInfoLog("Destroy controller prefeb because render model is changed, broadcast " + this.deviceType + " CONTROLLER_MODEL_UNLOADED");
					removeControllerFromMgr("DeleteControllerWhenDisconnect()");
					Destroy(controllerPrefab);
					resetControllerState();
					WaveVR_Utils.Event.Send(WaveVR_Utils.Event.CONTROLLER_MODEL_UNLOADED, this.deviceType);
					Resources.UnloadUnusedAssets();
					System.GC.Collect();

					if (_connected)
					{
						this.connected = _connected;
						onLoadController(_type);
					}
				}

				forceCheckRenderModelName = false;
			}
			else
			{
				PrintInfoLog("Destroy controller prefeb because it is disconnect, broadcast " + this.deviceType + " CONTROLLER_MODEL_UNLOADED");
				removeControllerFromMgr("DeleteControllerWhenDisconnect()");
				Destroy(controllerPrefab);
				resetControllerState();
				controllerPrefab = null;
				WaveVR_Utils.Event.Send(WaveVR_Utils.Event.CONTROLLER_MODEL_UNLOADED, this.deviceType);
				Resources.UnloadUnusedAssets();
				System.GC.Collect();
			}

			lastFrameConnection = _connected;
		}
	}

	private bool checkConnection()
	{
		if (!WaveVR.Instance.Initialized)
			return false;
		WaveVR.Device _device = WaveVR.Instance.getDeviceByType (this.deviceType);
		return _device.connected;
	}

	private void SetControllerBeamParameters(GameObject ctrlr)
	{
		if (ctrlr == null)
			return;

		WaveVR_Beam _wb = ctrlr.GetComponentInChildren<WaveVR_Beam> ();
		if (_wb != null)
		{
			_wb.ShowBeam = (
			    this.enableEmitter &&
			    this.ShowBeam
			);

			if (this.useBeamSystemConfig)
				ReadJsonValues_Beam ();

			_wb.ListenToDevice = true;
			_wb.device = this.deviceType;

			_wb.updateEveryFrame = this.updateEveryFrame;
			_wb.StartWidth = this.StartWidth;
			_wb.EndWidth = this.EndWidth;
			_wb.StartOffset = this.StartOffset;
			_wb.EndOffset = this.EndOffset;
			_wb.StartColor = this.StartColor;
			_wb.EndColor = this.EndColor;
			_wb.useDefaultMaterial = this.useDefaultMaterial;
			_wb.customMat = this.customMat;

			PrintDebugLog ("SetControllerBeamParameters() Beam ->show: " + _wb.ShowBeam
			+ ", ListenToDevice: " + _wb.ListenToDevice
			+ ", device: " + _wb.device
			+ ", use system config: " + this.useBeamSystemConfig
			+ ", updateEveryFrame: " + _wb.updateEveryFrame
			+ ", StartWidth: " + _wb.StartWidth
			+ ", EndWidth: " + _wb.EndWidth
			+ ", StartOffset: " + _wb.StartOffset
			+ ", EndOffset: " + _wb.EndOffset
			+ ", StartColor: " + _wb.StartColor
			+ ", EndColor: " + _wb.EndColor
			+ ", useDefaultMaterial: " + _wb.useDefaultMaterial
			+ ", customMat: " + _wb.customMat);
		}
	}

	private void SetControllerPointerParameters(GameObject ctrlr)
	{
		if (ctrlr == null)
			return;

		WaveVR_ControllerPointer _wcp = ctrlr.GetComponentInChildren<WaveVR_ControllerPointer> ();
		if (_wcp != null)
		{
			_wcp.ShowPointer = (
			    this.enableEmitter &&
			    this.showPointer
			);

			if (this.usePointerSystemConfig)
				ReadJsonValues_Pointer ();

			_wcp.device = this.deviceType;
			_wcp.Blink = this.Blink;
			_wcp.useTexture = this.useTexture;
			_wcp.PointerOuterDiameterMin = this.PointerOuterDiameterMin;
			_wcp.PointerDistanceInMeters = this.PointerDistanceInMeters;
			_wcp.UseDefaultTexture = this.UseDefaultTexture;
			_wcp.CustomTexture = this.customTexture;
			_wcp.TextureName = this.TextureName;
			_wcp.PointerColor = this.PointerColor;
			_wcp.borderColor = this.borderColor;
			_wcp.focusColor = this.focusColor;
			_wcp.focusBorderColor = this.focusBorderColor;

			PrintDebugLog ("Pointer -> show: " + _wcp.ShowPointer
				+ ", device: " + _wcp.device
				+ ", useSystemConfig: " + this.usePointerSystemConfig
				+ ", Blink: " + _wcp.Blink
				+ ", useTexture: " + _wcp.useTexture
				+ ", PointerOuterDiameterMin: " + _wcp.PointerOuterDiameterMin
				+ ", UseDefaultTexture: " + _wcp.UseDefaultTexture
				+ ", customTexture: " + _wcp.CustomTexture);
		}
	}

	#region OEM CONFIG
	private void UpdateStartColor(string color_string)
	{
		byte[] _color_r = BitConverter.GetBytes(Convert.ToInt32(color_string.Substring(1, 2), 16));
		byte[] _color_g = BitConverter.GetBytes(Convert.ToInt32(color_string.Substring(3, 2), 16));
		byte[] _color_b = BitConverter.GetBytes(Convert.ToInt32(color_string.Substring(5, 2), 16));
		byte[] _color_a = BitConverter.GetBytes(Convert.ToInt32(color_string.Substring(7, 2), 16));

		this.StartColor.r = _color_r [0];
		this.StartColor.g = _color_g [0];
		this.StartColor.b = _color_b [0];
		this.StartColor.a = _color_a [0];
	}

	private void UpdateEndColor(string color_string)
	{
		byte[] _color_r = BitConverter.GetBytes(Convert.ToInt32(color_string.Substring(1, 2), 16));
		byte[] _color_g = BitConverter.GetBytes(Convert.ToInt32(color_string.Substring(3, 2), 16));
		byte[] _color_b = BitConverter.GetBytes(Convert.ToInt32(color_string.Substring(5, 2), 16));
		byte[] _color_a = BitConverter.GetBytes(Convert.ToInt32(color_string.Substring(7, 2), 16));

		this.EndColor.r = _color_r [0];
		this.EndColor.g = _color_g [0];
		this.EndColor.b = _color_b [0];
		this.EndColor.a = _color_a [0];
	}

	/**
	 * OEM Config
	 * \"beam\": {
	   \"start_width\": 0.000625,
	   \"end_width\": 0.00125,
	   \"start_offset\": 0.015,
	   \"length\":  0.8,
	   \"start_color\": \"#FFFFFFFF\",
	   \"end_color\": \"#FFFFFF4D\"
	   },
	 **/
	private void ReadJsonValues_Beam()
	{
		string json_values = WaveVR_Utils.OEMConfig.getControllerConfig ();

		if (!json_values.Equals (""))
		{
			SimpleJSON.JSONNode jsNodes = SimpleJSON.JSONNode.Parse (json_values);

			string node_value = "";
			node_value = jsNodes ["beam"] ["start_width"].Value;
			if (!node_value.Equals (""))
				this.StartWidth = float.Parse (node_value);

			node_value = jsNodes ["beam"] ["end_width"].Value;
			if (!node_value.Equals (""))
				this.EndWidth = float.Parse (node_value);

			node_value = jsNodes ["beam"] ["start_offset"].Value;
			if (!node_value.Equals (""))
				this.StartOffset = float.Parse (node_value);

			node_value = jsNodes ["beam"] ["length"].Value;
			if (!node_value.Equals (""))
				this.EndOffset = float.Parse (node_value);

			node_value = jsNodes ["beam"] ["start_color"].Value;
			if (!node_value.Equals (""))
				UpdateStartColor (node_value);

			node_value = jsNodes ["beam"] ["end_color"].Value;
			if (!node_value.Equals (""))
				UpdateEndColor (node_value);
		}
	}

	/**
	 * OEM Config
	 * \"pointer\": {
	   \"diameter\": 0.01,
	   \"distance\": 1.3,
	   \"use_texture\": true,
	   \"color\": \"#FFFFFFFF\",
	   \"border_color\": \"#777777FF\",
	   \"focus_color\": \"#FFFFFFFF\",
	   \"focus_border_color\": \"#777777FF\",
	   \"texture_name\":  null,
	   \"Blink\": false
	   },
	 **/
	private void ReadJsonValues_Pointer()
	{
		string json_values = WaveVR_Utils.OEMConfig.getControllerConfig ();

		if (!json_values.Equals (""))
		{
			try
			{
				SimpleJSON.JSONNode jsNodes = SimpleJSON.JSONNode.Parse (json_values);
				string node_value = "";
				node_value = jsNodes ["pointer"] ["diameter"].Value;
				if (!node_value.Equals ("") && IsFloat (node_value) == true)
					this.PointerOuterDiameterMin = float.Parse (node_value);
				/* Ignore OEM CONFIG "distance" due to it is only used by overlay with default value "5"
				node_value = jsNodes ["pointer"] ["distance"].Value;
				if (!node_value.Equals ("") && IsFloat (node_value) == true)
					this.PointerDistanceInMeters = float.Parse (node_value);
				*/
				node_value = jsNodes ["pointer"] ["use_texture"].Value;
				if (!node_value.Equals ("") && IsBoolean (node_value) == true)
					this.useTexture = bool.Parse (node_value);

				node_value = jsNodes ["pointer"] ["color"].Value;
				if (!node_value.Equals (""))
					this.PointerColor = StringToColor32 (node_value, 0);

				node_value = jsNodes ["pointer"] ["border_color"].Value;
				if (!node_value.Equals (""))
					this.borderColor = StringToColor32 (node_value, 1);

				node_value = jsNodes ["pointer"] ["focus_color"].Value;
				if (!node_value.Equals (""))
					this.focusColor = StringToColor32 (node_value, 2);

				node_value = jsNodes ["pointer"] ["focus_border_color"].Value;
				if (!node_value.Equals (""))
					this.focusBorderColor = StringToColor32 (node_value, 3);
				node_value = jsNodes ["pointer"] ["pointer_texture_name"].Value;
				if (!node_value.Equals (""))
					this.TextureName = node_value;


				node_value = jsNodes ["pointer"] ["Blink"].Value;
				if (!node_value.Equals ("") && IsBoolean (node_value) == true)
					this.Blink = bool.Parse (node_value);

				PrintDebugLog ("ReadJsonValues() diameter: " + this.PointerOuterDiameterMin
				+ ", distance: " + this.PointerDistanceInMeters
				+ ", use_texture: " + this.useTexture
				+ ", color: " + this.PointerColor
				+ ", pointer_texture_name: " + this.TextureName
				+ ", Blink: " + this.Blink);
			} catch (Exception e)
			{
				Log.e (LOG_TAG, e.ToString ());
			}
		}
	}

	private static bool IsFloat(string value)
	{
		try
		{
			float i = Convert.ToSingle(value);
			Log.d(LOG_TAG, value + " Convert to float success: " + i.ToString());
			return true;
		}
		catch (Exception e)
		{
			Log.e(LOG_TAG, value + " Convert to float failed: " + e.ToString());
			return false;
		}
	}

	private static bool IsBoolean(string value)
	{
		try
		{
			bool i = Convert.ToBoolean(value);
			Log.d(LOG_TAG, value + " Convert to bool success: " + i.ToString());
			return true;
		}
		catch (Exception e)
		{
			Log.e(LOG_TAG, value + " Convert to bool failed: " + e.ToString());
			return false;
		}
	}

	private Color32 StringToColor32(string color_string , int value)
	{
		try
		{
			byte[] _color_r = BitConverter.GetBytes(Convert.ToInt32(color_string.Substring(1, 2), 16));
			byte[] _color_g = BitConverter.GetBytes(Convert.ToInt32(color_string.Substring(3, 2), 16));
			byte[] _color_b = BitConverter.GetBytes(Convert.ToInt32(color_string.Substring(5, 2), 16));
			byte[] _color_a = BitConverter.GetBytes(Convert.ToInt32(color_string.Substring(7, 2), 16));

			return new Color32(_color_r[0], _color_g[0], _color_b[0], _color_a[0]);
		}
		catch (Exception e)
		{
			Log.e(LOG_TAG, "StringToColor32: " + e.ToString());
			switch (value)
			{
			case 1:
				return new Color(119, 119, 119, 255);
			case 2:
				return new Color(255, 255, 255, 255);
			case 3:
				return new Color(119, 119, 119, 255);
			}
			return Color.white;
		}
	}
	#endregion

	private void PresetAdaptiveControllerParameters(GameObject ctrPrefab)
	{
		WaveVR_ControllerRootToEmitter macr = ctrPrefab.GetComponent<WaveVR_ControllerRootToEmitter>();
		if (macr != null)
		{
			PrintInfoLog("set WaveVR_ControllerRootToEmitter deviceType to " + this.deviceType);
			macr.deviceType = this.deviceType;
		} else
		{
			PrintInfoLog("No WaveVR_ControllerRootToEmitter!");
		}

		var ch = ctrPrefab.transform.childCount;

		for (int i = 0; i < ch; i++)
		{
			PrintInfoLog(ctrPrefab.transform.GetChild(i).gameObject.name);

			// get model
			if (ctrPrefab.transform.GetChild(i).gameObject.name == "Model")
			{
				GameObject CM = ctrPrefab.transform.GetChild(i).gameObject;

				WaveVR_RenderModel rm = CM.GetComponent<WaveVR_RenderModel>();

				if (rm != null)
				{
					rm.WhichHand = (WaveVR_RenderModel.ControllerHand)this.WhichHand;
					rm.updateDynamically = false;
					rm.mergeToOneBone = true;

					PrintDebugLog("Model -> WhichHand: " + rm.WhichHand);
				}

				WaveVR_AdaptiveControllerActions aca = CM.GetComponent<WaveVR_AdaptiveControllerActions>();

				if (aca != null)
				{
					aca.enableButtonEffect = this.enableButtonEffect;
					if (aca.enableButtonEffect)
					{
						PrintInfoLog("AdaptiveController button effect is active");
						aca.device = this.deviceType;
						aca.useSystemConfig = this.useEffectSystemConfig;
						if (!this.useEffectSystemConfig) aca.buttonEffectColor = this.buttonEffectColor;

						PrintDebugLog("Effect -> device: " + aca.device
							+ ", useSystemConfig: " + aca.useSystemConfig
							+ "buttonEffectColor" + aca.buttonEffectColor);
					}
					aca.collectInStart = false;
				}
			}
		}
	}

	private GameObject eventSystem = null;
	private void setEventSystemParameter()
	{
		if (EventSystem.current == null)
		{
			EventSystem _es = FindObjectOfType<EventSystem>();
			if (_es != null)
			{
				eventSystem = _es.gameObject;
			}
		}
		else
		{
			eventSystem = EventSystem.current.gameObject;
		}

		if (eventSystem != null)
		{
			WaveVR_ControllerInputModule wcim = eventSystem.GetComponent<WaveVR_ControllerInputModule>();

			if (wcim != null)
			{
				switch (this.deviceType)
				{
				case WaveVR_Controller.EDeviceType.Dominant:
					wcim.DomintEventEnabled = this.enableEmitter ? this.sendEvent : false;
					PrintInfoLog("Forced set RightEventEnabled to " + wcim.DomintEventEnabled);
					break;
				case WaveVR_Controller.EDeviceType.NonDominant:
					wcim.NoDomtEventEnabled = this.enableEmitter ? this.sendEvent : false;
					PrintInfoLog("Forced set LeftEventEnabled to " + wcim.NoDomtEventEnabled);
					break;
				default:
					break;
				}
			}
		}
	}

	private void ApplyIndicatorParameters()
	{
		if (!overwriteIndicatorSettings) return;
		WaveVR_ShowIndicator si = null;

		var ch = controllerPrefab.transform.childCount;
		bool found = false;

		for (int i = 0; i < ch; i++)
		{
			PrintInfoLog(controllerPrefab.transform.GetChild(i).gameObject.name);

			GameObject CM = controllerPrefab.transform.GetChild(i).gameObject;

			si = CM.GetComponentInChildren<WaveVR_ShowIndicator>();

			if (si != null)
			{
				found = true;
				break;
			}
		}

		if (found)
		{
			PrintInfoLog("WaveVR_ControllerLoader forced update WaveVR_ShowIndicator parameter!");
			si.showIndicator = this.showIndicator;

			if (showIndicator != true)
			{
				PrintInfoLog("WaveVR_ControllerLoader forced don't show WaveVR_ShowIndicator!");
				return;
			}
			si.showIndicator = this.showIndicator;
			si.showIndicatorAngle = showIndicatorAngle;
			si.hideIndicatorByRoll = hideIndicatorByRoll;
			si.basedOnEmitter = basedOnEmitter;
			si.lineColor = lineColor;
			si.lineEndWidth = lineEndWidth;
			si.lineStartWidth = lineStartWidth;
			si.lineLength = lineLength;
			si.textCharacterSize = textCharacterSize;
			si.zhCharactarSize = zhCharactarSize;
			si.textColor = textColor;
			si.textFontSize = textFontSize;

			si.buttonIndicationList.Clear();
			if (useIndicatorSystemConfig)
			{
				PrintInfoLog("WaveVR_ControllerLoader uses system default button indication!");
				addbuttonIndicationList();
			}
			else
			{
				PrintInfoLog("WaveVR_ControllerLoader uses customized button indication!");
				if (buttonIndicationList.Count == 0)
				{
					PrintInfoLog("WaveVR_ControllerLoader doesn't have button indication!");
					return;
				}
			}

			foreach (ButtonIndication bi in buttonIndicationList)
			{
				PrintInfoLog("use multilanguage: " + bi.useMultiLanguage);
				PrintInfoLog("indication: " + bi.indicationText);
				PrintInfoLog("alignment: " + bi.alignment);
				PrintInfoLog("offset: " + bi.indicationOffset);
				PrintInfoLog("keyType: " + bi.keyType);
				PrintInfoLog("followRotation: " + bi.followButtonRotation);

				si.buttonIndicationList.Add(bi);
			}

			si.createIndicator();
		} else
		{
			PrintInfoLog("Controller model doesn't support button indication feature!");
		}
	}

	private void addbuttonIndicationList()
	{
		buttonIndicationList.Clear();

		ButtonIndication home = new ButtonIndication();
		home.keyType = ButtonIndication.KeyIndicator.Home;
		home.alignment = ButtonIndication.Alignment.RIGHT;
		home.indicationOffset = new Vector3(0f, 0f, 0f);
		home.useMultiLanguage = true;
		home.indicationText = "system";
		home.followButtonRotation = true;

		buttonIndicationList.Add(home);

		ButtonIndication app = new ButtonIndication();
		app.keyType = ButtonIndication.KeyIndicator.App;
		app.alignment = ButtonIndication.Alignment.LEFT;
		app.indicationOffset = new Vector3(0f, 0.0004f, 0f);
		app.useMultiLanguage = true;
		app.indicationText = "system";
		app.followButtonRotation = true;

		buttonIndicationList.Add(app);

		ButtonIndication grip = new ButtonIndication();
		grip.keyType = ButtonIndication.KeyIndicator.Grip;
		grip.alignment = ButtonIndication.Alignment.RIGHT;
		grip.indicationOffset = new Vector3(0f, 0f, 0.01f);
		grip.useMultiLanguage = true;
		grip.indicationText = "system";
		grip.followButtonRotation = true;

		buttonIndicationList.Add(grip);

		ButtonIndication trigger = new ButtonIndication();
		trigger.keyType = ButtonIndication.KeyIndicator.Trigger;
		trigger.alignment = ButtonIndication.Alignment.RIGHT;
		trigger.indicationOffset = new Vector3(0f, 0f, 0f);
		trigger.useMultiLanguage = true;
		trigger.indicationText = "system";
		trigger.followButtonRotation = true;

		buttonIndicationList.Add(trigger);

		ButtonIndication dt = new ButtonIndication();
		dt.keyType = ButtonIndication.KeyIndicator.DigitalTrigger;
		dt.alignment = ButtonIndication.Alignment.RIGHT;
		dt.indicationOffset = new Vector3(0f, 0f, 0f);
		dt.useMultiLanguage = true;
		dt.indicationText = "system";
		dt.followButtonRotation = true;

		buttonIndicationList.Add(dt);

		ButtonIndication touchpad = new ButtonIndication();
		touchpad.keyType = ButtonIndication.KeyIndicator.TouchPad;
		touchpad.alignment = ButtonIndication.Alignment.LEFT;
		touchpad.indicationOffset = new Vector3(0f, 0f, 0f);
		touchpad.useMultiLanguage = true;
		touchpad.indicationText = "system";
		touchpad.followButtonRotation = true;

		buttonIndicationList.Add(touchpad);

		ButtonIndication vol = new ButtonIndication();
		vol.keyType = ButtonIndication.KeyIndicator.Volume;
		vol.alignment = ButtonIndication.Alignment.RIGHT;
		vol.indicationOffset = new Vector3(0f, 0f, 0f);
		vol.useMultiLanguage = true;
		vol.indicationText = "system";
		vol.followButtonRotation = true;

		buttonIndicationList.Add(vol);
	}
}
