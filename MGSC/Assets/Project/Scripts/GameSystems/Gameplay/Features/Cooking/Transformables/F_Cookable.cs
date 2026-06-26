namespace Features
{
    public class F_Cookable : TransformableAfterTimer
    {
        public F_Cookable(IFeatureCollection owner) : base(owner)
        {
            Globals.Bind(value => TransformationDuration = value, Globals.Gameplay.CookingDuration);
        }
    }
}