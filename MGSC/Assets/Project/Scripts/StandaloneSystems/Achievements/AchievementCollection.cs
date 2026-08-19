using NaughtyAttributes;
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AchievementCollection", menuName = "Scriptable Objects/AchievementCollection")]
public class AchievementCollection : ScriptableObject
{
    public string testAchievement;
    public string storePageURL;
    public uint appId;


    [ReadOnly] public List<SerializedAchievement> AllAchievements = new();


    [Button("Get all achievements")]
    public void FetchAllAchievements()
    {
        if (SteamClient.IsValid) SteamClient.Shutdown();
        SteamClient.Init(appId);
        AllAchievements.Clear();
        foreach (var a in SteamUserStats.Achievements)
        {
            AllAchievements.Add(new(a));
        }
    }

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(storePageURL)) return;
        string result = "";
        var elms = storePageURL.Split('/');
        foreach (var elm  in elms)
        {
            if (string.IsNullOrEmpty(elm)) continue;
            bool isValid = true;
            foreach (char c in elm)
            {
                if (!"0123456789".Contains(c))
                {
                    isValid = false;
                    break;
                }
            }
            if (isValid)
            {
                result = elm;
                break;
            }
        }
        if (!string.IsNullOrEmpty(result))
            appId = UInt32.Parse(result);
    }

    [Button("Clear all achievements")] void ClearAll() => Achievements.ClearAll();
    [Button("Unlock all achievements")] void UnlockAll() => Achievements.UnlockAll();

    [Button("Unlock test achievement")] void TestUnlock() => Achievements.Unlock(testAchievement);
    [Button("Clear test achievement")] void TestClear() => Achievements.Clear(testAchievement);
    [Button("Close Steam link")] void CloseSteamLink() => SteamClient.Shutdown();

    [System.Serializable]
    public class SerializedAchievement
    {
        public string Name;
        public string Description;
        public string API_Name;
        public bool Unlocked;

        [Button("Unlock")]
        public void Unlock() => new Achievement(API_Name).Trigger();

        public SerializedAchievement(Achievement ach)
        {
            Name = ach.Name;
            Description = ach.Description;
            API_Name = ach.Identifier;
            Unlocked = ach.State;
        }
    }
}
