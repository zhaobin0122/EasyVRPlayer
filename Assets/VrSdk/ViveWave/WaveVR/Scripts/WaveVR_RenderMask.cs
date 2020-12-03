// "WaveVR SDK 
// © 2017 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the WaveVR SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using wvr;
using WVR_Log;

// A GameObject(RenderMask) will be culled if it is not in camera view frustum.  Using the commandBuffer to show it.
public class WaveVR_RenderMask : MonoBehaviour
{
	private static string TAG = "RenderMask";

	class Data
	{
		public WVR_Eye eye;
		public Camera camera;
		public WaveVR_Camera wvrCamera;
		public CommandBuffer cmdBuf;
		public Mesh mesh;

		public Data()
		{
			eye = WVR_Eye.WVR_Eye_Left;
			camera = null;
			wvrCamera = null;
			cmdBuf = null;
			mesh = null;
		}
	}

	private List<Data> data = new List<Data>();
	private bool isGraphicReady = false;
	private bool isCameraReady = false;

	bool CheckCameras()
	{
		if (data == null)
			return false;

		foreach (var d in data)
		{
			if (d == null || d.camera == null || d.wvrCamera == null)
				return false;
		}
		return true;
	}

	bool CheckCommandBuffers()
	{
		if (data == null)
			return false;

		foreach (var d in data)
		{
			if (d == null || d.cmdBuf == null)
				return false;
		}
		return true;
	}

	void PrepareCameras(WaveVR_Render render)
	{
		if (isCameraReady)
			return;

		data.Clear();

		if (render.IsSinglePass)
		{
			if (render.botheyes == null) return;

			Data d = new Data();
			// both
			d.wvrCamera = WaveVR_Render.Instance.botheyes;
			d.eye = WVR_Eye.WVR_Eye_Both;
			d.camera = d.wvrCamera.GetCamera();
			data.Add(d);
		}
		else
		{
			if (render.lefteye == null || render.righteye == null) return;

			Data d = new Data();
			// left
			d.wvrCamera = WaveVR_Render.Instance.lefteye;
			d.eye = WVR_Eye.WVR_Eye_Left;
			d.camera = d.wvrCamera.GetCamera();
			data.Add(d);

			// right
			d = new Data();
			d.eye = WVR_Eye.WVR_Eye_Right;
			d.wvrCamera = WaveVR_Render.Instance.righteye;
			d.camera = d.wvrCamera.GetCamera();
			data.Add(d);
		}

		isCameraReady = true;
	}

	void MyPreCull(Camera cam)
	{
		if (!CheckCommandBuffers())
		{
			createMaskCommandBuffer();
		}
	}

	void MyPreRender(Camera cam)
	{
		if (!CheckCameras() || !CheckCommandBuffers())
			return;
		foreach (var d in data)
		{
			if (d.camera != cam)
				continue;

			removeRenderMaskCommandBuffer(d);
			addRenderMaskCommandBuffer(d);
		}
	}

	void OnConfigurationChanged(WaveVR_Render render)
	{
		PrepareCameras(render);
	}

	void OnEnable()
	{
		if (renderMaskShader || renderMaskMaterial || renderMaskMeshLeft || renderMaskMeshRight || renderMaskMeshBoth)
			Log.w(TAG, "Customized RenderMask. Non-Official.", true);
		StartCoroutine("Initialization");
	}

	void OnDisable()
	{
		StopCoroutine("Initialization");

		Camera.onPreCull -= MyPreCull;
		Camera.onPreRender -= MyPreRender;
		try
		{
			foreach (var d in data)
			{
				removeRenderMaskCommandBuffer(d);
			}

			WaveVR_Render.Instance.onConfigurationChanged -= OnConfigurationChanged;
		}
		catch (NullReferenceException)
		{
			// Camera and Render may be freed before CommandBuffer removing.
		}

		cleanData();
	}

	IEnumerator Initialization()
	{
		WaveVR_Render render = null;

		// Check first.  OnConfigurationChanged may already passed.
		while (!isCameraReady || !isGraphicReady)
		{
			//Log.d(TAG, "Coroutine check: isCameraReady=" + isCameraReady + " isGraphicReady" + isGraphicReady, true);

			render = WaveVR_Render.Instance;
			if (render == null) {
				yield return null;
				continue;
			}

			PrepareCameras(render);

			isGraphicReady = render.IsGraphicReady;

			if (!isCameraReady || !isGraphicReady)
			{
				yield return null;
			}
		}

		Camera.onPreCull += MyPreCull;
		Camera.onPreRender += MyPreRender;
		render.onConfigurationChanged += OnConfigurationChanged;

		Log.d(TAG, "RenderMask initialization finished");
	}

