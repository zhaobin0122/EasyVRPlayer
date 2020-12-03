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

[System.Serializable]
public class MeshObject
{
	public string MeshName;
	public bool hasEffect;
	public GameObject gameObject;
	public Vector3 originPosition;
	public Material originMat;
	public Material effectMat;
}

public class WaveVR_AdaptiveControllerActions : MonoBehaviour {
	private static string LOG_TAG = "WaveVR_AdaptiveControllerActions";
	public bool enableButtonEffect = true;
	public WaveVR_Controller.EDeviceType device = WaveVR_Controller.EDeviceType.Dominant;
	public bool useSystemConfig = true;
	public Color buttonEffectColor = new Color(0, 179, 227, 255);
	public bool collectInStart = true;
	private int volume_index;

	private void PrintDebugLog(string msg)
	{
		Log.d(LOG_TAG, "device: " + device + ", " + msg);
	}

	private void PrintInfoLog(string msg)
	{
		Log.i(LOG_TAG, "device: " + device + ", " + msg);
	}

	public class WVR_InputObject
	{
		public WVR_InputId destination;
		public WVR_InputId sourceId;
	}

	private enum keyMappingInputType
	{
		TouchDown,
		TouchUp,
		PressDown,
		PressUp
	};

	private static readonly WVR_InputId[] pressIds = new WVR_InputId[] {
			WVR_InputId.WVR_InputId_Alias1_System,
			WVR_InputId.WVR_InputId_Alias1_Menu,
			WVR_InputId.WVR_InputId_Alias1_Grip,
			WVR_InputId.WVR_InputId_Alias1_DPad_Left,
			WVR_InputId.WVR_InputId_Alias1_DPad_Up,
			WVR_InputId.WVR_InputId_Alias1_DPad_Right,
			WVR_InputId.WVR_InputId_Alias1_DPad_Down,
			WVR_InputId.WVR_InputId_Alias1_Volume_Up,
			WVR_InputId.WVR_InputId_Alias1_Volume_Down,
			WVR_InputId.WVR_InputId_Alias1_Digital_Trigger,
			WVR_InputId.WVR_InputId_Alias1_Touchpad,
			WVR_InputId.WVR_InputId_Alias1_Trigger,
			WVR_InputId.WVR_InputId_Alias1_Volume_Up,
			WVR_InputId.WVR_InputId_Alias1_Volume_Down,
	};

	private static readonly string[] PressEffectNames = new string[] {
		"__CM__HomeButton", // WVR_InputId_Alias1_System
		"__CM__AppButton", // WVR_InputId_Alias1_Menu
		"__CM__Grip", // WVR_InputId_Alias1_Grip
		"__CM__DPad_Left", // DPad_Left
		"__CM__DPad_Up", // DPad_Up
		"__CM__DPad_Right", // DPad_Right
		"__CM__DPad_Down", // DPad_Down
		"__CM__VolumeUp", // VolumeUpKey
		"__CM__VolumeDown", // VolumeDownKey
		"__CM__DigitalTriggerKey", // DigitalTriggerKey
		"__CM__TouchPad", // TouchPad_Press
		"__CM__TriggerKey", // TriggerKey
		"__CM__VolumeKey", // Volume
		"__CM__VolumeKey", // Volume
	};

	//private WVR_InputObject[] PressInputObjectArr = new WVR_InputObject[pressIds.Length];
	private MeshObject[] pressObjectArrays = new MeshObject[pressIds.Length];

	private static readonly WVR_InputId[] touchIds = new WVR_InputId[] {
			WVR_InputId.WVR_InputId_Alias1_Touchpad
	};

	private static readonly string[] TouchEffectNames = new string[] {
		"__CM__TouchPad_Touch" // TouchPad_Touch
	};

	//private WVR_InputObject[] TouchInputObjectArr = new WVR_InputObject[touchIds.Length];
	private MeshObject[] touchObjectArrays = new MeshObject[touchIds.Length];

