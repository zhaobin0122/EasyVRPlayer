// "WaveVR SDK 
// © 2017 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the WaveVR SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using wvr;
using WVR_Log;
using UnityEngine.SceneManagement;
using UnityEngine.Profiling;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(WaveVR_InputModuleManager))]
public class WaveVR_InputModuleManagerEditor : Editor
{
	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		WaveVR_InputModuleManager myScript = target as WaveVR_InputModuleManager;

		EditorGUILayout.HelpBox("Select to enable input module.", MessageType.Info);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("EnableInputModule"), true);
		serializedObject.ApplyModifiedProperties();

		if (myScript.EnableInputModule)
		{
			EditorGUILayout.HelpBox ("If this checkbox is not selected, it will use system settings, otherwise inspector will popup items which you can define your preferred settings", MessageType.Warning);
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("OverrideSystemSettings"), true);
			serializedObject.ApplyModifiedProperties ();

			if (myScript.OverrideSystemSettings)
			{
				EditorGUILayout.HelpBox ("Choose input module.", MessageType.Info);
				EditorGUILayout.PropertyField (serializedObject.FindProperty ("CustomInputModule"), true);
				serializedObject.ApplyModifiedProperties ();

				if (myScript.CustomInputModule == WaveVR_EInputModule.Gaze)
				{
					EditorGUILayout.PropertyField (serializedObject.FindProperty ("Gaze"), true);
					serializedObject.ApplyModifiedProperties ();
					serializedObject.Update ();
				} else
				{
					EditorGUILayout.PropertyField (serializedObject.FindProperty ("Controller"), true);
					serializedObject.ApplyModifiedProperties ();

					if (myScript != null && myScript.Controller != null)
					{
						if (myScript.Controller.RaycastMode == WaveVR_ControllerInputModule.ERaycastMode.Fixed)
						{
							myScript.FixedBeamLength = (float)EditorGUILayout.FloatField ("Beam Length", myScript.FixedBeamLength);
						}
					}
					serializedObject.Update ();
				}
			}
		}

		if (GUI.changed)
			EditorUtility.SetDirty ((WaveVR_InputModuleManager)target);
	}
}
#endif

public enum WaveVR_EInputModule {
	Controller,
	Gaze
}

public class WaveVR_InputModuleManager : MonoBehaviour
{
	private const string LOG_TAG = "WaveVR_InputModuleManager";

	private void DEBUG(string msg)
	{
		if (Log.EnableDebugLog)
			Log.d (LOG_TAG, msg, true);
	}

	public bool EnableInputModule = true;
	public bool OverrideSystemSettings = false;
	public bool AutoGaze = false;
	public bool AlwaysShowController = false;
	public WaveVR_EInputModule CustomInputModule = WaveVR_EInputModule.Controller;

	#region Gaze parameters
	[System.Serializable]
	public class CGazeInputModule
	{
		public bool UseWaveVRReticle = false;
		public bool TimerControl = true;
		public float TimeToGaze = 2.0f;
		public bool ProgressRate = false;  // The switch to show how many percent to click by TimeToGaze
		public float RateTextZPosition = 0.5f;
		public bool ProgressCounter = false;  // The switch to show how long to click by TimeToGaze
		public float CounterTextZPosition = 0.5f;
		public WaveVR_GazeInputModule.EGazeInputEvent InputEvent = WaveVR_GazeInputModule.EGazeInputEvent.PointerSubmit;
		public bool ButtonControl = false;
		public List<WaveVR_Controller.EDeviceType> ButtonControlDevices = new List<WaveVR_Controller.EDeviceType>();
		public List<WaveVR_ButtonList.EButtons> ButtonControlKeys = new List<WaveVR_ButtonList.EButtons>();
		public GameObject Head = null;
	}

	public CGazeInputModule Gaze = new CGazeInputModule ();
	#endregion

