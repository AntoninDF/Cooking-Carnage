using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(Canvas), typeof(CanvasScaler))]
    public class FullscreenWidget : UIWidget
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

        #endregion


        /// <summary>
        /// OnLoad gets called when the widget is instantiated into the scene
        /// </summary>
        public virtual void OnLoad() { }
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
            //Debug.Log($"[UIWidget] '{name}' has been enabled.");
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
            //Debug.Log($"[UIWidget] '{name}' has been disabled.");
        }

    }

}