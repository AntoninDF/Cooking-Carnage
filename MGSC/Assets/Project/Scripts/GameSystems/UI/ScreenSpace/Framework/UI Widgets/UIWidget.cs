using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(Canvas))]
    [HideScriptField]
    [HelpBox("If you name your GameObjects exactly the same as the variable names, you can click 'Auto assign fields' and it will work the magic for you :)")]
    public class UIWidget : MonoBehaviour
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



        #region Methods


        public virtual void OnLoad() { }
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

        [Button("Auto assign fields")]
        protected virtual void Reset()
        {
            BindAllFields();
            if (TryGetComponent(out Canvas canvas))
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        private void CheckIfFieldReferencesAreNull()
        {
            List<string> emptyFields = new();
            var fields = GetSerializedFields();
            foreach (var field in fields)
            {
                object obj = field.GetValue(this);
                if (obj == null)
                    emptyFields.Add(field.Name);
            }
            Debug.LogWarning($"[UIWidget] All fields have not been referenced on UI widget '{name}'. Please make sure they are all assigned in the inspector. " + emptyFields.ToString_EasyRead());
        }
        private void BindAllFields()
        {
            var fields = GetSerializedFields();
            foreach (var field in fields)
            {
                var obj = gameObject.FindAnyObjectByName(field.Name, field.FieldType, (a, b) => a.Replace(" ", "") == b.Replace(" ", ""));
                if (obj == null) continue;
                field.SetValue(this, obj);
            }
        }

        List<FieldInfo> GetSerializedFields()
        {
            List<FieldInfo> result = new();
            FieldInfo[] fields = this.GetType().GetFields();
            foreach (FieldInfo field in fields)
            {
                // Get rid of static fields, non serialized fields, and fields that aren't object references
                if (field.IsStatic) continue; 
                bool IsVisibleInInspector = field.GetCustomAttribute(typeof(SerializeField)) != null && field.GetCustomAttribute(typeof(HideInInspector)) == null;
                if (!field.IsPublic && !IsVisibleInInspector) continue;
                if (!field.FieldType.IsSubclassOf(typeof(UnityEngine.Object))) continue;

                result.Add(field);
            }
            
            //LogSerializedFields(result);
            return result;
        }
        string LogSerializedFields(List<FieldInfo> fields)
        {
            string result = $"Serialized fields found in {name} (calling type : {this.GetType().Name}";
            foreach(var info in fields)
            {
                result += $"\n\t|| {info.Name} : {info.FieldType}";
            }
            Debug.Log(result.ToString());
            return result.ToString();
        }
        #endregion
    }
}