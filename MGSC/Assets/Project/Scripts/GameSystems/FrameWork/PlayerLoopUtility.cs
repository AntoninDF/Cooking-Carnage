using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

// 
/// <summary>
/// Insert a custom update at a specified point (after an existing update phase) in the Unity Player Loop.
/// See https://docs.unity3d.com/6000.0/Documentation/ScriptReference/LowLevel.PlayerLoop.SetPlayerLoop.html
/// </summary>
public static class PlayerLoopUtility
{
    /// <summary>
    /// Insert a method call in the player loop after the specified player loop system.
    /// It is advised to use it in a static method marked with the attribute [<see cref="RuntimeInitializeOnLoadMethodAttribute"/>]
    /// </summary>
    /// <typeparam name="TPreviousLoop">The player loop system type after which to insert the callable</typeparam>
    /// <typeparam name="TNewLoop">The type of the loop to insert</typeparam>
    /// <param name="callable">Invoked right after <see cref="TPreviousLoop"/> on each update</param>
    public static void InsertMethodCallInPlayerLoopAfter<TPreviousLoop, TNewLoop>([NotNull] Action callable) where TPreviousLoop : struct
    {
        // Retrieve the default Player loop system. Get the current loop instead if the default was already modified previously.
        var defaultLoop = PlayerLoop.GetDefaultPlayerLoop();

        // Create a custom update system
        var myCustomUpdate = new PlayerLoopSystem
        {
            subSystemList = null,
            updateDelegate = callable.Invoke,
            type = typeof(TNewLoop)
        };
        // Add the custom update system after the PreLateUpdate phase in the Player Loop
        var loopWithCustomUpdate = InsertSystemAfter<TPreviousLoop>(in defaultLoop, myCustomUpdate);
        PlayerLoop.SetPlayerLoop(loopWithCustomUpdate);
    }

    private static PlayerLoopSystem InsertSystemAfter<T>(in PlayerLoopSystem loopSystem, PlayerLoopSystem newSystem) where T : struct
    {
        // Create a new root PlayerLoopSystem
        PlayerLoopSystem newPlayerLoop = new()
        {
            loopConditionFunction = loopSystem.loopConditionFunction,
            type = loopSystem.type,
            updateDelegate = loopSystem.updateDelegate,
            updateFunction = loopSystem.updateFunction
        };
        // Create a new list to populate with subsystems, including the custom system
        List<PlayerLoopSystem> newSubSystemList = new();

        //Iterate through the subsystems in the existing loop we passed in and add them to the new list
        if (loopSystem.subSystemList != null)
        {
            for (var i = 0; i < loopSystem.subSystemList.Length; i++)
            {
                newSubSystemList.Add(loopSystem.subSystemList[i]);
                // If the previously added subsystem is of the type to add after, add the custom system
                if (loopSystem.subSystemList[i].type == typeof(T))
                {
                    newSubSystemList.Add(newSystem);
                }
            }
        }

        newPlayerLoop.subSystemList = newSubSystemList.ToArray();
        return newPlayerLoop;
    }

    public static void LogCurrentPlayerLoop()
    {
        StringBuilder sb = new();
        // Pass the default Player loop to the recursive print function
        RecursivePlayerLoopPrint(PlayerLoop.GetCurrentPlayerLoop(), sb, 0);
        Debug.Log(sb.ToString());
    }

    // Loop through the Player loop tree structure and add the names of the systems to the StringBuilder
    private static void RecursivePlayerLoopPrint(PlayerLoopSystem playerLoopSystem, StringBuilder sb, int depth)
    {
        if (depth == 0)
        {
            sb.AppendLine("ROOT NODE");
        }
        else if (playerLoopSystem.type != null)
        {
            for (int i = 0; i < depth; i++)
            {
                sb.Append("\t");
            }
            sb.AppendLine(playerLoopSystem.type.Name);
        }
        if (playerLoopSystem.subSystemList != null)
        {
            depth++;
            foreach (var s in playerLoopSystem.subSystemList)
            {
                RecursivePlayerLoopPrint(s, sb, depth);
            }
            depth--;
        }
    }
}