	private GameObject touchpad = null;
	private Mesh touchpadMesh = null;
	private Mesh toucheffectMesh = null;
	private bool currentIsLeftHandMode = false;

	void onAdaptiveControllerModelReady(params object[] args)
	{
		WaveVR_Controller.EDeviceType device = (WaveVR_Controller.EDeviceType)args[0];

		if (device == this.device)
			CollectEffectObjects();
	}

	void OnEnable()
	{
		if (device == WaveVR_Controller.EDeviceType.Dominant)
		{
			modelSpecify = ModelSpecify.MS_Dominant;
		}
		else
		{
			modelSpecify = ModelSpecify.MS_NonDominant;
		}
		resetButtonState();
		WaveVR_Utils.Event.Listen(WaveVR_Utils.Event.ADAPTIVE_CONTROLLER_READY, onAdaptiveControllerModelReady);

}

	void OnDisable()
	{
		WaveVR_Utils.Event.Remove(WaveVR_Utils.Event.ADAPTIVE_CONTROLLER_READY, onAdaptiveControllerModelReady);
	}

	void OnApplicationPause(bool pauseStatus)
	{
		if (!pauseStatus) // resume
		{
			PrintInfoLog("Pause(" + pauseStatus + ") and reset button state");
			resetButtonState();
		}
	}

	void resetButtonState()
	{
		PrintDebugLog("reset button state");
		if (!enableButtonEffect)
		{
			PrintInfoLog("enable button effect : false");
			return;
		}

		for (int i=0; i < pressObjectArrays.Length; i++)
		{
			if (pressObjectArrays[i] == null) continue;
			if (pressObjectArrays[i].hasEffect)
			{
				if (pressObjectArrays[i].gameObject != null && pressObjectArrays[i].originMat != null && pressObjectArrays[i].effectMat != null)
				{
					pressObjectArrays[i].gameObject.GetComponent<MeshRenderer>().material = pressObjectArrays[i].originMat;
					if (mergeToOneBone) pressObjectArrays[i].gameObject.SetActive(false);
				}
			}
		}

		for (int i = 0; i < touchObjectArrays.Length; i++)
		{
			if (touchObjectArrays[i] == null) continue;
			if (touchObjectArrays[i].hasEffect)
			{
				if (touchObjectArrays[i].gameObject != null && touchObjectArrays[i].originMat != null && touchObjectArrays[i].effectMat != null)
				{
					touchObjectArrays[i].gameObject.GetComponent<MeshRenderer>().material = touchObjectArrays[i].originMat;
					touchObjectArrays[i].gameObject.SetActive(false);
				}
			}
		}
	}

	// Use this for initialization
	void Start () {
		resetButtonState();
		if (collectInStart) CollectEffectObjects();
	}

