using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using wvr;
using WVR_Log;
using UnityEngine.UI;

public class WaveVR_GestureInputModule : BaseInputModule {
	private const string LOG_TAG = "WaveVR_GestureInputModule";
	private void DEBUG(string msg)
	{
		if (Log.EnableDebugLog)
		{
			Log.d (LOG_TAG, gestureFocusHand + ", " + msg, true);
		}
	}
	private void INFO(string msg)
	{
		Log.i (LOG_TAG, msg, true);
	}


	// ---------------------- Public Variables begins ----------------------
	[Tooltip("If not selected, no events will be sent.")]
	public bool EnableEvent = true;
	[Tooltip("The gesture used to trigger events.")]
	public WaveVR_GestureManager.EStaticGestures SelectGesture = WaveVR_GestureManager.EStaticGestures.FIST;
	// ---------------------- Public Variables ends ----------------------


	#region Basic Declaration
	[SerializeField]
	private bool forceModuleActive = true;

	public bool ForceModuleActive
	{
		get { return forceModuleActive; }
		set { forceModuleActive = value; }
	}

	public override bool IsModuleSupported()
	{
		return forceModuleActive;
	}

	public override bool ShouldActivateModule()
	{
		if (!base.ShouldActivateModule ())
			return false;

		if (forceModuleActive)
			return true;

		return false;
	}

	public override void DeactivateModule() {
		base.DeactivateModule();
	}
	#endregion


	private WVR_HandGestureType currentGestureRight = WVR_HandGestureType.WVR_HandGestureType_Invalid;
	private WVR_HandGestureType previousGestureRight = WVR_HandGestureType.WVR_HandGestureType_Invalid;
	private WVR_HandGestureType currentGestureLeft = WVR_HandGestureType.WVR_HandGestureType_Invalid;
	private WVR_HandGestureType previousGestureLeft = WVR_HandGestureType.WVR_HandGestureType_Invalid;
	private WaveVR_GestureManager.EGestureHand gestureFocusHand = WaveVR_GestureManager.EGestureHand.RIGHT;

	private GameObject pointerObject = null;
	private WaveVR_GesturePointer gesturePointer = null;
	private Camera eventCamera = null;
	private PhysicsRaycaster pointerPhysicsRaycaster = null;
	private bool ValidateParameters()
	{
		gestureFocusHand = WaveVR_GestureManager.GestureFocusHand;
		GameObject new_pointer = WaveVR_GesturePointerProvider.Instance.GetGesturePointer (gestureFocusHand);
		if (new_pointer != null && !GameObject.ReferenceEquals (pointerObject, new_pointer))
		{
			pointerObject = new_pointer;
			gesturePointer = pointerObject.GetComponent<WaveVR_GesturePointer> ();
		}
		if (pointerObject == null)
			gesturePointer = null;

		if (WaveVR_GesturePointerTracker.Instance != null)
		{
			if (eventCamera == null)
				eventCamera = WaveVR_GesturePointerTracker.Instance.GetPointerTrackerCamera ();
			if (pointerPhysicsRaycaster == null)
				pointerPhysicsRaycaster = WaveVR_GesturePointerTracker.Instance.GetPhysicsRaycaster ();
		}

		if (gesturePointer == null || eventCamera == null)
		{
			if (Log.gpl.Print)
			{
				if (gesturePointer == null)
					Log.i (LOG_TAG, "ValidateParameters() No pointer of " + gestureFocusHand, true);
				if (eventCamera == null)
					Log.i (LOG_TAG, "ValidateParameters() Forget to put GesturePointerTracker??");
			}
			return false;
		}

		if (!this.EnableEvent)
			return false;

		return true;
	}

	private PointerEventData mPointerEventData = null;
	private readonly Vector2 centerOfScreen = new Vector2 (0.5f * Screen.width, 0.5f * Screen.height);
	private void ResetPointerEventData()
	{
		if (mPointerEventData == null)
		{
			mPointerEventData = new PointerEventData (eventSystem);
			mPointerEventData.pointerCurrentRaycast = new RaycastResult ();
		}

		mPointerEventData.Reset ();
		mPointerEventData.position = new Vector2 (0.5f * eventCamera.pixelWidth, 0.5f * eventCamera.pixelHeight); // center of screen
		firstRaycastResult.Clear();
		mPointerEventData.pointerCurrentRaycast = firstRaycastResult;
	}

