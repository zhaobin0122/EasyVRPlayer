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
using UnityEngine.EventSystems;
using UnityEngine.UI;
using wvr;
using WVR_Log;
using System;
using UnityEngine.Profiling;

public enum ERaycastStartPoint
{
	CenterOfEyes,
	LeftEye,
	RightEye
}

public class WaveVR_ControllerInputModule : BaseInputModule
{
	private const string LOG_TAG = "WaveVR_ControllerInputModule";

	private void DEBUG(string msg)
	{
		if (Log.EnableDebugLog)
			Log.d (LOG_TAG, msg, true);
	}
	private void INFO(string msg)
	{
		Log.i (LOG_TAG, msg, true);
	}

	public enum ERaycastMode
	{
		Beam,
		Fixed,
		Mouse
	}
	public static ERaycastMode[] RaycastModes = new ERaycastMode[] {
		ERaycastMode.Beam,
		ERaycastMode.Fixed,
		ERaycastMode.Mouse
	};

	#region Developer specified parameters
	[HideInInspector]
	public bool UnityMode = false;
	// In Unity mode, if drag is prior, the click event will NOT be sent after dragging.
	[HideInInspector]
	public bool UnityMode_PriorDrag = false;

	public bool DomintEventEnabled = true;
	public GameObject DominantController = null;
	public LayerMask DominantRaycastMask = ~0;
	public bool NoDomtEventEnabled = true;
	public GameObject NonDominantController = null;
	public LayerMask NonDominantRaycastMask = ~0;
	public List<WaveVR_ButtonList.EButtons> ButtonToTrigger = new List<WaveVR_ButtonList.EButtons> ();
	private GameObject Head = null;
	[HideInInspector]
	public ERaycastMode RaycastMode = ERaycastMode.Mouse;
	[HideInInspector]
	public ERaycastStartPoint RaycastStartPoint = ERaycastStartPoint.CenterOfEyes;
	[Tooltip("Will be obsoleted soon!")]
	public string CanvasTag = "EventCanvas";
	#endregion

	// Do NOT allow event DOWN being sent multiple times during CLICK_TIME.
	// Since UI element of Unity needs time to perform transitions.
	private const float CLICK_TIME = 0.2f;
	private const float DRAG_TIME = 0.2f;

	private const float raycastStartPointOffset = 0.0315f;

	private GameObject pointCameraNoDomt = null;
	private GameObject pointCameraDomint = null;
	public float FixedBeamLength = 9.5f;			// = Beam endOffsetMax, can be specified in editor mode.
	private float lengthFromBeamToPointer = 0.5f;   // = Beam enfOffsetMin
	private Color32 FlexiblePointerColor = Color.blue;
	private ERaycastMode preRaycastMode;
	private bool toChangeBeamPointer = true;   // Should change beam mesh on start.
	private Vector3 DomintIntersectPos_prev = Vector3.zero;
	private Vector3 NoDomtIntersectPos_prev = Vector3.zero;

	#region basic declaration
	[SerializeField]
	private bool mForceModuleActive = true;

	public bool ForceModuleActive
	{
		get { return mForceModuleActive; }
		set { mForceModuleActive = value; }
	}

	public override bool IsModuleSupported()
	{
		return mForceModuleActive;
	}

	public override bool ShouldActivateModule()
	{
		if (!base.ShouldActivateModule ())
			return false;

		if (mForceModuleActive)
			return true;

		return false;
	}

	public override void DeactivateModule() {
		base.DeactivateModule();

		for (int i = 0; i < WaveVR_Controller.DeviceTypes.Length; i++)
		{
			WaveVR_Controller.EDeviceType dev_type = WaveVR_Controller.DeviceTypes [i];
			EventController _event_controller = GetEventController (dev_type);
			if (_event_controller != null)
			{
				INFO ("DeactivateModule()");
				ExitAllObjects (_event_controller);
			}
		}
	}
	#endregion

	[System.Serializable]
	public class CBeamConfig
	{
		// Default mouse mode configurations.
		public float StartWidth;
		public float EndWidth;
		public float StartOffset;
		public float EndOffset;
		public Color32 StartColor;
		public Color32 EndColor;

		public void assignedTo(CBeamConfig src)
		{
			StartWidth = src.StartWidth;
			EndWidth = src.EndWidth;
			StartOffset = src.StartOffset;
			EndOffset = src.EndOffset;
			StartColor = src.StartColor;
			EndColor = src.EndColor;
		}
	}

	private CBeamConfig mouseBeamConfig = new CBeamConfig {
		StartWidth = 0.000625f,
		EndWidth = 0.00125f,
		StartOffset = 0.015f,
		EndOffset = 0.8f,
		StartColor = new Color32 (255, 255, 255, 255),
		EndColor = new Color32 (255, 255, 255, 77)
	};

	private CBeamConfig fixedBeamConfig = new CBeamConfig {
		StartWidth = 0.000625f,
		EndWidth = 0.00125f,
		StartOffset = 0.015f,
		EndOffset = 9.5f,
		StartColor = new Color32 (255, 255, 255, 255),
		EndColor = new Color32 (255, 255, 255, 255)
	};

	private CBeamConfig flexibleBeamConfig = new CBeamConfig {
		StartWidth = 0.000625f,
		EndWidth = 0.00125f,
		StartOffset = 0.015f,
		EndOffset = 0.8f,
		StartColor = new Color32 (255, 255, 255, 255),
		EndColor = new Color32 (255, 255, 255, 0)
	};

	public class RaycastModeSetting
	{
		public ERaycastMode Mode { get; set; }
		public CBeamConfig Config { get; set; }

		public RaycastModeSetting(ERaycastMode mode, CBeamConfig config)
		{
			this.Mode = mode;
			this.Config = config;
		}
	}

	public class EventController
	{
		public WaveVR_Controller.EDeviceType device {
			get;
			set;
		}

		public GameObject controller {
			get;
			set;
		}

		public GameObject prevRaycastedObject {
			get;
			set;
		}

		public PointerEventData event_data {
			get;
			set;
		}

		public WaveVR_ControllerPointer pointer {
			get;
			set;
		}

		public bool pointerEnabled {
			get;
			set;
		}

		public WaveVR_Beam beam {
			get;
			set;
		}

		public bool beamEnabled {
			get;
			set;
		}

		private List<RaycastModeSetting> raycastModeSettings;
		public void SetBeamConfig(ERaycastMode mode, CBeamConfig config)
		{
			for (int i = 0; i < raycastModeSettings.Count; i++)
			{
				if (raycastModeSettings [i].Mode == mode)
				{
					raycastModeSettings [i].Config.assignedTo (config);
				}
			}
		}
		public CBeamConfig GetBeamConfig(ERaycastMode mode)
		{
			for (int i = 0; i < raycastModeSettings.Count; i++)
			{
				if (raycastModeSettings [i].Mode == mode)
					return raycastModeSettings [i].Config;
			}
			return null;
		}

		public bool eligibleForButtonClick {
			get;
			set;
		}

		public EventController(WaveVR_Controller.EDeviceType type)
		{
			device = type;
			controller = null;
			prevRaycastedObject = null;
			event_data = null;
			eligibleForButtonClick = false;
			beam = null;
			beamEnabled = false;
			pointer = null;
			pointerEnabled = false;
			raycastModeSettings = new List<RaycastModeSetting>();
			for (int i = 0; i < RaycastModes.Length; i++)
			{
				ERaycastMode _mode = RaycastModes[i];
				raycastModeSettings.Add (new RaycastModeSetting(_mode, new CBeamConfig ()));
			}
		}
	}

	private List<EventController> EventControllers = new List<EventController>();
	private EventController GetEventController(WaveVR_Controller.EDeviceType dt)
	{
		for (int i = 0; i < EventControllers.Count; i++)
		{
			if (EventControllers [i].device == dt)
				return EventControllers [i];
		}
		return null;
	}

	private void UpdateControllerModelInProcess()
	{
		for (int i = 0; i < WaveVR_Controller.DeviceTypes.Length; i++)
		{
			WaveVR_Controller.EDeviceType dev_type = WaveVR_Controller.DeviceTypes [i];
			// HMD uses Gaze, not controller input module.
			if (dev_type == WaveVR_Controller.EDeviceType.Head)
				continue;

			EventController event_controller = GetEventController (dev_type);

			GameObject origin_model = event_controller.controller;
			GameObject new_model = WaveVR_EventSystemControllerProvider.Instance.GetControllerModel (dev_type);
			LayerMask _mask = ~0;
			if (dev_type == WaveVR_Controller.EDeviceType.Dominant)
				_mask = this.DominantRaycastMask;
			if (dev_type == WaveVR_Controller.EDeviceType.NonDominant)
				_mask = this.NonDominantRaycastMask;

			if (origin_model == null)
			{
				if (new_model != null)
				{
					// replace with new controller instance.
					if (dev_type == WaveVR_Controller.EDeviceType.Head)
						DEBUG ("UpdateControllerModelInProcess() Head replace null with new controller instance.");
					if (dev_type == WaveVR_Controller.EDeviceType.Dominant)
						DEBUG ("UpdateControllerModelInProcess() Dominant replace null with new controller instance.");
					if (dev_type == WaveVR_Controller.EDeviceType.NonDominant)
						DEBUG ("UpdateControllerModelInProcess() Non-Dominant replace null with new controller instance.");
					SetupEventController (event_controller, new_model, _mask);
				}
			} else
			{
				if (new_model == null)
				{
					if (WaveVR_EventSystemControllerProvider.Instance.HasControllerLoader(dev_type))
					{
						// clear controller instance.
						if (dev_type == WaveVR_Controller.EDeviceType.Head)
							DEBUG ("UpdateControllerModelInProcess() Head clear controller instance.");
						if (dev_type == WaveVR_Controller.EDeviceType.Dominant)
							DEBUG ("UpdateControllerModelInProcess() Dominant clear controller instance.");
						if (dev_type == WaveVR_Controller.EDeviceType.NonDominant)
							DEBUG ("UpdateControllerModelInProcess() Non-Dominant clear controller instance.");
						SetupEventController (event_controller, null, _mask);
					}
				} else
				{
					if (!GameObject.ReferenceEquals (origin_model, new_model))
					{
						// replace with new controller instance.
						if (dev_type == WaveVR_Controller.EDeviceType.Head)
							DEBUG ("UpdateControllerModelInProcess() Head set new controller instance.");
						if (dev_type == WaveVR_Controller.EDeviceType.Dominant)
							DEBUG ("UpdateControllerModelInProcess() Dominant set new controller instance.");
						if (dev_type == WaveVR_Controller.EDeviceType.NonDominant)
							DEBUG ("UpdateControllerModelInProcess() NonDominant set new controller instance.");
						SetupEventController (event_controller, new_model, _mask);
					}
				}
			}
		}
	}