	#region Controller Input Module parameters
	[System.Serializable]
	public class CControllerInputModule
	{
		public bool DominantEventEnabled = true;
		public GameObject DominantController = null;
		public LayerMask DominantRaycastMask = ~0;
		public bool NonDominantEventEnabled = true;
		public GameObject NonDominantController = null;
		public LayerMask NonDominantRaycastMask = ~0;
		public List<WaveVR_ButtonList.EButtons> ButtonToTrigger = new List<WaveVR_ButtonList.EButtons> ();
		public WaveVR_ControllerInputModule.ERaycastMode RaycastMode = WaveVR_ControllerInputModule.ERaycastMode.Mouse;
		public ERaycastStartPoint RaycastStartPoint = ERaycastStartPoint.CenterOfEyes;
		[Tooltip("Will be obsoleted soon!")]
		public string CanvasTag = "EventCanvas";
	}

	public CControllerInputModule Controller = new CControllerInputModule();
	#endregion
	public float FixedBeamLength = 9.5f;

	private static WaveVR_InputModuleManager instance = null;
	public static WaveVR_InputModuleManager Instance {
		get
		{
			return instance;
		}
	}

	#region Interaction Mode and Gaze Trigger Type
	private bool preOverrideSystemSettings = false;
	private WaveVR_EInputModule InteractionMode_User = WaveVR_EInputModule.Controller;
	private WVR_InteractionMode InteractionMode_System = WVR_InteractionMode.WVR_InteractionMode_Controller;
	private WVR_InteractionMode InteractionMode_Current = WVR_InteractionMode.WVR_InteractionMode_Controller;
	private WVR_GazeTriggerType gazeTriggerType_User = WVR_GazeTriggerType.WVR_GazeTriggerType_Timeout;
	private WVR_GazeTriggerType gazeTriggerType_System = WVR_GazeTriggerType.WVR_GazeTriggerType_Timeout;
	private WVR_GazeTriggerType gazeTriggerType_User_pre = WVR_GazeTriggerType.WVR_GazeTriggerType_Timeout;

	private void initInteractionModeAndGazeTriggerType()
	{
		this.preOverrideSystemSettings = this.OverrideSystemSettings;
		this.InteractionMode_User = this.CustomInputModule;
		updateGazeTriggerType_User ();  // set gazeTriggerType_User

		if (this.OverrideSystemSettings)
		{
			// Get default system settings.
			this.InteractionMode_System = WaveVR.Instance.InteractionMode;
			this.gazeTriggerType_System = WaveVR.Instance.GazeTriggerType;

			WVR_InteractionMode _mode = (this.InteractionMode_User == WaveVR_EInputModule.Controller) ?
					WVR_InteractionMode.WVR_InteractionMode_Controller : WVR_InteractionMode.WVR_InteractionMode_Gaze;

			// Change system settings to user settings.
			Interop.WVR_SetInteractionMode (_mode);
			Interop.WVR_SetGazeTriggerType (this.gazeTriggerType_User);

			this.gazeTriggerType_User_pre = this.gazeTriggerType_User;

			// Initialize by user settings.
			initializeInputModuleByCustomSettings ();
		} else
		{
			// Reset runtime settings to system default if no override.
			Interop.WVR_SetInteractionMode (WVR_InteractionMode.WVR_InteractionMode_SystemDefault);

			// Get default system settings.
			this.InteractionMode_System = WaveVR.Instance.InteractionMode;
			this.gazeTriggerType_System = WaveVR.Instance.GazeTriggerType;

			// Initialize by system settings.
			initializeInputModuleBySystemSetting ();
		}
		this.InteractionMode_Current = GetInteractionMode ();

		DEBUG ("initInteractionModeAndGazeTriggerType() OverrideSystemSettings: " + OverrideSystemSettings);
		DEBUG ("initInteractionModeAndGazeTriggerType() Interaction Mode - System: " + this.InteractionMode_System + ", User: " + this.InteractionMode_User + ", Current: " + this.InteractionMode_Current);
		DEBUG ("initInteractionModeAndGazeTriggerType() Gaze Trigger - System: " + this.gazeTriggerType_System + ", User: " + this.gazeTriggerType_User);
	}

