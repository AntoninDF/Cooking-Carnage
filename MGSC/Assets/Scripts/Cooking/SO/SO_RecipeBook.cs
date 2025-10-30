using NaughtyAttributes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cooking
{

    [CreateAssetMenu(fileName = "RecipeBook", menuName = "Scriptable Objects/Recipe Book")]
    public class SO_RecipeBook : ScriptableObject
    {
        public SO_Recipe DefaultRecipe { get => defaultRecipe;set=> defaultRecipe = value; }
        [Tooltip("Recipe that will be generated if no other matching recipe is found")]
        [SerializeField] private SO_Recipe defaultRecipe;
        public List<SO_Recipe> Recipes { get => recipes;set=>recipes =value; }
        [Tooltip("List all the recipes that can be made here")]
        [Expandable]
        [SerializeField] private List<SO_Recipe> recipes;

        public float TotalProbabilityRate => recipes.Select((r) => r.probabilityRate).Sum();
    }

}