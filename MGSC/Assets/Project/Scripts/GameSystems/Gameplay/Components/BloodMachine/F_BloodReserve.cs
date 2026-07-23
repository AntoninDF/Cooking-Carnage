using System;
using System.Collections.Generic;
using Clients;
using Features;
using UnityEngine;

namespace Project.Scripts.GameSystems.Gameplay.Components.BloodMachine
{
	public class F_BloodReserve : Feature
	{
		#region Properties
		
		public float  MaxBloodQaunity => _maxBloodQuantity;
		public Action OnBloodAdded    { get; set; }
		public Action OnBloodRemoved  { get; set; }
		
		#endregion
		
		#region Fields
		
		private float                          _maxBloodQuantity;
		private float                          _bloodPerDrink;
		private Dictionary<EClientType, float> _quantityPerClient;
		
		#endregion

		public F_BloodReserve(IFeatureCollection owner, float bloodPerDrink) : base(owner)
		{
			_bloodPerDrink     = bloodPerDrink;
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
				Debug.Log($"[F_BloodReserve] this client type {clientType} is not registered");
				return;
			}
			
			float addedBlood = _quantityPerClient[clientType] + quantity;
			_quantityPerClient[clientType] = Mathf.Clamp(addedBlood, 0, MaxBloodQaunity);
		}

		public void RemoveBlood(EClientType clientType, float specificQuantity = 0)
		{
			float finalReserveQuantity = _quantityPerClient[clientType] -
			                             (specificQuantity == 0 ? _bloodPerDrink : specificQuantity);
			_quantityPerClient[clientType] = Mathf.Clamp(finalReserveQuantity, 0, MaxBloodQaunity);
		}

		public bool CanServeDrink(EClientType clientType)
		{
			if (!_quantityPerClient.ContainsKey(clientType))
			{
				Debug.Log($"[F_BloodReserve] this client type {clientType} is not registered");
				return false;
			}

			if (_quantityPerClient[clientType] >= _bloodPerDrink)
			{
				return true;
			}

			return false;
		}

		public float GetDrinkQuantity(EClientType clientYpe)
		{
			return _quantityPerClient[clientYpe];
		}
		
	}
}