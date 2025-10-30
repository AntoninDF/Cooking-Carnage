using Cooking;
using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Orders
{
    /// <summary>
    /// The mighty physical form of an order
    /// </summary>
    public class OrderTicket : Item
    {
        #region Properties

        #endregion

        #region Fields
        [Header("Ticket references")]
        [Tooltip("The text object that will display the seat number")]
        [SerializeField] private TMP_Text _seat;
        [Tooltip("The text object that will display the recipe name")]
        [SerializeField] private TMP_Text _title;
        [Tooltip("The text object that will display the ingredient list")]
        [SerializeField] private TMP_Text _ingredients;


        private Order _targetOrder;
        #endregion

        #region Methods

        public void SetTargetOrder(Order targetOrder)
        {
            _targetOrder = targetOrder;
            _targetOrder.OnResolve += Resolve;
            _targetOrder.OnCancel += Cancel;
            UpdateText();
        }

        public void UpdateText()
        {
            EnforceReferences();
            _title.text = _targetOrder.Recipe.Name;
            _ingredients.text = IngredientsToString();
        }

        public void Resolve()
        {
            Destroy(gameObject);
        }

        public void Cancel()
        {
            if (Container != null)
            {
                Container.Drop(this);
            }
            Destroy(gameObject);
        }

        private void EnforceReferences()
        {
            if (_seat == null)
                Debug.LogError("[OrderTicket] : The seat number's text object reference has not been assigned in the inspector.");
            if (_title == null)
                Debug.LogError("[OrderTicket] : The recipe name's text object reference has not been assigned in the inspector.");
            if (_ingredients == null)
                Debug.LogError("[OrderTicket] : The ingredient list's text object reference has not been assigned in the inspector.");
            if (_targetOrder == null)
                Debug.LogError("[OrderTicket] : The target order has not been set. Please use SetTargetOrder to do so.");
        }

        private string IngredientsToString()
        {
            List<SO_Ingredient> ingredients = _targetOrder.Recipe.itemsNeeded;

            string output = string.Empty;
            foreach (var ingredient in ingredients)
            {
                output += ingredient.Name;
                output += "\n";
            }
            return output;
        }
        #endregion
    }
}