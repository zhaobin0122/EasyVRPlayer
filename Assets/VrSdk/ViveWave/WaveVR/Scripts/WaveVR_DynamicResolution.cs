// "WaveVR SDK 
// © 2017 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the WaveVR SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System.Collections.Generic;
using UnityEngine;
using WVR_Log;

namespace wvr.render
{
	using System.Linq;
#if UNITY_EDITOR
	using UnityEditor;
	[CustomEditor(typeof(WaveVR_DynamicResolution))]
	public class WaveVR_DynamicResolutionEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			WaveVR_DynamicResolution myScript = target as WaveVR_DynamicResolution;
			EditorGUILayout.HelpBox("Dynamic Resolution is feature to help adjust the Resolution Scale of the application according to system resources usage. It also helps determining a lower bound for the Resolution Scale to maintain text readability at certain text size.", MessageType.None);
			EditorGUILayout.HelpBox("Specify the smallest size of text that you will use in your application. This parameter will be used while determining the lower bound for maintaining text readability.", MessageType.Info);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("textSize"));
			serializedObject.ApplyModifiedProperties();

			EditorGUILayout.HelpBox("You can define a set of Resolution Scale values which will be applied according to the events triggered by AdaptiveQuality, adjust the default Resolution Scale by changing the deafult index value.", MessageType.Info);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("resolutionScaleList"), true);
			serializedObject.ApplyModifiedProperties();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultIndex"));
			serializedObject.ApplyModifiedProperties();
		}
	}
	#endif

	// The WaveVR_DynamicResolution is a very simple application of ResolutionScale.  See WaveVR_Render for more infomation of ResolutionScale.
	// The WaveVR_DynamicResolution will be triggered by WaveVR_AdaptiveQuality.  If AdaptiveQuality is not enabled, this feature will not work.

	public class WaveVR_DynamicResolution : MonoBehaviour
	{
		[Tooltip("The ResolutionScale help set a scaled resolution to be lower than the default.  An index will go higher or lower according to the AdaptiveQuality's event.  You can choose one of resolution scale from this list as a default resolution scale by setting the default index.")]
		[SerializeField]
		private List<float> resolutionScaleList = new List<float>();

		[Tooltip("You can choose one of resolution scale from this list as a default resolution scale by setting the default index.")]
		[SerializeField]
		private int defaultIndex = 0;
		private int index = 0;

		[Tooltip("The unit used for measuring text size here is dmm (Distance-Independent Millimeter). The method of conversion from Unity text size into dmm can be found in the documentation of the SDK.")]
		[SerializeField]
		[Range(20, 40)]
		private int textSize = 20;

		public float CurrentScale { get { return resolutionScaleList[index]; } }
		private float currentLowerBound = 0.1f;
		private bool isInitialized = false;
		private const string LOG_TAG = "WVRDynRes";

		public enum AQEvent
		{
			None,
			ManualHigher,
			ManualLower,
			Higher,
			Lower,
		};

		public AQEvent CurrentAQEvent { get; private set; }

		void OnEnable()
		{
			if (resolutionScaleList.Count < 2)
			{
				Log.e(LOG_TAG, "Not to enable because the list is empty.");
				return;
			}

			WaveVR_Utils.Event.Listen(WVR_EventType.WVR_EventType_RecommendedQuality_Higher.ToString(), HigherHandler);
			WaveVR_Utils.Event.Listen(WVR_EventType.WVR_EventType_RecommendedQuality_Lower.ToString(), LowerHandler);
			index = defaultIndex;
			CurrentAQEvent = AQEvent.None;
			WaveVR_Render.Instance.onFirstFrame = InitDynamicResolution;
		}

		void OnDisable()
		{
			WaveVR_Utils.Event.Remove(WVR_EventType.WVR_EventType_RecommendedQuality_Higher.ToString(), HigherHandler);
			WaveVR_Utils.Event.Remove(WVR_EventType.WVR_EventType_RecommendedQuality_Lower.ToString(), LowerHandler);
			index = defaultIndex;

			WaveVR_Render.Instance.SetResolutionScale(1);
		}

		// Let the function can be access by script.
		public void Higher() { HigherHandler(); CurrentAQEvent = AQEvent.ManualHigher; }
		void HigherHandler(params object[] args)
		{
			if (!isInitialized) return;

			if (--index < 0)
				index = 0;

			WaveVR_Render.Instance.SetResolutionScale(resolutionScaleList[index]);
			CurrentAQEvent = AQEvent.Higher;
			Log.d(LOG_TAG, "Event Higher: [" + index + "]=" + resolutionScaleList[index]);
		}

		// Let the function can be access by script.
		public void Lower() { LowerHandler(); CurrentAQEvent = AQEvent.ManualLower; }
		void LowerHandler(params object[] args)
		{
			if (!isInitialized) return;

			if (++index >= resolutionScaleList.Count)
				index = resolutionScaleList.Count - 1;

			WaveVR_Render.Instance.SetResolutionScale(resolutionScaleList[index]);
			CurrentAQEvent = AQEvent.Lower;
			Log.d(LOG_TAG, "Event Lower: [" + index + "]=" + resolutionScaleList[index]);
		}

		// Set the scale back to default.
		public void Reset()
		{
			CurrentAQEvent = AQEvent.None;

			if (!enabled)
				return;
			index = defaultIndex;
			WaveVR_Render.Instance.SetResolutionScale(resolutionScaleList[index]);
			Log.d(LOG_TAG, "Event Reset: [" + index + "]=" + resolutionScaleList[index]);
		}

		private void InitDynamicResolution(WaveVR_Render waveVR_Render)
		{
			DefineLowerBound();
			SetListLowerBound();
			isInitialized = true;
		}

		private void SetListLowerBound()
		{
			int counter = resolutionScaleList.Count - 1;
			while (resolutionScaleList[counter] < currentLowerBound)
			{
				resolutionScaleList.RemoveAt(counter);
				counter--;
			}
			resolutionScaleList.Add(currentLowerBound);

			FormatResolutionScaleList();
			if (index > counter)
			{
				index = defaultIndex = counter;
			}

			Log.d(LOG_TAG, "Finalilzed Resolution Scale List: " + resolutionScaleList.ToString());
			WaveVR_Render.Instance.SetResolutionScale(resolutionScaleList[index]);
		}

		private float GetResScaleFromDMM()
		{
			float P60D = 178.15f * (textSize * textSize) - 14419f * textSize + 356704f;

			Log.d(LOG_TAG, "Get P60D from DMM: " + P60D);

			float halfWidth = WaveVR_Render.Instance.sceneWidth / 2;
			float halfHeight = WaveVR_Render.Instance.sceneHeight / 2;
			float[] projection = WaveVR_Render.Instance.projRawL;
			float tan30 = Mathf.Tan(Mathf.Deg2Rad * 30f);

			float resolutionScale = Mathf.Sqrt(P60D / (Mathf.Pow(tan30,2) * halfHeight * halfWidth * (1/Mathf.Abs(projection[0]) + 1 / Mathf.Abs(projection[1])) * (1 / Mathf.Abs(projection[2]) + 1 / Mathf.Abs(projection[3]))));

			Log.d(LOG_TAG, "Eye Buffer Width: " + halfWidth + " Eye Buffer Height: " + halfHeight);
			Log.d(LOG_TAG, "Projection: " + string.Join(", ", projection.Select(p => p.ToString()).ToArray()));
			Log.d(LOG_TAG, "Get Resolution Scale from P60D: " + resolutionScale);

			return resolutionScale;
		}

		private void DefineLowerBound()
		{
			FormatResolutionScaleList();
			currentLowerBound = Mathf.Max(GetResScaleFromDMM(), resolutionScaleList[resolutionScaleList.Count-1]);
		}

		private void FormatResolutionScaleList()
		{
			//Sort List
			FloatComparer floatComparer = new FloatComparer();
			resolutionScaleList.Sort(floatComparer);
			//Remove duplicate values
			resolutionScaleList = resolutionScaleList.Distinct().ToList();
		}

		void OnValidate()
		{
			while (resolutionScaleList.Count < 2)
				resolutionScaleList.Add(1);

			if (defaultIndex < 0 || defaultIndex >= resolutionScaleList.Count)
				defaultIndex = 0;
		}
	}

	class FloatComparer : IComparer<float>
	{
		public int Compare(float x, float y)
		{
			return y.CompareTo(x);
		}
	}
}
