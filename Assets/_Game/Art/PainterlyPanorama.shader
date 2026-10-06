Shader "SalvageCrew/Painterly Panorama"
{
    Properties
    {
        _MainTex("Authored high resolution panorama",2D)="white"{}
        _Rotation("Rotation",Float)=110
        _Zenith("Zenith",Color)=(.065,.39,.65,1)
        _Horizon("Horizon",Color)=(.52,.78,.81,1)
        _CloudLight("Sunlit cloud",Color)=(1,.92,.76,1)
        _CloudShade("Cloud shade",Color)=(.34,.49,.62,1)
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct A{float3 vertex:POSITION;};
            struct V{float4 position:SV_POSITION;float3 ray:TEXCOORD0;};
            TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
            half4 _Zenith,_Horizon,_CloudLight,_CloudShade;
            float _Rotation;
            CBUFFER_END
            V Vert(A a){V o;o.position=TransformObjectToHClip(a.vertex);o.ray=a.vertex;return o;}
            half4 Frag(V i):SV_Target
            {
                float3 d=normalize(i.ray);
                float2 uv=float2(.5-atan2(d.z,d.x)/(2*PI),1-acos(clamp(d.y,-1,1))/PI);
                uv.x=frac(uv.x+_Rotation/360);
                // Keep the original 8K silhouette; simplify photographic tonal micro-detail.
                half3 source=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,uv).rgb;
                half3 broad=SAMPLE_TEXTURE2D_LOD(_MainTex,sampler_MainTex,uv,3).rgb;
                float neutral=saturate(min(source.r,source.g)/max(.03,source.b));
                float cloud=smoothstep(.30,.72,neutral)*smoothstep(.035,.18,dot(source,half3(.3,.5,.2)));
                half3 clear=lerp(_Horizon.rgb,_Zenith.rgb,pow(saturate(d.y),.38));
                float light=pow(saturate(dot(broad,half3(.3,.5,.2))),1.6);
                half3 clouds=lerp(_CloudShade.rgb,_CloudLight.rgb,light);
                return half4(lerp(clear,clouds,cloud),1);
            }
            ENDHLSL
        }
    }
}