	private GameObject prevRaycastedObject = null;
	private GameObject GetRaycastedObject()
	{
		if (mPointerEventData == null)
			return null;

		return mPointerEventData.pointerCurrentRaycast.gameObject;
	}

	private Vector3 GetIntersectionPosition(RaycastResult raycastResult)
	{
		if (eventCamera == null)
			return Vector3.zero;

		float intersectionDistance = raycastResult.distance + eventCamera.nearClipPlane;
		Vector3 intersectionPosition = eventCamera.transform.forward * intersectionDistance + eventCamera.transform.position;
		return intersectionPosition;
	}


	#region BaseInputModule Overrides
	private bool mInputModuleEnabled = false;
	protected override void OnEnable()
	{
		if (!mInputModuleEnabled)
		{
			base.OnEnable ();
			DEBUG ("OnEnable()");

			// Standalone Input Module
			StandaloneInputModule _sim = gameObject.GetComponent<StandaloneInputModule> ();
			if (_sim != null)
				_sim.enabled = false;

			mInputModuleEnabled = true;
		}
	}

	protected override void OnDisable()
	{
		if (mInputModuleEnabled)
		{
			base.OnDisable ();
			DEBUG ("OnDisable()");

			mInputModuleEnabled = false;
		}
	}

	public override void Process()
	{
		if (!ValidateParameters ())
			return;

		// Save previous raycasted object.
		prevRaycastedObject = GetRaycastedObject ();


		// ------------------- Raycast Actions begins -------------------
		ResetPointerEventData ();
		GraphicRaycast();
		PhysicsRaycast ();
		// ------------------- Raycast Actions ends -------------------


		GameObject curr_raycasted_object = GetRaycastedObject ();


		// ------------------- Check if receiving SelectGesture begins -------------------
		previousGestureRight = currentGestureRight;
		currentGestureRight = WaveVR_GestureManager.Instance.GetCurrentRightHandStaticGesture ();
		previousGestureLeft = currentGestureLeft;
		currentGestureLeft = WaveVR_GestureManager.Instance.GetCurrentLeftHandStaticGesture ();

		bool gesture_received = false;
		bool keep_dragging = false;
		if (gestureFocusHand == WaveVR_GestureManager.EGestureHand.RIGHT)
		{
			// Switch the focus hand to left.
			if ((previousGestureLeft != currentGestureLeft) &&
			    (currentGestureLeft == (WVR_HandGestureType)this.SelectGesture))
			{
				WaveVR_GestureManager.GestureFocusHand = WaveVR_GestureManager.EGestureHand.LEFT;
				return;
			}

			gesture_received =
			(
			    currentGestureRight == (WVR_HandGestureType)this.SelectGesture &&
			    currentGestureRight != WVR_HandGestureType.WVR_HandGestureType_Invalid
			);

			keep_dragging =
				(
					previousGestureRight == (WVR_HandGestureType)this.SelectGesture &&
					currentGestureRight == WVR_HandGestureType.WVR_HandGestureType_Unknown
				);
		}
		if (gestureFocusHand == WaveVR_GestureManager.EGestureHand.LEFT)
		{
			// Switch the focus hand to right.
			if ((previousGestureRight != currentGestureRight) &&
			    (currentGestureRight == (WVR_HandGestureType)this.SelectGesture))
			{
				WaveVR_GestureManager.GestureFocusHand = WaveVR_GestureManager.EGestureHand.RIGHT;
				return;
			}

			gesture_received =
			(
			    currentGestureLeft == (WVR_HandGestureType)this.SelectGesture &&
			    currentGestureLeft != WVR_HandGestureType.WVR_HandGestureType_Invalid
			);

			keep_dragging =
				(
					previousGestureLeft == (WVR_HandGestureType)this.SelectGesture &&
					currentGestureLeft == WVR_HandGestureType.WVR_HandGestureType_Unknown
				);
		}
		// ------------------- Check if receiving SelectGesture ends -------------------


		// ------------------- Event Handling begins -------------------
		OnGraphicPointerEnterExit ();
		OnPhysicsPointerEnterExit ();

		if (curr_raycasted_object != null && curr_raycasted_object == prevRaycastedObject)
		{
			OnPointerHover ();
			gesturePointer.OnHover (true);
		} else
		{
			gesturePointer.OnHover (false);
		}

		if (!mPointerEventData.eligibleForClick)
		{
			if (gesture_received)
				OnPointerDown ();
		} else if (mPointerEventData.eligibleForClick)
		{
			if (gesture_received || keep_dragging)
			{
				// Down before, and receives the selected gesture continuously.
				OnPointerDrag ();

			} else
			{
				// Down before, but not receive the selected gesture.
				OnPointerUp ();
			}
		}
		// ------------------- Event Handling ends -------------------


		Vector3 intersection_position = GetIntersectionPosition (mPointerEventData.pointerCurrentRaycast);
	}
	#endregion

