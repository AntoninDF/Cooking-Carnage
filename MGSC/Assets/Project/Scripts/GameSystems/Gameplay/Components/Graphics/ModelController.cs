using UnityEngine;
using UnityEngine.Rendering;

namespace Features
{
    public class ModelController : MonoBehaviour, IFeatureCollection
    {
        #region Properties
        public FeatureCollection Collection { get; set; } = new ();
        public MultiMeshController MeshController { get; set; }
        public F_BoundsFinder BoundsFinder { get; set; }

        #endregion

        #region Fields
        [Tooltip("If set to true, every change made to this model will also be applied on child ModelControllers")]
        [SerializeField] private bool _propagateChangesToChildren = true;
        [Tooltip("Set it to 0 to disable. Otherwise, if the volume of the object's bounding box is smaller than this, the object won't cast shadows")]
        [SerializeField] private float _disableShadowsIfSmallerThan = 0.5f;

        #endregion

        #region Methods
        private void Awake()
        {
            
            BoundsFinder = new(this, transform);


            MeshController = new(this, DetectControlledRenderers());

        }

        private void Start()
        {

        }
        private MeshRenderer[] DetectControlledRenderers()
        {
            MeshRenderer[] result;
            if (_propagateChangesToChildren)
                result = GetComponentsInChildren<MeshRenderer>();
            else
                result = new MeshRenderer[1] { GetComponent<MeshRenderer>() };
            return result;
        }


        public void TryDisableShadows()
        {
            if(BoundsFinder.GetVolume() < _disableShadowsIfSmallerThan)
                DisableShadows();
        }

        

        #region Forwarded

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
        // Forward all the outline methods
        public virtual void ToggleOutline() => MeshController.ToggleOutline();
        public virtual void EnableOutline() => MeshController.EnableOutline();
        public virtual void DisableOutline() => MeshController.DisableOutline();
        public virtual void SetOutline(bool active) => MeshController.SetOutline(active);
        public virtual void SetOutlineColor(Color color) => MeshController.SetOutlineColor(color);
        public virtual void SetOutlineThickness(float thickness) => MeshController.SetOutlineThickness(thickness);
        public virtual void SetOutlineVisible() => MeshController.SetOutlineVisible();
        public virtual void SetOutlineInvisible() => MeshController.SetOutlineInvisible();
        public virtual void SetOutlineVisibility(bool visible) => MeshController.SetOutlineVisibility(visible);
        #endregion
        #endregion
    }
}