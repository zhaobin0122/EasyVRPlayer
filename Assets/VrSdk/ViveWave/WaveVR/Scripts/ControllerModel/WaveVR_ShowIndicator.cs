// "WaveVR SDK 
// © 2017 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the WaveVR SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

//#define DEBUG
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using wvr;
using WVR_Log;

[System.Serializable]
public class ButtonIndication
{
	public enum Alignment
	{
		RIGHT,
		LEFT
	};

	public enum KeyIndicator
	{
		Trigger,
		TouchPad,
		DigitalTrigger,
		App,
		Home,
		Volume,
		VolumeUp,
		VolumeDown,
		Grip,
		DPad_Left,
		DPad_Right,
		DPad_Up,
		DPad_Down
	};

	public KeyIndicator keyType;
	public Alignment alignment = Alignment.RIGHT;
	public Vector3 indicationOffset = new Vector3(0f, 0f, 0f);
	public bool useMultiLanguage = false;
	public string indicationText = "system";
	public bool followButtonRotation = false;
}

[System.Serializable]
public class ComponentsIndication
{
	public string Name;  // Component name
	public string Description = "system";  // Component description
	public GameObject SourceObject;
	public GameObject LineIndicator;
	public GameObject DestObject;
	public ButtonIndication.Alignment alignment = ButtonIndication.Alignment.RIGHT;
	public Vector3 Offset;
	public bool followButtonRoration = false;
 }

public class WaveVR_ShowIndicator : MonoBehaviour {
	private static string LOG_TAG = "WaveVR_ShowIndicator";
	private void PrintDebugLog(string msg)
	{
		Log.d(LOG_TAG, msg);
	}

	private void PrintInfoLog(string msg)
	{
		Log.i(LOG_TAG, msg);
	}

	[Header("Indication feature")]
	public bool showIndicator = false;
	[Range(0, 90.0f)]
	public float showIndicatorAngle = 30.0f;
	public bool hideIndicatorByRoll = true;
	public bool basedOnEmitter = true;

	[Header("Line customization")]
	[Range(0.01f, 0.1f)]
	public float lineLength = 0.03f;
	[Range(0.0001f, 0.1f)]
	public float lineStartWidth = 0.0004f;
	[Range(0.0001f, 0.1f)]
	public float lineEndWidth = 0.0004f;
	public Color lineColor = Color.white;

	[Header("Text customization")]
	[Range(0.01f, 0.2f)]
	public float textCharacterSize = 0.08f;
	[Range(0.01f, 0.2f)]
	public float zhCharactarSize = 0.07f;
	[Range(50, 200)]
	public int textFontSize = 100;
	public Color textColor = Color.white;

	[Header("Indications")]
	public List<ButtonIndication> buttonIndicationList = new List<ButtonIndication>();

	private WaveVR_Resource rw = null;
	private string sysLang = null;
	private string sysCountry = null;
	private int checkCount = 0;
	private GameObject indicatorPrefab = null;
	private GameObject linePrefab = null;
	private List<ComponentsIndication> compInd = new List<ComponentsIndication>();
	private GameObject _HMD = null;
	private bool needRedraw = true;
	private GameObject Emitter = null;

	// reset for redraw
	void resetIndicator()
	{
		if (showIndicator)
		{
			rw = WaveVR_Resource.instance;
			sysLang = rw.getSystemLanguage();
			sysCountry = rw.getSystemCountry();

			needRedraw = true;
			clearResourceAndObject();
		}
	}

	void OnApplicationPause(bool pauseStatus)
	{
		if (pauseStatus == true)
		{
			resetIndicator();
		}
	}

	void clearResourceAndObject()
	{
		PrintDebugLog("clear Indicator!");
		foreach (ComponentsIndication ci in compInd)
		{
			if (ci.DestObject != null)
			{
				Destroy(ci.DestObject);
			}
			if (ci.LineIndicator != null)
			{
				Destroy(ci.LineIndicator);
			}
		}
		compInd.Clear();

		Resources.UnloadUnusedAssets();
	}
	void onAdaptiveControllerModelReady(params object[] args)
	{
		createIndicator();
	}

