using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Features
{
    public class MeshController : Feature, IFeatureCollection
    {
		#region Properties
		public MeshRenderer Renderer { get; set; }
		public virtual MeshRendererState DefaultState { get; set; }

		public FeatureCollection Collection { get; set; } = new();

        public OutlineController OutlineController;

        #endregion


        #region Methods
        public MeshController(IFeatureCollection owner, MeshRenderer target) : base(owner)
        {
			Renderer = target;
			DefaultState = new MeshRendererState(target);
			OutlineController = new(this, target);
        }

		public virtual void ResetToDefault()
		{
			DefaultState.SetState(Renderer);
		}

        public virtual void ReplaceAllMaterialsBy(Material material)
		{
			var materials = Renderer.materials;
			Material[] newMaterials = new Material[materials.Length];

			for(int i =0; i < materials.Length; i++)
			{
				newMaterials[i] = material;
			}

			Renderer.materials = newMaterials;
		}
		public virtual void AddMaterial(Material material)
		{
            var materials = Renderer.materials;
            Material[] newMaterials = new Material[materials.Length + 1];

            for (int i = 0; i < materials.Length; i++)
            {
                newMaterials[i] = materials[i];
            }
            newMaterials[materials.Length] = material;

            Renderer.materials = newMaterials;
        }
        public virtual void ResetMaterialsToDefault()
        {
            Renderer.sharedMaterials = DefaultState.sharedMaterials;
        }
		/// <summary>
		/// Controls the 'enabled' property of the controlled renderer
		/// </summary>
		/// <param name="active"></param>
		public virtual void SetActive(bool active)
		{
			Renderer.enabled = active;
		}
		public virtual void AllowCastShadows(bool allowCastShadows)
		{
			if (allowCastShadows)
				Renderer.shadowCastingMode = ShadowCastingMode.On;
            else
                Renderer.shadowCastingMode = ShadowCastingMode.Off;
        }
		public virtual void AllowCastShadows(ShadowCastingMode shadowCastingMode) => Renderer.shadowCastingMode = shadowCastingMode;
		public virtual void DisableShadows() => AllowCastShadows(false);
		public virtual void EnableShadows() => AllowCastShadows(true);
		public virtual void ReplaceMesh(Mesh mesh)
		{
			if (Renderer.TryGetComponent(out MeshFilter filter))
				filter.mesh = mesh;
		}
		public virtual void ResetMeshToDefault()
        {
            if (Renderer.TryGetComponent(out MeshFilter filter))
                filter.mesh = DefaultState.sharedMesh;
        }
		public Mesh GetMesh()
		{
			if (Renderer.TryGetComponent(out MeshFilter filter))
				return filter.mesh;
			return new();
        }

		// Forward all the outline methods
		public virtual void ToggleOutline() => OutlineController.ToggleOutline();
		public virtual void EnableOutline() => OutlineController.EnableOutline();
		public virtual void DisableOutline() => OutlineController.DisableOutline();
		public virtual void SetOutline(bool active) => OutlineController.SetOutline(active);
		public virtual void SetOutlineColor(Color color) => OutlineController.SetColor(color);
		public virtual void SetOutlineThickness(float thickness) => OutlineController.SetThickness(thickness);
        public virtual void SetOutlineVisible() => OutlineController.SetVisible();
        public virtual void SetOutlineInvisible() => OutlineController.SetInvisible();
		public virtual void SetOutlineVisibility(bool visible) => OutlineController.SetVisibility(visible);


        #endregion
    }

}