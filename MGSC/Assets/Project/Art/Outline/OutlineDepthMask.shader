// Renders outline-layer objects into a single-channel buffer storing their
// eye-space depth. Pixels with no object are left at the sentinel value the
// buffer is cleared to (see OutlineCustomPass.cs), so the fullscreen pass can
// tell "object" from "empty background" as well as internal depth seams.
Shader "Hidden/OutlineDepthMask"
{
    SubShader
    {
        Tags { "RenderPipeline" = "HDRenderPipeline" }

        Pass
        {
            Name "ForwardOnly"
            Tags { "LightMode" = "ForwardOnly" }

            ZWrite Off
            ZTest LEqual
            Cull Back

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 4.5

            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"

            struct MaskAttributes
            {
                float3 positionOS : POSITION;
            };

            struct MaskVaryings
            {
                float4 positionCS : SV_POSITION;
                float eyeDepth : TEXCOORD0;
            };

            MaskVaryings Vert(MaskAttributes input)
            {
                MaskVaryings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS);
                float3 positionVS = TransformWorldToView(positionWS);
                output.eyeDepth = -positionVS.z; // view space forward is -Z in Unity
                output.positionCS = TransformWorldToHClip(positionWS);
                return output;
            }

            float4 Frag(MaskVaryings input) : SV_Target
            {
                return float4(input.eyeDepth, 0, 0, 1);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
