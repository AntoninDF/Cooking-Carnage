Shader "Hidden/OutlineFullscreen"
{
    HLSLINCLUDE

    #pragma vertex Vert

    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"

    TEXTURE2D_X(_MaskTexture); // eye-space depth of outline-layer objects; sentinel = background
    float4 _OutlineColor;
    float _OutlineThickness;
    float _DepthMin;
    float _DepthMax;

    float4 FullScreenPass(Varyings varyings) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(varyings);

        float2 positionSS = varyings.positionCS.xy;
        float thickness = max(_OutlineThickness, 1);

        float centerDepth = LOAD_TEXTURE2D_X(_MaskTexture, positionSS).r;
        if (centerDepth > 999999.0)
            return float4(0,0,0,0);

        // Sample up/down/left/right, offset by thickness, and look at how far each neighbor's depth is from the center pixel's depth.
        // A big jump = either the silhouette's outer edge (neighbor is empty/background, whose depth is the large sentinel value) or an internal seam where one part
        // of the mesh occludes another.
        const int _sampleCount = 8;
        float2 offsets[_sampleCount] = {
            float2(1, 0), float2(-1, 0), float2(0, 1), float2(0, -1)
            ,float2(0.7, 0.7), float2(-0.7, 0.7), float2(0.7, -0.7), float2(-0.7, -0.7)
        };

        float maxDelta = 0;
        for (int i = 0; i < _sampleCount; i++)
        {
            float2 samplePos = positionSS + offsets[i] * thickness;
            float neighborDepth = LOAD_TEXTURE2D_X(_MaskTexture, samplePos).r;
            maxDelta = max(maxDelta, abs(neighborDepth - centerDepth));
        }

        float edge = smoothstep(_DepthMin, _DepthMax, maxDelta);

        if (edge < 0.01)
            return float4(0, 0, 0, 0); 

        return float4(_OutlineColor.rgb, edge * _OutlineColor.a);
    }

    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "OutlineFullscreen"

            ZWrite Off
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off

            HLSLPROGRAM
            #pragma fragment FullScreenPass
            ENDHLSL
        }
    }
    Fallback Off
}
