// "WaveVR SDK
// © 2017 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the WaveVR SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

#pragma warning disable 0219
#pragma warning disable 0414

using UnityEngine;
using wvr;
using System;
using WVR_Log;

/// <summary>
/// Draws a pointer of controller to indicate to which object is pointed.
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
public class WaveVR_ControllerPointer : MonoBehaviour {
	private const string LOG_TAG = "WaveVR_ControllerPointer";
	private void PrintDebugLog(string msg)
	{
		if (Log.EnableDebugLog)
			Log.d (LOG_TAG, msg, true);
	}

	public WaveVR_Controller.EDeviceType device;

	#region Variables Setter
	public bool ShowPointer = true;						 // true: show pointer, false: remove pointer
	public bool Blink = false;
	public float PointerOuterDiameterMin = 0.01f;		   // Current outer diameters of the pointer, before distance multiplication.
	[HideInInspector]
	public float PointerOuterDiameter = 0.024f;			   // Current outer diameters of the pointer, before distance multiplication.
	/// <summary>
	/// True: use defaultTexture,
	/// False: use CustomTexture
	/// </summary>
	public bool UseDefaultTexture = true;
	private const string defaultPointerResource_Texture = "focused_dot";
	private Texture2D defaultTexture = null;
	public Texture2D CustomTexture = null;
	[HideInInspector]
	public string TextureName = null;

	private const float pointerDistanceMin = 0.5f;		// Min length of Beam
	[HideInInspector]
	public const float pointerDistanceMax = 100.0f;		// Max length of Beam + 0.5m
	[HideInInspector]
	public float PointerDistanceInMeters = 1.3f;		// Current distance of the pointer (in meters) = beam.endOffset (0.8) + 0.5

	public bool useTexture = true;
	private MeshFilter pointerMeshFilter = null;
	private Mesh pointerMesh;

	/// <summary>
	/// Material resource of pointer.
	/// It contains shader **WaveVR/CtrlrPointer** and there are 5 attributes can be changed in runtime:
	/// <para>
	/// - _OuterDiameter
	/// - _DistanceInMeters
	/// - _MainTex
	/// - _Color
	/// - _useTexture
	///
	/// If _useTexture is set (default), the texture assign in _MainTex will be used.
	/// </summary>
	private const string defaultPointerResource_Material = "ControllerPointer";
	private Material pointerMaterial = null;
	private Material pointerMaterialInstance = null;

	private Color colorFactor = Color.white;			   // The color variable of the pointer
	[HideInInspector]
	public Color PointerColor = Color.white;			   // #FFFFFFFF
	[HideInInspector]
	public Color borderColor = new Color(119, 119, 119, 255);	  // #777777FF
	[HideInInspector]
	public Color focusColor = new Color(255, 255, 255, 255);	   // #FFFFFFFF
	[HideInInspector]
	public Color focusBorderColor = new Color(119, 119, 119, 255); // #777777FF

	private const int PointerRenderQueueMin = 1000;
	private const int PointerRenderQueueMax = 5000;
	public int PointerRenderQueue = PointerRenderQueueMax;
	#endregion

