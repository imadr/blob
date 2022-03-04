Shader "Unlit/Blob"
{
    Properties
    {
        _Tex ("Texture", 2D) = "white" {}
        _TexBlurred ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags{
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _Tex;
            float4 _Tex_ST;
            sampler2D _TexBlurred;
            float4 _TexBlurred_ST;

            v2f vert (appdata v){
                v2f o;
                float col = tex2Dlod(_TexBlurred, float4(v.uv, 0, 0)).r/5.;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.vertex = float4(o.vertex.x, o.vertex.y-col, o.vertex.z, o.vertex.w);
                o.uv = TRANSFORM_TEX(v.uv, _Tex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target{
                fixed4 col = tex2D(_Tex, i.uv);
                if(col.a == 0) discard;
                return lerp(float4(0.952, 0.556, 0.109, 0), float4(0.992, 0.894, 0.156, 1), col);
            }
            ENDCG
        }
    }
}
