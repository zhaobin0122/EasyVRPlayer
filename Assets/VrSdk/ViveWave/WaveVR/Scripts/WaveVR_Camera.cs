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
using System.Runtime.InteropServices;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using wvr;
using WVR_Log;
using wvr.render.thread;
using wvr.render.gl;

/**
 * To work with this camera's event, you could register your delegate to the Camera.OnXXX.
 * For example,
 * 
 *  public class YourScript : MonoBehaviour
 *  {
 *	  public WVR_Eye eye;
 *	  public void MyPreRender(Camera cam)
 *	  {
 *		  var wvrCam = cam.GetComponent<WaveVR_Camera>();
 *		  if (wvrCam == null)
 *			  return;
 *		  if (wvrCam.eye == eye)
 *		  {
 *			  // Do your actions here.
 *		  }
 *		  // or you can...
 *		  if (wvrCam == WaveVR_Render.Instance.lefteye) {
 *			  // Do your actions here.
 *		  }
 *	  }
 *	  
 *	  void OnEnable() {
 *		  Camera.onPreRender += MyPreRender;
 *	  }
 *	  
 *	  void OnDisable() {
 *		  Camera.onPreRender -= MyPreRender;
 *	  }
 *  }
 *  See also: https://docs.unity3d.com/ScriptReference/Camera-onPreRender.html
**/
[RequireComponent(typeof(Camera))]
public class WaveVR_Camera : MonoBehaviour, IEquatable<Camera>
{
	private static string TAG = "WVR_Camera";
	public WVR_Eye eye = WVR_Eye.WVR_Eye_None;

	private Camera cam;

	void Start()
	{
		cam = GetComponent<Camera>();
	}

	public Camera GetCamera()
	{
		if (cam != null)
			return cam;
		cam = GetComponent<Camera>();
		return cam;
	}

	[System.Obsolete("Use GetCamera() inestad.")]
	public Camera getCamera()
	{
		if (cam != null)
			return cam;
		cam = GetComponent<Camera>();
		return cam;
	}

	public bool Equals(Camera other)
	{
		return cam == other;
	}

	void OnPreRender()
	{
#if UNITY_ANDROID
		if (eye == WVR_Eye.WVR_Eye_Both)
		{
			SinglePassPreRender();
		}
#elif UNITY_STANDALONE
		if (WaveVR.UnityPlayerSettingsStereoRenderingPath == WaveVR_Render.StereoRenderingPath.MultiPass)
		{
			if (isLeft)
			{
				cam.worldToCameraMatrix = WaveVR_Render.Instance.uView[0];
			}
			else
			{
				cam.worldToCameraMatrix = WaveVR_Render.Instance.uView[1];
			}
		} else
		{
			cam.SetStereoViewMatrix(Camera.StereoscopicEye.Left, WaveVR_Render.Instance.uView[0]);
			cam.SetStereoViewMatrix(Camera.StereoscopicEye.Right, WaveVR_Render.Instance.uView[1]);
		}
			
	#endif
	}

	void OnPostRender()
	{
#if UNITY_ANDROID
		if (eye == WVR_Eye.WVR_Eye_Both)
		{
			SinglePassPostRender();
		}
#elif UNITY_STANDALONE
		cam.ResetStereoViewMatrices();
#endif
	}

#if UNITY_ANDROID
	#region render_thread
	private class RenderThreadContext : wvr.render.utils.Message
	{
		public int antialiasing = 0;
		public uint textureId = 0;
		public uint depthId = 0;

		public static void IssueUpdateConfig(RenderThreadSyncObject syncObj, uint textureId, uint depthId, int antialiasing)
		{
			var queue = syncObj.Queue;
			lock (queue)
			{
				var msg = queue.Obtain<RenderThreadContext>();
				msg.textureId = textureId;
				msg.depthId = depthId;
				msg.antialiasing = antialiasing;
				queue.Enqueue(msg);
			}
			syncObj.IssueEvent();
		}

