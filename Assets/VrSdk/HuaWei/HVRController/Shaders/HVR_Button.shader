Shader "HVRShaders/HVR_Button" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf myLightModel  

		//命名规则：Lighting接#pragma suface之后起的名字 
  		//lightDir :点到光源的单位向量   viewDir:点到摄像机的单位向量   atten:衰减系数 
  		float4 LightingmyLightModel(SurfaceOutput s, float3 lightDir,half3 viewDir, half atten) 
  		{ 
  		 	float4 c ; 
  		 	c.rgb =  s.Albedo;
  			c.a = s.Alpha; 
    		        return c; 
  		}

		sampler2D _MainTex;
                float4 _Color;
        
		struct Input {
			float2 uv_MainTex;
			float4 _Color;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = tex2D (_MainTex, IN.uv_MainTex)*_Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}

