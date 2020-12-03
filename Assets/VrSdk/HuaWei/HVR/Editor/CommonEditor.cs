using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[InitializeOnLoad]
public class CommonEditor {
	static CommonEditor() {
		VersionAdapter ();
	}

	static void VersionAdapter() {
		string version = Application.unityVersion;
		Debug.Log ("UnityVersion : " + version);
		if (version.Contains ("2017") && File.Exists(Application.dataPath + "/HVR/Extra/hvrbridge.jar.2017")) {
			if (EditorUtility.DisplayDialog ("Unity Version", "The current Unity Version is " + version +
				", so you need to replace the Assets/Plugins/Android/hvrbridge.jar with Assets/HVR/Extra/hvrbridge.jar.2017, otherwise run-time crashes may occur. " +
			    "Do you want to replace it automatically?", "Yes", "No")) {
				AssetDatabase.CopyAsset ("Assets/HVR/Extra/hvrbridge.jar.2017", "Assets/Plugins/Android/hvrbridge.jar");
				AssetDatabase.DeleteAsset ("Assets/HVR/Extra/hvrbridge.jar.2017");
				AssetDatabase.Refresh ();
			}
		}else if (version.Contains ("2018") && File.Exists(Application.dataPath + "/HVR/Extra/hvrbridge.jar.2018")) {
			if (EditorUtility.DisplayDialog ("Unity Version", "The current Unity Version is " + version +
				", so you need to replace the Assets/Plugins/Android/hvrbridge.jar with Assets/HVR/Extra/hvrbridge.jar.2018, otherwise run-time crashes may occur. " +
			    "Do you want to replace it automatically?", "Yes", "No")) {
				AssetDatabase.CopyAsset ("Assets/HVR/Extra/hvrbridge.jar.2018", "Assets/Plugins/Android/hvrbridge.jar");
				AssetDatabase.DeleteAsset ("Assets/HVR/Extra/hvrbridge.jar.2018");
				AssetDatabase.Refresh ();
			}
		}
	}
}