		public static void ReceiveBeforeOpaque(wvr.render.utils.PreAllocatedQueue queue)
		{
#if UNITY_EDITOR && UNITY_ANDROID
			bool isEditor = true;
			if (isEditor) return;
#endif
			if (contextRTOnly.textureId == 0)
			{
				Log.w(TAG, "Single pass textures are not ready");
				return;
			}

			int colorAttachment = (int)UGL.GLenum2.GL_COLOR_ATTACHMENT0;
			int depthAttachment = (int)UGL.GLenum3.GL_DEPTH_STENCIL_ATTACHMENT;
			int target = (int)UGL.GLenum3.GL_DRAW_FRAMEBUFFER;

			UGL.FramebufferTexture2D(target, colorAttachment, (int)UGL.GLenum2.GL_TEXTURE_2D, 0, 0);
			UGL.FramebufferRenderbuffer(target, depthAttachment, (int)UGL.GLenum2.GL_RENDERBUFFER, 0);

			if (contextRTOnly.antialiasing > 1)
			{
				UGL.FramebufferTextureMultisampleMultiviewOVR(target, colorAttachment, contextRTOnly.textureId, 0, contextRTOnly.antialiasing, 0, 2);
				UGL.FramebufferTextureMultisampleMultiviewOVR(target, depthAttachment, contextRTOnly.depthId, 0, contextRTOnly.antialiasing, 0, 2);
			}
			else
			{
				UGL.FramebufferTextureMultiviewOVR(target, colorAttachment, contextRTOnly.textureId, 0, 0, 2);
				UGL.FramebufferTextureMultiviewOVR(target, depthAttachment, contextRTOnly.depthId, 0, 0, 2);
			}

			// The Unity engine will clear but only the left eye.  We have to clear it by ourself.
			UGL.Clear((uint)(UGL.GLenum2.GL_COLOR_BUFFER_BIT | UGL.GLenum2.GL_DEPTH_BUFFER_BIT | UGL.GLenum2.GL_STENCIL_BUFFER_BIT));

			contextRTOnly.textureId = 0;
			contextRTOnly.depthId = 0;
		}

		public static void ReceiveUpdateConfig(wvr.render.utils.PreAllocatedQueue queue)
		{
			lock (queue)
			{
				var msg = (RenderThreadContext)queue.Dequeue();
				msg.CopyTo(contextRTOnly);
				queue.Release(msg);
			}
		}

		public void CopyTo(RenderThreadContext dest) {
			dest.antialiasing = antialiasing;
			dest.textureId = textureId;
			dest.depthId = depthId;
		}
	}

	private static readonly RenderThreadContext contextRTOnly = new RenderThreadContext();
	private static readonly RenderThreadSyncObject RTSOBeforeOpaque = new RenderThreadSyncObject(RenderThreadContext.ReceiveBeforeOpaque);
	private static readonly RenderThreadSyncObject RTSOUpdateConfig = new RenderThreadSyncObject(RenderThreadContext.ReceiveUpdateConfig);

	#endregion // render_thread

	#region single_pass

	//Shader Variables used for single-pass stereo rendering
	private Matrix4x4 unity_SingleCullMatrixP;
	private readonly Matrix4x4[] unity_StereoMatrixP = new Matrix4x4[2] { Matrix4x4.identity, Matrix4x4.identity };
	private readonly Matrix4x4[] unity_StereoMatrixInvP = new Matrix4x4[2] { Matrix4x4.identity, Matrix4x4.identity };
	private readonly Matrix4x4[] unity_StereoWorldToCamera = new Matrix4x4[2] { Matrix4x4.identity, Matrix4x4.identity };
	private readonly Matrix4x4[] unity_StereoCameraToWorld = new Matrix4x4[2] { Matrix4x4.identity, Matrix4x4.identity };
	private readonly Matrix4x4[] unity_StereoMatrixVP = new Matrix4x4[2] { Matrix4x4.identity, Matrix4x4.identity };

	private readonly Vector4[] eyesOffset = new Vector4[2];
	private readonly Matrix4x4[] eyesOffsetMatrix = new Matrix4x4[2];
	private readonly Matrix4x4[] eyesOffsetMatrixInv = new Matrix4x4[2];

