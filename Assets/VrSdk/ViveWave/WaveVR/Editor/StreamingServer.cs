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
using UnityEditor;
using System.Diagnostics;
using System;
using System.Linq;
using System.IO;

public class StreamingServer
{
	public static Process myProcess = new Process();

	[UnityEditor.MenuItem("WaveVR/DirectPreview/Start Streaming Server")]
	static void StartStreamingServerMenu()
	{
		StartStreamingServer();
	}

	[UnityEditor.MenuItem("WaveVR/DirectPreview/Stop Streaming Server")]
	static void StopStreamingServerMenu()
	{
		StopStreamingServer();
	}

	public static bool isStreamingServerExist()
	{
		var monoScripts = MonoImporter.GetAllRuntimeMonoScripts();
		var monoScript = monoScripts.FirstOrDefault(script => script.GetClass() == typeof(WaveVR));
		var path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(monoScript));
		var fullPath = Path.GetFullPath((path.Substring(0, path.Length - "Scripts".Length) + "Platform/Windows/dpServer.exe").Replace("\\", "/"));

		UnityEngine.Debug.Log("StartStreamingServer at " + fullPath);

		return File.Exists(fullPath);
	}

	// Launch rrServer
	public static void StartStreamingServer()
	{
		if (isStreamingServerExist())
		{
			try
			{
				var monoScripts = MonoImporter.GetAllRuntimeMonoScripts();
				var monoScript = monoScripts.FirstOrDefault(script => script.GetClass() == typeof(WaveVR));
				var path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(monoScript));
				var fullPath = Path.GetFullPath((path.Substring(0, path.Length - "Scripts".Length) + "Platform/Windows").Replace("\\", "/"));

				var wvrAarFolder = fullPath.Substring(fullPath.IndexOf("Assets"), fullPath.Length - fullPath.IndexOf("Assets"));
				UnityEngine.Debug.Log("StartStreamingServer at " + wvrAarFolder);
				//Get the path of the Game data folder
				myProcess.StartInfo.FileName = "C:\\Windows\\system32\\cmd.exe";
				myProcess.StartInfo.Arguments = "/c cd " + wvrAarFolder + " && dpServer";
				myProcess.Start();
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogError(e);
			}
		} else
		{
			// dpServer is not found
			UnityEngine.Debug.LogError("Streaming server is not found, please update full package from https://developer.vive.com/resources/knowledgebase/wave-sdk/");
		}

	}
	// Stop rrServer
	public static void StopStreamingServer()
	{
		if (isStreamingServerExist())
		{
			try
			{
				UnityEngine.Debug.Log("Stop Streaming Server.");
				myProcess.StartInfo.FileName = "C:\\Windows\\system32\\cmd.exe";
				myProcess.StartInfo.Arguments = "/c taskkill /F /IM dpServer.exe";
				myProcess.Start();
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogError(e);
			}
		}
		else
		{
			// dpServer is not found
			UnityEngine.Debug.LogError("Streaming server is not found, please update full package from https://developer.vive.com/resources/knowledgebase/wave-sdk/");
		}
	}
}
