namespace Features
{
    public class Cooker : TransformAfterTimer
    {
        public Cooker(IFeatureCollection owner) : base(owner)
        {
            Conditions.AddCondition(item => item is F_Cookable || item is F_OverCookable);
        }
    }
}