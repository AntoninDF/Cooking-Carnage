using Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using TableUtility;
using UnityEngine;

[CreateAssetMenu(fileName = "LocalizedTextDatabase", menuName = "Scriptable Objects/LocalizedTextDatabase")]
public class LocalizedTextDatabase : ScriptableObject, ISerializationCallbackReceiver
{
    [SerializeField]
    public TableReader Reader;
    public AvailableLanguages CurrentLanguage;
    public bool IsSetup => CheckSetup();


    public Dictionary<AvailableLanguages, Dictionary<string, string>> TextByIDByLanguage = new();
    public string[] AvailableLanguages = new string[0];

    private string[] NonLanguageKeywords = {"ID" };
    private int currentLanguageIndex;
    private TableData Data;


    public void Load()
    {
        if (Reader != null)
        {
            Reader.OnReadingComplete += UpdateData;
            Reader.OnReadingComplete += () => ResetAction(ref Reader.OnReadingComplete);
            Reader.ReadSheet();
        }
        else
            throw new System.MissingFieldException("[LocalizedTextDatabase] : Missing Reader field");
    }

    public string Get(string key)
    {
        return Get(key, CurrentLanguage);
    }

    public string Get(string key, AvailableLanguages language)
    {
        language = FindLanguage(language);
        if (!TextByIDByLanguage.ContainsKey(language))
            return "";
        var TextByID = TextByIDByLanguage[language];
        if (!TextByID.ContainsKey(key))
            return "";
        return TextByID[key];
    }

    private void UpdateAvailableLanguages()
    {
        // Is there a better way to catch nullref ?
        if (Reader == null || Reader.Result == null || Reader.Result.values == null || Reader.Result.values.Length == 0)
            return;
        AvailableLanguages = Reader.Result.values[0];

        // Clear values that shouldn't be treated as a language
        for (int i = 0;  i < AvailableLanguages.Length; i++)
            if (NonLanguageKeywords.Contains(AvailableLanguages[i]))
                AvailableLanguages[i] = "";
    }
    private void SetData(TableData data)
    {
        Data = data;
        UpdateAvailableLanguages();
        UpdateDictionary();
    }
    private void UpdateData() => SetData(Reader.Result);
    private void UpdateDictionary()
    {
        foreach(var lang in AvailableLanguages)
        {
            AvailableLanguages curr;
            if (Enum.TryParse(lang, true, out curr))
            {
                TextByIDByLanguage[curr] = TableToDict(Data.values, 0, curr.GetHashCode(), true);
            }
        }
    }
    private void ResetAction(ref Action action) => action = delegate { };
    private AvailableLanguages FindLanguage(AvailableLanguages language) => language.GetHashCode() == 0 ? (AvailableLanguages)CurrentLanguage : language;
    private bool CheckSetup()
    {
        return 
            Reader != null &&
            Reader.Result != null &&
            Data != null &&
            TextByIDByLanguage != null &&
            TextByIDByLanguage.Count > 0 &&
            true
            ;
    }
    private Dictionary<T, T> TableToDict<T>(T[][] table, int keyColumn, int valueColumn, bool skipFirstRow=true)
    {
        if (table == null || table.Length == 0) return null;
        Dictionary<T, T> result = new();
        int startRange = 0;
        if (skipFirstRow)
            startRange = 1;

        for (int i = startRange; i < table.Length; i++)
        {
            if (keyColumn <  table[i].Length && valueColumn < table[i].Length)
                result[table[i][keyColumn]] = table[i][valueColumn];
        }
        return result;
    }

    /// <summary>
    /// Code extracted from the Unity docs
    /// </summary>
    /// 
    [SerializeField]
    public List<AvailableLanguages> keys = new ();
    [SerializeField]
    public List<List<string>> valueKeys = new ();
    [SerializeField]
    public List<List<string>> values = new ();

    // Create a Dictionary. The Unity serializer doesn't support Dictionary types.
    public Dictionary<int, string> myDictionary = new Dictionary<int, string>();

    public void OnBeforeSerialize()
    {
        keys.Clear();
        valueKeys.Clear();
        values.Clear();
        // For each key/value pair in the dictionary, add the key to the keys list and the value to the values list
        foreach (var kvp in TextByIDByLanguage)
        {
            keys.Add(kvp.Key);
            valueKeys.Add(kvp.Value.Keys.ToList());
            values.Add(kvp.Value.Values.ToList());
        }
    }

    public void OnAfterDeserialize()
    {
        TextByIDByLanguage = new ();
        // Loop through the list of keys and values and add each key/value pair to the dictionary
        for (int i = 0; i != Math.Min(keys.Count, values.Count); i++)
        {
            Dictionary<string, string> keyValuePairs = new();
            for (int j = 0; j < keys.Count; j++)
                keyValuePairs[valueKeys[i][j]] = values[i][j];
            TextByIDByLanguage.Add(keys[i], new Dictionary<string, string>());
        }
    }
}
