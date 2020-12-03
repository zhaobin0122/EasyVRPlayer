Shader "Custom/ClipSphereRight" {
Properties{
        _Color("Color",Color)=(1,1,1,1)
        _P("p",Range(-0.5,0.5))=0
    }
    SubShader{
        Pass{
            Cull OFF
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _Color;
            float _P;

            struct a2v{
                float4 vertex:POSITION;
                float4 texcoord:TEXCOORD0;
            };
            struct v2f{
                float4 pos:POSITION;
                float4 uv:TEXCOORD0;
            };

            v2f vert(a2v v){
                v2f o;
                o.uv = v.vertex;
                o.pos=UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i):COLOR{
                if(i.uv.z< _P){
                    discard;
                }
                return _Color;
            }

            ENDCG
        }
    }
}
