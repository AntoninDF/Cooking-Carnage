using System;
using System.Collections.Generic;
using System.Linq;
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
        public MultiMeshController(IFeatureCollection owner, params Renderer[] renderers) : base(owner)
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
        public Mesh GetCombinedMeshes(Transform relativeTo=null, bool combineInactiveMeshes=false)
        {
            //if (Controllers.Length == 0) return new();

            List<CombineInstance> instances = new ();

            Mesh[] meshes;
            Transform[] transforms;
            
            // Linqzilla
            if (relativeTo == null)
            {
                meshes = Controllers
                        .Where(controller => controller.Renderer.enabled)
                        .Select(controller => controller.GetMesh())
                        .ToArray();
                transforms = Controllers
                    .Where(controller => controller.Renderer.enabled)
                    .Select(controller => controller.Renderer.transform)
                    .ToArray();
            }
            else
            {
                meshes = Controllers
                        .Where(controller => controller.Renderer.enabled)
                        .Select(controller => controller.GetMesh())
                        .Concat(
                            relativeTo.GetComponentsInChildren<SkinnedMeshRenderer>()
                            .Where(renderer => renderer.enabled)
                            .Select(renderer => renderer.sharedMesh)
                            )
                        .ToArray();
                transforms = Controllers
                    .Where(controller => controller.Renderer.enabled)
                    .Select(controller => controller.Renderer.transform)
                    .Concat(
                            relativeTo.GetComponentsInChildren<SkinnedMeshRenderer>()
                            .Where(renderer => renderer.enabled)
                            .Select(renderer => renderer.transform)
                            )
                    .ToArray();
            }

            for (int i = 0; i < Math.Min(meshes.Length, transforms.Length); i++)
            {
                var mesh = meshes[i];
                var transform = transforms[i];
                if (mesh == null || transform == null) continue;
                Matrix4x4 child = transform.localToWorldMatrix;
                Matrix4x4 parent = Matrix4x4.identity;
                if (relativeTo != null)
                    parent = relativeTo.worldToLocalMatrix;
                var newInstance = new CombineInstance
                {
                    mesh = mesh,
                    transform = parent * child
                };
                instances.Add(newInstance);

                if (!newInstance.mesh.isReadable)
                {
                    Debug.LogWarning("Tried to combine mesh '" + mesh.name + "', but it is not marked as read/write. Please check the import settings.");
                }
            }

            Mesh result = new();
            result.name = "Combined mesh";
            result.CombineMeshes(instances.ToArray());
            return result;
        }
        #region ForwardedMethods

        private void Apply(Action<MeshController> action) { foreach (var controller in Controllers) action(controller); }
        public virtual void ResetToDefault() => Apply(controller => controller.ResetToDefault());
        public virtual void ReplaceAllMaterialsBy(Material material) => Apply(obj => obj.ReplaceAllMaterialsBy(material));
        public virtual void AddMaterial(Material material) => Apply(obj => obj.AddMaterial(material));
        public virtual void ResetMaterialsToDefault() => Apply(obj => obj.ResetMaterialsToDefault());
        public virtual void SetActive(bool active) {Apply(obj => obj.SetActive(active)); enabled = active; }
        public virtual void AllowCastShadows(bool allowCastShadows) => Apply(obj => obj.AllowCastShadows(allowCastShadows));
        public virtual void AllowCastShadows(ShadowCastingMode shadowCastingMode) => Apply(obj => obj.AllowCastShadows(shadowCastingMode));
        public virtual void DisableShadows() => Apply(obj => obj.DisableShadows());
        public virtual void EnableShadows() => Apply(obj => obj.EnableShadows());
        public virtual void ReplaceMesh(Mesh mesh) => Apply(obj => obj.ReplaceMesh(mesh));
        public virtual void ResetMeshToDefault() => Apply(obj => obj.ResetMeshToDefault());
        public virtual void SetLayer(int layer) => Apply(obj => obj.SetLayer(layer));

        #endregion    
    }
}