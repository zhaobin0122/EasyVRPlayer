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
using System;
using System.Runtime.InteropServices;
using System.IO;

public class WaveVR_RenderModel : MonoBehaviour
{
	private static string LOG_TAG = "WaveVR_RenderModel";
	private void PrintDebugLog(string msg)
	{
		Log.d(LOG_TAG, "Hand: " + WhichHand + ", " + msg);
	}

	private void PrintInfoLog(string msg)
	{
		Log.i(LOG_TAG, "Hand: " + WhichHand + ", " + msg);
	}

	private void PrintWarningLog(string msg)
	{
		Log.w(LOG_TAG, "Hand: " + WhichHand + ", " + msg);
	}

	public enum ControllerHand
	{
		Controller_Dominant,
		Controller_NonDominant
	};

	public enum LoadingState
	{
		LoadingState_NOT_LOADED,
		LoadingState_LOADING,
		LoadingState_LOADED
	}

	public ControllerHand WhichHand = ControllerHand.Controller_Dominant;
	public GameObject defaultModel = null;
	public bool updateDynamically = false;
	public bool mergeToOneBone = false;

	private GameObject controllerSpawned = null;
	private WaveVR_Controller.EDeviceType deviceType = WaveVR_Controller.EDeviceType.Dominant;
	private bool connected = false;
	private string renderModelNamePath = "";
	private string renderModelName = "";
	private IntPtr ptrParameterName = IntPtr.Zero;
	private IntPtr ptrResult = IntPtr.Zero;

	private List<Color32> colors = new List<Color32>();
	private GameObject meshCom = null;
	private GameObject meshGO = null;
	private Mesh updateMesh;
	private Material modelMat;
	private Material ImgMaterial;
	private WaitForEndOfFrame wfef = null;
	private WaitForSeconds wfs = null;
	private bool showBatterIndicator = true;
	private bool isBatteryIndicatorReady = false;
	private BatteryIndicator currentBattery;
	private GameObject batteryGO = null;
	private MeshRenderer batteryMR = null;

	private ModelResource modelResource = null;
	private ModelSpecify modelSpecify;
	private LoadingState mLoadingState = LoadingState.LoadingState_NOT_LOADED;

	void OnEnable()
	{
		PrintDebugLog ("OnEnable");
		if (mLoadingState == LoadingState.LoadingState_LOADING)
		{
			deleteChild ("RenderModel doesn't expect model is in loading, delete all children");
		}

		if (WhichHand == ControllerHand.Controller_Dominant)
		{
			deviceType = WaveVR_Controller.EDeviceType.Dominant;
			modelSpecify = ModelSpecify.MS_Dominant;
		} else
		{
			deviceType = WaveVR_Controller.EDeviceType.NonDominant;
			modelSpecify = ModelSpecify.MS_NonDominant;
		}

		connected = checkConnection ();

		if (connected)
		{
			WaveVR.Device _device = WaveVR.Instance.getDeviceByType (this.deviceType);

			if (mLoadingState == LoadingState.LoadingState_LOADED)
			{
				if (isRenderModelNameSameAsPrevious ())
				{
					PrintDebugLog ("OnEnable - Controller connected, model was loaded!");
				} else
				{
					deleteChild ("Controller load when OnEnable, render model is different!");
					onLoadController (_device.type);
				}
			} else
			{
				PrintDebugLog ("Controller load when OnEnable!");
				onLoadController (_device.type);
			}
		}

		WaveVR_Utils.Event.Listen (WaveVR_Utils.Event.DEVICE_CONNECTED, onDeviceConnected);
		WaveVR_Utils.Event.Listen (WaveVR_Utils.Event.OEM_CONFIG_CHANGED, onOEMConfigChanged);
	}

	void OnDisable()
	{
		PrintDebugLog ("OnDisable");
		if (mLoadingState == LoadingState.LoadingState_LOADING)
		{
			deleteChild ("RenderModel doesn't complete creating meshes before OnDisable, delete all children");
		}

		WaveVR_Utils.Event.Remove (WaveVR_Utils.Event.DEVICE_CONNECTED, onDeviceConnected);
		WaveVR_Utils.Event.Remove (WaveVR_Utils.Event.OEM_CONFIG_CHANGED, onOEMConfigChanged);
	}

	private void onOEMConfigChanged(params object[] args)
	{
		PrintDebugLog("onOEMConfigChanged");
		ReadJsonValues();
	}

