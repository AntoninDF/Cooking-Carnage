

using Clients;
using Cooking;
using Features;

public class Servable : Feature
{
    #region Properties
    public SO_Recipe Recipe => (Owner as Recipe)?.RecipeFeature?.Recipe;
    public EClientType DrinkClientType => (Owner as Drink)? (Owner as Drink).DrinkType : 0;

    #endregion 


    #region Methods
    public Servable(IFeatureCollection owner) : base(owner)
    {
    } 
    
    #endregion
}