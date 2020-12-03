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
using System.IO;
using System.Collections.Generic;
using System.Linq;

[InitializeOnLoad]
public class WaveVR_Settings : EditorWindow
{
	List<Item> items;

	public class Item
	{
		const string ignore = "ignore.";
		const string useRecommended = "Use recommended ({0})";
		const string currentValue = " (current = {0})";

		public delegate bool DelegateIsReady();
		public delegate string DelegateGetCurrent();
		public delegate void DelegateSet();

		public DelegateIsReady IsReady;
		public DelegateGetCurrent GetCurrent;
		public DelegateSet Set;

		public string title { get; private set; }
		public string recommended { get; private set; }

		public Item(string title, string recommended)
		{
			this.title = title;
			this.recommended = recommended;
		}

		public bool IsIgnored { get { return EditorPrefs.HasKey(ignore + title); } }

		public void Ignore()
		{
			EditorPrefs.SetBool(ignore + title, true);
		}

		public void CleanIgnore()
		{
			EditorPrefs.DeleteKey(ignore + title);
		}

		// Return true when setting is not ready.
		public bool Show()
		{
			bool ignored = IsIgnored;
			GUILayout.Label(title + string.Format(currentValue, GetCurrent()) + (IsIgnored ? " (ignored)" : ""));
			if (ignored || IsReady())
				return false;

			GUILayout.BeginHorizontal();
			if (GUILayout.Button(string.Format(useRecommended, recommended)))
				Set();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Ignore"))
				Ignore();
			GUILayout.EndHorizontal();
			return true;
		}
	}

#region version_compatible

	public static bool GetVirtualRealitySupported(BuildTargetGroup group)
	{
#if UNITY_2017_2_OR_NEWER
		return PlayerSettings.GetVirtualRealitySupported(group);
#else
		return UnityEditorInternal.VR.VREditor.GetVREnabledOnTargetGroup(group);
#endif
	}