	#region OEM CONFIG JSON parser
	/**
	 * OEM Config
	 * \"pointer\": {
	   \"diameter\": 0.01,
	   \"distance\": 1.3,
	   \"use_texture\": true,
	   \"color\": \"#FFFFFFFF\",
	   \"border_color\": \"#777777FF\",
	   \"focus_color\": \"#FFFFFFFF\",
	   \"focus_border_color\": \"#777777FF\",
	   \"texture_name\":  null,
	   \"Blink\": false
	   },
	 **/
	private void ReadJsonValues()
	{
		string json_values = WaveVR_Utils.OEMConfig.getControllerConfig ();

		if (!json_values.Equals(""))
		{
			try
			{
				SimpleJSON.JSONNode jsNodes = SimpleJSON.JSONNode.Parse(json_values);
				string node_value = "";
				node_value = jsNodes["pointer"]["diameter"].Value;
				if (!node_value.Equals("") && IsFloat(node_value) == true)
					this.PointerOuterDiameterMin = float.Parse(node_value);

				node_value = jsNodes["pointer"]["distance"].Value;
				if (!node_value.Equals("") && IsFloat(node_value) == true)
					this.PointerDistanceInMeters = float.Parse(node_value);

				node_value = jsNodes["pointer"]["use_texture"].Value;
				if (!node_value.Equals("") && IsBoolean(node_value) == true)
					this.useTexture = bool.Parse(node_value);

				if (node_value.ToLower().Equals("false"))
				{
					PrintDebugLog ("ReadJsonValues() " + this.device + ", controller_pointer_use_texture = false, create texture");
					if (this.pointerMaterialInstance != null)
					{
						node_value = jsNodes["pointer"]["color"].Value;
						if (!node_value.Equals(""))
							this.PointerColor = StringToColor32(node_value,0);

						node_value = jsNodes["pointer"]["border_color"].Value;
						if (!node_value.Equals(""))
							this.borderColor = StringToColor32(node_value,1);

						node_value = jsNodes["pointer"]["focus_color"].Value;
						if (!node_value.Equals(""))
							this.focusColor = StringToColor32(node_value,2);

						node_value = jsNodes["pointer"]["focus_border_color"].Value;
						if (!node_value.Equals(""))
							this.focusBorderColor = StringToColor32(node_value,3);
					}
				}
				else
				{
					PrintDebugLog ("ReadJsonValues() " + this.device + ", controller_pointer_use_texture = true");
					node_value = jsNodes["pointer"]["pointer_texture_name"].Value;
					if (!node_value.Equals(""))
						this.TextureName = node_value;
				}


				node_value = jsNodes["pointer"]["Blink"].Value;
				if (!node_value.Equals("") && IsBoolean(node_value) == true)
					this.Blink = bool.Parse(node_value);
				node_value = jsNodes["pointer"]["use_texture"].Value;
				if (!node_value.Equals("") && IsBoolean(node_value) == true)
					this.useTexture = bool.Parse(node_value);
				PrintDebugLog("ReadJsonValues() " + this.device
					+ ", diameter: " + this.PointerOuterDiameterMin
					+ ", distance: " + this.PointerDistanceInMeters
					+ ", use_texture: " + this.useTexture
					+ ", color: " + this.PointerColor
					+ ", pointer_texture_name: " + this.TextureName
					+ ", Blink: " + this.Blink);
			}
			catch (Exception e) {
				Log.e(LOG_TAG, e.ToString(), true);
			}
		}
	}

	private bool IsBoolean(string value)
	{
		try
		{
			bool i = Convert.ToBoolean(value);
			PrintDebugLog (value + " Convert to bool success: " + i.ToString());
			return true;
		}
		catch (Exception e)
		{
			Log.e(LOG_TAG, value + " Convert to bool failed: " + e.ToString(), true);
			return false;
		}
	}

	private bool IsFloat(string value)
	{
		try
		{
			float i = Convert.ToSingle(value);
			PrintDebugLog (value + " Convert to float success: " + i.ToString());
			return true;
		}
		catch (Exception e)
		{
			Log.e(LOG_TAG, value + " Convert to float failed: " + e.ToString(), true);
			return false;
		}
	}

	private bool IsNumeric(string value)
	{
		try
		{
			int i = Convert.ToInt32(value);
			PrintDebugLog (value + " Convert to int success: " + i.ToString());
			return true;
		}
		catch (Exception e)
		{
			Log.e(LOG_TAG, value + " Convert to Int failed: " + e.ToString(), true);
			return false;
		}
	}

	private Color32 StringToColor32(string color_string , int value)
	{
		try
		{
			byte[] _color_r = BitConverter.GetBytes(Convert.ToInt32(color_string.Substring(1, 2), 16));
			byte[] _color_g = BitConverter.GetBytes(Convert.ToInt32(color_string.Substring(3, 2), 16));
			byte[] _color_b = BitConverter.GetBytes(Convert.ToInt32(color_string.Substring(5, 2), 16));
			byte[] _color_a = BitConverter.GetBytes(Convert.ToInt32(color_string.Substring(7, 2), 16));

			return new Color32(_color_r[0], _color_g[0], _color_b[0], _color_a[0]);
		}
		catch (Exception e)
		{
			Log.e(LOG_TAG, "StringToColor32: " + e.ToString(), true);
			switch (value)
			{
			case 1:
				return new Color(119, 119, 119, 255);
			case 2:
				return new Color(255, 255, 255, 255);
			case 3:
				return new Color(119, 119, 119, 255);
			}
			return Color.white;
		}
	}
	#endregion

	private int reticleSegments = 20;

	[HideInInspector]
	public float kpointerGrowthAngle = 90f;				 // Angle at which to expand the pointer when intersecting with an object (in degrees).

