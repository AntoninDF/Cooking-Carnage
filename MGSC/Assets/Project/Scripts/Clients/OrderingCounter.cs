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
        [Tooltip("List of all the available recipes")]
        [SerializeField] private SO_RecipeBook _recipeBook;
        [Tooltip("Should order tickets be spawned on order validation ? If not, the references are useless")]
        [SerializeField] private bool _spawnTickets;

        private OrderManager _orderManager;

        #endregion

        #region Methods

        public void TryAcceptOrder()
        {
            if (IsFree) return;

            if (Reference.GetAll<SittingCounter>().Any(counter => counter.IsFree) 
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
            Free();
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

            // Enforce the first three orders of the first shift to be A B and C
            ShiftManager sm;
            Reference.GetManager(out sm);
            if (sm!= null && sm.ShiftCount == 0)
            {
                OrderManager om;
                Reference.GetManager(out om);
                if (om != null)
                {
                    int ordersCount = om.PastOrders.Count + om.Orders.Count;
                    if (ordersCount < 3)
                    {
                        var rb = Reference.AssetReferences.CurrentRecipeBook;
                        if (rb != null)
                        {
                            ordersCount = Mathf.Clamp(ordersCount, 0, rb.Recipes.Count - 1);
                            output = rb.Recipes[ordersCount];
                        }
                    }
                }
            }
            return output;
        }

        private bool EnforceReferences()
        {
            if (!Reference.TryGetManager(out _orderManager))
            {
                Debug.LogError("[OrderingCounter] : No order manager found.");
                return false;
            }
            return true;
        }


        #endregion

    }

}