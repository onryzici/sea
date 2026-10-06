Shader "SalvageCrew/Calm Harbor Water"
{
    Properties
    {
        _DeepColor("Deep water",Color)=(0.012,0.22,0.27,1)
        _ShallowColor("Shallow water",Color)=(0.09,0.60,0.56,1)
        _WaveHeight("Open ocean wave amplitude",Range(0,1.5))=0.85
        _Speed("Wave speed",Range(0,2))=0.6
        _Environment("Reflected sky",2D)="gray" {}
        _RippleNormal("Authored water normal",2D)="bump" {}
        _NormalStrength("Small wave strength",Range(0,1))=.28
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent-20" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            struct Attributes { float4 positionOS:POSITION; };
            struct Varyings { float4 positionCS:SV_POSITION; float3 world:TEXCOORD0; float fog:TEXCOORD1; float eyeDepth:TEXCOORD2; };
            CBUFFER_START(UnityPerMaterial)
            half4 _DeepColor, _ShallowColor;
            float _Speed, _WaveHeight, _NormalStrength;
            CBUFFER_END
            TEXTURE2D(_Environment);SAMPLER(sampler_Environment);
            TEXTURE2D(_RippleNormal);SAMPLER(sampler_RippleNormal);
            float _HarborWaterTime;
            float Hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453); }
            float Noise(float2 p) { float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(Hash(i),Hash(i+float2(1,0)),f.x),lerp(Hash(i+float2(0,1)),Hash(i+1),f.x),f.y); }
            float Fbm(float2 p) { return Noise(p)*.55+Noise(p*2.07+13)*.28+Noise(p*4.13-7)*.17; }
            float Exposure(float2 p) {return lerp(.28,1,smoothstep(18,100,length((p-float2(7,10))*float2(.8,1))))*(.9+.1*sin(p.x*.003+p.y*.008));}
            float Height(float2 p,float t) {return _WaveHeight*Exposure(p)*(sin(dot(p,float2(.26,.13))+t*.8)*.50+sin(dot(p,float2(-.17,.31))-t*.93)*.28+sin(dot(p,float2(.62,.42))+t*1.31)*.14+sin(dot(p,float2(-.91,.32))-t*1.71)*.08);}
            // Shading-only capillary waves: two crossing, domain-warped noise fields.
            // The four buoyancy waves above deliberately remain identical to HarborWater.Height.
            float Ripple(float2 p,float t)
            {
                float2 warp=float2(Noise(p*.19+t*.035),Noise(p*.23+17-t*.027))-.5;
                float2 a=p*.65+warp*1.4+float2(t*.15,-t*.09);
                float2 b=float2(p.x*.64+p.y*.77,-p.x*.77+p.y*.64)*1.3-float2(t*.11,t*.13);
                return Noise(a)*.70+Noise(b)*.30;
            }
            Varyings Vert(Attributes v) { Varyings o; o.world=TransformObjectToWorld(v.positionOS.xyz);float range=distance(o.world.xz,_WorldSpaceCameraPos.xz);float t=_HarborWaterTime*_Speed;o.world.y+=Height(o.world.xz,t)*(1-smoothstep(80,180,range));o.world.y+=(Ripple(o.world.xz,t)-.5)*.18*(1-smoothstep(35,90,range));o.positionCS=TransformWorldToHClip(o.world);o.fog=ComputeFogFactor(o.positionCS.z);o.eyeDepth=-TransformWorldToView(o.world).z;return o; }
            half4 Frag(Varyings i):SV_Target
            {
                float2 p=i.world.xz;float t=_HarborWaterTime*_Speed;float2 drift=float2(t*.24,-t*.13);
                float h=Height(p,t),dx=(Height(p+float2(.12,0),t)-h)/.12,dz=(Height(p+float2(0,.12),t)-h)/.12;
                float range=distance(i.world,_WorldSpaceCameraPos);
                float swellFade=1-smoothstep(80,180,range);
                float detail=Ripple(p,t);
                float detailFade=1-smoothstep(35,160,range);
                float2 normalUV=p*.095;
                float3 microA=UnpackNormal(SAMPLE_TEXTURE2D(_RippleNormal,sampler_RippleNormal,normalUV+float2(t*.006,t*.008)));
                float3 microB=UnpackNormal(SAMPLE_TEXTURE2D(_RippleNormal,sampler_RippleNormal,float2(-normalUV.y,normalUV.x)*1.37+float2(t*.004,-t*.009)));
                float2 micro=(microA.xy+float2(microB.y,-microB.x))*.5;
                dx=dx*.4*swellFade+micro.x*_NormalStrength*detailFade;
                dz=dz*.4*swellFade+micro.y*_NormalStrength*detailFade;
                float3 n=normalize(float3(-dx*1.5,1,-dz*1.5)),v=GetWorldSpaceNormalizeViewDir(i.world);
                float2 uv=i.positionCS.xy/_ScaledScreenParams.xy;
                float depth=max(0,LinearEyeDepth(SampleSceneDepth(uv),_ZBufferParams)-i.eyeDepth);
                float2 refractedUV=uv+n.xz*.006*saturate(depth);
                if(LinearEyeDepth(SampleSceneDepth(refractedUV),_ZBufferParams)<i.eyeDepth)refractedUV=uv;
                half3 submerged=SampleSceneColor(refractedUV);
                float raw=SampleSceneDepth(uv);
                float3 bottom=ComputeWorldSpacePosition(uv,raw,UNITY_MATRIX_I_VP);
                float thickness=max(0,i.world.y-bottom.y);
                // Surface colour is not a flat green silhouette of every submerged collider.
                // Beer-Lambert attenuation retains the sand close to shore and hides deep hulls.
                float transmittance=exp(-depth*.42);
                half3 water=lerp(_ShallowColor.rgb,_DeepColor.rgb,1-exp(-thickness*.32));
                half3 transmission=submerged*half3(.55,.84,.87)*exp(-half3(.32,.095,.07)*depth);
                water=lerp(water,transmission,transmittance*.78);
                Light sun=GetMainLight(TransformWorldToShadowCoord(i.world));
                float diffuse=saturate(dot(n,sun.direction));
                water*=.62+diffuse*.48*sun.shadowAttenuation;
                // Luminous turquoise on forward-facing crests, darker blue in troughs.
                float peak=saturate(h/max(.01,_WaveHeight*Exposure(p))*.9+.25);
                float scatter=pow(saturate(dot(v,-sun.direction+float3(0,.4,0))),3);
                float patch=smoothstep(.25,.8,Fbm(p*.21+drift*.1));
                water+=_ShallowColor.rgb*scatter*peak*patch*.3*swellFade;
                float fresnel=.035+.965*pow(1-saturate(dot(n,v)),5);
                float3 reflected=reflect(-v,n);
                float2 skyUV=float2(atan2(reflected.x,reflected.z)/(2*PI)+.5,acos(clamp(reflected.y,-1,1))/PI);
                skyUV.x=frac(skyUV.x+110.0/360.0);
                half3 sky=SAMPLE_TEXTURE2D_LOD(_Environment,sampler_Environment,skyUV,4).rgb;
                sky=lerp(half3(.035,.26,.38),sky,.20);
                water=lerp(water,sky,fresnel*.46);
                float nh=saturate(dot(n,normalize(v+sun.direction)));
                float spec=pow(nh,120)*.8+pow(nh,18)*.025;
                water+=sun.color*spec*saturate(dot(n,sun.direction))*sun.shadowAttenuation;
                // Foam appears at geometry contact, not as a uniform dotted pattern.
                float edge=1-smoothstep(.015,.18,thickness);
                float foam=edge*smoothstep(.40,.68,Fbm(p*1.2+float2(t*.10,t*.05)))*.46;
                // Sparse soft crest strokes, not a photographic micro-normal or a tiled dot grid.
                float crest=smoothstep(.67,.92,peak)*smoothstep(.48,.7,Fbm(p*.37-drift*.15));
                float lace=1-smoothstep(.025,.11,abs(Fbm(p*1.6+drift*.18)-.5));
                foam=max(foam,crest*lace*.45*smoothstep(.35,.75,Exposure(p))*swellFade);
                water=lerp(water,half3(.83,.94,.91),foam);
                return half4(MixFog(water,i.fog),1);
            }
            ENDHLSL
        }
    }
}