	private float colorFlickerTime = 0.0f;				  // The color flicker time

	#region MonoBehaviour overrides
	void Awake()
	{
	}

	void Start()
	{
	}

	private bool isPointerEnabled = false;
	void OnEnable ()
	{
		if (!isPointerEnabled)
		{
			// Load default pointer material resource and create instance.
			this.pointerMaterial = Resources.Load (defaultPointerResource_Material) as Material;
			if (this.pointerMaterial != null)
				this.pointerMaterialInstance = Instantiate<Material> (this.pointerMaterial);
			if (this.pointerMaterialInstance == null)
				PrintDebugLog ("OnEnable() " + this.device + ", Can NOT load default material");
			else
				PrintDebugLog ("OnEnable() " + this.device + ", controller pointer material: " + this.pointerMaterialInstance.name);

			// Load default pointer texture resource.
			// If developer does not specify custom texture, default texture will be used.
			this.defaultTexture = (Texture2D)Resources.Load (defaultPointerResource_Texture);
			if (this.defaultTexture == null)
				Log.e (LOG_TAG, "OnEnable() Can NOT load default texture", true);

			// Get MeshFilter instance.
			this.pointerMeshFilter = gameObject.GetComponent<MeshFilter>();
			if (this.pointerMeshFilter == null)
				this.pointerMeshFilter = gameObject.AddComponent<MeshFilter>();

			// Get Quad mesh as default pointer mesh.
			// If developer does not use texture, pointer mesh will be created in CreatePointerMesh()
			GameObject _primGO = GameObject.CreatePrimitive (PrimitiveType.Quad);
			this.pointerMesh = Instantiate<Mesh>(_primGO.GetComponent<MeshFilter>().sharedMesh);
			this.pointerMesh.name = "CtrlQuadPointer";
			_primGO.SetActive (false);
			GameObject.Destroy (_primGO);

			isPointerEnabled = true;
		}
	}

	void OnDisable()
	{
		PrintDebugLog ("OnDisable() " + this.device);
		removePointer ();
		isPointerEnabled = false;
	}

	/// <summary>
	/// The attributes
	/// <para>
	/// - _Color
	/// - _OuterDiameter
	/// - _DistanceInMeters
	/// can be updated directly by changing
	/// - colorFactor
	/// - PointerOuterDiameter
	/// - PointerDistanceInMeters
	/// But if developer need to update texture in runtime, developer should
	/// 1.set ShowPointer to false to hide pointer first.
	/// 2.assign CustomTexture
	/// 3.set UseSystemConfig to false
	/// 4.set UseDefaultTexture to false
	/// 5.set ShowPointer to true to generate new pointer.
	/// </summary>
	void Update()
	{
		if (this.ShowPointer)
		{
			if (!this.pointerInitialized)
			{
				if (this.device == WaveVR_Controller.EDeviceType.Head)
					PrintDebugLog ("Update() Head, show pointer");
				if (this.device == WaveVR_Controller.EDeviceType.Dominant)
					PrintDebugLog ("Update() Dominant, show pointer");
				if (this.device == WaveVR_Controller.EDeviceType.NonDominant)
					PrintDebugLog ("Update() NonDominant, show pointer");
				initialPointer ();
			}
		} else
		{
			if (this.pointerInitialized)
			{
				if (this.device == WaveVR_Controller.EDeviceType.Head)
					PrintDebugLog ("Update() Head, hide pointer");
				if (this.device == WaveVR_Controller.EDeviceType.Dominant)
					PrintDebugLog ("Update() Dominant, hide pointer");
				if (this.device == WaveVR_Controller.EDeviceType.NonDominant)
					PrintDebugLog ("Update() NonDominant, hide pointer");
				removePointer ();
			}
		}

		// Pointer distance.
		this.PointerDistanceInMeters = Mathf.Clamp (this.PointerDistanceInMeters, pointerDistanceMin, pointerDistanceMax);

		if (this.Blink == true)
		{
			if (Time.unscaledTime - colorFlickerTime >= 0.5f)
			{
				colorFlickerTime = Time.unscaledTime;
				this.colorFactor = (this.colorFactor != Color.white) ? this.colorFactor = Color.white : this.colorFactor = Color.black;
			}
		} else
		{
			this.colorFactor = this.PointerColor;
		}

		if (this.pointerMaterialInstance != null)
		{
			this.pointerMaterialInstance.renderQueue = this.PointerRenderQueue;
			this.pointerMaterialInstance.SetColor ("_Color", this.colorFactor);
			this.pointerMaterialInstance.SetFloat ("_useTexture", this.useTexture ? 1.0f : 0.0f);
			this.pointerMaterialInstance.SetFloat ("_OuterDiameter", this.PointerOuterDiameter);
			this.pointerMaterialInstance.SetFloat ("_DistanceInMeters", this.PointerDistanceInMeters);
		} else
		{
			if (Log.gpl.Print)
			{
				if (this.device == WaveVR_Controller.EDeviceType.Head)
					PrintDebugLog ("Update() Head, Pointer material is null!!");
				if (this.device == WaveVR_Controller.EDeviceType.Dominant)
					PrintDebugLog ("Update() Dominant, Pointer material is null!!");
				if (this.device == WaveVR_Controller.EDeviceType.NonDominant)
					PrintDebugLog ("Update() NonDominant, Pointer material is null!!");
			}
		}

		if (Log.gpl.Print)
		{
			PrintDebugLog (this.device + " " + gameObject.name
				+ " is " + (this.ShowPointer ? "shown" : "hidden")
				+ ", pointer color: " + this.colorFactor
				+ ", use texture: " + this.useTexture
				+ ", pointer outer diameter: " + this.PointerOuterDiameter
				+ ", pointer distance: " + this.PointerDistanceInMeters
				+ ", render queue: " + this.PointerRenderQueue);
		}
	}
	#endregion

