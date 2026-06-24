using Features;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Furnace : MonoBehaviour, IFeatureCollection
{
    public FeatureCollection Collection { get; set; } = new();
    public F_Interactable Interactable { get; set;  }
    public F_SoloContainer Container { get; set; }
    public Cooker Cooker { get; set; }


    #region Fields
    [SerializeField] private SlotContainer _itemSlot;
    [SerializeField] private InteractionButton _button;
    [SerializeField] private Feedbacks ProgressFeedbacks;

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
        OnCookingProgress(0f);
        ResetDisplayedSprites();
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
                    return true;
                }
                );
            Container.OnGrab += _ => ShowRecipeSprites();
            Container.OnDrop += _ => ResetDisplayedSprites();
        }
        if (_button != null)
        {
            if (_button.TryGetFeature(out F_Interactable interactable))
            {
                interactable.OnInteraction += TryStartCooking;
            }
        }
    }

    private void BindEvents()
    {
        Interactable.OnInteraction += TryInteract;
        Cooker.OnTransformationStart += OnStartCooking;
        Cooker.OnTransformationOver += OnStopCooking;
        Cooker.OnTransformationProgress += OnCookingProgress;
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
        if (Container.HeldItem == null) return;
        if (!Container.HeldItem.TryGetSibling(out F_Cookable cookable, true) || !cookable.CanBeTransformed) return;

        Cooker.RequestStartTransformation(cookable);
    }
    private void OnStartCooking(TransformableAfterTimer target)
    {
        Container.IsLocked = true;
    }
    private void OnStopCooking(TransformableAfterTimer target)
    {
        Container.IsLocked = false;
        OnCookingProgress(0f);
    }

    private void OnCookingProgress(float progression01)
    {
        if (ProgressFeedbacks.MaterialProperty.Material != null)
        {
            ProgressFeedbacks.MaterialProperty.Material.SetFloat(ProgressFeedbacks.MaterialProperty.Name, progression01);
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
        Debug.Log("1");
        if (!Container.HeldItem.TryGetSibling(out F_MultiSlotContainer multiContainer)) return result;
        Debug.Log("2");
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
        public Material Material;
    }
    [Serializable]
    public struct AnimatedTransform
    {
        public Transform Transform;
        public Quaternion StartRotation;
        public Quaternion EndRotation;
    }
}
