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

[System.Serializable]
public class BatteryPercentage
{
	public float minBatteryPercentage;
	public float maxBatteryPercentage;
	public Texture texture = null;
}

public class UniversalControllerActions : MonoBehaviour {
	private static string LOG_TAG = "UniversalControllerActions";

	public enum AxisMapping
	{
		Y_Axis,
		Z_Axis
	}

	public WVR_DeviceType device = WVR_DeviceType.WVR_DeviceType_Controller_Right;
	private void PrintDebugLog(string msg)
	{
		Log.d (LOG_TAG, "device: " + device + ", " + msg, true);
	}
	public bool useSystemConfig = true;
	public GameObject TouchPad = null;
	public GameObject Touch_Dot = null;
	public AxisMapping touch_YAxis_mapping = AxisMapping.Z_Axis;
	public GameObject Touch_Press = null;
	public GameObject Trigger_Press = null;
	public GameObject VolumeUp_Press = null;
	public GameObject VolumeDown_Press = null;
	public GameObject Grip_Press = null;
	public GameObject DigitalTrigger_Press = null;
	public GameObject Menu_Press = null;
	public GameObject Home_Press = null;
	public GameObject Battery_Change = null;
	public List<BatteryPercentage> batteryPercentages = new List<BatteryPercentage>();

