using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Clients;
using Cooking;
using GameSystem;
using UnityEngine;
using Upgrades;

public class Debugger
{
    #region Properties

    // Lazy initialization
    public static Debugger Instance
    {
        get
        {
            if (_instance == null) _instance = new Debugger();
            return _instance;
        }
    }

    #endregion
    
    

    #region Fields

    private static Debugger _instance;
    private readonly List<string> logHistory = new();
    private List<Type> debugManagers = new();
    
    public enum LogType
    {
        Info,
        Warning,
        Error
    }


    #endregion

    #region Methods

    public Debugger()
    {
        //Instancier la fenetre de console
    }
    
    public void Log(IManager emitter, params object[] messages)
    {
        Log(emitter,LogType.Info, messages);
    }

    public void Log(IManager emitter, LogType logType , params object[] messages) 
    {
        if (messages == null || messages.Length == 0|| !debugManagers.Contains(emitter.GetType()))
            return;
        
        string prefix = $"[{DateTime.Now:HH:mm:ss}]  [Debugger] : ";
        string message = string.Join(" ", messages.Where(m => m != null).Select(Decrypt));

        switch (logType) 
        {
            case LogType.Info:
                Debug.Log(prefix + message);
                //ajouter la fonction pour mettre dans la console directement
                break;
            case LogType.Warning:
                Debug.LogWarning(prefix + message);
                //ajouter la fonction pour mettre dans la console directement
                break;
            case LogType.Error:
                Debug.LogError(prefix + message);
                //ajouter la fonction pour mettre dans la console directement
                break;
        }
        logHistory.Add(message);
    }

    string Decrypt<T>(T toDecrypt)
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
            ClientManager.ClientData cd  => $"Client{cd.id} ",
            SO_RecipeBook rb             => $"{Decrypt(rb.Recipes)} ",
            MoneyInfo mi                 => $"{mi} $ ",
            Upgrade up                   => $"Upgrade {up.GetType().Name} - lvl {up.Level} ",
            _                            => toDecrypt.ToString() + " "
        };
    }

    public void ToggleDebugManager(Type type)
    {
        if (debugManagers.Contains(type))
        {
            debugManagers.Remove(type);
        }
        else
        {
            debugManagers.Add(type);
        }
    }

    public bool CheckDebug(Type type)
    {
        if (debugManagers.Contains(type))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    #endregion
}
