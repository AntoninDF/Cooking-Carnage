using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Defines the set of data needed for a meal
/// </summary>
[CreateAssetMenu(fileName = "SO_Recipe", menuName = "Scriptable Objects/SO_Recipe")]
public class SO_Recipe : ScriptableObject
{
    public string Name { get => name; set { name = value; } }
    [Tooltip("The name of the recipe that will be displayed in game")]
    [SerializeField] new private string name;

    [Tooltip("The item prefab should contain the graphics, an item component and a recipe component")]
    public GameObject itemPrefab;
    [Tooltip("The list of ingredients that are needed to complete the recipe")]
    public List<SO_Item> itemsNeeded;
    [Tooltip("Do the ingredients need to be ordered to complete the recipe ?")]
    [HideInInspector] public bool ordered = false; // Disabled for now
    [Tooltip("Does the recipe need to have all and only these ingredients, or can other ingredients be in it too ?")]
    public bool exact = false;
}
