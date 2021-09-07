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
            };
            
            StructuredBuffer<LightData> lights;
            float resolution;
            float4 ambient;

            sampler2D _MainTex;
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

                float4 diffuse = float4(0.0, 0.0, 0.0, 0.0);
                for(int i = 0; i < lights.Length; i++)
                {
                    float2 lightPos = lights[i].position;
                    lightPos.y = resolution - lightPos.y;
                    float lightRange = lights[i].range;
                    
                    float2 direction = float2(lightPos.x - pixelPos.x, lightPos.y - pixelPos.y);
                    float distance = sqrt(pow(direction.x, 2) + pow(direction.y, 2));
                    
                    if(distance > lightRange)
                        continue;
                    
                    float constantAtt = 1.0;
                    float linearAtt = 0.0;
                    float quadraticAtt = 3.0;
                    float attenuation = (1.0 / (constantAtt + linearAtt * distance / lightRange + quadraticAtt * ((distance/lightRange) * (distance / lightRange))));
                    diffuse += lights[i].color * lights[i].intensity * attenuation;
                }

                return col * diffuse + col * ambient;
            }
            ENDCG
        }
    }
}
