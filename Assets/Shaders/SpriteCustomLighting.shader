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
        ZWrite Off

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

                float4 ambient = float4(0.25, 0.25, 0.25, 0.25);
                float4 diffuse = float4(0.0, 0.0, 0.0, 0.0);
                for(int i = 0; i < lights.Length; i++)
                {                
                    float2 lightPos = lights[i].position;
                    float x1 = vi.vertex.x;
                    float y1 = vi.vertex.y;
                    float x2 = lightPos.x;
                    float y2 = resolution - lightPos.y;
                    
                    float2 direction = float2(x2 - x1, y2 - y1);
                    //float angle = fmod(atan2(direction.y, -direction.x) + lightAngle + 2 * PI, 2 * PI);
                    float dist = sqrt(pow(direction.x, 2) + pow(direction.y, 2));
                    float2 directionNormalized = direction / dist;
                    float maxDistance = lights[i].range;
                    
                    if(dist > maxDistance)
                        continue;
                    
                    float constantAtt = 1.0;
                    float linearAtt = 0.0;
                    float quadraticAtt = 3.0;
                    float attenuation = (1.0 / (constantAtt + linearAtt * dist / maxDistance + quadraticAtt * ((dist/maxDistance) * (dist / maxDistance))));
                    diffuse += lights[i].color * lights[i].intensity * attenuation;
                }

                return col * diffuse + col * ambient;
            }
            ENDCG
        }
    }
}
