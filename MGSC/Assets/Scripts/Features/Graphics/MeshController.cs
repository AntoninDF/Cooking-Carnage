using UnityEngine;
using UnityEngine.Rendering;

namespace Features
{
    public class MeshController : Feature
    {
		#region Properties
		public MeshRenderer Renderer { get; set; }
		public virtual MeshRendererState DefaultState { get; set; }

        #endregion


        #region Methods
        public MeshController(IFeatureCollection owner, MeshRenderer target) : base(owner)
        {
			Renderer = target;
			DefaultState = new MeshRendererState(target);
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
			Debug.Log("SeET ACTIVE");
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
        #endregion
    }

}