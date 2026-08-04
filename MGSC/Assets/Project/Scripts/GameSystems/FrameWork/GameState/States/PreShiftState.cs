using GameSystem;
using Orders;
using System.Linq;
using UI;
public class PreShiftState : GameplayState
{
    HeadsUpDisplay HUD;
    public override void OnEnter()
    {
        //GameLoop.ResetAll();
        base.OnEnter();
        if (Reference.TryGetManager(out UISystem ui))
        {
            HUD = ui.GetWidget<HeadsUpDisplay>();
            HUD?.Enable();
        }

        if (Globals.Statistics.TutorialHasBeenPlayed)
            IngredientSpawnPosition.SpawnIngredients(6);
        else
        {
            // Setup the tutorial by adding only one ingredient to the table, and forcing the first client to be Fabrice, ordering some buttocks
            IngredientSpawnPosition.SpawnIngredients(1, ingredient => ingredient.Name.Contains("fiak"));
            Reference.GetManager<AdaptiveClientSpawner>()?.AddClientToQueue(Clients.EClientType.C);
            var book = Reference.AssetReferences.DefaultRecipeBook;
            if (book != null)
            {
                var recipe = book.Recipes.Where(recipe => recipe.Ingredients.Count == 1 && recipe.Ingredients[0].Name.Contains("fiak")).First();
                if (recipe != null)
                    OrderPicker.AddRecipeToQueue(recipe);
            }
        }
        
        AudioSystem.PlayAudio(AudioSystem.Database.beforeShiftMusic,true);
        AudioSystem.PlayAudio(AudioSystem.Database.fliesAmbience);
        AudioSystem.PlayAudio(AudioSystem.Database.kitchenAmbience);
    }

    public override void OnExit()
    {
        base.OnExit();
        if (HUD != null)
            HUD.Disable();
    }

    public override GameState FindNext(StateTransitions transition = StateTransitions.None)
    {
        switch (transition)
        {
            case StateTransitions.StartShiftCountdown:
                return new ShiftState();
            case StateTransitions.OpenMainMenu:
                return new MainMenuState();


            default:
            case StateTransitions.None:
                return base.FindNext(transition);
        }
    }
}
