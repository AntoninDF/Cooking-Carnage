using System.Collections.Generic;
using Clients;
using Features;
using UnityEngine;
using UnityEngine.Serialization;
using Upgrades;

namespace Project.Scripts.BloodMachine
{
	public class GlobalDrinkStation : MonoBehaviour, IFeatureCollection, IUpgradable<UnlockDrinkMachine>
	{
		#region Properties
		
		public FeatureCollection Collection      { get; set; } = new();
		
		public F_Valve          AssValve          => _assStation.valve;
		public F_BloodReserve   AssBloodReserve   => _assStation.reserve;
		public F_ValveAnimation AssValveAnimation => _assStation.animator;
		
		public F_Valve          EyeValve          => _eyesStation.valve;
		public F_BloodReserve   EyeBloodReserve   => _eyesStation.reserve;
		public F_ValveAnimation EyeValveAnimation => _eyesStation.animator;
		
		public F_Valve          ToothValve          => _toothStation.valve;
		public F_BloodReserve   ToothBloodReserve   => _toothStation.reserve;
		public F_ValveAnimation ToothValveAnimation => _toothStation.animator;
		
		#endregion
		
		
		[SerializeField] private List<BloodPerClient> BloodPerClients = new List<BloodPerClient>();
		
		[Header("Reglages")]
		[SerializeField] private float _bloodPerDrinkCl      = 30f; 
		[SerializeField] private int   _drinksPerFullReserve = 3;   
		[SerializeField] private float _timeToRefillDrink     = 5f; 
		[SerializeField] private float _timeBeforeLoosingBlood = 2f;

		private float _maxBloodQuantity      => _bloodPerDrinkCl * _drinksPerFullReserve;
		private float _bloodLostedPerSeconds => _bloodPerDrinkCl / _timeToRefillDrink;

		public DrinkStation _assStation;
		public DrinkStation _eyesStation;
		public DrinkStation _toothStation;

		private DrinkOrder drinkPicker;
	
		public F_BloodReserve _reserve;

		private void Awake()
		{
			drinkPicker = new DrinkOrder();
		}

		private void Start()
		{
			CreateFeatures();
		}

		public void CreateFeatures()
		{
			_reserve = new F_BloodReserve(this, _bloodPerDrinkCl, _maxBloodQuantity);

			_assStation.Initialize(this, this, _reserve, _timeToRefillDrink, _timeBeforeLoosingBlood, _bloodLostedPerSeconds);
			_eyesStation.Initialize(this, this, _reserve, _timeToRefillDrink, _timeBeforeLoosingBlood, _bloodLostedPerSeconds);
			_toothStation.Initialize(this, this, _reserve, _timeToRefillDrink, _timeBeforeLoosingBlood, _bloodLostedPerSeconds);

			BindEvents();
			FillReserveToMin();
		}
	
		private void BindEvents()
		{
			_assStation.BindEvents();
			_eyesStation.BindEvents();
			_toothStation.BindEvents();
		}

		public void TryGetBlood(EClientType clientType)
		{
			Debug.Log("[Global Drink Station]Try Get Blood Called");
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

			_reserve.FillAllToMin();
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

		public void OnUpgradeInit(UnlockDrinkMachine upgrade)
		{
			
		}

		public void OnUpgraded(UnlockDrinkMachine upgrade)
		{
			Globals.Gameplay.AllowBlending = new (true);
		}
	}
}