	// Update is called once per frame
	int touch_index = -1;
	void Update () {
		if (!enableButtonEffect)
			return;

		if (currentIsLeftHandMode != WaveVR_Controller.IsLeftHanded)
		{
			currentIsLeftHandMode = WaveVR_Controller.IsLeftHanded;
			PrintInfoLog("Controller role is changed to " + (currentIsLeftHandMode ? "Left" : "Right") + " and reset button state");
			resetButtonState();
		}

		for (int i=0; i<pressIds.Length; i++)
		{
			if (pressObjectArrays[i] == null) continue;
			if (WaveVR_Controller.Input(device).GetPressDown(pressIds[i]))
			{
				int _i = GetPressInputMapping(i, keyMappingInputType.PressDown);
				if (_i == -1) continue;
				if (pressObjectArrays[_i].hasEffect)
				{
					if (pressObjectArrays[_i].gameObject != null && pressObjectArrays[_i].originMat != null && pressObjectArrays[_i].effectMat != null)
					{
						pressObjectArrays[_i].gameObject.GetComponent<MeshRenderer>().material = pressObjectArrays[_i].effectMat;
						if (mergeToOneBone) pressObjectArrays[_i].gameObject.SetActive(true);
					}
				} else
				{
					PrintInfoLog(pressIds[_i] + " doesn't have effect");
				}
			}

			if (WaveVR_Controller.Input(device).GetPressUp(pressIds[i]))
			{
				int _i = GetPressInputMapping(i, keyMappingInputType.PressUp);
				if (_i == -1) continue;
				if (pressObjectArrays[_i].hasEffect)
				{
					if (pressObjectArrays[_i].gameObject != null && pressObjectArrays[_i].originMat != null && pressObjectArrays[_i].effectMat != null)
					{
						pressObjectArrays[_i].gameObject.GetComponent<MeshRenderer>().material = pressObjectArrays[_i].originMat;
						if (mergeToOneBone) pressObjectArrays[_i].gameObject.SetActive(false);
					}
				} else
				{
					PrintInfoLog(pressIds[_i] + " doesn't have effect");
				}
			}
		}

		for (int i = 0; i < touchIds.Length; i++)
		{
			if (touchObjectArrays[i] == null) continue;
			if (WaveVR_Controller.Input(device).GetTouchDown(touchIds[i]))
			{
				int _i = GetTouchInputMapping(i, keyMappingInputType.TouchDown);
				touch_index = _i;
				if (_i == -1) continue;
				if (touchObjectArrays[_i].hasEffect)
				{
					if (touchObjectArrays[_i].gameObject != null && touchObjectArrays[_i].originMat != null && touchObjectArrays[_i].effectMat != null)
					{
						touchObjectArrays[_i].gameObject.GetComponent<MeshRenderer>().material = touchObjectArrays[_i].effectMat;
						touchObjectArrays[_i].gameObject.SetActive(true);
					}
				} else
				{
					PrintInfoLog(touchIds[_i] + " doesn't have effect");
				}
			}

			if (WaveVR_Controller.Input(device).GetTouch(touchIds[i]))
			{
				int _i = touch_index;
				if (_i == -1)  continue;

				if (touchObjectArrays[_i].hasEffect && touchObjectArrays[_i].MeshName == "__CM__TouchPad_Touch")
				{
					if (touchObjectArrays[_i].gameObject != null && touchObjectArrays[_i].originMat != null && touchObjectArrays[_i].effectMat != null)
					{
						var axis = WaveVR_Controller.Input(this.device).GetAxis(WVR_InputId.WVR_InputId_Alias1_Touchpad);

						if (isTouchPadSetting)
						{
							float xangle = touchCenter.x / 100 + (axis.x * raidus * touchPtU.x) / 100 + (axis.y * raidus * touchPtW.x) / 100 + (touchptHeight * touchPtV.x) / 100;
							float yangle = touchCenter.y / 100 + (axis.x * raidus * touchPtU.y) / 100 + (axis.y * raidus * touchPtW.y) / 100 + (touchptHeight * touchPtV.y) / 100;
							float zangle = touchCenter.z / 100 + (axis.x * raidus * touchPtU.z) / 100 + (axis.y * raidus * touchPtW.z) / 100 + (touchptHeight * touchPtV.z) / 100;

							// touchAxis
							if (Log.gpl.Print)
								Log.d(LOG_TAG, "device: " + device + ", " + "Touchpad axis x: " + axis.x + " axis.y: " + axis.y + ", xangle: " + xangle + ", yangle: " + yangle + ", zangle: " + zangle);

							Vector3 touchPos = transform.TransformPoint(xangle, yangle, zangle);

							touchObjectArrays[_i].gameObject.transform.position = touchPos;

						}
						else
						{
							float xangle = axis.x * (touchpadMesh.bounds.size.x * touchpad.transform.localScale.x - toucheffectMesh.bounds.size.x * touchObjectArrays[_i].gameObject.transform.localScale.x) / 2;
							float yangle = axis.y * (touchpadMesh.bounds.size.z * touchpad.transform.localScale.z - toucheffectMesh.bounds.size.z * touchObjectArrays[_i].gameObject.transform.localScale.z) / 2;

							var height = touchpadMesh.bounds.size.y * touchpad.transform.localScale.y;

							var h = Mathf.Abs(touchpadMesh.bounds.max.y);
							if (Log.gpl.Print)
							{

								Log.d(LOG_TAG, "device: " + device + ", " + "Touchpad axis x: " + axis.x + " axis.y: " + axis.y + ", xangle: " + xangle + ", yangle: " + yangle + ", height: " + height + ",h: " + h);

#if DEBUG
								Log.d(LOG_TAG, "device: " + device + ", " + "TouchEffectMesh.bounds.size: " + toucheffectMesh.bounds.size.x + ", " + toucheffectMesh.bounds.size.y + ", " + toucheffectMesh.bounds.size.z);
								Log.d(LOG_TAG, "device: " + device + ", " + "TouchEffectMesh.scale: " + touchObjectArrays[_i].gameObject.transform.localScale.x + ", " + touchObjectArrays[_i].gameObject.transform.localScale.y + ", " + touchObjectArrays[_i].gameObject.transform.localScale.z);
								Log.d(LOG_TAG, "device: " + device + ", " + "TouchpadMesh.bounds.size: " + touchpadMesh.bounds.size.x + ", " + touchpadMesh.bounds.size.y + ", " + touchpadMesh.bounds.size.z);
								Log.d(LOG_TAG, "device: " + device + ", " + "TouchpadMesh. scale: " + touchObjectArrays[_i].gameObject.transform.localScale.x + ", " + touchObjectArrays[_i].gameObject.transform.localScale.y + ", " + touchObjectArrays[_i].gameObject.transform.localScale.z);
								Log.d(LOG_TAG, "device: " + device + ", " + "TouchEffect.originPosition: " + touchObjectArrays[_i].originPosition.x + ", " + touchObjectArrays[_i].originPosition.y + ", " + touchObjectArrays[_i].originPosition.z);
#endif
							}
							Vector3 translateVec = Vector3.zero;
							translateVec = new Vector3(xangle, h, yangle);
							touchObjectArrays[_i].gameObject.transform.localPosition = touchObjectArrays[_i].originPosition + translateVec;
						}
					}
				}
			}

			if (WaveVR_Controller.Input(device).GetTouchUp(touchIds[i]))
			{
				int _i = GetTouchInputMapping(i, keyMappingInputType.TouchUp);
				if (_i == -1)  continue;

				if (touchObjectArrays[_i].hasEffect)
				{
					//int _i = GetTouchInputMapping(i, "touch up");
					if (touchObjectArrays[_i].gameObject != null && touchObjectArrays[_i].originMat != null && touchObjectArrays[_i].effectMat != null)
					{
						touchObjectArrays[_i].gameObject.GetComponent<MeshRenderer>().material = touchObjectArrays[_i].originMat;
						touchObjectArrays[_i].gameObject.SetActive(false);
					}
				} else
				{
					PrintInfoLog(touchIds[_i] + " doesn't have effect");
				}
			}
		}
	}

