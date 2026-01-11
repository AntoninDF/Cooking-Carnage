using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Upgrades;

namespace GameSystem
{

    public class DataManager : IManager
    {
        #region Properties

        public static DataManager Instance { get { if (_instance == null) _instance = new DataManager(); return _instance; } }

        public MoneyInfo MoneyInfo { get { EnforceReferences(); return _moneyManager.MoneyInfo; } }
        
        public int ShiftCount => _shiftManager.ShiftCount;

        public GameData Data => currentGameData;

        #endregion

        #region Fields

        MoneyManager _moneyManager;
        UpgradeManager _upgradeManager;
        ClientManager _clientManager;
        ShiftManager _shiftManager;

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
            ReferenceManager.Instance.SetManager(this);
        }
        public void Unregister()
        {
            ReferenceManager.Instance.RemoveManager(this);
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
                shiftCount = gameData?.shiftCount ?? ShiftCount
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
            
            _shiftManager.SetShiftCount(currentGameData.shiftCount);
            
            this.Log(LogType.Log, $"Save loaded successfully, shift : {currentGameData.shiftCount},  money : {currentGameData.money}");
        }

        public void ResetSave()
        {
            currentGameData = DefaultData();
            SaveGameData(currentGameData);
            LoadGlobalData();
        }

        Dictionary<string, int> ConvertUpgradeData()
        {
            Dictionary<string, int> result = new Dictionary<string, int>();

            foreach (var upgrade in _upgradeManager.Upgrades)
            {
                result[upgrade.GetType().Name] = upgrade.Level;
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
                Type type = Type.GetType("Upgrades."+upgradeData.Key);
                Upgrade upgrade = _upgradeManager.GetUpgradeByType(type);
                
                for (int i = 0; i < upgradeData.Value; i++)
                {
                    upgrade.LevelUp();
                }
            }
        }

        private void EnforceReferences()
        {
            _moneyManager = ReferenceManager.Instance.GetManager<MoneyManager>();
            _upgradeManager = ReferenceManager.Instance.GetManager<UpgradeManager>();
            _clientManager = ReferenceManager.Instance.GetManager<ClientManager>();
            _shiftManager =  ReferenceManager.Instance.GetManager<ShiftManager>();
        }

        GameData DefaultData()
        {
            return new GameData
            {
                money = 0,
                upgrades = new Dictionary<string, int> { },
                shiftCount = 0
            };

            #endregion
        }

        private int GetCorrectOrdersCount()
        {
            if (_clientManager.GetClientsHistory() == null) return 0;
            return _clientManager.GetClientsHistory().Where((c) => !c.killed && c.order.ServingInfo.Recipe == c.order.Recipe).Count();
        }

        private int GetWrongOrdersCount()
        {
            if (_clientManager.GetClientsHistory() == null) return 0;
            return _clientManager.GetClientsHistory().Where((c) => !c.killed && c.order.ServingInfo.Recipe != c.order.Recipe).Count();
        }

        public string GetDataAsString(EDataInfo info)
        {
            EnforceReferences();
            string result = "";

            switch (info)
            {
                case EDataInfo.TimeElapsedSinceStartup:     return (Time.time).ToString();
                case EDataInfo.ShiftDuration:               return _shiftManager.ShiftDuration.ToString();
                case EDataInfo.RentPayedThisShift:          return _shiftManager.CurrentRentPrice.ToString();
                case EDataInfo.ShiftCount:                  return _shiftManager.ShiftCount.ToString();
                case EDataInfo.CorrectOrdersThisShift:      return GetCorrectOrdersCount().ToString();
                case EDataInfo.WrongOrdersThisShift:        return GetWrongOrdersCount().ToString();
                case EDataInfo.ClientsKilledThisShift:      return _clientManager.ClientsKilledThisShift.ToString();
                case EDataInfo.MoneyGainedThisShift:        return _moneyManager.GetMoneyGainedThisShift().ToString();
                case EDataInfo.MoneyAmount:                 return _moneyManager.MoneyInfo.ToString();
            }
            // REFACTO : To be implemented

            return result;
        }

        [System.Serializable]
        public class GameData
        {
            public float money;
            public Dictionary<string, int> upgrades;

            public int shiftCount = -1;
            // REFACTO : Player Settings
        }
    }
}