	#region Mask
	// If leave them null, these public variable will be generated at runtime according to the device.  You can override them at your own risk.
	public Shader renderMaskShader = null;
	public Material renderMaskMaterial = null;
	public Mesh renderMaskMeshLeft = null;
	public Mesh renderMaskMeshRight = null;
	public Mesh renderMaskMeshBoth = null;
	private Color32 color = Color.black;

	public void SetMaskColor(Color32 c) {
		Log.w(TAG, "Customized RenderMask. Non-Official color " + color, true);
		color = c;
	}

	private Mesh GetStencilMesh(WVR_Eye eye)
	{
		float[] vertexData = null;
		int[] indexData = null;
		uint vertexCount = 0, triangleCount = 0;

		try
		{
			WaveVR_Utils.WVR_GetStencilMesh(eye, ref vertexCount, ref triangleCount, 0, null, 0, null);
			Log.d(TAG, "vertexCount " + vertexCount + " triangleCount " + triangleCount);

			if (vertexCount <= 0 || vertexCount > 0xFF || triangleCount <= 0 || triangleCount > 0xFF)
			{
				return null;
			}

			vertexData = new float[vertexCount * 3];
			indexData = new int[triangleCount * 3];
			WaveVR_Utils.WVR_GetStencilMesh(eye, ref vertexCount, ref triangleCount, vertexCount * 3, vertexData, triangleCount * 3, indexData);
		}
		catch (EntryPointNotFoundException e)
		{
			Log.e(TAG, "API doesn't exist:\n" + e.Message);
			return null;
		}

		int indicesCount = (int)triangleCount * 3;

		if (indexData == null || vertexData == null)
		{
			Log.e(TAG, "Out of memory");
			return null;
		}

		// create mesh

		float ZDir = 0;
		if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3 ||
			SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2 ||
			SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLCore)
		{
			ZDir = -1;
		}
		else
		{
			if (SystemInfo.usesReversedZBuffer)
				ZDir = 1;
			else
				ZDir = 0;
		}

		Vector3[] verticesUnity = new Vector3[vertexCount];
		int[] indicesUnity = new int[indicesCount];

		for (int i = 0; i < vertexCount; i++)
		{
			verticesUnity[i] = new Vector3(vertexData[3 * i], vertexData[3 * i + 1], ZDir);
		}

		// The mesh from SDK is GL style.  Change to left hand rule.
		for (int i = 0; i < triangleCount; i++)
		{
			int j = i * 3;
			indicesUnity[j] = indexData[j];
			indicesUnity[j + 1] = indexData[j + 2];
			indicesUnity[j + 2] = indexData[j + 1];
		}

