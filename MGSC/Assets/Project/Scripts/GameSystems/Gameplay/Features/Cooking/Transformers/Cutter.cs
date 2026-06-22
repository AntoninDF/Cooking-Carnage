namespace Features
{
    public class Cutter : TransformAfterTimer
    {
        public Cutter(IFeatureCollection owner) : base(owner)
        {
            Conditions.AddCondition(item => item is F_Cuttable);
        }
    }
}