	private readonly Matrix4x4[] skybox_MatrixVP = new Matrix4x4[2] { Matrix4x4.identity, Matrix4x4.identity };

	private readonly Vector4[] unity_StereoScaleOffset = new Vector4[2] { new Vector4(1f, 1f, 0f, 0f), new Vector4(1f, 1f, 0f, 0f) };
	private readonly Vector4[] stereoWorldSpaceCameraPos = new Vector4[2] { Vector4.zero, Vector4.zero };

	private void SetStereoViewAndCullingMatrix(Matrix4x4 left, Matrix4x4 right)
	{
#if UNITY_2017_1_OR_NEWER
		cam.SetStereoViewMatrix(Camera.StereoscopicEye.Left, left);
		cam.SetStereoViewMatrix(Camera.StereoscopicEye.Right, right);
		if (!cam.areVRStereoViewMatricesWithinSingleCullTolerance)
		{
			Log.e(TAG, "The Camera.areVRStereoViewMatricesWithinSingleCullTolerance are false.  SinglePass may not enabled.");
		}
#endif
	}

	void DebugLogMatrix(ref Matrix4x4 m, string name)
	{
		Log.w(TAG, name + ":\n" + 
			"/ "  + m.m00 + " " + m.m01 + " " + m.m02 + " " + m.m03 + " \\\n" + 
			"| "  + m.m10 + " " + m.m11 + " " + m.m12 + " " + m.m13 + " |\n" + 
			"| "  + m.m20 + " " + m.m21 + " " + m.m22 + " " + m.m23 + " |\n" +
			"\\ " + m.m30 + " " + m.m31 + " " + m.m32 + " " + m.m33 + " /");
	}

	CommandBuffer cmdBufBeforeForwardOpaque, cmdBufBeforeSkybox, cmdBufAfterSkybox;

	private bool needInitPropertyId = true;
	private int id_unity_StereoCameraProjection = 0;
	private int id_unity_StereoCameraInvProjection = 0;
	private int id_unity_StereoMatrixP = 0;
	private int id_unity_StereoCameraToWorld = 0;
	private int id_unity_StereoWorldToCamera = 0;
	private int id_unity_StereoWorldSpaceCameraPos = 0;
	private int id_unity_StereoMatrixV = 0;
	private int id_unity_StereoMatrixInvV = 0;
	private int id_unity_StereoMatrixVP = 0;
	private int id_unity_StereoScaleOffset = 0;

