using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Features
{
    public class MultiMeshController : Feature, IFeatureCollection
    {
        public MeshController[] Controllers { get; set; }
        public MultiMeshController(IFeatureCollection owner, params MeshRenderer[] renderers) : base(owner)
        {
            Controllers = new MeshController[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                Controllers[i] = new MeshController(this, renderers[i]);
            }
        }

        public FeatureCollection Collection { get; set; } = new();


        #region ForwardedMethods

        private void Apply(Action<MeshController> action) { foreach (var controller in Controllers) action(controller); }
        public virtual void ResetToDefault() => Apply(controller => controller.ResetToDefault());
        public virtual void ReplaceAllMaterialsBy(Material material) => Apply(obj => obj.ReplaceAllMaterialsBy(material));
        public virtual void AddMaterial(Material material) => Apply(obj => obj.AddMaterial(material));
        public virtual void ResetMaterialsToDefault() => Apply(obj => obj.ResetMaterialsToDefault());
        public virtual void SetActive(bool active) => Apply(obj => obj.SetActive(active));
        public virtual void AllowCastShadows(bool allowCastShadows) => Apply(obj => obj.AllowCastShadows(allowCastShadows));
        public virtual void AllowCastShadows(ShadowCastingMode shadowCastingMode) => Apply(obj => obj.AllowCastShadows(shadowCastingMode));
        public virtual void DisableShadows() => Apply(obj => obj.DisableShadows());
        public virtual void EnableShadows() => Apply(obj => obj.EnableShadows());
        public virtual void ReplaceMesh(Mesh mesh) => Apply(obj => obj.ReplaceMesh(mesh));
        public virtual void ResetMeshToDefault() => Apply(obj => obj.ResetMeshToDefault());

        #endregion    
    }
}