	private Material effectMat;
	private Material touchMat;
	private bool mergeToOneBone = false;
	private bool isTouchPadSetting = false;
	private Vector3 touchCenter = new Vector3(0, 0, 0);
	private float raidus;
	private Vector3 touchPtW; //W is direction of the +y analog.
	private Vector3 touchPtU; //U is direction of the +x analog.
	private Vector3 touchPtV; //V is normal of moving plane of touchpad.
	private float touchptHeight = 0.0f;
	private ModelSpecify modelSpecify;

	private bool GetTouchPadParam()
	{
		WVR_DeviceType type = WaveVR_Controller.Input(this.device).DeviceType;
		bool _connected = WaveVR_Controller.Input(this.device).connected;
		if (!_connected)
		{
			PrintDebugLog("Device is disconnect: ");
			return false;
		}

		string parameterName = "GetRenderModelName";
		IntPtr ptrParameterName = Marshal.StringToHGlobalAnsi(parameterName);
		IntPtr ptrResult = Marshal.AllocHGlobal(64);
		uint resultVertLength = 64;

		uint ret = Interop.WVR_GetParameters(type, ptrParameterName, ptrResult, resultVertLength);
		string renderModelName = Marshal.PtrToStringAnsi(ptrResult);

		Marshal.FreeHGlobal(ptrParameterName);
		Marshal.FreeHGlobal(ptrResult);

		if (ret == 0)
		{
			PrintDebugLog("Get render model name fail!");
			return false;
		}

		PrintDebugLog("current render model name: " + renderModelName);

		ModelResource modelResource = WaveVR_ControllerResourceHolder.Instance.getRenderModelResource(renderModelName, modelSpecify, mergeToOneBone);

		if ((modelResource == null) && (modelSpecify == ModelSpecify.MS_NonDominant))
		{
			modelResource = WaveVR_ControllerResourceHolder.Instance.getRenderModelResource(renderModelName, ModelSpecify.MS_Dominant, mergeToOneBone);
		}

		if ((modelResource == null) || (modelResource.TouchSetting == null))
		{
			PrintDebugLog("Get render model resource fail!");
			return false;
		}

		touchCenter = modelResource.TouchSetting.touchCenter;
		touchPtW = modelResource.TouchSetting.touchPtW;
		touchPtU = modelResource.TouchSetting.touchPtU;
		touchPtV = modelResource.TouchSetting.touchPtV;
		raidus = modelResource.TouchSetting.raidus;
		touchptHeight = modelResource.TouchSetting.touchptHeight;

		PrintDebugLog("touchCenter! x: " + touchCenter.x + " ,y: " + touchCenter.y + " ,z: " + touchCenter.z);
		PrintDebugLog("touchPtW! x: " + touchPtW.x + " ,y: " + touchPtW.y + " ,z: " + touchPtW.z);
		PrintDebugLog("touchPtU! x: " + touchPtU.x + " ,y: " + touchPtU.y + " ,z: " + touchPtU.z);
		PrintDebugLog("touchPtV! x: " + touchPtV.x + " ,y: " + touchPtV.y + " ,z: " + touchPtV.z);
		PrintDebugLog("raidus: " + raidus);
		PrintDebugLog("Floating distance : " + touchptHeight);

		return true;
	}

