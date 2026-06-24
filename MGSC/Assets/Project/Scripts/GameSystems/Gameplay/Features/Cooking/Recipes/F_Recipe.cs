using Cooking;
using System.Collections.Generic;

namespace Features
{
    public class F_Recipe : Feature
    {
        #region Properties
        /// <summary>
        /// Shortcut for GetMatchingRecipe
        /// </summary>
        public SO_Recipe Recipe => GetMatchingRecipe();
        public List<F_Ingredient> Ingredients { get; set; }
        public List<SO_Ingredient> IngredientModels => GetIngredientModels();

        #endregion



        public F_Recipe(IFeatureCollection owner) : base(owner)
        {
        }

        /// <summary>
        ///  Finds a recipe model based on the ingredients currently stored in this recipe feature
        /// </summary>
        /// <returns></returns>
        public SO_Recipe GetMatchingRecipe() => RecipeFinder.GetMatchingRecipe(IngredientModels);


        /// <summary>
        /// Retrieves the models from the ingredient features stored in this recipe feature
        /// </summary>
        /// <returns></returns>
        private List<SO_Ingredient> GetIngredientModels()
        {
            List<SO_Ingredient> result = new();
            foreach (var ingredient in Ingredients)
                result.Add(ingredient.Model);
            return result;
        }

    }
}
