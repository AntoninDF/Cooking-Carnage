using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Upgrades;

namespace GameSystem
{
    /// <summary>
    /// The SaveManager is responsible for saving the game and loading saves.
    /// </summary>
    public static class SaveManager
    {
        #region Fields
        public static GameData Data;
        public static string filePath = Path.Combine(Application.persistentDataPath, "gameData.json");
        [GlobalEvent("Saves/OnGameSaved")]
        public static Action OnSuccessfulSave;
        [GlobalEvent("Saves/OnGameLoaded")]
        public static Action OnSaveLoaded;
        #endregion
        


        #region Methods
        private static GameData MakeGameData()
        {
            var result = new GameData()
            {
                money =  Reference.GetManager<MoneyManager>()?.MoneyInfo ?? 0f,
                upgrades = ConvertUpgradeData(),
                shiftNumber = Globals.Gameplay.ShiftNumber
            };
            return result;
        }
        /// <summary>
        /// Save multiple data (money, upgrades, daycount) into a file
        /// </summary>
        public static void SaveGameData() => SaveGameData(MakeGameData());
        /// <summary>
        /// Save multiple data (money, upgrades, daycount) into a file
        /// </summary>
        public static void SaveGameData(GameData gameData)
        {
            if (gameData == null)
            {
                Debug.LogWarning("[SaveManager] GameData was null, and could not be saved.");
                return;
            }

            if (filePath == null)
            {
                Debug.LogWarning("The specified file path for the save file is null. Make sure the manager was initialized properly");
                return;
            }
            
            string json = JsonUtility.ToJson(gameData, true);
            
            File.WriteAllText(filePath, json);
            OnSuccessfulSave?.Invoke();
        }
    
        public static bool SaveFileExists()
        {
            return File.Exists(filePath);
        }

        public static GameData GetCurrentData()
        {
            if (!SaveFileExists())
            {
                ResetSave();
            }
            string json = File.ReadAllText(filePath);
            return JsonUtility.FromJson<GameData>(json);
        }
        /// <summary>
        /// Load multiple data (money, upgrades, daycount) and reload the game
        /// </summary>
        /// <returns>Wether or not a save file could be loaded</returns>
        public static bool LoadGlobalData()
        {
            Data = GetCurrentData();
            
            if (Reference.TryGetManager(out MoneyManager moneyMgr))
                moneyMgr.MoneyInfo = Data.money;
            
            if (Reference.TryGetManager(out ShiftManager shiftMgr))
                shiftMgr.ShiftNumber = Data.shiftNumber;

            LoadUpgradeData();

            //Debug.Log($"Save loaded successfully, shift : {Data.shiftNumber},  money : {Data.money}");
            OnSaveLoaded?.Invoke();
            return true;
        }

        public static void ResetSave()
        {
            Data = DefaultData();
            SaveGameData(Data);
        }

        static List<UpgradeData> ConvertUpgradeData()
        {
            if (!Reference.TryGetManager(out UpgradeManager upgradeManager)) return new();

            List<UpgradeData> result = new List<UpgradeData>();

            for (var index = 0; index < upgradeManager.Upgrades.Count; index++)
            {
                var upgrade = upgradeManager.Upgrades[index];
                //Debug.Log(upgrade.GetType().Name + "found, lvl : " + upgrade.Level);
                result.Add(new UpgradeData());
                result[index].level = upgrade.Level;
                result[index].name = upgrade.GetType().Name;
            }

            return result;
        }

        static void LoadUpgradeData()
        {
            if (!Reference.TryGetManager(out UpgradeManager upgradeManager)) return;

            if (Data.upgrades == null)
            {
                Debug.LogWarning("No Upgrade data found");
                return;
            }
            foreach (var upgradeData in Data.upgrades)
            {
                Type type = Type.GetType("Upgrades."+upgradeData.name);
                Upgrade upgrade = upgradeManager.GetUpgradeByType(type);
                
                for (int i = 0; i < upgradeData.level; i++)
                {
                    upgrade.LevelUp();
                }
            }
        }

        static GameData DefaultData()
        {
            return new GameData
            {
                money = 0,
                upgrades = new List<UpgradeData> { },
                shiftNumber = 0
            };

            #endregion
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