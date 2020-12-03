Shader "WaveVR/CtrlrPointer" {
  Properties {
    _OuterDiameter ("OuterDiameter", Range(0.0, 10.0)) = 2.0
    _DistanceInMeters ("DistanceInMeters", Range(0.0, 100.0)) = 3.0
    _MainTex("Base (RGBA)", 2D) = "white" {}
    _Color("Color", Range(0.0, 1.0)) = 1.0
    _useTexture("isUseTexture", Range(0.0 , 1.0)) = 0.0
  }

  SubShader{
    Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
    Pass{
	Blend SrcAlpha OneMinusSrcAlpha
	AlphaTest Off
	Cull Off
	ZWrite Off
	ZTest Always

    CGPROGRAM

    #pragma vertex vert
    #pragma fragment frag

    #include "UnityCG.cginc"

	uniform float _OuterDiameter;
	uniform float _DistanceInMeters;
	uniform sampler2D _MainTex;
	uniform float _useTexture;
	uniform fixed4 _Color;

	struct vertIn {
	    float4 vertex : POSITION;
	    float2 uv : TEXCOORD0;
	};

	struct fragIn {
	    float2 uv : TEXCOORD0;
	    fixed4 color : COLOR;
	    float4 position : SV_POSITION;
	};

	fragIn vert(vertIn vi) {
	    float scale = lerp(_OuterDiameter, 0, vi.vertex.z);
	    float4 vert_out = float4(vi.vertex.x * scale, vi.vertex.y * scale, _DistanceInMeters, 1.0);

	    fragIn o;
	    o.position = UnityObjectToClipPos (vert_out);

	    _useTexture == 1.0 ? (o.uv = vi.uv) : (o.color = _Color);

	    return o;
	}

	fixed4 frag(fragIn fi) : SV_Target{
	    return (_useTexture == 1.0) ? tex2D(_MainTex, fi.uv) : fi.color;
	}

      ENDCG
    }
  }
}
