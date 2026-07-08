using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering.RendererUtils;


[System.Serializable]
public class OutlineCustomPass : CustomPass
{
    [Header("Setup")]
    public LayerMask outlineLayer = 0;          
    public Material fullscreenOutlineMaterial;  

    [Header("Look")]
    public Color outlineColor = Color.white;
    [Range(1f, 10f)]
    public float outlineThickness = 3f;
   
    public float depthMin = 0.05f;
    public float depthMax = 1.5f;

   
    const float k_DepthSentinel = 1e6f;

    RTHandle maskBuffer;
    ShaderTagId[] shaderTags;
    Material maskMaterial;
    MaterialPropertyBlock mpb;

    static readonly int s_OutlineColorId = Shader.PropertyToID("_OutlineColor");
    static readonly int s_OutlineThicknessId = Shader.PropertyToID("_OutlineThickness");
    static readonly int s_DepthMinId = Shader.PropertyToID("_DepthMin");
    static readonly int s_DepthMaxId = Shader.PropertyToID("_DepthMax");
    static readonly int s_MaskTextureId = Shader.PropertyToID("_MaskTexture");

    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        
        maskBuffer = RTHandles.Alloc(
            Vector2.one, TextureXR.slices, dimension: TextureXR.dimension,
            colorFormat: UnityEngine.Experimental.Rendering.GraphicsFormat.R32_SFloat,
            useDynamicScale: false, name: "Outline Depth Mask Buffer"
        );
        
        shaderTags = new ShaderTagId[]
        {
            new ShaderTagId("Forward"),
            new ShaderTagId("ForwardOnly"),
            new ShaderTagId("SRPDefaultUnlit"),
        };
        
        var maskShader = Shader.Find("Hidden/OutlineDepthMask");
        if (maskShader != null)
            maskMaterial = CoreUtils.CreateEngineMaterial(maskShader);

        mpb = new MaterialPropertyBlock();
    }

    protected override void Execute(CustomPassContext ctx)
    {
        if (fullscreenOutlineMaterial == null || maskMaterial == null)
            return;

        
        CoreUtils.SetRenderTarget(ctx.cmd, maskBuffer, ctx.cameraDepthBuffer, ClearFlag.Color, new Color(k_DepthSentinel, 0, 0, 0));

        var rendererListDesc = new RendererListDesc(shaderTags, ctx.cullingResults, ctx.hdCamera.camera)
        {
            renderQueueRange = RenderQueueRange.all,
            sortingCriteria = SortingCriteria.CommonOpaque,
            layerMask = outlineLayer,
            overrideMaterial = maskMaterial,
            overrideMaterialPassIndex = 0,
        };

        var rendererList = ctx.renderContext.CreateRendererList(rendererListDesc);
        ctx.cmd.DrawRendererList(rendererList);
        
        mpb.SetTexture(s_MaskTextureId, maskBuffer);
        mpb.SetColor(s_OutlineColorId, outlineColor);
        mpb.SetFloat(s_OutlineThicknessId, outlineThickness);
        mpb.SetFloat(s_DepthMinId, depthMin);
        mpb.SetFloat(s_DepthMaxId, depthMax);

        CoreUtils.SetRenderTarget(ctx.cmd, ctx.cameraColorBuffer);
        CoreUtils.DrawFullScreen(ctx.cmd, fullscreenOutlineMaterial, mpb, shaderPassId: 0);
    }

    protected override void Cleanup()
    {
        maskBuffer?.Release();
        CoreUtils.Destroy(maskMaterial);
    }
}