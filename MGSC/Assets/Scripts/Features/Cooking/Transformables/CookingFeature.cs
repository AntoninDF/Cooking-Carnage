

namespace Features
{
    public abstract class CookingFeature : Feature
    {
        #region Properties
        public abstract bool Completed { get;}
        public abstract bool CanBePerformed { get;}


        #endregion



        #region Fields


        #endregion


        #region Methods
        public CookingFeature(IFeatureCollection owner) : base(owner)
        {
        }




        #endregion
    }
}