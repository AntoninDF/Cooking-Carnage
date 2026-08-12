namespace Features
{
    public class F_OverCookable : TransformableAfterTimer
    {
        /// <summary>
        /// Be careful, getting this value willl return the value multiplied by <see cref="Globals.BalancingWeights.PreparationFailTimeMultiplier"/>.
        /// So setting it to X then getting it might not give back X.
        /// </summary>
        public override float TransformationDuration
        { 
            get => Globals.Gameplay.OvercookingDuration * Globals.BalancingWeights.PreparationFailTimeMultiplier;
            set => Globals.Gameplay.OvercookingDuration.Value = value;
        }
        public F_OverCookable(IFeatureCollection owner) : base(owner)
        {
            Globals.Bind(value => TransformationDuration = value, Globals.Gameplay.OvercookingDuration);
        }
    }
}