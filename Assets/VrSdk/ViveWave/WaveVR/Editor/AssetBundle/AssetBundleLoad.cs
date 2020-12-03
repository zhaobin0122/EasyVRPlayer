#if UNITY_EDITOR && UNITY_ANDROID
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class AssetBundleLoad : EditorWindow
{
	[MenuItem("Window/AssetBundle/Load an AssetBundle")]
	static void SelectFile()
	{
		string _file_path = EditorUtility.OpenFilePanel("Select folder of AssetBundle", "", "");

		if (_file_path != null && _file_path.Length != 0)
		{
			Debug.Log ("AssetBundle path: " + _file_path);
			LoadAssetBundle (_file_path);
		}
	}

	private static void LoadAssetBundle(string file_path)
	{
		string _file_name = Path.GetFileName (file_path);
		Debug.Log ("AssetBundle: " + _file_name);

		AssetBundle _ab = AssetBundle.LoadFromFile (file_path);
		GameObject _prefab = _ab.LoadAsset (_file_name) as GameObject;
		Debug.Log ("Prefab: " + _prefab.name);
		Instantiate (_prefab);

		_ab.Unload (false);
	}
}
#endif
