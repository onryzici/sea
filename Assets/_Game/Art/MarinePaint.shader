Shader "SalvageCrew/Weathered Marine Paint"
{
    Properties { [MainColor] _BaseColor("Paint",Color)=(.05,.45,.43,1) _RustColor("Oxidation",Color)=(.31,.12,.045,1) _Wear("Wear coverage",Range(0,1))=.24 _Smoothness("Smoothness",Range(0,1))=.32 _Metallic("Metallic",Range(0,1))=.2 }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            struct A {float4 vertex:POSITION;float3 normal:NORMAL;};
            struct V {float4 pos:SV_POSITION;float3 world:TEXCOORD0;float3 normal:TEXCOORD1;float3 local:TEXCOORD2;float fog:TEXCOORD3;};
            CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor,_RustColor;half _Wear,_Smoothness,_Metallic;
            CBUFFER_END
            float Hash(float3 p){return frac(sin(dot(p,float3(127.1,311.7,74.7)))*43758.5453);}
            float Noise(float3 p)
            {
                float3 i=floor(p),f=frac(p);f=f*f*(3-2*f);
                return lerp(lerp(lerp(Hash(i),Hash(i+float3(1,0,0)),f.x),lerp(Hash(i+float3(0,1,0)),Hash(i+float3(1,1,0)),f.x),f.y),lerp(lerp(Hash(i+float3(0,0,1)),Hash(i+float3(1,0,1)),f.x),lerp(Hash(i+float3(0,1,1)),Hash(i+1),f.x),f.y),f.z);
            }
            V Vert(A a){V o;o.world=TransformObjectToWorld(a.vertex.xyz);o.local=a.vertex.xyz;o.pos=TransformWorldToHClip(o.world);o.normal=TransformObjectToWorldNormal(a.normal);o.fog=ComputeFogFactor(o.pos.z);return o;}
            half4 Frag(V i):SV_Target
            {
                float pattern=Noise(i.local*13)+Noise(i.local*49)*.26;float chipped=1-smoothstep(_Wear-.04,_Wear+.03,pattern);float grain=Noise(i.local*120);
                SurfaceData s=(SurfaceData)0;s.albedo=lerp(_BaseColor.rgb*(.94+grain*.12),_RustColor.rgb*(.85+grain*.25),chipped);s.metallic=lerp(_Metallic,0,chipped);s.smoothness=lerp(_Smoothness,.09,chipped);s.normalTS=half3(0,0,1);s.occlusion=1;s.alpha=1;
                InputData d=(InputData)0;d.positionWS=i.world;d.normalWS=normalize(i.normal);d.viewDirectionWS=GetWorldSpaceNormalizeViewDir(i.world);d.shadowCoord=TransformWorldToShadowCoord(i.world);d.bakedGI=max(SampleSH(d.normalWS),half3(.20,.24,.23));d.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(i.pos);d.shadowMask=half4(1,1,1,1);
                half4 color=UniversalFragmentPBR(d,s);color.rgb=MixFog(color.rgb,i.fog);return color;
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
        UsePass "Universal Render Pipeline/Lit/DepthNormals"
    }
}