	private int GetPressInputMapping(int pressIds_Index, keyMappingInputType status)
	{
		WVR_InputId _btn = pressIds[pressIds_Index];
		bool _result = WaveVR_ButtonList.Instance.GetInputMappingPair(this.device, ref _btn);

		if (!_result)
		{
			PrintInfoLog("GetInputMappingPair failed.");
			return -1;
		}

		int _index = -1;
		for (int i = 0; i < pressIds.Length; i++)
		{
			if(_btn == pressIds[i])
			{
				_index = i;
				break;
			}
		}

		if (pressObjectArrays[pressIds_Index].hasEffect && pressObjectArrays[pressIds_Index].MeshName == "__CM__VolumeKey")
		{
			_index = volume_index;
		}

		if (_index >= 0 && _index < pressIds.Length)
		{
			PrintInfoLog(status.ToString() + " button: " + pressIds[pressIds_Index] + " is mapped to " + _btn);
		} else
		{
			PrintInfoLog("Can't get index in touchIds.");
		}

		return _index;
	}

	private int GetTouchInputMapping(int touchIds_Index, keyMappingInputType status)
	{
		WVR_InputId _btn = touchIds[touchIds_Index];
		bool _result = WaveVR_ButtonList.Instance.GetInputMappingPair(this.device, ref _btn);
		if (!_result)
		{
			PrintInfoLog("GetInputMappingPair failed.");
			return -1;
		}

		int _index = -1;
		for (int i = 0; i < touchIds.Length; i++)
		{
			if (_btn == touchIds[i])
			{
				_index = i;
				break;
			}
		}

		if (_index >= 0 && _index < touchIds.Length)
		{
			PrintInfoLog(status.ToString() + " button: " + touchIds[touchIds_Index] + " is mapped to " + _btn);
		} else
		{
			PrintInfoLog("Can't get index in touchIds.");
		}

		return _index;
	}

