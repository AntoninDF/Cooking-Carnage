using GameSystem;
using System.Linq;
using UI;
using Upgrades;

public class RecapState : MenuState
{
    // To Do : Change the menu according to wether the player has succeeded or failed
    protected override System.Type DefaultMenuType => typeof(ShiftRecapMenu);

    public override void OnEnter()
    {
        base.OnEnter();
        
        AudioSystem.PlayAudio(AudioSystem.Database.upgradeMusic,true);
    }

    public override void OnExit()
    {
        base.OnExit();
        if (!Reference.GetManager<ShiftManager>().HasFailed) 
            Globals.Gameplay.ShiftNumber.Value++;
        else
        {
            Reference.TryGetManager<MoneyManager>(out var mm);
            Reference.TryGetManager<ShiftManager>(out var sm);
            Reference.TryGetManager<OrderManager>(out var om);
            Reference.TryGetManager<UpgradeManager>(out var um);

            // Reset money transactions
            mm.AddMoney(-mm.GetMoneyTransactionsThisShift().Sum());
            mm.ResetTransactionsForShift(sm.ShiftNumber);

            // Reset the history of orders tracked for this shift
            om.ResetOrdersForShift(sm.ShiftNumber);

            // Revert upgrade levels to the ones before this shift
            var previousUpgradeLevels = um.GetUpgradeLevelsOnShift(sm.ShiftNumber - 1);
            um.ApplyLevels(previousUpgradeLevels);
            um.ActualizeAllUpgrade();
        }
    }

    public override GameState FindNext(StateTransitions transition = StateTransitions.None)
    {
        switch (transition)
        {
            case StateTransitions.UpgradeShop:
                return new UpgradeState();
            case StateTransitions.OpenMainMenu:
                return new MainMenuState();


            default:
            case StateTransitions.None:
                return base.FindNext(transition);
        }
    }
}
