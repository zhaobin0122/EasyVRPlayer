// "WaveVR SDK 
// © 2017 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the WaveVR SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

#if UNITY_EDITOR
using UnityEngine;
using System.IO;
using System;
using UnityEditor.Build;
using UnityEditor;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
#if UNITY_2017_2_OR_NEWER
using UnityEngine.XR;
#else
using UnityEngine.VR;
#endif
#if UNITY_2018_1_OR_NEWER
using UnityEditor.Build.Reporting;
#endif

static class CustomBuildProcessor
{
	private static bool buildAndroidVive = false;
	private static bool buildAndroidOculus = false;
	private static bool buildAndroidDaydream = false;

	private static string minWaveSDKVersion = "";
	private static string numDoFHmd = "";
	private static string numDoFController = "";
	private static string numController = "";
	private static bool initWaveVRAttributes = false;
	private static bool forceUpdateWaveVRAttributes = false;

	private static string defaultWVRAndroidManifestFile = "Assets/WaveVR/Platform/Android/AndroidManifest.xml";
	private static string customizedWVRAndroidManifestFile = "Assets/WaveVR/Platform/Android/Customize/AndroidManifest.xml";
	private static string wvrAarFolder = "Assets/WaveVR/Platform/Android";

#if !UNITY_2018_1_OR_NEWER
	private static string defaultGradleTemplateFile = "Assets/WaveVR/Platform/Android/mainTemplate.gradle";
#endif

	private const string WVRSinglePassDeviceName =
#if UNITY_2018_2_OR_NEWER
			"mockhmd";  // cast to lower
#else
			"split";
#endif

	public class WaveVRAttributesWindow : EditorWindow
	{
		[MenuItem("WaveVR/Preference/WaveVR Attributes")]
		static void Init()
		{
			EditorWindow tmp = EditorWindow.focusedWindow;
			// Get existing open window or if none, make a new one:
			WaveVRAttributesWindow window = (WaveVRAttributesWindow)EditorWindow.GetWindow(typeof(WaveVRAttributesWindow), true, "WaveVR Attributes", false);
			window.position = new Rect(Screen.width / 10, Screen.height / 10, 320, 240);
			forceUpdateWaveVRAttributes = true;
			window.Show();
			if (tmp != null)
				tmp.Focus();
		}

		public static void ShowWindow()
		{
			GetWindow<WaveVRAttributesWindow>("WaveVR Attributes");
		}