	private void SetupEventController(EventController eventController, GameObject controller_model, LayerMask mask)
	{
		// Diactivate old controller, replace with new controller, activate new controller
		if (eventController.controller != null)
		{
			DEBUG ("SetupEventController() deactivate " + eventController.controller.name);
			eventController.controller.SetActive (false);
		}

		eventController.controller = controller_model;

		// Note: must setup beam first.
		if (eventController.controller != null)
		{
			DEBUG ("SetupEventController() activate " + eventController.controller.name);
			eventController.controller.SetActive (true);

			eventController.SetBeamConfig (ERaycastMode.Beam, flexibleBeamConfig);
			eventController.SetBeamConfig (ERaycastMode.Fixed, fixedBeamConfig);
			eventController.SetBeamConfig (ERaycastMode.Mouse, mouseBeamConfig);

			// Get beam of controller.
			eventController.beam = eventController.controller.GetComponentInChildren<WaveVR_Beam> (true);
			if (eventController.beam != null)
			{
				DEBUG ("SetupEventController() set up WaveVR_Beam: " + eventController.beam.gameObject.name + " of " + eventController.device);
				SetupEventControllerBeam (eventController, Vector3.zero, true);
			}

			// Get pointer of controller.
			PhysicsRaycaster phy_raycaster = eventController.controller.GetComponentInChildren<PhysicsRaycaster> ();

			eventController.pointer = eventController.controller.GetComponentInChildren<WaveVR_ControllerPointer> (true);
			if (eventController.pointer != null)
			{
				DEBUG ("SetupEventController() set up WaveVR_ControllerPointer: " + eventController.pointer.gameObject.name + " of " + eventController.device);

				// Get PhysicsRaycaster of pointer. If none, add new one.
				if (phy_raycaster == null)
				{
					DEBUG ("SetupEventController() add PhysicsRaycaster on " + eventController.pointer.gameObject.name);
					phy_raycaster = eventController.pointer.gameObject.AddComponent<PhysicsRaycaster> ();
				}

				SetupEventControllerPointer (eventController);
			} else
			{
				// Get PhysicsRaycaster of controller. If none, add new one.
				if (phy_raycaster == null)
				{
					DEBUG ("SetupEventController() add PhysicsRaycaster on " + eventController.controller.name);
					phy_raycaster = eventController.controller.AddComponent<PhysicsRaycaster> ();
				}
			}
			phy_raycaster.eventMask = mask;
			DEBUG ("SetupEventController() physics mask: " + phy_raycaster.eventMask.value);

			// Disable Camera to save rendering cost.
			Camera event_camera = phy_raycaster.gameObject.GetComponent<Camera> ();
			if (event_camera != null)
			{
				event_camera.stereoTargetEye = StereoTargetEyeMask.None;
				event_camera.enabled = false;
			}

			Camera _controller_camera = eventController.controller.GetComponent<Camera>();
			if (_controller_camera != null)
			{
				DEBUG ("SetupEventController() found controller camera of " + eventController.controller.name);
				_controller_camera.enabled = false;
			}
		}
	}

	private void SetupEventControllerBeam(EventController eventController, Vector3 intersectionPosition, bool updateRaycastConfig = false)
	{
		if (eventController.beam == null)
			return;

		CBeamConfig _config = eventController.GetBeamConfig (this.RaycastMode);

		if (updateRaycastConfig)
		{
			_config.StartWidth = eventController.beam.StartWidth;
			_config.EndWidth = eventController.beam.EndWidth;
			_config.StartOffset = eventController.beam.StartOffset;
			_config.StartColor = eventController.beam.StartColor;
			_config.EndColor = eventController.beam.EndColor;

			switch (this.RaycastMode)
			{
			case ERaycastMode.Beam:
			case ERaycastMode.Mouse:
				_config.EndOffset = eventController.beam.EndOffset;
				break;
			case ERaycastMode.Fixed:
				_config.EndOffset = this.FixedBeamLength;
				break;
			default:
				break;
			}
			eventController.SetBeamConfig (this.RaycastMode, _config);

			DEBUG ("SetupEventControllerBeam() " + eventController.device + ", " + this.RaycastMode + " mode config - "
				+ "StartWidth: " + _config.StartWidth
				+ ", EndWidth: " + _config.EndWidth
				+ ", StartOffset: " + _config.StartOffset
				+ ", EndOffset: " + _config.EndOffset
				+ ", StartColor: " + _config.StartColor.ToString()
				+ ", EndColor: " + _config.EndColor.ToString()
			);
		}

		eventController.beam.StartWidth = _config.StartWidth;
		eventController.beam.EndWidth = _config.EndWidth;
		eventController.beam.StartOffset = _config.StartOffset;
		eventController.beam.EndOffset = _config.EndOffset;
		eventController.beam.StartColor = _config.StartColor;
		eventController.beam.EndColor = _config.EndColor;

		DEBUG ("SetupEventControllerBeam() " + eventController.device + ", " + this.RaycastMode + " mode"
			+ ", StartWidth: " + eventController.beam.StartWidth
			+ ", EndWidth: " + eventController.beam.EndWidth
			+ ", StartOffset: " + eventController.beam.StartOffset
			+ ", length: " + eventController.beam.EndOffset
			+ ", StartColor: " + eventController.beam.StartColor.ToString ()
			+ ", EndColor: " + eventController.beam.EndColor.ToString ());
	}

	private void SetupEventControllerPointer(EventController eventController, Camera eventCamera, Vector3 intersectionPosition)
	{
		if (eventController.pointer == null)
			return;

		float pointerDistanceInMeters = 0, pointerOuterDiameter = 0;
		GameObject curr_raycasted_obj = GetRaycastedObject (eventController.device);

		// Due to pointer distance is changed by beam length, do NOT load OEM CONFIG.
		switch (this.RaycastMode)
		{
		case ERaycastMode.Mouse:
			if (eventController.beam != null)
				pointerDistanceInMeters = eventController.beam.EndOffset + eventController.beam.endOffsetMin;
			else
				pointerDistanceInMeters = this.mouseBeamConfig.EndOffset + this.lengthFromBeamToPointer;

			pointerOuterDiameter = eventController.pointer.PointerOuterDiameterMin + (pointerDistanceInMeters / eventController.pointer.kpointerGrowthAngle);

			eventController.pointer.PointerDistanceInMeters = pointerDistanceInMeters;
			eventController.pointer.PointerOuterDiameter = pointerOuterDiameter;
			eventController.pointer.PointerColor = Color.white;
			eventController.pointer.PointerRenderQueue = 5000;
			break;
		case ERaycastMode.Fixed:
			if (eventController.beam != null)
				pointerDistanceInMeters = eventController.beam.EndOffset + eventController.beam.endOffsetMin;
			else
				pointerDistanceInMeters = this.fixedBeamConfig.EndOffset + this.lengthFromBeamToPointer;

			pointerOuterDiameter = eventController.pointer.PointerOuterDiameterMin * pointerDistanceInMeters;
			eventController.pointer.PointerDistanceInMeters = pointerDistanceInMeters;
			eventController.pointer.PointerOuterDiameter = pointerOuterDiameter;
			eventController.pointer.PointerRenderQueue = 1000;
			break;
		case ERaycastMode.Beam:
			if (curr_raycasted_obj != null)
			{
				eventController.pointer.OnPointerEnter (eventCamera, curr_raycasted_obj, intersectionPosition, true);
				eventController.pointer.PointerColor = FlexiblePointerColor;
			} else
			{
				eventController.pointer.PointerColor = Color.white;
				if (eventController.beam != null)
					eventController.pointer.PointerDistanceInMeters = eventController.beam.EndOffset + eventController.beam.endOffsetMin;
			}

			pointerDistanceInMeters = eventController.pointer.PointerDistanceInMeters;
			pointerOuterDiameter = eventController.pointer.PointerOuterDiameterMin
				+ (pointerDistanceInMeters / eventController.pointer.kpointerGrowthAngle);
			eventController.pointer.PointerOuterDiameter = pointerOuterDiameter;
			eventController.pointer.PointerRenderQueue = 5000;
			break;
		default:
			break;
		}

		DEBUG ("SetupEventControllerPointer() " + eventController.device + ", " + this.RaycastMode + " mode"
			+ ", pointerDistanceInMeters: " + pointerDistanceInMeters
			+ ", pointerOuterDiameter: " + pointerOuterDiameter);
	}

