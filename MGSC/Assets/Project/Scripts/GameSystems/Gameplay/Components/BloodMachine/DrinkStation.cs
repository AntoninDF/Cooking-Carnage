using System;
using System.Collections.Generic;
using Clients;
using Features;
using UnityEngine;
using UnityEngine.Serialization;

namespace Project.Scripts.GameSystems.Gameplay.Components.BloodMachine
{
	public class DrinkStation : MonoBehaviour, IFeatureCollection
	{
		public FeatureCollection Collection { get; }

		[SerializeField] private float _bloodPerDrinks = 10f;

		[SerializeField] private float _timeToRefillDrink      = 2f;
		[SerializeField] private float _timeBeforeLoosingBlood = 2f;
		[SerializeField] private float _bloodLostedPerSeconds  = 1f;
		
		[SerializeField] private InteractionButton _assValve;
		[SerializeField] private InteractionButton _toothValve;
		[SerializeField] private InteractionButton _eyeValeve;

		private F_BloodReserve                   _reserve;
		private Dictionary<EClientType, F_Valve> _valves;

		private void Start()
		{
			CreateFeatures();
			BindEvents();
		}

		public void CreateFeatures()
		{
			_reserve = new F_BloodReserve(this, _bloodPerDrinks);
			
			_valves  = new();
			foreach (EClientType clientType in Enum.GetValues(typeof(EClientType)))
			{
				F_Valve valve = new F_Valve(this, _reserve, clientType, _timeToRefillDrink, _timeBeforeLoosingBlood, _bloodLostedPerSeconds);
				_valves.Add(clientType, valve);
				Collection.Add(valve);
			}
			
			Collection.Add(_reserve);
			
		}

		public void BindEvents()
		{
			_eyeValeve.OnInteraction += _ => _valves[EClientType.A].TryServeDrink();
			_assValve.OnInteraction  += _ => _valves[EClientType.B].TryServeDrink();
			_toothValve.OnInteraction += _ => _valves[EClientType.C].TryServeDrink();
		}
	}
}