	void PrepareCommandBuffers()
	{
		// Make sure all command buffer can run in editor mode
		if (cmdBufBeforeForwardOpaque == null)
		{
			cmdBufBeforeForwardOpaque = new CommandBuffer();
#if UNITY_EDITOR && UNITY_ANDROID
			if (!Application.isEditor)
#endif
				RTSOBeforeOpaque.IssueInCommandBuffer(cmdBufBeforeForwardOpaque);
			//cmdBufBeforeForwardOpaque.ClearRenderTarget(true, true, cam.backgroundColor);  // Oops, this line cause black screen when using viewport (Camer.rect).
			cmdBufBeforeForwardOpaque.name = "SinglePassPrepare";
		}
		cam.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, cmdBufBeforeForwardOpaque);
		cam.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, cmdBufBeforeForwardOpaque);

		//The workaround for Skybox rendering
		//Since Unity5, skybox rendering after forward opaque
		//As skybox need a particular MatrixVP, two CommandBuffer used to handle this.
		//The MatrixVP must be changed back after skybox rendering.
		if (cmdBufAfterSkybox == null)
			cmdBufAfterSkybox = new CommandBuffer();

		cam.RemoveCommandBuffer(CameraEvent.AfterSkybox, cmdBufAfterSkybox);

		cmdBufAfterSkybox.Clear();
		cmdBufAfterSkybox.SetGlobalMatrixArray(id_unity_StereoMatrixVP, unity_StereoMatrixVP);
		cmdBufAfterSkybox.name = "SinglePassAfterSkyBox";
		cam.AddCommandBuffer(CameraEvent.AfterSkybox, cmdBufAfterSkybox);

		//Skybox View Matrix should be at world zero point.
		//As in OpenGL, camera's forward is the negative Z axis
		if (cmdBufBeforeSkybox == null)
			cmdBufBeforeSkybox = new CommandBuffer();

		Matrix4x4 viewMatrix1 = Matrix4x4.LookAt(Vector3.zero, cam.transform.forward, cam.transform.up) * Matrix4x4.Scale(new Vector3(1, 1, -1));
		//Change it from column major to row major.
		viewMatrix1 = viewMatrix1.transpose;
		Matrix4x4 proj0 = unity_StereoMatrixP[0];
		Matrix4x4 proj1 = unity_StereoMatrixP[1];
		//Trick here. I supporse skybox doesn't need clip in Projection Matrix
		//And m22 and m23 is calculated by clip near/far, -1 is the default value of m22.
		proj0.m22 = -1.0f;
		proj1.m22 = -1.0f;

		M4Multiply(ref skybox_MatrixVP[0], ref proj0, ref viewMatrix1);
		M4Multiply(ref skybox_MatrixVP[1], ref proj1, ref viewMatrix1);

		cam.RemoveCommandBuffer(CameraEvent.BeforeSkybox, cmdBufBeforeSkybox);

		//The MatrixVP should be set before skybox rendering.
		cmdBufBeforeSkybox.Clear();
		cmdBufBeforeSkybox.SetGlobalMatrixArray(id_unity_StereoMatrixVP, skybox_MatrixVP);
		cmdBufBeforeSkybox.name = "SinglePassBeforeSkybox";

		cam.AddCommandBuffer(CameraEvent.BeforeSkybox, cmdBufBeforeSkybox);
	}

	void SinglePassPreRender()
	{
		if (needInitPropertyId)
		{
			id_unity_StereoCameraProjection = Shader.PropertyToID("unity_StereoCameraProjection");
			id_unity_StereoCameraInvProjection = Shader.PropertyToID("unity_StereoCameraInvProjection");
			id_unity_StereoMatrixP = Shader.PropertyToID("unity_StereoMatrixP");
			id_unity_StereoCameraToWorld = Shader.PropertyToID("unity_StereoCameraToWorld");
			id_unity_StereoWorldToCamera = Shader.PropertyToID("unity_StereoWorldToCamera");
			id_unity_StereoWorldSpaceCameraPos = Shader.PropertyToID("unity_StereoWorldSpaceCameraPos");
			id_unity_StereoMatrixV = Shader.PropertyToID("unity_StereoMatrixV");
			id_unity_StereoMatrixInvV = Shader.PropertyToID("unity_StereoMatrixInvV");
			id_unity_StereoMatrixVP = Shader.PropertyToID("unity_StereoMatrixVP");
			id_unity_StereoScaleOffset = Shader.PropertyToID("unity_StereoScaleOffset");

			needInitPropertyId = false;
		}

		var texturepool = WaveVR_Render.Instance.textureManager.both;
		if (texturepool == null)
			return;
		RenderThreadContext.IssueUpdateConfig(RTSOUpdateConfig,
			(uint)texturepool.currentPtr,
			(uint)texturepool.currentDepthPtr,
			cam.allowMSAA ? QualitySettings.antiAliasing : 0);

		// TODO The matrix process here is too slow and cause many GL.Allocate().

		//Unity will not handle these Stereo shader variables for us, so we have to set it all by ourselves

		Shader.SetGlobalMatrixArray(id_unity_StereoCameraProjection, unity_StereoMatrixP);
		Shader.SetGlobalMatrixArray(id_unity_StereoCameraInvProjection, unity_StereoMatrixInvP);
		Shader.SetGlobalMatrixArray(id_unity_StereoMatrixP, unity_StereoMatrixP);

		//Since eyes are moving, so below variables need to re-calculate every frame.
		Matrix4x4 world2Camera = cam.worldToCameraMatrix;  // View matrix
		Matrix4x4 camera2World = cam.cameraToWorldMatrix;

		M4Multiply(ref unity_StereoCameraToWorld[0], ref camera2World, ref eyesOffsetMatrix[0]);
		M4Multiply(ref unity_StereoCameraToWorld[1], ref camera2World, ref eyesOffsetMatrix[1]);

		M4Multiply(ref unity_StereoWorldToCamera[0], ref eyesOffsetMatrixInv[0], ref world2Camera);
		M4Multiply(ref unity_StereoWorldToCamera[1], ref eyesOffsetMatrixInv[1], ref world2Camera);

		// TODO Need to know if this can help set shader...
		//SetStereoViewAndCullingMatrix(unity_StereoWorldToCamera[0], unity_StereoWorldToCamera[1]);

		// Put later than SetStereoViewMatrix() to avoid be overrided.
		Shader.SetGlobalMatrixArray(id_unity_StereoCameraToWorld, unity_StereoCameraToWorld);
		Shader.SetGlobalMatrixArray(id_unity_StereoWorldToCamera, unity_StereoWorldToCamera);

		//So the camera positons
		Vector4 campos = cam.transform.position;
		V4Add(ref stereoWorldSpaceCameraPos[0], ref campos, ref eyesOffset[0]);
		V4Add(ref stereoWorldSpaceCameraPos[1], ref campos, ref eyesOffset[1]);

		Shader.SetGlobalVectorArray(id_unity_StereoWorldSpaceCameraPos, stereoWorldSpaceCameraPos);

		//camera.worldToCameraMatrix is the view matrix
		Shader.SetGlobalMatrixArray(id_unity_StereoMatrixV, unity_StereoWorldToCamera);
		Shader.SetGlobalMatrixArray(id_unity_StereoMatrixInvV, unity_StereoCameraToWorld);

		//MatrixVP is the value UNITY_MATRIX_VP used in shader
		M4Multiply(ref unity_StereoMatrixVP[0], ref unity_StereoMatrixP[0], ref unity_StereoWorldToCamera[0]);
		M4Multiply(ref unity_StereoMatrixVP[1], ref unity_StereoMatrixP[1], ref unity_StereoWorldToCamera[1]);

		Shader.SetGlobalMatrixArray(id_unity_StereoMatrixVP, unity_StereoMatrixVP);

		Shader.SetGlobalVectorArray(id_unity_StereoScaleOffset, unity_StereoScaleOffset);

		PrepareCommandBuffers();
	}

	void SinglePassPostRender()
	{
		cam.ResetStereoViewMatrices();
	}
	#endregion