	private void ReadJsonValues()
	{
		showBatterIndicator = false;
		string json_values = WaveVR_Utils.OEMConfig.getBatteryConfig();

		if (!json_values.Equals(""))
		{
			try
			{
				SimpleJSON.JSONNode jsNodes = SimpleJSON.JSONNode.Parse(json_values);

				string node_value = "";
				node_value = jsNodes["show"].Value;
				if (!node_value.Equals(""))
				{
					if (node_value.Equals("2")) // always
					{
						showBatterIndicator = true;
					}
				}
			}
			catch (Exception e)
			{
				Log.e(LOG_TAG,"JsonParse failed: " + e.ToString());
			}
		}

		PrintDebugLog("showBatterIndicator: " + showBatterIndicator);
	}

	private void onDeviceConnected(params object[] args)
	{
		WVR_DeviceType eventType = (WVR_DeviceType)args[0];
		WVR_DeviceType _type = WVR_DeviceType.WVR_DeviceType_Invalid;

		bool _connected = false;

		{
			WaveVR.Device _device = WaveVR.Instance.getDeviceByType(this.deviceType);
			_connected = _device.connected;
			_type = _device.type;

			if (eventType != _type)
			{
				PrintDebugLog("onDeviceConnected() event type is " + eventType + ", this.deviceType is " + _type + ", skip");
				return;
			}
		}

		PrintDebugLog("onDeviceConnected() " + _type + " is " + (_connected ? "connected" : "disconnected") + ", left-handed? " + WaveVR_Controller.IsLeftHanded);

		if (connected != _connected)
		{
			connected = _connected;

			if (connected)
			{
				if (mLoadingState == LoadingState.LoadingState_LOADED)
				{
					if (isRenderModelNameSameAsPrevious())
					{
						PrintDebugLog("onDeviceConnected - Controller connected, model was loaded!");
					}
					else
					{
						deleteChild("Controller load when onDeviceConnected, render model is different!");
						onLoadController(_type);
					}
				}
				else
				{
					if (mLoadingState == LoadingState.LoadingState_LOADING)
					{
						PrintDebugLog("onDeviceConnected - Controller connected, model is loading!");
					}
					else
					{
						PrintDebugLog("Controller load when onDeviceConnected!");
						onLoadController(_type);
					}
				}
			}
		}
	}

	private bool isRenderModelNameSameAsPrevious()
	{
		WVR_DeviceType type = WaveVR_Controller.Input(this.deviceType).DeviceType;
		bool _connected = WaveVR_Controller.Input(this.deviceType).connected;
		bool _same = false;

		if (!_connected)
			return _same;

		string parameterName = "GetRenderModelName";
		ptrParameterName = Marshal.StringToHGlobalAnsi(parameterName);
		ptrResult = Marshal.AllocHGlobal(64);
		uint resultVertLength = 64;

		Interop.WVR_GetParameters(type, ptrParameterName, ptrResult, resultVertLength);
		string tmprenderModelName = Marshal.PtrToStringAnsi(ptrResult);

		PrintDebugLog("previous render model: " + renderModelName + ", current render model name: " + tmprenderModelName);

		if (tmprenderModelName == renderModelName)
		{
			_same = true;
		}
		Marshal.FreeHGlobal(ptrParameterName);
		Marshal.FreeHGlobal(ptrResult);

		return _same;
	}

	// Use this for initialization
	void Start()
	{
		PrintDebugLog("start() connect: " + connected + " Which hand: " + WhichHand);
		wfs = new WaitForSeconds(1.0f);
		ReadJsonValues();

		if (updateDynamically)
		{
			PrintDebugLog("updateDynamically, start a coroutine to check connection and render model name periodly");
			StartCoroutine(checkRenderModelAndDelete());
		}
	}

	int t = 0;
	bool IsFocusCapturedBySystemLastFrame = false;

	// Update is called once per frame
	void Update()
	{
		if (Interop.WVR_IsInputFocusCapturedBySystem())
		{
			IsFocusCapturedBySystemLastFrame = true;
			return;
		}

		if (IsFocusCapturedBySystemLastFrame || (t-- < 0))
		{
			updateBatteryLevel();
			t = 200;
			IsFocusCapturedBySystemLastFrame = false;
		}

		if (Log.gpl.Print)
			Log.d(LOG_TAG, "Update() render model " + WhichHand + " connect ? " + this.connected + ", child object count ? " + transform.childCount + ", showBatterIndicator: " + showBatterIndicator + ", hasBattery: " + isBatteryIndicatorReady);
	}