	#region Raycast Actions
	private RaycastResult firstRaycastResult = new RaycastResult ();
	private GraphicRaycaster[] graphic_raycasters;
	private List<RaycastResult> graphicRaycastResults = new List<RaycastResult>();
	private List<GameObject> graphicRaycastObjects = new List<GameObject>(), preGraphicRaycastObjects = new List<GameObject>();

	private void GraphicRaycast()
	{
		if (eventCamera == null)
			return;

		// Find GraphicRaycaster
		graphic_raycasters = GameObject.FindObjectsOfType<GraphicRaycaster> ();

		graphicRaycastObjects.Clear ();

		for (int i = 0; i < graphic_raycasters.Length; i++)
		{
			// Ignore the Blocker of Dropdown.
			if (graphic_raycasters [i].gameObject.name.Equals ("Blocker"))
				continue;

			// Change the Canvas' event camera.
			if (graphic_raycasters [i].gameObject.GetComponent<Canvas> () != null)
				graphic_raycasters [i].gameObject.GetComponent<Canvas> ().worldCamera = eventCamera;
			else
				continue;

			// Raycasting.
			graphic_raycasters [i].Raycast (mPointerEventData, graphicRaycastResults);
			if (graphicRaycastResults.Count == 0)
				continue;

			for (int g = 0; g < graphicRaycastResults.Count; g++)
				graphicRaycastObjects.Add (graphicRaycastResults [g].gameObject);

			firstRaycastResult = FindFirstRaycast (graphicRaycastResults);
			graphicRaycastResults.Clear ();

			// Found graphic raycasted object!
			if (firstRaycastResult.gameObject != null)
			{
				if (firstRaycastResult.worldPosition == Vector3.zero)
					firstRaycastResult.worldPosition = GetIntersectionPosition (firstRaycastResult);

				float new_dist =
					Mathf.Abs (
						firstRaycastResult.worldPosition.z -
						firstRaycastResult.module.eventCamera.transform.position.z);
				float origin_dist =
					Mathf.Abs (
						mPointerEventData.pointerCurrentRaycast.worldPosition.z -
						firstRaycastResult.module.eventCamera.transform.position.z);


				bool change_current_raycast = false;
				// Raycast to nearest (z-axis) target.
				if (mPointerEventData.pointerCurrentRaycast.gameObject == null)
				{
					change_current_raycast = true;
				} else
				{
					/*DEBUG ("GraphicRaycast() "
					+ ", raycasted: " + firstRaycastResult.gameObject.name
					+ ", raycasted position: " + firstRaycastResult.worldPosition
					+ ", distance: " + new_dist
					+ ", sorting order: " + firstRaycastResult.sortingOrder
					+ ", origin target: " +
					(mPointerEventData.pointerCurrentRaycast.gameObject == null ?
							"null" :
							mPointerEventData.pointerCurrentRaycast.gameObject.name)
					+ ", origin position: " + mPointerEventData.pointerCurrentRaycast.worldPosition
					+ ", origin distance: " + origin_dist
					+ ", origin sorting order: " + mPointerEventData.pointerCurrentRaycast.sortingOrder);*/

					if (origin_dist > new_dist)
					{
						DEBUG ("GraphicRaycast() "
						+ mPointerEventData.pointerCurrentRaycast.gameObject.name
						+ ", position: " + mPointerEventData.pointerCurrentRaycast.worldPosition
						+ ", distance: " + origin_dist
						+ " is farer than "
						+ firstRaycastResult.gameObject.name
						+ ", position: " + firstRaycastResult.worldPosition
						+ ", new distance: " + new_dist);

						change_current_raycast = true;
					} else if (origin_dist == new_dist)
					{
						int _so_origin = mPointerEventData.pointerCurrentRaycast.sortingOrder;
						int _so_result = firstRaycastResult.sortingOrder;

						if (_so_origin < _so_result)
						{
							DEBUG ("GraphicRaycast() "
							+ mPointerEventData.pointerCurrentRaycast.gameObject.name
							+ " sorting order: " + _so_origin + " is smaller than "
							+ firstRaycastResult.gameObject.name
							+ " sorting order: " + _so_result);

							change_current_raycast = true;
						}
					}
				}

				if (change_current_raycast)
				{
					mPointerEventData.pointerCurrentRaycast = firstRaycastResult;
					mPointerEventData.position = firstRaycastResult.screenPosition;
				}
			}
		}
	}