	private void updateInteractionModeAndGazeTriggerType()
	{
		if (Log.gpl.Print)
		{
			DEBUG (this.OverrideSystemSettings ? "updateInteractionModeAndGazeTriggerType() OverrideSystemSettings is true." : "updateInteractionModeAndGazeTriggerType() OverrideSystemSettings is false.");
			DEBUG (InteractionMode_System == WVR_InteractionMode.WVR_InteractionMode_Gaze ? "updateInteractionModeAndGazeTriggerType() InteractionMode_System is gaze." : "updateInteractionModeAndGazeTriggerType() InteractionMode_System is controller.");
			DEBUG (InteractionMode_User == WaveVR_EInputModule.Controller ? "updateInteractionModeAndGazeTriggerType() InteractionMode_User is controller." : "updateInteractionModeAndGazeTriggerType() InteractionMode_User is gaze.");
			if (gazeTriggerType_System == WVR_GazeTriggerType.WVR_GazeTriggerType_Timeout)
				DEBUG ("updateInteractionModeAndGazeTriggerType() gazeTriggerType_System is timeout.");
			if (gazeTriggerType_System == WVR_GazeTriggerType.WVR_GazeTriggerType_Button)
				DEBUG ("updateInteractionModeAndGazeTriggerType() gazeTriggerType_System is button.");
			if (gazeTriggerType_System == WVR_GazeTriggerType.WVR_GazeTriggerType_TimeoutButton)
				DEBUG ("updateInteractionModeAndGazeTriggerType() gazeTriggerType_System is timeout & button.");
			if (gazeTriggerType_User == WVR_GazeTriggerType.WVR_GazeTriggerType_Timeout)
				DEBUG ("updateInteractionModeAndGazeTriggerType() gazeTriggerType_User is timeout.");
			if (gazeTriggerType_User == WVR_GazeTriggerType.WVR_GazeTriggerType_Button)
				DEBUG ("updateInteractionModeAndGazeTriggerType() gazeTriggerType_User is button.");
			if (gazeTriggerType_User == WVR_GazeTriggerType.WVR_GazeTriggerType_TimeoutButton)
				DEBUG ("updateInteractionModeAndGazeTriggerType() gazeTriggerType_User is timeout & button.");
		}
		if (this.OverrideSystemSettings)
		{
			if ((this.InteractionMode_User != this.CustomInputModule) || (this.preOverrideSystemSettings != this.OverrideSystemSettings))
			{
				if (this.InteractionMode_User != this.CustomInputModule)
				{
					this.InteractionMode_User = this.CustomInputModule;
					DEBUG (this.InteractionMode_User == WaveVR_EInputModule.Controller ? "updateInteractionModeAndGazeTriggerType() InteractionMode_User is controller." : "updateInteractionModeAndGazeTriggerType() InteractionMode_User is gaze.");
				}
				if (this.preOverrideSystemSettings != this.OverrideSystemSettings)
				{
					this.preOverrideSystemSettings = this.OverrideSystemSettings;
					DEBUG (this.OverrideSystemSettings ? "updateInteractionModeAndGazeTriggerType() OverrideSystemSettings is true." : "updateInteractionModeAndGazeTriggerType() OverrideSystemSettings is false.");
				}

				WVR_InteractionMode _mode = (this.InteractionMode_User == WaveVR_EInputModule.Controller) ?
						WVR_InteractionMode.WVR_InteractionMode_Controller : WVR_InteractionMode.WVR_InteractionMode_Gaze;
				Interop.WVR_SetInteractionMode (_mode);
			}

			updateGazeTriggerType_User ();
			if (this.gazeTriggerType_User_pre != this.gazeTriggerType_User)
			{
				this.gazeTriggerType_User_pre = this.gazeTriggerType_User;
				Interop.WVR_SetGazeTriggerType (this.gazeTriggerType_User);

				if (gazeTriggerType_User == WVR_GazeTriggerType.WVR_GazeTriggerType_Timeout)
					DEBUG ("updateInteractionModeAndGazeTriggerType() gazeTriggerType_User is timeout.");
				if (gazeTriggerType_User == WVR_GazeTriggerType.WVR_GazeTriggerType_Button)
					DEBUG ("updateInteractionModeAndGazeTriggerType() gazeTriggerType_User is button.");
				if (gazeTriggerType_User == WVR_GazeTriggerType.WVR_GazeTriggerType_TimeoutButton)
					DEBUG ("updateInteractionModeAndGazeTriggerType() gazeTriggerType_User is timeout & button.");
			}

			updateInputModuleByCustomSettings ();
		} else
		{
			if (this.preOverrideSystemSettings != this.OverrideSystemSettings)
			{
				this.preOverrideSystemSettings = this.OverrideSystemSettings;
				// Restore runtime mirror system setting.
				Interop.WVR_SetInteractionMode (WVR_InteractionMode.WVR_InteractionMode_SystemDefault);
			}

			this.InteractionMode_System = WaveVR.Instance.InteractionMode;
			this.gazeTriggerType_System = WaveVR.Instance.GazeTriggerType;

			updateInputModuleBySystemSetting ();
		}
	}

