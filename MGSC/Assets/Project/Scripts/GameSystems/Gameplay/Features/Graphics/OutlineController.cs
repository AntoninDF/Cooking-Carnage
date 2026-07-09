using System;
using UnityEngine;

namespace Features
{
    /// <summary>
    /// This feature takes a renderer and makes a copy of it that only renders it's outline with a hull outline shader.
    /// </summary>
    // The outline controller makes a copy of the target object instead of just adding materials to it because that way it doesn't break on material changes and with multiple submeshes.
    public class OutlineController : Feature
    {
        #region Properties
        public Mesh Mesh { get; set; }
        public Transform TargetParent { get; set; }
        public GameObject OutlineObject { get; set; }
        public MeshRenderer OutlineRenderer { get => outlineRenderer; set => outlineRenderer = value; }
        protected MeshRenderer outlineRenderer;
        public MeshFilter OutlineMesh { get; set; }
        public bool CanShowOutline { get; set; } = true;
        public Color Color
        {
            get => color;
            set => SetColor(value);
        }
        protected Color color = Globals.Rendering.Outline.Color;
        public float Thickness
        {
            get => thickness;
            set => SetThickness(value);
        }
        protected float thickness = Globals.Rendering.Outline.Thickness;
        public Globals.Rendering.Outline.OutlineMode Mode
        {
            get => mode;
            set => SetMode(value);
        }
        protected Globals.Rendering.Outline.OutlineMode mode = Globals.Rendering.Outline.Mode;
        public bool Visible { get => visible; set => SetVisibility(value); }
        protected bool visible = Globals.Rendering.Outline.AllowOutlines;
        
        public MeshRenderer[] Renderers { get; set; }
        protected int[] originalLayers;
        #endregion


