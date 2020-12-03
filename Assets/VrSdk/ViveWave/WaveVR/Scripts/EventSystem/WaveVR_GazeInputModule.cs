// "WaveVR SDK 
// © 2017 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the WaveVR SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."
using UnityEngine.Profiling;

#pragma warning disable 0414 // private field assigned but not used.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using WVR_Log;
using UnityEngine.UI;
using wvr;

public class WaveVR_GazeInputModule : PointerInputModule
{
	private static string LOG_TAG = "WaveVR_GazeInputModule";
	private void DEBUG(string msg)
	{
		if (Log.EnableDebugLog)
			Log.d (LOG_TAG, msg, true);
	}

	public enum EGazeTriggerMouseKey
	{
		LeftClick,
		RightClick,
		MiddleClick
	}

	public enum EGazeInputEvent
	{
		PointerDown,
		PointerClick,
		PointerSubmit
	}

	#region Editor Variables.
	public bool UseWaveVRReticle = false;
	private bool useWaveVRReticle = false;

	public bool TimerControl = true;
	private bool timerControlDefault = false;
	public void EnableTimerControl(bool enable)
	{
		if (Log.gpl.Print)
			DEBUG ("EnableTimerControl() enable: " + enable);
		this.TimerControl = enable;
		this.timerControlDefault = TimerControl;
	}
	public float TimeToGaze = 2.0f;

	public bool ProgressRate = false;  // The switch to show how many percent to click by TimeToGaze
	public float RateTextZPosition = 0.5f;
	public bool ProgressCounter = false;  // The switch to show how long to click by TimeToGaze
	public float CounterTextZPosition = 0.5f;

	public EGazeInputEvent InputEvent = EGazeInputEvent.PointerSubmit;
	public bool ButtonControl = false;
	public List<WaveVR_Controller.EDeviceType> ButtonControlDevices = new List<WaveVR_Controller.EDeviceType>();
	public List<WaveVR_ButtonList.EButtons> ButtonControlKeys = new List<WaveVR_ButtonList.EButtons>();

	public GameObject Head = null;
	#endregion

	private bool btnPressDown = false;
	private bool btnPressed = false;
	private bool btnPressUp = false;
	private bool HmdEnterPressDown = false;
	private float currUnscaledTime = 0;
	private Vector3 gazeTargetPos = Vector3.zero;
	private Vector3 gazeScreenPos = Vector3.zero;
	private Vector3 gazeScreenPos2D = Vector2.zero;

	#region Raycast
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

	private PointerEventData pointerData = null;
	private void ResetPointerEventData()
	{
		if (pointerData == null)
			pointerData = new PointerEventData (eventSystem);

		//pointerData.Reset ();
		pointerData.position = gazeScreenPos2D;
	}

	private void CastToCenterOfScreen()
	{
		Camera _event_camera = Head.GetComponent<Camera> ();

		// center of screen
		gazeScreenPos2D.x = 0.5f * Screen.width;
		gazeScreenPos2D.y = 0.5f * Screen.height;
		gazeTargetPos = Vector3.zero;

		if (ringMesh != null)
		{
			ringMesh.RingPosition = gazeTargetPos;
		}

		ResetPointerEventData ();
		GraphicRaycast (_event_camera);

		PhysicsRaycaster phy_raycaster = null;
		phy_raycaster = Head.GetComponent<PhysicsRaycaster> ();
		if (phy_raycaster != null)
		{
			ResetPointerEventData ();
			PhysicsRaycast (phy_raycaster);
		}
	}

