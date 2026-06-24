using Cooking;
using System.Collections.Generic;

namespace Features
{
    public class F_Recipe : Feature
    {
        #region Properties
        public SO_Recipe Recipe => RecipeFinder.GetMatchingRecipe(IngredientModels);
        public List<F_Ingredient> Ingredients { get; set; }
        public List<SO_Ingredient> IngredientModels => GetIngredientModels();

        #endregion



        public F_Recipe(IFeatureCollection owner) : base(owner)
        {
        }



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
