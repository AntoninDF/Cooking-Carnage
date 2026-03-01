namespace Features
{
    public class Ingredient : Feature, IFeatureCollection
    {
        #region Properties

        public FeatureCollection Collection {get;set;} = new ();
        public Cookable Cookable {get;set;}


        #endregion



        #region Methods
        public Ingredient(IFeatureCollection owner) : base(owner)
        {
            Cookable = new(this);
        }


        #endregion
    }
}