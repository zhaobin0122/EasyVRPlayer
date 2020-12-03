// "WaveVR SDK 
// © 2017 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the WaveVR SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

#pragma warning disable 0414 // private field assigned but not used.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using WVR_Log;
using UnityEngine.UI;
using wvr;

#if UNITY_EDITOR
using UnityEditor;

#pragma warning disable 0618
[CustomEditor(typeof(GazeInputModule))]
public class GazeInputModuleEditor : Editor
{
	public override void OnInspectorGUI()
	{
		GazeInputModule myScript = target as GazeInputModule;
		if (myScript != null)
		{
			myScript.progressRate = EditorGUILayout.Toggle ("Progress Rate", myScript.progressRate);
			myScript.RateTextZPosition = EditorGUILayout.FloatField("Rate Text Z Position", myScript.RateTextZPosition);
			myScript.progressCounter = EditorGUILayout.Toggle ("Progress Counter", myScript.progressCounter);
			myScript.CounterTextZPosition = EditorGUILayout.FloatField("Counter Text Z Position", myScript.CounterTextZPosition);
			myScript.TimeToGaze = EditorGUILayout.FloatField("Time To Gaze", myScript.TimeToGaze);
			myScript.InputEvent = (EGazeInputEvent) EditorGUILayout.EnumPopup ("Input Event", myScript.InputEvent);
			myScript.Head = (GameObject) EditorGUILayout.ObjectField("Head", myScript.Head, typeof(GameObject), true);
			if (myScript.enabled)
			{
				myScript.BtnControl = EditorGUILayout.Toggle ("BtnControl", myScript.BtnControl);
				if (myScript.BtnControl)
				{
					myScript.GazeDevice = (EGazeTriggerDevice) EditorGUILayout.EnumPopup ("		Gaze Trigger Device", myScript.GazeDevice);
					myScript.ButtonToTrigger = (EGazeTriggerButton) EditorGUILayout.EnumPopup ("		Button To Trigger", myScript.ButtonToTrigger);
					myScript.WithTimeGaze = EditorGUILayout.Toggle ("		With Time Gaze", myScript.WithTimeGaze);
				}
			}
		}

		if (GUI.changed)
			EditorUtility.SetDirty ((GazeInputModule)target);
	}
}
#pragma warning restore 0618
#endif

public enum EGazeTriggerMouseKey
{
	LeftClick,
	RightClick,
	MiddleClick
}

public enum EGazeTriggerButton
{
	System = WVR_InputId.WVR_InputId_Alias1_System,
	Menu = WVR_InputId.WVR_InputId_Alias1_Menu,
	Grip = WVR_InputId.WVR_InputId_Alias1_Grip,
	DPad_Left = WVR_InputId.WVR_InputId_Alias1_DPad_Left,
	DPad_Up = WVR_InputId.WVR_InputId_Alias1_DPad_Up,
	DPad_Right = WVR_InputId.WVR_InputId_Alias1_DPad_Right,
	DPad_Down = WVR_InputId.WVR_InputId_Alias1_DPad_Down,
	Volume_Up = WVR_InputId.WVR_InputId_Alias1_Volume_Up,
	Volume_Down = WVR_InputId.WVR_InputId_Alias1_Volume_Down,
	DigitalTrigger = WVR_InputId.WVR_InputId_Alias1_Digital_Trigger,
	Touchpad = WVR_InputId.WVR_InputId_Alias1_Touchpad,
	Trigger = WVR_InputId.WVR_InputId_Alias1_Trigger
}

public enum EGazeTriggerDevice
{
	HMD,
	NonDominantController,
	DominantController,
	HMDWithNonDominantController,
	HMDWithDominantController,
	HMDWithTwoControllers
}

public enum EGazeInputEvent
{
	PointerDown,
	PointerClick,
	PointerSubmit
}

[System.Obsolete("This script is obsoleted, please use WaveVR_GazeInputModule instead.")]
public class GazeInputModule : PointerInputModule
{
	private static string LOG_TAG = "GazeInputModule";
	private void PrintDebugLog(string msg)
	{
		Log.d (LOG_TAG, msg, true);
	}

