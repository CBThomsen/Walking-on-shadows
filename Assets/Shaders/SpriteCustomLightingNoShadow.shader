Shader "Sprites/CustomLightingNoShadow"
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
                float rotation;
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
            static float PI = 3.14;

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

                if(lights.Length == 0)
                    return col;

                for(int i = 0; i < lights.Length; i++)
                {   
                    if(lights[i].isOn == 0)
                        continue;

                    float2 lightPos = lights[i].position;
                    lightPos.y = resolutionY - lightPos.y;
                    float lightRange = lights[i].range;
                    
                    float2 direction = float2(lightPos.x - pixelPos.x, lightPos.y - pixelPos.y);
                    float distance = sqrt(pow(direction.x, 2) + pow(direction.y, 2));
                    
                    if(distance > lightRange)
                        continue;
        
                    float2 lightMidVector = float2(cos(lights[i].rotation), sin(lights[i].rotation));
                    float angleBetweenMidAndRay = acos(dot(normalize(float2(direction.x, -direction.y)), lightMidVector)) * 180.0 / PI;

                    if(angleBetweenMidAndRay >= lights[i].angle * 0.5)
                        continue;

                    float outerRadius = lightRange * 0.75;
                    float innerRadius = lightRange - outerRadius;

                    float constantAtt = 1.0;
                    float linearAtt = 0.0;
                    float quadraticAtt = 1.0;
                    float attenuation = (1.0 / (constantAtt + linearAtt * distance / lightRange + quadraticAtt * ((distance/lightRange) * (distance / lightRange))));

                    if(distance > innerRadius)
                    {
                        attenuation *= 1.0 - (distance - innerRadius) / outerRadius;
                    }

                    diffuse += lights[i].color * lights[i].intensity * attenuation;
                }

                float4 outputColor = col * ambient + col * diffuse;
                outputColor.a = col.a;
                return outputColor;
            }
            ENDCG
        }
  
    }
}
