Shader "Sprites/CustomLighting"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha        
        ZTest Off
        ZWrite Off
        Cull Off


        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0

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

            struct LightData {
                float angle;
                float range;
                float intensity;
                float4 color;
                float2 position;
                int isOn;
            };
            
            StructuredBuffer<LightData> lights;
            int resolutionX;
            int resolutionY;
            float4 ambient;

            sampler2D _MainTex;
            sampler2D LightTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            fixed4 frag (v2f vi) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, vi.uv);
                float2 pixelPos = float2(vi.vertex.x, vi.vertex.y);

                float4 lightColor = tex2D(LightTex, float2(pixelPos.x / float(resolutionX), 1.0 - pixelPos.y / float(resolutionY)));
                
                return col * lightColor + col * ambient;
            }
            ENDCG
        }
    }
}