	void OnEnable()
	{
		WaveVR_Utils.Event.Listen(WaveVR_Utils.Event.ADAPTIVE_CONTROLLER_READY, onAdaptiveControllerModelReady);

	}

	void OnDisable()
	{
		WaveVR_Utils.Event.Remove(WaveVR_Utils.Event.ADAPTIVE_CONTROLLER_READY, onAdaptiveControllerModelReady);
	}

	// Use this for initialization
	void Start() {
	}

	public void createIndicator() {
		if (!showIndicator) return;
		clearResourceAndObject();
		PrintDebugLog("create Indicator!");
		rw = WaveVR_Resource.instance;
		indicatorPrefab = Resources.Load("ComponentIndicator") as GameObject;

		if (indicatorPrefab == null)
		{
			PrintInfoLog("ComponentIndicator is not found!");
			return;
		}
		else
		{
			PrintDebugLog("ComponentIndicator is found!");
		}

		linePrefab = Resources.Load("LineIndicator") as GameObject;

		if (linePrefab == null)
		{
			PrintInfoLog("LineIndicator is not found!");
			return;
		}
		else
		{
			PrintDebugLog("LineIndicator is found!");
		}

		if (_HMD == null)
			_HMD = WaveVR_Render.Instance.gameObject;

		if (_HMD == null)
		{
			PrintInfoLog("Can't get HMD!");
			return;
		}

		var gc = transform.childCount;

		for (int i = 0; i < gc; i++)
		{
			GameObject go = transform.GetChild(i).gameObject;

			PrintInfoLog("child name is " + go.name);
		}

		PrintInfoLog("showIndicatorAngle: " + showIndicatorAngle + ", hideIndicatorByRoll: " + hideIndicatorByRoll + ", basedOnEmitter: " + basedOnEmitter);
		PrintInfoLog("Line settings--\n lineLength: " + lineLength + ", lineStartWidth: " + lineStartWidth + ", lineEndWidth: " + lineEndWidth + ", lineColor: " + lineColor);
		PrintInfoLog("Text settings--\n textCharacterSize: " + textCharacterSize + ", zhCharactarSize: " + zhCharactarSize + ", textFontSize: " + textFontSize + ", textColor: " + textColor);

		foreach (ButtonIndication bi in buttonIndicationList)
		{
			PrintInfoLog("keyType: " + bi.keyType + ", alignment: " + bi.alignment + ", offset: " + bi.indicationOffset + ", useMultiLanguage: " + bi.useMultiLanguage +
				", indication: " + bi.indicationText + ", followRotation: " + bi.followButtonRotation);

			// find component by name
			string partName = null;
			string partName1 = null;
			string partName2 = null;
			string indicationKey = null;
			switch(bi.keyType)
			{
				case ButtonIndication.KeyIndicator.Trigger:
					partName = "_[CM]_TriggerKey";
					partName1 = "__CM__TriggerKey";
					partName2 = "__CM__TriggerKey.__CM__TriggerKey";
					indicationKey = "TriggerKey";
					break;
				case ButtonIndication.KeyIndicator.TouchPad:
					partName = "_[CM]_TouchPad";
					partName1 = "__CM__TouchPad";
					partName2 = "__CM__TouchPad.__CM__TouchPad";
					indicationKey = "TouchPad";
					break;
				case ButtonIndication.KeyIndicator.Grip:
					partName = "_[CM]_Grip";
					partName1 = "__CM__Grip";
					partName2 = "__CM__Grip.__CM__Grip";
					indicationKey = "Grip";
					break;
				case ButtonIndication.KeyIndicator.DPad_Left:
					partName = "_[CM]_DPad_Left";
					partName1 = "__CM__DPad_Left";
					partName2 = "__CM__DPad_Left.__CM__DPad_Left";
					indicationKey = "DPad_Left";
					break;
				case ButtonIndication.KeyIndicator.DPad_Right:
					partName = "_[CM]_DPad_Right";
					partName1 = "__CM__DPad_Right";
					partName2 = "__CM__DPad_Right.__CM__DPad_Right";
					indicationKey = "DPad_Right";
					break;
				case ButtonIndication.KeyIndicator.DPad_Up:
					partName = "_[CM]_DPad_Up";
					partName1 = "__CM__DPad_Up";
					partName2 = "__CM__DPad_Up.__CM__DPad_Up";
					indicationKey = "DPad_Up";
					break;
				case ButtonIndication.KeyIndicator.DPad_Down:
					partName = "_[CM]_DPad_Down";
					partName1 = "__CM__DPad_Down";
					partName2 = "__CM__DPad_Down.__CM__DPad_Down";
					indicationKey = "DPad_Down";
					break;
				case ButtonIndication.KeyIndicator.App:
					partName = "_[CM]_AppButton";
					partName1 = "__CM__AppButton";
					partName2 = "__CM__AppButton.__CM__AppButton";
					indicationKey = "AppKey";
					break;
				case ButtonIndication.KeyIndicator.Home:
					partName = "_[CM]_HomeButton";
					partName1 = "__CM__HomeButton";
					partName2 = "__CM__HomeButton.__CM__HomeButton";
					indicationKey = "HomeKey";
					break;
				case ButtonIndication.KeyIndicator.Volume:
					partName = "_[CM]_VolumeKey";
					partName1 = "__CM__VolumeKey";
					partName2 = "__CM__VolumeKey.__CM__VolumeKey";
					indicationKey = "VolumeKey";
					break;
				case ButtonIndication.KeyIndicator.VolumeUp:
					partName = "_[CM]_VolumeUp";
					partName1 = "__CM__VolumeUp";
					partName2 = "__CM__VolumeUp.__CM__VolumeUp";
					indicationKey = "VolumeUp";
					break;
				case ButtonIndication.KeyIndicator.VolumeDown:
					partName = "_[CM]_VolumeDown";
					partName1 = "__CM__VolumeDown";
					partName2 = "__CM__VolumeDown.__CM__VolumeDown";
					indicationKey = "VolumeDown";
					break;
				case ButtonIndication.KeyIndicator.DigitalTrigger:
					partName = "_[CM]_DigitalTriggerKey";
					partName1 = "__CM__DigitalTriggerKey";
					partName2 = "__CM__DigitalTriggerKey.__CM__DigitalTriggerKey";
					indicationKey = "DigitalTriggerKey";
					break;
				default:
					partName = "_[CM]_unknown";
					partName1 = "__CM__unknown";
					partName2 = "__CM__unknown.__CM__unknown";
					indicationKey = "unknown";
					PrintDebugLog("Unknown key type!");
					break;
			}

			Transform tmp = transform.Find(partName);
			if (tmp == null)
			{
				tmp = transform.Find(partName1);
				if (tmp == null)
				{
					tmp = transform.Find(partName2);
				}
			}

			if (tmp != null)
			{
				ComponentsIndication tmpCom = new ComponentsIndication();

				tmpCom.Name = partName;
				tmpCom.SourceObject = tmp.gameObject;
				tmpCom.alignment = bi.alignment;
				tmpCom.followButtonRoration = bi.followButtonRotation;

				Vector3 linePos;
				tmpCom.LineIndicator = null;

				linePos = transform.TransformPoint(new Vector3(0, tmp.localPosition.y, tmp.localPosition.z) + bi.indicationOffset);
				Quaternion spawnRot = Quaternion.identity;
				if (bi.followButtonRotation == true)
				{
					spawnRot = transform.rotation;
				}

				GameObject lineGO = Instantiate(linePrefab, linePos, spawnRot);
				lineGO.name = partName + "Line";

				var li = lineGO.GetComponent<IndicatorLine>();
				li.lineColor = lineColor;
				li.lineLength = lineLength;
				li.startWidth = lineStartWidth;
				li.endWidth = lineEndWidth;
				li.alignment = bi.alignment;
				li.updateMeshSettings();

				if (bi.followButtonRotation == true)
				{
					lineGO.transform.parent = tmpCom.SourceObject.transform;
				}
				lineGO.SetActive(false);
				tmpCom.LineIndicator = lineGO;

				tmpCom.DestObject = null;

				Vector3 spawnPos;
				if (bi.alignment == ButtonIndication.Alignment.RIGHT)
				{
					spawnPos = transform.TransformPoint(new Vector3(lineLength, tmp.localPosition.y, tmp.localPosition.z) + bi.indicationOffset);
				} else
				{
					spawnPos = transform.TransformPoint(new Vector3(lineLength * (-1), tmp.localPosition.y, tmp.localPosition.z) + bi.indicationOffset);
				}

				GameObject destGO = Instantiate(indicatorPrefab, spawnPos, transform.rotation);
				destGO.name = partName + "Ind";
				if (bi.followButtonRotation == true)
				{
					destGO.transform.parent = tmpCom.SourceObject.transform;
				}

				PrintInfoLog(" Source PartName: " + tmp.gameObject.name + " pos: " + tmp.position + " Rot: " + tmp.rotation);
				PrintInfoLog(" Line Name: " + lineGO.name + " pos: " + lineGO.transform.position + " Rot: " + lineGO.transform.rotation);
				PrintInfoLog(" Destination Name: " + destGO.name + " pos: " + destGO.transform.position + " Rot: " + destGO.transform.rotation);

				int childC = destGO.transform.childCount;
				for (int i = 0; i < childC; i++)
				{
					GameObject c = destGO.transform.GetChild(i).gameObject;
					if (bi.alignment == ButtonIndication.Alignment.LEFT)
					{
						float tx = c.transform.localPosition.x;
						c.transform.localPosition = new Vector3(tx * (-1), c.transform.localPosition.y, c.transform.localPosition.z);
					}
					TextMesh tm = c.GetComponent<TextMesh>();
					MeshRenderer mr = c.GetComponent<MeshRenderer>();

					if (tm == null) PrintInfoLog(" tm is null ");
					if (mr == null) PrintInfoLog(" mr is null ");

					if (tm != null && mr != null)
					{
						tm.characterSize = textCharacterSize;
						if (c.name != "Shadow")
						{
							mr.material.SetColor("_Color", textColor);
						} else
						{
							PrintDebugLog(" Shadow found ");
						}
						tm.fontSize = textFontSize;
						if (bi.useMultiLanguage)
						{
							sysLang = rw.getSystemLanguage();
							sysCountry = rw.getSystemCountry();
							PrintDebugLog(" System language is " + sysLang);
							if (sysLang.StartsWith("zh"))
							{
								PrintDebugLog(" Chinese language");
								tm.characterSize = zhCharactarSize;
							}

							// use default string - multi-language
							if (bi.indicationText == "system") {
								tm.text = rw.getString(indicationKey);
								PrintInfoLog(" Name: " + destGO.name + " uses default multi-language -> " + tm.text);
							} else {
								tm.text = rw.getString(bi.indicationText);
								PrintInfoLog(" Name: " + destGO.name + " uses custom multi-language -> " + tm.text);
							}
						} else
						{
							if (bi.indicationText == "system")
								tm.text = indicationKey;
							else
								tm.text = bi.indicationText;

							PrintInfoLog(" Name: " + destGO.name + " didn't uses multi-language -> " + tm.text);
						}

						if (bi.alignment == ButtonIndication.Alignment.LEFT)
						{
							tm.anchor = TextAnchor.MiddleRight;
							tm.alignment = TextAlignment.Right;
						}
					}
				}

				destGO.SetActive(false);
				tmpCom.DestObject = destGO;
				tmpCom.Offset = bi.indicationOffset;

				PrintInfoLog(tmpCom.Name + " line -> " + tmpCom.LineIndicator.name + " destObjName -> " + tmpCom.DestObject.name);
				compInd.Add(tmpCom);
			}
			else
			{
				PrintInfoLog("Neither " + partName + " or " + partName1 + " or " + partName2 + " is not in the model!");
			}
		}

		Emitter = null;
		if (basedOnEmitter)
		{
			WaveVR_RenderModel wrm = this.GetComponentInChildren<WaveVR_RenderModel>();

			if (wrm != null)
			{
				GameObject modelObj = wrm.gameObject;

				int modelchild = modelObj.transform.childCount;
				for (int j = 0; j < modelchild; j++)
				{
					GameObject childName = modelObj.transform.GetChild(j).gameObject;
					if (childName.name == "__CM__Emitter" || childName.name == "_[CM]_Emitter")
					{
						Emitter = childName;
					}
				}
			}
		}

		needRedraw = false;
	}