	private void SetupEventControllerPointer(EventController eventController)
	{
		if (eventController.pointer == null)
			return;

		SetupEventControllerPointer (eventController, null, Vector3.zero);
	}

	public void ChangeBeamLength(WaveVR_Controller.EDeviceType dt, float length)
	{
		EventController event_controller = GetEventController (dt);
		if (event_controller == null)
			return;

		if (this.RaycastMode == ERaycastMode.Fixed || this.RaycastMode == ERaycastMode.Mouse)
		{
			CBeamConfig _config = event_controller.GetBeamConfig (this.RaycastMode);
			_config.EndOffset = length;
			event_controller.SetBeamConfig (this.RaycastMode, _config);
		}

		SetupEventControllerBeam (event_controller, Vector3.zero, false);
		SetupEventControllerPointer (event_controller);
	}

	private void SetupPointerCamera(WaveVR_Controller.EDeviceType type)
	{
		if (this.Head == null)
		{
			DEBUG ("SetupPointerCamera() no Head!!");
			return;
		}
		if (type == WaveVR_Controller.EDeviceType.Dominant)
		{
			pointCameraDomint = new GameObject ("PointerCameraR");
			if (pointCameraDomint == null)
				return;

			DEBUG ("SetupPointerCamera() Dominant - add component WaveVR_PointerCameraTracker");
			pointCameraDomint.AddComponent<WaveVR_PointerCameraTracker> ();
			DEBUG ("SetupPointerCamera() Dominant add component - WaveVR_PoseTrackerManager");
			pointCameraDomint.AddComponent<WaveVR_PoseTrackerManager> ();
			PhysicsRaycaster phy_raycaster = pointCameraDomint.AddComponent<PhysicsRaycaster> ();
			if (phy_raycaster != null)
			{
				phy_raycaster.eventMask = this.DominantRaycastMask;
				DEBUG ("SetupPointerCamera() Dominant - set physics raycast mask to " + phy_raycaster.eventMask.value);
			}
			pointCameraDomint.transform.SetParent (this.Head.transform, false);
			DEBUG ("SetupPointerCamera() Dominant - set pointerCamera parent to " + this.pointCameraDomint.transform.parent.name);
			if (WaveVR_Render.Instance != null && WaveVR_Render.Instance.righteye != null)
			{
				this.pointCameraDomint.transform.position = WaveVR_Render.Instance.righteye.transform.position;
			} else
			{
				if (RaycastStartPoint == ERaycastStartPoint.LeftEye)
				{
					pointCameraDomint.transform.localPosition = new Vector3 (-raycastStartPointOffset, 0f, 0.15f);
				} else if (RaycastStartPoint == ERaycastStartPoint.RightEye)
				{
					pointCameraDomint.transform.localPosition = new Vector3 (raycastStartPointOffset, 0f, 0.15f);
				} else
				{
					pointCameraDomint.transform.localPosition = new Vector3 (0f, 0f, 0.15f);
				}
			}
			Camera pc = pointCameraDomint.GetComponent<Camera> ();
			if (pc != null)
			{
				pc.enabled = false;
				pc.fieldOfView = 1f;
				pc.nearClipPlane = 0.01f;
			}
			WaveVR_PointerCameraTracker pcTracker = pointCameraDomint.GetComponent<WaveVR_PointerCameraTracker> ();
			if (pcTracker != null)
			{
				pcTracker.setDeviceType (type);
			}
			WaveVR_PoseTrackerManager poseTracker = pointCameraDomint.GetComponent<WaveVR_PoseTrackerManager> ();
			if (poseTracker != null)
			{
				DEBUG ("SetupPointerCamera() Dominant - disable WaveVR_PoseTrackerManager");
				poseTracker.Type = type;
				poseTracker.TrackPosition = false;
				poseTracker.TrackRotation = false;
				poseTracker.enabled = false;
			}
		} else if (type == WaveVR_Controller.EDeviceType.NonDominant)
		{
			pointCameraNoDomt = new GameObject ("PointerCameraL");
			if (pointCameraNoDomt == null)
				return;

			DEBUG ("SetupPointerCamera() NonDominant - add component WaveVR_PointerCameraTracker");
			pointCameraNoDomt.AddComponent<WaveVR_PointerCameraTracker> ();
			DEBUG ("SetupPointerCamera() NonDominant add component - WaveVR_PoseTrackerManager");
			pointCameraNoDomt.AddComponent<WaveVR_PoseTrackerManager> ();
			PhysicsRaycaster phy_raycaster = pointCameraNoDomt.AddComponent<PhysicsRaycaster> ();
			if (phy_raycaster != null)
			{
				phy_raycaster.eventMask = this.NonDominantRaycastMask;
				DEBUG ("SetupPointerCamera() NonDominant - set physics raycast mask to " + phy_raycaster.eventMask.value);
			}
			pointCameraNoDomt.transform.SetParent (this.Head.transform, false);
			DEBUG ("SetupPointerCamera() NonDominant - set pointerCamera parent to " + this.pointCameraNoDomt.transform.parent.name);
			if (WaveVR_Render.Instance != null && WaveVR_Render.Instance.lefteye != null)
			{
				this.pointCameraNoDomt.transform.position = WaveVR_Render.Instance.lefteye.transform.position;
			} else
			{
				if (RaycastStartPoint == ERaycastStartPoint.LeftEye)
				{
					pointCameraNoDomt.transform.localPosition = new Vector3 (-raycastStartPointOffset, 0f, 0.15f);
				} else if (RaycastStartPoint == ERaycastStartPoint.RightEye)
				{
					pointCameraNoDomt.transform.localPosition = new Vector3 (raycastStartPointOffset, 0f, 0.15f);
				} else
				{
					pointCameraNoDomt.transform.localPosition = new Vector3 (0f, 0f, 0.15f);
				}
			}
			Camera pc = pointCameraNoDomt.GetComponent<Camera> ();
			if (pc != null)
			{
				pc.enabled = false;
				pc.fieldOfView = 1f;
				pc.nearClipPlane = 0.01f;
			}
			WaveVR_PointerCameraTracker pcTracker = pointCameraNoDomt.GetComponent<WaveVR_PointerCameraTracker> ();
			if (pcTracker != null)
			{
				pcTracker.setDeviceType (type);
			}
			WaveVR_PoseTrackerManager poseTracker = pointCameraNoDomt.GetComponent<WaveVR_PoseTrackerManager> ();
			if (poseTracker != null)
			{
				DEBUG ("SetupPointerCamera() NonDominant - disable WaveVR_PoseTrackerManager");
				poseTracker.Type = type;
				poseTracker.TrackPosition = false;
				poseTracker.TrackRotation = false;
				poseTracker.enabled = false;
			}
		}
	}

	#region Override BaseInputModule
	private bool enableControllerInputModule = false;
	protected override void OnEnable()
	{
		if (!enableControllerInputModule)
		{
			base.OnEnable ();
			DEBUG ("OnEnable()");

			enableControllerInputModule = true;
			for (int i = 0; i < WaveVR_Controller.DeviceTypes.Length; i++)
				EventControllers.Add (new EventController (WaveVR_Controller.DeviceTypes [i]));

			// Right controller
			if (this.DominantController != null)
			{
				EventController ec = GetEventController (WaveVR_Controller.EDeviceType.Dominant);
				SetupEventController (
					ec,
					this.DominantController,
					this.DominantRaycastMask
				);
			}

			// Left controller
			if (this.NonDominantController != null)
			{
				EventController ec = GetEventController (WaveVR_Controller.EDeviceType.NonDominant);
				SetupEventController (
					ec,
					this.NonDominantController,
					this.NonDominantRaycastMask
				);
			}

			if (this.Head == null)
			{
				if (WaveVR_Render.Instance != null)
				{
					this.Head = WaveVR_Render.Instance.gameObject;
					DEBUG ("OnEnable() set up Head to " + this.Head.name);
				}
			}

			this.preRaycastMode = this.RaycastMode;
		}
	}

	protected override void OnDisable()
	{
		if (enableControllerInputModule)
		{
			base.OnDisable ();
			DEBUG ("OnDisable()");

			enableControllerInputModule = false;
			for (int i = 0; i < WaveVR_Controller.DeviceTypes.Length; i++)
			{
				WaveVR_Controller.EDeviceType dev_type = WaveVR_Controller.DeviceTypes [i];
				EventController _event_controller = GetEventController (dev_type);
				if (_event_controller != null)
				{
					ExitAllObjects (_event_controller);
				}
			}

			EventControllers.Clear ();
		}
	}

