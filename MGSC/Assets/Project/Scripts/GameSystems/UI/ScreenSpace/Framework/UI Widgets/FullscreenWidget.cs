using Features;
using JetBrains.Annotations;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(Canvas), typeof(CanvasScaler))]
    public class FullscreenWidget : UIWidget, IFeatureCollection
    {
        #region Properties
        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (value) Enable();
                else Disable();
            }
        }
        private bool _enabled;
        public Action OnEnable;
        public Action OnDisable;

        public UISystem Owner { get; set; }
        public virtual bool ForceLoadOnUIInitialization => false;

        public FeatureCollection Collection { get; set; } = new();
        public F_FullscreenWidgetActions Actions;
        #endregion


        /// <summary>
        /// OnLoad gets called when the widget is instantiated into the scene
        /// </summary>
        public virtual void OnLoad() 
        {
            Actions = new(this);
            OnEnable += () => Actions.OnWidgetAppears?.Invoke();
            OnDisable += () => Actions.OnWidgetDisappears?.Invoke();
        }
        /// <summary>
        /// Init is called when the widget's prefab gets added to the UI System database (at the beginning of the game)
        /// </summary>
        public virtual void Init() { }
        /// <summary>
        /// Show the widget on the UI
        /// </summary>
        public void Enable()
        {
            if (Owner == null)
            {
                Debug.LogWarning($"[UIWidget] You are trying to enable a widget '{name}' which has not yet been loaded by a UI System. " +
                    $"Make sure to call UISystem.LoadAllWidgets() or UISystem.GetWidget() first.");
                return;
            }
            if (_enabled) return;
            Owner.ShowWidget(this);
            _enabled = true;
            OnEnable?.Invoke();
        }
        /// <summary>
        /// Hide the widget from the UI
        /// </summary>
        public void Disable()
        {
            if (!_enabled) return;
            Owner?.StashWidget(this);
            _enabled = false;
            OnDisable?.Invoke();
        }

        public class F_FullscreenWidgetActions : Feature
        {
            public F_FullscreenWidgetActions(IFeatureCollection owner) : base(owner){}

            public Action OnWidgetAppears;
            public Action OnWidgetDisappears;
        }
    }

}