#endif  // UNITY_ANDROID

	public void SetEyesPosition(Vector3 left, Vector3 right)
	{
#if UNITY_ANDROID
		eyesOffset[0] = left;
		eyesOffset[1] = right;
		eyesOffsetMatrix[0] = Matrix4x4.TRS(left, Quaternion.identity, transform.localScale);
		eyesOffsetMatrix[1] = Matrix4x4.TRS(right, Quaternion.identity, transform.localScale);
		eyesOffsetMatrixInv[0] = eyesOffsetMatrix[0].inverse;
		eyesOffsetMatrixInv[1] = eyesOffsetMatrix[1].inverse;
#endif
	}

	public void SetStereoCullingMatrix()
	{
#if UNITY_ANDROID
		cam.cullingMatrix = unity_SingleCullMatrixP * cam.worldToCameraMatrix;
#endif
	}

	public void SetStereoProjectionMatrix(Matrix4x4 left, Matrix4x4 right, Matrix4x4 cull)
	{
#if UNITY_ANDROID
		GetCamera();

		cam.ResetStereoProjectionMatrices();

		//cam.SetStereoProjectionMatrix(Camera.StereoscopicEye.Left, left);
		//cam.SetStereoProjectionMatrix(Camera.StereoscopicEye.Right, right);

		unity_StereoMatrixP[0] = left;
		unity_StereoMatrixInvP[0] = left.inverse;

		unity_StereoMatrixP[1] = right;
		unity_StereoMatrixInvP[1] = right.inverse;

		unity_SingleCullMatrixP = cull;
		DebugLogMatrix(ref unity_SingleCullMatrixP, "unity_SingleCullMatrixP");
#endif
	}

