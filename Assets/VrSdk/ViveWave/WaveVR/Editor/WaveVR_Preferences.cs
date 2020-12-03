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
using UnityEditor;

public class WaveVR_Preferences
{
	/// <summary>
	/// Should WaveVR automatically enable VR when opening Unity or pressing play.
	/// </summary>
	public static bool AutoEnableVR
	{
		get
		{
			return EditorPrefs.GetBool("WaveVR_AutoEnableVR", true);
		}
		set
		{
			EditorPrefs.SetBool("WaveVR_AutoEnableVR", value);
		}
	}

	[PreferenceItem("WaveVR")]
	static void PreferencesGUI()
	{
		EditorGUILayout.BeginVertical();
		EditorGUILayout.Space();

		// Automatically Enable VR
		{
			string title = "Automatically Enable VR";
			string tooltip = "Should WaveVR automatically enable VR on launch and play?";
			AutoEnableVR = EditorGUILayout.Toggle(new GUIContent(title, tooltip), AutoEnableVR);
			string helpMessage = "To enable VR manually:\n";
			helpMessage += "- go to Edit -> Project Settings -> Player,\n";
			helpMessage += "- tick 'Virtual Reality Supported',\n";
			helpMessage += "- make sure 'MockVive' or 'SplitScreen' is in the 'Virtual Reality SDKs' list.";
			EditorGUILayout.HelpBox(helpMessage, MessageType.Info);
		}

		EditorGUILayout.EndVertical();
	}
}