	public override void Process()
	{
		if (!enableControllerInputModule)
			return;

		UpdateControllerModelInProcess ();

		if (this.Head == null)
		{
			if (WaveVR_Render.Instance != null)
			{
				this.Head = WaveVR_Render.Instance.gameObject;
				DEBUG ("Process() setup Head to " + this.Head.name);
			}
		}

		for (int i = 0; i < WaveVR_Controller.DeviceTypes.Length; i++)
		{
			WaveVR_Controller.EDeviceType dev_type = WaveVR_Controller.DeviceTypes [i];
			// -------------------- Conditions for running loop begins -----------------
			// 1.HMD uses Gaze, not controller input module.
			if (dev_type == WaveVR_Controller.EDeviceType.Head)
				continue;

			// 2.Do nothing if no event controller.
			EventController event_controller = GetEventController (dev_type);
			if (event_controller == null)
				continue;

			GameObject controller_model = event_controller.controller;
			if (controller_model == null)
				continue;

			// 3.Check ths pointer & beam status of the event controller.
			CheckBeamPointerActive (event_controller);

			// 4. Exit the objects "entered" previously if disabling events.
			if ((dev_type == WaveVR_Controller.EDeviceType.Dominant && this.DomintEventEnabled == false) ||
				(dev_type == WaveVR_Controller.EDeviceType.NonDominant && this.NoDomtEventEnabled == false))
			{
				ExitAllObjects (event_controller);
				continue;
			}

			// 5. Exit the objects "entered" previously if losing the system focus.
			if (WaveVR.Instance.Initialized)
			{
				if (WaveVR.Instance.FocusCapturedBySystem)
				{
					ExitAllObjects (event_controller);
					continue;
				}
			}

			bool valid_pose = false;
			// "connected" from WaveVR means the "pose" is valid or not.
			WaveVR.Device wdev = WaveVR.Instance.getDeviceByType (dev_type);
			if (wdev != null)
				valid_pose = wdev.connected;

			// 5. Exit the objects "entered" previously if the device is disconnected.
			if (!valid_pose)
			{
				ExitAllObjects (event_controller);
				continue;
			}
			// -------------------- Conditions for running loop ends -----------------

			// -------------------- Set up the event camera begins -------------------
			event_controller.prevRaycastedObject = GetRaycastedObject (dev_type);

			if ((dev_type == WaveVR_Controller.EDeviceType.NonDominant && pointCameraNoDomt == null) ||
			    (dev_type == WaveVR_Controller.EDeviceType.Dominant && pointCameraDomint == null))
			{
				SetupPointerCamera (dev_type);
			}

			Camera event_camera = null;
			// Mouse mode: raycasting from HMD after direct raycasting from controller
			if (RaycastMode == ERaycastMode.Mouse)
			{
				event_camera = dev_type == WaveVR_Controller.EDeviceType.NonDominant ?
					(pointCameraNoDomt != null ? pointCameraNoDomt.GetComponent<Camera> () : null) :
					(pointCameraDomint != null ? pointCameraDomint.GetComponent<Camera> () : null);
				ResetPointerEventData_Hybrid (dev_type, event_camera);
			} else
			{
				event_camera = (Camera)controller_model.GetComponentInChildren (typeof(Camera));
				ResetPointerEventData (dev_type);
			}
			if (event_camera == null)
				continue;
			// -------------------- Set up the event camera ends ---------------------

			// -------------------- Raycast begins -------------------
			// 1. Get the nearest graphic raycast object.
			// Also, all raycasted graphic objects are stored in graphicRaycastObjects<device type>.
			GraphicRaycast (event_controller, event_camera);

			// 2. Get the physical raycast object.
			// If the physical object is nearer than the graphic object, pointerCurrentRaycast will be set to the physical object.
			// Also, all raycasted physical objects are stored in physicsRaycastObjects<device type>.
			PhysicsRaycaster phy_raycaster = null;
			if (RaycastMode == ERaycastMode.Mouse)
			{
				phy_raycaster = event_camera.GetComponent<PhysicsRaycaster> ();
			} else
			{
				phy_raycaster = controller_model.GetComponentInChildren<PhysicsRaycaster> ();
			}
			if (phy_raycaster != null)
			{
				if (RaycastMode == ERaycastMode.Mouse)
					ResetPointerEventData_Hybrid (dev_type, event_camera);
				else
					ResetPointerEventData (dev_type);

				// Issue: GC.Alloc 40 bytes.
				PhysicsRaycast (event_controller, phy_raycaster);
			}
			// -------------------- Raycast ends -------------------

			// Get the pointerCurrentRaycast object.
			GameObject curr_raycasted_obj = GetRaycastedObject (dev_type);

			// -------------------- Send Events begins -------------------
			// 1. Exit previous object, enter new object.
			//OnTriggerEnterAndExit (dev_type, event_controller.event_data);
			EnterExitGraphicObject (event_controller);
			EnterExitPhysicsObject (event_controller);

			// 2. Hover object.
			if (curr_raycasted_obj != null && curr_raycasted_obj == event_controller.prevRaycastedObject)
			{
				OnTriggerHover (dev_type, event_controller.event_data);
			}

			// 3. Get button states, some events are triggered by the button.
			bool btnPressDown = false, btnPressed = false, btnPressUp = false;
			for (int b = 0; b < this.ButtonToTrigger.Count; b++)
			{
				btnPressDown |= WaveVR_Controller.Input (dev_type).GetPressDown (this.ButtonToTrigger [b]);
				btnPressed |= WaveVR_Controller.Input (dev_type).GetPress (this.ButtonToTrigger [b]);
				btnPressUp |= WaveVR_Controller.Input (dev_type).GetPressUp (this.ButtonToTrigger [b]);
			}

			// Pointer Click equals to Button.onClick, we sent Pointer Click in OnTriggerUp()
			//if (btnPressDown)
			//	event_controller.eligibleForButtonClick = true;
			//if (btnPressUp && event_controller.eligibleForButtonClick)
			//	onButtonClick (event_controller);

			// 
			if (!btnPressDown && btnPressed)
			{
				// button hold means to drag.
				if (!UnityMode)
					OnDrag (dev_type, event_controller.event_data);
				else
					OnDragMouse (dev_type, event_controller.event_data);
			} else if (Time.unscaledTime - event_controller.event_data.clickTime < CLICK_TIME)
			{
				// Delay new events until CLICK_TIME has passed.
			} else if (btnPressDown && !event_controller.event_data.eligibleForClick)
			{
				// 1. button not pressed -> pressed.
				// 2. no pending Click should be procced.
				OnTriggerDown (dev_type, event_controller.event_data);
			} else if (!btnPressed)
			{
				// 1. If Down before, send Up event and clear Down state.
				// 2. If Dragging, send Drop & EndDrag event and clear Dragging state.
				// 3. If no Down or Dragging state, do NOTHING.
				if (!UnityMode)
					OnTriggerUp (dev_type, event_controller.event_data);
				else
					OnTriggerUpMouse (dev_type, event_controller.event_data);
			}
			// -------------------- Send Events ends -------------------

			PointerEventData _event_data = event_controller.event_data;
			Vector3 _intersectionPosition = GetIntersectionPosition (_event_data.enterEventCamera, _event_data.pointerCurrentRaycast);

			WaveVR_RaycastResultProvider.Instance.SetRaycastResult (
				dev_type,
				event_controller.event_data.pointerCurrentRaycast.gameObject,
				_intersectionPosition);

			// Update beam & pointer when:
			// 1. Raycast mode changed.
			// 2. Beam or Pointer active state changed.
			if (this.toChangeBeamPointer || this.preRaycastMode != this.RaycastMode)
			{
				if (this.RaycastMode == ERaycastMode.Beam)
					DEBUG ("Process() controller raycast mode is flexible beam.");
				if (this.RaycastMode == ERaycastMode.Fixed)
					DEBUG ("Process() controller raycast mode is fixed beam.");
				if (this.RaycastMode == ERaycastMode.Mouse)
					DEBUG ("Process() controller raycast mode is mouse.");
				SetupEventControllerBeam (event_controller, _intersectionPosition, false);
				SetupEventControllerPointer (event_controller, _event_data.enterEventCamera, _intersectionPosition);

				this.toChangeBeamPointer = false;
			}

			// Update flexible beam and pointer when intersection position changes.
			Vector3 _intersectionPosition_prev =
				(dev_type == WaveVR_Controller.EDeviceType.Dominant) ? DomintIntersectPos_prev : NoDomtIntersectPos_prev;
			if (_intersectionPosition_prev != _intersectionPosition)
			{
				_intersectionPosition_prev = _intersectionPosition;
				if (this.RaycastMode == ERaycastMode.Beam && curr_raycasted_obj != null)
				{
					if (event_controller.pointer != null)
						event_controller.pointer.OnPointerEnter (_event_data.enterEventCamera, curr_raycasted_obj, _intersectionPosition, true);
					if (event_controller.beam != null)
						event_controller.beam.SetEndOffset (_intersectionPosition, false);

					if (Log.gpl.Print)
						DEBUG ("Process() " + dev_type + ", _intersectionPosition_prev (" + _intersectionPosition_prev.x + ", " + _intersectionPosition_prev.y + ", " + _intersectionPosition_prev.z + ")");
				}

				if (dev_type == WaveVR_Controller.EDeviceType.Dominant)
					DomintIntersectPos_prev = _intersectionPosition_prev;
				if (dev_type == WaveVR_Controller.EDeviceType.NonDominant)
					NoDomtIntersectPos_prev = _intersectionPosition_prev;
			}
		}

		this.preRaycastMode = this.RaycastMode;

		SetPointerCameraTracker ();
	}
	#endregion

