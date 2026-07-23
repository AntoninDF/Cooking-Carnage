using System;
using Clients;
using Features;
using UnityEngine;

namespace Project.Scripts.GameSystems.Gameplay.Components.BloodMachine
{
	[Serializable]
	public class DrinkStation : IFeatureCollection
	{
		public FeatureCollection Collection { get; }

		[SerializeField] private EClientType       Type;
		[SerializeField] private InteractionButton DrinkButton;
		[SerializeField] private InteractionButton Valve;

		[SerializeField] private Renderer _drinkRenderer;
		[SerializeField] private int      _drinkMaterialIndex;
		[SerializeField] private Renderer _reserveRenderer;
		[SerializeField] private int      _reserveMaterialIndex;

		private MaterialPropertyBlock _drinkBlock;
		private MaterialPropertyBlock _reserveBlock;

		public F_Valve ValveFeature;

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

			ValveFeature = new F_Valve(owner, this, globalDrinkStation, reserve, Type, firstTimerTime, secondTimerTime, bloodDecreaseOverTime);
		}

		public void ActualizeDrinkValue(float targetValue)
		{
			_drinkRenderer.GetPropertyBlock(_drinkBlock, _drinkMaterialIndex);
			_drinkBlock.SetFloat("_LiquidLevel", targetValue);
			_drinkRenderer.SetPropertyBlock(_drinkBlock, _drinkMaterialIndex);
		}
	
		public void ActualizeReserveValue(float targetValue)
		{
			_reserveRenderer.GetPropertyBlock(_reserveBlock, _reserveMaterialIndex);
			_reserveBlock.SetFloat("_LiquidLevel", targetValue);
			_reserveRenderer.SetPropertyBlock(_reserveBlock, _reserveMaterialIndex);
		}

		public void BindEvents()
		{
			Valve.OnInteraction       += _ => ValveFeature.TryServeDrink();
			DrinkButton.OnInteraction += _ => ValveFeature.TryGrabbDrink();
		}
	}
}