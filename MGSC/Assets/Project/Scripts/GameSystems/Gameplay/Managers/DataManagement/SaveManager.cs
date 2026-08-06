using Newtonsoft.Json;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Transactions;
using UnityEngine;
using Upgrades;

namespace GameSystem
{
    /// <summary>
    /// The SaveManager is responsible for saving the game and loading saves.
    /// </summary>
    public static class SaveManager
    {

        static SaveManager()
        {
            Application.quitting += SaveUponQuit;
        }
        #region Fields
        public static GameData Data;
        public static string filePath = Path.Combine(Application.persistentDataPath, "gameData.json");
        [GlobalEvent("Saves/OnGameSaved")]
        public static Action OnSuccessfulSave;
        [GlobalEvent("Saves/OnGameLoaded")]
        public static Action OnSaveLoaded;
        #endregion



        #region Methods

        //⚠️ UPDATE THIS WHEN ADDING NEW STUFF TO SAVE (only if what you add should survive save resets)
        public static void ResetSave()
        {

            Data = GetCurrentData();
            GameData temp = DefaultData();
            
            // Some stats are too precious to be overriden
            temp.SecondsPlayed = Data.SecondsPlayed;

            temp.TutorialHasBeenPlayed = false;
            Data = temp;
            SaveGameData(Data);
        }

        //⚠️ UPDATE THIS WHEN ADDING NEW STUFF TO SAVE
        /// <summary>
        /// Load multiple data (money, upgrades, daycount) and reload the game
        /// </summary>
        /// <returns>Wether or not a save file could be loaded</returns>
        public static bool LoadGlobalData()
        {
            Data = GetCurrentData();
            
            // Apply money
            if (Reference.TryGetManager(out MoneyManager moneyMgr))
                moneyMgr.SetCurrentMoney(Data.money);
            
            // Apply shift number
            if (Reference.TryGetManager(out ShiftManager shiftMgr))
                shiftMgr.ShiftNumber = Data.shiftNumber;

            // Apply upgrades
            LoadUpgradeData();

            // Apply upgrade history
            var history = Data.LevelUpHistory;
            history ??= new();
            Reference.GetManager<UpgradeManager>().LevelUpHistory = history;

            // Apply tutorial being played
            // (we don't override it if it's false because maybe the GameSettings force it to true)
            if (Data.TutorialHasBeenPlayed)
                Globals.Statistics.TutorialHasBeenPlayed.Value = true;

            // Apply seconds played
            Globals.Statistics.SecondsPlayed.Value = Data.SecondsPlayed;

            // Apply transaction history
            var transactions = Data.Transactions;
            transactions ??= new();
            if (transactions == null) transactions = new();
            Reference.GetManager<MoneyManager>()?.SetTransactions(transactions);


            OnSaveLoaded?.Invoke();
            return true;
        }

        //⚠️ UPDATE THIS WHEN ADDING NEW STUFF TO SAVE
        /// <summary>Returns a <see cref="GameData"/> containing all the current data about the game </summary>
        public static GameData MakeGameData()
        {
            var result = new GameData()
            {
                money = Reference.GetManager<MoneyManager>()?.MoneyInfo ?? 0f,
                upgrades = ConvertUpgradeData(),
                shiftNumber = Globals.Gameplay.ShiftNumber,
                TutorialHasBeenPlayed = Globals.Statistics.TutorialHasBeenPlayed,
                Transactions = Reference.GetManager<MoneyManager>().GetTransactions(),
                // Save the recipes that were ordered. We don't care about the rest
                OrderHistory = ParseOrdersToStringHistory(Reference.GetManager<OrderManager>().PastOrders),
                LevelUpHistory = Reference.GetManager<UpgradeManager>().LevelUpHistory,
                SecondsPlayed = Globals.Statistics.SecondsPlayed
            };
            return result;
        }
        //⚠️ UPDATE THIS WHEN ADDING NEW STUFF TO SAVE
        [System.Serializable]
        public class GameData
        {
            public float money = 0;
            public List<UpgradeData> upgrades = new List<UpgradeData> { };
            public int shiftNumber = 0;
            public bool TutorialHasBeenPlayed;
            // Non-serializable complex types use backing fields that are their JSON representation to still allow being saved
            public Dictionary<int, List<float>> Transactions
            {
                get => JsonConvert.DeserializeObject< Dictionary<int, List<float>>>(transactions);
                set => transactions = JsonConvert.SerializeObject(value);
            }
            public List<KeyValuePair<int, List<string>>> OrderHistory
            {
                get => JsonConvert.DeserializeObject<List<KeyValuePair<int, List<string>>>>(orderHistory);
                set => orderHistory = JsonConvert.SerializeObject(value);
            }
            public List<UpgradeManager.UpgradeLevelUpInfo> LevelUpHistory;

            [SerializeField]
            private string transactions;
            [SerializeField]
            private string orderHistory;
            // Stats that should not be reset
            public int SecondsPlayed = 0;


            public override string ToString()
            {
                string result = 
                    $"GameData(money={money}, upgrades={upgrades.ToString_EasyRead()}, shiftNumber={shiftNumber})";

                return result;
            }
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


        private static void SaveUponQuit()
        {
            // Save how long the player has played
            Data.SecondsPlayed += (int)Time.realtimeSinceStartup;
            SaveGameData(Data);
        }
        public static List<UpgradeData> ConvertUpgradeData()
        {
            if (!Reference.TryGetManager(out UpgradeManager upgradeManager)) return new();

            List<UpgradeData> result = new List<UpgradeData>();

            for (var index = 0; index < upgradeManager.Upgrades.Count; index++)
            {
                var upgrade = upgradeManager.Upgrades[index];
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
        }

        public static List<KeyValuePair<int, List<string>>> ParseOrdersToStringHistory(Dictionary<int, List<Order>> orders)
        {
            return orders.Select(kvp => new KeyValuePair<int, List<string>>(kvp.Key, kvp.Value
                    .Select(order => order.Recipe.name)
                    .ToList()
                    ))
                    .ToList();
        }

        static GameData DefaultData() => new GameData();

        #endregion


        
        [Serializable]
        public class UpgradeData
        {
            public string name;
            public int level;
        }
    }
}