	#region Raycast
	private void PhysicsRaycast(EventController event_controller, PhysicsRaycaster raycaster)
	{
		RaycastResult _firstResult = new RaycastResult ();

		if (event_controller.device == WaveVR_Controller.EDeviceType.Dominant)
		{
			physicsRaycastObjectsDominant.Clear ();
			physicsRaycastResultsDominant.Clear ();

			Profiler.BeginSample ("PhysicsRaycaster.Raycast() dominant.");
			raycaster.Raycast (event_controller.event_data, physicsRaycastResultsDominant);
			Profiler.EndSample ();

			for (int i = 0; i < physicsRaycastResultsDominant.Count; i++)
				physicsRaycastObjectsDominant.Add (physicsRaycastResultsDominant [i].gameObject);

			_firstResult = FindFirstRaycast (physicsRaycastResultsDominant);
		} else if (event_controller.device == WaveVR_Controller.EDeviceType.NonDominant)
		{
			physicsRaycastObjectsNoDomint.Clear ();
			physicsRaycastResultsNoDomint.Clear ();

			Profiler.BeginSample ("PhysicsRaycaster.Raycast() non-dominant.");
			raycaster.Raycast (event_controller.event_data, physicsRaycastResultsNoDomint);
			Profiler.EndSample ();

			for (int i = 0; i < physicsRaycastResultsNoDomint.Count; i++)
				physicsRaycastObjectsNoDomint.Add (physicsRaycastResultsNoDomint [i].gameObject);

			_firstResult = FindFirstRaycast (physicsRaycastResultsNoDomint);
		}

		if (_firstResult.module != null)
		{
			//DEBUG ("PhysicsRaycast() device: " + event_controller.device + ", camera: " + _firstResult.module.eventCamera + ", first result = " + _firstResult);
		}

		if (_firstResult.gameObject != null)
		{
			if (_firstResult.worldPosition == Vector3.zero)
			{
				_firstResult.worldPosition = GetIntersectionPosition (
					_firstResult.module.eventCamera,
					_firstResult
				);
			}

			float new_dist =
				Mathf.Abs (
					_firstResult.worldPosition.z -
					_firstResult.module.eventCamera.transform.position.z);
			float origin_dist =
				Mathf.Abs (
					event_controller.event_data.pointerCurrentRaycast.worldPosition.z -
					_firstResult.module.eventCamera.transform.position.z);

			if (event_controller.event_data.pointerCurrentRaycast.gameObject == null || origin_dist > new_dist)
			{
				/*
				DEBUG ("PhysicsRaycast()" +
					", raycasted: " + _firstResult.gameObject.name +
					", raycasted position: " + _firstResult.worldPosition +
					", new_dist: " + new_dist +
					", origin target: " +
					(event_controller.event_data.pointerCurrentRaycast.gameObject == null ?
						"null" :
						event_controller.event_data.pointerCurrentRaycast.gameObject.name) +
					", origin position: " + event_controller.event_data.pointerCurrentRaycast.worldPosition +
					", origin distance: " + origin_dist);
				*/
				event_controller.event_data.pointerCurrentRaycast = _firstResult;
				event_controller.event_data.position = _firstResult.screenPosition;
			}
		}
	}

	private void GraphicRaycast(EventController event_controller, Camera event_camera)
	{
		if (event_controller.device == WaveVR_Controller.EDeviceType.Head)
			return;

		/* --------------------- Find GUIs those can be raycasted begins. ---------------------
		// 1. find Canvas by TAG
		GameObject[] _tag_GUIs = GameObject.FindGameObjectsWithTag (CanvasTag);
		// 2. Get Canvas from Pointer Canvas Provider
		GameObject[] _event_GUIs = WaveVR_EventSystemGUIProvider.GetEventGUIs();

		GameObject[] _GUIs = MergeArray (_tag_GUIs, _event_GUIs);
		// --------------------- Find GUIs those can be raycasted ends. --------------------- */

		// Reset pointerCurrentRaycast even no GUI.
		RaycastResult _firstResult = new RaycastResult ();
		event_controller.event_data.pointerCurrentRaycast = _firstResult;

		Profiler.BeginSample ("Find GraphicRaycaster.");
		GraphicRaycaster[] _graphic_raycasters = GameObject.FindObjectsOfType<GraphicRaycaster> ();
		Profiler.EndSample ();

		if (event_controller.device == WaveVR_Controller.EDeviceType.Dominant)
			graphicRaycastObjectsDominant.Clear ();
		if (event_controller.device == WaveVR_Controller.EDeviceType.NonDominant)
			graphicRaycastObjectsNoDomint.Clear ();

		for (int i = 0; i < _graphic_raycasters.Length; i++)
		{
			if (_graphic_raycasters [i].gameObject != null && _graphic_raycasters [i].gameObject.GetComponent<Canvas> () != null)
				_graphic_raycasters [i].gameObject.GetComponent<Canvas> ().worldCamera = event_camera;
			else
				continue;

			if (event_controller.device == WaveVR_Controller.EDeviceType.Dominant)
			{
				_graphic_raycasters [i].Raycast (event_controller.event_data, graphicRaycastResultsDominant);
				if (graphicRaycastResultsDominant.Count == 0)
					continue;

				for (int g = 0; g < graphicRaycastResultsDominant.Count; g++)
					graphicRaycastObjectsDominant.Add (graphicRaycastResultsDominant [g].gameObject);

				_firstResult = FindFirstRaycast (graphicRaycastResultsDominant);
				graphicRaycastResultsDominant.Clear ();
			}

			if (event_controller.device == WaveVR_Controller.EDeviceType.NonDominant)
			{
				_graphic_raycasters [i].Raycast (event_controller.event_data, graphicRaycastResultsNoDomint);
				if (graphicRaycastResultsNoDomint.Count == 0)
					continue;

				for (int g = 0; g < graphicRaycastResultsNoDomint.Count; g++)
					graphicRaycastObjectsNoDomint.Add (graphicRaycastResultsNoDomint [g].gameObject);

				_firstResult = FindFirstRaycast (graphicRaycastResultsNoDomint);
				graphicRaycastResultsNoDomint.Clear ();
			}

			if (_firstResult.module != null)
			{
				//DEBUG ("GraphicRaycast() device: " + event_controller.device + ", camera: " + _firstResult.module.eventCamera + ", first result = " + _firstResult);
			}

			// Found graphic raycasted object!
			if (_firstResult.gameObject != null)
			{
				if (_firstResult.worldPosition == Vector3.zero)
				{
					_firstResult.worldPosition = GetIntersectionPosition (
						_firstResult.module.eventCamera,
						_firstResult
					);
				}

				float new_dist =
					Mathf.Abs (
						_firstResult.worldPosition.z -
						_firstResult.module.eventCamera.transform.position.z);
				float origin_dist =
					Mathf.Abs (
						event_controller.event_data.pointerCurrentRaycast.worldPosition.z -
						_firstResult.module.eventCamera.transform.position.z);


				bool _changeCurrentRaycast = false;
				// Raycast to nearest (z-axis) target.
				if (event_controller.event_data.pointerCurrentRaycast.gameObject == null)
				{
					_changeCurrentRaycast = true;
				} else
				{
					/*
					DEBUG ("GraphicRaycast() "
						+ ", raycasted: " + _firstResult.gameObject.name
						+ ", raycasted position: " + _firstResult.worldPosition
						+ ", distance: " + new_dist
						+ ", sorting order: " + _firstResult.sortingOrder
						+ ", origin target: " +
						(event_controller.event_data.pointerCurrentRaycast.gameObject == null ?
							"null" :
							event_controller.event_data.pointerCurrentRaycast.gameObject.name)
						+ ", origin position: " + event_controller.event_data.pointerCurrentRaycast.worldPosition
						+ ", origin distance: " + origin_dist
						+ ", origin sorting order: " + event_controller.event_data.pointerCurrentRaycast.sortingOrder);
					*/
					if (origin_dist > new_dist)
					{
						DEBUG ("GraphicRaycast() "
							+ event_controller.device + ", "
							+ event_controller.event_data.pointerCurrentRaycast.gameObject.name
							+ ", position: " + event_controller.event_data.pointerCurrentRaycast.worldPosition
							+ ", distance: " + origin_dist
							+ " is farer than "
							+ _firstResult.gameObject.name
							+ ", position: " + _firstResult.worldPosition
							+ ", new distance: " + new_dist);

						_changeCurrentRaycast = true;
					} else if (origin_dist == new_dist)
					{
						int _so_origin = event_controller.event_data.pointerCurrentRaycast.sortingOrder;
						int _so_result = _firstResult.sortingOrder;

						if (_so_origin < _so_result)
						{
							DEBUG ("GraphicRaycast() "
								+ event_controller.device + ", "
								+ event_controller.event_data.pointerCurrentRaycast.gameObject.name
								+ " sorting order: " + _so_origin + " is smaller than "
								+ _firstResult.gameObject.name
								+ " sorting order: " + _so_result);

							_changeCurrentRaycast = true;
						}
					}
				}

				if (_changeCurrentRaycast)
				{
					event_controller.event_data.pointerCurrentRaycast = _firstResult;
					event_controller.event_data.position = _firstResult.screenPosition;
				}

				//break;
			}
		}
	}
	#endregion

	#region EventSystem
	List<RaycastResult> physicsRaycastResultsDominant = new List<RaycastResult>();
	List<RaycastResult> physicsRaycastResultsNoDomint = new List<RaycastResult>();

	List<GameObject> physicsRaycastObjectsDominant = new List<GameObject>(), prePhysicsRaycastObjectsDominant = new List<GameObject>();
	List<GameObject> physicsRaycastObjectsNoDomint = new List<GameObject>(), prePhysicsRaycastObjectsNoDomint = new List<GameObject>();
	List<GameObject> physicsRaycastObjectsTmp = new List<GameObject> (), prePhysicsRaycastObjectsTmp = new List<GameObject>();

	List<RaycastResult> graphicRaycastResultsDominant = new List<RaycastResult>();
	List<RaycastResult> graphicRaycastResultsNoDomint = new List<RaycastResult>();

	List<GameObject> graphicRaycastObjectsDominant = new List<GameObject>(), preGraphicRaycastObjectsDominant = new List<GameObject>();
	List<GameObject> graphicRaycastObjectsNoDomint = new List<GameObject>(), preGraphicRaycastObjectsNoDomint = new List<GameObject>();
	List<GameObject> graphicRaycastObjectsTmp = new List<GameObject>(), preGraphicRaycastObjectsTmp = new List<GameObject>();

