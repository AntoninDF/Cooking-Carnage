using Cooking;
using Features;
using GameSystem;
using Orders;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Clients
{

    public class ServingCounter : MonoBehaviour, IFeatureCollection
    {
        #region Properties
        public FeatureCollection Collection { get; set; } = new();
        public F_SoloContainer Container { get; set; }
        public Features.ServingCounter ServingListener { get; set; }
        public ServingEvaluator ServingEvaluator { get; set; }

        public Order CurrentOrder { get; set; }

        #endregion

        #region Fields
        [Header("References")]
        [Tooltip("Slot on which the meal will be served.")]
        [SerializeField] private SlotContainer _mealSlot;
        //[SerializeField] private float _timeBeforeMealDisappears = 2.5f;
        [SerializeField] private ClientSeat clientSeat;

        #endregion

        #region Methods

        private void Awake()
        {
            if (_mealSlot != null)
                Container = _mealSlot.Container;
            else
                Container = new(this);
            ServingListener = new(this);
            ServingEvaluator = new(this);

            Container.Filter.AddCondition(item => item.TryGetFeature<Servable>(out var temp, mustBeEnabled:true));

            BindEvents();
        }

        private void BindEvents()
        {
            Container.OnGrab += OnGrab;
        }

        private void OnGrab(F_Grabbable grabbable)
        {
            if (grabbable.TryGetSibling(out Servable servedObj))
            {
                TryValidateServing(servedObj, out var evaluation);
            }

        }

        public bool TryValidateServing(Servable servedObj, out ServingInfo evaluation)
        {
            evaluation = new();
            if (servedObj == null)
                return false;
            if (CurrentOrder == null)
                evaluation = ServingEvaluator.Evaluate(servedObj);
            else
                evaluation = ServingEvaluator.Evaluate(servedObj, CurrentOrder);
            return true;
        }

        public void SetCurrentOrder(Order order) => CurrentOrder = order;
        #endregion
    }

}