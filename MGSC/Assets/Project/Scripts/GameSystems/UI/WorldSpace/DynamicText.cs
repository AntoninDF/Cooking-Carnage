using GameSystem;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using Upgrades;


namespace UI
{
    [ExecuteAlways]
    public class DynamicText : MonoBehaviour
    {
        #region Fields

        [Header("References")]
        [Tooltip("The target text object's to update")]
        [SerializeField] private TMP_Text _textObject;
        [ReadOnly, Tooltip("This is the text that will be displayed. Place the markups in it for them to get replaced by actual data")]
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
            if (!Application.isPlaying) return;
            UpdateUI();
        }

        /*
#if UNITY_EDITOR
        private void OnEnable()
        {
            if (!Application.isPlaying) EditorApplication.update += ReplicateReferenceTextFromTextComponent;
        }
        private void OnDisable()
        {
            if (!Application.isPlaying) EditorApplication.update -= ReplicateReferenceTextFromTextComponent;
        }

#endif */

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
            var mm = Reference.GetManager<MoneyManager>();
            var um = Reference.GetManager<UpgradeManager>();
            var sm = Reference.GetManager<ShiftManager>();

            switch (info)
            {
                case EDataInfo.TimeElapsedSinceStartup:         return Time.time.ToString();
                case EDataInfo.ShiftDuration:                   return sm?.ShiftDuration.ToString();
                case EDataInfo.RentPayedThisShift:              return sm?.GetCurrentRentPrice().ToString();
                case EDataInfo.ShiftCount:                      return sm?.ShiftNumber.ToString();
                case EDataInfo.CorrectOrdersThisShift:          return GetCorrectOrdersCount().ToString();
                case EDataInfo.WrongOrdersThisShift:            return GetWrongOrdersCount().ToString();
                case EDataInfo.MissedOrdersThisShift:           return GetMissedOrdersCount().ToString();
                case EDataInfo.ClientsKilledThisShift:          return Reference.GetManager<ClientManager>()?.ClientsKilledThisShift.ToString();
                case EDataInfo.MoneyGainedThisShift:            return mm?.GetMoneyGainedThisShift().ToString();
                case EDataInfo.MoneyGainedOnPreviousShift:      return mm?.GetMoneyOnPreviousShift().ToString();
                case EDataInfo.MoneyAmount:                     return mm?.MoneyInfo.ToString();
                case EDataInfo.CookingSpeedUpgradeLevel:        return (um?.GetUpgradeByType<CookingSpeedUpgrade>()?.Level + 1).ToString();
                case EDataInfo.IngredientDropUpgradeLevel:      return (um?.GetUpgradeByType<IngredientDropUpgrade>()?.Level + 1).ToString();
                case EDataInfo.OvenCountUpgradeLevel:           return (um?.GetUpgradeByType<OvenCountUpgrade>()?.Level + 1).ToString();
                case EDataInfo.CuttingSpeedUpgradeLevel:        return (um?.GetUpgradeByType<BlenderSpeedUpgrade>()?.Level + 1).ToString();
                case EDataInfo.TimeLeft:                        return ProcessTimeLeft();
                case EDataInfo.LastMoneyDelta:                  return ((int)mm?.LastMoneyDelta).ToString() ;
                case EDataInfo.TotalAvailableUpgrades:          return um?.Upgrades.Select(up => up.Config.MaxLevel).Sum().ToString();
                case EDataInfo.TotalBoughtUpgrades:             return um?.Upgrades.Select(up => up.Level).Sum().ToString();
                case EDataInfo.MoneyAtTheBeginningOfTheShift:   return (mm?.MoneyInfo - mm?.GetMoneyTransactionsThisShift().Sum()).ToString(); // This ignores what was spent in upgrades
                case EDataInfo.CurrentProjectVersion:           return (Application.version).ToString();
                case EDataInfo.TimePlayed:                      return SecondsAsDuration(Globals.Statistics.SecondsPlayed + (int)Time.realtimeSinceStartup); // Adds up time played during previous sessions and this session

            }
            return result;
        }
        string SecondsAsDuration(int seconds)
        {
            int secondsUsed = 0;
            int secondsInADay = 3600 * 24;

            // Values
            int d = seconds / secondsInADay;
            secondsUsed += d * secondsInADay;

            int h = (seconds - secondsUsed) / 3600;
            secondsUsed += h * 3600;

            int mn = (seconds - secondsUsed) / 60;
            secondsUsed += mn * 60;
            
            int s = (seconds - secondsUsed);

            // Strings
            string ds = d + "d";
            string hs = h + "h";
            string mns = mn + "mn";
            string ss = s + "s";

            string result = "";
            if (d > 0) result += ds;
            if (h > 0) result += hs;
            if (mn > 0) result += mns;
            // if (s > 0) result += ss; // There's not much reason to include seconds, it's unnecessary noise
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
            return clientMgr.GetClientsHistory().Count(c => !c.killed && c.wasCorrectRecipe && c.shiftNumber == Globals.Gameplay.ShiftNumber);
        }

        public int GetWrongOrdersCount()
        {
            if (!Reference.TryGetManager(out ClientManager clientMgr)) return 0;
            return clientMgr.GetClientsHistory().Count(c => !c.killed && !c.wasCorrectRecipe && c.served && c.shiftNumber == Globals.Gameplay.ShiftNumber);
            //This version consider client leaving without food as wrong order, if u don't want to count them add && c.served (edit : I did)
        }
        public int GetMissedOrdersCount()
        {
            if (!Reference.TryGetManager(out ClientManager clientMgr)) return 0;
            return clientMgr.GetClientsHistory().Count(c => !c.killed && !c.wasCorrectRecipe && !c.served && c.shiftNumber == Globals.Gameplay.ShiftNumber);
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

        private void OnValidate() => ReplicateReferenceTextFromTextComponent();

        [Button("Apply text from text component")]
        private void ReplicateReferenceTextFromTextComponent()
        {
            if (_textObject != null)
                _baseText = _textObject.text;
        }
    }

}