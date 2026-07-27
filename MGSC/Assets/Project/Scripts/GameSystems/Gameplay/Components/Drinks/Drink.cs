using Clients;
using UnityEngine;

namespace Project.Scripts.GameSystems.Gameplay.Components.Drinks
{
	public class Drink : Item
	{
		#region Properties
		public                   EClientType   DrinkType => drinkType;
		#endregion
		
		#region Fields
		[SerializeField] private EClientType   drinkType;
		
		[SerializeField] private Animation     _animation;
		[SerializeField] private AnimationClip _grabbedAnimation;
		[SerializeField] private AnimationClip _droppedAnimation;
		#endregion
		
		
		protected override void BindEvents()
		{
			base.BindEvents();
			OnItemGrabbed.AddListener( (() => _animation.Play(_grabbedAnimation?.name)));
			Grabbable.OnDropSFX += (() => _animation.Play(_droppedAnimation?.name));
		}
	}
}