	List<RaycastResult> physicsRaycastResultsGaze = new List<RaycastResult>();
	List<GameObject> physicsRaycastObjectsGaze = new List<GameObject>(), prePhysicsRaycastObjectsGaze = new List<GameObject>();
	List<GameObject> physicsRaycastObjectsTmp = new List<GameObject> (), prePhysicsRaycastObjectsTmp = new List<GameObject>();
	private void EnterExitPhysicsObject()
	{
		physicsRaycastObjectsTmp = physicsRaycastObjectsGaze;
		prePhysicsRaycastObjectsTmp = prePhysicsRaycastObjectsGaze;

		if (physicsRaycastObjectsTmp.Count == 0 && prePhysicsRaycastObjectsTmp.Count == 0)
			return;

		for (int i = 0; i < physicsRaycastObjectsTmp.Count; i++)
		{
			if (!prePhysicsRaycastObjectsTmp.Contains (physicsRaycastObjectsTmp [i]))
			{
				ExecuteEvents.Execute (physicsRaycastObjectsTmp [i], pointerData, ExecuteEvents.pointerEnterHandler);
				DEBUG ("EnterExitPhysicsObject() enter: " + physicsRaycastObjectsTmp [i]);
			}
		}

		for (int i = 0; i < prePhysicsRaycastObjectsTmp.Count; i++)
		{
			if (!physicsRaycastObjectsTmp.Contains (prePhysicsRaycastObjectsTmp [i]))
			{
				ExecuteEvents.Execute (prePhysicsRaycastObjectsTmp [i], pointerData, ExecuteEvents.pointerExitHandler);
				DEBUG ("EnterExitPhysicsObject() exit: " + prePhysicsRaycastObjectsTmp [i]);
			}
		}

		prePhysicsRaycastObjectsTmp.Clear ();
		for (int i = 0; i < physicsRaycastObjectsTmp.Count; i++)
		{
			prePhysicsRaycastObjectsTmp.Add (physicsRaycastObjectsTmp [i]);
		}

		physicsRaycastObjectsGaze = physicsRaycastObjectsTmp;
		prePhysicsRaycastObjectsGaze = prePhysicsRaycastObjectsTmp;
	}

	List<RaycastResult> graphicRaycastResultsGaze = new List<RaycastResult>();
	List<GameObject> graphicRaycastObjectsGaze = new List<GameObject>(), preGraphicRaycastObjectsGaze = new List<GameObject>();
	List<GameObject> graphicRaycastObjectsTmp = new List<GameObject>(), preGraphicRaycastObjectsTmp = new List<GameObject>();

	private void EnterExitGraphicObject()
	{
		graphicRaycastObjectsTmp = graphicRaycastObjectsGaze;
		preGraphicRaycastObjectsTmp = preGraphicRaycastObjectsGaze;

		if (graphicRaycastObjectsTmp.Count == 0 && preGraphicRaycastObjectsTmp.Count == 0)
			return;

		for (int i = 0; i < graphicRaycastObjectsTmp.Count; i++)
		{
			if (!preGraphicRaycastObjectsTmp.Contains (graphicRaycastObjectsTmp [i]))
			{
				ExecuteEvents.Execute (graphicRaycastObjectsTmp [i], pointerData, ExecuteEvents.pointerEnterHandler);
				DEBUG ("EnterExitGraphicObject() enter: " + graphicRaycastObjectsTmp [i]);
			}
		}

		for (int i = 0; i < preGraphicRaycastObjectsTmp.Count; i++)
		{
			if (!graphicRaycastObjectsTmp.Contains (preGraphicRaycastObjectsTmp [i]))
			{
				ExecuteEvents.Execute (preGraphicRaycastObjectsTmp [i], pointerData, ExecuteEvents.pointerExitHandler);
				DEBUG ("EnterExitGraphicObject() exit: " + preGraphicRaycastObjectsTmp [i]);
			}
		}

		preGraphicRaycastObjectsTmp.Clear ();
		for (int i = 0; i < graphicRaycastObjectsTmp.Count; i++)
		{
			preGraphicRaycastObjectsTmp.Add (graphicRaycastObjectsTmp [i]);
		}

		graphicRaycastObjectsGaze = graphicRaycastObjectsTmp;
		preGraphicRaycastObjectsGaze = preGraphicRaycastObjectsTmp;
	}

	private void ExitAllObjects()
	{
		prePhysicsRaycastObjectsTmp = prePhysicsRaycastObjectsGaze;
		preGraphicRaycastObjectsTmp = preGraphicRaycastObjectsGaze;

		if (prePhysicsRaycastObjectsTmp.Count == 0 && preGraphicRaycastObjectsTmp.Count == 0)
			return;

		for (int i = 0; i < prePhysicsRaycastObjectsTmp.Count; i++)
		{
			ExecuteEvents.Execute (prePhysicsRaycastObjectsTmp [i], pointerData, ExecuteEvents.pointerExitHandler);
			DEBUG ("ExitAllObjects() exit: " + prePhysicsRaycastObjectsTmp [i]);
		}

		prePhysicsRaycastObjectsTmp.Clear ();

		for (int i = 0; i < preGraphicRaycastObjectsTmp.Count; i++)
		{
			ExecuteEvents.Execute (preGraphicRaycastObjectsTmp [i], pointerData, ExecuteEvents.pointerExitHandler);
			DEBUG ("ExitAllObjects() exit: " + preGraphicRaycastObjectsTmp [i]);
		}

		preGraphicRaycastObjectsTmp.Clear ();

		prePhysicsRaycastObjectsGaze = prePhysicsRaycastObjectsTmp;
		preGraphicRaycastObjectsGaze = preGraphicRaycastObjectsTmp;
	}

