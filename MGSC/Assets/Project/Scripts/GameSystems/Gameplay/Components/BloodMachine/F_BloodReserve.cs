using System;
using System.Collections.Generic;
using Clients;
using Features;
using UnityEngine;

namespace Project.Scripts.BloodMachine
{
    public class F_BloodReserve : Feature
    {
        #region Properties
       
        public float  MaxBloodQuantity => _maxBloodQuantity;
        public float  BloodPerDrink   => _bloodPerDrink;
        public Action<EClientType, float> OnBloodAdded    { get; set; }
        public Action<EClientType, float> OnBloodRemoved  { get; set; }
       
        #endregion
       
        #region Fields
       
        private float                          _maxBloodQuantity;
        private float                          _bloodPerDrink;
        private Dictionary<EClientType, float> _quantityPerClient;
       
        #endregion

        public F_BloodReserve(IFeatureCollection owner, float bloodPerDrink, float maxBloodQuantity) : base(owner)
        {
            _bloodPerDrink     = bloodPerDrink;
            _maxBloodQuantity  = maxBloodQuantity;
            _quantityPerClient = new();
          
            foreach (EClientType clientType in Enum.GetValues(typeof(EClientType)))
            {
                if (!_quantityPerClient.ContainsKey(clientType))
                {
                _quantityPerClient.Add(clientType, 0);
                }
            }
        }

        public void TryAddBlood(EClientType clientType,
            float                        quantity)
        {
            if (!_quantityPerClient.ContainsKey(clientType))
            {
                Debug.LogWarning($"[F_BloodReserve] this client type {clientType} is not registered");
                return;
            }
            float addedBlood = _quantityPerClient[clientType] + quantity;
            _quantityPerClient[clientType] = Mathf.Clamp(addedBlood, 0, MaxBloodQuantity);
            OnBloodAdded?.Invoke(clientType, _quantityPerClient[clientType]);
            Debug.Log($"[Blood Reserve] the current quantity of the reserve {clientType} is {_quantityPerClient[clientType]}");
        }

        public void RemoveBlood(EClientType clientType, float specificQuantity = 0)
        {
            float removedBlood = (specificQuantity == 0 ? _bloodPerDrink : specificQuantity);
            float finalReserveQuantity = _quantityPerClient[clientType] - removedBlood;
            _quantityPerClient[clientType] = Mathf.Clamp(finalReserveQuantity, 0, MaxBloodQuantity);
            OnBloodRemoved?.Invoke(clientType, _quantityPerClient[clientType]);
        }

        public bool CanServeDrink(EClientType clientType)
        {
            if (!_quantityPerClient.ContainsKey(clientType))
            {
                //Debug.Log($"[F_BloodReserve] this client type {clientType} is not registered");
                return false;
            }

            if (_quantityPerClient[clientType] >= _bloodPerDrink)
            {
                return true;
            }
            else
            {
                //Debug.Log(_quantityPerClient[clientType]);
                //Debug.Log(_bloodPerDrink);
            }

            return false;
        }

        /// <summary> Returns the amount of blood stored for the given client type </summary>
        public float GetDrinkQuantity(EClientType clientType)
        {
            return _quantityPerClient[clientType];
        }

        /// <summary> Returns the normalized amount of blood stored for the given client type</summary>
        public float GetReserveRatio(EClientType clientType)
        {
            if (MaxBloodQuantity <= 0) return 0f;
            return Mathf.Clamp01(GetDrinkQuantity(clientType) / MaxBloodQuantity);
        }

        public void FillAllToMax()
        {
            foreach (EClientType clientType in new List<EClientType>(_quantityPerClient.Keys))
            {
                _quantityPerClient[clientType] = _maxBloodQuantity;
                OnBloodAdded?.Invoke(clientType, _quantityPerClient[clientType]);
            }
        }
       
        public void FillAllToMin()
        {
            foreach (EClientType clientType in new List<EClientType>(_quantityPerClient.Keys))
            {
                _quantityPerClient[clientType] = 0;
                OnBloodAdded?.Invoke(clientType, _quantityPerClient[clientType]);
            }
        }
       
    }
}