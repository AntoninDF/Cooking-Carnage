using Controller;
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace Cooking
{

    /// <summary>
    /// The BaseObject is the foundational base for every interactable object in MGSC. It should provide every base mecanic to be combined in child objects.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class BaseObject : MonoBehaviour, IInteractable, IGrabbable, ISoloContainer, IWatchable
    {
        // Access game data

        // Cook

        // StartTimer

        // Consume

        // Trigger animation

        // Start VFX

        // Play sound

        // Modify game data
        #region Properties
        public PlayerInterface Player { get; set; }
        public CookingInterface Interface { get; set; }
        public Action OnInteract { get; set; }
        public IContainer Container { get; set; }
        public Action OnGrabbed { get; set; }
        public IGrabbable HeldItem { get; set; }
        public Action<IGrabbable> OnGrab { get; set; }
        public bool IsLookedAt { get; set; }
        public Action<IGrabbable> OnDrop { get; set; }
        public Transform ItemSlot => transform;
        public Collider Collider => _collider;

        #endregion

        #region Fields

        protected Transform _defaultParent;
        protected Quaternion _defaultRotation;
        private Collider _collider;
        protected Vector3 _targetPos;
        protected Quaternion _targetRot;
        #endregion

        #region Methods
        public virtual void Awake()
        {
            _defaultParent = transform.parent;
            _defaultRotation = transform.rotation;
            _collider = GetComponent<Collider>();
        }

        public virtual void GetGrabbed(IContainer newContainer)
        {
            Container?.Drop(this);
            Container = newContainer;
            transform.parent = newContainer.transform;
            OnGrabbed?.Invoke();
        }

        public virtual void Grab(IGrabbable item)
        {
            if (HeldItem == null)
            {
                item.GetGrabbed(this);
                HeldItem = item;
                OnGrab?.Invoke(item);
            }
        }

        public virtual void Interact(PlayerInteractor interactor)
        {
            
            Player = interactor.Interface;
            OnInteract?.Invoke();
        }

        public virtual void LookAt(PlayerInteractor interactor)
        {
            Player = interactor.Interface;
            IsLookedAt = true;
            StartCoroutine(StopBeingLookedAt());
        }

        /// <summary>
        /// Coroutine to ensure the IsLookedAt flag is reset next frame, unless it is still looked at in which case it will be set as true again.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerator StopBeingLookedAt()
        {
            yield return default(IEnumerator);
            IsLookedAt = false;
        }

        public void Drop(IGrabbable item)
        {
            HeldItem = null;
            OnDrop?.Invoke(item);
        }
        protected Vector3 FindLowestPoint()
        {
            Ray ray = new Ray(transform.position + Vector3.down * 3, Vector3.up);
            RaycastHit hitInfo;
            Collider.Raycast(ray, out hitInfo, 3);
            return hitInfo.point - transform.position;
        }

        /// <summary>
        /// Not implemented yet, returns true all the time for now
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static bool CheckValidSpot(BaseObject obj, Vector3 pos)
        {
            return true;
        }

        /// <summary>
        /// Automatically sets the target slot to the container being looked at
        /// </summary>
        public void SetTargetSlot()
        {
            if (Container is ISoloContainer)
            {
                _targetPos = ((ISoloContainer)Container).ItemSlot.position;
                _targetRot = ((ISoloContainer)Container).ItemSlot.rotation;
            }
            if (Container is IMultiContainer)
            {
                _targetPos = Player.LookInfo.point - FindLowestPoint();
                _targetRot = _defaultRotation;
            }
        }
        /// <summary>
        /// Sets the target slot at the specified position and rotation
        /// </summary>
        public void SetTargetSlot(Vector3 position, Quaternion rotation)
        {
            _targetPos = position;
            _targetRot = rotation;
        }

        public IEnumerator GoToTargetSlot(float duration = 0.2f)
        {
            AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;

            Vector3 offset = _targetPos - startPos;
            for (float timeElapsed = 0.0f; timeElapsed <= duration; timeElapsed += Time.deltaTime)
            {
                float t = (duration == 0) ? 1 : curve.Evaluate(timeElapsed / duration);
                transform.position = startPos + t * offset;
                transform.rotation = Quaternion.Lerp(startRot, _targetRot, t);
                yield return default(IEnumerator);
            }
        }
        #endregion
    }
}