using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [HideScriptField]
    [HelpBox("If you name your GameObjects exactly the same as the variable names, you can click 'Auto assign fields' and it will work the magic for you :)")]
    public class UIWidget : MonoBehaviour
    {
        
        #region Methods
        [Button("Auto assign fields")]
        protected virtual void Reset()
        {
            BindAllFields();
            if (TryGetComponent(out Canvas canvas))
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            if (TryGetComponent(out CanvasScaler scaler))
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new(1920, 1080);
            }
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

        #endregion
    }
}