	public bool progressRate = false;  // The switch to show how many percent to click by TimeToGaze
	public float RateTextZPosition = 0.5f;
	public bool progressCounter = false;  // The switch to show how long to click by TimeToGaze
	public float CounterTextZPosition = 0.5f;
	public float TimeToGaze = 2.0f;
	public EGazeInputEvent InputEvent = EGazeInputEvent.PointerSubmit;
	public GameObject Head = null;
	public bool BtnControl = false;
	[HideInInspector]
	public EGazeTriggerDevice GazeDevice = EGazeTriggerDevice.HMD;
	[HideInInspector]
	public EGazeTriggerButton ButtonToTrigger = EGazeTriggerButton.Trigger;
	[HideInInspector]
	public bool WithTimeGaze = false;
	private bool defWithTimeGaze = false;
	public void SetWithTimeGaze(bool withTimer)
	{
		if (Log.gpl.Print)
			Log.d (LOG_TAG, "SetWithTimeGaze() withTimer: " + withTimer, true);
		this.WithTimeGaze = withTimer;
		this.defWithTimeGaze = WithTimeGaze;
	}
	private bool btnPressDown = false;
	private bool btnPressed = false;
	private bool btnPressUp = false;
	private bool HmdEnterPressDown = false;
	private float currUnscaledTime = 0;

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
		Vector3 intersectionPosition = cam.transform.position + cam.transform.forward * intersectionDistance;
		return intersectionPosition;
	}

	private PointerEventData pointerData;

	private void CastToCenterOfScreen()
	{
		if (pointerData == null)
			pointerData = new PointerEventData (eventSystem);

		pointerData.Reset();
		pointerData.position = new Vector2 (0.5f * Screen.width, 0.5f * Screen.height);  // center of screen

		if (Head != null)
		{
			Camera _event_camera = Head.GetComponent<Camera> ();
			GraphicRaycast (_event_camera);

			if (pointerData.pointerCurrentRaycast.gameObject == null)
			{
				PhysicsRaycaster _raycaster = Head.GetComponent<PhysicsRaycaster> ();
				PhysicRaycast (_raycaster);
			}
		}
	}

	private void GraphicRaycast(Camera event_camera)
	{
		List<RaycastResult> _raycast_results = new List<RaycastResult>();

		// Reset pointerCurrentRaycast even no GUI.
		RaycastResult _firstResult = new RaycastResult ();
		pointerData.pointerCurrentRaycast = _firstResult;

		foreach (Canvas _canvas in sceneCanvases)
		{
			GraphicRaycaster _gr = _canvas.GetComponent<GraphicRaycaster> ();
			if (_gr == null)
				continue;

			// 1. Change event camera.
			_canvas.worldCamera = event_camera;

			// 2.
			_gr.Raycast (pointerData, _raycast_results);

			_firstResult = FindFirstRaycast (_raycast_results);
			pointerData.pointerCurrentRaycast = _firstResult;
			_raycast_results.Clear ();

			if (_firstResult.module != null)
			{
				//PrintDebugLog ("GraphicRaycast() device: " + event_controller.device + ", camera: " + _firstResult.module.eventCamera + ", first result = " + _firstResult);
			}

			// Found graphic raycasted object!
			if (_firstResult.gameObject != null)
			{
				if (_firstResult.worldPosition == Vector3.zero)
				{
					_firstResult.worldPosition = GetIntersectionPosition (
						_firstResult.module.eventCamera,
						//_eventController.event_data.enterEventCamera,
						_firstResult
					);
					pointerData.pointerCurrentRaycast = _firstResult;
				}

				pointerData.position = _firstResult.screenPosition;
				break;
			}
		}
	}

	private void PhysicRaycast(PhysicsRaycaster raycaster)
	{
		if (raycaster == null)
			return;

		List<RaycastResult> _raycast_results = new List<RaycastResult>();
		raycaster.Raycast (pointerData, _raycast_results);

		RaycastResult _firstResult = FindFirstRaycast (_raycast_results);
		pointerData.pointerCurrentRaycast = _firstResult;

		//PrintDebugLog ("PhysicRaycast() first result = " + _firstResult);

		if (_firstResult.gameObject != null)
		{
			if (_firstResult.worldPosition == Vector3.zero)
			{
				_firstResult.worldPosition = GetIntersectionPosition (
					_firstResult.module.eventCamera,
					//_eventController.event_data.enterEventCamera,
					_firstResult
				);
				pointerData.pointerCurrentRaycast = _firstResult;
			}

			pointerData.position = _firstResult.screenPosition;
		}
	}

	private float gazeTime = 0.0f;
	// { ------- Reticle --------
	private Text progressText = null;
	private Text counterText = null;
	private WaveVR_Reticle gazePointer = null;
	private GameObject percentCanvas = null, counterCanvas = null;

	private GameObject GetCurrentGameObject(PointerEventData pointerData) {
		if (pointerData != null && pointerData.enterEventCamera != null)
			return pointerData.pointerCurrentRaycast.gameObject;

		return null;
	}

	private Vector3 GetIntersectionPosition(PointerEventData pointerData) {
		if (null == pointerData.enterEventCamera)
			return Vector3.zero;

		float intersectionDistance = pointerData.pointerCurrentRaycast.distance + pointerData.enterEventCamera.nearClipPlane;
		Vector3 intersectionPosition = pointerData.enterEventCamera.transform.position + pointerData.enterEventCamera.transform.forward * intersectionDistance;
		return intersectionPosition;
	}

	private void UpdateProgressDistance(PointerEventData pointerEvent) {
		Vector3 intersectionPosition = GetIntersectionPosition(pointerEvent);
		if (gazePointer == null)
			return;

		if (percentCanvas != null) {
			Vector3 tmpVec = new Vector3(percentCanvas.transform.localPosition.x, percentCanvas.transform.localPosition.y, intersectionPosition.z - (RateTextZPosition >= 0 ? RateTextZPosition : 0));
			percentCanvas.transform.localPosition = tmpVec;
		}

		if (counterCanvas != null) {
			Vector3 tmpVec = new Vector3(counterCanvas.transform.localPosition.x, counterCanvas.transform.localPosition.y, intersectionPosition.z - (CounterTextZPosition >= 0 ? CounterTextZPosition : 0));
			counterCanvas.transform.localPosition = tmpVec;
		}
	}

	private void UpdateReticle (GameObject preGazedObject, PointerEventData pointerEvent) {
		if (gazePointer == null)
			return;

		GameObject curGazeObject = GetCurrentGameObject(pointerEvent);
		Vector3 intersectionPosition = GetIntersectionPosition(pointerEvent);
		bool isInteractive = pointerEvent.pointerPress != null || ExecuteEvents.GetEventHandler<IPointerClickHandler>(curGazeObject) != null;

		if (curGazeObject == preGazedObject) {
			if (curGazeObject != null) {
				gazePointer.OnGazeStay(pointerEvent.enterEventCamera, curGazeObject, intersectionPosition, isInteractive);
			} else {
				gazePointer.OnGazeExit(pointerEvent.enterEventCamera, preGazedObject);
				return;
			}
		} else {
			if (preGazedObject != null) {
				gazePointer.OnGazeExit(pointerEvent.enterEventCamera, preGazedObject);
			}
			if (curGazeObject != null) {
				gazePointer.OnGazeEnter(pointerEvent.enterEventCamera, curGazeObject, intersectionPosition, isInteractive);
			}
		}
		UpdateProgressDistance(pointerEvent);
	}
	// --------- Reticle -------- }

	private void UpdateButtonStates()
	{
		btnPressDown = Input.GetMouseButtonDown ((int)EGazeTriggerMouseKey.LeftClick);
		btnPressed = Input.GetMouseButton ((int)EGazeTriggerMouseKey.LeftClick);
		btnPressUp = Input.GetMouseButtonUp ((int)EGazeTriggerMouseKey.LeftClick);
		if (GazeDevice == EGazeTriggerDevice.HMD ||
			GazeDevice == EGazeTriggerDevice.HMDWithNonDominantController ||
			GazeDevice == EGazeTriggerDevice.HMDWithDominantController ||
			GazeDevice == EGazeTriggerDevice.HMDWithTwoControllers)
		{
			btnPressDown |= WaveVR_Controller.Input (WaveVR_Controller.EDeviceType.Head).GetPressDown ((WVR_InputId)ButtonToTrigger);
			btnPressed |= WaveVR_Controller.Input (WaveVR_Controller.EDeviceType.Head).GetPress ((WVR_InputId)ButtonToTrigger);
			btnPressUp |= WaveVR_Controller.Input (WaveVR_Controller.EDeviceType.Head).GetPressUp ((WVR_InputId)ButtonToTrigger);
		}

		if (GazeDevice == EGazeTriggerDevice.NonDominantController ||
			GazeDevice == EGazeTriggerDevice.HMDWithNonDominantController ||
			GazeDevice == EGazeTriggerDevice.HMDWithTwoControllers)
		{
			btnPressDown |= WaveVR_Controller.Input (WaveVR_Controller.EDeviceType.NonDominant).GetPressDown ((WVR_InputId)ButtonToTrigger);
			btnPressed |= WaveVR_Controller.Input (WaveVR_Controller.EDeviceType.NonDominant).GetPress ((WVR_InputId)ButtonToTrigger);
			btnPressUp |= WaveVR_Controller.Input (WaveVR_Controller.EDeviceType.NonDominant).GetPressUp ((WVR_InputId)ButtonToTrigger);
		}

		if (GazeDevice == EGazeTriggerDevice.DominantController ||
			GazeDevice == EGazeTriggerDevice.HMDWithDominantController ||
			GazeDevice == EGazeTriggerDevice.HMDWithTwoControllers)
		{
			btnPressDown |= WaveVR_Controller.Input (WaveVR_Controller.EDeviceType.Dominant).GetPressDown ((WVR_InputId)ButtonToTrigger);
			btnPressed |= WaveVR_Controller.Input (WaveVR_Controller.EDeviceType.Dominant).GetPress ((WVR_InputId)ButtonToTrigger);
			btnPressUp |= WaveVR_Controller.Input (WaveVR_Controller.EDeviceType.Dominant).GetPressUp ((WVR_InputId)ButtonToTrigger);
		}
	}

	private void UpdateProgressText()
	{
		if (!this.progressRate || this.Head == null)
		{
			if (this.progressText != null)
				this.progressText.text = "";
			return;
		}

		if (this.progressText == null)
		{
			Text[] _texts = this.Head.transform.GetComponentsInChildren<Text> ();
			foreach (Text _text in _texts)
			{
				if (_text.gameObject.name.Equals ("ProgressText"))
				{
					PrintDebugLog ("UpdateProgressText() Found ProgressText.");
					this.progressText = _text;
					break;
				}
			}
		}

		if (this.progressText == null)
			return;

		GameObject _curr_go = pointerData.pointerCurrentRaycast.gameObject;
		if (_curr_go == null)
		{
			this.progressText.text = "";
			return;
		}

		float _rate = (((this.currUnscaledTime - this.gazeTime) % TimeToGaze) / TimeToGaze) * 100;
		this.progressText.text = Mathf.Floor (_rate) + "%";
	}

	private void UpdateCounterText()
	{
		if (!this.progressCounter || this.Head == null)
		{
			if (this.counterText != null)
				this.counterText.text = "";
			return;
		}

		if (this.counterText == null)
		{
			Text[] _texts = this.Head.transform.GetComponentsInChildren<Text> ();
			foreach (Text _text in _texts)
			{
				if (_text.gameObject.name.Equals ("CounterText"))
				{
					PrintDebugLog ("UpdateCounterText() Found CounterText.");
					this.counterText = _text;
					break;
				}
			}
		}

		if (this.counterText == null)
			return;

		GameObject _curr_go = pointerData.pointerCurrentRaycast.gameObject;
		if (_curr_go == null)
		{
			this.counterText.text = "";
			return;
		}

		if (counterText != null)
			counterText.text = System.Math.Round(TimeToGaze - ((this.currUnscaledTime - this.gazeTime) % TimeToGaze), 2).ToString();
	}

	private void OnTriggeGaze()
	{
		// The gameobject to which raycast positions
		var currentOverGO = pointerData.pointerCurrentRaycast.gameObject;
		UpdateReticle(currentOverGO, pointerData);

		bool sendEvent = false;
		this.HmdEnterPressDown = WaveVR_Controller.Input (WaveVR_Controller.EDeviceType.Head).GetPressDown (WVR_InputId.WVR_InputId_Alias1_Enter);
		if (this.HmdEnterPressDown)
			sendEvent = true;

		if (pointerData.pointerEnter != currentOverGO)
		{
			PrintDebugLog ("OnTriggeGaze() pointerEnter: " + pointerData.pointerEnter + ", currentOverGO: " + currentOverGO);
			HandlePointerExitAndEnter (pointerData, currentOverGO);

			if (currentOverGO != null)
				gazeTime = this.currUnscaledTime;
		}
		else
		{
			if (currentOverGO != null)
			{
				if (gazePointer != null)
					gazePointer.triggerProgressBar (true);

				if (this.currUnscaledTime - gazeTime > TimeToGaze)
				{
					sendEvent = true;
					gazeTime = this.currUnscaledTime;
				} else
				{
					float rate = ((this.currUnscaledTime - gazeTime) / TimeToGaze) * 100;
					if (gazePointer != null)
						gazePointer.setProgressBarTime (rate);
				}

				if (this.BtnControl)
				{
					if (!this.WithTimeGaze)
					{
						gazeTime = this.currUnscaledTime;
						gazePointer.triggerProgressBar (false);
					}

					UpdateButtonStates ();
					if (btnPressDown)
					{
						sendEvent = true;
						this.gazeTime = this.currUnscaledTime;
					}
				}
			} else
			{
				if (gazePointer != null)
					gazePointer.triggerProgressBar (false);
			}
		}

		// Standalone Input Module information
		pointerData.delta = Vector2.zero;
		pointerData.dragging = false;

		DeselectIfSelectionChanged (currentOverGO, pointerData);

		if (sendEvent)
		{
			PrintDebugLog ("OnTriggeGaze() selected " + currentOverGO.name);
			if (InputEvent == EGazeInputEvent.PointerClick)
			{
				ExecuteEvents.ExecuteHierarchy (currentOverGO, pointerData, ExecuteEvents.pointerClickHandler);
				pointerData.clickTime = this.currUnscaledTime;
			} else if (InputEvent == EGazeInputEvent.PointerDown)
			{
				// like "mouse" action, press->release soon, do NOT keep the pointerPressRaycast cause do NOT need to controll "down" object while not gazing.
				pointerData.pressPosition = pointerData.position;
				pointerData.pointerPressRaycast = pointerData.pointerCurrentRaycast;

				var _pointerDownGO = ExecuteEvents.ExecuteHierarchy (currentOverGO, pointerData, ExecuteEvents.pointerDownHandler);
				ExecuteEvents.ExecuteHierarchy (_pointerDownGO, pointerData, ExecuteEvents.pointerUpHandler);
			} else if (InputEvent == EGazeInputEvent.PointerSubmit)
			{
				ExecuteEvents.ExecuteHierarchy (currentOverGO, pointerData, ExecuteEvents.submitHandler);
			}
		}
	}

	private void GazeControl()
	{
		bool _connD = WaveVR_Controller.Input (WaveVR_Controller.EDeviceType.Dominant).connected;
		bool _connN = WaveVR_Controller.Input (WaveVR_Controller.EDeviceType.NonDominant).connected;

		if (WaveVR_ButtonList.Instance == null)
		{
			// Set timer gaze if no button support.
			this.WithTimeGaze = true;
		} else
		{
			this.WithTimeGaze = this.defWithTimeGaze;
			if (!_connD && !_connN)
			{
				// Set timer gaze if no controller connected and HMD enter is unavailable.
				if (!WaveVR_ButtonList.Instance.IsButtonAvailable (WaveVR_Controller.EDeviceType.Head, WVR_InputId.WVR_InputId_Alias1_Enter))
					this.WithTimeGaze = true;
			}
		}

		CastToCenterOfScreen ();

		this.currUnscaledTime = Time.unscaledTime;
		OnTriggeGaze();

		UpdateProgressText ();
		UpdateCounterText ();
	}

	private bool EnableGaze = false;
	private Canvas[] sceneCanvases = null;
	protected override void OnEnable()
	{
		base.OnEnable ();

		EnableGaze = true;

		if (gazePointer == null)
		{
			// Set gazePointer only when null, or it will got null when WaveVR_Reticle gameObject is SetActive(false).
			if (Head == null)
			{
				if (WaveVR_InputModuleManager.Instance != null)
					Head = WaveVR_InputModuleManager.Instance.gameObject;
				else
					Head = WaveVR_Render.Instance.gameObject;
			}
			if (Head != null)
				gazePointer = Head.GetComponentInChildren<WaveVR_Reticle> ();
		}

		if (gazePointer != null)
		{
			PrintDebugLog ("OnEnable() Head: " + Head.name + ", enable pointer, percent and counter canvas.");
			percentCanvas = gazePointer.transform.Find ("PercentCanvas").gameObject;
			counterCanvas = gazePointer.transform.Find ("CounterCanvas").gameObject;
			ActivaeGazePointerCanvas (true);
		}

		sceneCanvases = GameObject.FindObjectsOfType<Canvas> ();
		this.defWithTimeGaze = this.WithTimeGaze;
	}

	protected override void OnDisable()
	{
		base.OnDisable ();

		EnableGaze = false;
		ActivaeGazePointerCanvas (false);

		if (pointerData != null)
			HandlePointerExitAndEnter (pointerData, null);
	}

	private bool focusCapturedBySystem = false;
	private void ActivaeGazePointerCanvas(bool active)
	{
		if (gazePointer != null)
		{
			MeshRenderer _mr = gazePointer.gameObject.GetComponentInChildren<MeshRenderer>();
			if (_mr != null)
			{
				PrintDebugLog ("ActivaeGazePointerCanvas() " + (active ? "enable" : "disable") + " pointer.");
				_mr.enabled = active;
			} else
			{
				Log.e (LOG_TAG, "ActivaeGazePointerCanvas() Oooooooooooooops! Why no MeshRender!!??");
			}
		}
		if (percentCanvas != null)
		{
			PrintDebugLog ("ActivaeGazePointerCanvas() " + (active ? "enable" : "disable") + " percentCanvas.");
			percentCanvas.SetActive (active);
		}
		if (counterCanvas != null)
		{
			PrintDebugLog ("ActivaeGazePointerCanvas() " + (active ? "enable" : "disable") + " counterCanvas.");
			counterCanvas.SetActive (active);
		}
	}
	public override void Process()
	{
		if (WaveVR.Instance.Initialized)
		{
			if (focusCapturedBySystem != WaveVR.Instance.FocusCapturedBySystem)
			{
				focusCapturedBySystem = WaveVR.Instance.FocusCapturedBySystem;
				// Do not gaze if focus is cpatured by system.
				if (focusCapturedBySystem)
				{
					PrintDebugLog ("Process() focus is captured by system.");
					EnableGaze = false;
					ActivaeGazePointerCanvas (false);

					if (pointerData != null)
						HandlePointerExitAndEnter (pointerData, null);
				} else
				{
					PrintDebugLog ("Process() focus is gained.");
					EnableGaze = true;
					ActivaeGazePointerCanvas (true);
				}
			}
		}

		if (EnableGaze)
			GazeControl ();
	}
}
