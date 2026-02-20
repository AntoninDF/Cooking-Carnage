using GameSystem;
using UI;
using UnityEngine;
using UnityEngine.Events;

namespace Cooking
{
    /// <summary>
    /// Implements the base behaviour for every ingredient
    /// </summary>
    public class Ingredient : Item
    {
        #region Properties
        public SO_Ingredient Model => _model;
        public bool IsBaseIngredient => Model.IsBaseIngredient;
        public bool IsCut {  get; set; }
        public UnityEvent OnCut { get => _onCut; set => _onCut = value; }
        #endregion

        #region Fields
        
        [SerializeField] string _itemID;

        [Header("Ingredient references")]
        [Tooltip("Data model for this ingredient")]
        [SerializeField] protected SO_Ingredient _model;

        [SerializeField]
        private UnityEvent _onCut;

        private ItemManager _itemManager;

        #endregion

        #region Methods

        protected override void Start()
        {
            base.Start();
            if (_model == null)
                throw new MissingReferenceException("Please assign an ingredient model in the inspector : " + name);
            UpdateID();
            Reference.GetManager(out _itemManager);
        }

        protected  string UpdateID()
        {
            //base.SetID(_itemID);
            return _itemID;
        }

        public void Cut()
        {
            _itemManager.StoreCopy(this);
           
            IsCut = true;
            _onCut?.Invoke();
            
            if (!_itemID.Contains(" Cut"))
            {
                _itemID += " Cut";
                //base.SetID(_itemID);
            }
        }

        #endregion

    }

}