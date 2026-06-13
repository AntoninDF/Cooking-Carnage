using Cooking;
using System.Collections.Generic;

namespace Features
{
    public class Ingredient : Feature, IFeatureCollection
    {
        #region Properties

        public FeatureCollection Collection {get;set;} = new ();
        public SO_Ingredient Model {get;set;}
        public List<Transformable> States {get;set;} = new();
        #endregion



        #region Methods
        public Ingredient(IFeatureCollection owner) : base(owner) { }
        public Ingredient(IFeatureCollection owner, SO_Ingredient model) : base(owner)
        {
            Model = model;
        }


        #endregion
    }
}