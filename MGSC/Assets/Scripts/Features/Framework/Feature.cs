using System;
using UnityEngine;

namespace Features
{
    public class Feature
    {
        #region Properties


        public FeatureManager Manager
        {
            get
            {
                if (manager == null)
                    Reference.GetManager(out manager);
                return manager;
            }

            set => manager = value;
        }

        public IFeatureCollection Owner { get; set; }

        public bool Enabled
        {
            get => enabled;
            set
            {
                if (enabled != value)
                {
                    enabled = value;
                    if (enabled) OnEnable?.Invoke();
                    else OnDisable?.Invoke();
                }

            }
        }
        public Action OnEnable { get; set; }
        public Action OnDisable { get; set; }
        #endregion


        #region Fields

        protected FeatureManager manager;
        protected bool enabled = true;

        #endregion

        #region Methods

        public Feature(IFeatureCollection owner)
        {
            Init();
            Owner = owner;
            Owner.Collection.Add(this);
        }

        public virtual void Init() { }
        public virtual void Update() { }
        public virtual void Track() => Manager.Track(this);
        public virtual void Untrack() => Manager.Untrack(this);
        public virtual void IsTracked() => Manager.IsTracked(this);
        public T GetSibling<T>() where T : Feature
        {
            if (Owner == null) return null;
            return Owner.Get<T>();
        }
        public bool TryGetSibling<T>(out T feature) where T : Feature
        {
            feature = null;
            if (Owner == null) return false;
            return Owner.TryGet(out feature);
        }
        /// <summary>
        /// Scans every owner in this feature's hierarchy and tries to find one that has a transform.
        /// </summary>
        /// <returns>The first transform found. If there is none, returns null</returns>
        public Transform GetParentTransform()
        {
            IFeatureCollection owner = Owner;
            int maxIterations = 30;
            int i = 0;
            if (HasTransform(owner, out var t)) return t;

            while (owner is Feature fc && i < maxIterations)
            {
                if (HasTransform(fc.Owner, out t))
                {
                    return t;
                }
                owner = fc.Owner;
                ++i;
            }
            return null;
        }
        /// <summary>
        /// Scans every owner in this feature's hierarchy and tries to find one that has a transform.
        /// </summary>
        /// <returns>Wether or not a transform was found. If there is none, t is null</returns>
        public bool TryGetTransform(out Transform t)
        {
            t = GetParentTransform();
            if (t == null) return false;
            return true;
        }
        private bool HasTransform(object obj, out Transform transform)
        {
            transform = null;
            if (obj == null) return false;
            if (obj is GameObject go)
            {
                transform = go.transform;
                return true;
            }
            if (obj is Component component)
            {
                transform = component.transform;
                return true;
            }
            return false;
        }

        #endregion
    }
}