#if UNITY_ANDROID
	// Hope this could be faster because of no GC.Alloc().
	static void M4Multiply(ref Matrix4x4 mout, ref Matrix4x4 lhs, ref Matrix4x4 rhs)
	{
		mout.m00 = lhs.m00 * rhs.m00 + lhs.m01 * rhs.m10 + lhs.m02 * rhs.m20 + lhs.m03 * rhs.m30;
		mout.m10 = lhs.m10 * rhs.m00 + lhs.m11 * rhs.m10 + lhs.m12 * rhs.m20 + lhs.m13 * rhs.m30;
		mout.m20 = lhs.m20 * rhs.m00 + lhs.m21 * rhs.m10 + lhs.m22 * rhs.m20 + lhs.m23 * rhs.m30;
		mout.m30 = lhs.m30 * rhs.m00 + lhs.m31 * rhs.m10 + lhs.m32 * rhs.m20 + lhs.m33 * rhs.m30;

		mout.m01 = lhs.m00 * rhs.m01 + lhs.m01 * rhs.m11 + lhs.m02 * rhs.m21 + lhs.m03 * rhs.m31;
		mout.m11 = lhs.m10 * rhs.m01 + lhs.m11 * rhs.m11 + lhs.m12 * rhs.m21 + lhs.m13 * rhs.m31;
		mout.m21 = lhs.m20 * rhs.m01 + lhs.m21 * rhs.m11 + lhs.m22 * rhs.m21 + lhs.m23 * rhs.m31;
		mout.m31 = lhs.m30 * rhs.m01 + lhs.m31 * rhs.m11 + lhs.m32 * rhs.m21 + lhs.m33 * rhs.m31;

		mout.m02 = lhs.m00 * rhs.m02 + lhs.m01 * rhs.m12 + lhs.m02 * rhs.m22 + lhs.m03 * rhs.m32;
		mout.m12 = lhs.m10 * rhs.m02 + lhs.m11 * rhs.m12 + lhs.m12 * rhs.m22 + lhs.m13 * rhs.m32;
		mout.m22 = lhs.m20 * rhs.m02 + lhs.m21 * rhs.m12 + lhs.m22 * rhs.m22 + lhs.m23 * rhs.m32;
		mout.m32 = lhs.m30 * rhs.m02 + lhs.m31 * rhs.m12 + lhs.m32 * rhs.m22 + lhs.m33 * rhs.m32;

		mout.m03 = lhs.m00 * rhs.m03 + lhs.m01 * rhs.m13 + lhs.m02 * rhs.m23 + lhs.m03 * rhs.m33;
		mout.m13 = lhs.m10 * rhs.m03 + lhs.m11 * rhs.m13 + lhs.m12 * rhs.m23 + lhs.m13 * rhs.m33;
		mout.m23 = lhs.m20 * rhs.m03 + lhs.m21 * rhs.m13 + lhs.m22 * rhs.m23 + lhs.m23 * rhs.m33;
		mout.m33 = lhs.m30 * rhs.m03 + lhs.m31 * rhs.m13 + lhs.m32 * rhs.m23 + lhs.m33 * rhs.m33;
	}

	// Hope this could be faster because of no GC.Alloc().
	static void V4Add(ref Vector4 vout, ref Vector4 lhs, ref Vector4 rhs)
	{
		vout.x = lhs.x + rhs.x;
		vout.y = lhs.y + rhs.y;
		vout.z = lhs.z + rhs.z;
		vout.w = lhs.w + rhs.w;
	}
#endif  // UNITY_ANDROID

