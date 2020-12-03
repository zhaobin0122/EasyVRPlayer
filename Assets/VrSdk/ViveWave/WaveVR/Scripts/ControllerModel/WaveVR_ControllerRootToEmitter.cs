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

public class WaveVR_ControllerRootToEmitter : MonoBehaviour {
	private static string LOG_TAG = "WaveVR_ControllerRootToEmitter";
	public WaveVR_Controller.EDeviceType deviceType = WaveVR_Controller.EDeviceType.Dominant;
	public GameObject[] moveToEmitter;

	private void PrintDebugLog(string msg)
	{
		Log.d(LOG_TAG, deviceType + ", " + msg);
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
	void Start () {

	}

	// Update is called once per frame
	void Update () {

	}

	private GameObject emitter = null;

	void onAdaptiveControllerModelReady(params object[] args)
	{
		WaveVR_Controller.EDeviceType _device = (WaveVR_Controller.EDeviceType)args[0];

		if (this.deviceType == _device)
		{
			WaveVR_RenderModel wrm = this.GetComponentInChildren<WaveVR_RenderModel>();

			if (wrm != null)
			{
				GameObject modelObj = wrm.gameObject;

				int modelchild = modelObj.transform.childCount;
				PrintDebugLog("onAdaptiveControllerModelReady() model child: " + modelchild);
				for (int j = 0; j < modelchild; j++)
				{
					GameObject childName = modelObj.transform.GetChild(j).gameObject;
					if (childName.name == "__CM__Emitter" || childName.name == "_[CM]_Emitter")
					{
						emitter = childName;
						PrintDebugLog("emitter local position (" + emitter.transform.localPosition.x + ", " + emitter.transform.localPosition.y + ", " + emitter.transform.localPosition.z + ")");
						PrintDebugLog("emitter local EulerAngles " + emitter.transform.localEulerAngles);

						if (moveToEmitter != null)
						{
							PrintDebugLog("__CM__Emitter is found, update objects' parent");

							foreach (GameObject mgo in moveToEmitter)
							{
								if (mgo != null)
								{
									PrintDebugLog("Move " + mgo.name + " to be children of emitter");
									mgo.transform.parent = emitter.transform;
									mgo.transform.localRotation = Quaternion.identity;
									mgo.transform.localPosition = Vector3.zero;
									mgo.SetActive(false);
									mgo.SetActive(true);
								}
							}
						}

						break;
					}
				}
			}
			else
			{
				PrintDebugLog("WaveVR_RenderModel is not found");
			}
		}
	}
}
