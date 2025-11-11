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

    public InputActionAsset InputActions { get => _inputActions; private set => _inputActions = value; }
    [Tooltip("The input action asset for the player")]
    [SerializeField] private InputActionAsset _inputActions;

    public SO_RecipeBook recipeBook;


    public List<SO_Client> clientList;

}
