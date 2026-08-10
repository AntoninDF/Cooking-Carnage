/// <summary>
/// Provides basic handling of requests for the use of the cursor,
/// ensuring the cursor is visible as long as some objects require it,
/// and that it is hidden as soon as noone requires the cursor anymore
/// </summary>
public static class CursorUtility
{
    public static bool CursorIsLocked => !CursorIsFree;
    public static bool CursorIsFree => _objectsRequiringCursor.Count > 0;
    private static System.Collections.Generic.List<object> _objectsRequiringCursor = new();

    /// <summary>
    /// Checks if the cursor has been requested and updates it's status based on that
    /// </summary>
    public static void Update()
    {
        if (_objectsRequiringCursor.Count > 0 )
        {
            UnityEngine.Cursor.lockState = UnityEngine.CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
        }
        else
        {
            UnityEngine.Cursor.lockState= UnityEngine.CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
        }
    }

    /// <summary>
    /// Tells the cursor that the requester no longer needs it.
    /// If the requester was the last object still requiring the cursor, the cursor goes back to locked and invisible
    /// </summary>
    /// <param name="requester"></param>
    public static void TryFreeCursor(object requester)
    {
        _objectsRequiringCursor.Remove(requester);
        Update();
    }
    /// <summary>
    /// Tells the cursor that the requester needs the cursor to be active and visible to function.
    /// Remember to call <see cref="TryFreeCursor(object)"/> when the requester no longer needs the cursor to work.
    /// </summary>
    /// <param name="requester"></param>
    public static void RequestCursor(object requester)
    {
        if (!_objectsRequiringCursor.Contains(requester))
            _objectsRequiringCursor.Add(requester);
        Update();
    }

    /// <summary>
    /// Removes every request for the cursor so that it can go back to being locked and invisible.
    /// </summary>
    public static void ClearRequests()
    {
        _objectsRequiringCursor.Clear();
        Update();
    }
}
