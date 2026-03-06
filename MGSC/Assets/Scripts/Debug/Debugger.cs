using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Clients;
using Cooking;
using GameSystem;
using UnityEngine;
using Upgrades;

public static class Debugger
{
    #region Properties

    public static bool CheckDebug(Type type, LogType info) => debugManagers[info].Contains(type);

    #endregion
    

    #region Fields
    
    private static readonly List<string> logHistory = new();
    private static Dictionary<LogType, List<Type>> debugManagers = new()
    {
        { LogType.Log,    new List<Type>() },
        { LogType.Warning, new List<Type>() },
        { LogType.Error,   new List<Type>() }
    };
    public static event Action<string,LogType> OnLogAdded;


    #endregion

    #region Methods

    
    
    public static void Log(this IManager emitter, params object[] messages)
    {
        Log(emitter,LogType.Log, messages);
    }

    public static void Log(this IManager emitter, LogType logType ,  params object[] messages) 
    {
        if (messages == null || !CheckDebug(emitter.GetType(), logType))
            return;

        string prefix = $"[{DateTime.Now:HH:mm:ss}] [{emitter.GetType().Name}] : ";
        string message = string.Join(" ", messages.Where(m => m != null).Select(Decrypt));

        switch (logType) 
        {
            case LogType.Log:
                Debug.Log(prefix + message);
                break;
            case LogType.Warning:
                Debug.LogWarning(prefix + message);
                break;
            case LogType.Error:
                Debug.LogError(prefix + message);
                break;
        }
        logHistory.Add(prefix+message);
        OnLogAdded?.Invoke(prefix+message, logType);
    }

    static string Decrypt<T>(T toDecrypt)
    {
        if (toDecrypt == null)
            return string.Empty;

        var type = toDecrypt.GetType();
        
        switch (toDecrypt)
        {
            case string :   return toDecrypt as string;
            case int :      return toDecrypt.ToString();
            case float :    return toDecrypt.ToString();
        }

        if (toDecrypt is IDictionary dict)
        {
            if (dict.Count == 0)
                return "Dictionary is empty";
            
            var parts = new List<string>();
            parts.Add("Dictionnary : ");
            foreach (var key in dict.Keys)
                parts.Add($"{Decrypt(key)} {Decrypt(dict[key])}");
            return string.Join("\n", parts);
        }

        if (toDecrypt is IEnumerable list && !(toDecrypt is string))
        {
            if (!list.GetEnumerator().MoveNext())
                return "List is empty";
            
            var parts = new List<string>();
            parts.Add("List : ");
            foreach (var elem in list)
                parts.Add(Decrypt(elem));
            return string.Join("\n", parts);
        }

        return toDecrypt switch
        {
            Orders.Order o               => $"Order n°{o.ID} for {o.Owner.name} ",
            SO_Ingredient ing            => $"{ing.Name} ",
            SO_Recipe r                  => $"{r.Name} ",
            SO_Client soClient           => $"{soClient.name} ",
            Client c                     => $"{c.name} ",
            ClientData cd  => $"Client{cd.id} ",
            SO_RecipeBook rb             => $"{Decrypt(rb.Recipes)} ",
            MoneyInfo mi                 => $"{mi} $ ",
            Upgrade up                   => $"Upgrade {up.GetType().Name} - lvl {up.Level} ",
            _                            => toDecrypt.ToString() + " something "
        };
    }

    public static void ToggleDebugManager(Type type, LogType info)
    {
        if (debugManagers[info].Contains(type))
        {
            debugManagers[info].Remove(type);
        }
        else
        {
            debugManagers[info].Add(type);
        }
    }

    #endregion
}
