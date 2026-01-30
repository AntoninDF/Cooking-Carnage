using GameSystem;
using Orders;
using System.Collections.Generic;
using UI;
using UnityEngine;

public class ServingFeedback : MonoBehaviour
{
    #region Fields
    [SerializeField] private List<OrderTooltip> _replaceData;
    #endregion

    #region Methods

    public void Play(int orderID)
    {
        var om = GameSystem.Reference.GetManager<OrderManager>();
        if (om == null) return;
        Order targetOrder = om.GetOrderByID(orderID);
        if (targetOrder == null) return;

        
    }




    [System.Serializable]
    public enum EOrderData
    {
        None,
        RequestedOrderName,
        ServedOrderName,
        MoneyGained,
        ClientSatisfaction,

    }

    [System.Serializable]
    public struct OrderTooltip
    {
        public Tooltip Tooltip;
        public EOrderData TargetOrderData;

    }
    #endregion
}
