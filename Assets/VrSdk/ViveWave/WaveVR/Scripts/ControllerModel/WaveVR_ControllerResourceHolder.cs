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
using wvr;
using WVR_Log;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using System;

public enum ModelSpecify
{
	MS_Dominant,
	MS_NonDominant
}

[System.Serializable]
public class BatteryIndicator
{
	public int level;
	public float min;
	public float max;
	public string texturePath;
	public bool textureLoaded;
	public Texture2D batteryTexture;
}

[System.Serializable]
public class TouchSetting
{
	public Vector3 touchForward;
	public Vector3 touchCenter;
	public Vector3 touchRight;
	public Vector3 touchPtU;
	public Vector3 touchPtW;
	public Vector3 touchPtV;
	public float raidus;
	public float touchptHeight;
}

[System.Serializable]
public class ModelResource
{
	public string renderModelName;
	public ModelSpecify modelSpecify;
	public bool mergeToOne;
	public uint sectionCount;

	public FBXInfo_t[] FBXInfo;
	public MeshInfo_t[] SectionInfo;
	public bool parserReady;

	public Texture2D modelTexture;

	public bool isTouchSetting;
	public TouchSetting TouchSetting;

	public bool isBatterySetting;
	public List<BatteryIndicator> batteryTextureList;
}

public class WaveVR_ControllerResourceHolder {
	private static string LOG_TAG = "WaveVR_ControllerResourceHolder";
	private Thread mthread;

	private static WaveVR_ControllerResourceHolder instance = null;
	public static WaveVR_ControllerResourceHolder Instance
	{
		get
		{
			if (instance == null)
			{
				Log.i(LOG_TAG, "create WaveVR_ControllerResourceHolder instance");

				instance = new WaveVR_ControllerResourceHolder();
			}
			return instance;
		}
	}

	private void PrintDebugLog(string msg)
	{
		Log.d(LOG_TAG, msg);
	}

	private void PrintInfoLog(string msg)
	{
		Log.i(LOG_TAG, msg);
	}

	private void PrintWarningLog(string msg)
	{
		Log.w(LOG_TAG, msg);
	}

	public List<ModelResource> renderModelList = new List<ModelResource>();

	public bool isRenderModelExist(string renderModel, ModelSpecify ms, bool merge)
	{
		foreach (ModelResource t in renderModelList)
		{
			if ((t.renderModelName == renderModel) && (t.mergeToOne == merge) && (t.modelSpecify == ms))
			{
				return true;
			}
		}

		return false;
	}

	public ModelResource getRenderModelResource(string renderModel, ModelSpecify ms, bool merge)
	{
		foreach (ModelResource t in renderModelList)
		{
			if ((t.renderModelName == renderModel) && (t.mergeToOne == merge) && (t.modelSpecify == ms))
			{
				return t;
			}
		}

		return null;
	}

	public bool addRenderModel(string renderModel, string ModelFolder, ModelSpecify ms, bool merge)
	{
		if (isRenderModelExist(renderModel, ms, merge))
			return false;

		string FBXFile = ModelFolder + "/";
		string imageFile = ModelFolder + "/";

		if (ms == ModelSpecify.MS_Dominant)
		{
			FBXFile += "controller00.fbx";
			imageFile += "controller00.png";
		} else
		{
			FBXFile += "controller01.fbx";
			imageFile += "controller01.png";
		}

		if (!File.Exists(FBXFile))
			return false;

		if (!File.Exists(imageFile))
			return false;
		PrintDebugLog("---  start  ---");
		ModelResource newMR = new ModelResource();
		newMR.renderModelName = renderModel;
		newMR.mergeToOne = merge;
		newMR.parserReady = false;
		newMR.modelSpecify = ms;
		renderModelList.Add(newMR);

		mthread = new Thread(() => readNativeData(newMR, merge, ModelFolder, ms));
		mthread.Start();

		PrintDebugLog("---  Read image file start  ---");
		byte[] imgByteArray = File.ReadAllBytes(imageFile);
		PrintDebugLog("---  Read image file end  ---");
		PrintDebugLog("---  Load image start  ---");
		Texture2D modelpng = new Texture2D(2, 2, TextureFormat.BGRA32, false);
		bool retLoad = modelpng.LoadImage(imgByteArray);
		if (retLoad)
		{
			PrintDebugLog("---  Load image end  ---, size: " + imgByteArray.Length);
		}
		else
		{
			PrintWarningLog("failed to load texture");
		}
		newMR.modelTexture = modelpng;
		PrintDebugLog("---  Parse battery image start  ---");
		newMR.isBatterySetting = getBatteryIndicatorParam(newMR, ModelFolder, ms);
		PrintDebugLog("---  Parse battery image end  ---");
		PrintDebugLog("---  end  ---");

		return true;
	}