	private List<RaycastResult> physicsRaycastResults = new List<RaycastResult>();
	private List<GameObject> physicsRaycastObjects = new List<GameObject> (), prePhysicsRaycastObjects = new List<GameObject>();

	private void PhysicsRaycast()
	{
		if (eventCamera == null || pointerPhysicsRaycaster == null)
			return;

		// Clear cache values.
		physicsRaycastResults.Clear ();
		physicsRaycastObjects.Clear ();

		// Raycasting.
		pointerPhysicsRaycaster.Raycast (mPointerEventData, physicsRaycastResults);
		if (physicsRaycastResults.Count == 0)
			return;

		for (int i = 0; i < physicsRaycastResults.Count; i++)
		{
			// Ignore the GameObject with WaveVR_BonePose component.
			if (physicsRaycastResults [i].gameObject.GetComponent<WaveVR_BonePose> () != null)
				continue;

			physicsRaycastObjects.Add (physicsRaycastResults [i].gameObject);
		}

		firstRaycastResult = FindFirstRaycast (physicsRaycastResults);
		physicsRaycastResults.Clear ();

		if (firstRaycastResult.gameObject != null)
		{
			if (firstRaycastResult.worldPosition == Vector3.zero)
				firstRaycastResult.worldPosition = GetIntersectionPosition (firstRaycastResult);

			float new_dist =
				Mathf.Abs (
					firstRaycastResult.worldPosition.z -
					firstRaycastResult.module.eventCamera.transform.position.z);
			float origin_dist =
				Mathf.Abs (
					mPointerEventData.pointerCurrentRaycast.worldPosition.z -
					firstRaycastResult.module.eventCamera.transform.position.z);

			if (mPointerEventData.pointerCurrentRaycast.gameObject == null || origin_dist > new_dist)
			{
				/*DEBUG ("PhysicsRaycast()" +
				", raycasted: " + firstRaycastResult.gameObject.name +
				", raycasted position: " + firstRaycastResult.worldPosition +
				", new_dist: " + new_dist +
				", origin target: " +
				(mPointerEventData.pointerCurrentRaycast.gameObject == null ?
						"null" :
						mPointerEventData.pointerCurrentRaycast.gameObject.name) +
				", origin position: " + mPointerEventData.pointerCurrentRaycast.worldPosition +
				", origin distance: " + origin_dist);*/

				mPointerEventData.pointerCurrentRaycast = firstRaycastResult;
				mPointerEventData.position = firstRaycastResult.screenPosition;
			}
		}
	}
	#endregion

