using Features;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Upgrades;

public class Oven : MonoBehaviour, IFeatureCollection
{
    public FeatureCollection Collection { get; set; } = new();
    public F_Interactable Interactable { get; set;  }
    public F_SoloContainer Container { get; set; }
    public Cooker Cooker { get; set; }

    //Will be moved later to be able to be used with Event display
    public UnityEvent OnOvenActivated; 
    public UnityEvent OnOvenDeactivated;
    
    #region Fields
    [SerializeField] private SlotContainer _itemSlot;
    [SerializeField] private InteractionButton _button;
    [SerializeField] private ModelController _graphics;
    [SerializeField] private Feedbacks ProgressFeedbacks;
    [SerializeField] private S_EventVariables EventVariables;
    

    #endregion

    void Awake()
    {
        Interactable = new(this);
        Cooker = new(this);

        BindEvents();
    }
    void Start()
    {
        Collection.LateSetup();
        BindExternalEvents();
        UpdateFeedbacks(0f, false);
        ResetDisplayedSprites();
    }

    private void OnEnable()
    {
        GameLoop.OnReset += ResetThis;
    }
    private void OnDisable()
    {
        GameLoop.OnReset -= ResetThis;
    }

    void ResetThis()
    {
        Container?.Empty();
        Cooker.OnTransformationOver?.Invoke(null);
        Cooker.OnTransformation?.Invoke();
        Container.OnDrop?.Invoke(null);
        ResetDisplayedSprites();
        OnStopCooking();
        UpdateFeedbacks(0f, false);
    }

    private void BindEvents()
    {
        Interactable.OnInteraction += TryInteract;
        Cooker.OnTransformationStart += OnStartCooking;
        Cooker.OnTransformationOver += OnStopCooking;
        Cooker.OnTransformationOver += _ => TryStartOverCooking();
        //Cooker.OnTransformationProgress += OnCookingProgress;
        Cooker.OnCookingProgress += p => UpdateFeedbacks(p, false);
        Cooker.OnOvercookingProgress += p => UpdateFeedbacks(p, true);

        Cooker.OnTransformationOver += obj => RaiseEventVarIfCookedOrOvercooked(obj, EventVariables.OnPlateCooked, EventVariables.OnPlateOverCooked);
    }
    private void BindExternalEvents()
    {
        if (_itemSlot != null)
        {
            Container = _itemSlot.Container;
            Container.Filter.AddCondition(
                item =>
                {
                    if (item is not Feature feature) return false;
                    if (!feature.TryGetSibling<F_Cookable>(out var temp)) return false;
                    if (!Globals.Gameplay.AllowPuttingPlatesInOvens) return false;
                    return true;
                }
                );
            Container.OnGrab += _ => ShowRecipeSprites();
            Container.OnDrop += _ => ResetDisplayedSprites();
            Container.OnDrop += _ => OnStopCooking();
            if (EventVariables.OnPlatePutInOven != null)
                Container.OnGrab += EventVariables.OnPlatePutInOven.Raise;
            if (EventVariables.OnPlateTakenFromOven != null)
                Container.OnDrop += EventVariables.OnPlateTakenFromOven.Raise;
            
            Container.IsLocked = !_isOvenActive;
            Container.OnDrop += obj => RaiseEventVarIfCookedOrOvercooked(obj, EventVariables.OnPlateCookedTaken, EventVariables.OnPlateOverCookedTaken);
        }
        if (_button != null)
        {
            if (_button.TryGetFeature(out F_Interactable interactable))
            {
                interactable.OnInteraction += TryStartCooking;
            }
        }
    }
    void RaiseEventVarIfCookedOrOvercooked(Feature plate, EventVariable cookedEvent, EventVariable overCookedEvent)
    {
        if (plate == null) return;
        bool cooked = false, overCooked = false;
        if (plate.TryGetSibling(out F_Cookable cookable) && cookable.Transformed)
            cooked = true;
        if (plate.TryGetSibling(out F_OverCookable overCookable) && overCookable.Transformed)
            overCooked = true;
        if (cooked && !overCooked)
            cookedEvent?.Raise();
        if (cooked && overCooked)
            overCookedEvent?.Raise();
    }


    private void TryInteract(InteractionContext context)
    {
        if ( context.TryGetFeature(out F_Container container))
        {
            if (Container.RequestGrab(container.HeldItem))
                return;

            container.RequestGrab(Container.HeldItem);
        }
    }
    private void TryStartCooking(InteractionContext context)
    {

        if (Container.HeldItem == null)
        {
            Cooker.FailedLaunchCooking?.Invoke();
            return;
        }
        _button.gameObject.SetActive(false);
        if (Container.HeldItem.TryGetSibling(out F_Cookable cookable, true) && cookable.CanBeTransformed)
        {
            Cooker.RequestStartTransformation(cookable);
            if (EventVariables.OnStartCooking != null) EventVariables.OnStartCooking.Raise();
        }
        else
        {
            TryStartOverCooking(context);
        }

    }
    private void TryStartOverCooking(InteractionContext context=null)
    {
        if (Container.HeldItem == null) return;
        if (!Container.HeldItem.TryGetSibling(out F_OverCookable cookable, true) || !cookable.CanBeTransformed) return;

        bool success = Cooker.RequestStartTransformation(cookable);
    }
    
