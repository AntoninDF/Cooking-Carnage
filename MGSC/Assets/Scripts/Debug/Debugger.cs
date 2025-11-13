using System;
using System.Collections;
using System.Collections.Generic;
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


    #endregion

    #region Methods

    public Debugger()
    {
        //Instancier la fenetre de console
    }

    public void Log(bool isErrorLog, object message1, object message2 = null, object message3 = null,
        object message4 = null, object message5 = null) 
    {
        
        string prefix = $"[{DateTime.Now:HH:mm:ss}]  [Debugger] : ";
        string message = "";

        if (message1 != null)
        {
            message += Decrypt(message1);
        }

        if (message2 != null)
        {
            message += Decrypt(message2);
        }

        if (message3 != null)
        {
            message += Decrypt(message3);
        }

        if (message4 != null)
        {
            message += Decrypt(message4);
        }

        if (message5 != null)
        {
            message += Decrypt(message5);
        }
        
        Debug.Log(prefix + message);
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
            var parts = new List<string>();
            parts.Add("Dictionnary : ");
            foreach (var key in dict.Keys)
                parts.Add($"{Decrypt(key)} {Decrypt(dict[key])}");
            return string.Join(" ", parts);
        }

        if (toDecrypt is IEnumerable list && !(toDecrypt is string))
        {
            var parts = new List<string>();
            parts.Add("List : ");
            foreach (var elem in list)
                parts.Add(Decrypt(elem));
            return string.Join(" ", parts);
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
            _                            => type.FullName + " "
        };
    }

    #endregion
}