	private void updateGazeTriggerType_User()
	{
		// Sync user settings of gaze trigger type
		if (Gaze.ButtonControl)
		{
			if (Gaze.TimerControl)
			{
				gazeTriggerType_User = WVR_GazeTriggerType.WVR_GazeTriggerType_TimeoutButton;
				// DEBUG("[User Settings] Sync gaze trigger type to WVR_GazeTriggerType_TimeoutButton!");
			} else
			{
				gazeTriggerType_User = WVR_GazeTriggerType.WVR_GazeTriggerType_Button;
				// DEBUG("[User Settings] Sync gaze trigger type to WVR_GazeTriggerType_Button!");
			}
		} else
		{
			gazeTriggerType_User = WVR_GazeTriggerType.WVR_GazeTriggerType_Timeout;
			// DEBUG("[User Settings] Sync gaze trigger type to WVR_GazeTriggerType_Timeout!");
		}
	}
	#endregion

	private GameObject Head = null;
	private GameObject eventSystem = null;
	private WaveVR_GazeInputModule gazeInputModule = null;
	private WaveVR_ControllerInputModule controllerInputModule = null;

	#region MonoBehaviour overrides.
	void Awake()
	{
		if (instance == null)
			instance = this;
	}

	void Start()
	{
		if (EventSystem.current == null)
		{
			EventSystem _es = FindObjectOfType<EventSystem> ();
			if (_es != null)
			{
				eventSystem = _es.gameObject;
				DEBUG ("Start() find current EventSystem: " + eventSystem.name);
			}

			if (eventSystem == null)
			{
				DEBUG ("Start() could not find EventSystem, create new one.");
				eventSystem = new GameObject ("EventSystem", typeof(EventSystem));
			}
		} else
		{
			eventSystem = EventSystem.current.gameObject;
		}

		// Standalone Input Module
		StandaloneInputModule _sim = eventSystem.GetComponent<StandaloneInputModule> ();
		if (_sim != null)
			_sim.enabled = false;

		// Old GazeInputModule
		GazeInputModule _gim = eventSystem.GetComponent<GazeInputModule>();
		if (_gim != null)
			Destroy (_gim);

		// Gaze Input Module
		gazeInputModule = eventSystem.GetComponent<WaveVR_GazeInputModule> ();
		WaveVR_Reticle gazePointer = gameObject.GetComponentInChildren<WaveVR_Reticle> ();
		if (gazePointer != null)
		{
			gazePointerRenderer = gazePointer.gameObject.GetComponent<MeshRenderer> ();
			DEBUG ("Start() found " + gazePointer.gameObject.name);
		}
		RingMeshDrawer ringMesh = gameObject.GetComponentInChildren<RingMeshDrawer> ();
		if (ringMesh != null)
		{
			gazeRingRenderer = ringMesh.gameObject.GetComponent<MeshRenderer> ();
			DEBUG ("Start() found " + gazePointer.gameObject.name);
		}
		ActivateGazePointer (false);	// disable reticle

		// Controller Input Module
		controllerInputModule = eventSystem.GetComponent<WaveVR_ControllerInputModule> ();

		initInteractionModeAndGazeTriggerType ();

		if (!this.EnableInputModule)
		{
			disableAllInputModules ();
		}
	}

