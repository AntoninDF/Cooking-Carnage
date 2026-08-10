namespace Upgrades
{
    public class ShiftDurationUpgrade : Upgrade<ShiftDurationUpgrade>, IUpgradable<ShiftDurationUpgrade>
    {
        public float SecondsGainedPerLevel => 10f;
        public float TotalShiftDuration => SecondsGainedPerLevel * Level + GameSettings.Instance.ShiftSettings.ShiftDuration;
        
        public ShiftDurationUpgrade()
        {
            (this as IUpgradable<ShiftDurationUpgrade>).TrySubscribe();
        }
        public void OnUpgraded(ShiftDurationUpgrade upgrade)
        {
            Globals.Gameplay.ShiftDuration.Value = TotalShiftDuration;
        }

        public void OnUpgradeInit(ShiftDurationUpgrade upgrade)
        {
            Globals.Gameplay.ShiftDuration.Value = TotalShiftDuration;
        }
    }

}