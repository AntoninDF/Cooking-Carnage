using UnityEngine;
public static partial class Globals
{
    public static class Rendering
    {
        
        public static class Outline
        {
            public static Param<UnityEngine.Material> Material = new Material(Shader.Find("CookingCarnage/SH_Outline"));
            public static Param<float> Thickness = 0.1f;
            public static Param<UnityEngine.Color> Color = UnityEngine.Color.white;
        }
    }
}