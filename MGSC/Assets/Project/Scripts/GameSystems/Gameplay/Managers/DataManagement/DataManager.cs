using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Upgrades;

namespace GameSystem
{
    /// <summary>
    /// The DataManager is responsible for saving the game and loading saves.
    /// </summary>
    public class DataManager : IManager
    {
        #region Properties

        public static DataManager Instance { get { if (_instance == null) _instance = new DataManager(); return _instance; } }

        public MoneyInfo MoneyInfo { get { EnforceReferences(); return _moneyManager.MoneyInfo; } }
        
        public int ShiftCount => _shiftManager.ShiftNumber;

        public GameData Data => currentGameData;

        #endregion

        #region Fields

        MoneyManager _moneyManager;
        UpgradeManager _upgradeManager;
        ClientManager _clientManager;
        ShiftManager _shiftManager;
        TimerManager _timerManager;

        private GameData currentGameData;
        private string filePath ;
        
        private static DataManager _instance;

        #endregion

        #region Methods

        public DataManager()
        {
            Register();
        }

        public void Register()
        {
            Reference.SetManager(this);
        }
        public void Unregister()
        {
            Reference.RemoveManager(this);
        }
        public void Init()
        {
            filePath = Path.Combine(Application.persistentDataPath, "gameData.json");
        }
        
        /// <summary>
        /// Save multiple data (money, upgrades, daycount) into a file
        /// </summary>
        public void SaveGameData(GameData gameData = null)
        {
            this.Log(LogType.Log, "Saving game data");
            if (_moneyManager == null || _upgradeManager == null || _clientManager == null || _shiftManager == null)
            {
                EnforceReferences();
            }
            
            currentGameData = new GameData()
            {
                money = gameData?.money ?? MoneyInfo,
                upgrades = gameData?.upgrades ?? ConvertUpgradeData(),
                shiftNumber = gameData?.shiftNumber ?? ShiftCount
            };
            
            string json = JsonUtility.ToJson(currentGameData, true);
            
            if (filePath == null)
            {
                this.Log(LogType.Warning, "The specified file path for the save file is null. Make sure the manager was initialized properly");
            }
            File.WriteAllText(filePath, json);
            this.Log(LogType.Log, "Saved successfully");
        }
    
        bool SaveFileExists()
        {
            return File.Exists(filePath);
        }

        /// <summary>
        /// Load multiple data (money, upgrades, daycount) and reload the game
        /// </summary>
        public void LoadGlobalData()
        {
            this.Log(LogType.Log, "Loading global data");
            
            if (_moneyManager == null || _upgradeManager == null || _clientManager == null || _shiftManager == null)
            {
                EnforceReferences();
            }
            
            if (!SaveFileExists())
            {
                this.Log(LogType.Warning, "No save file found");
                
                ResetSave();
                return;
            }
            string json = File.ReadAllText(filePath);
            currentGameData = JsonUtility.FromJson<GameData>(json);
            
            _moneyManager.MoneyInfo = new MoneyInfo(currentGameData.money);
            
            LoadUpgradeData();
            
            _shiftManager.ShiftNumber = currentGameData.shiftNumber;
            
            this.Log(LogType.Log, $"Save loaded successfully, shift : {currentGameData.shiftNumber},  money : {currentGameData.money}");
        }

        public void ResetSave()
        {
            currentGameData = DefaultData();
            SaveGameData(currentGameData);
            //LoadGlobalData();
        }

        List<UpgradeData> ConvertUpgradeData()
        {
            List<UpgradeData> result = new List<UpgradeData>();

            for (var index = 0; index < _upgradeManager.Upgrades.Count; index++)
            {
                var upgrade = _upgradeManager.Upgrades[index];
                this.Log(LogType.Warning, upgrade.GetType().Name + "found, lvl : " + upgrade.Level);
                result.Add(new UpgradeData());
                result[index].level = upgrade.Level;
                result[index].name = upgrade.GetType().Name;
            }

            return result;
        }

