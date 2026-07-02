using Clients;
using Cooking;
using Orders;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "New AssetReferences", menuName = "Scriptable Objects/AssetReferences")]
public class SO_AssetReferences : ScriptableObject
{
    public GameObject Player { get => _player; private set => _player = value; }
    [Tooltip("Player prefab to spawn on scene start")]
    [SerializeField] private GameObject _player;
    
    public GameObject GameLoop { get => _gameloop; private set => _gameloop = value; }
    [Tooltip("Gameloop prefab to spawn on scene start")]
    [SerializeField] private GameObject _gameloop;

    public InputActionAsset InputActions { get => _inputActions; private set => _inputActions = value; }
    [Tooltip("The input action asset for the player")]
    [SerializeField] private InputActionAsset _inputActions;

    public List<UIWidget> UIPrefabs;
    public Material OutlineMaterial;
    public SO_RecipeBook DefaultRecipeBook;

    public SO_RecipeBook CurrentRecipeBook => OrderPicker.FindCurrentRecipeBook();

    public List<RecipeBookByShiftCount> recipeBookByShiftCount;


    public List<SO_Client> clientList;

}

[System.Serializable]
public struct RecipeBookByShiftCount
{
    public int shift;
    public SO_RecipeBook recipeBook;
}
