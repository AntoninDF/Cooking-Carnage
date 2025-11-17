using UnityEngine;

namespace Cooking
{

    [CreateAssetMenu(fileName = "Ingredient", menuName = "Scriptable Objects/Ingredient")]
    public class SO_Ingredient : ScriptableObject
    {
        public string Name { get => name;set { name = value; } }
        [SerializeField] new private string name;

        public bool IsBaseIngredient => _isBaseIngredient;
        [Tooltip("Is this a base ingredient ?")]
        private bool _isBaseIngredient;

        public GameObject Prefab => _prefab;
        [Tooltip("The ingredient prefab should have an Ingredient component on it's root object")]
        [SerializeField] private GameObject _prefab;

        public Sprite Sprite => _sprite;
        [Tooltip("The ingredient sprite that should be displayed on UI")]
        [SerializeField] private Sprite _sprite;
    }

}