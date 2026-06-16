using System;
using UnityEngine;
public static partial class Globals
{
    public static class Rendering
    {
        public static class Outline
        {
            public static Param<bool> AllowOutlines = true;
            public static Param<UnityEngine.Material> DefaultMaterial = new Material(Shader.Find("CookingCarnage/SH_Outline"));
            public static Param<float> Thickness = 0.1f;
            public static Param<UnityEngine.Color> Color = UnityEngine.Color.white;

            static Outline()
            {
                // Make sure the outline cull mode is correct. It can break even if the shader has the right cull mode set as defaut (idk why)
                DefaultMaterial.Value.SetFloat("_CullMode", 1);
                DefaultMaterial.Value.SetFloat("_CullModeForward", 1);
            }
        }
    }
}