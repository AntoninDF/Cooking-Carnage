using Clients;
using Cooking;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "New AssetReferences", menuName = "Scriptable Objects/AssetReferences")]
public class SO_AssetReferences : ScriptableObject
{
    public GameObject Player { get => _player; private set => _player = value; }
    [Tooltip("Player prefab to spawn on scene start")]
    [SerializeField] private GameObject _player;
    public GameObject UI { get => _UI; private set => _UI = value; }
    [Tooltip("UI prefab to spawn on scene start")]
    [SerializeField] private GameObject _UI;
    
    public GameObject GameLoop { get => _gameloop; private set => _gameloop = value; }
    [Tooltip("Gameloop prefab to spawn on scene start")]
    [SerializeField] private GameObject _gameloop;

    public InputActionAsset InputActions { get => _inputActions; private set => _inputActions = value; }
    [Tooltip("The input action asset for the player")]
    [SerializeField] private InputActionAsset _inputActions;

    public SO_RecipeBook defaultRecipeBook;

    public SO_RecipeBook CurrentRecipeBook { get; set; }

    public List<RecipeBookByShiftCount> recipeBookByShiftCount;


    public List<SO_Client> clientList;

}

[System.Serializable]
public struct RecipeBookByShiftCount
{
    public int shift;
    public SO_RecipeBook recipeBook;
}