	public void Update()
	{
		if (WaveVR_Render.Instance != null)
			this.Head = WaveVR_Render.Instance.gameObject;

		// For Gaze
		if (this.Head != null)
		{
			gameObject.transform.position = this.Head.transform.position;
			gameObject.transform.rotation = this.Head.transform.rotation;
		}

		if (!this.EnableInputModule)
		{
			disableAllInputModules ();
			return;
		}

		if (WaveVR.Instance.FocusCapturedBySystem)
			return;

		updateInteractionModeAndGazeTriggerType ();

		WVR_InteractionMode _cur_mode = this.GetInteractionMode ();
		if (InteractionMode_Current != _cur_mode)
		{
			InteractionMode_Current = _cur_mode;
			if (InteractionMode_Current == WVR_InteractionMode.WVR_InteractionMode_Controller)
				DEBUG ("Update() InteractionMode_Current is controller.");
			if (InteractionMode_Current == WVR_InteractionMode.WVR_InteractionMode_Gaze)
				DEBUG ("Update() InteractionMode_Current is gaze.");
			WaveVR_Utils.Event.Send (WaveVR_Utils.Event.INTERACTION_MODE_CHANGED, InteractionMode_Current, this.AlwaysShowController);
		}
	}
	#endregion

	#region Gaze
	private MeshRenderer gazePointerRenderer = null;
	private MeshRenderer gazeRingRenderer = null;
	private void ActivateGazePointer(bool active)
	{
		if (gazeInputModule != null)
		{
			gazeInputModule.ActivatePointerAndRing (active);
		} else
		{
			if (gazePointerRenderer != null)
			{
				gazePointerRenderer.enabled = false;
				DEBUG ("ActivateGazePointer() no gaze input module, disable the gaze reticle.");
			}
			if (gazeRingRenderer != null)
			{
				gazeRingRenderer.enabled = false;
				DEBUG ("ActivateGazePointer() no gaze input module, disable the gaze ring.");
			}
		}
	}

	private void CreateGazeInputModule()
	{
		if (gazeInputModule == null)
		{
			// Before initializing variables of input modules, disable EventSystem to prevent the OnEnable() of input modules being executed.
			eventSystem.SetActive (false);

			gazeInputModule = eventSystem.AddComponent<WaveVR_GazeInputModule> ();
			SetGazeInputModuleParameters ();

			// Enable EventSystem after initializing input modules.
			eventSystem.SetActive (true);
		}
	}

	private void SetGazeInputModuleParameters()
	{
		if (gazeInputModule != null)
		{
			gazeInputModule.enabled = false;
			gazeInputModule.UseWaveVRReticle = Gaze.UseWaveVRReticle;
			gazeInputModule.TimerControl = Gaze.TimerControl;
			gazeInputModule.TimeToGaze = Gaze.TimeToGaze;
			gazeInputModule.ProgressRate = Gaze.ProgressRate;
			gazeInputModule.RateTextZPosition = Gaze.RateTextZPosition;
			gazeInputModule.ProgressCounter = Gaze.ProgressCounter;
			gazeInputModule.CounterTextZPosition = Gaze.CounterTextZPosition;
			gazeInputModule.InputEvent = Gaze.InputEvent;
			gazeInputModule.ButtonControl = Gaze.ButtonControl;
			gazeInputModule.ButtonControlDevices = Gaze.ButtonControlDevices;
			gazeInputModule.ButtonControlKeys = Gaze.ButtonControlKeys;
			gazeInputModule.Head = Gaze.Head;
			gazeInputModule.enabled = true;
		}
	}

