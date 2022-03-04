Shader "Unlit/Dish"{
    SubShader{
        Tags{
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha

        Pass{
            Tags{ "LightMode" = "ForwardBase" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct v2f{
                float4 position : SV_POSITION;
                float3 normal : NORMAL;
                float3 uv: TEXCOORD0;
            };

            v2f vert(appdata_base v){
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
                o.normal = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
                o.uv = v.texcoord;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target{
                float diffuse = max(0, dot(i.normal, _WorldSpaceLightPos0.xyz));
                float3 view_dir = normalize(_WorldSpaceCameraPos-i.position);
                float3 reflect_dir = reflect(-_WorldSpaceLightPos0.xyz, i.normal);
                float specular = pow(max(dot(view_dir, reflect_dir), 0.0), 64);
                float light = specular+diffuse;
                return float4(light, light, light, 0.22);
            }
            ENDCG
        }
    }
}
