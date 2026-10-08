using Steamworks;
using Steamworks.Data;
public static class Achievements
{
    public static bool IsValid => SteamClient.IsValid && Globals.Authorizations.AllowAchievements;
    static Achievements()
    {
        try
        {
            if (!SteamClient.IsValid)
                SteamClient.Init(4887440); // Cooking Carnage id : 4887440
        }
        catch { UnityEngine.Debug.Log("Couldn't connect to the Steam client. Steam is probably not running"); }

#if DEMO
        Globals.Authorizations.AllowAchievements.Value = false;
#else
        Globals.Authorizations.AllowAchievements.Value = true;
#endif
    }
    public static void Unlock(string API_name) {if (IsValid) new Achievement(API_name).Trigger(); }
    public static void UnlockAll() { if (IsValid) foreach (var ach in SteamUserStats.Achievements) ach.Trigger(); }
    public static void Clear(string API_name) { if (IsValid) new Achievement(API_name).Clear(); }
    public static void ClearAll() { if (IsValid) foreach (var ach in SteamUserStats.Achievements) ach.Clear(); }
}
