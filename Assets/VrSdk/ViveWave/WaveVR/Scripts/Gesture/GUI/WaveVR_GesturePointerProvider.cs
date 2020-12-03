using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WVR_Log;

public class WaveVR_GesturePointerProvider {
	private const string LOG_TAG = "WaveVR_GesturePointerProvider";
	private void DEBUG(string msg)
	{
		if (Log.EnableDebugLog)
			Log.d (LOG_TAG, msg, true);
	}

	private class GesturePointer
	{
		public WaveVR_GestureManager.EGestureHand Hand { get; set; }
		public GameObject Pointer { get; set; }

		public GesturePointer(WaveVR_GestureManager.EGestureHand type, GameObject pointer)
		{
			Hand = type;
			Pointer = pointer;
		}
	}
	private List<GesturePointer> gesturePointers = new List<GesturePointer>();
	private WaveVR_GestureManager.EGestureHand[] gestureHandList = new WaveVR_GestureManager.EGestureHand[] {
		WaveVR_GestureManager.EGestureHand.RIGHT,
		WaveVR_GestureManager.EGestureHand.LEFT
	};

	private static  WaveVR_GesturePointerProvider instance = null;
	public static WaveVR_GesturePointerProvider Instance
	{
		get {
			if (instance == null)
				instance = new WaveVR_GesturePointerProvider ();
			return instance;
		}
	}

	private WaveVR_GesturePointerProvider(){
		for (int i = 0; i < gestureHandList.Length; i++)
			gesturePointers.Add (new GesturePointer (gestureHandList [i], null));
	}

	public void SetGesturePointer(WaveVR_GestureManager.EGestureHand hand, GameObject pointer)
	{
		DEBUG ("SetGesturePointer() " + hand + ", pointer: " + (pointer != null ? pointer.name : "null"));

		for (int i = 0; i < gestureHandList.Length; i++)
		{
			if (gestureHandList [i] == hand)
			{
				// Deactivate original pointer.
				if (gesturePointers [i].Pointer != null)
					gesturePointers [i].Pointer.GetComponent<WaveVR_GesturePointer> ().ShowPointer = false;

				// Activate new pointer.
				gesturePointers [i].Pointer = pointer;
				gesturePointers [i].Pointer.GetComponent<WaveVR_GesturePointer> ().ShowPointer = true;
			}
		}
	}

	public GameObject GetGesturePointer(WaveVR_GestureManager.EGestureHand hand)
	{
		int index = 0;
		for (int i = 0; i < gestureHandList.Length; i++)
		{
			if (gestureHandList [i] == hand)
			{
				index = i;
				if (gesturePointers [i].Pointer != null)
					gesturePointers [i].Pointer.GetComponent<WaveVR_GesturePointer> ().ShowPointer = true;
			}
			else
			{
				// Deactivate the pointers not needed.
				if (gesturePointers [i].Pointer != null)
					gesturePointers [i].Pointer.GetComponent<WaveVR_GesturePointer> ().ShowPointer = false;
			}
		}

		return gesturePointers [index].Pointer;
	}
}
