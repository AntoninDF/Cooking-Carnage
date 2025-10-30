using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Upgrades;

namespace GameSystem
{

    public class DataManager : IManager
    {
        #region Properties

        public static DataManager Instance { get { if (_instance == null) _instance = new DataManager(); return _instance; } }

        public MoneyInfo MoneyInfo { get { EnforceReferences(); return _moneyManager.MoneyInfo; } }
        
        int ShiftCount => _shiftManager.ShiftCount;

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
            filePath = Path.Combine(Application.persistentDataPath, "gameData.json");
        }

        public void Register()
        {
            ReferenceManager.Instance.SetManager(this);
        }
        
        /// <summary>
        /// Save multiple data (money, upgrades, daycount) into a file
        /// </summary>
        public void SaveGameData()
        {
            Debug.Log("SAVE : Saving global data");
            if (_moneyManager == null || _upgradeManager == null || _clientManager == null || _shiftManager == null)
            {
                EnforceReferences();
            }
            
            currentGameData = new GameData()
            {
                money = MoneyInfo,
                upgrades = ConvertUpgradeData(),
                shiftCount = ShiftCount
            };
            
            string json = JsonUtility.ToJson(currentGameData, true);
            
            File.WriteAllText(filePath, json);
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
            Debug.Log("SAVE : Loading global data");
            
            if (_moneyManager == null || _upgradeManager == null || _clientManager == null || _shiftManager == null)
            {
                EnforceReferences();
            }
            
            if (!SaveFileExists())
            {
                ResetSave();
                return;
            }
            string json = File.ReadAllText(filePath);
            currentGameData = JsonUtility.FromJson<GameData>(json);
            
            _moneyManager.MoneyInfo = new MoneyInfo(currentGameData.money);
            Debug.Log("Money : "+_moneyManager.MoneyInfo);
            
            LoadUpgradeData();
            
            _shiftManager.SetShiftCount(currentGameData.shiftCount);
        }

        void ResetSave()
        {
            currentGameData = DefaultData();
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
                shiftCount = 1
            };

            #endregion
        }

        [System.Serializable]
        public class GameData
        {
            public float money;
            public Dictionary<string, int> upgrades;

            public int shiftCount;
            // WIP : Player Settings
        }
    }
}