	private void CollectEffectObjects() // collect controller object which has effect
	{
		effectMat = Resources.Load("ColorOffsetMaterial") as Material;
		touchMat = new Material(Shader.Find("Unlit/Texture"));
		if (useSystemConfig)
		{
			PrintInfoLog("use system config in controller model!");
			ReadJsonValues();
		}
		else
		{
			Log.w(LOG_TAG, "use custom config in controller model!");
		}

		var ch = this.transform.childCount;
		PrintDebugLog("childCount: " + ch);
		effectMat.color = buttonEffectColor;

		WaveVR_RenderModel wrm = this.GetComponent<WaveVR_RenderModel>();

		if (wrm != null)
		{
			mergeToOneBone = wrm.mergeToOneBone;
		}

		isTouchPadSetting = GetTouchPadParam();

		for (var j = 0; j < PressEffectNames.Length; j++)
		{
			pressObjectArrays[j] = new MeshObject();
			pressObjectArrays[j].MeshName = PressEffectNames[j];
			pressObjectArrays[j].hasEffect = false;
			pressObjectArrays[j].gameObject = null;
			pressObjectArrays[j].originPosition = new Vector3(0, 0, 0);
			pressObjectArrays[j].originMat = null;
			pressObjectArrays[j].effectMat = null;

			bool found = false;
			for (int i = 0; i < ch; i++)
			{
				GameObject CM = this.transform.GetChild(i).gameObject;
				string[] t = CM.name.Split("."[0]);
				var childname = t[0];
				if (pressObjectArrays[j].MeshName == childname)
				{
					if (childname == "__CM__VolumeKey" || childname == "__CM__VolumeUp" || childname == "__CM__VolumeDown")
					{
						volume_index = j;
					}
					PrintInfoLog(childname + " is found, active = " + CM.activeInHierarchy);
					pressObjectArrays[j].gameObject = CM;
					pressObjectArrays[j].originPosition = CM.transform.localPosition;
					pressObjectArrays[j].originMat = CM.GetComponent<MeshRenderer>().material;
					pressObjectArrays[j].effectMat = effectMat;
					pressObjectArrays[j].hasEffect = true;

					if (childname == "__CM__TouchPad")
					{
						touchpad = pressObjectArrays[j].gameObject;
						touchpadMesh = touchpad.GetComponent<MeshFilter>().mesh;
						if (touchpadMesh != null)
						{
							PrintInfoLog("touchpad is found! ");
						}
					}
					found = true;
					break;
				}
			}

			if (!found)
			{
				PrintInfoLog(pressObjectArrays[j].MeshName + " is not found");
			}
		}

		for (var j = 0; j < TouchEffectNames.Length; j++)
		{
			touchObjectArrays[j] = new MeshObject();
			touchObjectArrays[j].MeshName = TouchEffectNames[j];
			touchObjectArrays[j].hasEffect = false;
			touchObjectArrays[j].gameObject = null;
			touchObjectArrays[j].originPosition = new Vector3(0f, 0f, 0f);
			touchObjectArrays[j].originMat = null;
			touchObjectArrays[j].effectMat = null;

			bool found = false;
			for (int i = 0; i < ch; i++)
			{
				GameObject CM = this.transform.GetChild(i).gameObject;
				string[] t = CM.name.Split("."[0]);
				var childname = t[0];

				if (touchObjectArrays[j].MeshName == childname)
				{
					PrintInfoLog(childname + " is found, active = " + CM.activeInHierarchy);
					touchObjectArrays[j].gameObject = CM;
					touchObjectArrays[j].originPosition = CM.transform.localPosition;
					touchObjectArrays[j].originMat = CM.GetComponent<MeshRenderer>().material;
					touchObjectArrays[j].effectMat = effectMat;
					touchObjectArrays[j].hasEffect = true;

					if (childname == "__CM__TouchPad_Touch")
					{
						toucheffectMesh = touchObjectArrays[j].gameObject.GetComponent<MeshFilter>().mesh;
						if (toucheffectMesh != null)
						{
							PrintInfoLog("toucheffectMesh is found! ");
						}
					}
					found = true;
					break;
				}
			}

			if (!found)
			{
				PrintInfoLog(touchObjectArrays[j].MeshName + " is not found");
			}
		}

		resetButtonState();
	}

