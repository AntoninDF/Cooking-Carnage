using GameSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Upgrades;


namespace UI
{

    public class DynamicText : MonoBehaviour
    {
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
        #endregion

        #region Methods
        private void Start()
        {
            if (_textObject == null) _textObject = GetComponent<TMP_Text>();
            if (_textObject == null) Debug.LogWarning("[DynamicText] No text component has been referenced on " + name);
        }
        private void Update()
        {
            UpdateUI();
        }

        public void UpdateUI()
        {
            if (!isActiveAndEnabled) return;
            if (_textObject == null) return;
            string result = string.Copy(_baseText);
            foreach (var textInfo in _dataReplacement)
            {
                if (textInfo.markup == "") continue;
                string data = "";
                if (textInfo.info.GetHashCode() > 49)
                    data = GetDataFromGameObject(textInfo.info);
                else 
                    data = GetDataAsString(textInfo.info) ?? data;
                result = result.Replace(textInfo.markup, data);
            }
            _textObject.text = result;
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
                    var meal = _dataSource.GetComponent<Plate>();
                    if (meal != null) result = meal.Recipe?.Name;
                    break;
                case (EDataInfo.MealIngredients):
                    var mel = _dataSource.GetComponent<Plate>();
                    if (mel != null) result = ToString(mel.Recipe?.Ingredients);
                    break;
                
            }

            return result;
        }
        public string GetDataAsString(EDataInfo info)
        {
            string result = "";

            switch (info)
            {
                case EDataInfo.TimeElapsedSinceStartup:         return Time.time.ToString();
                case EDataInfo.ShiftDuration:                   return Reference.GetManager<ShiftManager>()?.ShiftDuration.ToString();
                case EDataInfo.RentPayedThisShift:              return Reference.GetManager<ShiftManager>()?.GetCurrentRentPrice().ToString();
                case EDataInfo.ShiftCount:                      return Reference.GetManager<ShiftManager>()?.ShiftNumber.ToString();
                case EDataInfo.CorrectOrdersThisShift:          return GetCorrectOrdersCount().ToString();
                case EDataInfo.WrongOrdersThisShift:            return GetWrongOrdersCount().ToString();
                case EDataInfo.MissedOrdersThisShift:           return GetMissedOrdersCount().ToString();
                case EDataInfo.ClientsKilledThisShift:          return Reference.GetManager<ClientManager>()?.ClientsKilledThisShift.ToString();
                case EDataInfo.MoneyGainedThisShift:            return Reference.GetManager<MoneyManager>()?.GetMoneyGainedThisShift().ToString();
                case EDataInfo.MoneyGainedOnPreviousShift:      return Reference.GetManager<MoneyManager>()?.GetMoneyOnPreviousShift().ToString();
                case EDataInfo.MoneyAmount:                     return Reference.GetManager<MoneyManager>()?.MoneyInfo.ToString();
                case EDataInfo.CookingSpeedUpgradeLevel:        return (Reference.GetManager<UpgradeManager>()?.GetUpgradeByType<CookingSpeedUpgrade>()?.Level + 1).ToString();
                case EDataInfo.IngredientDropUpgradeLevel:      return (Reference.GetManager<UpgradeManager>()?.GetUpgradeByType<IngredientDropUpgrade>()?.Level + 1).ToString();
                case EDataInfo.OvenCountUpgradeLevel:           return (Reference.GetManager<UpgradeManager>()?.GetUpgradeByType<OvenCountUpgrade>()?.Level + 1).ToString();
                case EDataInfo.CuttingSpeedUpgradeLevel:        return (Reference.GetManager<UpgradeManager>()?.GetUpgradeByType<BlenderSpeedUpgrade>()?.Level + 1).ToString();
                case EDataInfo.TimeLeft:                        return ProcessTimeLeft();
                case EDataInfo.LastMoneyDelta:                  return ((int)Reference.GetManager<MoneyManager>()?.LastMoneyDelta).ToString() ;
                case EDataInfo.TotalAvailableUpgrades:          return Reference.GetManager<UpgradeManager>()?.Upgrades.Select(up => up.Config.MaxLevel).Sum().ToString();
                case EDataInfo.TotalBoughtUpgrades:             return Reference.GetManager<UpgradeManager>()?.Upgrades.Select(up => up.Level).Sum().ToString();

            }
            return result;
        }
        string ProcessTimeLeft()
        {
            if (!TimerManager.Timers.ContainsKey("shiftDuration"))
            {
                return "";
            }
            float timeLeft = TimerManager.TimeLeft("shiftDuration");

            if (timeLeft > 0)
                return TimeSpan.FromSeconds(timeLeft).ToString(@"mm\:ss");
            else
                return "OVER";
        }
        public int GetCorrectOrdersCount()
        {
            if (!Reference.TryGetManager(out ClientManager clientMgr)) return 0;
            return clientMgr.GetClientsHistory().Count(c => !c.killed && c.wasCorrectRecipe);
        }

        public int GetWrongOrdersCount()
        {
            if (!Reference.TryGetManager(out ClientManager clientMgr)) return 0;
            return clientMgr.GetClientsHistory().Count(c => !c.killed && !c.wasCorrectRecipe && c.served);
            //This version consider client leaving without food as wrong order, if u don't want to count them add && c.served (edit : I did)
        }
        public int GetMissedOrdersCount()
        {
            if (!Reference.TryGetManager(out ClientManager clientMgr)) return 0;
            return clientMgr.GetClientsHistory().Count(c => !c.killed && !c.wasCorrectRecipe && !c.served);
            //This version consider client leaving without food as wrong order, if u don't want to count them add && c.served
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