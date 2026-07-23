using System;
using System.Collections;
using Clients;
using Features;
using GameSystem;
using UnityEngine;

namespace Project.Scripts.GameSystems.Gameplay.Components.BloodMachine
{
	public class F_Valve : Feature
	{
		public EClientType AssociedClientType => _associedClientType;
		
		public Action StartServeDrink   { get; set; }
		public Action DrinkReady        { get; set; }
		public Action StartLoosingBlood { get; set; }
		public Action OnDrinkTaked     { get; set; }
		
		private EClientType    _associedClientType;
		private F_BloodReserve _reserve;
		private DrinkStation    _station;
		
		private bool           isServing;

		private float _bloodDecreaseOverTime;
		private float _firstTimerTime;
		private float _secondTimerTime;

		private string FirstTimerName  => _associedClientType + "first timer launched";
		private string SecondTimerName => _associedClientType + "wait before loosing Blood";

		public F_Valve(IFeatureCollection owner,
			F_BloodReserve                reserve, 
			EClientType clientType, 
			float firstTimerTime, 
			float secondTimerTime, 
			float bloodDecreaseOverTime) : base(owner)
		{
			_reserve            = reserve;
			_station        = Owner as DrinkStation;
			_associedClientType = clientType;

			_firstTimerTime        = firstTimerTime;
			_secondTimerTime       = secondTimerTime;
			_bloodDecreaseOverTime = bloodDecreaseOverTime;
		}

		public void TryServeDrink()
		{
			if (_reserve.CanServeDrink(_associedClientType) && !isServing)
			{
				StartServeDrink?.Invoke();
				TimerManager.StartTimer(FirstTimerName, _firstTimerTime, WaitForPlayerToRetrieve);
			}
		}

		public void WaitForPlayerToRetrieve()
		{
			DrinkReady?.Invoke();
			TimerManager.StartTimer(SecondTimerName, _secondTimerTime, LooseBlood);
		}

		public void LooseBlood()
		{
			StartLoosingBlood?.Invoke();
			_station.StartCoroutine(DecreaseBlood());
		}

		private IEnumerator DecreaseBlood()
		{
			float value = _reserve.GetDrinkQuantity(_associedClientType);
			while (_reserve.GetDrinkQuantity(_associedClientType) > 0 && isServing)
			{
				value -= _bloodDecreaseOverTime * Time.deltaTime;
				_reserve.RemoveBlood(_associedClientType, value);
				yield return null;
			}
		}

		public void TryTakeDrink()
		{
			if (TimerManager.IsTimerRunning(FirstTimerName))
			{
				Debug.Log("[F_Valve] votre boisson est occupée");
				return;
			}
			else
			{
				DrinkTaken();
			}
		}
		
		public void DrinkTaken()
		{
			isServing = false;
			OnDrinkTaked?.Invoke();
			if(TimerManager.IsTimerRunning(SecondTimerName))
				TimerManager.StopTimer(SecondTimerName);
		}
	}
}