	private Color StringToColor(string color_string)
	{
		float _color_r = (float)Convert.ToInt32(color_string.Substring(1, 2), 16);
		float _color_g = (float)Convert.ToInt32(color_string.Substring(3, 2), 16);
		float _color_b = (float)Convert.ToInt32(color_string.Substring(5, 2), 16);
		float _color_a = (float)Convert.ToInt32(color_string.Substring(7, 2), 16);

		return new Color(_color_r, _color_g, _color_b, _color_a);
	}

	private Texture2D GetTexture2D(string texture_path)
	{
		if (System.IO.File.Exists(texture_path))
		{
			var _bytes = System.IO.File.ReadAllBytes(texture_path);
			var _texture = new Texture2D(1, 1);
			_texture.LoadImage(_bytes);
			return _texture;
		}
		return null;
	}

	public void Circle(Texture2D tex, int cx, int cy, int r, Color col)
	{
		int x, y, px, nx, py, ny, d;

		for (x = 0; x <= r; x++)
		{
			d = (int)Mathf.Ceil(Mathf.Sqrt(r * r - x * x));
			for (y = 0; y <= d; y++)
			{
				px = cx + x;
				nx = cx - x;
				py = cy + y;
				ny = cy - y;

				tex.SetPixel(px, py, col);
				tex.SetPixel(nx, py, col);

				tex.SetPixel(px, ny, col);
				tex.SetPixel(nx, ny, col);

			}
		}
		tex.Apply();
	}

	private void ReadJsonValues()
	{
		string json_values = WaveVR_Utils.OEMConfig.getControllerConfig();
		if (!json_values.Equals(""))
		{
			SimpleJSON.JSONNode jsNodes = SimpleJSON.JSONNode.Parse(json_values);
			string node_value = "";

			node_value = jsNodes["model"]["touchpad_dot_use_texture"].Value;
			if (node_value.ToLower().Equals("false"))
			{
				PrintDebugLog("touchpad_dot_use_texture = false, create texture");

				// effect color.
				node_value = jsNodes["model"]["touchpad_dot_color"].Value;
				if (!node_value.Equals(""))
				{
					PrintInfoLog("touchpad_dot_color: " + node_value);
					buttonEffectColor = StringToColor(node_value);

					var texture = new Texture2D(256, 256, TextureFormat.ARGB32, false);
					Color o = Color.clear;
					o.r = 1f;
					o.g = 1f;
					o.b = 1f;
					o.a = 0f;
					for (int i = 0; i < 256; i++)
					{
						for (int j = 0; j < 256; j++)
						{
							texture.SetPixel(i, j, o);
						}
					}
					texture.Apply();

					Circle(texture, 128, 128, 100, buttonEffectColor);

					touchMat.mainTexture = texture;
				}
			}
			else
			{
				PrintDebugLog("touchpad_dot_use_texture = true");
				node_value = jsNodes["model"]["touchpad_dot_texture_name"].Value;
				if (!node_value.Equals(""))
				{
					if (System.IO.File.Exists(node_value))
					{
						var _bytes = System.IO.File.ReadAllBytes(node_value);
						var _texture = new Texture2D(1, 1);
						_texture.LoadImage(_bytes);

						PrintInfoLog("touchpad_dot_texture_name: " + node_value);
						touchMat.mainTexture = _texture;
						touchMat.color = buttonEffectColor;
					}
				}
			}
		}
	}
}