	private void onLoadController(WVR_DeviceType type)
	{
		mLoadingState = LoadingState.LoadingState_LOADING;
		PrintDebugLog("Pos: " + this.transform.localPosition.x + " " + this.transform.localPosition.y + " " + this.transform.localPosition.z);
		PrintDebugLog("Rot: " + this.transform.localEulerAngles);

		if (Interop.WVR_GetWaveRuntimeVersion() < 2)
		{
			PrintDebugLog("onLoadController in old service");
			if (defaultModel != null)
			{
				controllerSpawned = Instantiate(defaultModel, this.transform);
				controllerSpawned.transform.parent = this.transform;
			}
			mLoadingState = LoadingState.LoadingState_NOT_LOADED;
			return;
		}

		string parameterName = "GetRenderModelName";
		ptrParameterName = Marshal.StringToHGlobalAnsi(parameterName);
		ptrResult = Marshal.AllocHGlobal(64);
		uint resultVertLength = 64;
		uint retOfRenderModel = Interop.WVR_GetParameters(type, ptrParameterName, ptrResult, resultVertLength);
		if (retOfRenderModel == 0)
		{
			PrintDebugLog("Can not find render model.");
			if (defaultModel != null)
			{
				PrintDebugLog("Can't load controller model from DS, load default model");
				controllerSpawned = Instantiate(defaultModel, this.transform);
				controllerSpawned.transform.parent = this.transform;
				mLoadingState = LoadingState.LoadingState_NOT_LOADED;
			}
			Marshal.FreeHGlobal(ptrParameterName);
			Marshal.FreeHGlobal(ptrResult);
			return;
		}
		renderModelName = Marshal.PtrToStringAnsi(ptrResult);

		int deviceIndex = -1;
		parameterName = "backdoor_get_device_index";
		ptrParameterName = Marshal.StringToHGlobalAnsi(parameterName);
		IntPtr ptrResultDeviceIndex = Marshal.AllocHGlobal(2);
		Interop.WVR_GetParameters(type, ptrParameterName, ptrResultDeviceIndex, 2);

		int _out = 0;
		bool _ret = int.TryParse(Marshal.PtrToStringAnsi(ptrResultDeviceIndex), out _out);
		if (_ret)
			deviceIndex = _out;

		PrintInfoLog("get controller id from runtime is " + renderModelName + ", deviceIndex = " + deviceIndex);

		// 1. check if there are assets in private folder
		string renderModelFolderPath = Interop.WVR_DeployRenderModelAssets(deviceIndex, renderModelName);

		mLoadingState = (renderModelFolderPath != "") ? LoadingState.LoadingState_LOADING : LoadingState.LoadingState_NOT_LOADED;

		if (renderModelFolderPath != "")
		{
			bool retModel = false;
			modelResource = null;
			renderModelNamePath = renderModelFolderPath + "Model";

			retModel = WaveVR_ControllerResourceHolder.Instance.addRenderModel(renderModelName, renderModelNamePath, modelSpecify, mergeToOneBone);
			if (retModel)
			{
				PrintDebugLog("Add " + renderModelName + " with " + modelSpecify + " model sucessfully!");
			}

			modelResource = WaveVR_ControllerResourceHolder.Instance.getRenderModelResource(renderModelName, modelSpecify, mergeToOneBone);

			if ((modelResource == null) && (modelSpecify == ModelSpecify.MS_NonDominant))
			{
				retModel = WaveVR_ControllerResourceHolder.Instance.addRenderModel(renderModelName, renderModelNamePath, ModelSpecify.MS_Dominant, mergeToOneBone);
				if (retModel)
				{
					PrintDebugLog("Add " + renderModelName + " Dominant model sucessfully!");
				}

				modelResource = WaveVR_ControllerResourceHolder.Instance.getRenderModelResource(renderModelName, ModelSpecify.MS_Dominant, mergeToOneBone);
			}

			if (modelResource != null)
			{
				mLoadingState = LoadingState.LoadingState_LOADING;

				PrintDebugLog("Starting load " + renderModelName + " with <" + modelResource.modelSpecify + "> model!");

				ImgMaterial = new Material(Shader.Find("Unlit/Texture"));
				wfef = new WaitForEndOfFrame();

				StartCoroutine(SpawnRenderModel());
			} else
			{
				PrintDebugLog("Model is null!");

				if (defaultModel != null)
				{
					PrintDebugLog("Can't load controller model from DS, load default model");
					controllerSpawned = Instantiate(defaultModel, this.transform);
					controllerSpawned.transform.parent = this.transform;
					mLoadingState = LoadingState.LoadingState_LOADED;
				}
			}
		}

		Marshal.FreeHGlobal(ptrParameterName);
		Marshal.FreeHGlobal(ptrResult);
	}

