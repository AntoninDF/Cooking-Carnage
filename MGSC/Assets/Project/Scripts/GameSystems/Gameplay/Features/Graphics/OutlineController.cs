using System;
using System.Collections.Generic;
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
        public MeshRenderer Target { get; set; }
        public GameObject OutlineObject { get; set; }
        public MeshRenderer OutlineRenderer { get => outlineRenderer; set => outlineRenderer = value; }
        protected MeshRenderer outlineRenderer;
        public MeshFilter OutlineMesh { get; set; }
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
        public bool Visible { get => visible; set => SetVisibility(value); }
        protected bool visible = Globals.Rendering.Outline.AllowOutlines;
        #endregion


        #region Methods
        public OutlineController(IFeatureCollection owner, MeshRenderer target) : base(owner)
        {
            Target = target;
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
            Debug.Log("Enabling outline");
            enabled = true;
            if (OutlineObject == null)
            {
                OutlineObject = MakeOutlineVariant(Target, out outlineRenderer);
            }
            OutlineObject.SetActive(true);
        }
        public virtual void DisableOutline()
        {

            Debug.Log("Disabling outline");
            enabled = false;
            if (OutlineObject == null) return;
            OutlineObject.SetActive(false);
        }

        public void SetVisible() => SetVisibility(true);
        public void SetInvisible() => SetVisibility(false);
        public void SetVisibility(bool visible)
        {
            this.visible = visible;
            if (OutlineRenderer != null)
                OutlineRenderer.enabled = visible;
        }

        public GameObject MakeOutlineVariant(MeshRenderer target, out MeshRenderer outlineRenderer)
        {
            // Make an object dedicated to rendering the outline
            GameObject result = new("Outline variant (" + target.name + ")", typeof(MeshFilter), typeof(MeshRenderer));
            outlineRenderer = result.GetComponent<MeshRenderer>();

            // Place the object in the world and hierarchy
            result.transform.SetParent(target.transform);
            result.transform.localPosition = Vector3.zero;
            result.transform.localScale = Vector3.one;
            result.transform.localRotation = Quaternion.identity;


            // Apply the right mesh on said object
            var filter = result.GetComponent<MeshFilter>();
            filter.sharedMesh = target.GetComponent<MeshFilter>().sharedMesh;

            // Shadows shouldn't be cast by the outline geometry
            outlineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            int matCount = target.sharedMaterials.Length;
            Material outlineMat = Globals.Rendering.Outline.DefaultMaterial;

            // 0 = Outline by scaling ; 1 = Outline by normal offset
            outlineMat.SetInt("_OUTLINEMODE", 1);

            Material[] materials = new Material[matCount];
            for (int i = 0; i < matCount; i++)
                materials[i] = new(outlineMat);

            outlineRenderer.sharedMaterials = materials;

            // Apply the parameters
            SetColor(Color);
            SetThickness(Thickness);
            SetVisibility(Visible);

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

        #endregion
    }

}