	void readNativeData(ModelResource curr, bool mergeTo, string modelFolderPath, ModelSpecify ms)
	{
		PrintDebugLog("---  thread start  ---");
		PrintInfoLog("Render model name: " + curr.renderModelName + ", merge = " + curr.mergeToOne);

		IntPtr ptrError = Marshal.AllocHGlobal(64);
		string FBXFile = modelFolderPath + "/";
		if (ms == ModelSpecify.MS_Dominant)
		{
			FBXFile += "controller00.fbx";
		}
		else
		{
			FBXFile += "controller01.fbx";
		}

		bool ret = false;
		uint sessionid = 0;
		uint sectionCount = 0;
		string errorCode = "";

		if (File.Exists(FBXFile))
		{
			ret = Interop.WVR_OpenMesh(FBXFile, ref sessionid, ptrError, mergeTo);
			errorCode = Marshal.PtrToStringAnsi(ptrError);

			if (!ret)
			{
				PrintWarningLog("FBX parse failed: " + errorCode);
				return;
			}
		} else
		{
			PrintWarningLog("FBX is not found");
			return;
		}

		PrintInfoLog("FBX parse succeed, sessionid = " + sessionid);
		bool finishLoading = Interop.WVR_GetSectionCount(sessionid, ref sectionCount);

		if (!finishLoading || sectionCount == 0)
		{
			PrintWarningLog("failed to load mesh");
			return;
		}

		curr.sectionCount = sectionCount;
		curr.FBXInfo = new FBXInfo_t[curr.sectionCount];
		curr.SectionInfo = new MeshInfo_t[curr.sectionCount];

		for (int i = 0; i < curr.sectionCount; i++)
		{
			curr.FBXInfo[i] = new FBXInfo_t();
			curr.SectionInfo[i] = new MeshInfo_t();

			curr.FBXInfo[i].meshName = Marshal.AllocHGlobal(256);
		}


		ret = Interop.WVR_GetMeshData(sessionid, curr.FBXInfo);
		if (!ret)
		{
			for (int i = 0; i < sectionCount; i++)
			{
				Marshal.FreeHGlobal(curr.FBXInfo[i].meshName);
			}

			curr.SectionInfo = null;
			curr.FBXInfo = null;
			Interop.WVR_ReleaseMesh(sessionid);
			return;
		}

		for (uint i = 0; i < curr.sectionCount; i++)
		{
			curr.SectionInfo[i]._vectice = new Vector3[curr.FBXInfo[i].verticeCount];
			for (int j = 0; j < curr.FBXInfo[i].verticeCount; j++)
			{
				curr.SectionInfo[i]._vectice[j] = new Vector3();
			}
			curr.SectionInfo[i]._normal = new Vector3[curr.FBXInfo[i].normalCount];
			for (int j = 0; j < curr.FBXInfo[i].verticeCount; j++)
			{
				curr.SectionInfo[i]._normal[j] = new Vector3();
			}
			curr.SectionInfo[i]._uv = new Vector2[curr.FBXInfo[i].uvCount];
			for (int j = 0; j < curr.FBXInfo[i].verticeCount; j++)
			{
				curr.SectionInfo[i]._uv[j] = new Vector2();
			}
			curr.SectionInfo[i]._indice = new int[curr.FBXInfo[i].indiceCount];
			for (int j = 0; j < curr.FBXInfo[i].verticeCount; j++)
			{
				curr.SectionInfo[i]._indice[j] = new int();
			}

			bool active = false;

			bool tret = Interop.WVR_GetSectionData(sessionid, i, curr.SectionInfo[i]._vectice, curr.SectionInfo[i]._normal, curr.SectionInfo[i]._uv, curr.SectionInfo[i]._indice, ref active);
			if (!tret) continue;

			curr.SectionInfo[i]._active = active;

			PrintInfoLog("i = " + i + ", name = " + Marshal.PtrToStringAnsi(curr.FBXInfo[i].meshName) + ", active = " + curr.SectionInfo[i]._active);
			PrintInfoLog("i = " + i + ", relative transform = [" + curr.FBXInfo[i].matrix.m0 + " , " + curr.FBXInfo[i].matrix.m1 + " , " + curr.FBXInfo[i].matrix.m2 + " , " + curr.FBXInfo[i].matrix.m3 + "] ");
			PrintInfoLog("i = " + i + ", relative transform = [" + curr.FBXInfo[i].matrix.m4 + " , " + curr.FBXInfo[i].matrix.m5 + " , " + curr.FBXInfo[i].matrix.m6 + " , " + curr.FBXInfo[i].matrix.m7 + "] ");
			PrintInfoLog("i = " + i + ", relative transform = [" + curr.FBXInfo[i].matrix.m8 + " , " + curr.FBXInfo[i].matrix.m9 + " , " + curr.FBXInfo[i].matrix.m10 + " , " + curr.FBXInfo[i].matrix.m11 + "] ");
			PrintInfoLog("i = " + i + ", relative transform = [" + curr.FBXInfo[i].matrix.m12 + " , " + curr.FBXInfo[i].matrix.m13 + " , " + curr.FBXInfo[i].matrix.m14 + " , " + curr.FBXInfo[i].matrix.m15 + "] ");
			PrintInfoLog("i = " + i + ", vertice count = " + curr.FBXInfo[i].verticeCount + ", normal count = " + curr.FBXInfo[i].normalCount + ", uv count = " + curr.FBXInfo[i].uvCount + ", indice count = " + curr.FBXInfo[i].indiceCount);
		}
		Interop.WVR_ReleaseMesh(sessionid);
		curr.isTouchSetting = GetTouchPadParam(curr, modelFolderPath, ms);
		curr.parserReady = true;
		PrintDebugLog("---  thread end  ---");
	}

