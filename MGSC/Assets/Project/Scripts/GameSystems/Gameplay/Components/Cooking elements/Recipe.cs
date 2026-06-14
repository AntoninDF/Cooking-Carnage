using UnityEngine;

using Features;

public class Recipe : MonoBehaviour, IFeatureCollection
{
    public FeatureCollection Collection { get; set; } = new();
    public Features.Recipe RecipeFeature { get; set; }


    private void Awake()
    {
        RecipeFeature = new(this);
    }
}
