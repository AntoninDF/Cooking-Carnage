using System;
using System.Linq;
using Cooking;

namespace Orders
{
	public static class RecipeCategoryHelper
	{
		public static RecipeCategory GetCategory(SO_Recipe recipe)
		{
			int total    = recipe.Ingredients.Count;
			int distinct = recipe.Ingredients.Distinct().Count();

			if (total == 0) return RecipeCategory.Vide;
			if (total == 1) return RecipeCategory.Simple;
			if (total == 2 && distinct == 1) return RecipeCategory.DoubleSupplement;
			if (total == 2 && distinct == 2) return RecipeCategory.MixteSimple;
			if (total == 3 && distinct == 2) return RecipeCategory.MixteSupplement;
			if (total == 3 && distinct == 3) return RecipeCategory.MixteTriple;

			throw new ArgumentException($"[RecipeCategoryHelper] Composition non gérée pour {recipe.name}");
		}
	}
}