	private void PhysicsRaycast(PhysicsRaycaster raycaster)
	{
		RaycastResult _firstResult = new RaycastResult ();

		physicsRaycastObjectsGaze.Clear ();
		physicsRaycastResultsGaze.Clear ();

		Profiler.BeginSample ("PhysicsRaycaster.Raycast() Gaze.");
		raycaster.Raycast (pointerData, physicsRaycastResultsGaze);
		Profiler.EndSample ();

		for (int i = 0; i < physicsRaycastResultsGaze.Count; i++)
			physicsRaycastObjectsGaze.Add (physicsRaycastResultsGaze [i].gameObject);

		_firstResult = FindFirstRaycast (physicsRaycastResultsGaze);

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
					pointerData.pointerCurrentRaycast.worldPosition.z -
					_firstResult.module.eventCamera.transform.position.z);

			if (pointerData.pointerCurrentRaycast.gameObject == null || origin_dist > new_dist)
			{
				/*
				DEBUG ("PhysicsRaycast()" +
					", raycasted: " + _firstResult.gameObject.name +
					", raycasted position: " + _firstResult.worldPosition +
					", new_dist: " + new_dist +
					", origin target: " +
					(pointerData.pointerCurrentRaycast.gameObject == null ?
						"null" :
						pointerData.pointerCurrentRaycast.gameObject.name) +
					", origin position: " + pointerData.pointerCurrentRaycast.worldPosition +
					", origin distance: " + origin_dist);
				*/
				pointerData.pointerCurrentRaycast = _firstResult;
				pointerData.position = _firstResult.screenPosition;
			}
		}
	}

	private void GraphicRaycast(Camera event_camera)
	{
		// Reset pointerCurrentRaycast even no GUI.
		RaycastResult _firstResult = new RaycastResult ();
		pointerData.pointerCurrentRaycast = _firstResult;

		Profiler.BeginSample ("Find GraphicRaycaster for Gaze.");
		GraphicRaycaster[] _graphic_raycasters = GameObject.FindObjectsOfType<GraphicRaycaster> ();
		Profiler.EndSample ();

		graphicRaycastObjectsGaze.Clear ();

		for (int i = 0; i < _graphic_raycasters.Length; i++)
		{
			if (_graphic_raycasters [i].gameObject != null && _graphic_raycasters [i].gameObject.GetComponent<Canvas> () != null)
				_graphic_raycasters [i].gameObject.GetComponent<Canvas> ().worldCamera = event_camera;
			else
				continue;

			_graphic_raycasters [i].Raycast (pointerData, graphicRaycastResultsGaze);
			if (graphicRaycastResultsGaze.Count == 0)
				continue;

			for (int g = 0; g < graphicRaycastResultsGaze.Count; g++)
				graphicRaycastObjectsGaze.Add (graphicRaycastResultsGaze [g].gameObject);

			_firstResult = FindFirstRaycast (graphicRaycastResultsGaze);
			graphicRaycastResultsGaze.Clear ();

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
						pointerData.pointerCurrentRaycast.worldPosition.z -
						_firstResult.module.eventCamera.transform.position.z);


				bool _changeCurrentRaycast = false;
				// Raycast to nearest (z-axis) target.
				if (pointerData.pointerCurrentRaycast.gameObject == null)
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
						(pointerData.pointerCurrentRaycast.gameObject == null ?
							"null" :
							pointerData.pointerCurrentRaycast.gameObject.name)
						+ ", origin position: " + pointerData.pointerCurrentRaycast.worldPosition
						+ ", origin distance: " + origin_dist
						+ ", origin sorting order: " + pointerData.pointerCurrentRaycast.sortingOrder);
					*/
					if (origin_dist > new_dist)
					{
						DEBUG ("GraphicRaycast() "
						+ pointerData.pointerCurrentRaycast.gameObject.name
						+ ", position: " + pointerData.pointerCurrentRaycast.worldPosition
						+ ", distance: " + origin_dist
						+ " is farer than "
						+ _firstResult.gameObject.name
						+ ", position: " + _firstResult.worldPosition
						+ ", new distance: " + new_dist);

						_changeCurrentRaycast = true;
					} else if (origin_dist == new_dist)
					{
						int _so_origin = pointerData.pointerCurrentRaycast.sortingOrder;
						int _so_result = _firstResult.sortingOrder;

						if (_so_origin < _so_result)
						{
							DEBUG ("GraphicRaycast() "
							+ pointerData.pointerCurrentRaycast.gameObject.name
							+ " sorting order: " + _so_origin + " is smaller than "
							+ _firstResult.gameObject.name
							+ " sorting order: " + _so_result);

							_changeCurrentRaycast = true;
						}
					}
				}

				if (_changeCurrentRaycast)
				{
					pointerData.pointerCurrentRaycast = _firstResult;
					pointerData.position = _firstResult.screenPosition;
				}
			}
		}
	}
	#endregion

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

		WaveVR_RaycastResultProvider.Instance.SetRaycastResult (
			WaveVR_Controller.EDeviceType.Head,
			curGazeObject,
			intersectionPosition);

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
		btnPressDown = false;
		btnPressed = false;
		btnPressUp = false;

		for (int d = 0; d < this.ButtonControlDevices.Count; d++)
		{
			for (int k = 0; k < this.ButtonControlKeys.Count; k++)
			{
				btnPressDown |= WaveVR_Controller.Input (this.ButtonControlDevices [d]).GetPressDown (this.ButtonControlKeys [k]);
				btnPressed |= WaveVR_Controller.Input (this.ButtonControlDevices [d]).GetPress (this.ButtonControlKeys [k]);
				btnPressUp |= WaveVR_Controller.Input (this.ButtonControlDevices [d]).GetPressUp (this.ButtonControlKeys [k]);
			}
		}
	}

	private void UpdateProgressText()
	{
		if (!this.ProgressRate)
		{
			if (this.progressText != null)
				this.progressText.text = "";
			return;
		}

		if (this.progressText == null)
		{
			Text[] _texts = this.Head.transform.GetComponentsInChildren<Text> ();
			for (int t = 0; t < _texts.Length; t++)
			{
				if (_texts [t].gameObject.name.Equals ("ProgressText"))
				{
					DEBUG ("UpdateProgressText() Found ProgressText.");
					this.progressText = _texts [t];
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
		if (!this.ProgressCounter)
		{
			if (this.counterText != null)
				this.counterText.text = "";
			return;
		}

		if (this.counterText == null)
		{
			Text[] _texts = this.Head.transform.GetComponentsInChildren<Text> ();
			for (int t = 0; t < _texts.Length; t++)
			{
				if (_texts [t].gameObject.name.Equals ("CounterText"))
				{
					DEBUG ("UpdateCounterText() Found CounterText.");
					this.counterText = _texts [t];
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

	private Vector3 ringPos = Vector3.zero;
	private void OnTriggeGaze()
	{
		UpdateReticle(preGazeObject, pointerData);
		// The gameobject to which raycast positions
		var curGazeObject = pointerData.pointerCurrentRaycast.gameObject;
		bool isInteractive = pointerData.pointerPress != null || ExecuteEvents.GetEventHandler<IPointerClickHandler>(curGazeObject) != null;

		bool sendEvent = false;
		this.HmdEnterPressDown = WaveVR_Controller.Input (WaveVR_Controller.EDeviceType.Head).GetPressDown (WVR_InputId.WVR_InputId_Alias1_Enter);
		if (this.HmdEnterPressDown)
			sendEvent = true;

		EnterExitGraphicObject ();
		EnterExitPhysicsObject ();

		if (preGazeObject != curGazeObject)
		{
			DEBUG ("preGazeObject: "
				+ (preGazeObject != null ? preGazeObject.name : "null")
				+ ", curGazeObject: "
				+ (curGazeObject != null ? curGazeObject.name : "null"));
			if (curGazeObject != null)
				gazeTime = this.currUnscaledTime;
		}
		else
		{
			if (curGazeObject != null)
			{
				if (useWaveVRReticle && gazePointer != null)
					gazePointer.triggerProgressBar (true);

				if (this.TimerControl)
				{
					if (this.currUnscaledTime - gazeTime > TimeToGaze)
					{
						sendEvent = true;
						gazeTime = this.currUnscaledTime;
					}
					float rate = ((this.currUnscaledTime - gazeTime) / TimeToGaze) * 100;
					if (useWaveVRReticle && gazePointer != null)
						gazePointer.setProgressBarTime (rate);
					else
					{
						if (ringMesh != null)
						{
							ringMesh.RingPercent = isInteractive ? (int)rate : 0;
						}
					}
				}

				if (this.ButtonControl)
				{
					if (!this.TimerControl)
					{
						if (useWaveVRReticle && gazePointer != null)
							gazePointer.triggerProgressBar (false);
						else
						{
							if (ringMesh != null)
								ringMesh.RingPercent = 0;
						}
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
				if (useWaveVRReticle && gazePointer != null)
					gazePointer.triggerProgressBar (false);
				else
				{
					if (ringMesh != null)
						ringMesh.RingPercent = 0;
				}
			}
		}

		// Standalone Input Module information
		pointerData.delta = Vector2.zero;
		pointerData.dragging = false;

		DeselectIfSelectionChanged (curGazeObject, pointerData);

		if (sendEvent)
		{
			DEBUG ("OnTriggeGaze() selected " + curGazeObject.name);
			if (InputEvent == EGazeInputEvent.PointerClick)
			{
				ExecuteEvents.ExecuteHierarchy (curGazeObject, pointerData, ExecuteEvents.pointerClickHandler);
				pointerData.clickTime = this.currUnscaledTime;
			} else if (InputEvent == EGazeInputEvent.PointerDown)
			{
				// like "mouse" action, press->release soon, do NOT keep the pointerPressRaycast cause do NOT need to controll "down" object while not gazing.
				pointerData.pressPosition = pointerData.position;
				pointerData.pointerPressRaycast = pointerData.pointerCurrentRaycast;

				var _pointerDownGO = ExecuteEvents.ExecuteHierarchy (curGazeObject, pointerData, ExecuteEvents.pointerDownHandler);
				ExecuteEvents.ExecuteHierarchy (_pointerDownGO, pointerData, ExecuteEvents.pointerUpHandler);
			} else if (InputEvent == EGazeInputEvent.PointerSubmit)
			{
				ExecuteEvents.ExecuteHierarchy (curGazeObject, pointerData, ExecuteEvents.submitHandler);
			}
		}
	}

	private GameObject preGazeObject = null;
	private void GazeControl()
	{
		bool _connD = WaveVR_Controller.Input (WaveVR_Controller.EDeviceType.Dominant).connected;
		bool _connN = WaveVR_Controller.Input (WaveVR_Controller.EDeviceType.NonDominant).connected;

		this.TimerControl = this.timerControlDefault;
		if (WaveVR_ButtonList.Instance == null || !this.ButtonControl)
		{
			// Set timer gaze if no button support.
			this.TimerControl = true;
		} else
		{
			if (!_connD && !_connN)
			{
				// Set timer gaze if no controller connected and HMD enter is unavailable.
				if (!WaveVR_ButtonList.Instance.IsButtonAvailable (WaveVR_Controller.EDeviceType.Head, WVR_InputId.WVR_InputId_Alias1_Enter))
					this.TimerControl = true;
			}
		}

		preGazeObject = GetCurrentGameObject (pointerData);
		CastToCenterOfScreen ();

		this.currUnscaledTime = Time.unscaledTime;
		OnTriggeGaze();

		UpdateProgressText ();
		UpdateCounterText ();
	}

	#region PointerInputModule overrides. 
	private bool mEnabled = false;
	private bool mEnableGaze = false;
	private RingMeshDrawer ringMesh = null;
	protected override void OnEnable()
	{
		if (!mEnabled)
		{
			base.OnEnable ();

			mEnableGaze = true;

			if (this.Head == null)
			{
				if (WaveVR_InputModuleManager.Instance != null)
				{
					// Reticle Pointer & Ring are put under InputModuleManager
					Head = WaveVR_InputModuleManager.Instance.gameObject;
				} else
				{
					Head = WaveVR_Render.Instance.gameObject;
				}
			}

			if (gazePointer == null)
			{
				// Set gazePointer only when null, or it will got null when WaveVR_Reticle gameObject is SetActive(false).
				if (Head != null)
					gazePointer = Head.GetComponentInChildren<WaveVR_Reticle> ();
			}

			if (gazePointer != null)
			{
				DEBUG ("OnEnable() Head: " + Head.name + ", enable pointer, percent and counter canvas.");
				percentCanvas = gazePointer.transform.Find ("PercentCanvas").gameObject;
				counterCanvas = gazePointer.transform.Find ("CounterCanvas").gameObject;
			}

			this.timerControlDefault = this.TimerControl;

			if (ringMesh == null)
			{
				if (this.Head != null)
				{
					ringMesh = this.Head.GetComponentInChildren<RingMeshDrawer> ();
					DEBUG ("OnEnable() found ringMesh " + (ringMesh != null ? ringMesh.gameObject.name : "null"));
				}
			}

			mEnabled = true;
		}
	}

	protected override void OnDisable()
	{
		if (mEnabled)
		{
			DEBUG ("OnDisable()");
			base.OnDisable ();

			mEnableGaze = false;
			ActivateGazePointerCanvas (false);

			ActivateMeshDrawer (false);
			ringMesh = null;

			ExitAllObjects ();

			mEnabled = false;
		}
	}

	private bool focusCapturedBySystem = false;
	public override void Process()
	{
		useWaveVRReticle = this.UseWaveVRReticle;

		if (this.Head == null || !WaveVR.Instance.Initialized)
		{
			if (Log.gpl.Print)
				DEBUG ("Process() Ooooooooooooops! Check the Head settings or WaveVR initialization!!");
			return;
		}

		if (focusCapturedBySystem != WaveVR.Instance.FocusCapturedBySystem)
		{
			focusCapturedBySystem = WaveVR.Instance.FocusCapturedBySystem;
			// Do not gaze if focus is cpatured by system.
			if (focusCapturedBySystem)
			{
				DEBUG ("Process() focus is captured by system, exit all objects.");
				ExitAllObjects ();
			} else
			{
				DEBUG ("Process() get focus, reset the timer.");
				gazeTime = Time.unscaledTime;
			}
		}

		if (focusCapturedBySystem)
			return;

		if (mEnableGaze)
		{
			ActivatePointerAndRing (true);
			GazeControl ();
		} else
		{
			ActivatePointerAndRing (false);
			ActivateMeshDrawer (false);
		}
	}
	#endregion

	public void ActivatePointerAndRing(bool active)
	{
		if (active)
		{
			ActivateGazePointerCanvas (useWaveVRReticle);
			ActivateMeshDrawer (!useWaveVRReticle);
		} else
		{
			ActivateGazePointerCanvas (false);
			ActivateMeshDrawer (false);
		}
	}

	private void ActivateGazePointerCanvas(bool active)
	{
		if (gazePointer != null)
		{
			MeshRenderer _mr = gazePointer.gameObject.GetComponentInChildren<MeshRenderer>();
			if (_mr != null)
			{
				if (_mr.enabled != active)
				{
					DEBUG (active ? "ActivateGazePointerCanvas() enable pointer." : "ActivateGazePointerCanvas() disable pointer.");
					_mr.enabled = active;
				}
			} else
			{
				if (Log.gpl.Print)
					Log.e (LOG_TAG, "ActivateGazePointerCanvas() Oooooooooooops! No MeshRenderer of " + gazePointer.gameObject.name);
			}
		}
		if (percentCanvas != null && percentCanvas.activeSelf != active)
		{
			DEBUG (active ? "ActivateGazePointerCanvas() enable percentCanvas." : "ActivateGazePointerCanvas() disable percentCanvas.");
			percentCanvas.SetActive (active);
		}
		if (counterCanvas != null && counterCanvas.activeSelf != active)
		{
			DEBUG (active ? "ActivateGazePointerCanvas() enable counterCanvas." : "ActivateGazePointerCanvas() disable counterCanvas.");
			counterCanvas.SetActive (active);
		}
	}

	private void ActivateMeshDrawer(bool active)
	{
		if (ringMesh != null)
		{
			MeshRenderer _mr = ringMesh.gameObject.GetComponentInChildren<MeshRenderer> ();
			if (_mr != null)
			{
				if (_mr.enabled != active)
				{
					DEBUG (active ? "ActivateMeshDrawer() enable ring mesh." : "ActivateMeshDrawer() disable ring mesh.");
					_mr.enabled = active;
				}
			} else
			{
				if (Log.gpl.Print)
					Log.e (LOG_TAG, "ActivateMeshDrawer() Oooooooooooops! No MeshRenderer of " + ringMesh.gameObject.name);
			}
		}
	}
}
