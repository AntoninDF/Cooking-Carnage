using Steamworks;
using Steamworks.Data;
public static class Achievements
{
    static Achievements()
    {
        if (!SteamClient.IsValid)
            SteamClient.Init(4887440); // Cooking Carnage id : 4887440
    }
    public static void Unlock(string API_name) => new Achievement(API_name).Trigger();
    public static void UnlockAll() { foreach (var ach in SteamUserStats.Achievements) ach.Trigger(); }
    public static void Clear(string API_name) => new Achievement(API_name).Clear();
    public static void ClearAll() { foreach(var ach in SteamUserStats.Achievements) ach.Clear(); }
}
