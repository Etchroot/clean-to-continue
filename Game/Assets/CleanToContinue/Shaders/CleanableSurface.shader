Shader "CleanToContinue/Cleanable Surface"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _BumpScale ("Normal Scale", Range(0, 2)) = 1
        _MetallicGlossMap ("Metallic Map", 2D) = "white" {}
        _Metallic ("Metallic", Range(0, 1)) = 0
        _DustMask ("Dust Remaining Mask", 2D) = "white" {}
        _PolishRemainingMask ("Polish Remaining Mask", 2D) = "white" {}
        _DustColor ("Dust Color", Color) = (0.459, 0.435, 0.4, 1)
        _DustOpacity ("Dust Opacity", Range(0, 1)) = 0.55
        _DirtySmoothness ("Dirty Smoothness", Range(0, 1)) = 0.08
        _CleanSmoothness ("Clean Smoothness", Range(0, 1)) = 0.72
        _HighlightPulse ("Highlight Pulse", Range(0, 1)) = 0
        [HDR] _HighlightColor ("Highlight Color", Color) = (1.4, 1.05, 0.45, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_BumpMap);
            SAMPLER(sampler_BumpMap);
            TEXTURE2D(_MetallicGlossMap);
            SAMPLER(sampler_MetallicGlossMap);
            TEXTURE2D(_DustMask);
            SAMPLER(sampler_DustMask);
            TEXTURE2D(_PolishRemainingMask);
            SAMPLER(sampler_PolishRemainingMask);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half _BumpScale;
                half _Metallic;
                half4 _DustColor;
                half _DustOpacity;
                half _DirtySmoothness;
                half _CleanSmoothness;
                half _HighlightPulse;
                half4 _HighlightColor;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                half fogFactor : TEXCOORD3;
                float4 shadowCoord : TEXCOORD4;
                half4 tangentWS : TEXCOORD5;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positions = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normals = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.positionCS = positions.positionCS;
                output.positionWS = positions.positionWS;
                output.normalWS = normals.normalWS;
                output.tangentWS = half4(
                    normals.tangentWS,
                    input.tangentOS.w * GetOddNegativeScale());
                output.uv = input.uv;
                output.fogFactor = ComputeFogFactor(positions.positionCS.z);
                output.shadowCoord = GetShadowCoord(positions);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 baseUv = TRANSFORM_TEX(input.uv, _BaseMap);
                half4 baseSample = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, baseUv) * _BaseColor;
                half3 normalTS = UnpackNormalScale(
                    SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, baseUv),
                    _BumpScale);
                half metallic = SAMPLE_TEXTURE2D(
                    _MetallicGlossMap,
                    sampler_MetallicGlossMap,
                    baseUv).r * _Metallic;
                half dustRemaining = SAMPLE_TEXTURE2D(_DustMask, sampler_DustMask, input.uv).r;
                half polishClean = 1.0h - SAMPLE_TEXTURE2D(
                    _PolishRemainingMask,
                    sampler_PolishRemainingMask,
                    input.uv).r;

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = lerp(
                    baseSample.rgb,
                    _DustColor.rgb,
                    saturate(dustRemaining * _DustOpacity));
                surfaceData.metallic = metallic;
                surfaceData.specular = half3(0.04h, 0.04h, 0.04h);
                surfaceData.smoothness = lerp(
                    _DirtySmoothness,
                    _CleanSmoothness,
                    polishClean) * lerp(1.0h, 0.35h, dustRemaining);
                surfaceData.normalTS = normalTS;
                surfaceData.emission = _HighlightColor.rgb
                    * max(dustRemaining, 1.0h - polishClean)
                    * _HighlightPulse;
                surfaceData.occlusion = 1.0h;
                surfaceData.alpha = baseSample.a;
                surfaceData.clearCoatMask = 0.0h;
                surfaceData.clearCoatSmoothness = 0.0h;

                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                half3 bitangentWS = cross(input.normalWS, input.tangentWS.xyz)
                    * input.tangentWS.w;
                inputData.normalWS = normalize(TransformTangentToWorld(
                    normalTS,
                    half3x3(input.tangentWS.xyz, bitangentWS, input.normalWS)));
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                inputData.shadowCoord = input.shadowCoord;
                inputData.fogCoord = input.fogFactor;
                inputData.vertexLighting = VertexLighting(input.positionWS, inputData.normalWS);
                inputData.bakedGI = SampleSH(inputData.normalWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                inputData.shadowMask = half4(1, 1, 1, 1);

                half4 color = UniversalFragmentPBR(inputData, surfaceData);
                color.rgb = MixFog(color.rgb, inputData.fogCoord);
                return color;
            }
            ENDHLSL
        }

        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }

    FallBack Off
}
