using System;
using UnityEngine;
public static partial class Globals
{
    public static class Rendering
    {
        public static class Outline
        {
            public static Param<bool> AllowOutlines = new(true);
            public static Param<float> Thickness = new(0.01f);
            public static Param<UnityEngine.Color> Color = new(UnityEngine.Color.white);
            public static Param<OutlineMode> Mode = new(OutlineMode.Offset);
            public static Param<OutlineOptions> Options => new(new OutlineOptions(Color, Thickness, Mode));

            /// <summary>
            /// Creates a new Instance of an outline material setup with the global parameters
            /// </summary>
            /// <returns></returns>
            public static Material GetDefaultMaterial()
            {
                // Shader.Find doesn't work here
                Shader shader = Shader.Find("CookingCarnage/SH_Outline");
                Material result;
                if (shader == null)
                {
                    //Debug.LogError("Shader couldn't be found with Shader.Find()");
                    result = Reference.AssetReferences.OutlineMaterial;
                    //result = new Material(Shader.Find("Legacy Shaders/Diffuse"));
                }
                else
                {
                    result = new Material(shader);
                }
                // Make sure the outline cull mode is correct. It can break even if the shader has the right cull mode set as defaut (idk why)
                result.SetFloat("_CullMode", 1);
                result.SetFloat("_CullModeForward", 1);
                return result;
            }

            public enum OutlineMode { Scale, Offset }

            [Serializable]
            public struct OutlineOptions
            {
                
                public Color Color;
                public float Thickness;
                public OutlineMode Mode;

                public OutlineOptions(Color color, float thickness, OutlineMode mode)
                {
                    Color = color;
                    Thickness = thickness;
                    Mode = mode;
                }
            }
        }
    }
}