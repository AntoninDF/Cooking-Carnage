using Cooking;
using GameSystem;
using Orders;
using UnityEngine;

namespace Clients
{

    public class OrderingCounter : Counter
    {
        #region Properties

        #endregion

        #region Fields
        [Header("OrderingCounter references")]
        [Tooltip("Prefab that will be instantiated as an order ticket")]
        [SerializeField] private GameObject _ticketPrefab;
        [Tooltip("Slot on which to put the order ticket")]
        [SerializeField] private SlotContainer _ticketSlot;
        [Tooltip("List of all the available recipes")]
        [SerializeField] private SO_RecipeBook _recipeBook;

        private OrderManager _orderManager;

        #endregion

        #region Methods

        public void TryAcceptOrder()
        {
            if (IsFree) return;
            AcceptOrder();
            return;
        }

        private void AcceptOrder()
        {
            if (!EnforceReferences()) return;
            Order newOrder = _orderManager.NewOrder(_currentClient, GetOrder());
            if (newOrder == null)
            {
                Debug.LogError("[OrderingCounter] AcceptOrder : The newly created order was null. Please look into that");
            }
            _currentClient.AcceptOrder(newOrder);
            OrderTicket ticket = MakeTicket(newOrder);
            StoreTicket(ticket);
            Free();
        }

        private OrderTicket MakeTicket(Order order)
        {
            if (_ticketPrefab == null)
                Debug.LogError("[OrderingCounter] : The ticket prefab's reference is missing, please assign it in the inspector");
            GameObject go = Instantiate(_ticketPrefab);
            OrderTicket ticket = go.GetComponent<OrderTicket>();
            if (ticket == null)
                Debug.LogError("[OrderingCounter] : The ticket prefab's OrderTicket component is missing. Please make sure there is one on the root object");
            ticket.SetTargetOrder(order);

            return ticket;
        }

        private void StoreTicket(OrderTicket ticket)
        {
            if (_ticketSlot == null)
                Debug.LogError("[OrderingCounter] : The ticket slot's reference is missing, please assign it in the inspector");
            ticket.SetAnimationMode(false);
            _ticketSlot.Grab(ticket);
        }

        /// <summary>
        /// Defines what order should be asked by the client. Can be modified to take the context into account.
        /// </summary>
        /// <returns></returns>
        private SO_Recipe GetOrder()
        {
            float r = Random.Range(0,1);
            int index = Mathf.FloorToInt((_recipeBook.Recipes.Count - 1) * r);
            return _recipeBook.Recipes[index];
        }

        private bool EnforceReferences()
        {
            if (_orderManager == null)
                _orderManager = ReferenceManager.Instance.GetManager<OrderManager>();
            if (_orderManager == null)
            {
                Debug.LogError("[OrderingCounter] : No order manager found.");
                return false;
            }
            return true;
        }

        #endregion

    }

}