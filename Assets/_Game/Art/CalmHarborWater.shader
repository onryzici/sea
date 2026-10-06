Shader "SalvageCrew/Calm Harbor Water"
{
    Properties { _DeepColor("Turquoise",Color)=(0.025,0.47,0.53,1) _ShallowColor("Sunlit surface",Color)=(0.19,0.75,0.74,1) _Speed("Surface speed",Range(0,2))=0.3 }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            struct Attributes { float4 positionOS:POSITION; };
            struct Varyings { float4 positionCS:SV_POSITION; float3 world:TEXCOORD0; };
            CBUFFER_START(UnityPerMaterial)
            half4 _DeepColor, _ShallowColor;
            float _Speed;
            CBUFFER_END
            Varyings Vert(Attributes v) { Varyings o; o.world=TransformObjectToWorld(v.positionOS.xyz); o.positionCS=TransformWorldToHClip(o.world); return o; }
            half4 Frag(Varyings i):SV_Target
            {
                float t=_Time.y*_Speed;
                float a=i.world.x*2.2+i.world.z*1.4+t;
                float b=i.world.z*3.7-i.world.x*.8-t*.7;
                float wave=sin(a)*cos(b);
                float3 n=normalize(float3(-cos(a)*.09,1,-sin(b)*.07));
                float3 v=GetWorldSpaceNormalizeViewDir(i.world);
                Light sun=GetMainLight();
                half sparkle=pow(saturate(dot(n,normalize(v+sun.direction))),90)*.45;
                half fresnel=pow(1-saturate(dot(n,v)),3);
                half3 color=lerp(_DeepColor.rgb,_ShallowColor.rgb,saturate(.35+wave*.2+fresnel*.35));
                // Thin stylized ripples only: no displacement, depth, reflection or physics dependency.
                half foam=smoothstep(.975,1,wave)*.15;
                return half4(color+sparkle*sun.color+foam,1);
            }
            ENDHLSL
        }
    }
}
