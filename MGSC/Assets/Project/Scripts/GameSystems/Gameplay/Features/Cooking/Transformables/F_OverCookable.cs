namespace Features
{
    public class F_OverCookable : TransformableAfterTimer
    {
        public F_OverCookable(IFeatureCollection owner) : base(owner)
        {
            Globals.Bind(value => TransformationDuration = value, Globals.Gameplay.OvercookingDuration);
        }
    }
}