// "WaveVR SDK 
// © 2017 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the WaveVR SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

// Copyright 2017 hTC Inc. All rights reserved.
//
// This file contains code derived from Google Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//	 http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using UnityEngine;
using wvr;
using System.Collections;
using System;
using WVR_Log;
using UnityEngine.Profiling;

/// <summary>
/// Draws a circular reticle in front of any object that the user gazes at.
/// The circle dilates if the object is clickable.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class WaveVR_Reticle : MonoBehaviour {
	public bool ListenToDevice = false;
	private const string LOG_TAG = "WaveVR_Reticle";
	private void DEBUG(string msg)
	{
		if (Log.EnableDebugLog)
		{
			Log.d (LOG_TAG, msg, true);
		}
	}
	public WaveVR_Controller.EDeviceType device;
	/// <summary>
	/// Number of segments making the reticle circle.
	/// </summary>
	public int reticleSegments = 20;
	/// <summary>
	/// Growth speed multiplier for the reticle.
	/// </summary>
	public float reticleGrowthSpeed = 8.0f;
	/// <summary>
	/// Color of reticle pointer.
	/// </summary>
	public Color reticleColor = Color.white;
	/// <summary>
	/// The color flicker flag of reticle pointer
	/// </summary>
	public bool colorFlickerPerSecond = false;
	/// <summary>
	/// The color deepening flag of reticle pointer during rotation status
	/// </summary>
	public bool deepeningColorRotation = false;
	/// <summary>
	/// The rotation of reticle pointer
	/// </summary>
	public int rotationSpeed = 6;  // 1 the highest speed
	public float reticleInnerDiameter = 0.0f;
	public float reticleOuterDiameter = 0.005236002f;
	public float interactObjInnerDiameter = 0.02094472f;
	public float interactObjOuterDiameter = 0.02618144f;
	public float kReticleDistanceMax = 10.0f;	// Maximum distance of the reticle (in meters).

	private Material materialComp;
	private Mesh mesh;
	private float reticleDistanceInMeters = 10.0f;	  // Current distance of the reticle (in meters).
	private const float kReticleDistanceMin = 1.0f;	 // Minimum distance of the reticle (in meters).
	//private const float kReticleDistanceMax = 10.0f;	// Maximum distance of the reticle (in meters).
	private float reticleInnerAngle = 0.0f;			 // Current inner angle of the reticle (in degrees).
	private float reticleOuterAngle = 0.3f;			 // Current outer angle of the reticle (in degrees).
	private float kReticleMinInnerAngle = 0.0f;   // Minimum inner angle of the reticle (in degrees).
	private float kReticleMinOuterAngle = 0.3f;   // Minimum outer angle of the reticle (in degrees).
	// private float kReticleMaxInnerAngle = 0.0f;
	// private float kReticleMaxOuterAngle = 0.3f;
	private float kReticleGrowthInnerAngle = 1.2f;
	private float kReticleGrowthOuterAngle = 1.2f;	 // Angle at which to expand the reticle when intersecting with an object (in degrees).
	//private float reticleInnerDiameter = 0.0f;		  // Current inner diameters of the reticle, before distance multiplication.
	//private float reticleOuterDiameter = 0.0f;		  // Current outer diameters of the reticle, before distance multiplication.
	private Color colorFactor = Color.black;			// The color variable of reticle pointer
	private float colorFlickerTime = 0.0f;			  // The color flicker time
	private float progressTime = 0.0f;
	private bool isTriggerProgress = false;			 // true: show progress effect of reticle, false: remove progress effect of reticle
	private int rotSpeedLimit = 36;					 // Internal rotation speed limit, it means the lowest rotationSpeed is 9
	private bool enabledReticle = true;				 // true: show reticle, false: remove reticle
	private bool meshIsCreated = false;				 // true: the mesh of reticle is created, false: the mesh of reticle is not ready
	int internalRotationSpeed;
	int colorIter = 0;
	float[] colorRotation;
	int[] rotSpeedBound;

	void Start()
	{
		ReadJsonValues();
		Validate();
		if (enabledReticle)
		{
			if (!meshIsCreated)
			{
				initialReticle();
			}
		}
		else
		{
			if (meshIsCreated)
			{
				removeReticle();
			}
		}
	}

	private void setReticleAngle()
	{

		reticleInnerAngle = (Mathf.Atan(reticleInnerDiameter) * 180) / Mathf.PI;
		kReticleMinInnerAngle = reticleInnerAngle;
		reticleOuterAngle = (Mathf.Atan(reticleOuterDiameter) *180) / Mathf.PI;
		kReticleMinOuterAngle = reticleOuterAngle;

		float kReticleMaxInnerAngle = (Mathf.Atan(interactObjInnerDiameter) * 180) / Mathf.PI;
		float kReticleMaxOuterAngle = (Mathf.Atan(interactObjOuterDiameter) * 180) / Mathf.PI;
		kReticleGrowthInnerAngle = kReticleMaxInnerAngle - kReticleMinInnerAngle;
		kReticleGrowthOuterAngle = kReticleMaxOuterAngle - kReticleMinOuterAngle;
	}

	MaterialPropertyBlock materialProperty = null;
	void Update() {
		if (ListenToDevice)
		{
			enabledReticle = WaveVR_Controller.Input (this.device).connected;
		}

		if (enabledReticle) {
			if (!meshIsCreated) {
				initialReticle();
			}
		} else {
			if (meshIsCreated) {
				removeReticle();
			}
			return;
		}

		reticleDistanceInMeters = Mathf.Clamp(reticleDistanceInMeters, kReticleDistanceMin, kReticleDistanceMax);

		if (reticleInnerAngle < kReticleMinInnerAngle)
			reticleInnerAngle = kReticleMinInnerAngle;
		if (reticleOuterAngle < kReticleMinOuterAngle)
			reticleOuterAngle = kReticleMinOuterAngle;

		float innerHalfAngelRadians = Mathf.Deg2Rad * reticleInnerAngle * 0.5f;
		float outerHalfAngelRadians = Mathf.Deg2Rad * reticleOuterAngle * 0.5f;
		float innerDiameter = 2.0f * Mathf.Tan(innerHalfAngelRadians);
		float outerDiameter = 2.0f * Mathf.Tan(outerHalfAngelRadians);

		if (rotationSpeed < 1)
			rotationSpeed = 1;

		if (internalRotationSpeed != rotationSpeed * 4)
		{
			UpdateRotSpeedBound(rotationSpeed * 4);
		}

		if (colorFlickerPerSecond) {
			if (Time.unscaledTime - colorFlickerTime >= 1.0f) {
				colorFlickerTime = Time.unscaledTime;
				if (isTriggerProgress) {
					 colorFactor = deepeningColorRotation ? Color.Lerp(Color.white, reticleColor == Color.white ? Color.black : reticleColor, progressTime/100) : reticleColor;
				} else {
					 if (reticleColor != Color.white) {
						 colorFactor = Color.white;
					 } else {
						 colorFactor = Color.black;
					 }
				}
			} else {
				if (isTriggerProgress) {
					 colorFactor = deepeningColorRotation ? Color.Lerp(Color.white, reticleColor == Color.white ? Color.black : reticleColor, progressTime/100) : reticleColor;
				} else {
					 colorFactor = reticleColor;
				}
			}
		} else {
			if (isTriggerProgress) {
				 colorFactor = deepeningColorRotation ? Color.Lerp(Color.white, reticleColor == Color.white ? Color.black : reticleColor, progressTime/100) : reticleColor;
			} else {
				 colorFactor = reticleColor;
			}
		}
		int option = colorIter % internalRotationSpeed;
		if (option >= rotSpeedBound[0] && option <= rotSpeedBound[1]) {
			 colorRotation[0] = 1f;
			 colorRotation[1] = 0f;
			 colorRotation[2] = 0f;
			 colorRotation[3] = 0f;
		} else if (option >= rotSpeedBound[2] && option <= rotSpeedBound[3]) {
			 colorRotation[0] = 0f;
			 colorRotation[1] = 1f;
			 colorRotation[2] = 0f;
			 colorRotation[3] = 0f;
		} else if (option >= rotSpeedBound[4] && option <= rotSpeedBound[5]) {
			 colorRotation[0] = 0f;
			 colorRotation[1] = 0f;
			 colorRotation[2] = 1f;
			 colorRotation[3] = 0f;
		} else if (option >= rotSpeedBound[6] && option <= rotSpeedBound[7]) {
			 colorRotation[0] = 0f;
			 colorRotation[1] = 0f;
			 colorRotation[2] = 0f;
			 colorRotation[3] = 1f;
		}

		if (materialProperty == null)
			materialProperty = new MaterialPropertyBlock();

		materialProperty.SetFloatArray("colorRotFactor", colorRotation);
		gameObject.GetComponent<Renderer>().SetPropertyBlock(materialProperty);
		materialComp.SetColor("_Color", colorFactor);
		colorIter = (colorIter + 1) % internalRotationSpeed;

		reticleInnerDiameter = Mathf.Lerp(reticleInnerDiameter, innerDiameter, Time.deltaTime * reticleGrowthSpeed);
		reticleOuterDiameter = Mathf.Lerp(reticleOuterDiameter, outerDiameter, Time.deltaTime * reticleGrowthSpeed);

		materialComp.SetFloat("_InnerDiameter", reticleInnerDiameter * reticleDistanceInMeters);
		materialComp.SetFloat("_OuterDiameter", reticleOuterDiameter * reticleDistanceInMeters);
		materialComp.SetFloat("_DistanceInMeters", reticleDistanceInMeters);
	}

	private void initialReticle() {
		CreateGazePointer();
	setReticleAngle();
		colorFlickerTime = Time.unscaledTime;
		materialComp = gameObject.GetComponent<Renderer>().material;
		UpdateRotSpeedBound(rotationSpeed * 4);
		meshIsCreated = true;
	}

	private void removeReticle () {
		mesh.Clear();
		meshIsCreated = false;
	}

	private void CreateGazePointer() {
		int vertexCount = (reticleSegments + 1) * 2;
		Vector3[] vertices = new Vector3[vertexCount];
		for (int vi = 0, si = 0; si <= reticleSegments; si++) {
			float angle = (float)si / (float)reticleSegments * Mathf.PI * 2.0f;
			float x = Mathf.Sin(angle);
			float y = Mathf.Cos(angle);
			vertices[vi++] = new Vector3(x, y, 0.0f);
			vertices[vi++] = new Vector3(x, y, 1.0f);
		}

		int indicesCount = (reticleSegments + 1) * 6;
		int[] indices = new int[indicesCount];
		int vert = 0;
		for (int ti = 0, si = 0; si < reticleSegments; si++) {
			indices[ti++] = vert + 1;
			indices[ti++] = vert;
			indices[ti++] = vert + 2;
			indices[ti++] = vert + 1;
			indices[ti++] = vert + 2;
			indices[ti++] = vert + 3;

			vert += 2;
		}

		mesh = new Mesh();
		gameObject.AddComponent<MeshFilter>();
		GetComponent<MeshFilter>().mesh = mesh;
		mesh.vertices = vertices;
		mesh.triangles = indices;
		mesh.RecalculateBounds();
	}

	private void UpdateRotSpeedBound(int speedSetting) {
		internalRotationSpeed = (speedSetting <= rotSpeedLimit) ? speedSetting : rotSpeedLimit;
		colorRotation = new float[4];
		rotSpeedBound = new int[8];
		rotSpeedBound[0] = 0;
		rotSpeedBound[1] = (internalRotationSpeed / 4) - 1;
		rotSpeedBound[2] = internalRotationSpeed / 4;
		rotSpeedBound[3] = (internalRotationSpeed / 2)- 1;
		rotSpeedBound[4] = internalRotationSpeed / 2;
		rotSpeedBound[5] = ((internalRotationSpeed / 4) * 3) - 1;
		rotSpeedBound[6] = (internalRotationSpeed / 4) * 3;
		rotSpeedBound[7] = internalRotationSpeed - 1;
	}

	public void ShowReticle() {
		enabledReticle = true;
	}

	public void RemoveReticle() {
		enabledReticle = false;
	}

	public void SetColorFlicker(bool switchOn) {
		colorFlickerPerSecond = switchOn;
	}

	public bool GetColorFlicker() {
		return colorFlickerPerSecond;
	}

	public void OnGazeEnter (Camera camera, GameObject target, Vector3 intersectionPosition, bool isInteractive) {
		SetGazeTarget(intersectionPosition, isInteractive);
	}

	public void OnGazeStay (Camera camera, GameObject target, Vector3 intersectionPosition, bool isInteractive) {
		SetGazeTarget(intersectionPosition, isInteractive);
	}

	public void OnGazeExit (Camera camera, GameObject target) {
		reticleDistanceInMeters = kReticleDistanceMax;
		reticleInnerAngle = kReticleMinInnerAngle;
		reticleOuterAngle = kReticleMinOuterAngle;
	}

	public void OnGazeTriggerStart (Camera camera) { }

	public void OnGazeTriggerEnd (Camera camera) { }

	public void GetPointerRadius (out float innerRadius, out float outerRadius) {
		float minInnerAngleRadians = Mathf.Deg2Rad * kReticleMinInnerAngle;
		float maxInnerAngleRadians = Mathf.Deg2Rad * (kReticleMinInnerAngle + kReticleGrowthInnerAngle);
		innerRadius = 2.0f * Mathf.Tan(minInnerAngleRadians);
		outerRadius = 2.0f * Mathf.Tan(maxInnerAngleRadians);
	}

	public void setProgressBarTime (float time) {
		progressTime = time;
	}

	public float getReticleCurrentDistance() {
		 return reticleDistanceInMeters;
	}

	public void triggerProgressBar (bool switchOn) {
		//bool preValue = isTriggerProgress;

		isTriggerProgress = switchOn;
		if (enabledReticle/* && preValue*/) {
			materialComp.SetFloat("_TriggerProgress", isTriggerProgress ? 1.0f : 0.0f);
		}
	}

	private void SetGazeTarget (Vector3 target, bool interactive) {
		Vector3 targetLocalPosition = transform.InverseTransformPoint(target);
		reticleDistanceInMeters = Mathf.Clamp(targetLocalPosition.z, kReticleDistanceMin, kReticleDistanceMax);
		if (interactive) {
			reticleInnerAngle = kReticleMinInnerAngle + kReticleGrowthInnerAngle;
			reticleOuterAngle = kReticleMinOuterAngle + kReticleGrowthOuterAngle;
		} else {
			reticleInnerAngle = kReticleMinInnerAngle;
			reticleOuterAngle = kReticleMinOuterAngle;
		}
	}

	public void SetReticleColor(Color reticle_color)
	{
		reticleColor = reticle_color;
	}

	private void ReadJsonValues()
	{
		DEBUG ("reticleInnerDiameter: " + reticleInnerDiameter + ", reticleOuterDiameter: " + reticleOuterDiameter + ", interactObjInnerDiameter: " + interactObjInnerDiameter + ", interactObjOuterDiameter: " + interactObjOuterDiameter + ", kReticleDistanceMax: " + kReticleDistanceMax + ", reticleSegments: " + reticleSegments + ", reticleColor: " + reticleColor.ToString () + ", rotationSpeed: " + rotationSpeed + ", colorFlickerPerSecond: " + colorFlickerPerSecond + ", deepeningColorRotation: " + deepeningColorRotation);
		string json_values = WaveVR_Utils.OEMConfig.getControllerConfig();
		if (!json_values.Equals(""))
		{
			try {
				SimpleJSON.JSONNode jsNodes = SimpleJSON.JSONNode.Parse(json_values);

				string node_value = "";
				node_value = jsNodes["reticle"]["inner_diameter"].Value;
				if (!node_value.Equals("") && IsFloat(node_value) == true)
					reticleInnerDiameter = float.Parse(node_value);

				node_value = jsNodes["reticle"]["outer_diameter"].Value;
				if (!node_value.Equals("") && IsFloat(node_value) == true)
					reticleOuterDiameter = float.Parse(node_value);

				node_value = jsNodes["reticle"]["interact_object_inner_diameter"].Value;
				if (!node_value.Equals("") && IsFloat(node_value) == true)
					interactObjInnerDiameter = float.Parse(node_value);

				node_value = jsNodes["reticle"]["interact_object_outer_diameter"].Value;
				if (!node_value.Equals("") && IsFloat(node_value) == true)
					interactObjOuterDiameter = float.Parse(node_value);

				node_value = jsNodes["reticle"]["distance"].Value;
				if (!node_value.Equals("") && IsFloat(node_value) == true)
					kReticleDistanceMax = float.Parse(node_value);

				node_value = jsNodes["reticle"]["segment"].Value;
				if (!node_value.Equals("") && IsNumeric(node_value) == true)
					reticleSegments = int.Parse(node_value);

				node_value = jsNodes["reticle"]["color"].Value;
				if (!node_value.Equals(""))
					reticleColor = StringToColor32(node_value);

				node_value = jsNodes["reticle"]["rotation_speed"].Value;
				if (!node_value.Equals("") && IsNumeric(node_value) == true)
					rotationSpeed = int.Parse(node_value);

				node_value = jsNodes["reticle"]["colorFlicker"].Value;
				if (!node_value.Equals("") && IsBoolean(node_value) == true)
					colorFlickerPerSecond = bool.Parse(node_value);

				node_value = jsNodes["reticle"]["deepening_color_rotation"].Value;
				if (!node_value.Equals("") && IsBoolean(node_value) == true)
					deepeningColorRotation = bool.Parse(node_value);

				DEBUG ("reticleInnerDiameter: " + reticleInnerDiameter + ", reticleOuterDiameter: " + reticleOuterDiameter + ", interactObjInnerDiameter: " + interactObjInnerDiameter + ", interactObjOuterDiameter: " + interactObjOuterDiameter + ", kReticleDistanceMax: " + kReticleDistanceMax + ", reticleSegments: " + reticleSegments + ", reticleColor: " + reticleColor.ToString() + ", rotationSpeed: " + rotationSpeed + ", colorFlickerPerSecond: " + colorFlickerPerSecond + ", deepeningColorRotation: " + deepeningColorRotation);
			}
			catch (Exception e)
			{
				Log.e(LOG_TAG, "JsonParse failed: " + e.ToString());
			}
		}

	}

	private bool IsBoolean(string value)
	{
		try
		{
			bool i = Convert.ToBoolean(value);
			DEBUG (value + " Convert to bool success: " + i.ToString());
			return true;
		}
		catch (Exception e)
		{
			Log.e(LOG_TAG, value + " Convert to bool failed: " + e.ToString());
			return false;
		}
	}

	private bool IsFloat(string value)
	{
		try
		{
			float i = Convert.ToSingle(value);
			DEBUG (value + " Convert to float success: " + i.ToString());
			return true;
		}
		catch(Exception e)
		{
			Log.e(LOG_TAG, value + " Convert to float failed: " + e.ToString());
			return false;
		}
	}

	private bool IsNumeric(string value)
	{
		try
		{
			int i = Convert.ToInt32(value);
			DEBUG (value + " Convert to int success: " + i.ToString());
			return true;
		}
		catch (Exception e)
		{
			Log.e(LOG_TAG, value + " Convert to Int failed: " + e.StackTrace.ToString());
			return false;
		}
	}

	private const float reticleInnerDiameterMin = 0.0f;
	private const float reticleInnerDiameterMax = 0.0002f;

	private const float reticleOuterDiameterMin = 0.005f;
	private const float reticleOuterDiameterMax = 0.01f;

	private const float interactObjInnerDiameterMin = 0.015f;
	private const float interactObjInnerDiameterMax = 0.022f;

	private const float interactObjOuterDiameterMin = 0.026f;
	private const float interactObjOuterDiameterMax = 0.035f;

	private const float kReticleDistanceValidateMax = 10.0f;

	private void Validate()
	{
		if (reticleInnerDiameter < reticleInnerDiameterMin)
			reticleInnerDiameter = reticleInnerDiameterMin;

		if (reticleInnerDiameter > reticleInnerDiameterMax)
			reticleInnerDiameter = reticleInnerDiameterMax;

		if (reticleOuterDiameter < reticleOuterDiameterMin)
			reticleOuterDiameter = reticleOuterDiameterMin;

		if (reticleOuterDiameter > reticleOuterDiameterMax)
			reticleOuterDiameter = reticleOuterDiameterMax;

		if (interactObjInnerDiameter < interactObjInnerDiameterMin)
			interactObjInnerDiameter = interactObjInnerDiameterMin;

		if (interactObjInnerDiameter > interactObjInnerDiameterMax)
			interactObjInnerDiameter = interactObjInnerDiameterMax;

		if (interactObjOuterDiameter < interactObjOuterDiameterMin)
			interactObjOuterDiameter = interactObjOuterDiameterMin;

		if (interactObjOuterDiameter > interactObjOuterDiameterMax)
			interactObjOuterDiameter = interactObjOuterDiameterMax;

		if (kReticleDistanceMax > kReticleDistanceValidateMax)
			kReticleDistanceMax = kReticleDistanceValidateMax;
	}

	private Color32 StringToColor32(string color_string)
	{
		try
		{
			byte[] _color_r = BitConverter.GetBytes(Convert.ToInt32(color_string.Substring(1, 2), 16));
			byte[] _color_g = BitConverter.GetBytes(Convert.ToInt32(color_string.Substring(3, 2), 16));
			byte[] _color_b = BitConverter.GetBytes(Convert.ToInt32(color_string.Substring(5, 2), 16));
			byte[] _color_a = BitConverter.GetBytes(Convert.ToInt32(color_string.Substring(7, 2), 16));

			return new Color32(_color_r[0], _color_g[0], _color_b[0], _color_a[0]);
		}
		catch(Exception e)
		{
			Log.e(LOG_TAG, "StringToColor32: " + e.ToString());
			return Color.white;
		}
	}
}
