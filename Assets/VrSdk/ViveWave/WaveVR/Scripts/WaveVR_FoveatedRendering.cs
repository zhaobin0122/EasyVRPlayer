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
using UnityEngine;
using WVR_Log;

namespace wvr.render
{

	public class WaveVR_FoveatedRendering : MonoBehaviour
	{
		public static string TAG = "WVR_Foveated";
		public static string StaticTrackedObjectName = "StaticFoveatedTrackedObject";

		public static WaveVR_FoveatedRendering Instance { get; private set; }

		// Set true to use the FOV and PeripheralQuality in this inspector.  Directly invoke 
		// SetFoveatedRenderingParameter will set this value to false.  And stop 
		// update FoveatedRenderingParameter by this script.
		private static bool useValuesInInspector = true;

		[Tooltip("The the central region around left eye's focal point.  We guarantee the" +
			" resolution here will be 100%.  The FOV value is diameter.")]
		[Range(1, 179)]
		[SerializeField]
		private float leftClearVisionFOV = 38;
		public float LeftClearVisionFOV { get { return leftClearVisionFOV; } set { leftClearVisionFOV = Mathf.Clamp(value, 1, 179); isDirty = true; } }

		[Tooltip("The the central region around right eye's focal point.  We guarantee the" +
			" resolution here will be 100%.  The FOV value is diameter.")]
		[Range(1, 179)]
		[SerializeField]
		private float rightClearVisionFOV = 38;
		public float RightClearVisionFOV { get { return rightClearVisionFOV; } set { rightClearVisionFOV = Mathf.Clamp(value, 1, 179); isDirty = true; } }

		[Tooltip("The peripheral is the region around the left eye's clear vision FOV.  Its " +
			"resolution will depend on this quality setting.")]
		[SerializeField]
		private WVR_PeripheralQuality leftPeripheralQuality = WVR_PeripheralQuality.High;
		public WVR_PeripheralQuality LeftPeripheralQuality { get { return leftPeripheralQuality; } set { leftPeripheralQuality = value; isDirty = true; } }

		[Tooltip("The peripheral is the region around the right eye's clear vision FOV.  Its" +
			"resolution will depend on this quality setting.")]
		[SerializeField]
		private WVR_PeripheralQuality rightPeripheralQuality = WVR_PeripheralQuality.High;
		public WVR_PeripheralQuality RightPeripheralQuality { get { return rightPeripheralQuality; } set { rightPeripheralQuality = value; isDirty = true; } }

		[Tooltip("A Focal Point for static foveated rendering without eye tracking.  Both eye " +
			"will focus on this point.  Therefore the both lines linked from focal point to " +
			"two eyes will not be parallel.  We will calculate the focal point on screen for " +
			"you.\nThe default focal point will generate a tracked object in front of the eye " +
			"center.  Assign a custom tracked object will ignore this setting.")]
		[SerializeField]
		private Vector3 staticFocalPoint = new Vector3(0, 0, 10);
		public Vector3 StaticFocalPoint { get { return staticFocalPoint; } set { staticFocalPoint = value; isDirty = true; } }

		// prevent double enable
		private static bool isEnabled = false;

		[Tooltip("The focal point will always on the tracked object.")]
		[SerializeField]
		private GameObject trackedObject;
		public GameObject TrackedObject { get { return trackedObject; } set { trackedObject = value; isDirty = true; } }
		private GameObject staticTrackedObject = null;

		public Vector2 LeftNDCSpace { get; private set; }
		public Vector2 RightNDCSpace { get; private set; }

		private bool isDirty = true;

		// Apply the settings in this script now.  Override the settings of SetFoveatedRenderingParameter.
		public void Apply()
		{
			useValuesInInspector = true;
			doUpdate();
		}

		private void ValidateFOV(float fov)
		{
			if (fov < 1 || fov > 179)
				throw new System.ArgumentOutOfRangeException("FOV should be in the range of [1, 179] degrees.");
		}

		private void ValidateQuality(WVR_PeripheralQuality quality)
		{
			if (quality < WVR_PeripheralQuality.Low || quality > WVR_PeripheralQuality.High)
				throw new System.ArgumentOutOfRangeException("Quality should be one of the WVR_PeripheralQuality.");
		}

		public void Validate()
		{
			ValidateFOV(leftClearVisionFOV);
			ValidateFOV(rightClearVisionFOV);
			ValidateQuality(leftPeripheralQuality);
			ValidateQuality(rightPeripheralQuality);
		}

		// Each eye can have individual value.
		public void Set(WVR_Eye eye, float clearVisionFOV, WVR_PeripheralQuality quality)
		{
			ValidateFOV(clearVisionFOV);
			ValidateQuality(quality);

			if (eye == WVR_Eye.WVR_Eye_Left)
			{
				leftClearVisionFOV  = clearVisionFOV;
				leftPeripheralQuality  = quality;
			}
			else if (eye == WVR_Eye.WVR_Eye_Right)
			{
				rightClearVisionFOV = clearVisionFOV;
				rightPeripheralQuality = quality;
			}
			else
			{
				throw new System.ArgumentException("Eye (" + eye + ") should be WVR_Eye_Left or WVR_Eye_Right.");
			}
			isDirty = true;
		}

		// Two eye set the same value.
		public void Set(float clearVisionFOV, WVR_PeripheralQuality quality)
		{
			ValidateFOV(clearVisionFOV);
			ValidateQuality(quality);

			leftClearVisionFOV = rightClearVisionFOV = clearVisionFOV;
			leftPeripheralQuality = rightPeripheralQuality = quality;
			isDirty = true;
		}