	private bool IsPhysicalRaycasted(EventController event_controller, GameObject go)
	{
		if (event_controller.device == WaveVR_Controller.EDeviceType.Dominant)
			return physicsRaycastObjectsDominant.Contains (go);
		if (event_controller.device == WaveVR_Controller.EDeviceType.NonDominant)
			return physicsRaycastObjectsNoDomint.Contains (go);
		return false;
	}

	private bool IsGraphicRaycasted(EventController event_controller, GameObject go)
	{
		if (event_controller.device == WaveVR_Controller.EDeviceType.Dominant)
			return graphicRaycastObjectsDominant.Contains (go);
		if (event_controller.device == WaveVR_Controller.EDeviceType.NonDominant)
			return graphicRaycastObjectsNoDomint.Contains (go);
		return false;
	}

	private void EnterExitPhysicsObject(EventController event_controller)
	{
		if (event_controller.device == WaveVR_Controller.EDeviceType.Dominant)
		{
			CopyList (physicsRaycastObjectsDominant, physicsRaycastObjectsTmp);
			CopyList (prePhysicsRaycastObjectsDominant, prePhysicsRaycastObjectsTmp);
		} else if (event_controller.device == WaveVR_Controller.EDeviceType.NonDominant)
		{
			CopyList (physicsRaycastObjectsNoDomint, physicsRaycastObjectsTmp);
			CopyList (prePhysicsRaycastObjectsNoDomint, prePhysicsRaycastObjectsTmp);
		}

		if (physicsRaycastObjectsTmp.Count != 0)
		{
			for (int i = 0; i < physicsRaycastObjectsTmp.Count; i++)
			{
				if (physicsRaycastObjectsTmp [i] != null && !prePhysicsRaycastObjectsTmp.Contains (physicsRaycastObjectsTmp [i]))
				{
					ExecuteEvents.Execute (physicsRaycastObjectsTmp [i], event_controller.event_data, ExecuteEvents.pointerEnterHandler);
					DEBUG ("EnterExitPhysicsObject() enter: " + physicsRaycastObjectsTmp [i]);
				}
			}
		}

		if (prePhysicsRaycastObjectsTmp.Count != 0)
		{
			for (int i = 0; i < prePhysicsRaycastObjectsTmp.Count; i++)
			{
				if (prePhysicsRaycastObjectsTmp [i] != null && !physicsRaycastObjectsTmp.Contains (prePhysicsRaycastObjectsTmp [i]))
				{
					ExecuteEvents.Execute (prePhysicsRaycastObjectsTmp [i], event_controller.event_data, ExecuteEvents.pointerExitHandler);
					DEBUG ("EnterExitPhysicsObject() exit: " + prePhysicsRaycastObjectsTmp [i]);
				}
			}
		}

		prePhysicsRaycastObjectsTmp.Clear ();
		for (int i = 0; i < physicsRaycastObjectsTmp.Count; i++)
		{
			prePhysicsRaycastObjectsTmp.Add (physicsRaycastObjectsTmp [i]);
		}

		if (event_controller.device == WaveVR_Controller.EDeviceType.Dominant)
		{
			CopyList (physicsRaycastObjectsTmp, physicsRaycastObjectsDominant);
			CopyList (prePhysicsRaycastObjectsTmp, prePhysicsRaycastObjectsDominant);
		}
		if (event_controller.device == WaveVR_Controller.EDeviceType.NonDominant)
		{
			CopyList (physicsRaycastObjectsTmp, physicsRaycastObjectsNoDomint);
			CopyList (prePhysicsRaycastObjectsTmp, prePhysicsRaycastObjectsNoDomint);
		}
	}

	private void EnterExitGraphicObject(EventController event_controller)
	{
		if (event_controller.device == WaveVR_Controller.EDeviceType.Dominant)
		{
			CopyList (graphicRaycastObjectsDominant, graphicRaycastObjectsTmp);
			CopyList (preGraphicRaycastObjectsDominant, preGraphicRaycastObjectsTmp);
		} else if (event_controller.device == WaveVR_Controller.EDeviceType.NonDominant)
		{
			CopyList (graphicRaycastObjectsNoDomint, graphicRaycastObjectsTmp);
			CopyList (preGraphicRaycastObjectsNoDomint, preGraphicRaycastObjectsTmp);
		}

		if (graphicRaycastObjectsTmp.Count != 0)
		{
			for (int i = 0; i < graphicRaycastObjectsTmp.Count; i++)
			{
				if (graphicRaycastObjectsTmp [i] != null && !preGraphicRaycastObjectsTmp.Contains (graphicRaycastObjectsTmp [i]))
				{
					ExecuteEvents.Execute (graphicRaycastObjectsTmp [i], event_controller.event_data, ExecuteEvents.pointerEnterHandler);
					DEBUG ("EnterExitGraphicObject() enter: " + graphicRaycastObjectsTmp [i]);
				}
			}
		}

		if (preGraphicRaycastObjectsTmp.Count != 0)
		{
			for (int i = 0; i < preGraphicRaycastObjectsTmp.Count; i++)
			{
				if (preGraphicRaycastObjectsTmp [i] != null && !graphicRaycastObjectsTmp.Contains (preGraphicRaycastObjectsTmp [i]))
				{
					ExecuteEvents.Execute (preGraphicRaycastObjectsTmp [i], event_controller.event_data, ExecuteEvents.pointerExitHandler);
					DEBUG ("EnterExitGraphicObject() exit: " + preGraphicRaycastObjectsTmp [i]);
				}
			}
		}

		preGraphicRaycastObjectsTmp.Clear ();
		for (int i = 0; i < graphicRaycastObjectsTmp.Count; i++)
		{
			preGraphicRaycastObjectsTmp.Add (graphicRaycastObjectsTmp [i]);
		}

		if (event_controller.device == WaveVR_Controller.EDeviceType.Dominant)
		{
			CopyList (graphicRaycastObjectsTmp, graphicRaycastObjectsDominant);
			CopyList (preGraphicRaycastObjectsTmp, preGraphicRaycastObjectsDominant);
		}
		if (event_controller.device == WaveVR_Controller.EDeviceType.NonDominant)
		{
			CopyList (graphicRaycastObjectsTmp, graphicRaycastObjectsNoDomint);
			CopyList (preGraphicRaycastObjectsTmp, preGraphicRaycastObjectsNoDomint);
		}
	}

	private void ExitAllObjects(EventController event_controller)
	{
		if (event_controller.device == WaveVR_Controller.EDeviceType.Dominant)
		{
			CopyList (prePhysicsRaycastObjectsDominant, prePhysicsRaycastObjectsTmp);
			CopyList (preGraphicRaycastObjectsDominant, preGraphicRaycastObjectsTmp);
		} else if (event_controller.device == WaveVR_Controller.EDeviceType.NonDominant)
		{
			CopyList (prePhysicsRaycastObjectsNoDomint, prePhysicsRaycastObjectsTmp);
			CopyList (preGraphicRaycastObjectsNoDomint, preGraphicRaycastObjectsTmp);
		}

		if (prePhysicsRaycastObjectsTmp.Count != 0)
		{
			for (int i = 0; i < prePhysicsRaycastObjectsTmp.Count; i++)
			{
				if (prePhysicsRaycastObjectsTmp [i] != null)
				{
					ExecuteEvents.Execute (prePhysicsRaycastObjectsTmp [i], event_controller.event_data, ExecuteEvents.pointerExitHandler);
					DEBUG ("ExitAllObjects() exit: " + prePhysicsRaycastObjectsTmp [i]);
				}
			}

			prePhysicsRaycastObjectsTmp.Clear ();
		}

		if (preGraphicRaycastObjectsTmp.Count != 0)
		{
			for (int i = 0; i < preGraphicRaycastObjectsTmp.Count; i++)
			{
				if (preGraphicRaycastObjectsTmp [i] != null)
				{
					ExecuteEvents.Execute (preGraphicRaycastObjectsTmp [i], event_controller.event_data, ExecuteEvents.pointerExitHandler);
					DEBUG ("ExitAllObjects() exit: " + preGraphicRaycastObjectsTmp [i]);
				}
			}

			preGraphicRaycastObjectsTmp.Clear ();
		}

		if (event_controller.device == WaveVR_Controller.EDeviceType.Dominant)
		{
			CopyList (prePhysicsRaycastObjectsTmp, prePhysicsRaycastObjectsDominant);
			CopyList (preGraphicRaycastObjectsTmp, preGraphicRaycastObjectsDominant);
		}
		if (event_controller.device == WaveVR_Controller.EDeviceType.NonDominant)
		{
			CopyList (prePhysicsRaycastObjectsTmp, prePhysicsRaycastObjectsNoDomint);
			CopyList (preGraphicRaycastObjectsTmp, preGraphicRaycastObjectsNoDomint);
		}
	}

