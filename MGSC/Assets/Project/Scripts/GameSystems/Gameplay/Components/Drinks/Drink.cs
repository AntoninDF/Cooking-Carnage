using UnityEngine;

namespace Project.Scripts.GameSystems.Gameplay.Components.Drinks
{
	public class Drink : Item
	{
		[SerializeField] private Animation     _animation;
		[SerializeField] private AnimationClip _grabbedAnimation;
		[SerializeField] private AnimationClip _droppedAnimation;
		
		protected override void BindEvents()
		{
			base.BindEvents();
			OnItemGrabbed.AddListener( (() => _animation.Play(_grabbedAnimation.name)));
			Grabbable.OnDropSFX += (() => _animation.Play(_droppedAnimation.name));
		}
	}
}