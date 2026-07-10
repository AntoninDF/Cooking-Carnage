using System.Collections.Generic;
using Cooking;

namespace Orders
{
	/// <summary>
	/// This class isn't used, it could be deleted
	/// </summary>
	public static class RecipePriceTable
	{
		private const float Price_Single      = 10f; 
		private const float Price_DoubleSame  = 20f; 
		private const float Price_DoubleDiff  = 25f; 
		private const float Price_TripleMixed = 40f; 
		private const float Price_TripleDiff  = 50f; 

		public static float GetBasePrice(SO_Recipe recipe)
		{
			if (recipe == null)
				return 0f;

			var counts = new Dictionary<object, int>();
			foreach (var ingredient in recipe.Ingredients)
			{
				if (!counts.ContainsKey(ingredient)) counts[ingredient] = 0;
				counts[ingredient]++;
			}

			int total    = recipe.Ingredients.Count;
			int distinct = counts.Count;

			return (total, distinct) switch
			       {
				       (1, 1) => Price_Single,
				       (2, 1) => Price_DoubleSame,
				       (2, 2) => Price_DoubleDiff,
				       (3, 2) => Price_TripleMixed,
				       (3, 3) => Price_TripleDiff,
				       _      => 0f 
			       };
		}
	}
}