	// Update is called once per frame
	void Update () {
		if (!showIndicator) return;
		if (_HMD == null) return;
		checkCount++;
		if (checkCount > 50) {
			checkCount = 0;
			if (rw != null)
			{
				if (rw.getSystemLanguage() != sysLang || rw.getSystemCountry() != sysCountry) resetIndicator();
			}
		}
		if (needRedraw == true) createIndicator();

		Vector3 _targetForward;
		if (basedOnEmitter && (Emitter != null))
			_targetForward = Emitter.transform.rotation * Vector3.forward;
		else
			_targetForward = transform.rotation * Vector3.forward;
		Vector3 _targetRight = transform.rotation * Vector3.right;
		Vector3 _targetUp = transform.rotation * Vector3.up;

		float zAngle = Vector3.Angle(_targetForward, _HMD.transform.forward);
		float xAngle = Vector3.Angle(_targetRight, _HMD.transform.right);
#if DEBUG
		float yAngle = Vector3.Angle(_targetUp, _HMD.transform.up);

		if (Log.gpl.Print)
			Log.d(LOG_TAG, "Z: " + _targetForward + ":" + zAngle + ", X: " + _targetRight + ":" + xAngle + ", Y: " + _targetUp + ":" + yAngle);
#endif
		if ((_targetForward.y < (showIndicatorAngle / 90f)) || (zAngle < showIndicatorAngle))
		{
			foreach (ComponentsIndication ci in compInd)
			{
				if (ci.LineIndicator != null)
				{
					ci.LineIndicator.SetActive(false);
				}
				if (ci.DestObject != null)
				{
					ci.DestObject.SetActive(false);
				}
			}

			return;
		}

		if (hideIndicatorByRoll)
		{
			if (xAngle > 90.0f)
			//if ((_targetRight.x < 0f) || (xAngle > 90f))
			{
				foreach (ComponentsIndication ci in compInd)
				{
					if (ci.LineIndicator != null)
					{
						ci.LineIndicator.SetActive(false);
					}
					if (ci.DestObject != null)
					{
						ci.DestObject.SetActive(false);
					}
				}

				return;
			}
		}

		foreach (ComponentsIndication ci in compInd)
		{
			if (ci.SourceObject != null)
			{
				ci.SourceObject.SetActive(true);
			}

			if (ci.LineIndicator != null)
			{
				ci.LineIndicator.SetActive(true);
			}

			if (ci.DestObject != null)
			{
				ci.DestObject.SetActive(true);
				if (ci.followButtonRoration == false)
				{
					ci.LineIndicator.transform.position = ci.SourceObject.transform.position + ci.Offset;
					if (ci.alignment == ButtonIndication.Alignment.RIGHT)
					{
						ci.DestObject.transform.position = new Vector3(transform.position.x + lineLength, ci.SourceObject.transform.position.y, ci.SourceObject.transform.position.z) + ci.Offset;
					} else
					{
						ci.DestObject.transform.position = new Vector3(transform.position.x - lineLength, ci.SourceObject.transform.position.y, ci.SourceObject.transform.position.z) + ci.Offset;

						TextMesh[] texts = ci.DestObject.GetComponentsInChildren<TextMesh>();
						foreach (TextMesh tm in texts)
						{
							if (tm != null)
							{
								tm.anchor = TextAnchor.MiddleRight;
								tm.alignment = TextAlignment.Right;
							}
						}
					}

					Transform[] transforms = ci.DestObject.GetComponentsInChildren<Transform>();
					foreach (Transform tf in transforms)
					{
						if (tf != null)
						{
							tf.rotation = Quaternion.identity;
						}
					}
				}
			}
		}
	}
}
