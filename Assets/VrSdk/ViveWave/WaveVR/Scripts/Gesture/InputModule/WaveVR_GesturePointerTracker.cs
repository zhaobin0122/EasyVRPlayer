using UnityEngine;
using WVR_Log;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Camera), typeof(PhysicsRaycaster))]
public class WaveVR_GesturePointerTracker : MonoBehaviour {
	private const string LOG_TAG = "WaveVR_GesturePointerTracker";
	private void DEBUG(string msg)
	{
		if (Log.EnableDebugLog)
			Log.d (LOG_TAG, msg, true);
	}

	private static WaveVR_GesturePointerTracker instance = null;
	public static WaveVR_GesturePointerTracker Instance {
		get {
			return instance;
		}
	}

	private WaveVR_GestureManager.EGestureHand gestureFocusHand = WaveVR_GestureManager.EGestureHand.RIGHT;
	private GameObject pointerObject = null;
	private WaveVR_GesturePointer gesturePointer = null;
	private bool ValidateParameters()
	{
		gestureFocusHand = WaveVR_GestureManager.GestureFocusHand;
		GameObject new_pointer = WaveVR_GesturePointerProvider.Instance.GetGesturePointer (gestureFocusHand);
		if (new_pointer != null && !GameObject.ReferenceEquals (pointerObject, new_pointer))
		{
			pointerObject = new_pointer;
			gesturePointer = pointerObject.GetComponent<WaveVR_GesturePointer> ();
		}

		if (pointerObject == null || gesturePointer == null)
			return false;

		return true;
	}

	void Awake()
	{
		instance = this;
	}

	void Start () {
		GetComponent<Camera>().enabled = false;
		transform.position = WaveVR_Render.Instance.righteye.transform.position;
		DEBUG ("Start() " + gameObject.name);
	}

	private Vector3 pointerPosition = Vector3.zero;
	private Vector3 lookDirection = Vector3.zero;
	void Update () {
		if (!ValidateParameters())
			return;

		pointerPosition = gesturePointer.GetPointerPosition ();
		lookDirection = pointerPosition - transform.position;
		transform.rotation = Quaternion.LookRotation (lookDirection);
		//Debug.DrawRay (transform.position, lookDirection, Color.red);
	}

	public Camera GetPointerTrackerCamera()
	{
		return GetComponent<Camera> ();
	}

	public PhysicsRaycaster GetPhysicsRaycaster()
	{
		return GetComponent<PhysicsRaycaster> ();
	}
}