	/// <summary>
	/// Updates only parameters related to GazeTriggerType of gaze input module
	/// </summary>
	private void updateGazeInputModule()
	{
		if (gazeInputModule == null)
			return;

		if (this.OverrideSystemSettings)
		{
			gazeInputModule.ButtonControl = this.Gaze.ButtonControl;
			gazeInputModule.EnableTimerControl (this.Gaze.TimerControl);
		} else
		{
			switch (this.gazeTriggerType_System)
			{
			case WVR_GazeTriggerType.WVR_GazeTriggerType_Button:
				gazeInputModule.ButtonControl = true;
				gazeInputModule.EnableTimerControl (false);
				break;
			case WVR_GazeTriggerType.WVR_GazeTriggerType_TimeoutButton:
				gazeInputModule.ButtonControl = true;
				gazeInputModule.EnableTimerControl (true);
				break;
			default:
				gazeInputModule.ButtonControl = false;
				gazeInputModule.EnableTimerControl (false);
				break;
			}
		}
	}

	private void SetActiveGaze(bool value)
	{
		if (gazeInputModule != null)
		{
			gazeInputModule.enabled = value;
		}
		else
		{
			if (value)
				CreateGazeInputModule ();
		}
	}

	public void UseWaveVRReticle(bool use)
	{
		this.Gaze.UseWaveVRReticle = use;
		if (gazeInputModule != null)
			gazeInputModule.UseWaveVRReticle = this.Gaze.UseWaveVRReticle;
	}
	#endregion

	#region Controller
	private void CreateControllerInputModule()
	{
		if (controllerInputModule == null)
		{
			// Before initializing variables of input modules, disable EventSystem to prevent the OnEnable() of input modules being executed.
			eventSystem.SetActive (false);

			controllerInputModule = eventSystem.AddComponent<WaveVR_ControllerInputModule> ();
			SetControllerInputModuleParameters ();

			// Enable EventSystem after initializing input modules.
			eventSystem.SetActive (true);
		}
	}

	private void SetControllerInputModuleParameters()
	{
		if (controllerInputModule != null)
		{
			controllerInputModule.enabled = false;
			controllerInputModule.DomintEventEnabled = Controller.DominantEventEnabled;
			if (Controller.DominantController != null)
			{
				// Controller model should not contain WaveVR_ControllerLoader
				if (Controller.DominantController.GetComponentInChildren<WaveVR_ControllerLoader> () == null)
					controllerInputModule.DominantController = Controller.DominantController;
			}
			controllerInputModule.DominantRaycastMask = Controller.DominantRaycastMask;
			controllerInputModule.NoDomtEventEnabled = Controller.NonDominantEventEnabled;
			if (Controller.NonDominantController != null)
			{
				// Controller model should not contain WaveVR_ControllerLoader
				if (Controller.NonDominantController.GetComponentInChildren<WaveVR_ControllerLoader> () == null)
					controllerInputModule.NonDominantController = Controller.NonDominantController;
			}
			controllerInputModule.NonDominantRaycastMask = Controller.NonDominantRaycastMask;
			controllerInputModule.ButtonToTrigger = Controller.ButtonToTrigger;
			controllerInputModule.RaycastMode = Controller.RaycastMode;
			controllerInputModule.RaycastStartPoint = Controller.RaycastStartPoint;
			controllerInputModule.CanvasTag = Controller.CanvasTag;
			controllerInputModule.FixedBeamLength = this.FixedBeamLength;
			controllerInputModule.enabled = true;
		}
	}

	private void updateControllerInputModule()
	{
		if (controllerInputModule == null)
			return;

		controllerInputModule.RaycastMode = this.Controller.RaycastMode;
	}

	private void SetActiveController(bool value)
	{
		if (controllerInputModule != null)
			controllerInputModule.enabled = value;
		else
		{
			if (value)
				CreateControllerInputModule ();
		}
	}
	#endregion

	private bool IsAnyControllerConnected()
	{
		bool _result = false;

		for (int i = 0; i < WaveVR.DeviceTypes.Length; i++)
		{
			if (WaveVR.DeviceTypes[i] == WVR_DeviceType.WVR_DeviceType_HMD)
				continue;

			if (WaveVR.Instance.Initialized)
			{
				WaveVR.Device _dev = WaveVR.Instance.getDeviceByType (WaveVR.DeviceTypes[i]);
				if (_dev.connected)
				{
					_result = true;
					break;
				}
			}
		}

		return _result;
	}