	private void CreatePointerMesh()
	{
		int vertexCount = (reticleSegments + 1) * 2;
		Vector3[] vertices = new Vector3[vertexCount];
		for (int vi = 0, si = 0; si <= reticleSegments; si++)
		{
			float angle = (float)si / (float)reticleSegments * Mathf.PI * 2.0f;
			float x = Mathf.Sin (angle);
			float y = Mathf.Cos (angle);
			vertices [vi++] = new Vector3 (x, y, 0.0f);
			vertices [vi++] = new Vector3 (x, y, 1.0f);
		}

		int indicesCount = (reticleSegments + 1) * 6;
		int[] indices = new int[indicesCount];
		int vert = 0;
		for (int ti = 0, si = 0; si < reticleSegments; si++)
		{
			indices [ti++] = vert + 1;
			indices [ti++] = vert;
			indices [ti++] = vert + 2;
			indices [ti++] = vert + 1;
			indices [ti++] = vert + 2;
			indices [ti++] = vert + 3;

			vert += 2;
		}

		if (this.device == WaveVR_Controller.EDeviceType.Head)
			PrintDebugLog ("CreatePointerMesh() Head, create Mesh and add MeshFilter component.");
		if (this.device == WaveVR_Controller.EDeviceType.Dominant)
			PrintDebugLog ("CreatePointerMesh() Dominant, create Mesh and add MeshFilter component.");
		if (this.device == WaveVR_Controller.EDeviceType.NonDominant)
			PrintDebugLog ("CreatePointerMesh() NonDominant, create Mesh and add MeshFilter component.");

		this.pointerMesh = new Mesh ();
		this.pointerMesh.vertices = vertices;
		this.pointerMesh.triangles = indices;
		this.pointerMesh.name = "WaveVR_Mesh_Q";
		this.pointerMesh.RecalculateBounds ();
	}

	private bool pointerInitialized = false;					 // true: the mesh of reticle is created, false: the mesh of reticle is not ready

