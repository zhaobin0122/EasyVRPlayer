// "WaveVR SDK 
// © 2017 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the WaveVR SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using UnityEngine;
using wvr;

[RequireComponent(typeof(Camera))]
public class WaveVR_Distortion : MonoBehaviour
{
#if UNITY_EDITOR && UNITY_ANDROID
	public class Config
	{
		public Mesh mesh;
		public Rect rect;
		public Material material;
		public bool eye;
		public Vector2 center;
	}
	private static wvr.distortion.Config current;
	private wvr.distortion.Config Cleft, Cright;
	private Camera cam;

	Vector2 GetCenter(WVR_Eye eye)
	{
		Vector2 o;
		if (Application.isEditor)
		{
			o = new Vector2(0.5f, 0.5f);
		}
		else
		{
			float l = 0f, r = 0f, t = 0f, b = 0f;
			Interop.WVR_GetClippingPlaneBoundary(eye, ref l, ref r, ref t, ref b);
			o = new Vector2(-l / (r - l), t / (t - b));
		}
		return o;
	}

	void Awake() {
		if (Application.isEditor)
			return;
	}

	void Start()
	{
		init();
		reset();
	}

	void reset()
	{
		// NOTE: On Unity version 5.6.03f, If the antiAliasing was enabled on the
		// main window surface with front buffer rendering in editor mode. The view will
		// render black.  And HDR has the same problem.  Disable them for Editor mode.
#if UNITY_5_6_OR_NEWER && (UNITY_EDITOR && UNITY_ANDROID)
			cam.allowMSAA = false;
			cam.allowHDR = false;
#endif
	}

	[Tooltip("If you need see the original result without distortion, use this checkbox.")]
	public bool NoDistortion = false;
	Mesh FlatMesh = null;

	public void init()
	{
		wvr.distortion.Distortion obj = new wvr.distortion.Distortion();
		Cleft = new wvr.distortion.Config();
		Cleft.eye = true;
		Cleft.rect = new Rect(0, 0, 0.5f, 1);
		Cleft.expandRatio = 1;
		Cleft.center = GetCenter(WVR_Eye.WVR_Eye_Left);
		Cleft.mesh = obj.createMesh(Cleft);
		Cleft.mesh.name = "left_eye_distortion";
		Cright = new wvr.distortion.Config();
		Cright.eye = false;
		Cright.rect = new Rect(0.5f, 0, 0.5f, 1);
		Cright.expandRatio = 1;
		Cright.center = GetCenter(WVR_Eye.WVR_Eye_Right);
		Cright.mesh = obj.createMesh(Cright);
		Cright.mesh.name = "right_eye_distortion";

		Shader shader = Resources.Load<Shader>("WaveVR_Distortion");
		if (shader == null)
		{
			Debug.LogError("Can't find WaveVR/Distortion shader");
		}
		var material = new Material(shader);
		Cleft.material = material;
		Cright.material = material;

		cam = GetComponent<Camera>();
		cam.orthographic = true;
		cam.orthographicSize = 1;
		cam.cullingMask = 0;
		cam.backgroundColor = Color.black;
		cam.projectionMatrix = Matrix4x4.identity;
		cam.enabled = false;

		current = Cright;

		FlatMesh = new Mesh();
		FlatMesh.vertices = new Vector3 [] { new Vector3(-1, 1), new Vector3(1, 1), new Vector3(1, -1), new Vector3(-1, -1) };
		FlatMesh.uv = new Vector2[] { new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0) };
		FlatMesh.SetIndices(new int[] { 0, 1, 2, 0, 2, 3 }, MeshTopology.Triangles, 0);
	}

	void OnPostRender()
	{
		if (Camera.current != cam)
			return;
		Matrix4x4 mat = Matrix4x4.identity;

		current.material.SetPass(NoDistortion ? 1 : 0);
		if (NoDistortion)
			Graphics.DrawMeshNow(FlatMesh, mat);
		else
			Graphics.DrawMeshNow(current.mesh, mat);
	}

	public void RenderEye(WVR_Eye eye, RenderTexture texture)
	{
		reset();
		bool isleft = eye == WVR_Eye.WVR_Eye_Left;
		WaveVR_Utils.Trace.BeginSection(isleft ? "Distortion_WVR_Eye_Left" : "Distortion_WVR_Eye_Right");
		current = isleft ? Cleft : Cright;
		current.material.mainTexture = texture;

		cam.enabled = true;
		// When VR mode is enabled, the camera clean will not follow camera.rect.  Not to clear when drawing right eyes.
		cam.clearFlags = isleft ? CameraClearFlags.SolidColor : CameraClearFlags.Nothing;
		cam.rect = current.rect;
		cam.Render();
		cam.enabled = false;
		WaveVR_Utils.Trace.EndSection();
	}
#endif
}