	#region Event Handling
	private void OnGraphicPointerEnterExit()
	{
		if (graphicRaycastObjects.Count != 0)
		{
			for (int i = 0; i < graphicRaycastObjects.Count; i++)
			{
				if (graphicRaycastObjects [i] != null && !preGraphicRaycastObjects.Contains (graphicRaycastObjects [i]))
				{
					ExecuteEvents.Execute (graphicRaycastObjects [i], mPointerEventData, ExecuteEvents.pointerEnterHandler);
					DEBUG ("OnGraphicPointerEnterExit() enter: " + graphicRaycastObjects [i]);
				}
			}
		}

		if (preGraphicRaycastObjects.Count != 0)
		{
			for (int i = 0; i < preGraphicRaycastObjects.Count; i++)
			{
				if (preGraphicRaycastObjects [i] != null && !graphicRaycastObjects.Contains (preGraphicRaycastObjects [i]))
				{
					ExecuteEvents.Execute (preGraphicRaycastObjects [i], mPointerEventData, ExecuteEvents.pointerExitHandler);
					DEBUG ("OnGraphicPointerEnterExit() exit: " + preGraphicRaycastObjects [i]);
				}
			}
		}

		CopyList (graphicRaycastObjects, preGraphicRaycastObjects);
	}

	private void OnPhysicsPointerEnterExit()
	{
		if (physicsRaycastObjects.Count != 0)
		{
			for (int i = 0; i < physicsRaycastObjects.Count; i++)
			{
				if (physicsRaycastObjects [i] != null && !prePhysicsRaycastObjects.Contains (physicsRaycastObjects [i]))
				{
					ExecuteEvents.Execute (physicsRaycastObjects [i], mPointerEventData, ExecuteEvents.pointerEnterHandler);
					DEBUG ("OnPhysicsPointerEnterExit() enter: " + physicsRaycastObjects [i]);
				}
			}
		}

		if (prePhysicsRaycastObjects.Count != 0)
		{
			for (int i = 0; i < prePhysicsRaycastObjects.Count; i++)
			{
				if (prePhysicsRaycastObjects [i] != null && !physicsRaycastObjects.Contains (prePhysicsRaycastObjects [i]))
				{
					ExecuteEvents.Execute (prePhysicsRaycastObjects [i], mPointerEventData, ExecuteEvents.pointerExitHandler);
					DEBUG ("OnPhysicsPointerEnterExit() exit: " + prePhysicsRaycastObjects [i]);
				}
			}
		}

		CopyList (physicsRaycastObjects, prePhysicsRaycastObjects);
	}

	private void OnPointerHover()
	{
		GameObject go = GetRaycastedObject ();
		ExecuteEvents.ExecuteHierarchy(go, mPointerEventData, WaveVR_ExecuteEvents.pointerHoverHandler);
	}

	private void OnPointerDown()
	{
		GameObject go = GetRaycastedObject ();
		if (go == null) return;

		// Send a Pointer Down event. If not received, get handler of Pointer Click.
		mPointerEventData.pressPosition = mPointerEventData.position;
		mPointerEventData.pointerPressRaycast = mPointerEventData.pointerCurrentRaycast;
		mPointerEventData.pointerPress =
			ExecuteEvents.ExecuteHierarchy(go, mPointerEventData, ExecuteEvents.pointerDownHandler)
			?? ExecuteEvents.GetEventHandler<IPointerClickHandler>(go);

		DEBUG ("OnPointerDown() send Pointer Down to " + mPointerEventData.pointerPress + ", current GameObject is " + go);

		// If Drag Handler exists, send initializePotentialDrag event.
		mPointerEventData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(go);
		if (mPointerEventData.pointerDrag != null)
		{
			DEBUG ("OnPointerDown() send initializePotentialDrag to " + mPointerEventData.pointerDrag + ", current GameObject is " + go);
			ExecuteEvents.Execute(mPointerEventData.pointerDrag, mPointerEventData, ExecuteEvents.initializePotentialDrag);
		}

		// Press happened (even not handled) object.
		mPointerEventData.rawPointerPress = go;
		// Allow to send Pointer Click event
		mPointerEventData.eligibleForClick = true;
		// Reset the screen position of press, can be used to estimate move distance
		mPointerEventData.delta = Vector2.zero;
		// Current Down, reset drag state
		mPointerEventData.dragging = false;
		mPointerEventData.useDragThreshold = true;
		// Record the count of Pointer Click should be processed, clean when Click event is sent.
		mPointerEventData.clickCount = 1;
		// Set clickTime to current time of Pointer Down instead of Pointer Click
		// since Down & Up event should not be sent too closely. (< CLICK_TIME)
		mPointerEventData.clickTime = Time.unscaledTime;
	}

