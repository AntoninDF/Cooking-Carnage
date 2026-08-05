using System;
using System.Collections.Generic;
using Clients;
using Features;
using UnityEngine;

namespace Project.Scripts.BloodMachine
{
	public class GlobalDrinkStation : MonoBehaviour, IFeatureCollection
	{
		public FeatureCollection Collection { get; set; } = new();
		
		[SerializeField] private List<BloodPerClient> BloodPerClients = new List<BloodPerClient>();

		[SerializeField] private float _maxBloodQuantity = 100f;
		[SerializeField] private float _bloodPerDrinks   = 10f;

		[SerializeField] private float _timeToRefillDrink      = 2f;
		[SerializeField] private float _timeBeforeLoosingBlood = 2f;
		[SerializeField] private float _bloodLostedPerSeconds  = 1f;

		public DrinkStation _assStation;
		public DrinkStation _eyesStattion;
		public DrinkStation _toothStation;
	
		public F_BloodReserve _reserve;
	
		private void Start()
		{
			CreateFeatures();
			
		}

		public void CreateFeatures()
		{
			_reserve = new F_BloodReserve(this, _bloodPerDrinks, _maxBloodQuantity);
		
			_assStation.Initialize(this, this, _reserve, _timeToRefillDrink, _timeBeforeLoosingBlood, _bloodLostedPerSeconds);
			_eyesStattion.Initialize(this, this, _reserve, _timeToRefillDrink, _timeBeforeLoosingBlood, _bloodLostedPerSeconds);
			_toothStation.Initialize(this, this, _reserve, _timeToRefillDrink, _timeBeforeLoosingBlood, _bloodLostedPerSeconds);

			BindEvents();
			FillReserveToMax();
		}
	
		private void BindEvents()
		{
			_assStation.BindEvents();
			_eyesStattion.BindEvents();
			_toothStation.BindEvents();
		}

		public void TryGetBlood(EClientType clientType)
		{
			_reserve.TryAddBlood(clientType, GetBloodQuantity(clientType));
		}
		
		[ContextMenu("Remplir la réserve au maximum")]
		private void FillReserveToMax()
		{
			if (_reserve == null)
			{
				return;
			}

			_reserve.FillAllToMax();
		}

		[ContextMenu("Min")]
		private void FillReserveToMin()
		{
			if (_reserve == null)
			{
				return;
			}

			_reserve.FillAllToMax();
		}

		#region Helpers

		private float GetBloodQuantity(EClientType clientType)
		{
			foreach (var client in BloodPerClients)		
			{
				if(client.clientType == clientType) return client.bloodGained;
			}
			
			Debug.Log($"[Global Drink Station] this client type :{clientType} do not have a blood quantity associated");
			return 0f;
		}

		#endregion
	}

	[Serializable]
	public class BloodPerClient
	{
		public float bloodGained;
		public EClientType clientType;
	}
}