	string emitterMeshName = "__CM__Emitter";

	IEnumerator SpawnRenderModel()
	{
		while(true)
		{
			if (modelResource != null)
			{
				if (modelResource.parserReady) break;
			}
			PrintDebugLog("SpawnRenderModel is waiting");
			yield return wfef;
		}

		PrintDebugLog("Start to spawn all meshes!");

		if (modelResource == null)
		{
			PrintDebugLog("modelResource is null, skipping spawn objects");
			mLoadingState = LoadingState.LoadingState_NOT_LOADED;
			yield return null;
		}

		string meshName = "";
		for (uint i = 0; i < modelResource.sectionCount; i++)
		{
			meshName = Marshal.PtrToStringAnsi(modelResource.FBXInfo[i].meshName);
			meshCom = null;
			meshGO = null;

			bool meshAlready = false;

			for (uint j = 0; j < i; j++)
			{
				string tmp = Marshal.PtrToStringAnsi(modelResource.FBXInfo[j].meshName);

				if (tmp.Equals(meshName))
				{
					meshAlready = true;
				}
			}

			if (meshAlready)
			{
				PrintDebugLog(meshName + " is created! skip.");
				continue;
			}

			if (mergeToOneBone && modelResource.SectionInfo[i]._active)
			{
				meshName = "Merge_" + meshName;
			}
			updateMesh = new Mesh();
			meshCom = new GameObject();
			meshCom.AddComponent<MeshRenderer>();
			meshCom.AddComponent<MeshFilter>();
			meshGO = Instantiate(meshCom);
			meshGO.transform.parent = this.transform;
			meshGO.name = meshName;

			Matrix4x4 t = WaveVR_Utils.RigidTransform.toMatrix44(modelResource.FBXInfo[i].matrix);

			Vector3 x = WaveVR_Utils.GetPosition(t);
			meshGO.transform.localPosition = new Vector3(x.x, x.y, -x.z);

			meshGO.transform.localRotation = WaveVR_Utils.GetRotation(t);
			Vector3 r = meshGO.transform.localEulerAngles;
			meshGO.transform.localEulerAngles = new Vector3(-r.x, r.y, r.z);
			meshGO.transform.localScale = WaveVR_Utils.GetScale(t);

			PrintDebugLog("i = " + i + " MeshGO = " + meshName + ", localPosition: " + meshGO.transform.localPosition.x + ", " + meshGO.transform.localPosition.y + ", " + meshGO.transform.localPosition.z);
			PrintDebugLog("i = " + i + " MeshGO = " + meshName + ", localRotation: " + meshGO.transform.localEulerAngles);
			PrintDebugLog("i = " + i + " MeshGO = " + meshName + ", localScale: " + meshGO.transform.localScale);

			var meshfilter = meshGO.GetComponent<MeshFilter>();
			updateMesh.Clear();
			updateMesh.vertices = modelResource.SectionInfo[i]._vectice;
			updateMesh.uv = modelResource.SectionInfo[i]._uv;
			updateMesh.uv2 = modelResource.SectionInfo[i]._uv;
			updateMesh.colors32 = colors.ToArray();
			updateMesh.normals = modelResource.SectionInfo[i]._normal;
			updateMesh.SetIndices(modelResource.SectionInfo[i]._indice, MeshTopology.Triangles, 0);
			updateMesh.name = meshName;
			if (meshfilter != null)
			{
				meshfilter.mesh = updateMesh;
			}
			var meshRenderer = meshGO.GetComponent<MeshRenderer>();
			if (meshRenderer != null)
			{
				if (ImgMaterial == null)
				{
					PrintDebugLog("ImgMaterial is null");
				}
				meshRenderer.material = ImgMaterial;
				meshRenderer.material.mainTexture = modelResource.modelTexture;
				meshRenderer.enabled = true;
			}

			if (meshName.Equals(emitterMeshName))
			{
				PrintDebugLog(meshName + " is found, set " + meshName + " active: true");
				meshGO.SetActive(true);
			}
			else if (meshName.Equals("__CM__Battery"))
			{
				isBatteryIndicatorReady = false;
				if (modelResource.isBatterySetting)
				{
					if (modelResource.batteryTextureList != null)
					{
						batteryMR = meshGO.GetComponent<MeshRenderer>();
						var mat = Resources.Load("TransparentMat") as Material;
						if (mat != null)
						{
							batteryMR.material = mat;
						}

						batteryMR.material.mainTexture = modelResource.batteryTextureList[0].batteryTexture;
						batteryMR.enabled = true;
						isBatteryIndicatorReady = true;
					}
				}
				meshGO.SetActive(false);
				PrintDebugLog(meshName + " is found, set " + meshName + " active: false (waiting for update");
				batteryGO = meshGO;
			}
			else if (meshName == "__CM__TouchPad_Touch")
			{
				PrintDebugLog(meshName + " is found, set " + meshName + " active: false");
				meshGO.SetActive(false);
			}
			else
			{
				PrintDebugLog("set " + meshName + " active: " + modelResource.SectionInfo[i]._active);
				meshGO.SetActive(modelResource.SectionInfo[i]._active);
			}

			yield return wfef;
		}
		PrintDebugLog("send " + deviceType + " ADAPTIVE_CONTROLLER_READY ");
		WaveVR_Utils.Event.Send(WaveVR_Utils.Event.ADAPTIVE_CONTROLLER_READY, deviceType);

		Resources.UnloadUnusedAssets();
		mLoadingState = LoadingState.LoadingState_LOADED;
	}