		Mesh mesh = new Mesh()
		{
			name = "RenderMask",
			vertices = verticesUnity,
		};
		mesh.SetIndices(indicesUnity, MeshTopology.Triangles, 0);
		Log.d(TAG, "RenderMask " + eye + " is loaded");
		return mesh;
	}

	Mesh GetEyeBothMesh(Mesh l, Mesh r)
	{
		var MeshXOffset = Mathf.Max(Mathf.Abs(l.bounds.max.x), Mathf.Abs(r.bounds.min.x));

		var both = new Mesh();
		both.name = "WaveVR_RenderMask_Both";

		var cil = new CombineInstance();
		cil.mesh = l;
		var matrixL = Matrix4x4.identity;
		matrixL.SetTRS(Vector3.left * MeshXOffset, Quaternion.identity, Vector3.one);
		cil.transform = matrixL;

		var cir = new CombineInstance();
		cir.mesh = r;
		var matrixR = Matrix4x4.identity;
		matrixR.SetTRS(Vector3.right * MeshXOffset, Quaternion.identity, Vector3.one);
		cir.transform = matrixR;

		CombineInstance[] cis = new CombineInstance[] { cil, cir };
		both.CombineMeshes(cis);

		return both;
	}

	public delegate void BeforeCreateMaskCommandBuffer(WaveVR_RenderMask renderMask);

	// If you want to override the Shader, Material, and Mesh, do it here.
	public BeforeCreateMaskCommandBuffer beforeCreateMaskCommandBuffer;

	private void createMaskCommandBuffer()
	{
		WaveVR_Utils.SafeExecuteAllDelegate<BeforeCreateMaskCommandBuffer>(beforeCreateMaskCommandBuffer, a => a(this));

		if (renderMaskShader == null)
			renderMaskShader = Shader.Find("Hidden/WaveVR/RenderMask");

		if (renderMaskMaterial == null && renderMaskShader != null)
		{
			renderMaskMaterial = new Material(renderMaskShader);
			renderMaskMaterial.SetColor("_Color", color);
		}
		if (renderMaskMaterial == null) {
			Log.e(TAG, "RenderMask Shader not found");
			return;
		}

		// You can custom them
		if (renderMaskMeshLeft == null || renderMaskMeshRight == null)
		{
#if UNITY_EDITOR && UNITY_ANDROID
			if (Application.isEditor)
			{
				if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3 ||
					SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2 ||
					SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLCore)
				{
					// Cause Unity error: "Unknown shader count".  Need solve.
					//renderMaskMeshLeft = renderMaskMeshRight = Resources.Load<Mesh>("WaveVR_RenderMask_GL");
				}
				else
				{
					//if (SystemInfo.usesReversedZBuffer)
					//	renderMaskMeshLeft = renderMaskMeshRight = Resources.Load<Mesh>("WaveVR_RenderMask_D3D_ReversedZ");
					//else
					//	renderMaskMeshLeft = renderMaskMeshRight = Resources.Load<Mesh>("WaveVR_RenderMask_D3D");
				}
			}
			else
#endif
			{
				renderMaskMeshLeft = GetStencilMesh(WVR_Eye.WVR_Eye_Left);
				renderMaskMeshRight = GetStencilMesh(WVR_Eye.WVR_Eye_Right);
			}
		}

		if (renderMaskMeshLeft == null || renderMaskMeshRight == null)
		{
			Log.w(TAG, "RenderMask resource not exist. Disable RenderMask.");
			enabled = false;
			return;
		}

		if (renderMaskMeshBoth == null)
		{
			renderMaskMeshBoth = GetEyeBothMesh(renderMaskMeshLeft, renderMaskMeshRight);
			var MeshXOffset = Mathf.Max(Mathf.Abs(renderMaskMeshLeft.bounds.max.x), Mathf.Abs(renderMaskMeshRight.bounds.min.x));
			renderMaskMaterial.SetFloat("MeshXOffset", MeshXOffset);
		}

		foreach (var d in data)
		{
			if (d.eye == WVR_Eye.WVR_Eye_Left)
			{
				var cmdBuf = new CommandBuffer();
				cmdBuf.name = "WVRMaskLeft";
				cmdBuf.DrawMesh(renderMaskMeshLeft, Matrix4x4.identity, renderMaskMaterial, 0, 0);
				d.cmdBuf = cmdBuf;
			}
			else if (d.eye == WVR_Eye.WVR_Eye_Right)
			{
				var cmdBuf = new CommandBuffer();
				cmdBuf.name = "WVRMaskRight";
				cmdBuf.DrawMesh(renderMaskMeshRight, Matrix4x4.identity, renderMaskMaterial, 0, 0);
				d.cmdBuf = cmdBuf;
			}
			else if (d.eye == WVR_Eye.WVR_Eye_Both)
			{
				var cmdBuf = new CommandBuffer();
				cmdBuf.name = "WVRMaskBoth";
				cmdBuf.DrawMesh(renderMaskMeshBoth, Matrix4x4.identity, renderMaskMaterial, 0, 1);
				d.cmdBuf = cmdBuf;
			}
		}
	}

	private void addRenderMaskCommandBuffer(Data d)
	{
		d.camera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, d.cmdBuf);
	}

	// Should do all check before call this function
	private void removeRenderMaskCommandBuffer(Data d)
	{
		try
		{
			d.camera.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, d.cmdBuf);
		}
		catch (MissingReferenceException e)
		{
			// Avoid error if WVR_Render destoried earlier
			Log.e(TAG, e.ToString());
			cleanData();
		}
	}

	private void cleanData()
	{
		data.Clear();

		renderMaskMeshLeft = null;
		renderMaskMeshRight = null;
		renderMaskMeshBoth = null;
		renderMaskShader = null;
		renderMaskMaterial = null;

		isGraphicReady = false;
		isCameraReady = false;
	}
	#endregion
}
