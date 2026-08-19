using Steamworks;
using Steamworks.Data;
public static class Achievements
{
    static Achievements()
    {
        try
        {
            if (!SteamClient.IsValid)
                SteamClient.Init(4887440); // Cooking Carnage id : 4887440
        }
        catch { UnityEngine.Debug.Log("Couldn't connect to the Steam client. Steam is probably not running"); }
    }
    public static void Unlock(string API_name) {if (SteamClient.IsValid) new Achievement(API_name).Trigger(); }
    public static void UnlockAll() { if (SteamClient.IsValid) foreach (var ach in SteamUserStats.Achievements) ach.Trigger(); }
    public static void Clear(string API_name) { if (SteamClient.IsValid) new Achievement(API_name).Clear(); }
    public static void ClearAll() { if (SteamClient.IsValid) foreach (var ach in SteamUserStats.Achievements) ach.Clear(); }
}
