using NaughtyAttributes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cooking
{

    [CreateAssetMenu(fileName = "RecipeBook", menuName = "Scriptable Objects/Recipe Book")]
    public class SO_RecipeBook : ScriptableObject
    {
        public SO_Recipe EmptyRecipe { get => emptyRecipe;set=> emptyRecipe = value; }
        [Tooltip("Recipe that will be generated if no ingredient is contained")]
        [SerializeField] private SO_Recipe emptyRecipe;
        public SO_Recipe FailedRecipe { get => failedRecipe;set=> failedRecipe = value; }
        [Tooltip("Recipe that will be generated if no other matching recipe is found but it contains ingredients")]
        [SerializeField] private SO_Recipe failedRecipe;
        public List<SO_Recipe> Recipes { get => recipes;set=>recipes =value; }
        [Tooltip("List all the recipes that can be made here")]
        [Expandable]
        [SerializeField] private List<SO_Recipe> recipes;

        public float TotalProbabilityRate => recipes.Select((r) => r.probabilityRate).Sum();
    }

}