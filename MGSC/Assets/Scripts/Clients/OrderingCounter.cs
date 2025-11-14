using System.Linq;
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

            if (ReferenceManager.Instance.GetElementsOfType<SittingCounter>().Any(counter => counter.IsFree) 
                && _currentClient.CheckClientCounterDistance(this))
            {
                AcceptOrder(); 
            }
            
            // Order can't be accepted cause there is no sitting counter available
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
            _ticketSlot.Drop(_ticketSlot.HeldItem);
            _ticketSlot.Grab(ticket);
        }

        /// <summary>
        /// Defines what order should be asked by the client. Can be modified to take the context into account.
        /// </summary>
        /// <returns></returns>
        private SO_Recipe GetOrder()
        {
            SO_Recipe output;
            /*
            if (_recipeBook == null)
                Debug.LogError("[OrderingCounter] (" + name + ") : Please reference a recipe book in the inspector");
            int index = Random.Range(0, _recipeBook.Recipes.Count);
            output = _recipeBook.Recipes[index];
            */
            output = OrderPicker.Pick();
            return output;
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

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _reachDistance);
        }

        #endregion

    }

}