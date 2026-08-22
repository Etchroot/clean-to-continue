Shader "CleanToContinue/Cleaning Overlay"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _DustMask ("Dust Remaining Mask", 2D) = "white" {}
        _PolishRemainingMask ("Polish Remaining Mask", 2D) = "white" {}
        _OverlayMode ("Overlay Mode", Range(0, 1)) = 0
        _DustColor ("Dust Color", Color) = (0.42, 0.38, 0.31, 1)
        _DustOpacity ("Dust Opacity", Range(0, 1)) = 0.68
        _DullOpacity ("Dull Coat Opacity", Range(0, 1)) = 0.72
        _HighlightPulse ("Highlight Pulse", Range(0, 1)) = 0
        [HDR] _HighlightColor ("Highlight Color", Color) = (1.4, 1.05, 0.45, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent+10"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "CleaningOverlay"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back
            Offset -1, -1

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_DustMask);
            SAMPLER(sampler_DustMask);
            TEXTURE2D(_PolishRemainingMask);
            SAMPLER(sampler_PolishRemainingMask);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half _OverlayMode;
                half4 _DustColor;
                half _DustOpacity;
                half _DullOpacity;
                half _HighlightPulse;
                half4 _HighlightColor;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positions = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positions.positionCS;
                output.positionWS = positions.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 baseUv = TRANSFORM_TEX(input.uv, _BaseMap);
                half4 baseSample = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, baseUv) * _BaseColor;
                half dustRemaining = SAMPLE_TEXTURE2D(_DustMask, sampler_DustMask, input.uv).r;
                half polishRemaining = SAMPLE_TEXTURE2D(
                    _PolishRemainingMask,
                    sampler_PolishRemainingMask,
                    input.uv).r;
                half mode = step(0.5h, _OverlayMode);
                half dustAlpha = dustRemaining * _DustOpacity;
                half dullAlpha = polishRemaining * _DullOpacity;
                half remaining = lerp(dustRemaining, polishRemaining, mode);
                half alpha = lerp(dustAlpha, dullAlpha, mode);
                half3 baseColor = lerp(_DustColor.rgb, baseSample.rgb, mode);
                half3 highlight = _HighlightColor.rgb * remaining * _HighlightPulse;

                return half4(baseColor + highlight, saturate(alpha + _HighlightPulse * remaining * 0.25h));
            }
            ENDHLSL
        }
    }

    FallBack Off
}
