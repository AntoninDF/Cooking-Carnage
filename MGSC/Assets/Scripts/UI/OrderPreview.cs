using Clients;
using Cooking;
using GameSystem;
using Orders;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{

    public class OrderPreview : MonoBehaviour
    {
        #region Properties

        public Order CurrentOrder => _currentOrder;
        
        #endregion

        #region Fields
        [Header("References")]
        [Tooltip("Ordered list of images which will display the ingredients needed to complete the order")]
        [SerializeField] private List<Image> _ingredientSprites = new();
        [Tooltip("The counter that this order preview belongs to. It is only required if 'Filter by client' is checked")]
        [SerializeField] private SittingCounter _counter;

        [Header("Parameters")]
        [Tooltip("How long it will take for the order to disappear after it has been displayed. 0 means never")]
        [SerializeField] private float _resetDelay;
        [SerializeField] private float _updateDelay;
        [Tooltip("Should recipes only be displayed if they are requested by a client that reserved this counter")]
        [SerializeField] private bool _filterByClient;

        [Header("Events")]
        [SerializeField] private UnityEvent _onOrderReceived;
        [SerializeField] private UnityEvent _onOrderResolved;
        
        [SerializeField] private DynamicText _priceText;
        [SerializeField] private List<Sprite> _priceBackground = new();
        [SerializeField] private Image _background;

        private Order _currentOrder;
        private List<OrderingCounter> _orderingCounters;
        private TimerManager _timerManager;
        private OrderManager _orderManager;
        private AdaptiveClientSpawner _spawner;

        #endregion

        #region Methods

        private void Start()
        {
            _timerManager = ReferenceManager.Instance.GetManager<TimerManager>();
            _orderManager = ReferenceManager.Instance.GetManager<OrderManager>();
            Subscribe();

        }

        private void Subscribe()
        {
            if (_orderManager == null)
            {
                Debug.LogWarning("[OrderPreview] : An order preview script tried to setup, but there is no OrderManager to be found");
                return;
            }
            if (!_filterByClient)
            {
                _orderManager.OnNewOrder += SetOrder;
            }
            else
            {
                if (_counter != null)
                {
                    _counter.OnUserRegister.AddListener(() => SetOrder());
                }
            }
            _orderManager.OnOrderResolved += OrderResolved;
            _orderManager.OnOrderCancelled += OrderResolved;

        }

        private void ResetSprites()
        {
            if (_ingredientSprites == null)
            {
                Debug.LogWarning("[OrderPreview] : The ingredient list was null");
                return;
            }
            foreach (Image image in _ingredientSprites)
            {
                if (image == null) continue;
                image.sprite = null;
                image.color = new Color(0f, 0f, 0f, 0f);
            }
            
            if(_priceBackground.Count>0)
                _background.sprite = _priceBackground[0];
            
            if(_priceText)
                _priceText.UpdateUI();
        }

        public void Reset()
        {
            ResetSprites();
            _currentOrder = null;
            _onOrderResolved?.Invoke();
        }

        public void SetOrder(Order order=null)
        {
            if (order == null)
            {
                Client client = _counter.CurrentClient;
                order = client.Order;
            }
            if (order == null)
            {
                Debug.LogWarning("[OrderPreview] : Order was null during setup. Setup didn't occur");
                return;
            }
            _currentOrder = order;
            
            UpdateRecipe(order.Recipe);
            
            if (_updateDelay != 0)
            {
                _timerManager.StartTimer("UpdateOrderPreview (" + Time.time + ")", _updateDelay, ()=> _onOrderReceived?.Invoke());
            }
            else
            {
                _onOrderReceived?.Invoke();
            }
            
            if (_timerManager != null && _resetDelay != 0)
            {
                //Debug.Log("[OrderPreview] : Timer started for resetting the order");
                _timerManager.StartTimer("ResetOrderPreview", _resetDelay, Reset);
            }

        }

        private void OrderResolved(Order order)
        {
            if (order == _currentOrder)
            {
                Reset();
            }
        }

        public void UpdateRecipe(SO_Recipe recipe)
        {
            if (_ingredientSprites == null)
            {
                Debug.LogWarning("[OrderPreview] : The ingredient list was null");
                return;
            }

            int recipeCount = recipe.Ingredients.Count;
            
            if (recipeCount > _ingredientSprites.Count)
            {
                Debug.LogWarning("[OrderPreview] : The requested recipe had more ingredients than there are image slots referenced. Please add more image slots.");
                return;
            }
            for (int i = 0; i < recipeCount; i++)
            {
                SO_Ingredient ingredient = recipe.Ingredients[i];
                Image image = _ingredientSprites[i];
                if (ingredient == null) continue;
                if (image == null) continue;
                image.sprite = ingredient.Sprite;
                image.color = Color.white;
            }
        }

        public void UpdateRecipePreview()
        {
            if(_priceBackground.Count >= _currentOrder.Recipe.Ingredients.Count)
                _background.sprite = _priceBackground[_currentOrder.Recipe.Ingredients.Count];
            
            if(_priceText)
                _priceText.UpdateUI();
        }
        
        public void CameraFocus(float duration)
        {
            bool revealMouse = duration == 0;
            
            ReferenceManager.Instance.GetManager<CameraManager>().LookAt(GetComponentInChildren<Canvas>().transform,duration,revealMouse);
        }


        #endregion
    }

}