	#region Input Module
	private void initializeInputModuleByCustomSettings()
	{
		switch (this.InteractionMode_User)
		{
		case WaveVR_EInputModule.Controller:
			SetActiveGaze (false);

			if (controllerInputModule == null)
				CreateControllerInputModule ();
			else
				SetControllerInputModuleParameters ();
			break;
		case WaveVR_EInputModule.Gaze:
			SetActiveController (false);
			ActivateGazePointer (true);

			if (gazeInputModule == null)
				CreateGazeInputModule ();
			else
				SetGazeInputModuleParameters ();
			break;
		default:
			break;
		}
	}

	private void initializeInputModuleBySystemSetting()
	{
		switch (this.InteractionMode_System)
		{
		case WVR_InteractionMode.WVR_InteractionMode_Controller:
			SetActiveGaze (false);

			if (controllerInputModule == null)
				CreateControllerInputModule ();
			else
				SetControllerInputModuleParameters ();
			break;
		case WVR_InteractionMode.WVR_InteractionMode_Gaze:
			SetActiveController (false);
			ActivateGazePointer (true);

			if (gazeInputModule == null)
				CreateGazeInputModule ();
			else
				SetGazeInputModuleParameters ();
			break;
		default:
			break;
		}
	}

	private void disableAllInputModules()
	{
		SetActiveController (false);
		SetActiveGaze (false);
	}

	private void updateInputModuleByCustomSettings()
	{
		switch (this.InteractionMode_User)
		{
		case WaveVR_EInputModule.Gaze:
			ActivateGazePointer (true);
			SetActiveGaze (true);
			SetActiveController (false);
			updateGazeInputModule ();
			break;
		case WaveVR_EInputModule.Controller:
			SetActiveGaze (false);
			if (IsAnyControllerConnected ())
			{
				SetActiveController (true);
				updateControllerInputModule ();
			} else
			{
				SetActiveController (false);
				if (this.AutoGaze)
				{
					// No controller connected, using gaze input module.
					SetActiveGaze (true);
					updateGazeInputModule ();
				}
			}
			break;
		default:
			break;
		}
	}

	private void updateInputModuleBySystemSetting()
	{
		// Sync system settings of interaction mode
		switch (this.InteractionMode_System)
		{
		case WVR_InteractionMode.WVR_InteractionMode_Gaze:
			ActivateGazePointer (true);
			SetActiveGaze (true);
			SetActiveController (false);
			updateGazeInputModule ();
			break;
		case WVR_InteractionMode.WVR_InteractionMode_Controller:
			SetActiveGaze (false);
			if (IsAnyControllerConnected ())
			{
				SetActiveController (true);
				updateControllerInputModule ();
			} else
			{
				SetActiveController (false);
				if (this.AutoGaze)
				{
					SetActiveGaze (true);
					updateGazeInputModule ();
				}
			}
			break;
		default:
			break;
		}
	}
	#endregion

	public WaveVR_ControllerInputModule.ERaycastMode GetRaycastMode()
	{
		if (controllerInputModule != null)
			return controllerInputModule.RaycastMode;
		else
			return Controller.RaycastMode;
	}

	public WVR_InteractionMode GetInteractionMode()
	{
		WVR_InteractionMode _custom_mode =
			(this.InteractionMode_User == WaveVR_EInputModule.Controller) ?
			WVR_InteractionMode.WVR_InteractionMode_Controller : WVR_InteractionMode.WVR_InteractionMode_Gaze;
		return (OverrideSystemSettings) ? _custom_mode : this.InteractionMode_System;
	}

	public WVR_GazeTriggerType GetGazeTriggerType()
	{
		return (OverrideSystemSettings)? gazeTriggerType_User : gazeTriggerType_System;
	}

	public WVR_GazeTriggerType GetUserGazeTriggerType()
	{
		return gazeTriggerType_User;
	}

	public void SetControllerBeamLength(WaveVR_Controller.EDeviceType dt, float length)
	{
		if (controllerInputModule != null)
			controllerInputModule.ChangeBeamLength (dt, length);
	}
}
