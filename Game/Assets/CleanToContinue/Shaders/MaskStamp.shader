Shader "Hidden/CleanToContinue/MaskStamp"
{
    Properties
    {
        _MainTex ("Current Mask", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }

        Pass
        {
            ZTest Always
            Cull Off
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _BrushUV;
            float _BrushRadius;
            float _WriteValue;

            fixed4 frag(v2f_img input) : SV_Target
            {
                float oldValue = tex2D(_MainTex, input.uv).r;
                float distanceFromBrush = distance(input.uv, _BrushUV.xy);
                float strength = 1.0 - smoothstep(_BrushRadius * 0.75, _BrushRadius, distanceFromBrush);
                float value = lerp(oldValue, _WriteValue, strength);
                return float4(value, value, value, 1.0);
            }
            ENDHLSL
        }
    }
}