    private void OnStartCooking(TransformableAfterTimer target)
    {
        if (target is F_Cookable)
            Container.IsLocked = true;
    }
    private void OnStopCooking(TransformableAfterTimer target=null)
    {
        Container.IsLocked = false;
        _button.gameObject.SetActive(true);
        UpdateFeedbacks(0f, false);
        Cooker.RequestStopTransformation();
    }

    private void UpdateFeedbacks(float progression01, bool isOvercooking)
    {
        
        var renderer = ProgressFeedbacks.MaterialProperty.TargetRenderer;
        if (renderer != null)
        {
            MaterialPropertyBlock mpb = new();
            renderer.GetPropertyBlock(mpb);
            mpb.SetFloat(ProgressFeedbacks.MaterialProperty.Name, progression01);
            mpb.SetFloat("_IsOvercooking", isOvercooking ? 1f : 0f);
            renderer.SetPropertyBlock(mpb);
        }
        if (ProgressFeedbacks.AnimatedTransform.Transform != null)
        {
            ProgressFeedbacks.AnimatedTransform.Transform.localRotation = Quaternion.Lerp(ProgressFeedbacks.AnimatedTransform.StartRotation, ProgressFeedbacks.AnimatedTransform.EndRotation, progression01);
        }
        if (ProgressFeedbacks.PercentageText != null)
        {
            ProgressFeedbacks.PercentageText.text = Mathf.Round(100f * progression01).ToString() + "%";
        }
    }
    public List<F_Ingredient> GetContainedIngredients()
    {
        List<F_Ingredient> result = new();
        if (Container.HeldItem == null) return result;
        if (!Container.HeldItem.TryGetSibling(out F_MultiSlotContainer multiContainer)) return result;
        foreach(var slot in multiContainer.SoloContainers)
        {
            // Gros pavelard de if oulala
            if (slot == null) continue;
            if (slot.HeldItem == null) continue;
            if (!slot.HeldItem.TryGetSibling(out F_Grabbable grabbable)) continue;
            if (grabbable == null) continue;
            if (!grabbable.TryGetSibling(out F_Ingredient ingredient)) continue;
            result.Add(ingredient);
        }
        return result;
    }
    public List<Sprite> GetSpritesFromIngredients(List<F_Ingredient> ingredients)
    {
        List<Sprite> result = new();
        foreach(var  ingredient in ingredients)
        {
            if (ingredient == null) continue;
            if (ingredient.Model == null) continue;
            if (ingredient.Model.Sprite == null) continue;
            result.Add(ingredient.Model.Sprite);
        }
        return result;
    }
    public void ShowSpriteList(List<Sprite> sprites)
    {
        ResetDisplayedSprites();

        // Show as many sprites as possible given the input sprites and the available Image components
        int minLength = Mathf.Min(ProgressFeedbacks.IngredientInfoScreens.Count, sprites.Count);
        for (int i = 0; i < minLength; i++)
        {
            Image img = ProgressFeedbacks.IngredientInfoScreens[i];
            Sprite sprite = sprites[i];

            img.gameObject.SetActive(true);
            img.sprite = sprite;
        }
    }

    public void ResetDisplayedSprites()
    {
        // Reset sprites
        foreach (var img in ProgressFeedbacks.IngredientInfoScreens)
        {
            img.sprite = null;
            img.gameObject.SetActive(false);
        }
    }
    
    public bool _isOvenActive = true;

    public void SetOvenActive(bool active)
    {
        Debug.Log($"[Oven] {name} was {(active ? "enabled" : "disabled")}.");
        _isOvenActive = active;
        if (_button != null)
        {
            _button.gameObject.SetActive(active);
            _button.Model.AuthorizeOutlines(active);
        }

        _graphics.AuthorizeOutlines(active); 
        if(Container != null) Container.IsLocked = !active;
        
        if(active) OnOvenActivated?.Invoke();
        else OnOvenDeactivated?.Invoke();

    }

    public void ShowRecipeSprites() => ShowSpriteList(GetSpritesFromIngredients(GetContainedIngredients()));
    [Serializable]
    public struct Feedbacks
    {
        public MaterialProperty MaterialProperty;
        public AnimatedTransform AnimatedTransform;
        public TMP_Text PercentageText;
        public List<Image> IngredientInfoScreens;
    }
    [Serializable]
    public struct MaterialProperty
    {
        public string Name;
        public MeshRenderer TargetRenderer;
    }
    [Serializable]
    public struct AnimatedTransform
    {
        public Transform Transform;
        public Quaternion StartRotation;
        public Quaternion EndRotation;
    }
    [Serializable]
    public struct S_EventVariables
    {
        [SerializeField] public EventVariable OnPlatePutInOven;
        [SerializeField] public EventVariable OnPlateTakenFromOven;
        [SerializeField] public EventVariable OnStartCooking;
        [SerializeField] public EventVariable OnPlateCooked;
        [SerializeField] public EventVariable OnPlateOverCooked;
        [SerializeField] public EventVariable OnPlateOverCookedTaken;
        [SerializeField] public EventVariable OnPlateCookedTaken;
    }
}