	private bool GetTouchPadParam(ModelResource curr, string modelFolderPath, ModelSpecify ms)
	{
		if (curr == null)
		{
			PrintWarningLog("Model resource is null!");
			return false;
		}

		string TouchPadJsonPath = modelFolderPath + "/";

		if (ms == ModelSpecify.MS_Dominant)
		{
			TouchPadJsonPath += "Touchpad.json";
		}
		else
		{
			TouchPadJsonPath += "Touchpad01.json";
		}

		if (!File.Exists(TouchPadJsonPath))
		{
			PrintWarningLog(TouchPadJsonPath + " is not found!");
			return false;
		}

		StreamReader json_sr = new StreamReader(TouchPadJsonPath);

		string JsonString = json_sr.ReadToEnd();
		PrintInfoLog("Touchpad json: " + JsonString);
		json_sr.Close();

		if (JsonString.Equals(""))
		{
			PrintWarningLog("JsonString is empty!");
			return false;
		}

		curr.TouchSetting = new TouchSetting();
		try
		{
			SimpleJSON.JSONNode jsNodes = SimpleJSON.JSONNode.Parse(JsonString);
			string xvalue = "";
			string yvalue = "";
			string zvalue = "";
			string floatingStr = "";

			xvalue = jsNodes["center"]["x"].Value;
			yvalue = jsNodes["center"]["y"].Value;
			zvalue = jsNodes["center"]["z"].Value;
			if (xvalue.Equals("") || yvalue.Equals("") || zvalue.Equals(""))
			{
				PrintWarningLog("Touch Center pointer is not found!");
				return false;
			}
			curr.TouchSetting.touchCenter = new Vector3(float.Parse(xvalue), float.Parse(yvalue), float.Parse(zvalue));
			PrintDebugLog("Touch Center pointer is found! x: " + curr.TouchSetting.touchCenter.x + " ,y: " + curr.TouchSetting.touchCenter.y + " ,z: " + curr.TouchSetting.touchCenter.z);

			xvalue = jsNodes["up"]["x"].Value;
			yvalue = jsNodes["up"]["y"].Value;
			zvalue = jsNodes["up"]["z"].Value;
			if (xvalue.Equals("") || yvalue.Equals("") || zvalue.Equals(""))
			{
				PrintWarningLog("Touch Up pointer is not found!");
				return false;
			}
			curr.TouchSetting.touchForward = new Vector3(float.Parse(xvalue), float.Parse(yvalue), float.Parse(zvalue));
			PrintDebugLog("Touch Up pointer is found! x: " + curr.TouchSetting.touchForward.x + " ,y: " + curr.TouchSetting.touchForward.y + " ,z: " + curr.TouchSetting.touchForward.z);

			xvalue = jsNodes["right"]["x"].Value;
			yvalue = jsNodes["right"]["y"].Value;
			zvalue = jsNodes["right"]["z"].Value;
			if (xvalue.Equals("") || yvalue.Equals("") || zvalue.Equals(""))
			{
				PrintWarningLog("Touch right pointer is not found!");
				return false;
			}
			curr.TouchSetting.touchRight = new Vector3(float.Parse(xvalue), float.Parse(yvalue), float.Parse(zvalue));
			PrintDebugLog("Touch right pointer is found! x: " + curr.TouchSetting.touchRight.x + " ,y: " + curr.TouchSetting.touchRight.y + " ,z: " + curr.TouchSetting.touchRight.z);
			floatingStr = jsNodes["FloatingDistance"].Value;

			if (floatingStr.Equals(""))
			{
				PrintWarningLog("floatingStr is not found!");
				return false;
			}

			curr.TouchSetting.touchptHeight = float.Parse(floatingStr);
			PrintInfoLog("Floating distance : " + curr.TouchSetting.touchptHeight);

			curr.TouchSetting.touchPtW = (curr.TouchSetting.touchForward - curr.TouchSetting.touchCenter).normalized; //analog +y direction.
			curr.TouchSetting.touchPtU = (curr.TouchSetting.touchRight - curr.TouchSetting.touchCenter).normalized; //analog +x direction.
			curr.TouchSetting.touchPtV = Vector3.Cross(curr.TouchSetting.touchPtU, curr.TouchSetting.touchPtW).normalized;
			curr.TouchSetting.raidus = (curr.TouchSetting.touchForward - curr.TouchSetting.touchCenter).magnitude;

			PrintInfoLog("touchPtW! x: " + curr.TouchSetting.touchPtW.x + " ,y: " + curr.TouchSetting.touchPtW.y + " ,z: " + curr.TouchSetting.touchPtW.z);
			PrintInfoLog("touchPtU! x: " + curr.TouchSetting.touchPtU.x + " ,y: " + curr.TouchSetting.touchPtU.y + " ,z: " + curr.TouchSetting.touchPtU.z);
			PrintInfoLog("touchPtV! x: " + curr.TouchSetting.touchPtV.x + " ,y: " + curr.TouchSetting.touchPtV.y + " ,z: " + curr.TouchSetting.touchPtV.z);
			PrintInfoLog("raidus: " + curr.TouchSetting.raidus);
		}
		catch (Exception e)
		{
			Log.e(LOG_TAG, "JsonParse failed: " + e.ToString());
			return false;
		}
		return true;
	}

