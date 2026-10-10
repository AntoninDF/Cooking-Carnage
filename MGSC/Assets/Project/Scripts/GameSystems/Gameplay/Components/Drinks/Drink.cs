using Clients;
using Features;
using UnityEngine;

public class Drink : Item
{
	#region Properties
	public EClientType DrinkType => drinkType;
	#endregion

	#region Fields
	[SerializeField] private EClientType   drinkType;

	[SerializeField] private Animation     _animation;
	[SerializeField] private AnimationClip _spawnedAnimation;
	[SerializeField] private AnimationClip _grabbedAnimation;
	[SerializeField] private AnimationClip _droppedAnimation;

	[Header("Liquid")]
	[SerializeField] private Renderer _liquidRenderer;
	[SerializeField] private int      _liquidMaterialIndex;

	private MaterialPropertyBlock _liquidBlock;
	
	public Servable Servable;
	#endregion

	protected override void Awake()
	{
		base.Awake();
		_liquidBlock = new MaterialPropertyBlock();
		_animation.Play(_spawnedAnimation?.name);
		Servable = new(this);
	}

	protected override void BindEvents()
	{
		base.BindEvents();
		Grabbable.OnDropSFX       += (() => _animation.Play(_droppedAnimation?.name));
		Grabbable.OnPlayerGrabSFX += (() => _animation.Play(_grabbedAnimation?.name));
	}

	public void SetLiquidLevel(float shaderValue)
	{
        if (_liquidRenderer == null) return;
        shaderValue = Mathf.Clamp01(shaderValue);
		_liquidRenderer.GetPropertyBlock(_liquidBlock);
		_liquidBlock.SetFloat("_LiquidLevel", shaderValue);
		_liquidRenderer.SetPropertyBlock(_liquidBlock);
	}
		
	protected override void TryInteract(InteractionContext context)
	{
		if (context.TryGetFeature(out F_Container playerContainer))
		{
			if (playerContainer.HeldItem != null && playerContainer.HeldItem.Owner is Item )
			{
				SwapWith(playerContainer.HeldItem);
			}
			else
			{
				playerContainer.RequestGrab(Grabbable);
			}
		}
	}

	// private bool CanBeSwap(F_Container container)
	// {
	// 	return (
	// }
}