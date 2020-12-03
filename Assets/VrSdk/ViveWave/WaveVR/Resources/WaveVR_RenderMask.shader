Shader "Hidden/WaveVR/RenderMask"
{
	Properties
	{
		MeshXOffset("MeshXOffset", Float) = 1  // value shoud be greater than zero
		_Color("MaskColor", Color) = (0, 0, 0, 1)  // value shoud be greater than zero
	}

	SubShader
	{

		Tags { "RenderType" = "Opaque" "Queue" = "Background"}
		LOD 100

		Pass
		{
			ZWrite On
			Cull Off
			ZTest Always

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = v.vertex;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return fixed4(_Color);
			}
			ENDCG
		}

		Pass
		{
			ZWrite On
			Cull Off
			ZTest Always

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float MeshXOffset;
			fixed4 _Color;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = float4(v.vertex.x - (unity_StereoEyeIndex * 2.0f - 1) * MeshXOffset, v.vertex.yz, 1);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return fixed4(_Color);
			}
			ENDCG
		}
	}
}