		void OnGUI()
		{
			if (!initWaveVRAttributes || forceUpdateWaveVRAttributes)
				updateMetadata();
			{
#if UNITY_5_6_OR_NEWER
				var packagename = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);
#else
				var packagename = PlayerSettings.bundleIdentifier;
#endif
				GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
				style.wordWrap = true;
				{
					var origin = GUI.color;
					GUI.color = Color.white;
					GUILayout.Label ("Extracting from " + packagename + " manifest:", style);
					GUI.color = origin;
				}

				if (minWaveSDKVersion.Equals(""))
				{
					var origin = GUI.color;
					GUI.color = Color.red;
					GUILayout.Label ("minWaveSDKVersion is NULL!", EditorStyles.boldLabel);
					GUI.color = origin;
				}
				else
					GUILayout.Label ("minWaveSDKVersion is " + minWaveSDKVersion);
				if (numDoFHmd.Equals(""))
				{
					var origin = GUI.color;
					GUI.color = Color.red;
					GUILayout.Label ("NumDoFHmd is NULL!", EditorStyles.boldLabel);
					GUI.color = origin;
				}
				else
					GUILayout.Label ("NumDoFHmd is " + numDoFHmd);
				if (numDoFController.Equals(""))
				{
					var origin = GUI.color;
					GUI.color = Color.red;
					GUILayout.Label ("NumDoFController is NULL!", EditorStyles.boldLabel);
					GUI.color = origin;
				}
				else
					GUILayout.Label ("NumDoFController is " + numDoFController);
				if (numController.Equals(""))
				{
					var origin = GUI.color;
					GUI.color = Color.red;
					GUILayout.Label ("NumController is NULL!", EditorStyles.boldLabel);
					GUI.color = origin;
				}
				else
					GUILayout.Label ("NumController is " + numController);
				{
					var origin = GUI.color;
					GUI.color = Color.red;
					GUILayout.Label ("Please ensure that these metadata in your manifest matches the capabilities of your title. This metadata will affect how VIVEPORT store distributes and displays your title.", style);
					GUI.color = origin;
				}

				if (GUILayout.Button("Guide Document for More Details"))
				{
					Application.OpenURL("https://hub.vive.com/storage/app/doc/en-us/ConfigureAppCapabilities.html");
				}
				if (GUILayout.Button("Close"))
				{
					this.Close();
				}
			}
		}
	}

	static void enableGoogleVRAars(bool enable)
	{
		var info = new DirectoryInfo("Assets/GoogleVR/Plugins/Android");
		if (info != null && info.Exists)
		{
			var fileInfo = info.GetFiles("*.aar");
			foreach (var file in fileInfo)
			{
				PluginImporter googlevrPlugin = AssetImporter.GetAtPath("Assets/GoogleVR/Plugins/Android/" + file.Name) as PluginImporter;
				if (googlevrPlugin != null)
					googlevrPlugin.SetCompatibleWithPlatform(BuildTarget.Android, enable);
			}
		}
	}

	static void enableWaveVRAars(bool enable)
	{
		var info = new DirectoryInfo(wvrAarFolder);
		if (info != null && info.Exists)
		{
			var fileInfo = info.GetFiles("*.aar");
			foreach (var file in fileInfo)
			{
				PluginImporter wavevrPlugin = AssetImporter.GetAtPath(wvrAarFolder + "/" + file.Name) as PluginImporter;
				if (wavevrPlugin != null)
					wavevrPlugin.SetCompatibleWithPlatform(BuildTarget.Android, enable);
			}
		}
	}

	static void enableOculusVRAars(bool enable)
	{
		var info = new DirectoryInfo("Assets/Oculus/VR/Plugins");
		if (info != null && info.Exists)
		{
			var dirInfo = info.GetDirectories();
			foreach (var dir in dirInfo)
			{
				var dirinfo2 = dir.GetDirectories("Android");
				foreach (var dir2 in dirinfo2) {
					var fileInfo = info.GetFiles("*.aar");
					foreach (var file in fileInfo)
					{
						PluginImporter oculusPlugin = AssetImporter.GetAtPath("Assets/Oculus/VR/Plugins/" + dir + "/Android/" + file.Name) as PluginImporter;
						if (oculusPlugin != null)
							oculusPlugin.SetCompatibleWithPlatform(BuildTarget.Android, enable);
					}
				}
			}
		}
	}

	static void findAndroidManifestFiles()
	{
		var monoScripts = MonoImporter.GetAllRuntimeMonoScripts();
		var monoScript = monoScripts.FirstOrDefault(script => script.GetClass() == typeof(WaveVR));
		var path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(monoScript));
		var fullPath = Path.GetFullPath((path.Substring(0, path.Length - "Scripts".Length) + "Platform/Android").Replace("\\", "/"));

		wvrAarFolder = fullPath.Substring(fullPath.IndexOf("Assets"), fullPath.Length - fullPath.IndexOf("Assets")).Replace("\\", "/");
		defaultWVRAndroidManifestFile = (wvrAarFolder + "/AndroidManifest.xml").Replace("\\", "/");
		customizedWVRAndroidManifestFile = (wvrAarFolder + "/Customize/AndroidManifest.xml").Replace("\\", "/");

#if !UNITY_2018_1_OR_NEWER
		defaultGradleTemplateFile = (wvrAarFolder + "/mainTemplate.gradle").Replace("\\", "/");
#endif

		Debug.Log("defaultWVRAndroidManifestFile = " + defaultWVRAndroidManifestFile);
		Debug.Log("customizedWVRAndroidManifestFile = " + customizedWVRAndroidManifestFile);
		Debug.Log("wvrAarFolder = " + wvrAarFolder);
	}

	static void copyAndroidManifest()
	{
		if (!Directory.Exists("Assets/Plugins/Android"))
			Directory.CreateDirectory("Assets/Plugins/Android");
		if (File.Exists(customizedWVRAndroidManifestFile))
			File.Copy(customizedWVRAndroidManifestFile, "Assets/Plugins/Android/AndroidManifest.xml", true);
		else if (File.Exists(defaultWVRAndroidManifestFile))
			File.Copy(defaultWVRAndroidManifestFile, "Assets/Plugins/Android/AndroidManifest.xml", true);

#if !UNITY_2018_1_OR_NEWER
		if (File.Exists(defaultGradleTemplateFile))
			File.Copy(defaultGradleTemplateFile, "Assets/Plugins/Android/mainTemplate.gradle");
#endif
	}

	static void delAndroidManifest()
	{
		if (File.Exists("Assets/Plugins/Android/AndroidManifest.xml"))
			File.Delete("Assets/Plugins/Android/AndroidManifest.xml");
#if !UNITY_2018_1_OR_NEWER
		if (File.Exists("Assets/Plugins/Android/mainTemplate.gradle"))
			File.Delete("Assets/Plugins/Android/mainTemplate.gradle");
#endif
	}

	static void updateMetadata() {
		XmlDocument doc = new XmlDocument();
		XmlNodeList metadataNodeList = null;
		if (File.Exists(customizedWVRAndroidManifestFile))
		{
			doc.Load(customizedWVRAndroidManifestFile);
			metadataNodeList = doc.SelectNodes("/manifest/application/meta-data");
		}
		else if (File.Exists(defaultWVRAndroidManifestFile))
		{
			doc.Load(defaultWVRAndroidManifestFile);
			metadataNodeList = doc.SelectNodes("/manifest/application/meta-data");
		}
		minWaveSDKVersion = "";
		numDoFHmd = "";
		numDoFController = "";
		numController = "";
		if (metadataNodeList != null)
		{
			foreach (XmlNode metadataNode in metadataNodeList)
			{
				string name = metadataNode.Attributes["android:name"].Value;
				string value = metadataNode.Attributes["android:value"].Value;
				if (name.Equals("minWaveSDKVersion"))
				{
					minWaveSDKVersion = string.Copy(value);
				}
				if (name.Equals("com.htc.vr.content.NumDoFHmd"))
				{
					numDoFHmd = string.Copy(value);
				}
				if (name.Equals("com.htc.vr.content.NumDoFController"))
				{
					numDoFController = string.Copy(value);
				}
				if (name.Equals("com.htc.vr.content.NumController"))
				{
					numController = string.Copy(value);
				}
			}
		}
		initWaveVRAttributes = true;
		forceUpdateWaveVRAttributes = false;
	}

	private class CustomPreprocessor :
