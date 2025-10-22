using UnityEngine;

namespace Cooking
{

    [CreateAssetMenu(fileName = "Ingredient", menuName = "Scriptable Objects/Ingredient")]
    public class SO_Ingredient : ScriptableObject
    {
        public string Name { get => name;set { name = value; } }
        [SerializeField] new private string name;

        public GameObject Prefab => _prefab;
        [Tooltip("The ingredient prefab should have an Ingredient component on it's root object")]
        [SerializeField] private GameObject _prefab;
    }

}