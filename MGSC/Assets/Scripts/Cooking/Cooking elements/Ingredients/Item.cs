using Controller;
using GameSystem;
using System;
using System.Collections;
using UI;
using UnityEngine;
using UnityEngine.Events;

namespace Cooking
{
    /// <summary>
    /// Adds every necessary item behaviour
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Item : MonoBehaviour, IGrabbable, IWatchable
    {
        #region Properties

        public UnityEvent OnGrabbed => _onGrabbed;
        public UnityEvent OnLookAt => _onLookAt;
        public UnityEvent OnStopLookAt => _onStopLookAt;
        public IContainer Container { get; set; }
        public PlayerInterface Player { get; set; }
        public Collider Collider => _collider;
        public bool IsLookedAt => Player.LookTarget == gameObject;

        public string ID => UpdateID();
   
        #endregion

        #region Fields

        [Header("Item references")]
        [Tooltip("Reference the canvas or panel game object that must show when the ingredient is hovered.")]
        [SerializeField] protected GameObject _tooltip;
        [Tooltip("The lowest point of the object")]
        [SerializeField] protected Transform _lowestPoint;
        [Space]
        [Header("Item parameters")]
        [Tooltip("How long wil the ingredient take to go from a place to another when grabbed")]
        [SerializeField] private float _grabAnimationDuration = 0.2f;
        [Space]
        [Header("Item events")]
        [Tooltip("Invoked when this item is grabbed by the player or another container")]
        [SerializeField] private UnityEvent _onGrabbed;
        [Tooltip("Invoked when this item is looked at by the player")]
        [SerializeField] private UnityEvent _onLookAt;
        [Tooltip("Invoked when this item stops being looked at by the player")]
        [SerializeField] private UnityEvent _onStopLookAt;
        [Tooltip("Invoked when this item is disabled or destroyed")]
        [SerializeField] private UnityEvent _onDisable;
        [Tooltip("Invoked when this item is enabled")]
        [SerializeField] private UnityEvent _onEnable;

        protected Tooltip _tooltipComponent; 
        private Coroutine _coroutine;
        protected Transform _defaultParent;
        protected Vector3 _targetPos;
        protected Quaternion _targetRot;
        protected Quaternion _defaultRotation;
        private Collider _collider;
        private bool _wasLookedAtThisFrame;
        private bool _animate=true;
        

        private string itemID;
        private Vector3 _defaultPos;
        #endregion

        #region Methods

        protected virtual void Start()
        {
            _defaultPos = transform.position;
            _defaultRotation = transform.rotation;
            _collider = GetComponent<Collider>();
            _tooltipComponent = _tooltip.GetComponent<Tooltip>();
        }
        
        protected virtual void Update()
        {
            UpdateUI();
        }

        protected void SetID(string id)
        {
            itemID = id;
        }

        protected virtual string UpdateID()
        {
            return itemID;
        }

        public virtual void Reset()
        {
            transform.position = _defaultPos;
            transform.rotation = _defaultRotation;
        }

        public void GetGrabbed(IContainer newContainer)
        {
            
            Container?.Drop(this);
         
            Container = newContainer;
            if (newContainer is ISoloContainer)
            {
                ISoloContainer soloContainer = (ISoloContainer)newContainer;
                transform.SetParent(soloContainer.ItemSlot);
            }
            else
            {
                transform.parent = newContainer.transform;
            }
            OnGrabbed?.Invoke();
            GrabAnimation(!_animate);
        }
        public void GetGrabbedWithoutFeedback(IContainer newContainer)
        {
            Container?.Drop(this);
            Container = newContainer;
            if (newContainer is ISoloContainer)
            {
                ISoloContainer soloContainer = (ISoloContainer)newContainer;
                transform.SetParent(soloContainer.ItemSlot);
            }
            else
                transform.parent = newContainer.transform;
            GrabAnimation(!_animate);
        }

        public void SetAnimationMode(bool animate=true)
        {
            _animate = animate;
        }

        private void GrabAnimation(bool instantaneous=false)
        {
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }
            SetTargetSlot();
            if (instantaneous)
            {
                transform.localPosition = _targetPos;
                transform.localRotation = _targetRot;
            }
            else
            {
                _coroutine = StartCoroutine(GoToTargetSlot(_grabAnimationDuration));
            }

        }

        /// <summary>
        /// Tell this IWatchable that it is being looked at
        /// </summary>
        /// <param name="interactor"></param>
        public void LookAt(PlayerInteractor interactor)
        {
            Player = interactor.Player;
            _onLookAt?.Invoke();
            
        }
        public void StopLookAt()
        {
            OnStopLookAt?.Invoke();
        }
        /// <summary>
        /// Automatically sets the target slot to the container being looked at
        /// </summary>
        public void SetTargetSlot()
        {
            if (Container is ISoloContainer)
            {
                _targetPos = Vector3.zero;
                _targetRot = ((ISoloContainer)Container).ItemSlot.rotation;
            }
            if (Container is IMultiContainer)
            {
                _targetPos = Quaternion.Inverse(Container.transform.rotation) * (Player.LookInfo.point - FindLowestPoint() - Container.transform.position);
                //Debug.Log("Target pos is " + _targetPos + "\nDistance to player is " + Vector3.Distance(Player.transform.position, _targetPos));
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

        /// <summary>
        /// Play an ease-in-out animation to go from the current position the the target slot. The target slot should be defined before calling this method.
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        public IEnumerator GoToTargetSlot(float duration = 0.2f)
        {
            AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            Vector3 startPos = transform.localPosition;
            Quaternion startRot = transform.rotation;
            SetTargetSlot();
            Vector3 offset = _targetPos - startPos;

            for (float timeElapsed = 0.0f; timeElapsed <= duration; timeElapsed += Time.deltaTime)
            {
                float t = (duration == 0) ? 1 : curve.Evaluate(timeElapsed / duration);
                transform.localPosition = startPos + t * offset;
                transform.rotation = Quaternion.Lerp(startRot, _targetRot, t);
                yield return default(IEnumerator);
            }
            transform.localPosition = _targetPos;
        }
        /// <summary>
        /// Finds the lowest point of an object via a raycast sent fomr beneath the object
        /// </summary>
        /// <returns>The offset vector from the pivot point towards the lowest point</returns>
        protected Vector3 FindLowestPoint()
        {
            Vector3 lowestPoint;
            if (_lowestPoint == null)
                lowestPoint = Vector3.zero;
            else
                lowestPoint = _lowestPoint.position - transform.position;
            return lowestPoint;
        }


        protected virtual void UpdateUI()
        {
            if (_tooltip == null)
                throw new MissingReferenceException("Please reference the tooltip gameobject in the inspector of " + name);
            if (Player == null || !IsLookedAt)
            {
                if (_wasLookedAtThisFrame) StopLookAt();
                _wasLookedAtThisFrame = false;
                _tooltip.SetActive(false);
                return;
            }
            _wasLookedAtThisFrame = true;
            _tooltip.SetActive(true);
        }

        public void OnDisable() => _onDisable?.Invoke();
        private void OnEnable() => _onEnable?.Invoke();
        #endregion
    }

}