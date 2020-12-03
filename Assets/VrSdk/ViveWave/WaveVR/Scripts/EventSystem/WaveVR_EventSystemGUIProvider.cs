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

static class WaveVR_EventSystemGUIProvider
{
	static List<GameObject> EventGUIs = new List<GameObject> ();

	public static void AddEventGUI(GameObject go)
	{
		EventGUIs.Add (go);
	}

	public static void RemoveEventGUI(GameObject go)
	{
		EventGUIs.Remove (go);
	}

	public static GameObject[] GetEventGUIs()
	{
		if (EventGUIs.Count == 0)
			return null;

		return EventGUIs.ToArray ();
	}
}
