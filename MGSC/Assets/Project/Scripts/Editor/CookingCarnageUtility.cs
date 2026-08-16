using GameSystem;
using UnityEditor;
using UnityEngine;

public static class CookingCarnageUtility
{

    [MenuItem("CookingCarnage/Motherlode (get rich fast)")]
    static void AddMoneyToSave()
    {
        var data = SaveManager.GetCurrentData();
        data.money = 10000;
        SaveManager.SaveGameData(data);
        if (Application.isPlaying)
        {
            Reference.GetManager<MoneyManager>()?.AddMoney(10000);
        }
    }
}
