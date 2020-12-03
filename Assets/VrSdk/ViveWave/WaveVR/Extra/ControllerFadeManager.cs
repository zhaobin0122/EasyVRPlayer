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

[RequireComponent(typeof(FadeManger))]
public class ControllerFadeManager : MonoBehaviour {
	private FadeManger fadeManager;

	void Start () {
		fadeManager = GetComponent<FadeManger>();

		MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
		fadeManager.Materials = new List<Material>();

		foreach (var renderer in renderers)
		{
			if (renderer == null)
				continue;

			Material material = renderer.material;
			if (material != null && material.shader.name == "WaveVR/UnlitControllerShader")
			{
				if (material.HasProperty("_FadeAlpha") && !fadeManager.Materials.Contains(material))
					fadeManager.Materials.Add(material);
			}
		}
	}

	private float AngleHide = 15; // 0-90

	void Update () {
		// Fade out when pitch angle is high.
		// angle here is in degree
		if (fadeManager != null)
		{
			float angle = Mathf.Acos(Vector3.Dot(transform.forward, Vector3.up)) * Mathf.Rad2Deg;
			if (angle < AngleHide)
			{
				fadeManager.Fade(true);
			}
			else
			{
				fadeManager.Fade(false);
			}
		}
	}
}