	void updateBatteryLevel()
	{
		if (batteryGO != null)
		{
			if (showBatterIndicator && isBatteryIndicatorReady)
			{
				if ((modelResource == null) || (modelResource.batteryTextureList == null))
					return;
				bool found = false;

				WaveVR.Device _device = WaveVR.Instance.getDeviceByType(this.deviceType);

				float batteryP = Interop.WVR_GetDeviceBatteryPercentage(_device.type);

				if (batteryP < 0)
				{
					PrintDebugLog("updateBatteryLevel BatteryPercentage is negative, return");
					batteryGO.SetActive(false);
					return;
				}

				foreach (BatteryIndicator bi in modelResource.batteryTextureList)
				{
					if (batteryP >= bi.min/100 && batteryP <= bi.max/100)
					{
						currentBattery = bi;
						found = true;
						break;
					}
				}

				if (found)
				{
					if (batteryMR != null)
					{
						batteryMR.material.mainTexture = currentBattery.batteryTexture;
						PrintDebugLog("updateBatteryLevel battery level to " + currentBattery.level + ", battery percent: " + batteryP);
						batteryGO.SetActive(true);
					}
					else
					{
						PrintDebugLog("updateBatteryLevel Can't get battery mesh renderer");
						batteryGO.SetActive(false);
					}
				} else
				{
					batteryGO.SetActive(false);
				}
			} else
			{
				batteryGO.SetActive(false);
			}
		}
	}

	IEnumerator checkRenderModelAndDelete()
	{
		while (true)
		{
			DeleteControllerWhenDisconnect();
			yield return wfs;
		}
	}

	private void deleteChild(string reason)
	{
		PrintInfoLog(reason);
		var ch = transform.childCount;

		for (int i = 0; i < ch; i++)
		{
			PrintInfoLog("deleteChild: " + transform.GetChild(i).gameObject.name);

			GameObject CM = transform.GetChild(i).gameObject;

			Destroy(CM);
		}
		mLoadingState = LoadingState.LoadingState_NOT_LOADED;
	}

	private void DeleteControllerWhenDisconnect()
	{
		if (mLoadingState != LoadingState.LoadingState_LOADED)
			return ;

		bool _connected = WaveVR_Controller.Input(this.deviceType).connected;

		if (_connected)
		{
			WVR_DeviceType type = WaveVR_Controller.Input(this.deviceType).DeviceType;
			string parameterName = "GetRenderModelName";
			ptrParameterName = Marshal.StringToHGlobalAnsi(parameterName);
			ptrResult = Marshal.AllocHGlobal(64);
			uint resultVertLength = 64;

			uint ret = Interop.WVR_GetParameters(type, ptrParameterName, ptrResult, resultVertLength);
			string tmprenderModelName = Marshal.PtrToStringAnsi(ptrResult);

			Marshal.FreeHGlobal(ptrParameterName);
			Marshal.FreeHGlobal(ptrResult);

			if ((ret > 0) && (tmprenderModelName != renderModelName))
			{
				deleteChild("Destroy controller prefeb because render model is different");
			}
		}
		else
		{
			deleteChild("Destroy controller prefeb because it is disconnect");
		}
		return ;
	}

	private bool checkConnection()
	{
		WaveVR.Device _device = WaveVR.Instance.getDeviceByType(this.deviceType);
		return _device.connected;
	}
}
