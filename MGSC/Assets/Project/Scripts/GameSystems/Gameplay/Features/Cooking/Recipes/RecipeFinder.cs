using Cooking;
using System.Collections.Generic;
using System.Linq;

public static class RecipeFinder
{
    public static SO_RecipeBook AllRecipes
    {
        get
        {
            var result = Reference.AssetReferences.DefaultRecipeBook;
            if (result == null)
                UnityEngine.Debug.Log("RecipeFinder : No recipe book found (AllRecipes)");
            return result;
        }
    }

    public static SO_Recipe GetMatchingRecipe(IEnumerable<SO_Ingredient> ingredients)
    {
        if (ingredients.Count() == 0) return AllRecipes.EmptyRecipe; // If empty recipe
        foreach (var r in AllRecipes.Recipes)
        {
            if (SO_Recipe.Compare(ingredients, r))
            {
                return r;
            }
        }

        return AllRecipes.FailedRecipe;
    }
}
