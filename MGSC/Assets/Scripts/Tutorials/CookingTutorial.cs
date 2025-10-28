
using Clients;
using Cooking;
using GameSystem;
using Orders;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Events;

namespace Tutorials

{
    public class CookingTutorial : Tutorial
    {
        #region Fields
        [SerializeField] private SittingCounter _counter;
        [SerializeField] private Oven _oven;


        #endregion

        #region Methods

        private void Start()
        {
            Setup();
        }

        public override void Setup()
        {
            base.Setup();
            if (_counter == null)
                Debug.LogError("[OrderTutorial] : Please reference the default counter in the inspector");
            if (_oven == null)
                Debug.LogError("[OrderTutorial] : Please reference the default oven in the inspector");

            SO_Ingredient ingredient = ReferenceManager.AssetReferences.recipeBook.Recipes[0].itemsNeeded[0];
            _counter.Spawn(ingredient);
            _counter.Spawn(ingredient);
        }

        #endregion
    }
}