using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(RectTransform))]
    [HideScriptField]
    [HelpBox("If you name your GameObjects exactly the same as the variable names, you can click 'Auto assign fields' and it will work the magic for you :)")]
    public class UIWidget : MonoBehaviour
    {
        private bool _debugFieldScan = false;
        #region Methods
        [Button("Auto assign fields")]
        protected void Reset()
        {
            BindAllFields();
            if (TryGetComponent(out Canvas canvas))
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            if (TryGetComponent(out CanvasScaler scaler))
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new(1920, 1080);
            }
            OnReset();
        }
        protected virtual void OnReset() {}

        private void BindAllFields()
        {
            var fields = GetSerializedFields();
            foreach (var field in fields)
            {
                if (_debugFieldScan) Debug.Log(field.Name + ": " + field.FieldType.Name);
                var obj = gameObject.FindAnyObjectByName(field.Name, field.FieldType, (a, b) => a.Replace(" ", "") == b.Replace(" ", ""));
                if (_debugFieldScan) Debug.Log("Found : " + obj);
                if (obj == null) continue;
                field.SetValue(this, obj);
            }
        }

        List<FieldInfo> GetSerializedFields()
        {
            List<FieldInfo> result = new();
            FieldInfo[] fields = this.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                if (_debugFieldScan) Debug.Log("Scanning field " + field.Name);
                // Get rid of static fields, non serialized fields, and fields that aren't object references
                if (field.IsStatic) continue;
                bool IsVisibleInInspector = field.GetCustomAttribute(typeof(SerializeField)) != null && field.GetCustomAttribute(typeof(HideInInspector)) == null;
                if (!field.IsPublic && !IsVisibleInInspector) continue;
                if (!field.FieldType.IsSubclassOf(typeof(UnityEngine.Object))) continue;
                if (_debugFieldScan) Debug.Log("Field " + field.Name + " is valid");

                result.Add(field);
            }
            
            //LogSerializedFields(result);
            return result;
        }

        #endregion
    }
}