using Clients;
using Cooking;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "New AssetReferences", menuName = "Scriptable Objects/AssetReferences")]
public class SO_AssetReferences : ScriptableObject
{
    public InputActionAsset inputActionsAsset;
    public SO_RecipeBook recipeBook;


    public List<SO_Client> clientList;

}
