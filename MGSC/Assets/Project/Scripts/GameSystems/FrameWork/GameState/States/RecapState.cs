using GameSystem;
using System.Collections.Generic;
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
        SaveGameWithoutProgressFromThisShift();
        AudioSystem.PlayAudio(AudioSystem.Database.upgradeMusic,true);
    }

    public override void OnExit()
    {
        base.OnExit();
        if (!Reference.GetManager<ShiftManager>().HasFailed) 
            Globals.Gameplay.ShiftNumber.Value++;
        else
        {
            // Reverting is done after the shift recap, whereas saving the reverted stuff is done before. (Quite troubling I know)
            // This helps ensure that the game if saved even if we close the game before exitting the recap,
            // While still allowing data to linger so that the recap can display what has been done this shift.
            RevertEverythingDoneThisShift();
        }
    }

    private static void RevertEverythingDoneThisShift()
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
    /// <summary>
    /// This saves the game, but omits everything made in the current shift from the save. Useful when the shift is failed.
    /// Nothing is actually reverted in the game, only in the save.
    /// </summary>
    private void SaveGameWithoutProgressFromThisShift()
    {
        Reference.TryGetManager<ShiftManager>(out var sm);
        Reference.TryGetManager<MoneyManager>(out var mm);
        Reference.TryGetManager<OrderManager>(out var om);
        Reference.TryGetManager<UpgradeManager>(out var um);

        var data = SaveManager.MakeGameData();
        data.money = mm.GetMoneyAtTheBeginningOfThisShift();
        data.Transactions = mm.GetTransactions();
        data.Transactions[sm.ShiftNumber] = new(); // Make sure not to save the current shift's transactions

        // Save the orders up until (excluding) this shift
        // Shallow copy to avoid modifying the original dict (lists are not deep-copied because we don't touch them anyway)
        Dictionary<int, List<Order>> orders = new();
        foreach (var kvp in om.PastOrders)
            orders[kvp.Key] = kvp.Value;
        orders[sm.ShiftNumber] = new();
        data.OrderHistory = SaveManager.ParseOrdersToStringHistory(orders);

        // Get previous upgrade levels and save these instead of the current ones (which should be reverted)
        var previousUpgradeLevels = um.GetUpgradeLevelsOnShift(sm.ShiftNumber - 1);
        List<SaveManager.UpgradeData> upgrades = SaveManager.ConvertUpgradeData();
        foreach (var upgrade in upgrades)
            foreach (var kvp in previousUpgradeLevels)
                if (kvp.Key.Name == upgrade.name)
                    upgrade.level = kvp.Value;
        data.upgrades = upgrades;

        SaveManager.SaveGameData(data);
    }
    public override GameState FindNext(StateTransitions transition = StateTransitions.None)
    {
        switch (transition)
        {
            case StateTransitions.UpgradeShop:
                // End the demo at the end of the third shift
                if (Globals.Gameplay.ShiftNumber >= 2)
                    return new DemoEndState();
                else
                    return new UpgradeState();
            case StateTransitions.OpenMainMenu:
                return new MainMenuState();


            default:
            case StateTransitions.None:
                return base.FindNext(transition);
        }
    }
}