	private void OnTriggerDown(WaveVR_Controller.EDeviceType type, PointerEventData event_data)
	{
		GameObject _go = GetRaycastedObject (type);
		if (_go == null)
			return;

		// Send Pointer Down. If not received, get handler of Pointer Click.
		event_data.pressPosition = event_data.position;
		event_data.pointerPressRaycast = event_data.pointerCurrentRaycast;
		event_data.pointerPress =
			ExecuteEvents.ExecuteHierarchy(_go, event_data, ExecuteEvents.pointerDownHandler)
			?? ExecuteEvents.GetEventHandler<IPointerClickHandler>(_go);

		DEBUG ("OnTriggerDown() device: " + type + " send Pointer Down to " + event_data.pointerPress + ", current GameObject is " + _go);

		// If Drag Handler exists, send initializePotentialDrag event.
		event_data.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(_go);
		if (event_data.pointerDrag != null)
		{
			DEBUG ("OnTriggerDown() device: " + type + " send initializePotentialDrag to " + event_data.pointerDrag + ", current GameObject is " + _go);
			ExecuteEvents.Execute(event_data.pointerDrag, event_data, ExecuteEvents.initializePotentialDrag);
		}

		// press happened (even not handled) object.
		event_data.rawPointerPress = _go;
		// allow to send Pointer Click event
		event_data.eligibleForClick = true;
		// reset the screen position of press, can be used to estimate move distance
		event_data.delta = Vector2.zero;
		// current Down, reset drag state
		event_data.dragging = false;
		event_data.useDragThreshold = true;
		// record the count of Pointer Click should be processed, clean when Click event is sent.
		event_data.clickCount = 1;
		// set clickTime to current time of Pointer Down instead of Pointer Click.
		// since Down & Up event should not be sent too closely. (< CLICK_TIME)
		event_data.clickTime = Time.unscaledTime;
	}

	private void OnTriggerUp(WaveVR_Controller.EDeviceType type, PointerEventData event_data)
	{
		if (!event_data.eligibleForClick && !event_data.dragging)
		{
			// 1. no pending click
			// 2. no dragging
			// Mean user has finished all actions and do NOTHING in current frame.
			return;
		}

		GameObject _go = GetRaycastedObject (type);
		// _go may be different with event_data.pointerDrag so we don't check null

		if (event_data.pointerPress != null)
		{
			// In the frame of button is pressed -> unpressed, send Pointer Up
			DEBUG ("OnTriggerUp type: " + type + " send Pointer Up to " + event_data.pointerPress);
			ExecuteEvents.Execute (event_data.pointerPress, event_data, ExecuteEvents.pointerUpHandler);
		}
		if (event_data.eligibleForClick)
		{
			// In the frame of button from being pressed to unpressed, send Pointer Click if Click is pending.
			DEBUG ("OnTriggerUp type: " + type + " send Pointer Click to " + event_data.pointerPress);
			ExecuteEvents.Execute(event_data.pointerPress, event_data, ExecuteEvents.pointerClickHandler);
		} else if (event_data.dragging)
		{
			// In next frame of button from being pressed to unpressed, send Drop and EndDrag if dragging.
			DEBUG ("OnTriggerUp type: " + type + " send Pointer Drop to " + _go + ", EndDrag to " + event_data.pointerDrag);
			ExecuteEvents.ExecuteHierarchy(_go, event_data, ExecuteEvents.dropHandler);
			ExecuteEvents.Execute(event_data.pointerDrag, event_data, ExecuteEvents.endDragHandler);

			event_data.pointerDrag = null;
			event_data.dragging = false;
		}

		// Down of pending Click object.
		event_data.pointerPress = null;
		// press happened (even not handled) object.
		event_data.rawPointerPress = null;
		// clear pending state.
		event_data.eligibleForClick = false;
		// Click is processed, clearcount.
		event_data.clickCount = 0;
		// Up is processed thus clear the time limitation of Down event.
		event_data.clickTime = 0;
	}

	private void OnTriggerUpMouse(WaveVR_Controller.EDeviceType type, PointerEventData event_data)
	{
		if (!event_data.eligibleForClick && !event_data.dragging)
		{
			// 1. no pending click
			// 2. no dragging
			// Mean user has finished all actions and do NOTHING in current frame.
			return;
		}

		GameObject _go = GetRaycastedObject (type);
		// _go may be different with event_data.pointerDrag so we don't check null

		if (event_data.pointerPress != null)
		{
			// In the frame of button is pressed -> unpressed, send Pointer Up
			DEBUG ("OnTriggerUpMouse() type: " + type + " send Pointer Up to " + event_data.pointerPress);
			ExecuteEvents.Execute (event_data.pointerPress, event_data, ExecuteEvents.pointerUpHandler);
		}

		if (event_data.eligibleForClick)
		{
			GameObject _pointerClick = ExecuteEvents.GetEventHandler<IPointerClickHandler> (_go);
			if (!this.UnityMode_PriorDrag)
			{
				if (_pointerClick != null)
				{
					if (_pointerClick == event_data.pointerPress)
					{
						// In the frame of button from being pressed to unpressed, send Pointer Click if Click is pending.
						DEBUG ("OnTriggerUpMouse() type: " + type + " send Pointer Click to " + event_data.pointerPress);
						ExecuteEvents.Execute (event_data.pointerPress, event_data, ExecuteEvents.pointerClickHandler);
					} else
					{
						DEBUG ("OnTriggerUpMouse() type: " + type
							+ " pointer down object " + event_data.pointerPress
							+ " is different with click object " + _pointerClick);
					}
				} else
				{
					if (event_data.dragging)
					{
						GameObject _pointerDrop = ExecuteEvents.GetEventHandler<IDropHandler> (_go);
						if (_pointerDrop == event_data.pointerDrag)
						{
							// In next frame of button from being pressed to unpressed, send Drop and EndDrag if dragging.
							DEBUG ("OnTriggerUpMouse() type: " + type + " send Pointer Drop to " + event_data.pointerDrag);
							ExecuteEvents.Execute (event_data.pointerDrag, event_data, ExecuteEvents.dropHandler);
						}
						DEBUG ("OnTriggerUpMouse() type: " + type + " send Pointer endDrag to " + event_data.pointerDrag);
						ExecuteEvents.Execute (event_data.pointerDrag, event_data, ExecuteEvents.endDragHandler);

						event_data.pointerDrag = null;
						event_data.dragging = false;
					}
				}
			} else
			{
				if (event_data.dragging)
				{
					GameObject _pointerDrop = ExecuteEvents.GetEventHandler<IDropHandler> (_go);
					if (_pointerDrop == event_data.pointerDrag)
					{
						// In next frame of button from being pressed to unpressed, send Drop and EndDrag if dragging.
						DEBUG ("OnTriggerUpMouse() type: " + type + " send Pointer Drop to " + event_data.pointerDrag);
						ExecuteEvents.Execute (event_data.pointerDrag, event_data, ExecuteEvents.dropHandler);
					}
					DEBUG ("OnTriggerUpMouse() type: " + type + " send Pointer endDrag to " + event_data.pointerDrag);
					ExecuteEvents.Execute (event_data.pointerDrag, event_data, ExecuteEvents.endDragHandler);

					event_data.pointerDrag = null;
					event_data.dragging = false;
				} else
				{
					if (_pointerClick != null)
					{
						if (_pointerClick == event_data.pointerPress)
						{
							// In the frame of button from being pressed to unpressed, send Pointer Click if Click is pending.
							DEBUG ("OnTriggerUpMouse() type: " + type + " send Pointer Click to " + event_data.pointerPress);
							ExecuteEvents.Execute (event_data.pointerPress, event_data, ExecuteEvents.pointerClickHandler);
						} else
						{
							DEBUG ("OnTriggerUpMouse() type: " + type
							+ " pointer down object " + event_data.pointerPress
							+ " is different with click object " + _pointerClick);
						}
					}
				}
			}
		}

		// Down of pending Click object.
		event_data.pointerPress = null;
		// press happened (even not handled) object.
		event_data.rawPointerPress = null;
		// clear pending state.
		event_data.eligibleForClick = false;
		// Click is processed, clearcount.
		event_data.clickCount = 0;
		// Up is processed thus clear the time limitation of Down event.
		event_data.clickTime = 0;
	}

	private void OnDrag(WaveVR_Controller.EDeviceType type, PointerEventData event_data)
	{
		if (Time.unscaledTime - event_data.clickTime < DRAG_TIME)
			return;
		if (event_data.pointerDrag == null)
			return;

		if (event_data.pointerDrag != null && !event_data.dragging)
		{
			DEBUG ("OnDrag() device: " + type + " send BeginDrag to " + event_data.pointerDrag);
			ExecuteEvents.Execute(event_data.pointerDrag, event_data, ExecuteEvents.beginDragHandler);
			event_data.dragging = true;
		}

		// Drag notification
		if (event_data.dragging && event_data.pointerDrag != null)
		{
			// Before doing drag we should cancel any pointer down state
			if (event_data.pointerPress != null && event_data.pointerPress != event_data.pointerDrag)
			{
				DEBUG ("OnDrag device: " + type + " send Pointer Up to " + event_data.pointerPress + ", drag object: " + event_data.pointerDrag);
				ExecuteEvents.Execute(event_data.pointerPress, event_data, ExecuteEvents.pointerUpHandler);

				// since Down state is cleaned, no Click should be processed.
				event_data.eligibleForClick = false;
				event_data.pointerPress = null;
				event_data.rawPointerPress = null;
			}
			/*
			DEBUG ("OnDrag() device: " + type + " send Pointer Drag to " + event_data.pointerDrag +
				"camera: " + event_data.enterEventCamera +
				" (" + event_data.enterEventCamera.ScreenToWorldPoint (
					new Vector3 (
						event_data.position.x,
						event_data.position.y,
						event_data.pointerDrag.transform.position.z
					)) +
				")");
			*/
			ExecuteEvents.Execute(event_data.pointerDrag, event_data, ExecuteEvents.dragHandler);
		}
	}

