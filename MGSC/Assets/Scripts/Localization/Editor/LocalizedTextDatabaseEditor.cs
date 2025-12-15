using Localization;
using NaughtyAttributes.Test;
using System;
using System.Collections.Generic;
using TableUtility;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LocalizedTextDatabase))]
public class LocalizedTextDatabaseEditor : Editor
{
    bool dictExpanded;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        LocalizedTextDatabase data = (LocalizedTextDatabase)target;

        DrawDict("Data", data.TextByIDByLanguage, ref dictExpanded);


        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Read from google spreadsheets"))
        {
            if (!(data.Reader != null && data.Reader is GoogleSheetsReader))
                data.Reader = ScriptableObject.CreateInstance<GoogleSheetsReader>();
            data.Load();
        }
        /* Not implemented yet
        if (GUILayout.Button("Read from CSV"))
        {
            data.Reader = ScriptableObject.CreateInstance<GoogleSheetReader>();
        }*/
        GUILayout.EndHorizontal();

        DrawReader(data);
        EnforceValidLanguage(data);
    }

    private void EnforceValidLanguage(LocalizedTextDatabase data)
    {
        if (data.CurrentLanguage.GetHashCode() == 0)
            data.CurrentLanguage = (AvailableLanguages)1;
    }

    private void DrawReader(LocalizedTextDatabase data)
    {
        if (data.Reader == null)
            return;

        Editor editor = Editor.CreateEditor(data.Reader);
        editor.DrawDefaultInspector();
    }

    private void DrawDict<TKey, TValue>(string label, Dictionary<TKey, TValue> dict, ref bool expanded)
    {
        if (dict == null) 
            return;
        if (!(typeof(TKey).IsSerializable && typeof(TValue).IsSerializable))
            return;

        expanded = EditorGUILayout.Foldout(expanded, label);

        if (expanded)
        {
            GUILayout.BeginVertical();
            EditorGUI.indentLevel++;
            foreach (var kvp in dict)
            {
                GUILayout.BeginHorizontal();
                DrawValue(kvp.Key);
                DrawValue(kvp.Value);
                GUILayout.EndHorizontal();

            }
            EditorGUI.indentLevel--;

            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            GUILayout.EndVertical();
        }
    }

    // Thanks ChatGPT
    private void DrawValue(object value)
    {
        if (value == null)
        {
            EditorGUILayout.LabelField("null");
            return;
        }

        switch (value)
        {
            case int i:
                EditorGUILayout.IntField(i);
                break;

            case float f:
                EditorGUILayout.FloatField(f);
                break;

            case string s:
                EditorGUILayout.TextField(s);
                break;

            case bool b:
                EditorGUILayout.Toggle(b);
                break;

            case Enum e:
                EditorGUILayout.EnumPopup(e);
                break;

            case Dictionary<string, string> d:
                bool yup = true;
                DrawDict("Dict", d, ref yup);
                break;

            case UnityEngine.Object obj:
                EditorGUILayout.ObjectField(obj, obj.GetType(), false);
                break;

            default:
                EditorGUILayout.LabelField(value.ToString());
                break;
        }
    }
}
