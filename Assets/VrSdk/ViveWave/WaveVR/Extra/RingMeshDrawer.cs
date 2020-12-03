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
using System;
using WVR_Log;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class RingMeshDrawer : MonoBehaviour {
	private const string LOG_TAG = "RingMeshDrawer";
	private void DEBUG(string msg)
	{
		if (Log.EnableDebugLog)
			Log.d (LOG_TAG, msg, true);
	}

	// Radius of ring.
	private const float DEF_RING_WIDTH = 0.01f;
	public float RingWidth = DEF_RING_WIDTH;
	private const float MIN_RING_WIDTH = 0.001f;
	// Radius of inner circle.
	private const float DEF_INNER_CIRCLE_RADIUS = 0.02f;
	public float InnerCircleRadius = DEF_INNER_CIRCLE_RADIUS;
	private const float MIN_INNER_CIRCLE_RADIUS = 0.001f;
	// World position of ring, used by WaveVR_GazeInputModule.
	[HideInInspector]
	public Vector3 RingPosition = Vector3.zero;
	// Z distance of ring.
	private const float DEF_RING_DISTANCE = 2.0f;
	public float RingDistance = DEF_RING_DISTANCE;
	private const float MIN_RING_DISTANCE = 0.3f;
	// Color of ring background
	public Color Color = Color.white;
	// Color of ring foreground
	public Color ProgressColor = new Color(0, 245, 255);
	[HideInInspector]
	public int RingPercent = 0;
	private const float percentAngle = 3.6f;	// 100% = 100 * 3.6f = 360 degrees.

	private const string RING_MATERIAL = "RingUnlitTransparentMat";
	private Material ringMaterial = null;
	private Material ringMaterialInstance = null;
	private MeshRenderer meshRend = null;
	private const int RENDER_QUEUE_MIN = 1000;
	private const int RENDER_QUEUE_MAX = 5000;
	private MeshFilter meshFilt = null;

	private Camera mCamera = null;

	private bool ValidateParameters()
	{
		if (meshRend == null || meshFilt == null)
			return false;

		if (mCamera == null)
		{
			if (WaveVR_InputModuleManager.Instance != null)
				mCamera = WaveVR_InputModuleManager.Instance.gameObject.GetComponent<Camera> ();
		}
		if (mCamera == null)
			return false;

		if (this.RingWidth < MIN_RING_WIDTH)
			this.RingWidth = DEF_RING_WIDTH;

		if (this.InnerCircleRadius < MIN_INNER_CIRCLE_RADIUS)
			this.InnerCircleRadius = DEF_INNER_CIRCLE_RADIUS;

		if (this.RingDistance < MIN_RING_DISTANCE)
			this.RingDistance = DEF_RING_DISTANCE;

		return true;
	}

	#region MonoBehaviour overrides
	private bool mEnabled = false;
	void OnEnable ()
	{
		if (!mEnabled)
		{
			meshRend = GetComponent<MeshRenderer> ();

			ringMaterial = Resources.Load (RING_MATERIAL) as Material;
			if (ringMaterial != null)
				ringMaterialInstance = Instantiate<Material> (ringMaterial);
			if (ringMaterialInstance != null)
			{
				meshRend.material = ringMaterialInstance;
				meshRend.material.renderQueue = RENDER_QUEUE_MAX;
				DEBUG ("OnEnable() ring material: " + meshRend.material.name);
			}
			else
				DEBUG ("OnEnable() CANNOT load material.");

			meshFilt = gameObject.GetComponent<MeshFilter> ();

			mEnabled = true;
		}
	}
	
	// Update is called once per frame
	private Vector3 ringPos = Vector3.zero;
	void Update ()
	{
		if (!ValidateParameters ())
			return;

		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;

		ringPos = this.RingPosition;
		if (ringPos == Vector3.zero)
			ringPos.z += this.RingDistance;
		ringPos.z = ringPos.z < MIN_RING_DISTANCE ? this.RingDistance : ringPos.z;

		float calcRingWidth = this.RingWidth * (ringPos.z / DEF_RING_DISTANCE);
		float calcInnerCircleRadius = this.InnerCircleRadius * (ringPos.z / DEF_RING_DISTANCE);

		DrawRing(calcRingWidth + calcInnerCircleRadius, calcInnerCircleRadius, this.RingPercent, ringPos);
	}

	void OnDisable()
	{
		if (mEnabled)
		{
			Mesh mesh = meshFilt.mesh;
			mesh.Clear ();
			ringMaterial = null;
			ringMaterialInstance = null;
			mEnabled = false;
			DEBUG ("OnDisable()");
		}
	}
	#endregion

	private const int VERTEX_COUNT = 400;		// 100 percents * 2 + 2, ex: 80% ring -> 80 * 2 + 2
	private Vector3[] ringVert = new Vector3[VERTEX_COUNT];
	private Color[] ringColor = new Color[VERTEX_COUNT];
	private const int TRIANGLE_COUNT = 100 * 6;	// 100 percents * 6, ex: 80% ring -> 80 * 6
	private int[] ringTriangle = new int[TRIANGLE_COUNT];
	private Vector2[] ringUv = new Vector2[VERTEX_COUNT];
	/// <summary>
	/// Draw a ring.
	/// </summary>
	/// <param name="radius">Radius of ring.</param>
	/// <param name="innerRadius">Radius of inner circle.</param>
	/// <param name="percent">Percent of ring.</param>
	/// <param name="position">Position of ring center.</param>
	public void DrawRing(float radius, float innerRadius, int percent, Vector3 position)
	{
		// vertices and colors
		float start_angle = 90;				// Start angle of drawing ring.
		for (int i = 0; i < VERTEX_COUNT; i += 2)
		{
			float radian_cur = start_angle * Mathf.Deg2Rad;
			float cosA = Mathf.Cos (radian_cur);
			float sinA = Mathf.Sin (radian_cur);

			ringVert [i].x = radius * cosA + position.x - (innerRadius / 2);
			ringVert [i].y = radius * sinA + position.y + innerRadius / 2;
			ringVert [i].z = position.z;
			ringColor [i] = (i <= (percent * 2) && i > 0) ? this.ProgressColor : this.Color;

			ringVert [i + 1].x = innerRadius * cosA + position.x - (innerRadius / 2);
			ringVert [i + 1].y = innerRadius * sinA + position.y + (innerRadius / 2);
			ringVert [i + 1].z = position.z;
			ringColor [i + 1] = (i <= (percent * 2) && i > 0) ? this.ProgressColor : this.Color;

			start_angle -= percentAngle;
		}

		// triangles
		for (int i = 0, vi = 0; i < TRIANGLE_COUNT; i += 6,vi += 2)
		{
			ringTriangle [i] = vi;
			ringTriangle [i + 1] = vi + 3;
			ringTriangle [i + 2] = vi + 1;
			ringTriangle [i + 3] = vi + 2;
			ringTriangle [i + 4] = vi + 3;
			ringTriangle [i + 5] = vi;
		}

		// uv
		for (int i = 0; i < VERTEX_COUNT; i++)
		{
			ringUv [i].x = ringVert [i].x / radius / 2 + 0.5f;
			ringUv [i].y = ringVert [i].z / radius / 2 + 0.5f;
		}

		Mesh mesh = meshFilt.mesh;
		mesh.Clear ();

		mesh.vertices = ringVert;
		mesh.colors = ringColor;
		mesh.triangles = ringTriangle;
		mesh.uv = ringUv;
	}
}
