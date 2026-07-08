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
        foreach (var r in AllRecipes.Recipes)
        {
            if (SO_Recipe.Compare(ingredients, r))
            {
                return r;
            }
        }

        return AllRecipes.FailedRecipe;
    }
    
    public static SO_Recipe FindByIngredients(this SO_RecipeBook book, IEnumerable<SO_Ingredient> ingredients)
    {
        List<SO_Ingredient> target = ingredients.OrderBy(i => i.GetInstanceID()).ToList();

        foreach (var recipe in book.Recipes)
        {
            if (recipe.Ingredients.Count != target.Count) continue;

            List<SO_Ingredient> candidate = recipe.Ingredients.OrderBy(i => i.GetInstanceID()).ToList();
            if (candidate.SequenceEqual(target)) return recipe;
        }
        return null;
    }
}
