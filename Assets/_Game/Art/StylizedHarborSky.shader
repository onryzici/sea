Shader "SalvageCrew/Stylized Harbor Sky"
{
 Properties { _Zenith("Blue sky",Color)=(.08,.40,.85,1) _Horizon("Horizon",Color)=(.65,.86,.95,1) }
 SubShader {
 Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
 Cull Off ZWrite Off
 Pass {
 HLSLPROGRAM
 #pragma vertex Vert
 #pragma fragment Frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 struct A{float3 vertex:POSITION;};struct V{float4 pos:SV_POSITION;float3 ray:TEXCOORD0;};
 half4 _Zenith,_Horizon;
 float Hash(float3 p){return frac(sin(dot(p,float3(127.1,311.7,74.7)))*43758.5453);}
 float Noise(float3 p){float3 a=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(lerp(Hash(a),Hash(a+float3(1,0,0)),f.x),lerp(Hash(a+float3(0,1,0)),Hash(a+float3(1,1,0)),f.x),f.y),lerp(lerp(Hash(a+float3(0,0,1)),Hash(a+float3(1,0,1)),f.x),lerp(Hash(a+float3(0,1,1)),Hash(a+1),f.x),f.y),f.z);}
 V Vert(A a){V v;v.pos=TransformObjectToHClip(a.vertex);v.ray=a.vertex;return v;}
 half4 Frag(V v):SV_Target{
 float3 d=normalize(v.ray);float y=max(d.y,.015);
 half3 sky=lerp(_Horizon.rgb,_Zenith.rgb,pow(saturate(d.y),.45));
 float3 sun=normalize(GetMainLight().direction);
 sky+=half3(1,.78,.38)*pow(saturate(dot(d,sun)),90)*.9;
 // Rounded painted masses sampled on the sky direction: stable at the horizon.
 float3 p=d*5.5+float3(2.7,.4,_Time.y*.003);
 float shape=Noise(p)*.76+Noise(p*2.2+3)*.24;
 float edge=max(fwidth(shape)*1.5,.026);
 float opacity=smoothstep(.56-edge,.56+edge,shape)*smoothstep(.005,.10,d.y);
 float lighting=smoothstep(.25,.72,Noise(p*.7+float3(0,2,0)));
 half3 cloud=lerp(half3(.73,.82,.94),half3(1.16,1.12,1.02),lighting);
 return half4(lerp(sky,cloud,opacity),1);
 }
 ENDHLSL
 }
 }
}