	public static void SetVirtualRealitySupported(BuildTargetGroup group, bool set)
	{
#if UNITY_2017_2_OR_NEWER
		PlayerSettings.SetVirtualRealitySupported(group, set);
#else
		UnityEditorInternal.VR.VREditor.SetVREnabledOnTargetGroup(group, set);
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

	public static void SetVirtualRealitySDKs(BuildTargetGroup group, string[] devices)
	{
#if UNITY_2017_2_OR_NEWER
		PlayerSettings.SetVirtualRealitySDKs(group, devices);
#else
		UnityEditorInternal.VR.VREditor.SetVREnabledDevicesOnTargetGroup(group, devices);
#endif
	}

	public static bool GetMobileMTRendering(BuildTargetGroup group)
	{
#if UNITY_2017_2_OR_NEWER
		return PlayerSettings.GetMobileMTRendering(group);
#else
		return PlayerSettings.mobileMTRendering;
#endif
	}

	public static void SetMobileMTRendering(BuildTargetGroup group, bool set)
	{
#if UNITY_2017_2_OR_NEWER
		PlayerSettings.SetMobileMTRendering(group, set);
#else
		PlayerSettings.mobileMTRendering = set;
#endif
	}
	#endregion

	public const string WVRSPDEF = "WAVEVR_SINGLEPASS_ENABLED";
	public const string WVRSinglePassDeviceName =
#if UNITY_2018_2_OR_NEWER
			"MockHMD";
#else
			"split";
#endif

	public const string WVRSinglePassDeviceDescriptionName =
#if UNITY_2018_2_OR_NEWER
			"MockHMD";
#elif UNITY_2018_1
			"MockHMD - Vive";
#else  // UNITY_5_6
			"Split screen";
#endif


	public static List<string> GetDefineSymbols(BuildTargetGroup group)
	{
		//https://github.com/UnityCommunity/UnityLibrary/blob/master/Assets/Scripts/Editor/AddDefineSymbols.cs
		var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
		return symbols.Split(';').ToList();
	}

	public static void SetSinglePassDefine(BuildTargetGroup group, bool set, List<string> allDefines)
	{
		var hasDefine = allDefines.Contains(WVRSPDEF);

		if (set)
		{
			if (hasDefine)
				return;
			allDefines.Add(WVRSPDEF);
			PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", allDefines.ToArray()));
			Debug.Log("Add \"" + WVRSPDEF + "\" to define symbols");
		}
		else
		{
			if (hasDefine)
			{
				allDefines.Remove(WVRSPDEF);
				PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", allDefines.ToArray()));
				Debug.Log("Remove \"" + WVRSPDEF + "\" from define symbols");
			}
		}
	}

	public static Item GetVRItem()
	{
		var message = "Enable VR and single-pass Support (Experimental)\n" +
			"  This WVR feature is under development.  Use it at your own risk.\n" +
			"  If you apply this item, it will help to:\n" +
			"	1.Check VR Support, 2.Set " + WVRSinglePassDeviceDescriptionName + "device, \n" +
			"	3.Set StereoRenderingPath as SinglePass, \n" +
			"	4.Add " + WVRSPDEF + " to define symbols. \n" +
			"  You still need disable Auto Graphic and select GLES3.1 only in Project Settings.\n";

		return new Item(message, false.ToString())
		{
#if UNITY_STANDALONE
			IsReady = () => {
				var vrSupported = GetVirtualRealitySupported(BuildTargetGroup.Standalone);
				var list = GetVirtualRealitySDKs(BuildTargetGroup.Standalone);
				var stereoRenderingPath = PlayerSettings.stereoRenderingPath;

				List<string> allDefines = GetDefineSymbols(BuildTargetGroup.Standalone);
				var symbolDefined = allDefines.Contains(WVRSPDEF);
				var hasVRDevice = ArrayUtility.Contains<string>(list, WVRSinglePassDeviceName);
				return vrSupported && hasVRDevice && stereoRenderingPath == StereoRenderingPath.SinglePass && symbolDefined;
			},
			GetCurrent = () =>
			{
				var vrSupported = GetVirtualRealitySupported(BuildTargetGroup.Standalone);
				var list = GetVirtualRealitySDKs(BuildTargetGroup.Standalone);
				var stereoRenderingPath = PlayerSettings.stereoRenderingPath;
				var hasVRDevice = ArrayUtility.Contains<string>(list, WVRSinglePassDeviceName);

				return (vrSupported && hasVRDevice && stereoRenderingPath == StereoRenderingPath.SinglePass).ToString();
			},
			Set = () => {
				SetVirtualRealitySupported(BuildTargetGroup.Standalone, true);
				var list = GetVirtualRealitySDKs(BuildTargetGroup.Standalone);
				if (!ArrayUtility.Contains<string>(list, WVRSinglePassDeviceName))
				{
					ArrayUtility.Insert<string>(ref list, 0, WVRSinglePassDeviceName);
				}
#if UNITY_2018_2_OR_NEWER
				// Remove old name
				if (ArrayUtility.Contains<string>(list, "split"))
				{
					ArrayUtility.Remove<string>(ref list, "split");
				}
#endif
				SetVirtualRealitySDKs(BuildTargetGroup.Standalone, list);
				PlayerSettings.stereoRenderingPath = StereoRenderingPath.SinglePass;

				List<string> allDefines = GetDefineSymbols(BuildTargetGroup.Standalone);
				WaveVR_Settings.SetSinglePassDefine(BuildTargetGroup.Standalone, true, allDefines);
			}
#else
			IsReady = () => {
				var vrSupported = GetVirtualRealitySupported(BuildTargetGroup.Android);
				var list = GetVirtualRealitySDKs(BuildTargetGroup.Android);
				var stereoRenderingPath = PlayerSettings.stereoRenderingPath;

				List<string> allDefines = GetDefineSymbols(BuildTargetGroup.Android);
				var symbolDefined = allDefines.Contains(WVRSPDEF);
				var hasVRDevice = ArrayUtility.Contains<string>(list, WVRSinglePassDeviceName);
				return vrSupported && hasVRDevice && stereoRenderingPath == StereoRenderingPath.SinglePass && symbolDefined;
			},
			GetCurrent = () =>
			{
				var vrSupported = GetVirtualRealitySupported(BuildTargetGroup.Android);
				var list = GetVirtualRealitySDKs(BuildTargetGroup.Android);
				var stereoRenderingPath = PlayerSettings.stereoRenderingPath;
				var hasVRDevice = ArrayUtility.Contains<string>(list, WVRSinglePassDeviceName);

				return (vrSupported && hasVRDevice && stereoRenderingPath == StereoRenderingPath.SinglePass).ToString();
			},
			Set = () => {
				SetVirtualRealitySupported(BuildTargetGroup.Android, true);
				var list = GetVirtualRealitySDKs(BuildTargetGroup.Android);
				if (!ArrayUtility.Contains<string>(list, WVRSinglePassDeviceName))
				{
					ArrayUtility.Insert<string>(ref list, 0, WVRSinglePassDeviceName);
				}
#if UNITY_2018_2_OR_NEWER
				// Remove old name
				if (ArrayUtility.Contains<string>(list, "split"))
				{
					ArrayUtility.Remove<string>(ref list, "split");
				}
#endif
				SetVirtualRealitySDKs(BuildTargetGroup.Android, list);
				PlayerSettings.stereoRenderingPath = StereoRenderingPath.SinglePass;

				List<string> allDefines = GetDefineSymbols(BuildTargetGroup.Android);
				WaveVR_Settings.SetSinglePassDefine(BuildTargetGroup.Android, true, allDefines);
			}
#endif
		};
	}

	static List<Item> GetItems() {
		var builtTarget = new Item("Build target", BuildTarget.Android.ToString())
		{
			IsReady = () => { return EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android; },
			GetCurrent = () => { return EditorUserBuildSettings.activeBuildTarget.ToString(); },
			Set = () => { EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android); }
		};

		var defaultOrigin = new Item("Default orientation", UIOrientation.LandscapeLeft.ToString())
		{
			IsReady = () => { return PlayerSettings.defaultInterfaceOrientation == UIOrientation.LandscapeLeft; },
			GetCurrent = () => { return PlayerSettings.defaultInterfaceOrientation.ToString(); },
			Set = () => { PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft; }
		};

		var enableMTRendering = new Item("Enable multi-threading rendering", true.ToString())
		{
			IsReady = () => { return GetMobileMTRendering(BuildTargetGroup.Android); },
			GetCurrent = () => { return GetMobileMTRendering(BuildTargetGroup.Android).ToString(); },
			Set = () => { SetMobileMTRendering(BuildTargetGroup.Android, true); }
		};

		var graphicsJobs = new Item("Enable Graphics Jobs", true.ToString())
		{
			IsReady = () => { return PlayerSettings.graphicsJobs; },
			GetCurrent = () => { return PlayerSettings.graphicsJobs.ToString(); },
			Set = () => { PlayerSettings.graphicsJobs = true; }
		};

		var autoGraphicsAPi = new Item("Set Auto Graphics Api", false.ToString())
		{
			IsReady = () => { return (PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.Android) == false); },
			GetCurrent = () => { return PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.Android).ToString(); },
			Set = () => { PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false); }
		};

		UnityEngine.Rendering.GraphicsDeviceType[] apis = { UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 };
		var graphicsApis = new Item("Graphics Apis", UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3.ToString())
		{
			IsReady = () => { var curapi = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android); return curapi[0].Equals(UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3); },
			GetCurrent = () => { var curapi = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);  return (curapi.Length > 0) ? curapi[0].ToString() : "null"; },
			Set = () => { PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, apis); }
		};

		var AndroidMinSDK = new Item("Android Min SDK version", AndroidSdkVersions.AndroidApiLevel25.ToString())
		{
			IsReady = () => { return PlayerSettings.Android.minSdkVersion >= AndroidSdkVersions.AndroidApiLevel25; },
			GetCurrent = () => { return PlayerSettings.Android.minSdkVersion.ToString(); },
			Set = () => { PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel25; }
		};

		var AndroidTargetSDK = new Item("Android Target SDK version", AndroidSdkVersions.AndroidApiLevel26.ToString())
		{
			IsReady = () => { return PlayerSettings.Android.targetSdkVersion >= AndroidSdkVersions.AndroidApiLevel26; },
			GetCurrent = () => { return PlayerSettings.Android.targetSdkVersion.ToString(); },
			Set = () => { PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel26; }
		};

		var gpuSkinning = new Item("GPU Skinning", false.ToString())
		{
			IsReady = () => { return !PlayerSettings.gpuSkinning; },
			GetCurrent = () => { return PlayerSettings.gpuSkinning.ToString(); },
			Set = () => { PlayerSettings.gpuSkinning = false; }
		};

		var virtualRealityAndSinglePassSupport = GetVRItem();

		return new List<Item>()
		{
			builtTarget,
			defaultOrigin,
			enableMTRendering,
			gpuSkinning,
			graphicsJobs,
			autoGraphicsAPi,
			graphicsApis,
			AndroidMinSDK,
			AndroidTargetSDK,
			virtualRealityAndSinglePassSupport
		};
	}

	static WaveVR_Settings window;

	static WaveVR_Settings()
	{
		EditorApplication.update += Update;
	}

	[UnityEditor.MenuItem("WaveVR/Preference/DefaultPreferenceDialog")]
	static void UpdateWithClearIgnore()
	{
		var items = GetItems();
		UpdateInner(items, true);
	}

	static void Update()
	{
		Debug.Log("Check for WaveVR prefered editor settings");
		if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
		{
			var items = GetItems();
			UpdateInner(items, false);
		}
		EditorApplication.update -= Update;
	}

	public static void UpdateInner(List<Item> items, bool forceShow)
	{
		bool show = forceShow;
		if (!forceShow)
		{
			foreach (var item in items)
			{
				show |= !item.IsIgnored && !item.IsReady();
			}
		}

		if (show)
		{
			window = GetWindow<WaveVR_Settings>(true);
			window.minSize = new Vector2(640, 320);
			window.items = items;
		}
	}

	Vector2 scrollPosition;

	string GetResourcePath()
	{
		var ms = MonoScript.FromScriptableObject(this);
		var path = AssetDatabase.GetAssetPath(ms);
		path = Path.GetDirectoryName(path);
		return path.Substring(0, path.Length - "Editor".Length) + "Textures/";
	}

	public void OnGUI()
	{
		var resourcePath = GetResourcePath();
		var logo = AssetDatabase.LoadAssetAtPath<Texture2D>(resourcePath + "vivewave_logo_flat.png");
		var rect = GUILayoutUtility.GetRect(position.width, 150, GUI.skin.box);
		if (logo)
			GUI.DrawTexture(rect, logo, ScaleMode.ScaleToFit);

		EditorGUILayout.HelpBox("Recommended project settings for WaveVR:", MessageType.Warning);

		if (items == null)
			return;

		scrollPosition = GUILayout.BeginScrollView(scrollPosition);

		int notReadyItems = 0;
		foreach(var item in items)
		{
			if (item.Show())
				notReadyItems++;
		}

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();

		if (GUILayout.Button("Clear All Ignores"))
		{
			foreach (var item in items)
			{
				item.CleanIgnore();
			}
		}

		GUILayout.EndHorizontal();

		GUILayout.EndScrollView();

		GUILayout.FlexibleSpace();

		GUILayout.BeginHorizontal();
		if (notReadyItems > 0)
		{
			if (GUILayout.Button("Accept All"))
			{
				foreach (var item in items)
				{
					// Only set those that have not been explicitly ignored.
					if (!item.IsIgnored)
						item.Set();
				}

				EditorUtility.DisplayDialog("Accept All", "You made the right choice!", "Ok");

				Close();
			}

			if (GUILayout.Button("Ignore All"))
			{
				if (EditorUtility.DisplayDialog("Ignore All", "Are you sure?", "Yes, Ignore All", "Cancel"))
				{
					foreach (var item in items)
					{
						// Only ignore those that do not currently match our recommended settings.
						if (!item.IsReady())
							item.Ignore();
					}

					Close();
				}
			}
		}
		else 
		{
			if (GUILayout.Button("Close"))
				Close();
		}
		GUILayout.EndHorizontal();
	}
}
