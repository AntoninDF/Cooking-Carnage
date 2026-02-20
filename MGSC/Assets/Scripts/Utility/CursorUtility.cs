using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public static class CursorUtility
{
    private static List<object> _objectsRequiringCursor = new();

    public static void Update()
    {
        if (_objectsRequiringCursor.Count > 0 )
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState= CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public static void TryFreeCursor(object obj)
    {
        _objectsRequiringCursor.Remove(obj);
        Update();
    }
    public static void RequestCursor(object obj)
    {
        if (!_objectsRequiringCursor.Contains(obj))
            _objectsRequiringCursor.Add(obj);
        Update();
    }
}
