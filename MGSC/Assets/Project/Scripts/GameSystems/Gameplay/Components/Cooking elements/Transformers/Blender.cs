using Features;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;


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
        [SerializeField] private EventVariable OnCutStart;
        [SerializeField] private EventVariable OnCutStop;
        [SerializeField] private UnityEvent OnCutFail;
        #endregion



        #region Methods
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
            Cutter.RequestStopTransformation();
            Container?.Empty();
            Cutter.OnTransformationOver?.Invoke(null);
        }
        void Awake()
        {
            BindEvents();
        }

        private void BindEvents()
        {
            Cutter = new(this);
            if (OnCutStop != null) Cutter.OnTransformationOver += OnCutStop.Raise;
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
        private void TryCut(InteractionContext context)
        {
            if (Container.HeldItem == null) goto CutFail;
            if (!Container.HeldItem.TryGetSibling(out F_Cuttable cuttable, true) || !cuttable.CanBeTransformed) goto CutFail; // Ensure it is cuttable and not already cut
            if (!Cutter.CanStartTransformation(cuttable)) goto CutFail;
            Cutter.RequestStartTransformation(cuttable);
            if (OnCutStart != null) OnCutStart.Raise();

            CutFail:
                OnCutFail?.Invoke();
                return;
        }
        #endregion
    }

}