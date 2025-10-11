using System.Collections.Generic;
using UnityEngine;

namespace Cooking
{

    [CreateAssetMenu(fileName = "RecipeBook", menuName = "Scriptable Objects/Recipe Book")]
    public class SO_RecipeBook : ScriptableObject
    {
        public List<SO_Recipe> Recipes { get => recipes;set=>recipes =value; }
        [Tooltip("List all the recipes that can be made here")]
        [SerializeField] private List<SO_Recipe> recipes;
    }

}