/************************************************************************************

Filename    :   HVRNoticeText.shader
Authors     :   HuaweiVRSDK
Copyright   :   Copyright HUAWEI Technologies Co., Ltd. 2015. All Rights reserved.

*************************************************************************************/

Shader "HVRShaders/HVRNoticeText"
{
	Properties
	{
		_MainTex("Texture",2D)="white"{}
		_Color("Color",Color)=(1,1,1,1)
		
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Overlay+1002"
			"IgnoreProjector" = "True"
			"RenderType" = "Overlay"
		}

		Lighting Off 
		Cull Off
		ZTest Always
		ZWrite Off
		Fog{Mode Off}
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			Color[_Color]
			SetTexture[_MainTex]
			{
				combine primary, texture * primary
			}
		}
	} 
}
