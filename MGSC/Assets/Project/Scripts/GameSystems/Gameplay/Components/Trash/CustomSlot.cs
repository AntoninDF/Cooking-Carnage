using Features;
using Unity.VisualScripting;
using UnityEngine;

namespace Trash
{
    [RequireComponent(typeof(Collider))]
    public class CustomSlot : MonoBehaviour, IFeatureCollection
    {
        public Transform TargetPoint;
        public FeatureCollection Collection { get; set; } = new();

        public F_Grabbable HeldItem => Container.HeldItem;
        public F_SoloContainer Container { get; set; }
        public F_Interactable Interactable { get; set; }
        public F_LookAtTarget LookAtTarget { get; set; }

        public bool IsInteractable
        {
            get => Interactable.Enabled;
            set => Interactable.Enabled = value;
        }

        #region Fields
        [SerializeField] protected bool isInteractable = true;
        [Tooltip("If the slot is passthrough, it will allow the player to grab the item by clicking " +
            "on the slot's collider as well as the ingredient, instead of just the ingredient")]
        [SerializeField] protected bool passThrough = true;
        [SerializeField] protected ModelController Model;
        [SerializeField] protected EventVariable OnGrabEventVar;
        #endregion

        void Awake()
        {
            Container = new(this, TargetPoint);
            Interactable = new(this);
            LookAtTarget = new(this, transform);
            IsInteractable = isInteractable;
        }
        void Start()
        {
            BindEvents();
            SetupMeshCollider();
        }
        void Reset()
        {
            if (isInteractable && !GetComponent<Collider>())
            {
                var col = gameObject.AddComponent<SphereCollider>();
                col.radius = 0.1f;
            }
            else if (!isInteractable && TryGetComponent(out Collider col))
            {
                col.enabled = false;
            }
        }

        protected virtual void BindEvents()
        {
            Interactable.OnInteraction += OnInteract;
            if (Model != null)
            {
                LookAtTarget.OnStartLookAt += Model.Outline.EnableOutline;
                LookAtTarget.OnStopLookAt += Model.Outline.DisableOutline;

            }
        }
        protected void OnInteract(InteractionContext context)
        {
            if (!context.TryGetFeature(out F_Container container)) return;

            if (Container.RequestGrab(container.HeldItem))
            {
                if (OnGrabEventVar != null) OnGrabEventVar.Raise();
                return;
            }

            if (passThrough && container.RequestGrab(Container.HeldItem))
            {
                if (OnGrabEventVar != null) OnGrabEventVar.Raise();
                return;
            }

            if (passThrough
                && container.HeldItem != null
                && container.HeldItem.TryGetSibling(out F_Container heldContainer)
                && heldContainer.RequestGrab(Container.HeldItem))
            {
                if (OnGrabEventVar != null) OnGrabEventVar.Raise();
                return;
            }

            var incoming = container.HeldItem;
            var existing = Container.HeldItem;

            if (incoming != null && existing != null && incoming.Owner is Item incomingItem)
            {
                if (incomingItem.SwapWith(existing))
                {
                    if (OnGrabEventVar != null)
                        OnGrabEventVar.Raise();
                    return;
                }
            }
        }

        protected void SetupMeshCollider()
        {
            if (Model == null) return;

            MeshCollider col;
            if (!TryGetComponent(out col))
            {
                foreach (var collider in transform.GetComponents<Collider>())
                {
                    collider.enabled = false;
                }
                col = transform.AddComponent<MeshCollider>();
            }
            
            col.sharedMesh = Model.GetCombinedMeshes(transform);
        }
    }
}