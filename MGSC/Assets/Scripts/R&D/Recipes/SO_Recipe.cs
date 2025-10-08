using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Defines the set of data needed for a meal
/// </summary>
[CreateAssetMenu(fileName = "SO_Recipe", menuName = "Scriptable Objects/SO_Recipe")]
public class SO_Recipe : ScriptableObject
{
    [Tooltip("The item prefab should contain the graphics, an item component and a recipe component")]
    public GameObject itemPrefab;
    [Tooltip("The list of ingredients that are needed to complete the recipe")]
    public List<SO_Item> itemsNeeded;
    [Tooltip("Do the ingredients need to be ordered to complete the recipe ?")]
    public bool ordered = false;
}
