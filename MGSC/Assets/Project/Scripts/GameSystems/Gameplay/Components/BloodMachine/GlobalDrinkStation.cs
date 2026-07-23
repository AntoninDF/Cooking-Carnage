using Features;
using UnityEngine;

namespace Project.Scripts.GameSystems.Gameplay.Components.BloodMachine
{
	public class GlobalDrinkStation : MonoBehaviour, IFeatureCollection
	{
		public FeatureCollection Collection { get; }

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
			BindEvents();
		}

		public void CreateFeatures()
		{
			_reserve = new F_BloodReserve(this, _bloodPerDrinks, _maxBloodQuantity);
		
			_assStation.Initialize(this, this, _reserve, _timeToRefillDrink, _timeBeforeLoosingBlood, _bloodLostedPerSeconds);
			_eyesStattion.Initialize(this, this, _reserve, _timeToRefillDrink, _timeBeforeLoosingBlood, _bloodLostedPerSeconds);
			_toothStation.Initialize(this, this, _reserve, _timeToRefillDrink, _timeBeforeLoosingBlood, _bloodLostedPerSeconds);
		}
	
		private void BindEvents()
		{
			_assStation.BindEvents();
			_eyesStattion.BindEvents();
			_toothStation.BindEvents();
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
	}
}