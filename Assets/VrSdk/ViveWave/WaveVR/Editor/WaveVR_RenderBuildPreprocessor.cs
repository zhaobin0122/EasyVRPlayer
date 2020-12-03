// "WaveVR SDK
// © 2017 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the WaveVR SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
#if UNITY_2018_1_OR_NEWER
using UnityEditor.Build.Reporting;
#endif
using UnityEngine;

public class WaveVR_RenderBuildPreprocessor :
#if UNITY_2018_1_OR_NEWER
		IPreprocessBuildWithReport
#else
		IPreprocessBuild
#endif
{
	public int callbackOrder { get { return 0; } }

	void SinglePassPreProcess()
	{
		if (target != BuildTarget.Android && target != BuildTarget.StandaloneWindows && target != BuildTarget.StandaloneWindows64)
		{
			Debug.LogError("Target platform is not Android or Windows");
			return;
		}

#if UNITY_STANDALONE
		var vrSupported = WaveVR_Settings.GetVirtualRealitySupported(BuildTargetGroup.Standalone);
		var list = WaveVR_Settings.GetVirtualRealitySDKs(BuildTargetGroup.Standalone);
		var hasVRDevice = ArrayUtility.Contains<string>(list, WaveVR_Settings.WVRSinglePassDeviceName);
#if UNITY_2018_2_OR_NEWER
		// Please remove old name
		if (ArrayUtility.Contains<string>(list, "split"))
			Debug.LogError("Contains old VR device name in XR settings.\nPlease remove it.");
#endif
		var stereoRenderingPath = PlayerSettings.stereoRenderingPath;
		List<string> allDefines = WaveVR_Settings.GetDefineSymbols(BuildTargetGroup.Standalone);
		var hasDefine = allDefines.Contains(WaveVR_Settings.WVRSPDEF);

		// if single pass enabled in PlayerSettings, set the define.  Here is a final check.
		Debug.Log("SinglePassPreProcess: vrSupported=" + vrSupported + ", stereoRenderingPath=" + stereoRenderingPath +
			", hasVRDevice=" + hasVRDevice + ", hasDefine=" + hasDefine);
		var set = vrSupported && hasVRDevice && stereoRenderingPath == StereoRenderingPath.SinglePass;

		WaveVR_Settings.SetSinglePassDefine(group, set, allDefines);
#else
		var vrSupported = WaveVR_Settings.GetVirtualRealitySupported(BuildTargetGroup.Android);
		var list = WaveVR_Settings.GetVirtualRealitySDKs(BuildTargetGroup.Android);
		var hasVRDevice = ArrayUtility.Contains<string>(list, WaveVR_Settings.WVRSinglePassDeviceName);
#if UNITY_2018_2_OR_NEWER
		// Please remove old name
		if (ArrayUtility.Contains<string>(list, "split"))
			Debug.LogError("Contains old VR device name in XR settings.\nPlease remove it.");
#endif
		var stereoRenderingPath = PlayerSettings.stereoRenderingPath;
		List<string> allDefines = WaveVR_Settings.GetDefineSymbols(BuildTargetGroup.Android);
		var hasDefine = allDefines.Contains(WaveVR_Settings.WVRSPDEF);

		// if single pass enabled in PlayerSettings, set the define.  Here is a final check.
		Debug.Log("SinglePassPreProcess: vrSupported=" + vrSupported + ", stereoRenderingPath=" + stereoRenderingPath +
			", hasVRDevice=" + hasVRDevice + ", hasDefine=" + hasDefine);
		var set = vrSupported && hasVRDevice && stereoRenderingPath == StereoRenderingPath.SinglePass;

		WaveVR_Settings.SetSinglePassDefine(group, set, allDefines);
#endif
	}

	public BuildTargetGroup group;
	public BuildTarget target;

	public void OnPreprocessBuild(BuildTarget target, string path)
	{
		this.target = target;
		if (target != BuildTarget.Android && target != BuildTarget.StandaloneWindows && target != BuildTarget.StandaloneWindows64)
			return;
#if UNITY_STANDALONE
		this.group = BuildTargetGroup.Standalone;
#else
		this.group = BuildTargetGroup.Android;
#endif

		SinglePassPreProcess();
	}

#if UNITY_2018_1_OR_NEWER
	public void OnPreprocessBuild(BuildReport report)
	{
		target = report.summary.platform;
		group = report.summary.platformGroup;

		SinglePassPreProcess();
	}
#endif
}