	bool getBatteryIndicatorParam(ModelResource curr, string modelFolderPath, ModelSpecify ms)
	{
		if (curr == null)
		{
			PrintWarningLog("Model resource is null!");
			return false;
		}

		string batteryJsonFile = modelFolderPath + "/";

		if (ms == ModelSpecify.MS_Dominant)
		{
			batteryJsonFile += "BatteryIndicator.json";
		}
		else
		{
			batteryJsonFile += "BatteryIndicator01.json";
		}

		if (!File.Exists(batteryJsonFile))
		{
			PrintWarningLog(batteryJsonFile + " is not found!");
			return false;
		}

		StreamReader json_sr = new StreamReader(batteryJsonFile);

		string JsonString = json_sr.ReadToEnd();
		PrintInfoLog("BatteryIndicator json: " + JsonString);
		json_sr.Close();

		if (JsonString.Equals(""))
		{
			PrintWarningLog("JsonString is empty!");
			return false;
		}

		SimpleJSON.JSONNode jsNodes = SimpleJSON.JSONNode.Parse(JsonString);

		string tmpStr = "";
		tmpStr = jsNodes["LevelCount"].Value;

		if (tmpStr.Equals(""))
		{
			PrintWarningLog("Battery level is not found!");
			return false;
		}

		int batteryLevel = int.Parse(tmpStr);
		PrintInfoLog("Battery level is " + batteryLevel);

		if (batteryLevel <= 0)
		{
			PrintWarningLog("Battery level is less or equal to 0!");
			return false;
		}
		List<BatteryIndicator> batteryTextureList = new List<BatteryIndicator>();

		for (int i = 0; i < batteryLevel; i++)
		{
			string minStr = jsNodes["BatteryLevel"][i]["min"].Value;
			string maxStr = jsNodes["BatteryLevel"][i]["max"].Value;
			string pathStr = jsNodes["BatteryLevel"][i]["path"].Value;

			if (minStr.Equals("") || maxStr.Equals("") || pathStr.Equals(""))
			{
				PrintWarningLog("Min, Max or Path is not found!");
				batteryLevel = 0;
				batteryTextureList.Clear();
				return false;
			}

			string batteryLevelFile = modelFolderPath + "/" + pathStr;

			if (!File.Exists(batteryLevelFile))
			{
				PrintWarningLog(batteryLevelFile + " is not found!");
				batteryLevel = 0;
				batteryTextureList.Clear();
				return false;
			}

			BatteryIndicator tmpBI = new BatteryIndicator();
			tmpBI.level = i;
			tmpBI.min = float.Parse(minStr);
			tmpBI.max = float.Parse(maxStr);
			tmpBI.texturePath = batteryLevelFile;

			byte[] imgByteArray = File.ReadAllBytes(batteryLevelFile);
			PrintDebugLog("Image size: " + imgByteArray.Length);

			tmpBI.batteryTexture = new Texture2D(2, 2, TextureFormat.BGRA32, false);
			tmpBI.textureLoaded = tmpBI.batteryTexture.LoadImage(imgByteArray);

			PrintInfoLog("Battery Level: " + tmpBI.level + " min: " + tmpBI.min + " max: " + tmpBI.max + " path: " + tmpBI.texturePath + " loaded: " + tmpBI.textureLoaded);

			batteryTextureList.Add(tmpBI);
		}

		curr.batteryTextureList = batteryTextureList;
		PrintInfoLog("BatteryIndicator is ready!");
		return true;
	}
}
