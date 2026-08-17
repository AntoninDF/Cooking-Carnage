using Clients;
using Features;
using System.Collections.Generic;
using UnityEditor;
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

		/// <summary>
		/// This method is invoked by the UnityEvent <see cref="OrderingArea.OnCustomerBlended"/> in inspector
		/// </summary>
		/// <param name="clientType"></param>
		public void TryGetBlood(EClientType clientType)
		{
		//	Debug.Log("[Global Drink Station]Try Get Blood Called");
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
			
			//Debug.Log($"[Global Drink Station] this client type :{clientType} do not have a blood quantity associated");
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
		
#if UNITY_EDITOR
    void OnDrawGizmos()
        {
            float baseHandleHeight = 1f;
            float increment = 0.2f;
            var cam = SceneView.lastActiveSceneView.camera;
            if (cam != null) increment = Vector3.Distance(cam.transform.position, transform.position) / 50f;
            int handleCount = 0;
            float GetY()
            {
                handleCount++;
                return baseHandleHeight + (handleCount - 1) * increment;
            }
            if (!Application.isPlaying) return;

			void DisplayInfoForDrinkStation(DrinkStation drinkStation)
			{
				Gizmos.color = Color.white;
                UnityEditor.Handles.Label(transform.position + Vector3.up * GetY(), "Station type : " + drinkStation.Type);
                UnityEditor.Handles.Label(transform.position + Vector3.up * GetY(), "Reserve : " + Mathf.Round(drinkStation.reserve.GetDrinkQuantity(drinkStation.Type)*100)/100 + "cL (ratio is " 
																					+ Mathf.Round(drinkStation.reserve.GetReserveRatio(drinkStation.Type) * 100) / 100 + "/1)");
				UnityEditor.Handles.Label(transform.position + Vector3.up * GetY(), "Is serving ? : " + (drinkStation.valve.IsServing ? "Yup" : "Nope"));
				UnityEditor.Handles.Label(transform.position + Vector3.up * GetY(), "Serving timer : " + 
					(drinkStation.valve.ServingTimer != null ? (Mathf.Round(drinkStation.valve.ServingTimer.Elapsed * 100) / 100) + "/" + drinkStation.valve.ServingTimer.Duration + "s" : "0/0s"));
				UnityEditor.Handles.Label(transform.position + Vector3.up * GetY(), "Overflowing timer : " + 
					(drinkStation.valve.OverflowingTimer != null ? (Mathf.Round(drinkStation.valve.OverflowingTimer.Elapsed * 100) / 100) + "/" + drinkStation.valve.OverflowingTimer.Duration + "s" : "0/0s"));
            }
            
        }
#endif
    }
}