using UnityEngine;
using UnityEngine.Events;

// I don't know what namespace to use

/// <summary>
/// Helper tool to scan a class and serialize it's events to be linked with other stuff
/// </summary>
public class EventBinder : MonoBehaviour
{
    #region Fields
    [Header("References")]
    [Tooltip("Object we need to access the events of")]
    [SerializeField] public MonoBehaviour boundObject;

    [SerializeField] private UnityEvent myEvent;
    #endregion

    #region Methods

    #endregion
}
