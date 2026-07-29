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
                money = Reference.GetManager<MoneyManager>()?.MoneyInfo ?? 0f,
                upgrades = ConvertUpgradeData(),
                shiftNumber = Globals.Gameplay.ShiftNumber,
                TutorialHasBeenPlayed = Globals.Statistics.TutorialHasBeenPlayed,

                SecondsPlayed = Globals.Statistics.SecondsPlayed
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

        /// <summary>
        /// Should never return null
        /// </summary>
        /// <returns></returns>
        public static GameData GetCurrentData()
        {
            if (!SaveFileExists())
            {
                Data = DefaultData();
                SaveGameData(Data);
                return Data;
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
            Debug.Log("Loading save data : " + Data.ToString());
            
            // Apply money
            if (Reference.TryGetManager(out MoneyManager moneyMgr))
                moneyMgr.SetCurrentMoney(Data.money);
            
            // Apply shift number
            if (Reference.TryGetManager(out ShiftManager shiftMgr))
                shiftMgr.ShiftNumber = Data.shiftNumber;

            // Apply upgrades
            LoadUpgradeData();

            // Apply tutorial being played
            // (we don't override it if it's false because maybe the GameSettings force it to true)
            if (Data.TutorialHasBeenPlayed)
                Globals.Statistics.TutorialHasBeenPlayed.Value = true;

            // Apply seconds played
            Globals.Statistics.SecondsPlayed.Value = Data.SecondsPlayed;

            //Debug.Log($"Save loaded successfully, shift : {Data.shiftNumber},  money : {Data.money}");
            OnSaveLoaded?.Invoke();
            return true;
        }

        public static void ResetSave()
        {
            Debug.Log("Resetting save file");

            Data = GetCurrentData();
            GameData temp = DefaultData();
            
            // Some stats are too precious to be overriden
            temp.SecondsPlayed = Data.SecondsPlayed;

            temp.TutorialHasBeenPlayed = false;
            Data = temp;
            SaveGameData(Data);
        }

        static List<UpgradeData> ConvertUpgradeData()
        {
            if (!Reference.TryGetManager(out UpgradeManager upgradeManager)) return new();

            List<UpgradeData> result = new List<UpgradeData>();

            for (var index = 0; index < upgradeManager.Upgrades.Count; index++)
            {
                var upgrade = upgradeManager.Upgrades[index];
                Debug.Log(upgrade.GetType().Name + "found, lvl : " + upgrade.Level);
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

            if(upgradeManager.Upgrades.Count != Data.upgrades.Count)
                Debug.Log("the number of saved upgrades is not the same as the current number of upgrades");
            else
            {
                // Reset all the upgrades in case they were not saved
                foreach (var upgrade in upgradeManager.Upgrades)
                    upgrade.SetLevel(0);

                // Apply the levels for the upgrades that were saved
                for (int i = 0; i < Data.upgrades.Count; i++)
                {
                    UpgradeData upgradeData = Data.upgrades[i];
                    var correspondingUpgrades = upgradeManager.Upgrades.Where(upgrade => upgrade.GetType().Name == upgradeData.name).ToArray();
                    if (correspondingUpgrades.Length == 0)
                        continue;
                    correspondingUpgrades[0].SetLevel(upgradeData.level);
                }
                upgradeManager.ActualizeAllUpgrade();
                Debug.Log("All upgrades have been loaded");
            }
            
        }

        static GameData DefaultData() => new GameData();

        #endregion

        
        [System.Serializable]
        public class GameData
        {
            public float money = 0;
            public List<UpgradeData> upgrades = new List<UpgradeData> { };
            public int shiftNumber = 0;
            public bool TutorialHasBeenPlayed;

            // Stats that should not be reset
            public int SecondsPlayed = 0;


            public override string ToString()
            {
                string result = 
                    $"GameData(money={money}, upgrades={upgrades.ToString_EasyRead()}, shiftNumber={shiftNumber})";

                return result;
            }
        }
        
        [Serializable]
        public class UpgradeData
        {
            public string name;
            public int level;
        }
    }
}