namespace Features
{
    public class F_Cuttable : TransformableAfterTimer
    {
        public F_Cuttable(IFeatureCollection owner) : base(owner)
        {
            Globals.Bind(value => TransformationDuration = value, Globals.Gameplay.CuttingDuration);
        }
    }
}