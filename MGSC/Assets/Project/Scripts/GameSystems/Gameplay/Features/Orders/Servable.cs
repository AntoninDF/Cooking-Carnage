

using Cooking;
using Features;

public class Servable : Feature
{
    #region Properties
    public SO_Recipe Recipe => (Owner as Recipe)?.RecipeFeature?.Recipe;

    #endregion 


    #region Methods
    public Servable(IFeatureCollection owner) : base(owner)
    {
    } 
    
    #endregion
}