#if UNITY_2018_1_OR_NEWER
		IPreprocessBuildWithReport
#else
		IPreprocessBuild
#endif
	{
		public int callbackOrder { get { return 0; } }

		public static bool GetVirtualRealitySupported(BuildTargetGroup group)
		{
#if UNITY_2017_2_OR_NEWER
			return PlayerSettings.GetVirtualRealitySupported(group);
#else
			return UnityEditorInternal.VR.VREditor.GetVREnabledOnTargetGroup(group);
#endif
		}

		public static string[] GetVirtualRealitySDKs(BuildTargetGroup group)
		{
#if UNITY_2017_2_OR_NEWER
			return PlayerSettings.GetVirtualRealitySDKs(group);
#else
			return UnityEditorInternal.VR.VREditor.GetVREnabledDevicesOnTargetGroup(group);
#endif
		}
		
		void AssetManagment(BuildTarget target, string path)
		{
			if (target == BuildTarget.Android)
			{
				findAndroidManifestFiles();
				buildAndroidVive = false;
				buildAndroidOculus = false;
				buildAndroidDaydream = false;
#if UNITY_2017_2_OR_NEWER
		var devices = XRSettings.supportedDevices;
#else
		var devices = VRSettings.supportedDevices;
#endif
				foreach (var dev in devices)
				{
					var lower = dev.ToLower();
					if (lower.Equals("daydream"))
					{
						buildAndroidDaydream = true;
					}
					if (lower.Equals(WVRSinglePassDeviceName))
					{
						buildAndroidVive = true;
					}
					if (lower.Equals("oculus"))
					{
						buildAndroidOculus = true;
					}
				}

				if (buildAndroidVive || (!buildAndroidOculus && !buildAndroidDaydream))
				{
					copyAndroidManifest();
					enableGoogleVRAars(false);
					enableWaveVRAars(true);
					enableOculusVRAars(false);

					EditorWindow tmp = EditorWindow.focusedWindow;
					WaveVRAttributesWindow window = (WaveVRAttributesWindow)EditorWindow.GetWindow(typeof(WaveVRAttributesWindow), true, "WaveVR Attributes", false);
					forceUpdateWaveVRAttributes = true;
					window.Show();
					if (tmp != null)
						tmp.Focus();
				}
			}
		}

		public void OnPreprocessBuild(BuildTarget target, string path)
		{
			AssetManagment(target, path);
		}

#if UNITY_2018_1_OR_NEWER
		public void OnPreprocessBuild(BuildReport report)
		{
			OnPreprocessBuild(report.summary.platform, report.summary.outputPath);
		}
#endif
	}

	private class CustomPostprocessor :
#if UNITY_2018_1_OR_NEWER
		IPostprocessBuildWithReport
#else
		IPostprocessBuild
#endif
	{
		public int callbackOrder { get { return 0; } }
		public void OnPostprocessBuild(BuildTarget target, string path)
		{
			if (target == BuildTarget.Android)
			{
				if (buildAndroidVive || (!buildAndroidOculus && !buildAndroidDaydream))
				{
					delAndroidManifest();
					enableGoogleVRAars(true);
					enableWaveVRAars(false);
					enableOculusVRAars(true);
				}
			}
		}

#if UNITY_2018_1_OR_NEWER
		public void OnPostprocessBuild(BuildReport report)
		{
			OnPostprocessBuild(report.summary.platform, report.summary.outputPath);
		}
#endif
	}
}
#endif
