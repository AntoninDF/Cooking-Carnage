using Controller;
using System;
using System.Collections;
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

        public  UnityEvent OnGrabbed { get; set; }
        public IContainer Container { get; set; }
        public PlayerInterface Player { get; set; }
        public Collider Collider => _collider;
        public bool IsLookedAt => Player.LookTarget == gameObject;
        #endregion

        #region Fields

        [Header("References")]
        [Tooltip("Reference the canvas or panel game object that must show when the ingredient is hovered.")]
        [SerializeField] protected GameObject _tooltip;
        [Space]
        [Header("Parameters")]
        [Tooltip("How long wil the ingredient take to go from a place to another when grabbed")]
        [SerializeField] private float _grabAnimationDuration = 0.2f;
        [Space]

        private Coroutine _coroutine;
        protected Transform _defaultParent;
        protected Vector3 _targetPos;
        protected Quaternion _targetRot;
        protected Quaternion _defaultRotation;
        private Collider _collider;
        private bool _wasLookedAtThisFrame;
        #endregion

        #region Methods

        protected virtual void Start()
        {
            _defaultRotation = transform.rotation;
            _collider = GetComponent<Collider>();
        }
        protected virtual void Update()
        {
            UpdateUI();
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
            GrabAnimation();

        }

        private void GrabAnimation()
        {
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }
            SetTargetSlot();
            _coroutine = StartCoroutine(GoToTargetSlot(_grabAnimationDuration));
        }

        /// <summary>
        /// Tell this IWatchable that it is being looked at
        /// </summary>
        /// <param name="interactor"></param>
        public void LookAt(PlayerInteractor interactor)
        {
            Player = interactor.Player;
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
                _targetPos = Player.LookInfo.point - FindLowestPoint() - Container.transform.position;
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
            Ray ray = new Ray(transform.position + Vector3.down * 3, Vector3.up);
            RaycastHit hitInfo;
            Collider.Raycast(ray, out hitInfo, 3);
            return hitInfo.point - transform.position;
        }


        protected virtual void UpdateUI()
        {
            if (_tooltip == null)
                throw new MissingReferenceException("Please reference the tooltip gameobject in the inspector of " + name);
            if (Player == null || !IsLookedAt)
            {
                _tooltip.SetActive(false);
                return;
            }
            _tooltip.SetActive(true);
        }
        #endregion
    }

}