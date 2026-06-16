using Cooking;
using System.Collections.Generic;

namespace Features
{
    public class Recipe : Feature
    {
        #region Properties

        public List<F_Ingredient> Ingredients;
        public List<SO_Ingredient> IngredientModels => GetIngredientModels();

        #endregion



        public Recipe(IFeatureCollection owner) : base(owner)
        {
        }

        public SO_Recipe GetMatchingRecipe() => RecipeFinder.GetMatchingRecipe(IngredientModels);



        private List<SO_Ingredient> GetIngredientModels()
        {
            List<SO_Ingredient> result = new();
            foreach (var ingredient in Ingredients)
                result.Add(ingredient.Model);
            return result;
        }

        public void SetIngredients(List<F_Ingredient> ingredients) => Ingredients = ingredients;
        public void AddIngredient(F_Ingredient ingredient) => Ingredients.Add(ingredient);
        public void RemoveIngredient(F_Ingredient ingredient) => Ingredients.Remove(ingredient);
    }
}
