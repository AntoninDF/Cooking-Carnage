using GameSystem;
using UnityEditor;

public static class CookingCarnageUitlity
{

    [MenuItem("CookingCarnage/Motherlode (get rich fast)")]
    static void AddMoneyToSave()
    {
        var data = SaveManager.GetCurrentData();
        data.money = 10000;
        SaveManager.SaveGameData(data);
    }
}
