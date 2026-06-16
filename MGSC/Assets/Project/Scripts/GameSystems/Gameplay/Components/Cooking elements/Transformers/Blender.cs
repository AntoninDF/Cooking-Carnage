using System;
using Controller;
using Features;
using GameSystem;
using UI;
using UnityEngine;
using UnityEngine.Events;


namespace Cooking
{


    public class Blender : MonoBehaviour, IFeatureCollection
    {
        #region Properties
        public FeatureCollection Collection { get; set; } = new();

        public F_SoloContainer Container { get; set; }
        public Cutter Cutter { get; set; }
        public F_Interactable Button;
        #endregion

        #region Fields
        [SerializeField] private SlotContainer _slotContainer;
        [SerializeField] private InteractionButton _cuttingTrigger;
        #endregion



        #region Methods
        void Awake()
        {
            BindEvents();
        }

        private void BindEvents()
        {
            Cutter = new(this);
        }

        void Start()
        {

            if (_slotContainer == null)
                Debug.LogWarning("Blender " + name + " has no slot referenced in the inspector");
            else
                Container = _slotContainer.Container;

            BindExternalEvents();
            Collection.LateSetup();
        }

        private void BindExternalEvents()
        {
            if (_cuttingTrigger != null)
            {
                if (_cuttingTrigger.TryGetFeature(out Button))
                {
                    Button.OnInteraction += TryCut;
                }
            }
            if (Container != null)
            {
                Cutter.OnTransformationStart += _ => Container.IsLocked = true;
                Cutter.OnTransformationOver += _ => Container.IsLocked = false;
            }
        }
        private void TryGrab(InteractionContext context)
        {
            if (context.TryGetFeature(out F_Container container))
            {
                if (Container.RequestGrab(container.HeldItem))
                    return;

                container.RequestGrab(Container.HeldItem);
            }
        }
        private void TryCut(InteractionContext context)
        {
            if (Container.HeldItem == null) return;
            if (!Container.HeldItem.TryGetSibling(out F_Cuttable cuttable, true) || !cuttable.CanBeTransformed) return;
            if (!Cutter.CanStartTransformation(cuttable)) return;
            Cutter.RequestStartTransformation(cuttable);
        }
        #endregion
    }

}