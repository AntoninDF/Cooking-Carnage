using Features;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
    }
    

    private void BindExternalEvents()
    {
        if (_itemSlot != null)
        {
            Container = _itemSlot.Container;
            Container.Filter.AddCondition(
                item =>
                {
                    Debug.Log("Filter evaluation");
                    if (item is not Feature feature) return false;
                    Debug.Log("Feature detected");
                    if (!feature.TryGetSibling<F_Cookable>(out var temp)) return false;
                    Debug.Log("Cookable detected");
                    return true;
                }
                );
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
    [Serializable]
    public struct Feedbacks
    {
        public MaterialProperty MaterialProperty;
        public AnimatedTransform AnimatedTransform;
        public TMP_Text PercentageText;
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
