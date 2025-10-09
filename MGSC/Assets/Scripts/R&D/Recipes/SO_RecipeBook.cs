using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_RecipeBook", menuName = "Scriptable Objects/SO_RecipeBook")]
public class SO_RecipeBook : ScriptableObject
{
    public List<SO_Recipe> Recipes { get => recipes;set=>recipes =value; }
    [Tooltip("List all the recipes that can be made here")]
    [SerializeField] private List<SO_Recipe> recipes;
}
