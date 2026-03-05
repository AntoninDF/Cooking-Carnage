using Clients;
using Cooking;
using Features;
using GameSystem;
using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;


namespace UI
{

    public class DynamicText : MonoBehaviour
    {
        #region Properties

        #endregion

        #region Fields

        [Header("References")]
        [Tooltip("The target text object's to update")]
        [SerializeField] private TMP_Text _textObject;
        [Tooltip("This is the text that will be displayed. Place the markups in it for them to get replaced by actual data")]
        [SerializeField] private string _baseText;
        [Tooltip("Optional gameobject that can provide data")]
        [SerializeField] private GameObject _dataSource;
        [Tooltip("Each element specifies which markup to replace with which value")]
        [SerializeField] private List<UIDataReplacement> _dataReplacement = new();

        private DataManager _dataManager;
        #endregion

        #region Methods
        private void Awake()
        {
            if (!Reference.TryGetManager(out _dataManager)) Debug.LogWarning("[Dynamic text] Start : No datamanager found");
        }
        private void Start()
        {
            if (_textObject == null) _textObject = GetComponent<TMP_Text>();
        }
        private void Update()
        {
            UpdateUI();
        }

        public void UpdateUI()
        {
            if (!isActiveAndEnabled) return;
            if (_textObject == null) return;
            if (_dataManager == null)
            {
                Debug.LogWarning("[DynamicText] : Couldn't update the UI, no DataManager found");
                return;
            }
            string result = string.Copy(_baseText);
            foreach (var textInfo in _dataReplacement)
            {
                if (textInfo.markup == "") continue;
                string data = "";
                if (textInfo.info.GetHashCode() > 49)
                    data = GetDataFromGameObject(textInfo.info);
                else 
                    data = _dataManager.GetDataAsString(textInfo.info);
                result = result.Replace(textInfo.markup, data);
            }
            _textObject.text = result;
            //dataManager.Log( "Data replacement is ", _dataReplacement);
        }

        private string GetDataFromGameObject(EDataInfo info)
        {
            string result = "";
            switch (info)
            {
                case (EDataInfo.IngredientName):
                    var ing = _dataSource.GetComponent<Ingredient>();
                    if (ing != null) result = ing.Model.Name;
                    break;
                case (EDataInfo.MealName):
                    var meal = _dataSource.GetComponent<Meal>();
                    if (meal != null) result = meal.Recipe.Name;
                    break;
                case (EDataInfo.ClientName):
                    var client = _dataSource.GetComponent<Client>();
                    if (client != null) result = client.Model.name;
                    break;
                case (EDataInfo.MealIngredients):
                    var mel = _dataSource.GetComponent<Meal>();
                    if (mel != null) result = ToString(mel.Recipe.Ingredients);
                    break;
                
            }

            return result;
        }

        private string ToString<T>(IEnumerable<T> items)
        {
            string result = "";
            foreach (T item in items)
            {
                result += item.ToString();
                result += ", ";
            }
            return result;
        }

        #endregion


        [System.Serializable]
        public struct UIDataReplacement
        {
            [Tooltip("This text will be replaced by the requested info type")]
            public string markup;
            public EDataInfo info;

            public override string ToString()
            {
                return markup + " as " + info.ToString();
            }
        }

    }

}