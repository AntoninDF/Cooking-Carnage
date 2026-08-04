using Cooking;
using GameSystem;
using UnityEngine;

namespace Orders
{
	public static class RecipeUnlockChecker
	{
		public static bool IsUnlocked(SO_Recipe recipe)
		{
			return IsUnlocked(RecipeCategoryHelper.GetCategory(recipe));
		}

		public static bool IsUnlocked(RecipeCategory category)
		{
			var config = SO_RecipeUnlockConfig.Instance;
			if (config == null) return true; 

			if (!config.TryGetRequirement(category, out var requirement))
			{
				Debug.LogWarning($"[RecipeUnlockChecker] Aucune contrainte définie pour la catégorie {category}, débloquée par défaut.");
				return true;
			}

			ShiftManager shiftManager = Reference.GetManager<ShiftManager>();
			MoneyManager moneyManager = Reference.GetManager<MoneyManager>();

			int   currentShift = shiftManager != null ? shiftManager.ShiftNumber : 0;
			// currentMoney is actually the sum of all money gained
			float currentMoney = moneyManager != null ? moneyManager.GetMoneyGainedSinceBeginning() : 0f;

			return currentShift >= requirement.minShift && currentMoney >= requirement.minMoney;
		}
	}
}