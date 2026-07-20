using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using Features;

public class ModelController : MonoBehaviour, IFeatureCollection
{
    #region Properties
    public FeatureCollection Collection { get; set; } = new();
    public MultiMeshController MeshController { get; set; }
    public F_BoundsFinder BoundsFinder { get; set; }
    public OutlineController Outline { get; set; }

    #endregion

    #region Fields
    [Tooltip("If set to true, every change made to this model will also be applied on child ModelControllers")]
    [SerializeField] private bool _propagateChangesToChildren = true;
    [Tooltip("Set it to 0 to disable. Otherwise, if the volume of the object's bounding box is smaller than this, the object won't cast shadows")]
    [SerializeField] private float _disableShadowsIfSmallerThan = 0.5f;
    [SerializeField] private bool _allowCustomOutlineValues = false;
    [SerializeField] private Globals.Rendering.Outline.OutlineOptions _outline = Globals.Rendering.Outline.Options;
    #endregion

    #region Methods
    private void Awake()
    {
        BoundsFinder = new(this, transform);

        MeshController = new(this, DetectControlledRenderers());

        Outline = new(this, MeshController.GetCombinedMeshes(transform), transform, DetectControlledRenderers());
            
        if (!_allowCustomOutlineValues)
        {
            _outline = Globals.Rendering.Outline.Options;
        }
    }

    private void Start()
    {

        Shader shader = Shader.Find("CookingCarnage/SH_Outline");
        //Debug.Log("Shader is '" + shader + "'");
        Outline.SetParameters(_outline);
        MeshController.OnDisable += Outline.DisableOutline;
    }
    void OnValidate()
    {
        if (Outline != null)
            Outline.SetParameters(_outline);
    }
    private Renderer[] DetectControlledRenderers()
    {
        Renderer[] result;
        if (_propagateChangesToChildren)
            result = GetComponentsInChildren<Renderer>()
                .Where(r => r is MeshRenderer || r is SkinnedMeshRenderer) //Doesn't take the other renderer like ParticleSystemRenderer
                .ToArray();
        else
        {
            var r = GetComponent<Renderer>();
            result = (r is MeshRenderer || r is SkinnedMeshRenderer) ? new Renderer[1] { r } : Array.Empty<Renderer>();
        }
        
        return result;
    }


    public void TryDisableShadows()
    {
        if(BoundsFinder.GetVolume() < _disableShadowsIfSmallerThan)
            DisableShadows();
    }


    public void AuthorizeOutlines(bool allow)
    {
        Outline.CanShowOutline = allow;
    }
    
    

    #region Forwarded
    public virtual Mesh GetCombinedMeshes(Transform relativeTo=null) => MeshController.GetCombinedMeshes(relativeTo);
    public virtual void ResetToDefault() => MeshController.ResetToDefault();
    public virtual void ReplaceAllMaterialsBy(Material material) => MeshController.ReplaceAllMaterialsBy(material);
    public virtual void AddMaterial(Material material) => MeshController.AddMaterial(material);
    public virtual void ResetMaterialsToDefault() => MeshController.ResetMaterialsToDefault();
    public virtual void SetActive(bool active) => MeshController.SetActive(active);
    public virtual void AllowCastShadows(bool allowCastShadows) => MeshController.AllowCastShadows(allowCastShadows);
    public virtual void AllowCastShadows(ShadowCastingMode shadowCastingMode) => MeshController.AllowCastShadows(shadowCastingMode);
    public virtual void DisableShadows() => MeshController.DisableShadows();
    public virtual void EnableShadows() => MeshController.EnableShadows();
    public virtual void ReplaceMesh(Mesh mesh) => MeshController.ReplaceMesh(mesh);
    public virtual void ResetMeshToDefault() => MeshController.ResetMeshToDefault();
    public virtual void SetLayer(int layer) => MeshController.SetLayer(layer);

    #endregion
    #endregion
}