        void LoadUpgradeData()
        {
            if (currentGameData.upgrades == null)
            {
                this.Log(LogType.Warning, "No Upgrade data found");
                return;
            }
            foreach (var upgradeData in currentGameData.upgrades)
            {
                this.Log(LogType.Log, "Upgrade loaded");
                
                Type type = Type.GetType("Upgrades."+upgradeData.name);
                Upgrade upgrade = _upgradeManager.GetUpgradeByType(type);
                
                for (int i = 0; i < upgradeData.level; i++)
                {
                    upgrade.LevelUp();
                }
            }
        }

        private void EnforceReferences()
        {
            Reference.GetManager(out _moneyManager);
            Reference.GetManager(out _upgradeManager);
            Reference.GetManager(out _clientManager);
            Reference.GetManager(out _shiftManager); 
        }

        GameData DefaultData()
        {
            return new GameData
            {
                money = 0,
                upgrades = new List<UpgradeData> { },
                shiftNumber = 0
            };

            #endregion
        }

        public int GetCorrectOrdersCount()
        {
            if (_clientManager.GetClientsHistory() == null) return 0;
            return _clientManager.GetClientsHistory().Where((c) => !c.killed && c.order.ServingInfo.Recipe == c.order.Recipe).Count();
        }

        public int GetWrongOrdersCount()
        {
            if (_clientManager.GetClientsHistory() == null) return 0;
            return _clientManager.GetClientsHistory().Where((c) => !c.killed && c.order.ServingInfo.Recipe != c.order.Recipe).Count();
        }

        public int GetTotalOrdersCount()
        {
            if (_clientManager.GetClientsHistory() == null) return 0;
            return _clientManager.GetClientsHistory().Where((c) => !c.killed).Count();
        }

        public string GetDataAsString(EDataInfo info)
        {
            EnforceReferences();
            string result = "";

            switch (info)
            {
                case EDataInfo.TimeElapsedSinceStartup:     return (Time.time).ToString();
                case EDataInfo.ShiftDuration:               return _shiftManager.ShiftDuration.ToString();
                case EDataInfo.RentPayedThisShift:          return _shiftManager.GetCurrentRentPrice().ToString();
                case EDataInfo.ShiftCount:                  return _shiftManager.ShiftNumber.ToString();
                case EDataInfo.CorrectOrdersThisShift:      return GetCorrectOrdersCount().ToString();
                case EDataInfo.WrongOrdersThisShift:        return GetWrongOrdersCount().ToString();
                case EDataInfo.ClientsKilledThisShift:      return _clientManager.ClientsKilledThisShift.ToString();
                case EDataInfo.MoneyGainedThisShift:        return _moneyManager.GetMoneyGainedThisShift().ToString();
                case EDataInfo.MoneyAmount:                 return _moneyManager.MoneyInfo.ToString();
                case EDataInfo.CookingSpeedUpgradeLevel:    return (_upgradeManager.GetUpgradeByType<CookingSpeedUpgrade>()?.Level+1).ToString();
                case EDataInfo.IngredientDropUpgradeLevel:  return (_upgradeManager.GetUpgradeByType<IngredientDropUpgrade>()?.Level+1).ToString();
                case EDataInfo.OvenCountUpgradeLevel:       return (_upgradeManager.GetUpgradeByType<OvenCountUpgrade>()?.Level+1).ToString();
                case EDataInfo.CuttingSpeedUpgradeLevel:    return (_upgradeManager.GetUpgradeByType<BlenderSpeedUpgrade>()?.Level + 1).ToString();
                case EDataInfo.TimeLeft:                    return ProcessTimeLeft();
                    
            }
            // REFACTO : To be implemented

            return result;
        }

        string ProcessTimeLeft()
        {
            if (ShiftCount < 1)
            {
                return "";
            }
            float timeLeft = TimerManager.TimeLeft("shiftDuration") + TimerManager.TimeLeft("shiftNoonDuration");

            if (timeLeft > 0)
                return TimeSpan.FromSeconds(timeLeft).ToString(@"mm\:ss");
            else
                return "OVER";
        }

        [System.Serializable]
        public class GameData
        {
            public float money;
            public List<UpgradeData> upgrades;

            public int shiftNumber = -1;
            // REFACTO : Player Settings
        }
        
        [Serializable]
        public class UpgradeData
        {
            public string name;
            public int level;
        }
    }
}