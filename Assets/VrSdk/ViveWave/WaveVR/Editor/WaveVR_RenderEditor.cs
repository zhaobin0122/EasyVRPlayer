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
using UnityEditor;
using System.IO;
using System.Collections.Generic;

[CustomEditor(typeof(WaveVR_Render))]
public class WaveVR_RenderEditor : Editor
{
	WaveVR_Render render;
	private int bannerHeightMax = 150;
	Texture logo = null;

	string GetResourcePath()
	{
		var ms = MonoScript.FromScriptableObject(this);
		var path = AssetDatabase.GetAssetPath(ms);
		path = Path.GetDirectoryName(path);
		return path.Substring(0, path.Length - "Editor".Length) + "Textures/";
	}

	void OnEnable()
	{
		render = (WaveVR_Render) target;

		var resourcePath = GetResourcePath();
#if UNITY_5_0
		logo = Resources.LoadAssetAtPath<Texture2D>(resourcePath + "vivewave_logo_flat.png");
#else
		logo = AssetDatabase.LoadAssetAtPath<Texture2D>(resourcePath + "vivewave_logo_flat.png");
#endif
		Validate();
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		if (logo)
		{
			// Logo need have aspect rate 2:1
			int bannerWidth, bannerHeight;
			bannerWidth = Screen.width - 35;
			bannerHeight = (int) (bannerWidth / (float) 2);
			if (bannerHeight > bannerHeightMax)
			{
				bannerHeight = bannerHeightMax;
				bannerWidth = bannerHeight * 2;
			}
			var rect = GUILayoutUtility.GetRect(bannerWidth, bannerHeight, GUI.skin.box);
			GUI.DrawTexture(rect, logo, ScaleMode.ScaleToFit);
		}

		if (!Application.isPlaying)
		{
			var expand = false;
			var collapse = false;

			if (render.isExpanded)
				collapse = true;
			else
				expand = true;

			if (expand)
			{
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Expand"))
				{
					if (!render.isExpanded)
					{
						WaveVR_Render.Expand(render);
						EditorUtility.SetDirty(render);
					}
				}
				GUILayout.EndHorizontal();
			}

			if (collapse)
			{
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Collapse"))
				{
					if (render.isExpanded)
					{
						WaveVR_Render.Collapse(render);
						EditorUtility.SetDirty(render);
					}
				}
				GUILayout.EndHorizontal();
			}
		}
		serializedObject.ApplyModifiedProperties();

		EditorGUI.BeginChangeCheck();
		DrawDefaultInspector();
		if (EditorGUI.EndChangeCheck())
		{
			Validate();
		}
	}

	private void Validate()
	{
		//var preferredStereoRenderingPath = serializedObject.FindProperty("preferredStereoRenderingPath");
		//if (preferredStereoRenderingPath != null && preferredStereoRenderingPath.enumValueIndex != 0)
		//{
		//	// Not Multi-pass.  Need check if single pass settings is complete.
		//	var item = WaveVR_Settings.GetVRItem();
		//	if (!item.IsReady())
		//	{
		//		var list = new List<WaveVR_Settings.Item>() { item };
		//		WaveVR_Settings.UpdateInner(list, true);
		//	};
		//}
	}
}