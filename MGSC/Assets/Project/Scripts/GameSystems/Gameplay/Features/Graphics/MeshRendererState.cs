using UnityEngine;
using UnityEngine.Rendering;

namespace Features
{
    public class MeshRendererState
	{
		public bool enabled {  get; set; }
		public Transform transform { get; set; }
		public Material material {  get; set; }
		public Material[] materials {  get; set; }
		public Material sharedMaterial {  get; set; }
		public Material[] sharedMaterials {  get; set; }
		public Bounds bounds { get; set; }
        public ShadowCastingMode shadowCastingMode { get; set; }
		public Mesh sharedMesh { get; set; }


		#region Methods
		public MeshRendererState(MeshRenderer renderer)
		{
			GetState(renderer);
		}

		public void GetState(MeshRenderer renderer)
		{
			enabled = renderer.enabled;
			transform = renderer.transform;
			material = renderer.material;
			materials = renderer.materials;
			sharedMaterial = renderer.sharedMaterial;
			sharedMaterials = renderer.sharedMaterials;
			bounds = renderer.bounds;
			shadowCastingMode = renderer.shadowCastingMode;

			if (renderer.TryGetComponent(out MeshFilter filter))
			{
				sharedMesh = filter.sharedMesh;
			}
		}

		public void SetState(MeshRenderer renderer)
		{
			// Every property isn't set back because it isn't always good (or allowed) to do so (for example, the transform)
			renderer.enabled = enabled;
			renderer.material = material;
			renderer.materials = materials;
			renderer.sharedMaterial = sharedMaterial;
			renderer.sharedMaterials = sharedMaterials;
			renderer.shadowCastingMode = shadowCastingMode;


		} 
		#endregion
	}

}