	private const float DRAG_TIME = 0.3f;
	private void OnPointerDrag()
	{
		if (Time.unscaledTime - mPointerEventData.clickTime < DRAG_TIME)
			return;
		if (mPointerEventData.pointerDrag == null)
			return;

		if (!mPointerEventData.dragging)
		{
			DEBUG ("OnPointerDrag() send BeginDrag to " + mPointerEventData.pointerDrag);
			ExecuteEvents.Execute (mPointerEventData.pointerDrag, mPointerEventData, ExecuteEvents.beginDragHandler);
			mPointerEventData.dragging = true;
		} else
		{
			ExecuteEvents.Execute (mPointerEventData.pointerDrag, mPointerEventData, ExecuteEvents.dragHandler);
		}
	}

	private void OnPointerUp()
	{
		GameObject go = GetRaycastedObject ();
		// The "go" may be different with mPointerEventData.pointerDrag so we don't check null.

		if (mPointerEventData.pointerPress != null)
		{
			// In the frame of button is pressed -> unpressed, send Pointer Up
			DEBUG ("OnPointerUp() send Pointer Up to " + mPointerEventData.pointerPress);
			ExecuteEvents.Execute (mPointerEventData.pointerPress, mPointerEventData, ExecuteEvents.pointerUpHandler);
		}

		if (mPointerEventData.eligibleForClick)
		{
			GameObject click_object = ExecuteEvents.GetEventHandler<IPointerClickHandler> (go);
			if (click_object != null)
			{
				if (click_object == mPointerEventData.pointerPress)
				{
					// In the frame of button from being pressed to unpressed, send Pointer Click if Click is pending.
					DEBUG ("OnPointerUp() send Pointer Click to " + mPointerEventData.pointerPress);
					ExecuteEvents.Execute (mPointerEventData.pointerPress, mPointerEventData, ExecuteEvents.pointerClickHandler);
				} else
				{
					DEBUG ("OnTriggerUpMouse() pointer down object " + mPointerEventData.pointerPress + " is different with click object " + click_object);
				}
			}

			if (mPointerEventData.dragging)
			{
				GameObject drop_object = ExecuteEvents.GetEventHandler<IDropHandler> (go);
				if (drop_object == mPointerEventData.pointerDrag)
				{
					// In the frame of button from being pressed to unpressed, send Drop and EndDrag if dragging.
					DEBUG ("OnPointerUp() send Pointer Drop to " + mPointerEventData.pointerDrag);
					ExecuteEvents.Execute (mPointerEventData.pointerDrag, mPointerEventData, ExecuteEvents.dropHandler);
				}

				DEBUG ("OnPointerUp() send Pointer endDrag to " + mPointerEventData.pointerDrag);
				ExecuteEvents.Execute (mPointerEventData.pointerDrag, mPointerEventData, ExecuteEvents.endDragHandler);

				mPointerEventData.pointerDrag = null;
				mPointerEventData.dragging = false;
			}
		}

		// Down object.
		mPointerEventData.pointerPress = null;
		// Press happened (even not handled) object.
		mPointerEventData.rawPointerPress = null;
		// Clear pending state.
		mPointerEventData.eligibleForClick = false;
		// Click event is sent, clear count.
		mPointerEventData.clickCount = 0;
		// Up event is sent, clear the time limitation of Down event.
		mPointerEventData.clickTime = 0;
	}
	#endregion

	private void CopyList(List<GameObject> src, List<GameObject> dst)
	{
		dst.Clear ();
		for (int i = 0; i < src.Count; i++)
			dst.Add (src [i]);
	}
}