		IEnumerator SetEnableCoroutine()
		{
			while (enabled)
			{
				if (isEnabled)
					yield break;

				var render = WaveVR_Render.Instance;
				if (render != null && render.IsGraphicReady)
				{
#if UNITY_EDITOR && UNITY_ANDROID
					if (Application.isEditor)
					{
						Instance = this;
						isEnabled = true;
					}
					else
#endif
					if (Interop.WVR_IsRenderFoveationSupport())
					{
						Interop.WVR_RenderFoveation(true);

						Instance = this;
						isEnabled = true;
						Log.d(TAG, "Foveated rendering is enabled.", true);
					}
					else
					{
						Log.d(TAG, "Foveated rendering is not supported.");
					}
				}
				yield return null;
				Log.gpl.d(TAG, "Waiting for graphic ready to enable foveated rendering.", true);
			}
		}

		void OnEnable()
		{
			if (!isEnabled)
				StartCoroutine("SetEnableCoroutine");
		}

		void OnDisable()
		{
			if (isEnabled)
			{
				if (Instance == this)
					Instance = null;
				else
					return;

				StopCoroutine("SetEnableCoroutine");

#if UNITY_EDITOR && UNITY_ANDROID
				if (!Application.isEditor)
#endif
				Interop.WVR_RenderFoveation(false);
				isEnabled = false;
			}
		}

		void CreateTrackedObject(WaveVR_Render render)
		{
			Transform t;
			if (staticTrackedObject != null)
				t = staticTrackedObject.transform;
			else
				t = render.centerWVRCamera.transform.Find(StaticTrackedObjectName);

			if (t == null)
			{
				var obj = new GameObject(StaticTrackedObjectName);
				t = obj.transform;
				t.localPosition = staticFocalPoint;
				t.SetParent(render.centerWVRCamera.transform, false);
			}

			trackedObject = t.gameObject;
		}

		void LateUpdate()
		{
			if (isDirty)
				doUpdate();
		}

		void doUpdate()
		{
			if (!isEnabled || !useValuesInInspector || Instance != this)
			{
				Log.gpl.d(TAG, "LateUpdate: !isEnabled || !useValuesInInspector || Instance != this");
				return;
			}

			var render = WaveVR_Render.Instance;
			if (render == null || !render.isExpanded)
			{
				Log.gpl.d(TAG, "LateUpdate: render == null || !render.isExpanded");
				return;
			}

			if (trackedObject == null)
				CreateTrackedObject(render);

			var worldfocalPoint = trackedObject.transform.position;

			// Left eye
			LeftNDCSpace = WorldToNDC_GL(render.lefteye.GetCamera(), worldfocalPoint);
			var length = LeftNDCSpace.sqrMagnitude;
			if (length > 1)
				LeftNDCSpace = LeftNDCSpace.normalized;
			SetFoveatedRenderingParameterCheck(WVR_Eye.WVR_Eye_Left, LeftNDCSpace.x, LeftNDCSpace.y, leftClearVisionFOV, leftPeripheralQuality);

			// Right eye
			RightNDCSpace = WorldToNDC_GL(render.righteye.GetCamera(), worldfocalPoint);
			length = RightNDCSpace.sqrMagnitude;
			if (length > 1)
				RightNDCSpace = RightNDCSpace.normalized;
			SetFoveatedRenderingParameterCheck(WVR_Eye.WVR_Eye_Right, RightNDCSpace.x, RightNDCSpace.y, rightClearVisionFOV, rightPeripheralQuality);

			// Keep dirty if tracked object exist.
			if (trackedObject == staticTrackedObject)
				isDirty = false;
		}

		public static Vector2 WorldToNDC_GL(Camera camera, Vector3 worldPoint)
		{
			Matrix4x4 mat = camera.projectionMatrix * camera.worldToCameraMatrix;
			Vector4 temp = mat * new Vector4(worldPoint.x, worldPoint.y, worldPoint.z, 1f);
			var ndc = new Vector2(temp.x, temp.y) / temp.w;
			return ndc;
		}

		#region api
		private static void SetFoveatedRenderingParameterCheck(WVR_Eye eye, float ndcFocalPointX, float ndcFocalPointY, float clearVisionFOV, WVR_PeripheralQuality quality)
		{
			if (eye == WVR_Eye.WVR_Eye_Both || eye == WVR_Eye.WVR_Eye_None)
				throw new System.ArgumentException("Invalid argument: eye (" + eye + ") should be WVR_Eye_Left or WVR_Eye_Right.");

			if (quality < WVR_PeripheralQuality.Low || quality > WVR_PeripheralQuality.High)
				throw new System.ArgumentException("Invalid argument: level (" + quality + ") should be in WVR_PeripheralQuality range.");

			//Log.d(TAG, "eye " + eye + " XY = (" + ndcFocalPointX + ", " + ndcFocalPointY + ", " + clearVisionFOV + ", " + level + ")");
			WaveVR_Render.SetFoveatedRenderingParameter(eye, ndcFocalPointX, ndcFocalPointY, clearVisionFOV, quality);
		}

		public static void SetFoveatedRenderingParameter(WVR_Eye eye, float ndcFocalPointX, float ndcFocalPointY, float clearVisionFOV, WVR_PeripheralQuality quality)
		{
			WaveVR_FoveatedRendering.useValuesInInspector = true;
			SetFoveatedRenderingParameterCheck(eye, ndcFocalPointX, ndcFocalPointY, clearVisionFOV, quality);
		}

		public static void SetFoveatedRenderingParameter(WVR_Eye eye, Vector2 ndcSpace, float clearVisionFOV, WVR_PeripheralQuality quality)
		{
			WaveVR_FoveatedRendering.useValuesInInspector = true;
			ndcSpace = ndcSpace.normalized;
			SetFoveatedRenderingParameterCheck(eye, ndcSpace.x, ndcSpace.y, clearVisionFOV, quality);
		}
		#endregion
	}
}

