using UnityEngine;

using Features;

public class Recipe : MonoBehaviour, IFeatureCollection
{
    public FeatureCollection Collection { get; set; } = new();
    public F_Recipe RecipeFeature { get; set; }
    public Servable Servable;


    private void Awake()
    {
        RecipeFeature = new(this);
        Servable = new(this);
    }
}
