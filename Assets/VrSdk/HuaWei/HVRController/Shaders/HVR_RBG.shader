Shader "HVRShaders/HVR_RBG" {
	Properties {
	    _MainColor ("MainColor", Color) = (1.0,1.0,1.0,1.0)
		_Color ("Color", Color) = (1.0,1.0,1.0,1.0)
	    _MainTex("_MainTex", 2D) = "white" {}
        _RimPow ("Rim Power", Range(0.0,5.0)) = 2.0
		 _AddRimPow ("AddRim Power", Range(0.0,5.0)) = 2.0
        _Glossiness ("Brightness",Range(0.0,3.0)) = 3.0
    }
    SubShader {
      Tags { "RenderType" = "Transparent" "Queue"="Transparent" "IgnoreProjector"="True"}

	 Pass {
		ZWrite On
		ColorMask 0
	   }

      CGPROGRAM
      #pragma surface surf Lambert alpha noambient nolightmap nodirlightmap  novertexlights
      struct Input {
       
		  float2 uv_MainTex;
          float2 uv_BumpMap;
          float2 viewDir;
      };
    
	  sampler2D _MainTex;
      sampler2D _BumpMap;
	  float4 _MainColor;
      float4 _Color;
      float _RimPow;
	  float _AddRimPow;
      float _Glossiness;
	  

      void surf (Input IN, inout SurfaceOutput o) {
      	
		  half4 c = tex2D(_MainTex, IN.uv_MainTex)*_MainColor*_AddRimPow;
     		
		  o.Albedo = c.rgb;
          o.Normal = UnpackNormal (tex2D (_BumpMap, IN.uv_BumpMap));
		  o.Emission = saturate(_Color.rgb * pow((1.0 - saturate(dot (normalize(IN.viewDir), o.Normal))), _RimPow) * _Glossiness + c);
          o.Alpha = c.a * _Glossiness * o.Emission;
      }
      ENDCG
    }
    Fallback "Diffuse"
  }
