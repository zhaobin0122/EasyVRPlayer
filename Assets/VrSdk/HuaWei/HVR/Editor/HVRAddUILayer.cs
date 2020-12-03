/************************************************************************************

Filename    :   HVRAddUILayer.cs
Authors     :   HuaweiVRSDK
Copyright   :   Copyright HUAWEI Technologies Co., Ltd. 2016. All Rights reserved.

*************************************************************************************/
#if UNITY_EDITOR

using UnityEngine;
using System.Collections;
using UnityEditor;
using HVRCORE;

namespace HVRCORE
{

    [InitializeOnLoad]
    public class HVRAddUILayer
    {
		static HVRAddUILayer()
        {
            SetAndroidBlitType();
            AddLayer(HVRDefCore.m_UILayerName);
            AddLayer (HVRDefCore.m_VolumeUILayerName);
			if(LayerMask.NameToLayer(HVRDefCore.m_UILayerName)<0){
				EditorUtility.DisplayDialog ("Add  HVRUILayer Layer failed" , "Add HVRUILayer failed, please add it by yourself manually " ,"ok" ,"");
			}
			if(LayerMask.NameToLayer(HVRDefCore.m_VolumeUILayerName)<0){
				EditorUtility.DisplayDialog ("Add  HVRVolumeUILayer Layer failed" , "Add HVRVolumeUILayer failed, please add it by yourself manually " ,"ok" ,"");
			}
            return;
        }
		
        private static void SetAndroidBlitType()
        {
#if UNITY_2018_3_OR_NEWER
            PlayerSettings.Android.blitType = AndroidBlitType.Never;
            Debug.Log("PlayerSettings.Android.blitType: " + PlayerSettings.Android.blitType);
#endif
        }
		
        private static void AddLayer(string layer)
        {
            if (!IsHasLayer(layer))
            {
                SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                SerializedProperty it = tagManager.GetIterator();
                while (it.NextVisible(true))
                {
                    if (it.name == "layers")
                    {
                        for (int i = 31; i > 7; i--)
                        {
                            SerializedProperty dataPoint = it.GetArrayElementAtIndex(i);
                            if (string.IsNullOrEmpty(dataPoint.stringValue))
                            {
                                dataPoint.stringValue = layer;
                                tagManager.ApplyModifiedProperties();
                                return;
                            }
                        }
                    }
                }
            }
        }

        private static bool IsHasLayer(string layer)
        {
            for (int i = 0; i < UnityEditorInternal.InternalEditorUtility.layers.Length; i++)
            {
                if (UnityEditorInternal.InternalEditorUtility.layers[i].Contains(layer))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
#endif