#if UNITY_STANDALONE
	private bool isLeft = false;

	void Update()
	{
		isLeft = true;
	}

	private void OnRenderImage(RenderTexture src, RenderTexture dst)
	{
		Graphics.Blit(src, dst);
		//Debug.Log("Width: " + src.width + " Height: " + src.height);

		if (WaveVR_Render.Instance.leftTexPtr == null || WaveVR_Render.Instance.rightTexPtr == null)
		{
			StoreRenderTextureToSDK(dst);
		}
		else
		{
			if (WaveVR.UnityPlayerSettingsStereoRenderingPath == WaveVR_Render.StereoRenderingPath.MultiPass)
				MultiPassSubmit(src, dst);
			else
				SinglePassSubmit(src, dst);
		}
	}

	private void MultiPassSubmit(RenderTexture src, RenderTexture dst)
	{
		if (isLeft == false)
		{
			int eventID = (int)WaveVR.Instance.frameInx % 100;
			GL.IssuePluginEvent(WaveVR_Utils.GetRenderEventFunc(), (int)WaveVR_Utils.RENDEREVENTID_Wait_Get_Poses);
			GL.IssuePluginEvent(WaveVR_Utils.GetRenderEventFunc(), (int)WaveVR_Utils.RENDEREVENTID_SubmitL_Index_Min + (int)eventID);
			GL.IssuePluginEvent(WaveVR_Utils.GetRenderEventFunc(), (int)WaveVR_Utils.RENDEREVENTID_SubmitR_Index_Min + (int)eventID);
		}

		isLeft = false;
	}


	private void SinglePassSubmit(RenderTexture src, RenderTexture dst)
	{
		int eventID = (int)WaveVR.Instance.frameInx % 100;
		GL.IssuePluginEvent(WaveVR_Utils.GetRenderEventFunc(), (int)WaveVR_Utils.RENDEREVENTID_Wait_Get_Poses);
		GL.IssuePluginEvent(WaveVR_Utils.GetRenderEventFunc(), (int)WaveVR_Utils.RENDEREVENTID_SubmitL_Index_Min + (int)eventID);
		GL.IssuePluginEvent(WaveVR_Utils.GetRenderEventFunc(), (int)WaveVR_Utils.RENDEREVENTID_SubmitR_Index_Min + (int)eventID);
	}

	public void OnPreCull()
	{
		WaveVR_Render.Instance.OnUpdateFrame();
	}

	private void StoreRenderTextureToSDK(RenderTexture dst)
	{
		if (WaveVR.UnityPlayerSettingsStereoRenderingPath == WaveVR_Render.StereoRenderingPath.MultiPass)
		{
			if (isLeft)
			{
				WaveVR_Render.Instance.leftTexPtr = new System.IntPtr[1] { dst.GetNativeTexturePtr() };
				Interop.WVR_StoreRenderTextures(WaveVR_Render.Instance.leftTexPtr, 1, true, WVR_TextureTarget.WVR_TextureTarget_2D);
				isLeft = false;
			}
			else
			{
				WaveVR_Render.Instance.rightTexPtr = new System.IntPtr[1] { dst.GetNativeTexturePtr() };
				Interop.WVR_StoreRenderTextures(WaveVR_Render.Instance.rightTexPtr, 1, false, WVR_TextureTarget.WVR_TextureTarget_2D);
			}
		}
		else if (WaveVR.UnityPlayerSettingsStereoRenderingPath == WaveVR_Render.StereoRenderingPath.SinglePass)
		{
			WaveVR_Render.Instance.leftTexPtr = new System.IntPtr[1] { dst.GetNativeTexturePtr() };
			Interop.WVR_StoreRenderTextures(WaveVR_Render.Instance.leftTexPtr, 1, true, WVR_TextureTarget.WVR_TextureTarget_2D);

			WaveVR_Render.Instance.rightTexPtr = new System.IntPtr[1] { dst.GetNativeTexturePtr() };
			Interop.WVR_StoreRenderTextures(WaveVR_Render.Instance.rightTexPtr, 1, false, WVR_TextureTarget.WVR_TextureTarget_2D);
		}
	}
#endif
}

