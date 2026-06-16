using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Features
{
    /// <summary>
    /// MultiMeshController is just a MeshController that works on multiple Renderers. Every method is (and must be) forwarded from MeshController
    /// </summary>
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

        public Mesh[] GetMeshes()
        {
            Mesh[] meshes = new Mesh[Controllers.Length];
            for (int i = 0; i < meshes.Length; i++)
            {
                meshes[i] = Controllers[i].GetMesh();
            }
            return meshes;
        }
        public Mesh GetFirstMesh()
        {
            if (Controllers.Length == 0) return new();
            return Controllers[0].GetMesh();
        }
        /// <summary>
        /// Combines all the meshes from all the renderers tracked into one mesh, taking into account their relative positions in the world.
        /// </summary>
        /// <returns>A single mesh representing all the meshes from the MeshRenderers, with one single submesh</returns>
        public Mesh GetCombinedMeshes(Transform relativeTo=null)
        {
            if (Controllers.Length == 0) return new();

            CombineInstance[] instances = new CombineInstance[Controllers.Length];

            for (int i = 0; i < Controllers.Length; i++)
            {
                var Controller = Controllers[i];

                Matrix4x4 child = Controller.Renderer.transform.localToWorldMatrix;
                Matrix4x4 parent = Matrix4x4.identity;
                if (relativeTo != null)
                    parent = relativeTo.worldToLocalMatrix;
                instances[i] = new CombineInstance
                {
                    mesh = Controller.GetMesh(),
                    transform = parent * child
                };
            }

            Mesh result = new();
            result.name = "Combined mesh";
            result.CombineMeshes(instances);
            return result;
        }
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
        // Forward all the outline methods
        public virtual void ToggleOutline() => Apply(obj => obj.ToggleOutline());
        public virtual void EnableOutline() => Apply(obj => obj.EnableOutline());
        public virtual void DisableOutline() => Apply(obj => obj.DisableOutline());
        public virtual void SetOutline(bool active) => Apply(obj => obj.SetOutline(active));
        public virtual void SetOutlineColor(Color color) => Apply(obj => obj.SetOutlineColor(color));
        public virtual void SetOutlineThickness(float thickness) => Apply(obj => obj.SetOutlineThickness(thickness));
        public virtual void SetOutlineVisible() => Apply(obj => obj.SetOutlineVisible());
        public virtual void SetOutlineInvisible() => Apply(obj => obj.SetOutlineInvisible());
        public virtual void SetOutlineVisibility(bool visible) => Apply(obj => obj.SetOutlineVisibility(visible));

        #endregion    
    }
}