        #region Methods
        public OutlineController(IFeatureCollection owner, Mesh target, Transform targetParent, MeshRenderer[] renderers) : base(owner)
        {
            Mesh = target;
            TargetParent = targetParent;
            Renderers = renderers;
            originalLayers = new int[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
                originalLayers[i] = renderers[i].gameObject.layer;

            OnEnable += EnableOutline;
            OnDisable += DisableOutline;

            Globals.Rendering.Outline.Color.OnValueChange += SetColor;
            Globals.Rendering.Outline.Thickness.OnValueChange += SetThickness;
            Globals.Rendering.Outline.AllowOutlines.OnValueChange += SetVisibility;
        }

        public virtual void ToggleOutline() => SetOutline(!enabled);
        public virtual void SetOutline(bool active)
        {
            if (active) EnableOutline();
            else DisableOutline();
        }
        public virtual void EnableOutline()
        {
            if(!CanShowOutline) return;
            enabled = true;
            foreach (var renderer in Renderers)
                renderer.gameObject.layer = LayerMask.NameToLayer("Outlined");
            enabled = true;
            
            /*
            if (OutlineObject == null)
            {
                OutlineObject = MakeOutlineVariant(Mesh, out outlineRenderer);
                SetParent(TargetParent);
                // Apply the parameters
                SetColor(Color);
                SetThickness(Thickness);
                SetMode(Mode);
                SetVisibility(Visible);
            }
            OutlineObject.SetActive(true);
            */
        }
        public virtual void DisableOutline()
        {
            enabled = false;
            for (int i = 0; i < Renderers.Length; i++)
            {
                if(IsHeld()) continue;
                Renderers[i].gameObject.layer = originalLayers[i];
            }
                
            
            //if (OutlineObject == null) return;
            //OutlineObject.SetActive(false);
        }

        private bool IsHeld()
        {
            return TargetParent != null && TargetParent.GetComponentInParent<Item>() is { IsHeld: true };
        }

        public void SetVisible() => SetVisibility(true);
        public void SetInvisible() => SetVisibility(false);
        public void SetVisibility(bool visible)
        {
            this.visible = visible;
            if (OutlineRenderer != null)
                OutlineRenderer.enabled = visible;
        }
        public void SetParent(Transform target)
        {
            if (OutlineObject == null || target == null) return;
            // Place the object in the world and hierarchy
            OutlineObject.transform.SetParent(target.transform);
            OutlineObject.transform.localPosition = Vector3.zero;
            OutlineObject.transform.localScale = Vector3.one;
            OutlineObject.transform.localRotation = Quaternion.identity;
            // Rename the object according to it's parent
            OutlineObject.name = "Outline variant (" + target.name + ")";
        }
        public GameObject MakeOutlineVariant(Mesh target, out MeshRenderer outlineRenderer)
        {
            // Make an object dedicated to rendering the outline
            GameObject result = new("Outline variant (" + target.name + ")", typeof(MeshFilter), typeof(MeshRenderer));
            outlineRenderer = result.GetComponent<MeshRenderer>();

            // Apply the right mesh on the outline object
            var filter = result.GetComponent<MeshFilter>();
            filter.sharedMesh = target;

            // Shadows shouldn't be cast by the outline geometry
            outlineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            int matCount = target.subMeshCount;
            Material outlineMat = Globals.Rendering.Outline.GetDefaultMaterial();

            Material[] materials = new Material[matCount];
            for (int i = 0; i < matCount; i++)
                materials[i] = new(outlineMat);

            outlineRenderer.sharedMaterials = materials;

            return result;
        }

        public void ApplyOnAllMaterials(Action<Material> action)
        {
            if (OutlineObject == null || OutlineRenderer == null)
                return;

            // Get the current materials
            int matCount = OutlineRenderer.sharedMaterials.Length;
            Material[] newMaterials = OutlineRenderer.materials;

            // Modify them
            for (int i = 0; i < matCount; i++)
            {
                if (newMaterials[i] == null) continue;
                action?.Invoke(newMaterials[i]);
            }

            // Reapply
            OutlineRenderer.materials = newMaterials;
        }
        
        public void SetLayer(int layer)
        {
            if (OutlineObject != null)
                OutlineObject.layer = layer;
        }

        public void SetParameters(Globals.Rendering.Outline.OutlineOptions parameters)
        {
            SetColor(parameters.Color);
            SetThickness(parameters.Thickness);
            SetMode(parameters.Mode);
        }
        public void SetColor(Color color)
        {
            this.color = color;

            if (OutlineObject == null || OutlineRenderer == null)
                return;

            ApplyOnAllMaterials( mat => mat.SetColor("_Color", color) );
        }
        public void SetThickness(float thickness)
        {
            this.thickness = thickness;

            if (OutlineObject == null || OutlineRenderer == null)
                return;

            ApplyOnAllMaterials(mat => mat.SetFloat("_Thickness", thickness));
        }
        public void SetMode(Globals.Rendering.Outline.OutlineMode mode)
        {
            this.mode = mode;

            if (OutlineObject == null || OutlineRenderer == null)
                return;

            ApplyOnAllMaterials(mat => SetKeyWord(mat, "_OUTLINEMODE", mode));
        }
        /// <summary>
        /// From StackOverflow https://discussions.unity.com/t/how-to-manipulate-a-shader-graph-enum-using-c/926912/5
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="material"></param>
        /// <param name="parameterBaseName"></param>
        /// <param name="selectedKeyword"></param>
        private void SetKeyWord<T>(Material material, string parameterBaseName, T selectedKeyword) where T : Enum
        {
            var allKeywords = Enum.GetValues(typeof(T));
            foreach (T keyword in allKeywords)
            {
                var keywordText = $"{parameterBaseName}_{keyword.ToString().ToUpper()}";

                if (keyword.Equals(selectedKeyword))
                    material.EnableKeyword(keywordText);
                else
                    material.DisableKeyword(keywordText);
            }
        }
        #endregion
    }
    
    

}