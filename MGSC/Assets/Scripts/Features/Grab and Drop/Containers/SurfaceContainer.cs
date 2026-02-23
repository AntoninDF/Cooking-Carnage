using Controller;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Cooking
{
    /// <summary>
    /// The surface container is a surface that can receive any IGrabbable object on top of it.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class SurfaceContainer : MonoBehaviour, IFeatureCollection
    {
        #region Properties
        public bool IsLocked { get => Container.IsLocked; set => Container.IsLocked = value; }
        public bool Passthrough => false;

        public FeatureCollection Collection { get; set; } = new();
        public MultiContainer Container { get; set; }
        public ContainerFilter Filter { get; set; }
        public LookAtTarget LookAtTarget { get; set; }
        #endregion

        #region Fields
        [Header("Surface container parameters")]
        [SerializeField] private List<Filter> _filters;
        


        #endregion

        #region Methods

        void Start()
        {
            Container = new(this, transform);
            Filter = new(this);
            Container.Filter = Filter;
            Filter.Filters = _filters;
            LookAtTarget = new LookAtTargetByRaycast(this, transform);

            LookAtTarget.OnStartLookAt += () => UnityEngine.Debug.Log("StartLookat");
            LookAtTarget.OnStopLookAt += () => UnityEngine.Debug.Log("StopLookat");
        }


        void Update()
        {
            Collection.Update();
        }
        
        #endregion
    }

}