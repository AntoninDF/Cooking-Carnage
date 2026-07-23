using System;
using Clients;
using Features;
using UnityEngine;

namespace Project.Scripts.BloodMachine
{
	[Serializable]
	public class DrinkStation : IFeatureCollection
	{
		public FeatureCollection Collection { get; } = new();

		[SerializeField] private EClientType       Type;
		[SerializeField] private InteractionButton ValveButton;
		
		[Header("Drink")]
		[SerializeField] private InteractionButton DrinkButton;
		[SerializeField] private GameObject Drink;

		[Header("Liquid")]
		[SerializeField] private Renderer _drinkRenderer;
		[SerializeField] private int      _drinkMaterialIndex;
		[SerializeField] private Renderer _reserveRenderer;
		[SerializeField] private int      _reserveMaterialIndex;

		[Header("Animation")]
		[SerializeField] private Animator _animator;

		private MaterialPropertyBlock _drinkBlock;
		private MaterialPropertyBlock _reserveBlock;

		public F_Valve valve;
		public F_BloodReserve reserve;
		public F_ValveAnimation animator;

		public void Initialize(
			IFeatureCollection owner,
			GlobalDrinkStation globalDrinkStation,
			F_BloodReserve     reserve, 
			float              firstTimerTime, 
			float              secondTimerTime, 
			float              bloodDecreaseOverTime)
		{
			_drinkBlock   = new MaterialPropertyBlock();
			_reserveBlock = new MaterialPropertyBlock();

			this.reserve = reserve;
			valve = new F_Valve(owner, this, globalDrinkStation, reserve, Type, firstTimerTime, secondTimerTime, bloodDecreaseOverTime);
			animator = new F_ValveAnimation(this, _animator);
		}

		public void ActualizeDrinkValue(float targetValue)
		{
			_drinkRenderer.GetPropertyBlock(_drinkBlock, _drinkMaterialIndex);
			_drinkBlock.SetFloat("_LiquidLevel", targetValue);
			_drinkRenderer.SetPropertyBlock(_drinkBlock, _drinkMaterialIndex);
		}
	
		public void ActualizeReserveValue(float targetValue)
		{
			///HARD CODE
			float clampedValue = targetValue / 100;
			_reserveRenderer.GetPropertyBlock(_reserveBlock, _reserveMaterialIndex);
			_reserveBlock.SetFloat("_LiquidLevel", clampedValue);
			_reserveRenderer.SetPropertyBlock(_reserveBlock, _reserveMaterialIndex);
		}

		public void BindEvents()
		{
			ValveButton.OnInteraction       += _ => valve.TryServeDrink();
			DrinkButton.OnInteraction += _ => valve.TryGrabbDrink();
			
			
			reserve.OnBloodAdded += CheckReserve;
			reserve.OnBloodRemoved += CheckReserve;

			valve.StartServeDrink += animator.StartServing;
			valve.OnDrinkTaked += animator.StopServe;
			valve.OnDrinkTaked += () => Drink.SetActive(false);
		}

		private void CheckReserve(EClientType clientType, float targetValue)
		{
			if(clientType == Type)
				ActualizeReserveValue(targetValue);
		}
	}
}