	private void initialPointer()
	{
		if (!this.isPointerEnabled)
		{
			if (this.device == WaveVR_Controller.EDeviceType.Head)
				PrintDebugLog ("initialPointer() Head, pointer is not enabled yet, do NOT initial.");
			if (this.device == WaveVR_Controller.EDeviceType.Dominant)
				PrintDebugLog ("initialPointer() Dominant, pointer is not enabled yet, do NOT initial.");
			if (this.device == WaveVR_Controller.EDeviceType.NonDominant)
				PrintDebugLog ("initialPointer() NonDominant, pointer is not enabled yet, do NOT initial.");
			return;
		}

		if (this.device == WaveVR_Controller.EDeviceType.Head)
			PrintDebugLog ("initialPointer() Head.");
		if (this.device == WaveVR_Controller.EDeviceType.Dominant)
			PrintDebugLog ("initialPointer() Dominant.");
		if (this.device == WaveVR_Controller.EDeviceType.NonDominant)
			PrintDebugLog ("initialPointer() NonDominant.");

		if (this.useTexture == false) {
			colorFlickerTime = Time.unscaledTime;
			CreatePointerMesh ();
			PrintDebugLog ("initialPointer() " + this.device + " be used to user custom mesh. ( WaveVR_Mesh_Q mesh )");
		} else {
			PrintDebugLog ("initialPointer() " + this.device + " be used to default mesh. ( CtrlQuadPointer mesh )");
		}

		this.pointerMeshFilter.mesh = this.pointerMesh;

		if (this.pointerMaterialInstance != null)
		{
			if (this.UseDefaultTexture || (null == this.CustomTexture))
			{
				if (this.device == WaveVR_Controller.EDeviceType.Head)
					PrintDebugLog ("initialPointer() Head, use default texture.");
				if (this.device == WaveVR_Controller.EDeviceType.Dominant)
					PrintDebugLog ("initialPointer() Dominant, use default texture.");
				if (this.device == WaveVR_Controller.EDeviceType.NonDominant)
					PrintDebugLog ("initialPointer() NonDominant, use default texture.");

				this.pointerMaterialInstance.mainTexture = this.defaultTexture;
				this.pointerMaterialInstance.SetTexture ("_MainTex", this.defaultTexture);
			} else
			{
				if (this.device == WaveVR_Controller.EDeviceType.Head)
					PrintDebugLog ("initialPointer() Head, use custom texture.");
				if (this.device == WaveVR_Controller.EDeviceType.Dominant)
					PrintDebugLog ("initialPointer() Dominant, use custom texture.");
				if (this.device == WaveVR_Controller.EDeviceType.NonDominant)
					PrintDebugLog ("initialPointer() NonDominant, use custom texture.");

				this.pointerMaterialInstance.mainTexture = this.CustomTexture;
				this.pointerMaterialInstance.SetTexture ("_MainTex", this.CustomTexture);
			}
		} else
		{
			Log.e (LOG_TAG, "initialPointer() Pointer material is null!!", true);
		}

		Renderer _rend = GetComponent<Renderer> ();
		_rend.enabled = true;
		_rend.material = this.pointerMaterialInstance;
		_rend.sortingOrder = 32767;

		this.pointerInitialized = true;
	}

	private void removePointer() {
		if (this.device == WaveVR_Controller.EDeviceType.Head)
			PrintDebugLog ("removePointer() Head");
		if (this.device == WaveVR_Controller.EDeviceType.Dominant)
			PrintDebugLog ("removePointer() Dominant");
		if (this.device == WaveVR_Controller.EDeviceType.NonDominant)
			PrintDebugLog ("removePointer() NonDominant");

		Renderer _rend = GetComponent<Renderer> ();
		_rend = GetComponent<Renderer>();
		_rend.enabled = false;
		this.pointerInitialized = false;
	}

	public void OnPointerEnter (Camera camera, GameObject target, Vector3 intersectionPosition, bool isInteractive) {
		SetPointerTarget(intersectionPosition, isInteractive);
	}

	/*
	public void OnPointerExit (Camera camera, GameObject target) {
		PointerDistanceInMeters = pointerDistanceMax;
		PointerOuterDiameter = PointerOuterDiameterMin + (PointerDistanceInMeters / kpointerGrowthAngle);
		PrintDebugLog ("OnPointerExit() PointerDistanceInMeters: " + this.PointerDistanceInMeters
		+ ", PointerOuterDiameter: " + this.PointerOuterDiameter);
	}
	*/

	#region Pointer Distance
	private void SetPointerTarget (Vector3 target, bool interactive)
	{
		Vector3 targetLocalPosition = transform.InverseTransformPoint (target);
		this.PointerDistanceInMeters = Mathf.Clamp (targetLocalPosition.z, pointerDistanceMin, pointerDistanceMax);
		this.PointerOuterDiameter = PointerOuterDiameterMin + (this.PointerDistanceInMeters / kpointerGrowthAngle);

		if (Log.gpl.Print)
		{
			PrintDebugLog ("SetPointerTarget() " + this.device + ", " + gameObject.name + ", "
				+ "SetPointerTarget() interactive: " + interactive
				+ ", targetLocalPosition.z: " + targetLocalPosition.z
				+ ", PointerDistanceInMeters: " + this.PointerDistanceInMeters
				+ ", PointerOuterDiameter: " + PointerOuterDiameter);
		}
	}
	#endregion
}