	private Vector3 originPosition;
	private MeshRenderer batteryMeshRenderer = null;
	//private bool getValidBattery = false;
	private Mesh toucheffectMesh = null;
	private Mesh touchpadMesh = null;
	private bool isTouchPressed = false;
	private Color materialColor = new Color(0, 179, 227, 255); // #00B3E3FF
	private bool currentIsLeftHandMode = false;
	private int batteryLevels = 0;

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
			try {
			SimpleJSON.JSONNode jsNodes = SimpleJSON.JSONNode.Parse(json_values);
			string node_value = "";

			node_value = jsNodes["model"]["touchpad_dot_use_texture"].Value;
			if (node_value.ToLower().Equals("false"))
			{
				Log.d(LOG_TAG, "touchpad_dot_use_texture = false, create texture");
				if (Touch_Dot != null)
				{
					// Material of touchpad dot.
					node_value = jsNodes["model"]["touchpad_dot_color"].Value;
					if (!node_value.Equals(""))
					{
						Log.d(LOG_TAG, "touchpad_dot_color: " + node_value);
						materialColor = StringToColor(node_value);

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

						Circle(texture, 128, 128, 100, materialColor);
						MeshRenderer mshr = Touch_Dot.GetComponent<MeshRenderer>();
						//  mshr.
						mshr.material.mainTexture = texture;
					}
				}
			}
			else
			{
				Log.d(LOG_TAG, "touchpad_dot_use_texture = true");
				node_value = jsNodes["model"]["touchpad_dot_texture_name"].Value;
				if (!node_value.Equals(""))
				{
					if (System.IO.File.Exists(node_value))
					{
						var _bytes = System.IO.File.ReadAllBytes(node_value);
						var _texture = new Texture2D(1, 1);
						_texture.LoadImage(_bytes);

						if (Touch_Dot != null)
						{
							Log.d(LOG_TAG, "touchpad_dot_texture_name: " + node_value);
							MeshRenderer _mrdr = Touch_Dot.GetComponentInChildren<MeshRenderer>();
							Material _mat = _mrdr.materials[0];
							_mat.mainTexture = _texture;
							_mat.color = materialColor;
						}
					}
				}
			}

			// Battery
			node_value = jsNodes["battery"]["battery_level_count"].Value;
			if (!node_value.Equals(""))
			{
				batteryLevels = Convert.ToInt32(node_value, 10);

				if (batteryLevels > 0)
				{
					bool updateBatteryTextures = true;
					string texName = "";
					string minPercentStr = "";
					string maxPercentStr = "";
					for (int i = 0; i < batteryLevels; i++)
					{
						texName = jsNodes["battery"]["battery_levels"][i]["level_texture_name"].Value;
						minPercentStr = jsNodes["battery"]["battery_levels"][i]["level_min_value"].Value;
						maxPercentStr = jsNodes["battery"]["battery_levels"][i]["level_max_value"].Value;
						if (!texName.Equals("") && !minPercentStr.Equals("") && !maxPercentStr.Equals(""))
						{
							Texture _tex = GetTexture2D(texName);
							if (_tex == null)
							{
								updateBatteryTextures = false;
								break;
							}
						}
						else
						{
							updateBatteryTextures = false;
							break;
						}
					}

					if (updateBatteryTextures) {
						Log.d(LOG_TAG, "updateBatteryTextures, battery_level_count: " + batteryLevels);
						batteryPercentages.Clear();

						for (int i = 0; i < batteryLevels; i++)
						{
							texName = jsNodes["battery"]["battery_levels"][i]["level_texture_name"].Value;
							minPercentStr = jsNodes["battery"]["battery_levels"][i]["level_min_value"].Value;
							maxPercentStr = jsNodes["battery"]["battery_levels"][i]["level_max_value"].Value;
							if (!texName.Equals("") && !minPercentStr.Equals("") && !maxPercentStr.Equals(""))
							{
								Texture _tex = GetTexture2D(texName);
								if (_tex != null)
								{
									BatteryPercentage tmpBP = new BatteryPercentage();
									tmpBP.texture = _tex;
									tmpBP.minBatteryPercentage = float.Parse(minPercentStr);
									tmpBP.maxBatteryPercentage = float.Parse(maxPercentStr);

									Log.d(LOG_TAG, "updateBatteryTextures, level: " + i + ", min = " + minPercentStr + ", max = " + maxPercentStr + ", texName = " + texName);
									batteryPercentages.Add(tmpBP);
								}
							}
						}
					}
				}
			}
			}
			catch (Exception e)
			{
				Log.e(LOG_TAG, "JsonParse failed: " + e.ToString());
			}
		}
	}

	void OnEnable()
	{
		if (useSystemConfig)
		{
			Log.d(LOG_TAG, "use system config in controller model!");
			ReadJsonValues();
		}
		else
		{
			Log.w(LOG_TAG, "use custom config in controller model!");
		}
		resetButtonState();
		WaveVR_Utils.Event.Listen(WaveVR_Utils.Event.BATTERY_STATUS_UPDATE, onBatteryStatusUpdate);
	}

	void OnDisable()
	{
		WaveVR_Utils.Event.Remove(WaveVR_Utils.Event.BATTERY_STATUS_UPDATE, onBatteryStatusUpdate);
	}

	void OnApplicationPause(bool pauseStatus)
	{
		if (!pauseStatus) // resume
		{
			Log.d(LOG_TAG, "Pause(" + pauseStatus + ") and reset button state");
			resetButtonState();
		}
	}

	private void onBatteryStatusUpdate(params object[] args)
	{
		Log.d(LOG_TAG, "receive battery status update event");
		if (Battery_Change != null)
		{
			//getValidBattery = updateBatteryInfo();
		}
	}

	private bool updateBatteryInfo()
	{
		if (Application.isEditor)
			return false;

		WVR_DeviceType _type = WaveVR_Controller.Input(this.device).DeviceType;
		float batteryPer = Interop.WVR_GetDeviceBatteryPercentage(_type);
		PrintDebugLog ("updateBatteryInfo() _type: " + _type + ", percentage: " + batteryPer);
		if (batteryPer < 0)
		{
			PrintDebugLog ("updateBatteryInfo() _type: " + _type + " BatteryPercentage is negative, return false");
			return false;
		}
		foreach (BatteryPercentage bp in batteryPercentages)
		{
			if (batteryPer >= bp.minBatteryPercentage && batteryPer <= bp.maxBatteryPercentage)
			{
				batteryMeshRenderer.material.mainTexture = bp.texture;
				Log.d(LOG_TAG, "BatteryPercentage device: " + device + ", between " + bp.minBatteryPercentage  +" and " + bp.maxBatteryPercentage);
				Battery_Change.SetActive(true);
			}
		}

		return true;
	}

	void Start()
	{
		if (TouchPad != null && Touch_Dot != null)
		{
			originPosition = Touch_Dot.transform.localPosition;
			Touch_Dot.SetActive(false);
			toucheffectMesh = Touch_Dot.GetComponent<MeshFilter>().mesh;
			touchpadMesh = TouchPad.GetComponent<MeshFilter>().mesh;
			Log.d(LOG_TAG, "Touch Y-axis mapping to " + touch_YAxis_mapping.ToString());
		}
		if (Battery_Change != null)
		{
			batteryMeshRenderer = Battery_Change.GetComponent<MeshRenderer>();

			Battery_Change.SetActive(false);
		}

		resetButtonState();
	}

	void resetButtonState()
	{
		Log.d(LOG_TAG, "reset button state");
		if (Touch_Dot != null)
		{
			Touch_Dot.SetActive(false);
		}

		if (Grip_Press != null)
		{
			Grip_Press.SetActive(false);
		}

		if (Trigger_Press != null)
		{
			Trigger_Press.SetActive(false);
		}

		if (VolumeUp_Press != null)
		{
			VolumeUp_Press.SetActive(false);
		}

		if (VolumeDown_Press != null)
		{
			VolumeDown_Press.SetActive(false);
		}

		if (Touch_Press != null)
		{
			Touch_Press.SetActive(false);
		}

		if (DigitalTrigger_Press != null)
		{
			DigitalTrigger_Press.SetActive(false);
		}

		if (Menu_Press != null)
		{
			Menu_Press.SetActive(false);
		}

		if (Home_Press != null)
		{
			Home_Press.SetActive(false);
		}
	}

	int t = 0;

	// Update is called once per frame
	void Update()
	{
		if (currentIsLeftHandMode != WaveVR_Controller.IsLeftHanded)
		{
			currentIsLeftHandMode = WaveVR_Controller.IsLeftHanded;
			Log.d(LOG_TAG, "Controller role is changed to " + (currentIsLeftHandMode ? "Left" : "Right"));
			resetButtonState();
		}

		if (Battery_Change != null)
		{
			if (t++ > 150)
			{
				//getValidBattery = updateBatteryInfo();

				t = 0;
			}
		}

		//WVR_InputId_Alias1_Trigger
		if (WaveVR_Controller.Input(this.device).GetPressDown(WVR_InputId.WVR_InputId_Alias1_Trigger))
		{
			Log.d(LOG_TAG, "WVR_InputId_Alias1_Trigger press down");
			if (Trigger_Press != null)
			{
				Trigger_Press.SetActive(true);
			}
		}

		if (WaveVR_Controller.Input(this.device).GetPress(WVR_InputId.WVR_InputId_Alias1_Trigger))
		{
			if (Trigger_Press != null)
			{
				Trigger_Press.SetActive(true);
			}
		}

		if (WaveVR_Controller.Input(this.device).GetPressUp(WVR_InputId.WVR_InputId_Alias1_Trigger))
		{
			Log.d(LOG_TAG, "WVR_InputId_Alias1_Trigger press up");
			if (Trigger_Press != null)
			{
				Trigger_Press.SetActive(false);
			}
		}

		//WVR_InputId_Alias1_Volume_Up
		if (WaveVR_Controller.Input(this.device).GetPressDown(WVR_InputId.WVR_InputId_Alias1_Volume_Up))
		{
			Log.d(LOG_TAG, "WVR_InputId_Alias1_Volume_Up press down");
			if (VolumeUp_Press != null)
			{
				VolumeUp_Press.SetActive(true);
			}
		}

		if (WaveVR_Controller.Input(this.device).GetPress(WVR_InputId.WVR_InputId_Alias1_Volume_Up))
		{
			if (VolumeUp_Press != null)
			{
				VolumeUp_Press.SetActive(true);
			}
		}

		if (WaveVR_Controller.Input(this.device).GetPressUp(WVR_InputId.WVR_InputId_Alias1_Volume_Up))
		{
			Log.d(LOG_TAG, "WVR_InputId_Alias1_Volume_Up press up");
			if (VolumeUp_Press != null)
			{
				VolumeUp_Press.SetActive(false);
			}
		}

		//WVR_InputId_Alias1_Volume_Down
		if (WaveVR_Controller.Input(this.device).GetPressDown(WVR_InputId.WVR_InputId_Alias1_Volume_Down))
		{
			Log.d(LOG_TAG, "WVR_InputId_Alias1_Volume_Down press down");
			if (VolumeDown_Press != null)
			{
				VolumeDown_Press.SetActive(true);
			}
		}

		if (WaveVR_Controller.Input(this.device).GetPress(WVR_InputId.WVR_InputId_Alias1_Volume_Down))
		{
			if (VolumeDown_Press != null)
			{
				VolumeDown_Press.SetActive(true);
			}
		}

		if (WaveVR_Controller.Input(this.device).GetPressUp(WVR_InputId.WVR_InputId_Alias1_Volume_Down))
		{
			Log.d(LOG_TAG, "WVR_InputId_Alias1_Volume_Down press up");
			if (VolumeDown_Press != null)
			{
				VolumeDown_Press.SetActive(false);
			}
		}

		//WVR_InputId_Alias1_Grip
		if (WaveVR_Controller.Input(this.device).GetPressDown(WVR_InputId.WVR_InputId_Alias1_Grip))
		{
			Log.d(LOG_TAG, "WVR_InputId_Alias1_Grip press down");
			if (Grip_Press != null)
			{
				Grip_Press.SetActive(true);
			}
		}

		if (WaveVR_Controller.Input(this.device).GetPress(WVR_InputId.WVR_InputId_Alias1_Grip))
		{
			if (Grip_Press != null)
			{
				Grip_Press.SetActive(true);
			}
		}

		if (WaveVR_Controller.Input(this.device).GetPressUp(WVR_InputId.WVR_InputId_Alias1_Grip))
		{
			Log.d(LOG_TAG, "WVR_InputId_Alias1_Grip press up");
			if (Grip_Press != null)
			{
				Grip_Press.SetActive(false);
			}
		}

		//WVR_InputId_Alias1_Digital_Trigger
		if (WaveVR_Controller.Input(this.device).GetPressDown(WVR_InputId.WVR_InputId_Alias1_Digital_Trigger))
		{
			Log.d(LOG_TAG, "WVR_InputId_Alias1_Digital_Trigger press down");
			if (DigitalTrigger_Press != null)
			{
				DigitalTrigger_Press.SetActive(true);
			}
		}

		if (WaveVR_Controller.Input(this.device).GetPress(WVR_InputId.WVR_InputId_Alias1_Digital_Trigger))
		{
			if (DigitalTrigger_Press != null)
			{
				DigitalTrigger_Press.SetActive(true);
			}
		}

		if (WaveVR_Controller.Input(this.device).GetPressUp(WVR_InputId.WVR_InputId_Alias1_Digital_Trigger))
		{
			Log.d(LOG_TAG, "WVR_InputId_Alias1_Digital_Trigger press up");
			if (DigitalTrigger_Press != null)
			{
				DigitalTrigger_Press.SetActive(false);
			}
		}

		//WVR_InputId_Alias1_Menu
		if (WaveVR_Controller.Input(device).GetPressDown(WVR_InputId.WVR_InputId_Alias1_Menu))
		{
			Log.d(LOG_TAG, "WVR_InputId_Alias1_Menu press down");
			if (Menu_Press != null)
			{
				Menu_Press.SetActive(true);
			}
		}

		if (WaveVR_Controller.Input(device).GetPress(WVR_InputId.WVR_InputId_Alias1_Menu))
		{
			if (Menu_Press != null)
			{
				Menu_Press.SetActive(true);
			}
		}

		if (WaveVR_Controller.Input(device).GetPressUp(WVR_InputId.WVR_InputId_Alias1_Menu))
		{
			Log.d(LOG_TAG, "WVR_InputId_Alias1_Menu press up");
			if (Menu_Press != null)
			{
				Menu_Press.SetActive(false);
			}

		}

		//WVR_InputId_Alias1_System
		if (WaveVR_Controller.Input(this.device).GetPressDown(WVR_InputId.WVR_InputId_Alias1_System))
		{
			Log.d(LOG_TAG, "WVR_InputId_Alias1_System press down");
			if (Home_Press != null)
			{
				Home_Press.SetActive(true);
			}
		}

		if (WaveVR_Controller.Input(this.device).GetPress(WVR_InputId.WVR_InputId_Alias1_System))
		{
			if (Home_Press != null)
			{
				Home_Press.SetActive(true);
			}
		}

		if (WaveVR_Controller.Input(this.device).GetPressUp(WVR_InputId.WVR_InputId_Alias1_System))
		{
			Log.d(LOG_TAG, "WVR_InputId_Alias1_System press up");
			if (Home_Press != null)
			{
				Home_Press.SetActive(false);
			}
		}

		//WVR_InputId_Alias1_Touchpad
		if (WaveVR_Controller.Input(this.device).GetPressDown(WVR_InputId.WVR_InputId_Alias1_Touchpad))
		{
			isTouchPressed = true;
			if (Touch_Press != null)
			{
				Touch_Press.SetActive(true);
			}
		}

		if (WaveVR_Controller.Input(this.device).GetPress(WVR_InputId.WVR_InputId_Alias1_Touchpad))
		{
			if (Touch_Press != null)
			{
				if (Touch_Dot != null)
				{
					Touch_Dot.SetActive(false);
				}
				Touch_Press.SetActive(true);
			}
		}

		if (WaveVR_Controller.Input(this.device).GetPressUp(WVR_InputId.WVR_InputId_Alias1_Touchpad))
		{
			isTouchPressed = false;
			if (Touch_Press != null)
			{
				Touch_Press.SetActive(false);
			}
		}
		// button touch down
		if (WaveVR_Controller.Input(this.device).GetTouchDown(WVR_InputId.WVR_InputId_Alias1_Touchpad))
		{
			if (!isTouchPressed && Touch_Dot != null)
			{
				Touch_Dot.SetActive(true);
			}
		}

		// button touch up
		if (WaveVR_Controller.Input(this.device).GetTouchUp(WVR_InputId.WVR_InputId_Alias1_Touchpad))
		{
			if (Touch_Dot != null)
			{
				Touch_Dot.SetActive(false);
			}
		}

		// button touched
		if (WaveVR_Controller.Input(this.device).GetTouch(WVR_InputId.WVR_InputId_Alias1_Touchpad))
		{
			if (!isTouchPressed)
			{
				if (Touch_Dot != null)
				{
					Touch_Dot.SetActive(true);
				}

				if (TouchPad != null && Touch_Dot != null)
				{
					var axis = WaveVR_Controller.Input(this.device).GetAxis(WVR_InputId.WVR_InputId_Alias1_Touchpad);

					float xangle = axis.x * (touchpadMesh.bounds.size.x * TouchPad.transform.localScale.x - toucheffectMesh.bounds.size.x * Touch_Dot.transform.localScale.x) / 2;
					float yangle = 0f;
					if (touch_YAxis_mapping == AxisMapping.Y_Axis)
					{
						yangle = axis.y * (touchpadMesh.bounds.size.z * TouchPad.transform.localScale.z - toucheffectMesh.bounds.size.z * Touch_Dot.transform.localScale.z) / 2;
					} else
					{
						yangle = axis.y * (touchpadMesh.bounds.size.z * TouchPad.transform.localScale.z - toucheffectMesh.bounds.size.z * Touch_Dot.transform.localScale.z) / 2;
					}

					if (Log.gpl.Print)
					{

						Log.d(LOG_TAG, "WVR_InputId_Alias1_Touchpad axis x: " + axis.x + ", xangle: " + xangle + " axis.y: " + axis.y + ", yangle: " + yangle);
#if DEBUG
						Log.d(LOG_TAG, "Touch_EffectMesh.bounds.size.x: " + toucheffectMesh.bounds.size.x);
						Log.d(LOG_TAG, "Touch_EffectMesh.bounds.size.y: " + toucheffectMesh.bounds.size.y);
						Log.d(LOG_TAG, "Touch_EffectMesh. x scale: " + Touch_Dot.transform.localScale.x);
						Log.d(LOG_TAG, "Touch_EffectMesh. y scale: " + Touch_Dot.transform.localScale.y);

						Log.d(LOG_TAG, "touchpadMesh.bounds.size.x: " + touchpadMesh.bounds.size.x);
						Log.d(LOG_TAG, "touchpadMesh.bounds.size.y: " + touchpadMesh.bounds.size.y);
						Log.d(LOG_TAG, "touchpadMesh. x scale: " + TouchPad.transform.localScale.x);
						Log.d(LOG_TAG, "touchpadMesh. y scale: " + TouchPad.transform.localScale.y);
#endif
					}
					Vector3 translateVec = Vector3.zero;

					if (touch_YAxis_mapping == AxisMapping.Y_Axis)
					{
						translateVec = new Vector3(xangle, yangle, 0);
					} else
					{
						translateVec = new Vector3(xangle, 0, yangle);
					}
					Touch_Dot.transform.localPosition = originPosition + translateVec;
				}
			}
		}
	}
}