	private void OnDragMouse(WaveVR_Controller.EDeviceType type, PointerEventData event_data)
	{
		if (Time.unscaledTime - event_data.clickTime < DRAG_TIME)
			return;
		if (event_data.pointerDrag == null)
			return;

		if (!event_data.dragging)
		{
			DEBUG ("OnDragMouse() device: " + type + " send BeginDrag to " + event_data.pointerDrag);
			ExecuteEvents.Execute(event_data.pointerDrag, event_data, ExecuteEvents.beginDragHandler);
			event_data.dragging = true;
		} else
		{
			ExecuteEvents.Execute(event_data.pointerDrag, event_data, ExecuteEvents.dragHandler);
		}
	}

	private void OnTriggerHover(WaveVR_Controller.EDeviceType type, PointerEventData event_data)
	{
		GameObject _go = GetRaycastedObject (type);

		ExecuteEvents.ExecuteHierarchy(_go, event_data, WaveVR_ExecuteEvents.pointerHoverHandler);
	}

	private void OnTriggerEnterAndExit(WaveVR_Controller.EDeviceType type, PointerEventData event_data)
	{
		GameObject _go = GetRaycastedObject (type);

		if (event_data.pointerEnter != _go)
		{
			DEBUG ("OnTriggerEnterAndExit() " + type + ", enter: " + _go + ", exit: " + event_data.pointerEnter);

			HandlePointerExitAndEnter (event_data, _go);

			DEBUG ("OnTriggerEnterAndExit() " + type + ", pointerEnter: " + event_data.pointerEnter + ", camera: " + event_data.enterEventCamera);
		}
	}
	/*
	private Stack<GameObject> enterObjects = new Stack<GameObject> ();
	private void OnTriggerEnterAndExit(WaveVR_Controller.EDeviceType type, PointerEventData event_data)
	{
		GameObject _go = GetRaycastedObject (type);

		// ---------------- Exit event ---------------
		// Casting to nothing, exit all previous objects.
		if (_go == null)
		{
			event_data.pointerEnter = null;

			while (enterObjects.Count > 0)
			{
				GameObject _prev = enterObjects.Pop ();
				DEBUG ("OnTriggerEnterAndExit() " + type + ", exit: " + _prev + " due to no casting object.");
				ExecuteEvents.Execute (_prev, event_data, ExecuteEvents.pointerExitHandler);
			}

			return;
		}

		// Meaning hover.
		if (_go == event_data.pointerEnter)
			return;


		if (event_data.pointerEnter != null && !_go.transform.IsChildOf (event_data.pointerEnter.transform))
		{
			DEBUG ("OnTriggerEnterAndExit() " + type + ", exit: " + event_data.pointerEnter);
			ExecuteEvents.Execute (event_data.pointerEnter, event_data, ExecuteEvents.pointerExitHandler);

			enterObjects.Pop ();
			if (enterObjects.Count > 0)
				event_data.pointerEnter = enterObjects.Peek ();
			else
				event_data.pointerEnter = null;
		}

		// ---------------- Enter event ------------------
		if (_go != event_data.pointerEnter)
		{
			DEBUG ("OnTriggerEnterAndExit() " + type + ", enter: " + _go.name);
			event_data.pointerEnter = _go;
			ExecuteEvents.Execute (event_data.pointerEnter, event_data, ExecuteEvents.pointerEnterHandler);
			enterObjects.Push(event_data.pointerEnter);
		}
	}
	*/
	#endregion

	private void onButtonClick(EventController event_controller)
	{
		GameObject _go = GetRaycastedObject (event_controller.device);
		event_controller.eligibleForButtonClick = false;

		if (_go == null)
			return;

		Button _btn = _go.GetComponent<Button> ();
		if (_btn != null)
		{
			DEBUG ("onButtonClick() trigger Button.onClick to " + _btn + " from " + event_controller.device);
			_btn.onClick.Invoke ();
		} else
		{
			DEBUG ("onButtonClick() " + event_controller.device + ", " + _go.name + " does NOT contain Button!");
		}
	}

	private Vector2 centerOfScreen = new Vector2 (0.5f * Screen.width, 0.5f * Screen.height);
	private void ResetPointerEventData(WaveVR_Controller.EDeviceType type)
	{
		EventController event_controller = GetEventController (type);
		if (event_controller != null)
		{
			if (event_controller.event_data == null)
				event_controller.event_data = new PointerEventData (eventSystem);

			event_controller.event_data.Reset ();
			event_controller.event_data.position = centerOfScreen; // center of screen
		}
	}

	private void ResetPointerEventData_Hybrid(WaveVR_Controller.EDeviceType type, Camera eventCam)
	{
		EventController event_controller = GetEventController (type);
		if (event_controller != null && eventCam != null)
		{
			if (event_controller.event_data == null)
				event_controller.event_data = new PointerEventData(EventSystem.current);

			event_controller.event_data.Reset();
			event_controller.event_data.position = new Vector2(0.5f * eventCam.pixelWidth, 0.5f * eventCam.pixelHeight); // center of screen
		}
	}

	/**
	 * @brief get intersection position in world space
	 **/
	private Vector3 GetIntersectionPosition(Camera cam, RaycastResult raycastResult)
	{
		// Check for camera
		if (cam == null) {
			return Vector3.zero;
		}

		float intersectionDistance = raycastResult.distance + cam.nearClipPlane;
		Vector3 intersectionPosition = cam.transform.forward * intersectionDistance + cam.transform.position;
		return intersectionPosition;
	}

	private GameObject GetRaycastedObject(WaveVR_Controller.EDeviceType type)
	{
		EventController event_controller = GetEventController (type);
		if (event_controller != null)
		{
			PointerEventData _ped = event_controller.event_data;
			if (_ped != null)
				return _ped.pointerCurrentRaycast.gameObject;
		}
		return null;
	}

	private void CheckBeamPointerActive(EventController eventController)
	{
		if (eventController == null)
			return;

		if (eventController.pointer != null)
		{
			bool _enabled = eventController.pointer.gameObject.activeSelf && eventController.pointer.ShowPointer;
			if (eventController.pointerEnabled != _enabled)
			{
				eventController.pointerEnabled = _enabled;
				this.toChangeBeamPointer = eventController.pointerEnabled;
				DEBUG ("CheckBeamPointerActive() " + eventController.device + ", pointer is " + (eventController.pointerEnabled ? "active." : "inactive."));
			}
		} else
		{
			eventController.pointerEnabled = false;
		}

		if (eventController.beam != null)
		{
			bool _enabled = eventController.beam.gameObject.activeSelf && eventController.beam.ShowBeam;
			if (eventController.beamEnabled != _enabled)
			{
				eventController.beamEnabled = _enabled;
				this.toChangeBeamPointer = eventController.beamEnabled;
				DEBUG ("CheckBeamPointerActive() " + eventController.device + ", beam is " + (eventController.beamEnabled ? "active." : "inactive."));
			}
		} else
		{
			eventController.beamEnabled = false;
		}
	}

	private void SetPointerCameraTracker()
	{
		for (int i = 0; i < WaveVR_Controller.DeviceTypes.Length; i++)
		{
			WaveVR_Controller.EDeviceType dev_type = WaveVR_Controller.DeviceTypes [i];
			// HMD uses Gaze, not controller input module.
			if (dev_type == WaveVR_Controller.EDeviceType.Head)
				continue;

			if (GetEventController(dev_type) == null)
				continue;

			WaveVR_PointerCameraTracker pcTracker = null;

			switch (dev_type)
			{
			case WaveVR_Controller.EDeviceType.Dominant:
				if (pointCameraDomint != null)
					pcTracker = pointCameraDomint.GetComponent<WaveVR_PointerCameraTracker> ();
				break;
			case WaveVR_Controller.EDeviceType.NonDominant:
				if (pointCameraNoDomt != null)
					pcTracker = pointCameraNoDomt.GetComponent<WaveVR_PointerCameraTracker> ();
				break;
			default:
				break;
			}

			if (pcTracker != null && pcTracker.reticleObject == null)
			{
				EventController event_controller = GetEventController (dev_type);
				bool isConnected = true;
				WaveVR.Device wdev = WaveVR.Instance.getDeviceByType (dev_type);
				if (wdev != null)
					isConnected = wdev.connected;

				if (event_controller != null && isConnected)
				{
					if (event_controller.pointer == null && event_controller.controller != null)
						event_controller.pointer = event_controller.controller.GetComponentInChildren<WaveVR_ControllerPointer> ();
					if (event_controller.pointer != null)
					{
						pcTracker.reticleObject = event_controller.pointer.gameObject;
					}
				}
			}
		}
	}

	private GameObject[] MergeArray(GameObject[] start, GameObject[] end)
	{
		GameObject[] _merged = null;

		if (start == null)
		{
			if (end != null)
				_merged = end;
		} else
		{
			if (end == null)
			{
				_merged = start;
			} else
			{
				uint _duplicate = 0;
				for (int i = 0; i < start.Length; i++)
				{
					for (int j = 0; j < end.Length; j++)
					{
						if (GameObject.ReferenceEquals (start [i], end [j]))
						{
							_duplicate++;
							end [j] = null;
						}
					}
				}

				_merged = new GameObject[start.Length + end.Length - _duplicate];
				uint _merge_index = 0;

				for (int i = 0; i < start.Length; i++)
					_merged [_merge_index++] = start [i];

				for (int j = 0; j < end.Length; j++)
				{
					if (end [j] != null)
						_merged [_merge_index++] = end [j];
				}

				//Array.Copy (start, _merged, start.Length);
				//Array.Copy (end, 0, _merged, start.Length, end.Length);
			}
		}

		return _merged;
	}

	private void CopyList(List<GameObject> src, List<GameObject> dst)
	{
		dst.Clear ();
		for (int i = 0; i < src.Count; i++)
			dst.Add (src [i]);
	}
}
